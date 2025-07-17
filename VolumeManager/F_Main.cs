using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using VolumeManager.Properties;

namespace VolumeManager
{
    public partial class F_Main : C_BorderlessFormBase
    {
        private static readonly log4net.ILog _LogHelper = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string FormTitle = "Volume Manager";
        private const string AutoSessionMarker = "AUTO";
        private const int UpdateInterval = 100;

        private C_MMDevice _ActualDevice;
        private C_AudioSessionControl2 _ActualSession;
        private string _SelectedDevice = string.Empty;
        private string _SelectedSession = string.Empty;
        private Timer _AudioMeterUpdater;

        private C_GlobalHotKey _VolUp;
        private C_GlobalHotKey _VolDown;
        private C_GlobalHotKey _Mute;

        private UdpClient _UdpClient;
        private IPEndPoint _BroadcastEndPoint;
        private Guid _ServerGuid;

        private bool _Send2UDP;
        private bool _SessionMuted;
        private bool _DeviceMuted;

        private C_RingBuffer<float> _LastVolume;

        public F_Main()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { _LogHelper.Error(string.Empty, (Exception)e.ExceptionObject); };
        }

        protected override void WndProc(ref Message m)
        {
            C_GlobalHotKey.ProcessWndProc(ref m);

            switch (m.Msg)
            {
                case WindowsConsts.WM_NCHITTEST:
                    {
                        if (ClientRectangle.Contains(PointToClient(new System.Drawing.Point(m.LParam.ToInt32()))))
                        {
                            m.Result = new IntPtr(WindowsConsts.HTCAPTION);
                            return;
                        }
                        break;
                    }
                default:
                    {
                        base.WndProc(ref m);
                        break;
                    }
            }
        }

        #region events
        private void F_Main_Shown(object sender, EventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            Init();

            Cursor = _Cursor_;
        }

        private void F_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            _VolUp.UnregisterGlobalHotKey();

            _AudioMeterUpdater.Stop();

            Settings.Default.AudioDeviceID = _SelectedDevice;
            Settings.Default.AudioSessionName = _SelectedSession;
            Settings.Default.Save();

            SendUDP("I'm quit!");

            ActualSession = null;
            ActualDevice = null;

            Cursor = _Cursor_;
        }

        private void AudioMeterUpdater_Tick(object sender, EventArgs e)
        {
            try
            {
                if (ActualDevice != null)
                {
                    if (ActualDevice.AudioMeterInformation.PeakValues.Count > 0)
                        UC_AI_Left.Value2 = (ActualDevice.AudioMeterInformation.PeakValues[0] * 100);

                    if (ActualDevice.AudioMeterInformation.PeakValues.Count > 1)
                        UC_AI_Right.Value2 = (ActualDevice.AudioMeterInformation.PeakValues[1] * 100);

                    DeviceMuted = ActualDevice.AudioEndpointVolume.Mute;
                }

                if (ActualSession != null)
                {
                    if (ActualSession.State != E_AudioSessionState.AudioSessionStateActive)
                    {
                        TB_Volume.Enabled = false;

                        UC_AI_Left.Value = 0f;
                        UC_AI_Right.Value = 0f;

                        var _Session_ = Search4Session(_SelectedSession);
                        if (_Session_ != null)
                        {
                            ActualSession = _Session_;
                            MakeFormTitle();

                            UC_AI_Left.Value = 0.01f;
                            UC_AI_Right.Value = 0.01f;
                        }
                        else
                        {
                            // Wenn inaktiv und auto an -> neu suchen
                            if (_SelectedSession == AutoSessionMarker)
                                AutoSelectNew(ActualDevice);
                        }
                    }
                    else
                    {
                        TB_Volume.Enabled = true;

                        if (ActualSession.AudioMeterInformation.PeakValues.Count > 0)
                            UC_AI_Left.Value = (ActualSession.AudioMeterInformation.PeakValues[0] * 100);

                        if (ActualSession.AudioMeterInformation.PeakValues.Count > 1)
                            UC_AI_Right.Value = (ActualSession.AudioMeterInformation.PeakValues[1] * 100);

                        if (ActualSession.SimpleAudioVolume.MasterVolume != ((float)TB_Volume.Value) / 100)
                            ChangeVolumeDisplay(ActualSession.SimpleAudioVolume.MasterVolume);

                        SessionMuted = ActualSession.SimpleAudioVolume.Mute;

                        // Wenn zu lange still -> neu suchen wenn auto an
                        if (_SelectedSession == AutoSessionMarker)
                        {
                            var _Reconnect_ = true;
                            foreach (var _val_ in _LastVolume)
                            {
                                if (_val_ > 0.01)
                                {
                                    _Reconnect_ = false;
                                    break;
                                }
                            }

                            if (_Reconnect_)
                                AutoSelectNew(ActualDevice);
                        }
                    }
                }
                else
                {
                    TB_Volume.Enabled = false;
                    UC_AI_Left.Value = 0f;
                    UC_AI_Right.Value = 0f;

                    var _Session_ = Search4Session(_SelectedSession);
                    if (_Session_ != null)
                    {
                        ActualDevice = Search4Device(_Session_);
                        ActualSession = _Session_;
                        MakeFormTitle();

                        UC_AI_Left.Value = 0.01f;
                        UC_AI_Right.Value = 0.01f;
                    }
                }

                _LastVolume.Add(UC_AI_Left.Value + UC_AI_Right.Value);
                SendUDP($"Vol#{UC_AI_Left.Value}/{UC_AI_Right.Value}/{UC_AI_Left.Value2}/{UC_AI_Right.Value2}");
            }
            catch (Exception ex)
            {
                _LogHelper.Error(string.Empty, ex);
            }
        }

        private void CMS_Main_Opening(object sender, CancelEventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            using (var _DeviceEnum_ = new C_MMDeviceEnumerator())
            using (var _DeviceCollection_ = _DeviceEnum_.EnumerateAudioEndPoints(E_DataFlow.eRender, E_DeviceState.DEVICE_STATE_ACTIVE))
            {
                var _DeviceList_ = new List<ToolStripMenuItem>();

                for (var _i_ = 0; _i_ < _DeviceCollection_.Count; _i_++)
                    using (var _Device_ = _DeviceCollection_[_i_])
                    {
                        var _MIDevice_ = new ToolStripMenuItem($"{_Device_.FriendlyName} - {GetDeviceName(_Device_)}") { Tag = _Device_.ID };

                        if ((ActualDevice != null) && (ActualDevice.ID == _Device_.ID))
                            _MIDevice_.Checked = true;

                        using (var _Manager_ = _Device_.AudioSessionManager2)
                        using (var _Sessions_ = _Manager_.Sessions)
                        {
                            #region *** Sessions submenu creation ***
                            var _SessionList_ = new List<ToolStripMenuItem>();

                            for (var _j_ = 0; _j_ < _Sessions_.Count; _j_++)
                            {
                                try
                                {
                                    using (var _Session_ = _Sessions_[_j_])
                                    {
                                        var _Name_ = GetSessionName(_Session_);
                                        if (!_Name_.StartsWith("PID:", StringComparison.InvariantCultureIgnoreCase) && !_Name_.StartsWith("svchost", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            var _MISession_ = new ToolStripMenuItem(_Name_) { Tag = _Session_.GetSessionIdentifier };
                                            _MISession_.Click += CMI_Click;

                                            if (((ActualDevice != null) && (ActualDevice.ID == _Device_.ID) && (ActualSession != null) &&
                                                (ActualSession.GetSessionIdentifier == _Session_.GetSessionIdentifier)) || (_SelectedSession == _Session_.GetSessionIdentifier))
                                                _MISession_.Checked = true;

                                            if (_Session_.State == E_AudioSessionState.AudioSessionStateInactive)
                                                _MISession_.ForeColor = System.Drawing.Color.Gray;

                                            if (_Session_.State == E_AudioSessionState.AudioSessionStateExpired)
                                                _MISession_.Font = new System.Drawing.Font(_MISession_.Font, System.Drawing.FontStyle.Strikeout);

                                            _SessionList_.Add(_MISession_);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Trace.WriteLine($"exception:{ex.Message}");
                                }
                            }

                            _SessionList_.Sort(CompareMenuItem);

                            #region *** Auto entry ***
                            var _MIAuto_ = new ToolStripMenuItem("Auto") { Tag = AutoSessionMarker };
                            _MIAuto_.Click += CMI_Click;

                            if ((ActualDevice != null) && (ActualDevice.ID == _Device_.ID) && (_SelectedSession == AutoSessionMarker))
                                _MIAuto_.Checked = true;

                            _SessionList_.Insert(0, _MIAuto_);
                            #endregion

                            if ((ActualDevice != null) && (ActualDevice.ID == _Device_.ID) && (ActualSession == null) && (_SelectedSession != AutoSessionMarker))
                                _MIDevice_.DropDownItems.Add(new ToolStripMenuItem($"Suche nach {_SelectedSession}") { Enabled = false });
                            #endregion

                            _MIDevice_.DropDownItems.AddRange(_SessionList_.ToArray());
                            _DeviceList_.Add(_MIDevice_);
                        }
                    }

                _DeviceList_.Sort(CompareMenuItem);

                for (var _i_ = 0; _i_ < CMS_Main.Items.Count; _i_++)
                    while ((CMS_Main.Items.Count > _i_) && (CMS_Main.Items[_i_].Tag != null))
                        CMS_Main.Items.RemoveAt(_i_);

                var _p_ = CMS_Main.Items.IndexOfKey("MI_Placeholder");

                for (var _i_ = _DeviceList_.Count - 1; _i_ >= 0; _i_--)
                    CMS_Main.Items.Insert(_p_, _DeviceList_[_i_]);
            }

            Cursor = _Cursor_;
        }

        private void CMI_Click(object sender, EventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            var _MI_ = (ToolStripMenuItem)sender;

            if (_MI_.Tag is string _Tag_)
            {
                var _ParentTag_ = (string)_MI_.OwnerItem.Tag;

                if (_Tag_ == AutoSessionMarker)
                    AutoSelectNew(Search4Device(_ParentTag_));
                else
                {
                    ActualDevice = Search4Device(_ParentTag_);
                    ActualSession = Search4SessionID(ActualDevice, _Tag_);

                    _SelectedSession = ActualSession.GetSessionIdentifier;
                    _SelectedDevice = ActualDevice.ID;

                    ChangeVolumeDisplay(ActualSession.SimpleAudioVolume.MasterVolume);

                    UC_AI_Left.Value = 0;
                    UC_AI_Right.Value = 0;
                }
            }

            MakeFormTitle();

            Cursor = _Cursor_;
        }

        private void MI_Exit_Click(object sender, EventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            Application.Exit();

            Cursor = _Cursor_;
        }

        private void MI_Options_Click(object sender, EventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            using (var _Options_ = new F_Option())
                if (_Options_.ShowDialog(this) == DialogResult.OK)
                {
                    //ToDo:reload options
                    Settings.Default.Reload();
                    LoadOptions();
                }

            Cursor = _Cursor_;
        }

        private void TB_Volume_ValueChanged(object sender, EventArgs e)
        {
            var _Cursor_ = Cursor;
            Cursor = Cursors.WaitCursor;

            if (ActualSession != null)
                ActualSession.SimpleAudioVolume.MasterVolume = ((float)TB_Volume.Value) / 100;

            Cursor = _Cursor_;
        }

        private void L_ActiveSession_DoubleClick(object sender, EventArgs e)
        {
            if (ActualSession != null)
                ActualSession.SimpleAudioVolume.Mute = !ActualSession.SimpleAudioVolume.Mute;
        }

        private void L_ActiveDevice_DoubleClick(object sender, EventArgs e)
        {
            if (ActualDevice != null)
                ActualDevice.AudioEndpointVolume.Mute = !ActualDevice.AudioEndpointVolume.Mute;
        }
        #endregion

        private void Init()
        {
            LoadOptions();
            InitUDP();

            if (Settings.Default.AudioDeviceID != null)
            {
                _SelectedSession = Settings.Default.AudioSessionName;
                _SelectedDevice = Settings.Default.AudioDeviceID;
                ActualDevice = Search4Device(Settings.Default.AudioDeviceID);

                if (ActualDevice != null)
                    ActualSession = Search4SessionID(ActualDevice, Settings.Default.AudioSessionName);

                MakeFormTitle();
            }

            StartAudioMeter();

            // register system global hotkeys
            _VolUp = new C_GlobalHotKey();
            _VolUp.HotkeyPressed += VolUp_HotkeyPressed;
            _VolUp.RegisterGlobalHotKey((int)Keys.Add, WindowsConsts.MOD_ALT | WindowsConsts.MOD_CONTROL, Handle);

            _VolDown = new C_GlobalHotKey();
            _VolDown.HotkeyPressed += VolDown_HotkeyPressed;
            _VolDown.RegisterGlobalHotKey((int)Keys.Subtract, WindowsConsts.MOD_ALT | WindowsConsts.MOD_CONTROL, Handle);

            _Mute = new C_GlobalHotKey();
            _Mute.HotkeyPressed += Mute_HotkeyPressed;
            _Mute.RegisterGlobalHotKey((int)Keys.Enter, WindowsConsts.MOD_ALT | WindowsConsts.MOD_CONTROL, Handle);
        }

        private void VolUp_HotkeyPressed(object sender, EventArgs e)
        {
            if (TB_Volume.Enabled)
                if (ActualSession != null)
                    if (ActualSession.SimpleAudioVolume.MasterVolume + 0.10F <= 1F)
                        ActualSession.SimpleAudioVolume.MasterVolume += 0.10F;
                    else
                        ActualSession.SimpleAudioVolume.MasterVolume = 1F;
        }

        private void VolDown_HotkeyPressed(object sender, EventArgs e)
        {
            if (TB_Volume.Enabled)
                if (ActualSession != null)
                    if (ActualSession.SimpleAudioVolume.MasterVolume - 0.10F >= 0F)
                        ActualSession.SimpleAudioVolume.MasterVolume -= 0.10F;
                    else
                        ActualSession.SimpleAudioVolume.MasterVolume = 0F;
        }

        private void Mute_HotkeyPressed(object sender, EventArgs e)
        {
            if (TB_Volume.Enabled)
                ActualSession.SimpleAudioVolume.Mute = !ActualSession.SimpleAudioVolume.Mute;
        }

        private void StartAudioMeter()
        {
            _AudioMeterUpdater = new Timer() { Interval = UpdateInterval };
            _AudioMeterUpdater.Tick += new EventHandler(AudioMeterUpdater_Tick);

            _AudioMeterUpdater.Start();
        }

        private void LoadOptions()
        {
            _Send2UDP = Settings.Default.Option_SendUDP;
            _LastVolume = new C_RingBuffer<float>(Settings.Default.Option_SilentTime / UpdateInterval);
        }

        #region CoreAudio
        private C_MMDevice ActualDevice
        {
            get
            {
                return _ActualDevice;
            }
            set
            {
                if ((_ActualDevice == null) || (value == null) || (_ActualDevice.ID != value.ID))
                {
                    if (_ActualDevice != null)
                        _ActualDevice.Dispose();
                    _ActualDevice = value;
                }
            }
        }

        private C_AudioSessionControl2 ActualSession
        {
            get
            {
                return _ActualSession;
            }
            set
            {
                if (_ActualSession != null)
                    _ActualSession.Dispose();
                _ActualSession = value;
            }
        }

        private static C_MMDevice Search4Device(string AudioDeviceID)
        {
            using (var _DeviceEnum_ = new C_MMDeviceEnumerator())
            using (var _DeviceCollection_ = _DeviceEnum_.EnumerateAudioEndPoints(E_DataFlow.eRender, E_DeviceState.DEVICE_STATE_ACTIVE))
                for (var _i_ = 0; _i_ < _DeviceCollection_.Count; _i_++)
                    using (var _Device_ = _DeviceCollection_[_i_])
                        if (_Device_.ID == AudioDeviceID)
                            return _DeviceCollection_[_i_];

            return null;
        }

        private static C_MMDevice Search4Device(C_AudioSessionControl2 Session)
        {
            using (var _DeviceEnum_ = new C_MMDeviceEnumerator())
            using (var _DeviceCollection_ = _DeviceEnum_.EnumerateAudioEndPoints(E_DataFlow.eRender, E_DeviceState.DEVICE_STATE_ACTIVE))
                for (var _i_ = 0; _i_ < _DeviceCollection_.Count; _i_++)
                {
                    using (var _Device_ = _DeviceCollection_[_i_])
                    using (var _Manager_ = _Device_.AudioSessionManager2)
                    {
                        using (var _Sessions_ = _Manager_.Sessions)
                            for (var _j_ = 0; _j_ < _Sessions_.Count; _j_++)
                                using (var _Session_ = _Sessions_[_j_])
                                    if ((Session.GetSessionIdentifier == _Session_.GetSessionIdentifier) && (_Session_.State == E_AudioSessionState.AudioSessionStateActive))
                                        return _DeviceCollection_[_i_];
                    }
                }

            return null;
        }

        private C_AudioSessionControl2 Search4Session(string SessionIdentifier)
        {
            using (var _DeviceEnum_ = new C_MMDeviceEnumerator())
            {
                using (var _DeviceCollection_ = _DeviceEnum_.EnumerateAudioEndPoints(E_DataFlow.eRender, E_DeviceState.DEVICE_STATE_ACTIVE))
                {
                    for (var _i_ = 0; _i_ < _DeviceCollection_.Count; _i_++)
                    {
                        using (var _Device_ = _DeviceCollection_[_i_])
                        {
                            using (var _Manager_ = _Device_.AudioSessionManager2)
                            {
                                using (var _Sessions_ = _Manager_.Sessions)
                                    for (var _j_ = 0; _j_ < _Sessions_.Count; _j_++)
                                    {
                                        if ((SessionIdentifier == _Sessions_[_j_].GetSessionIdentifier) && (_Sessions_[_j_].State == E_AudioSessionState.AudioSessionStateActive))
                                            return _Sessions_[_j_];

                                        // nimm die nächst beste session die grad was abspielt... wenn auto gewählt ist ;)
                                        if ((SessionIdentifier == AutoSessionMarker) && (_DeviceCollection_[_i_].ID == _SelectedDevice) && (_Sessions_[_j_].AudioMeterInformation.MasterPeakValue != 0F) && (!GetSessionName(_Sessions_[_j_]).StartsWith("svchost", StringComparison.InvariantCultureIgnoreCase)))
                                            return _Sessions_[_j_];

                                        _Sessions_[_j_].Dispose();
                                    }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static C_AudioSessionControl2 Search4Session(C_MMDevice Device, string SessionIdentifier)
        {
            using (var _Manager_ = Device.AudioSessionManager2)
            {
                using (var _Sessions_ = _Manager_.Sessions)
                {
                    for (var _j_ = 0; _j_ < _Sessions_.Count; _j_++)
                    {
                        if ((SessionIdentifier == _Sessions_[_j_].GetSessionIdentifier) && (_Sessions_[_j_].State == E_AudioSessionState.AudioSessionStateActive))
                            return _Sessions_[_j_];

                        _Sessions_[_j_].Dispose();
                    }
                }
            }

            return null;
        }

        private static C_AudioSessionControl2 Search4SessionID(C_MMDevice Device, string SessionIdentifier)
        {
            using (var _Manager_ = Device.AudioSessionManager2)
            {
                using (var _Sessions_ = _Manager_.Sessions)
                {
                    for (var _j_ = 0; _j_ < _Sessions_.Count; _j_++)
                    {
                        if (SessionIdentifier == _Sessions_[_j_].GetSessionIdentifier)
                            return _Sessions_[_j_];

                        _Sessions_[_j_].Dispose();
                    }
                }
            }

            return null;
        }

        private static string GetDeviceName(C_MMDevice Device)
        {
            if (Device != null)
                for (var _x_ = 0; _x_ < Device.Properties.Count; _x_++)
                    if (Device.Properties[_x_].Key == C_PKEY.PKEY_Device_DeviceDesc)
                        return Device.Properties[_x_].Value.ToString();

            return string.Empty;
        }

        private string GetSessionName(C_AudioSessionControl2 Session)
        {
            if (Session != null)
            {
                try
                {
                    var _pid_ = System.Diagnostics.Process.GetProcessById((int)Session.GetProcessID);

                    try
                    {
                        return $"{_pid_.MainModule.FileVersionInfo.FileDescription} ({_pid_.ProcessName})";
                    }
                    catch
                    {
                        return $"{_pid_.ProcessName} [{Session.GetProcessID}]";
                    }
                }
                catch
                {
                    return $"PID:{Session.GetProcessID}";
                }
            }

            if (_SelectedSession == AutoSessionMarker)
                return "no Active Session";
            else
                return $" ... {_SelectedSession}";
        }

        private void AutoSelectNew(C_MMDevice tmpDevice)
        {
            ActualSession = null;
            ActualDevice = tmpDevice;

            _SelectedSession = AutoSessionMarker;
            _SelectedDevice = ActualDevice.ID;

            ChangeVolumeDisplay(0);

            UC_AI_Left.Value = 0;
            UC_AI_Right.Value = 0;
        }
        #endregion

        delegate void ChangeVolumeDisplayCallback(float NewVolume);
        private void ChangeVolumeDisplay(float NewVolume)
        {
            if (TB_Volume.InvokeRequired)
                Invoke(new ChangeVolumeDisplayCallback(ChangeVolumeDisplay), new object[] { NewVolume });
            else
                TB_Volume.Value = (int)(NewVolume * 100);
        }

        private void ChangeMuted()
        {
            if (_DeviceMuted)
                L_ActiveDevice.Font = new System.Drawing.Font(L_ActiveDevice.Font, System.Drawing.FontStyle.Strikeout);
            else
                L_ActiveDevice.Font = new System.Drawing.Font(L_ActiveDevice.Font, System.Drawing.FontStyle.Regular);

            if (_SessionMuted)
                L_ActiveSession.Font = new System.Drawing.Font(L_ActiveDevice.Font, System.Drawing.FontStyle.Strikeout);
            else
                L_ActiveSession.Font = new System.Drawing.Font(L_ActiveDevice.Font, System.Drawing.FontStyle.Regular);
        }

        private void MakeFormTitle()
        {
            var _DeviceName_ = string.Empty;

            if (ActualDevice != null)
                _DeviceName_ = ActualDevice.FriendlyName;

            var _SessionName_ = GetSessionName(ActualSession);

            Text = $"{FormTitle} - {_DeviceName_}/{_SessionName_}";
            L_ActiveDevice.Text = string.Format(Settings.Default.Option_UpperText, FormTitle, _DeviceName_, _SessionName_);
            L_ActiveSession.Text = string.Format(Settings.Default.Option_LowerText, FormTitle, _DeviceName_, _SessionName_);
        }

        private bool SessionMuted
        {
            get
            {
                return _SessionMuted;
            }
            set
            {
                if (_SessionMuted != value)
                {
                    _SessionMuted = value;
                    ChangeMuted();
                }
            }
        }

        private bool DeviceMuted
        {
            get
            {
                return _DeviceMuted;
            }
            set
            {
                if (_DeviceMuted != value)
                {
                    _DeviceMuted = value;
                    ChangeMuted();
                }
            }
        }

        private int CompareMenuItem(ToolStripMenuItem x, ToolStripMenuItem y)
        {
            return string.Compare(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        #region udp broadcast
        private void InitUDP()
        {
            _ServerGuid = Guid.NewGuid();
            _UdpClient = new UdpClient() { EnableBroadcast = true };
            _BroadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 42000);
            SendUDP("I'm new");
        }

        private void SendUDP(string Message)
        {
            if (_Send2UDP)
            {
                var _ByteMessage_ = System.Text.Encoding.ASCII.GetBytes($"{_ServerGuid}:{Message}");
                _UdpClient.Send(_ByteMessage_, _ByteMessage_.Length, _BroadcastEndPoint);
            }
        }
        #endregion
    }
}

