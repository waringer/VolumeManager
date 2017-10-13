// Based on code from https://github.com/ThiefMaster/coreaudio-dotnet.git

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VolumeManager
{
    #region enums
    public enum E_DataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2,
        EDataFlow_enum_count = 3
    }

    public enum E_Role
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2,
        ERole_enum_count = 3
    }

    [Flags]
    public enum E_DeviceState : uint
    {
        DEVICE_STATE_ACTIVE = 0x00000001,
        DEVICE_STATE_DISABLED = 0x00000002,
        DEVICE_STATE_NOTPRESENT = 0x00000004,
        DEVICE_STATE_UNPLUGGED = 0x00000008,
        DEVICE_STATEMASK_ALL = 0x0000000F
    }

    [Flags]
    internal enum E_CLSCTX : uint
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        INPROC_SERVER16 = 0x8,
        REMOTE_SERVER = 0x10,
        INPROC_HANDLER16 = 0x20,
        RESERVED1 = 0x40,
        RESERVED2 = 0x80,
        RESERVED3 = 0x100,
        RESERVED4 = 0x200,
        NO_CODE_DOWNLOAD = 0x400,
        RESERVED5 = 0x800,
        NO_CUSTOM_MARSHAL = 0x1000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        NO_FAILURE_LOG = 0x4000,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        FROM_DEFAULT_CONTEXT = 0x20000,
        INPROC = INPROC_SERVER | INPROC_HANDLER,
        SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
        ALL = SERVER | INPROC_HANDLER
    }

    internal enum E_StgmAccess
    {
        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002
    }

    [Flags]
    public enum E_EndpointHardwareSupport
    {
        Volume = 0x00000001,
        Mute = 0x00000002,
        Meter = 0x00000004
    }

    public enum E_AudioSessionState
    {
        AudioSessionStateInactive = 0,
        AudioSessionStateActive = 1,
        AudioSessionStateExpired = 2
    }

    public enum E_AudioSessionDisconnectReason
    {
        DisconnectReasonDeviceRemoval = 0,
        DisconnectReasonServerShutdown = (DisconnectReasonDeviceRemoval + 1),
        DisconnectReasonFormatChanged = (DisconnectReasonServerShutdown + 1),
        DisconnectReasonSessionLogoff = (DisconnectReasonFormatChanged + 1),
        DisconnectReasonSessionDisconnected = (DisconnectReasonSessionLogoff + 1),
        DisconnectReasonExclusiveModeOverride = (DisconnectReasonSessionDisconnected + 1)
    }

    public enum E_ConnectorType
    {
        Unknown_Connector = 0,
        Physical_Internal = (Unknown_Connector + 1),
        Physical_External = (Physical_Internal + 1),
        Software_IO = (Physical_External + 1),
        Software_Fixed = (Software_IO + 1),
        Network = (Software_Fixed + 1)
    }

    public enum E_PartType
    {
        Connector = 0,
        Subunit = (Connector + 1)
    }

    [Flags]
    public enum E_AudioClientReturnFlags : uint
    {
        S_OK = 0,
        AUDCLNT_E_NOT_INITIALIZED = 0x001,
        AUDCLNT_E_ALREADY_INITIALIZED = 0x002,
        AUDCLNT_E_WRONG_ENDPOINT_TYPE = 0x003,
        AUDCLNT_E_DEVICE_INVALIDATED = 0x004,
        AUDCLNT_E_NOT_STOPPED = 0x005,
        AUDCLNT_E_BUFFER_TOO_LARGE = 0x006,
        AUDCLNT_E_OUT_OF_ORDER = 0x007,
        AUDCLNT_E_UNSUPPORTED_FORMAT = 0x008,
        AUDCLNT_E_INVALID_SIZE = 0x009,
        AUDCLNT_E_DEVICE_IN_USE = 0x00a,
        AUDCLNT_E_BUFFER_OPERATION_PENDING = 0x00b,
        AUDCLNT_E_THREAD_NOT_REGISTERED = 0x00c,
        AUDCLNT_E_EXCLUSIVE_MODE_NOT_ALLOWED = 0x00e,
        AUDCLNT_E_ENDPOINT_CREATE_FAILED = 0x00f,
        AUDCLNT_E_SERVICE_NOT_RUNNING = 0x010,
        AUDCLNT_E_EVENTHANDLE_NOT_EXPECTED = 0x011,
        AUDCLNT_E_EXCLUSIVE_MODE_ONLY = 0x012,
        AUDCLNT_E_BUFDURATION_PERIOD_NOT_EQUAL = 0x013,
        AUDCLNT_E_EVENTHANDLE_NOT_SET = 0x014,
        AUDCLNT_E_INCORRECT_BUFFER_SIZE = 0x015,
        AUDCLNT_E_BUFFER_SIZE_ERROR = 0x016,
        AUDCLNT_E_CPUUSAGE_EXCEEDED = 0x017,
        AUDCLNT_E_BUFFER_ERROR = 0x018,
        AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED = 0x019,
        AUDCLNT_E_INVALID_DEVICE_PERIOD = 0x020,
        AUDCLNT_S_BUFFER_EMPTY = 0x001,
        AUDCLNT_S_THREAD_ALREADY_REGISTERED = 0x002,
        AUDCLNT_S_POSITION_STALLED = 0x003
    }

    public enum E_AudioClientBufferFlags
    {
        AUDCLNT_BUFFERFLAGS_DATA_DISCONTINUITY = 0x1,
        AUDCLNT_BUFFERFLAGS_SILENT = 0x2,
        AUDCLNT_BUFFERFLAGS_TIMESTAMP_ERROR = 0x4
    }
    #endregion

    #region structs
    internal struct S_Blob
    {
        public int Length;
        public IntPtr Data;

        private void FixCS0649()
        {
            Length = 0;
            Data = IntPtr.Zero;
        }
    }

    public struct S_PropertyKey
    {
        public Guid fmtid;
        public int pid;

        public S_PropertyKey(Guid fmtid, int pid)
        {
            this.fmtid = fmtid;
            this.pid = pid;
        }

        public static bool operator ==(S_PropertyKey pk1, S_PropertyKey pk2)
        {
            return (pk1.fmtid == pk2.fmtid) && (pk1.pid == pk2.pid);
        }

        public static bool operator !=(S_PropertyKey pk1, S_PropertyKey pk2)
        {
            return !(pk1 == pk2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct S_PropVariant
    {
        [FieldOffset(0)]
        short vt;
        [FieldOffset(8)]
        byte bVal;
        [FieldOffset(8)]
        short iVal;
        [FieldOffset(8)]
        int lVal;
        [FieldOffset(8)]
        uint ulVal;
        [FieldOffset(8)]
        long hVal;
        [FieldOffset(8)]
        S_Blob blobVal;
        [FieldOffset(8)]
        IntPtr everything_else;

        //I'm sure there is a more efficient way to do this but this works ..for now..
        internal byte[] GetBlob()
        {
            var _Result_ = new byte[blobVal.Length];

            for (var _i_ = 0; _i_ < blobVal.Length; _i_++)
                _Result_[_i_] = Marshal.ReadByte((IntPtr)((long)(blobVal.Data) + _i_));

            return _Result_;
        }

        public object Value
        {
            get
            {
                var _ve_ = (VarEnum)vt;
                switch (_ve_)
                {
                    case VarEnum.VT_I1:
                        return bVal;
                    case VarEnum.VT_I2:
                        return iVal;
                    case VarEnum.VT_I4:
                        return lVal;
                    case VarEnum.VT_I8:
                        return hVal;
                    case VarEnum.VT_INT:
                        return iVal;
                    case VarEnum.VT_UI4:
                        return ulVal;
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(everything_else);
                    case VarEnum.VT_BLOB:
                        return GetBlob();
                }
                return $"FIXME Type = {_ve_.ToString()}";
            }
        }
    }

    internal struct S_AudioVolumeNotificationData
    {
        public Guid guidEventContext;
        public bool bMuted;
        public float fMasterVolume;
        public uint nChannels;
        public float ChannelVolume;

        //Code Should Compile at warning level4 without any warnings, 
        //However this struct will give us Warning CS0649: Field [Fieldname] 
        //is never assigned to, and will always have its default value
        //You can disable CS0649 in the project options but that will disable
        //the warning for the whole project, it's a nice warning and we do want 
        //it in other places so we make a nice dummy function to keep the compiler
        //happy.
        private void FixCS0649()
        {
            guidEventContext = Guid.Empty;
            bMuted = false;
            fMasterVolume = 0;
            nChannels = 0;
            ChannelVolume = 0;
        }
    }

    public struct S_LevelRange
    {
        public float minLevel;
        public float maxLevel;
        public float stepping;

        public S_LevelRange(float minLevel, float maxLevel, float stepping)
        {
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
            this.stepping = stepping;
        }
    }
    #endregion

    #region interfaces
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out int count);
        [PreserveSig]
        int GetAt(int iProp, out S_PropertyKey pkey);
        [PreserveSig]
        int GetValue(ref S_PropertyKey key, out S_PropVariant pv);
        [PreserveSig]
        int SetValue(ref S_PropertyKey key, ref S_PropVariant propvar);
        [PreserveSig]
        int Commit();
    };

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, E_CLSCTX dwClsCtx, IntPtr pActivationParams, [Out(), MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig]
        int OpenPropertyStore(E_StgmAccess stgmAccess, out IPropertyStore propertyStore);
        [PreserveSig]
        int GetId([Out(), MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig]
        int GetState(out E_DeviceState pdwState);
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(E_DataFlow dataFlow, E_DeviceState StateMask, out IMMDeviceCollection device);
        [PreserveSig]
        int GetDefaultAudioEndpoint(E_DataFlow dataFlow, E_Role role, out IMMDevice ppEndpoint);
        [PreserveSig]
        int GetDevice(string pwstrId, out IMMDevice ppDevice);
        [PreserveSig]
        int RegisterEndpointNotificationCallback(IntPtr pClient);
        [PreserveSig]
        int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(out uint pcDevices);
        [PreserveSig]
        int Item(uint nDevice, out IMMDevice Device);
    }

    [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioMeterInformation
    {
        [PreserveSig]
        int GetPeakValue(out float pfPeak);
        [PreserveSig]
        int GetMeteringChannelCount(out int pnChannelCount);
        [PreserveSig]
        int GetChannelsPeakValues(int u32ChannelCount, [In] IntPtr afPeakValues);
        [PreserveSig]
        int QueryHardwareSupport(out int pdwHardwareSupportMask);
    };

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        [PreserveSig]
        int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        [PreserveSig]
        int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        [PreserveSig]
        int GetChannelCount(out int pnChannelCount);
        [PreserveSig]
        int SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);
        [PreserveSig]
        int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);
        [PreserveSig]
        int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig]
        int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig]
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);
        [PreserveSig]
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);
        [PreserveSig]
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        [PreserveSig]
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        [PreserveSig]
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, Guid pguidEventContext);
        [PreserveSig]
        int GetMute(out bool pbMute);
        [PreserveSig]
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        [PreserveSig]
        int VolumeStepUp(Guid pguidEventContext);
        [PreserveSig]
        int VolumeStepDown(Guid pguidEventContext);
        [PreserveSig]
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        [PreserveSig]
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolumeCallback
    {
        [PreserveSig]
        int OnNotify(IntPtr pNotifyData);
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        [PreserveSig]
        int GetAudioSessionControl(ref Guid AudioSessionGuid, uint StreamFlags, out IAudioSessionControl2 ISessionControl);
        [PreserveSig]
        int GetSimpleAudioVolume(ref Guid AudioSessionGuid, uint StreamFlags, out ISimpleAudioVolume SimpleAudioVolume);
        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
        [PreserveSig]
        int RegisterSessionNotification(IAudioSessionNotification SessionNotification);
        [PreserveSig]
        int UnregisterSessionNotification(IAudioSessionNotification SessionNotification);
        [PreserveSig]
        int RegisterDuckNotification(string sessionID, IAudioSessionNotification IAudioVolumeDuckNotification);
        [PreserveSig]
        int UnregisterDuckNotification(IntPtr IAudioVolumeDuckNotification);
    };

    [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControl2
    {
        [PreserveSig]
        int GetState(out E_AudioSessionState state);
        [PreserveSig]
        int GetDisplayName([Out(), MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig]
        int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string value, Guid EventContext);
        [PreserveSig]
        int GetIconPath([Out(), MarshalAs(UnmanagedType.LPWStr)] out string Path);
        [PreserveSig]
        int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, Guid EventContext);
        [PreserveSig]
        int GetGroupingParam(out Guid GroupingParam);
        [PreserveSig]
        int SetGroupingParam(Guid Override, Guid Eventcontext);
        [PreserveSig]
        int RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        [PreserveSig]
        int UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        [PreserveSig]
        int GetSessionIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
        [PreserveSig]
        int GetSessionInstanceIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
        [PreserveSig]
        int GetProcessId(out uint retvVal);
        [PreserveSig]
        int IsSystemSoundsSession();
        [PreserveSig]
        int SetDuckingPreference(bool optOut);
    }

    [Guid("24918ACC-64B3-37C1-8CA9-74A66E9957A8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionEvents
    {
        [PreserveSig]
        int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, Guid EventContext);
        [PreserveSig]
        int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, Guid EventContext);
        [PreserveSig]
        int OnSimpleVolumeChanged(float NewVolume, bool newMute, Guid EventContext);
        [PreserveSig]
        int OnChannelVolumeChanged(uint ChannelCount, IntPtr NewChannelVolumeArray, uint ChangedChannel, Guid EventContext);
        [PreserveSig]
        int OnGroupingParamChanged(Guid NewGroupingParam, Guid EventContext);
        [PreserveSig]
        int OnStateChanged(E_AudioSessionState NewState);
        [PreserveSig]
        int OnSessionDisconnected(E_AudioSessionDisconnectReason DisconnectReason);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid EventContext);
        [PreserveSig]
        int GetMasterVolume(out float pfLevel);
        [PreserveSig]
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, Guid EventContext);
        [PreserveSig]
        int GetMute(out bool bMute);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        int GetCount(out int SessionCount);
        int GetSession(int SessionCount, out IAudioSessionControl2 Session);
    }

    [Guid("641DD20B-4D41-49CC-ABA3-174B9477BB08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionNotification
    {
        [PreserveSig]
        int OnSessionCreated(IAudioSessionControl2 NewSession);
    }

    [Guid("9c2c4058-23f5-41de-877a-df3af236a09e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IConnector
    {
        [PreserveSig]
        int GetType(out E_ConnectorType type);
        [PreserveSig]
        int GetDataFlow(out E_DataFlow flow);
        [PreserveSig]
        int ConnectTo(IConnector connectTo);
        [PreserveSig]
        int Disconnect();
        [PreserveSig]
        int IsConnected(out bool connected);
        [PreserveSig]
        int GetConnectedTo(out IConnector connectedTo);
        [PreserveSig]
        int GetConnectorIdConnectedTo([Out(), MarshalAs(UnmanagedType.LPWStr)] out string connectorId);
        [PreserveSig]
        int GetDeviceIdConnectedTo([Out(), MarshalAs(UnmanagedType.LPWStr)] out string deviceId);
    }

    [Guid("82149A85-DBA6-4487-86BB-EA8F7FEFCC71"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISubunit
    { }

    [Guid("6DAA848C-5EB0-45CC-AEA5-998A2CDA1FFB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPartsList
    {
        [PreserveSig]
        int GetCount(out int count);
        [PreserveSig]
        int GetPart(int index, out IPart part);
    }

    [Guid("45d37c3f-5140-444a-ae24-400789f3cbf3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IControlInterface
    {
        [PreserveSig]
        int GetName([Out(), MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig]
        int GetID(out Guid id);
    }

    [Guid("A09513ED-C709-4d21-BD7B-5F34C47F3947"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IControlChangeNotify
    {
        [PreserveSig]
        int OnNotify(uint dwSenderProcessId, ref Guid pguidEventContext);
    }

    [Guid("AE2DE0E4-5BCA-4F2D-AA46-5D13F8FDB3A9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPart
    {
        [PreserveSig]
        int GetName([Out(), MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig]
        int GetLocalId(out int id);
        [PreserveSig]
        int GetGlobalId([Out(), MarshalAs(UnmanagedType.LPWStr)] out string globalId);
        [PreserveSig]
        int GetPartType(out E_PartType partType);
        [PreserveSig]
        int GetSubType(out Guid subType);
        [PreserveSig]
        int GetControlInterfaceCount(out int count);
        [PreserveSig]
        int GetControlInterface(int index, out IControlInterface pInterface);
        [PreserveSig]
        int EnumPartsIncoming(out IPartsList parts);
        [PreserveSig]
        int EnumPartsOutgoing(out IPartsList parts);
        [PreserveSig]
        int GetTopologyObject(out IDeviceTopology topology);
        [PreserveSig]
        int Activate(E_CLSCTX dwClsContext, ref Guid refiid, [Out(), MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
        [PreserveSig]
        int RegisterControlChangeCallback(ref Guid refiid, IControlChangeNotify notify);
        [PreserveSig]
        int UnregisterControlChangeCallback(IControlChangeNotify notify);
    }

    [Guid("2A07407E-6497-4A18-9787-32F79BD0D98F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDeviceTopology
    {
        [PreserveSig]
        int GetConnectorCount(out int count);
        [PreserveSig]
        int GetConnector(int index, out IConnector connector);
        [PreserveSig]
        int GetSubunitCount(out int count);
        [PreserveSig]
        int GetSubunit(int index, out ISubunit subunit);
        [PreserveSig]
        int GetPartById(int id, out IPart part);
        [PreserveSig]
        int GetDeviceId([Out(), MarshalAs(UnmanagedType.LPWStr)] out string deviceId);
        [PreserveSig]
        int GetSignalPath(IPart partFrom, IPart partTo, bool rejectMixedPaths, out IPartsList parts);
    }

    [Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioVolumeLevel : IPerChannelDbLevel
    { }

    [Guid("C2F8E001-F205-4BC9-99BC-C13B1E048CCB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPerChannelDbLevel
    {
        [PreserveSig]
        int GetChannelCount(out uint count);
        [PreserveSig]
        int GetLevelRange(uint channel, out float minLevel, out float maxLevel, out float stepping);
        [PreserveSig]
        int GetLevel(uint channel, out float level);
        [PreserveSig]
        int SetLevel(uint channel, float level, out Guid eventContext);
        [PreserveSig]
        int SetLevelUniform(float level, out Guid eventContext);
        [PreserveSig]
        int SetLevelAllChannels(float[] levelsDB, ulong channels, Guid eventContext);
    }

    [Guid("DF45AEEA-B74A-4B6B-AFAD-2366B6AA012E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioMute
    {
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool muted, out Guid eventContext);
        int GetMute([Out(), MarshalAs(UnmanagedType.Bool)] out bool muted);
    }

    [Guid("DD79923C-0599-45e0-B8B6-C8DF7DB6E796"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioPeakMeter
    {
        int GetChannelCount(out int pcChannels);
        int GetLevel(uint channel, out float level);
    }

    [Guid("7D8B1437-DD53-4350-9C1B-1EE2890BD938"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioLoudness
    {
        int GetEnabled([Out(), MarshalAs(UnmanagedType.Bool)] out bool enabled);
        int SetEnabled([MarshalAs(UnmanagedType.Bool)] bool enabled, out Guid eventContext);
    }

    [Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioCaptureClient
    {
        [PreserveSig]
        E_AudioClientReturnFlags GetBuffer(out byte[] ppData, out E_AudioClientBufferFlags pNumFramesToRead, ulong pu64DevicePosition, ulong pu64QPCPosition);
        [PreserveSig]
        E_AudioClientReturnFlags ReleaseBuffer(uint NumFramesRead);
        [PreserveSig]
        E_AudioClientReturnFlags GetNextPacketSize(out uint pNumFramesInNextPacket);
    }

    [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMEndpoint
    {
        [PreserveSig]
        int GetDataFlow(out E_DataFlow pDataFlow);
    };

    [Guid("568b9108-44bf-40b4-9006-86afe5b5a620"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfigVista
    {
        [PreserveSig]
        int GetMixFormat();
        [PreserveSig]
        int GetDeviceFormat();
        [PreserveSig]
        int SetDeviceFormat();
        [PreserveSig]
        int GetProcessingPeriod();
        [PreserveSig]
        int SetProcessingPeriod();
        [PreserveSig]
        int GetShareMode();
        [PreserveSig]
        int SetShareMode();
        [PreserveSig]
        int GetPropertyValue();
        [PreserveSig]
        int SetPropertyValue();
        [PreserveSig]
        int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, E_Role eRole);
        [PreserveSig]
        int SetEndpointVisibility();
    }
    #endregion

    #region constants
    internal static class C_IIDs
    {
        public static Guid IID_IAudioVolumeLevel = typeof(IAudioVolumeLevel).GUID;
        public static Guid IID_IAudioMute = typeof(IAudioMute).GUID;
        public static Guid IID_IAudioPeakMeter = typeof(IAudioPeakMeter).GUID;
        public static Guid IID_IAudioLoudness = typeof(IAudioLoudness).GUID;

        public static Guid IID_IAudioMeterInformation = typeof(IAudioMeterInformation).GUID;
        public static Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
        public static Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
        public static Guid IID_IDeviceTopology = typeof(IDeviceTopology).GUID;

        public static Guid IID_IPart = typeof(IPart).GUID;
    }

    public static class C_KSNODETYPE
    {
        public static Guid INPUT_UNDEFINED = new Guid("DFF21BE0-F70F-11D0-B917-00A0C9223196");
        public static Guid MICROPHONE = new Guid("DFF21BE1-F70F-11D0-B917-00A0C9223196");
        public static Guid DESKTOP_MICROPHONE = new Guid("DFF21BE2-F70F-11D0-B917-00A0C9223196");
        public static Guid PERSONAL_MICROPHONE = new Guid("DFF21BE3-F70F-11D0-B917-00A0C9223196");
        public static Guid OMNI_DIRECTIONAL_MICROPHONE = new Guid("DFF21BE4-F70F-11D0-B917-00A0C9223196");
        public static Guid MICROPHONE_ARRAY = new Guid("DFF21BE5-F70F-11D0-B917-00A0C9223196");
        public static Guid PROCESSING_MICROPHONE_ARRAY = new Guid("DFF21BE6-F70F-11D0-B917-00A0C9223196");
        public static Guid OUTPUT_UNDEFINED = new Guid("DFF21CE0-F70F-11D0-B917-00A0C9223196");
        public static Guid SPEAKER = new Guid("DFF21CE1-F70F-11D0-B917-00A0C9223196");
        public static Guid HEADPHONES = new Guid("DFF21CE2-F70F-11D0-B917-00A0C9223196");
        public static Guid HEAD_MOUNTED_DISPLAY_AUDIO = new Guid("DFF21CE3-F70F-11D0-B917-00A0C9223196");
        public static Guid DESKTOP_SPEAKER = new Guid("DFF21CE4-F70F-11D0-B917-00A0C9223196");
        public static Guid ROOM_SPEAKER = new Guid("DFF21CE5-F70F-11D0-B917-00A0C9223196");
        public static Guid COMMUNICATION_SPEAKER = new Guid("DFF21CE6-F70F-11D0-B917-00A0C9223196");
        public static Guid LOW_FREQUENCY_EFFECTS_SPEAKER = new Guid("DFF21CE7-F70F-11D0-B917-00A0C9223196");
        public static Guid BIDIRECTIONAL_UNDEFINED = new Guid("DFF21DE0-F70F-11D0-B917-00A0C9223196");
        public static Guid HANDSET = new Guid("DFF21DE1-F70F-11D0-B917-00A0C9223196");
        public static Guid HEADSET = new Guid("DFF21DE2-F70F-11D0-B917-00A0C9223196");
        public static Guid SPEAKERPHONE_NO_ECHO_REDUCTION = new Guid("DFF21DE3-F70F-11D0-B917-00A0C9223196");
        public static Guid ECHO_SUPPRESSING_SPEAKERPHONE = new Guid("DFF21DE4-F70F-11D0-B917-00A0C9223196");
        public static Guid ECHO_CANCELING_SPEAKERPHONE = new Guid("DFF21DE5-F70F-11D0-B917-00A0C9223196");
        public static Guid TELEPHONY_UNDEFINED = new Guid("DFF21EE0-F70F-11D0-B917-00A0C9223196");
        public static Guid PHONE_LINE = new Guid("DFF21EE1-F70F-11D0-B917-00A0C9223196");
        public static Guid TELEPHONE = new Guid("DFF21EE2-F70F-11D0-B917-00A0C9223196");
        public static Guid DOWN_LINE_PHONE = new Guid("DFF21EE3-F70F-11D0-B917-00A0C9223196");
        public static Guid EXTERNAL_UNDEFINED = new Guid("DFF21FE0-F70F-11D0-B917-00A0C9223196");
        public static Guid ANALOG_CONNECTOR = new Guid("DFF21FE1-F70F-11D0-B917-00A0C9223196");
        public static Guid DIGITAL_AUDIO_INTERFACE = new Guid("DFF21FE2-F70F-11D0-B917-00A0C9223196");
        public static Guid LINE_CONNECTOR = new Guid("DFF21FE3-F70F-11D0-B917-00A0C9223196");
        public static Guid LEGACY_AUDIO_CONNECTOR = new Guid("DFF21FE4-F70F-11D0-B917-00A0C9223196");
        public static Guid SPDIF_INTERFACE = new Guid("DFF21FE5-F70F-11D0-B917-00A0C9223196");
        public static Guid DA_STREAM_1394 = new Guid("DFF21FE6-F70F-11D0-B917-00A0C9223196");
        public static Guid DV_STREAM_1394_SOUNDTRACK = new Guid("DFF21FE7-F70F-11D0-B917-00A0C9223196");
        public static Guid EMBEDDED_UNDEFINED = new Guid("DFF220E0-F70F-11D0-B917-00A0C9223196");
        public static Guid LEVEL_CALIBRATION_NOISE_SOURCE = new Guid("DFF220E1-F70F-11D0-B917-00A0C9223196");
        public static Guid EQUALIZATION_NOISE = new Guid("DFF220E2-F70F-11D0-B917-00A0C9223196");
        public static Guid CD_PLAYER = new Guid("DFF220E3-F70F-11D0-B917-00A0C9223196");
        public static Guid DAT_IO_DIGITAL_AUDIO_TAPE = new Guid("DFF220E4-F70F-11D0-B917-00A0C9223196");
        public static Guid DCC_IO_DIGITAL_COMPACT_CASSETTE = new Guid("DFF220E5-F70F-11D0-B917-00A0C9223196");
        public static Guid MINIDISK = new Guid("DFF220E6-F70F-11D0-B917-00A0C9223196");
        public static Guid ANALOG_TAPE = new Guid("DFF220E7-F70F-11D0-B917-00A0C9223196");
        public static Guid PHONOGRAPH = new Guid("DFF220E8-F70F-11D0-B917-00A0C9223196");
        public static Guid VCR_AUDIO = new Guid("DFF220E9-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_DISC_AUDIO = new Guid("DFF220EA-F70F-11D0-B917-00A0C9223196");
        public static Guid DVD_AUDIO = new Guid("DFF220EB-F70F-11D0-B917-00A0C9223196");
        public static Guid TV_TUNER_AUDIO = new Guid("DFF220EC-F70F-11D0-B917-00A0C9223196");
        public static Guid SATELLITE_RECEIVER_AUDIO = new Guid("DFF220ED-F70F-11D0-B917-00A0C9223196");
        public static Guid CABLE_TUNER_AUDIO = new Guid("DFF220EE-F70F-11D0-B917-00A0C9223196");
        public static Guid DSS_AUDIO = new Guid("DFF220EF-F70F-11D0-B917-00A0C9223196");
        public static Guid RADIO_RECEIVER = new Guid("DFF220F0-F70F-11D0-B917-00A0C9223196");
        public static Guid RADIO_TRANSMITTER = new Guid("DFF220F1-F70F-11D0-B917-00A0C9223196");
        public static Guid MULTITRACK_RECORDER = new Guid("DFF220F2-F70F-11D0-B917-00A0C9223196");
        public static Guid SYNTHESIZER = new Guid("DFF220F3-F70F-11D0-B917-00A0C9223196");
        public static Guid HDMI_INTERFACE = new Guid("D1B9CC2A-F519-417f-91C9-55FA65481001");
        public static Guid DISPLAYPORT_INTERFACE = new Guid("E47E4031-3EA6-418d-8F9B-B73843CCBA97");
        public static Guid MIDI_JACK = new Guid("265E0C3F-FA39-4df3-AB04-BE01B91E299A");
        public static Guid MIDI_ELEMENT = new Guid("01C6FE66-6E48-4c65-AC9B-52DB5D656C7E");
        public static Guid SWSYNTH = new Guid("423274A0-8B81-11D1-A050-0000F8004788");
        public static Guid SWMIDI = new Guid("CB9BEFA0-A251-11D1-A050-0000F8004788");
        public static Guid DRM_DESCRAMBLE = new Guid("FFBB6E3F-CCFE-4D84-90D9-421418B03A8E");
        public static Guid DAC = new Guid("507AE360-C554-11D0-8A2B-00A0C9255AC1");
        public static Guid ADC = new Guid("4D837FE0-C555-11D0-8A2B-00A0C9255AC1");
        public static Guid SRC = new Guid("9DB7B9E0-C555-11D0-8A2B-00A0C9255AC1");
        public static Guid SUPERMIX = new Guid("E573ADC0-C555-11D0-8A2B-00A0C9255AC1");
        public static Guid MUX = new Guid("2CEAF780-C556-11D0-8A2B-00A0C9255AC1");
        public static Guid DEMUX = new Guid("C0EB67D4-E807-11D0-958A-00C04FB925D3");
        public static Guid SUM = new Guid("DA441A60-C556-11D0-8A2B-00A0C9255AC1");
        public static Guid MUTE = new Guid("02B223C0-C557-11D0-8A2B-00A0C9255AC1");
        public static Guid VOLUME = new Guid("3A5ACC00-C557-11D0-8A2B-00A0C9255AC1");
        public static Guid TONE = new Guid("7607E580-C557-11D0-8A2B-00A0C9255AC1");
        public static Guid EQUALIZER = new Guid("9D41B4A0-C557-11D0-8A2B-00A0C9255AC1");
        public static Guid AGC = new Guid("E88C9BA0-C557-11D0-8A2B-00A0C9255AC1");
        public static Guid NOISE_SUPPRESS = new Guid("E07F903F-62FD-4e60-8CDD-DEA7236665B5");
        public static Guid DELAY = new Guid("144981E0-C558-11D0-8A2B-00A0C9255AC1");
        public static Guid LOUDNESS = new Guid("41887440-C558-11D0-8A2B-00A0C9255AC1");
        public static Guid PROLOGIC_DECODER = new Guid("831C2C80-C558-11D0-8A2B-00A0C9255AC1");
        public static Guid STEREO_WIDE = new Guid("A9E69800-C558-11D0-8A2B-00A0C9255AC1");
        public static Guid REVERB = new Guid("EF0328E0-C558-11D0-8A2B-00A0C9255AC1");
        public static Guid CHORUS = new Guid("20173F20-C559-11D0-8A2B-00A0C9255AC1");
        public static Guid EFFECTS_3D = new Guid("55515860-C559-11D0-8A2B-00A0C9255AC1");
        public static Guid PARAMETRIC_EQUALIZER = new Guid("19BB3A6A-CE2B-4442-87EC-6727C3CAB477");
        public static Guid UPDOWN_MIX = new Guid("B7EDC5CF-7B63-4ee2-A100-29EE2CB6B2DE");
        public static Guid DYN_RANGE_COMPRESSOR = new Guid("08C8A6A8-601F-4af8-8793-D905FF4CA97D");
        public static Guid DEV_SPECIFIC = new Guid("941C7AC0-C559-11D0-8A2B-00A0C9255AC1");
        public static Guid PROLOGIC_ENCODER = new Guid("8074C5B2-3C66-11D2-B45A-3078302C2030");
        public static Guid PEAKMETER = new Guid("A085651E-5F0D-4b36-A869-D195D6AB4B9E");
        public static Guid SURROUND_ENCODER = new Guid("8074C5B2-3C66-11D2-B45A-3078302C2030");
        public static Guid VIDEO_STREAMING = new Guid("DFF229E1-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_INPUT_TERMINAL = new Guid("DFF229E2-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_OUTPUT_TERMINAL = new Guid("DFF229E3-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_SELECTOR = new Guid("DFF229E4-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_PROCESSING = new Guid("DFF229E5-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_CAMERA_TERMINAL = new Guid("DFF229E6-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_INPUT_MTT = new Guid("DFF229E7-F70F-11D0-B917-00A0C9223196");
        public static Guid VIDEO_OUTPUT_MTT = new Guid("DFF229E8-F70F-11D0-B917-00A0C9223196");
    }

    public static class C_KSCATEGORY
    {
        public static Guid MICROPHONE_ARRAY_PROCESSOR = new Guid("830a44f2-a32d-476b-be97-42845673b35a");
        public static Guid AUDIO = new Guid("6994AD04-93EF-11D0-A3CC-00A0C9223196");
        public static Guid VIDEO = new Guid("6994AD05-93EF-11D0-A3CC-00A0C9223196");
        public static Guid REALTIME = new Guid("EB115FFC-10C8-4964-831D-6DCB02E6F23F");
        public static Guid TEXT = new Guid("6994AD06-93EF-11D0-A3CC-00A0C9223196");
        public static Guid NETWORK = new Guid("67C9CC3C-69C4-11D2-8759-00A0C9223196");
        public static Guid TOPOLOGY = new Guid("DDA54A40-1E4C-11D1-A050-405705C10000");
        public static Guid VIRTUAL = new Guid("3503EAC4-1F26-11D1-8AB0-00A0C9223196");
        public static Guid ACOUSTIC_ECHO_CANCEL = new Guid("BF963D80-C559-11D0-8A2B-00A0C9255AC1");
        public static Guid SYSAUDIO = new Guid("A7C7A5B1-5AF3-11D1-9CED-00A024BF0407");
        public static Guid WDMAUD = new Guid("3E227E76-690D-11D2-8161-0000F8775BF1");
        public static Guid AUDIO_GFX = new Guid("9BAF9572-340C-11D3-ABDC-00A0C90AB16F");
        public static Guid AUDIO_SPLITTER = new Guid("9EA331FA-B91B-45F8-9285-BD2BC77AFCDE");
        public static Guid AUDIO_DEVICE = new Guid("FBF6F530-07B9-11D2-A71E-0000F8004788");
        public static Guid PREFERRED_WAVEOUT_DEVICE = new Guid("D6C5066E-72C1-11D2-9755-0000F8004788");
        public static Guid PREFERRED_WAVEIN_DEVICE = new Guid("D6C50671-72C1-11D2-9755-0000F8004788");
        public static Guid PREFERRED_MIDIOUT_DEVICE = new Guid("D6C50674-72C1-11D2-9755-0000F8004788");
        public static Guid WDMAUD_USE_PIN_NAME = new Guid("47A4FA20-A251-11D1-A050-0000F8004788");
        public static Guid ESCALANTE_PLATFORM_DRIVER = new Guid("74f3aea8-9768-11d1-8e07-00a0c95ec22e");
        public static Guid TVTUNER = new Guid("a799a800-a46d-11d0-a18c-00a02401dcd4");
        public static Guid CROSSBAR = new Guid("a799a801-a46d-11d0-a18c-00a02401dcd4");
        public static Guid TVAUDIO = new Guid("a799a802-a46d-11d0-a18c-00a02401dcd4");
        public static Guid VPMUX = new Guid("a799a803-a46d-11d0-a18c-00a02401dcd4");
        public static Guid VBICODEC = new Guid("07dad660-22f1-11d1-a9f4-00c04fbbde8f");
        public static Guid ENCODER = new Guid("19689BF6-C384-48fd-AD51-90E58C79F70B");
        public static Guid MULTIPLEXER = new Guid("7A5DE1D3-01A1-452c-B481-4FA2B96271E8");
    }

    public static class C_PKEY
    {
        // Audio Endpoint Properties
        public const int ENDPOINT_SYSFX_ENABLED = 0x00000000;
        public const int ENDPOINT_SYSFX_DISABLED = 0x00000001;

        public static readonly S_PropertyKey PKEY_AudioEndpoint_FormFactor = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 0);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_ControlPanelPageProvider = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 1);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_Association = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 2);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_PhysicalSpeakers = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 3);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_GUID = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 4);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_Disable_SysFx = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 5);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_FullRangeSpeakers = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 6);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_Supports_EventDriven_Mode = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 7);
        public static readonly S_PropertyKey PKEY_AudioEndpoint_JackSubType = new S_PropertyKey(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 8);
        public static readonly S_PropertyKey PKEY_AudioEngine_DeviceFormat = new S_PropertyKey(new Guid(0xf19f064d, 0x82c, 0x4e27, 0xbc, 0x73, 0x68, 0x82, 0xa1, 0xbb, 0x8e, 0x4c), 0);
        public static readonly S_PropertyKey PKEY_AudioEngine_OEMFormat = new S_PropertyKey(new Guid(0xe4870e26, 0x3cc5, 0x4cd2, 0xba, 0x46, 0xca, 0xa, 0x9a, 0x70, 0xed, 0x4), 3);

        // Device Properties
        // These PKEYs correspond to the old setupapi SPDRP_XXX properties
        public static readonly S_PropertyKey PKEY_Device_DeviceDesc = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 2);
        public static readonly S_PropertyKey PKEY_Device_HardwareIds = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 3);
        public static readonly S_PropertyKey PKEY_Device_CompatibleIds = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 4);
        public static readonly S_PropertyKey PKEY_Device_Service = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 6);
        public static readonly S_PropertyKey PKEY_Device_Class = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 9);
        public static readonly S_PropertyKey PKEY_Device_ClassGuid = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 10);
        public static readonly S_PropertyKey PKEY_Device_Driver = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 11);
        public static readonly S_PropertyKey PKEY_Device_ConfigFlags = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 12);
        public static readonly S_PropertyKey PKEY_Device_Manufacturer = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 13);
        public static readonly S_PropertyKey PKEY_Device_FriendlyName = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 14);
        public static readonly S_PropertyKey PKEY_Device_LocationInfo = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 15);
        public static readonly S_PropertyKey PKEY_Device_PDOName = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 16);
        public static readonly S_PropertyKey PKEY_Device_Capabilities = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 17);
        public static readonly S_PropertyKey PKEY_Device_UINumber = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 18);
        public static readonly S_PropertyKey PKEY_Device_UpperFilters = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 19);
        public static readonly S_PropertyKey PKEY_Device_LowerFilters = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 20);
        public static readonly S_PropertyKey PKEY_Device_BusTypeGuid = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 21);
        public static readonly S_PropertyKey PKEY_Device_LegacyBusType = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 22);
        public static readonly S_PropertyKey PKEY_Device_BusNumber = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 23);
        public static readonly S_PropertyKey PKEY_Device_EnumeratorName = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 24);
        public static readonly S_PropertyKey PKEY_Device_Security = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 25);
        public static readonly S_PropertyKey PKEY_Device_SecuritySDS = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 26);
        public static readonly S_PropertyKey PKEY_Device_DevType = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 27);
        public static readonly S_PropertyKey PKEY_Device_Exclusive = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 28);
        public static readonly S_PropertyKey PKEY_Device_Characteristics = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 29);
        public static readonly S_PropertyKey PKEY_Device_Address = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 30);
        public static readonly S_PropertyKey PKEY_Device_UINumberDescFormat = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 31);
        public static readonly S_PropertyKey PKEY_Device_PowerData = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 32);
        public static readonly S_PropertyKey PKEY_Device_RemovalPolicy = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 33);
        public static readonly S_PropertyKey PKEY_Device_RemovalPolicyDefault = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 34);
        public static readonly S_PropertyKey PKEY_Device_RemovalPolicyOverride = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 34);
        public static readonly S_PropertyKey PKEY_Device_InstallState = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 36);
        public static readonly S_PropertyKey PKEY_Device_LocationPaths = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 37);
        public static readonly S_PropertyKey PKEY_Device_BaseContainerId = new S_PropertyKey(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 38);

        // Device properties
        // These PKEYs correspond to a device's status and problem code
        public static readonly S_PropertyKey PKEY_Device_DevNodeStatus = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 2);
        public static readonly S_PropertyKey PKEY_Device_ProblemCode = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 3);

        // Device properties
        // These PKEYs correspond to device relations
        public static readonly S_PropertyKey PKEY_Device_EjectionRelations = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 4);
        public static readonly S_PropertyKey PKEY_Device_RemovalRelations = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 5);
        public static readonly S_PropertyKey PKEY_Device_PowerRelations = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 6);
        public static readonly S_PropertyKey PKEY_Device_BusRelations = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 7);
        public static readonly S_PropertyKey PKEY_Device_Parent = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 8);
        public static readonly S_PropertyKey PKEY_Device_Children = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 9);
        public static readonly S_PropertyKey PKEY_Device_Siblings = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 10);
        public static readonly S_PropertyKey PKEY_Device_TransportRelations = new S_PropertyKey(new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7), 11);


        // Other Device properties
        public static readonly S_PropertyKey PKEY_Device_Reported = new S_PropertyKey(new Guid(0x80497100, 0x8c73, 0x48b9, 0xaa, 0xd9, 0xce, 0x38, 0x7e, 0x19, 0xc5, 0x6e), 2);
        public static readonly S_PropertyKey PKEY_Device_Legacy = new S_PropertyKey(new Guid(0x80497100, 0x8c73, 0x48b9, 0xaa, 0xd9, 0xce, 0x38, 0x7e, 0x19, 0xc5, 0x6e), 3);
        public static readonly S_PropertyKey PKEY_Device_InstanceId = new S_PropertyKey(new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57), 256);

        public static readonly S_PropertyKey PKEY_Device_ContainerId = new S_PropertyKey(new Guid(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c), 2);

        public static readonly S_PropertyKey PKEY_Device_ModelId = new S_PropertyKey(new Guid(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b), 2);
        public static readonly S_PropertyKey PKEY_Device_FriendlyNameAttributes = new S_PropertyKey(new Guid(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b), 3);
        public static readonly S_PropertyKey PKEY_Device_ManufacturerAttributes = new S_PropertyKey(new Guid(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b), 4);
        public static readonly S_PropertyKey PKEY_Device_PresenceNotForDevice = new S_PropertyKey(new Guid(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b), 5);

        public static readonly S_PropertyKey PKEY_Numa_Proximity_Domain = new S_PropertyKey(new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), 1);
        public static readonly S_PropertyKey PKEY_Device_DHP_Rebalance_Policy = new S_PropertyKey(new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), 2);
        public static readonly S_PropertyKey PKEY_Device_Numa_Node = new S_PropertyKey(new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), 3);
        public static readonly S_PropertyKey PKEY_Device_BusReportedDeviceDesc = new S_PropertyKey(new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), 4);

        public static readonly S_PropertyKey PKEY_Device_InstallInProgress = new S_PropertyKey(new Guid(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29), 9);

        // Device driver properties
        public static readonly S_PropertyKey PKEY_Device_DriverDate = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 2);
        public static readonly S_PropertyKey PKEY_Device_DriverVersion = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 3);
        public static readonly S_PropertyKey PKEY_Device_DriverDesc = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 4);
        public static readonly S_PropertyKey PKEY_Device_DriverInfPath = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 5);
        public static readonly S_PropertyKey PKEY_Device_DriverInfSection = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 6);
        public static readonly S_PropertyKey PKEY_Device_DriverInfSectionExt = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 7);
        public static readonly S_PropertyKey PKEY_Device_MatchingDeviceId = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 8);
        public static readonly S_PropertyKey PKEY_Device_DriverProvider = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 9);
        public static readonly S_PropertyKey PKEY_Device_DriverPropPageProvider = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 10);
        public static readonly S_PropertyKey PKEY_Device_DriverCoInstallers = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 11);
        public static readonly S_PropertyKey PKEY_Device_ResourcePickerTags = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 12);
        public static readonly S_PropertyKey PKEY_Device_ResourcePickerExceptions = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 13);
        public static readonly S_PropertyKey PKEY_Device_DriverRank = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 14);
        public static readonly S_PropertyKey PKEY_Device_DriverLogoLevel = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 15);
        public static readonly S_PropertyKey PKEY_Device_NoConnectSound = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 17);
        public static readonly S_PropertyKey PKEY_Device_GenericDriverInstalled = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 18);
        public static readonly S_PropertyKey PKEY_Device_AdditionalSoftwareRequested = new S_PropertyKey(new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6), 19);

        // Device safe-removal properties
        public static readonly S_PropertyKey PKEY_Device_SafeRemovalRequired = new S_PropertyKey(new Guid(0xafd97640, 0x86a3, 0x4210, 0xb6, 0x7c, 0x28, 0x9c, 0x41, 0xaa, 0xbe, 0x55), 2);
        public static readonly S_PropertyKey PKEY_Device_SafeRemovalRequiredOverride = new S_PropertyKey(new Guid(0xafd97640, 0x86a3, 0x4210, 0xb6, 0x7c, 0x28, 0x9c, 0x41, 0xaa, 0xbe, 0x55), 3);

        // Device properties that were set by the driver package that was installed
        // on the device.
        public static readonly S_PropertyKey PKEY_DrvPkg_Model = new S_PropertyKey(new Guid(0xcf73bb51, 0x3abf, 0x44a2, 0x85, 0xe0, 0x9a, 0x3d, 0xc7, 0xa1, 0x21, 0x32), 2);
        public static readonly S_PropertyKey PKEY_DrvPkg_VendorWebSite = new S_PropertyKey(new Guid(0xcf73bb51, 0x3abf, 0x44a2, 0x85, 0xe0, 0x9a, 0x3d, 0xc7, 0xa1, 0x21, 0x32), 2);
        public static readonly S_PropertyKey PKEY_DrvPkg_DetailedDescription = new S_PropertyKey(new Guid(0xcf73bb51, 0x3abf, 0x44a2, 0x85, 0xe0, 0x9a, 0x3d, 0xc7, 0xa1, 0x21, 0x32), 2);
        public static readonly S_PropertyKey PKEY_DrvPkg_DocumentationLink = new S_PropertyKey(new Guid(0xcf73bb51, 0x3abf, 0x44a2, 0x85, 0xe0, 0x9a, 0x3d, 0xc7, 0xa1, 0x21, 0x32), 2);
        public static readonly S_PropertyKey PKEY_DrvPkg_Icon = new S_PropertyKey(new Guid(0xcf73bb51, 0x3abf, 0x44a2, 0x85, 0xe0, 0x9a, 0x3d, 0xc7, 0xa1, 0x21, 0x32), 2);
        public static readonly S_PropertyKey PKEY_DrvPkg_BrandingIcon = new S_PropertyKey(new Guid(0xcf73bb51, 0x3abf, 0x44a2, 0x85, 0xe0, 0x9a, 0x3d, 0xc7, 0xa1, 0x21, 0x32), 2);

        // Device setup class properties
        // These PKEYs correspond to the old setupapi SPCRP_XXX properties
        public static readonly S_PropertyKey PKEY_DeviceClass_UpperFilters = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 19);
        public static readonly S_PropertyKey PKEY_DeviceClass_LowerFilters = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 20);
        public static readonly S_PropertyKey PKEY_DeviceClass_Security = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 25);
        public static readonly S_PropertyKey PKEY_DeviceClass_SecuritySDS = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 26);
        public static readonly S_PropertyKey PKEY_DeviceClass_DevType = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 27);
        public static readonly S_PropertyKey PKEY_DeviceClass_Exclusive = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 28);
        public static readonly S_PropertyKey PKEY_DeviceClass_Characteristics = new S_PropertyKey(new Guid(0x4321918b, 0xf69e, 0x470d, 0xa5, 0xde, 0x4d, 0x88, 0xc7, 0x5a, 0xd2, 0x4b), 29);

        // Device setup class properties
        // These PKEYs correspond to registry values under the device class GUID key
        public static readonly S_PropertyKey PKEY_DeviceClass_Name = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 2);
        public static readonly S_PropertyKey PKEY_DeviceClass_ClassName = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 3);
        public static readonly S_PropertyKey PKEY_DeviceClass_Icon = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 4);
        public static readonly S_PropertyKey PKEY_DeviceClass_ClassInstaller = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 5);
        public static readonly S_PropertyKey PKEY_DeviceClass_PropPageProvider = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 6);
        public static readonly S_PropertyKey PKEY_DeviceClass_NoInstallClass = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 7);
        public static readonly S_PropertyKey PKEY_DeviceClass_NoDisplayClass = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 8);
        public static readonly S_PropertyKey PKEY_DeviceClass_SilentInstall = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 9);
        public static readonly S_PropertyKey PKEY_DeviceClass_NoUseClass = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 10);
        public static readonly S_PropertyKey PKEY_DeviceClass_DefaultService = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 11);
        public static readonly S_PropertyKey PKEY_DeviceClass_IconPath = new S_PropertyKey(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 12);

        // Other Device setup class properties
        public static readonly S_PropertyKey PKEY_DeviceClass_ClassCoInstallers = new S_PropertyKey(new Guid(0x713d1703, 0xa2e2, 0x49f5, 0x92, 0x14, 0x56, 0x47, 0x2e, 0xf3, 0xda, 0x5c), 2);

        // Device interface properties
        public static readonly S_PropertyKey PKEY_DeviceInterface_FriendlyName = new S_PropertyKey(new Guid(0x026e516e, 0xb814, 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 2);
        public static readonly S_PropertyKey PKEY_DeviceInterface_Enabled = new S_PropertyKey(new Guid(0x026e516e, 0xb814, 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 3);
        public static readonly S_PropertyKey PKEY_DeviceInterface_ClassGuid = new S_PropertyKey(new Guid(0x026e516e, 0xb814, 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 4);

        // Device interface class properties
        public static readonly S_PropertyKey PKEY_DeviceInterfaceClass_DefaultInterface = new S_PropertyKey(new Guid(0x14c83a99, 0x0b3f, 0x44b7, 0xbe, 0x4c, 0xa1, 0x78, 0xd3, 0x99, 0x05, 0x64), 2);
    }
    #endregion

    #region subclasses
    #region AudioMeterInformation
    public class C_AudioMeterInformation : IDisposable
    {
        private IAudioMeterInformation _AudioMeterInformation;
        private E_EndpointHardwareSupport _HardwareSupport;
        private C_AudioMeterInformationChannels _Channels;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioMeterInformation(IAudioMeterInformation realInterface)
        {
            int _HardwareSupp_;

            _AudioMeterInformation = realInterface;
            Marshal.ThrowExceptionForHR(_AudioMeterInformation.QueryHardwareSupport(out _HardwareSupp_));
            _HardwareSupport = (E_EndpointHardwareSupport)_HardwareSupp_;
            _Channels = new C_AudioMeterInformationChannels(_AudioMeterInformation);

        }
        ~C_AudioMeterInformation()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioMeterInformation != null)
                    {
                        Marshal.ReleaseComObject(_AudioMeterInformation);
                        _AudioMeterInformation = null;
                    }

                    if (_Channels != null)
                    {
                        _Channels.Dispose();
                        _Channels = null;
                    }
                }
            }
        }
        #endregion

        public C_AudioMeterInformationChannels PeakValues
        {
            get
            {
                return _Channels;
            }
        }

        public E_EndpointHardwareSupport HardwareSupport
        {
            get
            {
                return _HardwareSupport;
            }
        }

        public float MasterPeakValue
        {
            get
            {
                float _result_;
                Marshal.ThrowExceptionForHR(_AudioMeterInformation.GetPeakValue(out _result_));
                return _result_;
            }
        }
    }

    public class C_AudioMeterInformationChannels : IDisposable
    {
        IAudioMeterInformation _AudioMeterInformation;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioMeterInformationChannels(IAudioMeterInformation parent)
        {
            _AudioMeterInformation = parent;
        }
        ~C_AudioMeterInformationChannels()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioMeterInformation != null)
                    {
                        Marshal.ReleaseComObject(_AudioMeterInformation);
                        _AudioMeterInformation = null;
                    }
                }
            }
        }
        #endregion

        public int Count
        {
            get
            {
                int _result_;
                Marshal.ThrowExceptionForHR(_AudioMeterInformation.GetMeteringChannelCount(out _result_));
                return _result_;
            }
        }

        public float this[int index]
        {
            get
            {
                var _peakValues_ = new float[Count];
                var _Params_ = GCHandle.Alloc(_peakValues_, GCHandleType.Pinned);
                Marshal.ThrowExceptionForHR(_AudioMeterInformation.GetChannelsPeakValues(_peakValues_.Length, _Params_.AddrOfPinnedObject()));
                _Params_.Free();
                return _peakValues_[index];
            }
        }
    }
    #endregion

    #region CPolicyConfigVistaClient
    public class C_CPolicyConfigVistaClient : IDisposable
    {
        [ComImport, Guid("294935CE-F637-4E7C-A41B-AB255460B862")]
        private class C_InternalCPolicyConfigVistaClient
        { }

        private IPolicyConfigVista _policyConfigVistaClient = new C_InternalCPolicyConfigVistaClient() as IPolicyConfigVista;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        ~C_CPolicyConfigVistaClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_policyConfigVistaClient != null)
                    {
                        Marshal.ReleaseComObject(_policyConfigVistaClient);
                        _policyConfigVistaClient = null;
                    }
                }
            }
        }
        #endregion

        public int SetDefaultDevie(string deviceID)
        {
            _policyConfigVistaClient.SetDefaultEndpoint(deviceID, E_Role.eConsole);
            _policyConfigVistaClient.SetDefaultEndpoint(deviceID, E_Role.eMultimedia);
            _policyConfigVistaClient.SetDefaultEndpoint(deviceID, E_Role.eCommunications);

            return 0;
        }
    }
    #endregion

    #region AudioEndpointVolume
    public class C_AudioEndpointVolume : IDisposable
    {
        private IAudioEndpointVolume _AudioEndPointVolume;
        private C_AudioEndpointVolumeChannels _Channels;
        private C_AudioEndpointVolumeStepInformation _StepInformation;
        private C_AudioEndPointVolumeVolumeRange _VolumeRange;
        private E_EndpointHardwareSupport _HardwareSupport;
        private C_AudioEndpointVolumeCallback _CallBack;
        public event EventHandler<C_AudioVolumeNotificationData> OnVolumeNotification;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioEndpointVolume(IAudioEndpointVolume realEndpointVolume)
        {
            uint _HardwareSupp_;

            _AudioEndPointVolume = realEndpointVolume;
            _Channels = new C_AudioEndpointVolumeChannels(_AudioEndPointVolume);
            _StepInformation = new C_AudioEndpointVolumeStepInformation(_AudioEndPointVolume);
            Marshal.ThrowExceptionForHR(_AudioEndPointVolume.QueryHardwareSupport(out _HardwareSupp_));
            _HardwareSupport = (E_EndpointHardwareSupport)_HardwareSupp_;
            _VolumeRange = new C_AudioEndPointVolumeVolumeRange(_AudioEndPointVolume);
            _CallBack = new C_AudioEndpointVolumeCallback(this);
            Marshal.ThrowExceptionForHR(_AudioEndPointVolume.RegisterControlChangeNotify(_CallBack));
        }
        ~C_AudioEndpointVolume()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_CallBack != null)
                    {
                        Marshal.ThrowExceptionForHR(_AudioEndPointVolume.UnregisterControlChangeNotify(_CallBack));
                        _CallBack.Dispose();
                        _CallBack = null;
                    }

                    if (_Channels != null)
                    {
                        _Channels.Dispose();
                        _Channels = null;
                    }

                    if (_StepInformation != null)
                    {
                        _StepInformation.Dispose();
                        _StepInformation = null;
                    }

                    if (_VolumeRange != null)
                    {
                        _VolumeRange.Dispose();
                        _VolumeRange = null;
                    }

                    if (_AudioEndPointVolume != null)
                    {
                        Marshal.ReleaseComObject(_AudioEndPointVolume);
                        _AudioEndPointVolume = null;
                    }
                }
            }
        }
        #endregion

        public C_AudioEndPointVolumeVolumeRange VolumeRange
        {
            get
            {
                return _VolumeRange;
            }
        }

        public E_EndpointHardwareSupport HardwareSupport
        {
            get
            {
                return _HardwareSupport;
            }
        }

        public C_AudioEndpointVolumeStepInformation StepInformation
        {
            get
            {
                return _StepInformation;
            }
        }

        public C_AudioEndpointVolumeChannels Channels
        {
            get
            {
                return _Channels;
            }
        }

        public float MasterVolumeLevel
        {
            get
            {
                float result;
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.GetMasterVolumeLevel(out result));
                return result;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.SetMasterVolumeLevel(value, Guid.Empty));
            }
        }

        public float MasterVolumeLevelScalar
        {
            get
            {
                float result;
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.GetMasterVolumeLevelScalar(out result));
                return result;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.SetMasterVolumeLevelScalar(value, Guid.Empty));
            }
        }

        public bool Mute
        {
            get
            {
                bool result;
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.GetMute(out result));
                return result;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.SetMute(value, Guid.Empty));
            }
        }

        public void VolumeStepUp()
        {
            Marshal.ThrowExceptionForHR(_AudioEndPointVolume.VolumeStepUp(Guid.Empty));
        }

        public void VolumeStepDown()
        {
            Marshal.ThrowExceptionForHR(_AudioEndPointVolume.VolumeStepDown(Guid.Empty));
        }

        internal void FireNotification(C_AudioVolumeNotificationData e)
        {
            try
            {
                var _handler_ = OnVolumeNotification;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_AudioVolumeNotificationData> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, e });
                        else
                            _singleCast_(this, e);
                    }
                }
            }
            catch
            { }
        }
    }

    public class C_AudioEndpointVolumeChannel : IDisposable
    {
        private uint _Channel;
        private IAudioEndpointVolume _AudioEndpointVolume;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioEndpointVolumeChannel(IAudioEndpointVolume parent, int channel)
        {
            _Channel = (uint)channel;
            _AudioEndpointVolume = parent;
        }
        ~C_AudioEndpointVolumeChannel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioEndpointVolume != null)
                    {
                        Marshal.ReleaseComObject(_AudioEndpointVolume);
                        _AudioEndpointVolume = null;
                    }
                }
            }
        }
        #endregion

        public float VolumeLevel
        {
            get
            {
                float _result_;
                Marshal.ThrowExceptionForHR(_AudioEndpointVolume.GetChannelVolumeLevel(_Channel, out _result_));
                return _result_;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_AudioEndpointVolume.SetChannelVolumeLevel(_Channel, value, Guid.Empty));
            }
        }

        public float VolumeLevelScalar
        {
            get
            {
                float _result_;
                Marshal.ThrowExceptionForHR(_AudioEndpointVolume.GetChannelVolumeLevelScalar(_Channel, out _result_));
                return _result_;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_AudioEndpointVolume.SetChannelVolumeLevelScalar(_Channel, value, Guid.Empty));
            }
        }
    }

    public class C_AudioEndpointVolumeChannels : IDisposable
    {
        IAudioEndpointVolume _AudioEndPointVolume;
        C_AudioEndpointVolumeChannel[] _Channels;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioEndpointVolumeChannels(IAudioEndpointVolume parent)
        {
            _AudioEndPointVolume = parent;

            var _ChannelCount_ = Count;
            _Channels = new C_AudioEndpointVolumeChannel[_ChannelCount_];
            for (var _i_ = 0; _i_ < _ChannelCount_; _i_++)
                _Channels[_i_] = new C_AudioEndpointVolumeChannel(_AudioEndPointVolume, _i_);
        }
        ~C_AudioEndpointVolumeChannels()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (_Channels != null)
                    {
                        for (var _i_ = 0; _i_ < _Channels.Length; _i_++)
                            _Channels[_i_].Dispose();

                        _Channels = null;
                    }

                    if (_AudioEndPointVolume != null)
                    {
                        Marshal.ReleaseComObject(_AudioEndPointVolume);
                        _AudioEndPointVolume = null;
                    }
                }
            }
        }
        #endregion

        public int Count
        {
            get
            {
                int result;
                Marshal.ThrowExceptionForHR(_AudioEndPointVolume.GetChannelCount(out result));
                return result;
            }
        }

        public C_AudioEndpointVolumeChannel this[int index]
        {
            get
            {
                return _Channels[index];
            }
        }
    }

    public class C_AudioEndpointVolumeStepInformation : IDisposable
    {
        private uint _Step;
        private uint _StepCount;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioEndpointVolumeStepInformation(IAudioEndpointVolume parent)
        {
            Marshal.ThrowExceptionForHR(parent.GetVolumeStepInfo(out _Step, out _StepCount));
        }
        ~C_AudioEndpointVolumeStepInformation()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    //nothing to do
                }
            }
        }
        #endregion

        public uint Step
        {
            get
            {
                return _Step;
            }
        }

        public uint StepCount
        {
            get
            {
                return _StepCount;
            }
        }
    }

    public class C_AudioEndPointVolumeVolumeRange : IDisposable
    {
        float _VolumeMindB;
        float _VolumeMaxdB;
        float _VolumeIncrementdB;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioEndPointVolumeVolumeRange(IAudioEndpointVolume parent)
        {
            Marshal.ThrowExceptionForHR(parent.GetVolumeRange(out _VolumeMindB, out _VolumeMaxdB, out _VolumeIncrementdB));
        }
        ~C_AudioEndPointVolumeVolumeRange()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion

        public float MindB
        {
            get
            {
                return _VolumeMindB;
            }
        }

        public float MaxdB
        {
            get
            {
                return _VolumeMaxdB;
            }
        }

        public float IncrementdB
        {
            get
            {
                return _VolumeIncrementdB;
            }
        }
    }

    // This class implements the IAudioEndpointVolumeCallback interface,
    // it is implemented in this class because implementing it on AudioEndpointVolume 
    // (where the functionality is really wanted, would cause the OnNotify function 
    // to show up in the public API. 
    internal class C_AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback, IDisposable
    {
        private C_AudioEndpointVolume _Parent;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioEndpointVolumeCallback(C_AudioEndpointVolume parent)
        {
            _Parent = parent;
        }
        ~C_AudioEndpointVolumeCallback()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                }
            }
        }
        #endregion

        [PreserveSig]
        public int OnNotify(IntPtr NotifyData)
        {
            //Since AUDIO_VOLUME_NOTIFICATION_DATA is dynamic in length based on the
            //number of audio channels available we cannot just call PtrToStructure 
            //to get all data, that's why it is split up into two steps, first the static
            //data is marshalled into the data structure, then with some IntPtr math the
            //remaining floats are read from memory.
            //Determine offset in structure of the first float
            //Determine offset in memory of the first float
            var _data_ = (S_AudioVolumeNotificationData)Marshal.PtrToStructure(NotifyData, typeof(S_AudioVolumeNotificationData));
            var _FirstFloatPtr_ = (IntPtr)((long)NotifyData + (long)Marshal.OffsetOf(typeof(S_AudioVolumeNotificationData), "ChannelVolume"));
            var _voldata_ = new float[_data_.nChannels];

            //Read all floats from memory.
            for (var _i_ = 0; _i_ < _data_.nChannels; _i_++)
                _voldata_[_i_] = (float)Marshal.PtrToStructure(_FirstFloatPtr_, typeof(float));

            //Create combined structure and Fire Event in parent class.
            using (var _NotificationData_ = new C_AudioVolumeNotificationData(_data_.guidEventContext, _data_.bMuted, _data_.fMasterVolume, _voldata_))
                _Parent.FireNotification(_NotificationData_);

            return 0; //S_OK
        }
    }

    public class C_AudioVolumeNotificationData : EventArgs, IDisposable
    {
        private Guid _EventContext;
        private bool _Muted;
        private float _MasterVolume;
        private int _Channels;
        private float[] _ChannelVolume;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_AudioVolumeNotificationData(Guid eventContext, bool muted, float masterVolume, float[] channelVolume)
        {
            _EventContext = eventContext;
            _Muted = muted;
            _MasterVolume = masterVolume;
            _Channels = channelVolume.Length;
            _ChannelVolume = channelVolume;
        }
        ~C_AudioVolumeNotificationData()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion

        public Guid EventContext
        {
            get
            {
                return _EventContext;
            }
        }

        public bool Muted
        {
            get
            {
                return _Muted;
            }
        }

        public float MasterVolume
        {
            get
            {
                return _MasterVolume;
            }
        }

        public int Channels
        {
            get
            {
                return _Channels;
            }
        }

        public float[] ChannelVolume
        {
            get
            {
                return _ChannelVolume;
            }
        }
    }
    #endregion

    #region AudioSessionManager2
    public class C_AudioSessionManager2 : IDisposable
    {
        public event EventHandler<C_SessionCreatedEventArgs> OnSessionCreated;

        private IAudioSessionManager2 _AudioSessionManager2;
        private C_SessionCollection _Sessions;
        private C_AudioSessionNotification _AudioSessionNotification;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioSessionManager2(IAudioSessionManager2 realAudioSessionManager2)
        {
            _AudioSessionManager2 = realAudioSessionManager2;

            RefreshSessions();
        }
        ~C_AudioSessionManager2()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (_Sessions != null)
                    {
                        _Sessions.Dispose();
                        _Sessions = null;
                    }

                    try
                    {
                        if (_AudioSessionNotification != null)
                            Marshal.ThrowExceptionForHR(_AudioSessionManager2.UnregisterSessionNotification(_AudioSessionNotification));
                        _AudioSessionNotification.Dispose();
                        _AudioSessionNotification = null;
                    }
                    catch
                    {
                        _AudioSessionNotification = null;
                    }

                    if (_AudioSessionManager2 != null)
                    {
                        Marshal.ReleaseComObject(_AudioSessionManager2);
                        _AudioSessionManager2 = null;
                    }
                }
            }
        }
        #endregion

        internal void FireSessionCreated(IAudioSessionControl2 e)
        {
            try
            {
                var _handler_ = OnSessionCreated;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_SessionCreatedEventArgs> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new C_SessionCreatedEventArgs(e) });
                        else
                            _singleCast_(this, new C_SessionCreatedEventArgs(e));
                    }
                }
            }
            catch
            { }
        }

        public void RefreshSessions()
        {
            UnregisterNotifications();

            IAudioSessionEnumerator _SessionEnum;
            Marshal.ThrowExceptionForHR(_AudioSessionManager2.GetSessionEnumerator(out _SessionEnum));
            _Sessions = new C_SessionCollection(_SessionEnum);

            _AudioSessionNotification = new C_AudioSessionNotification(this);
            Marshal.ThrowExceptionForHR(_AudioSessionManager2.RegisterSessionNotification(_AudioSessionNotification));
        }

        public C_SessionCollection Sessions
        {
            get
            {
                return _Sessions;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }

        private void UnregisterNotifications()
        {
            if (_Sessions != null)
            {
                _Sessions.Dispose();
                _Sessions = null;
            }

            try
            {
                if (_AudioSessionNotification != null)
                    Marshal.ThrowExceptionForHR(_AudioSessionManager2.UnregisterSessionNotification(_AudioSessionNotification));
                _AudioSessionNotification = null;
            }
            catch
            {
                _AudioSessionNotification = null;
            }
        }
    }

    public class C_SessionCreatedEventArgs : EventArgs, IDisposable
    {
        public IAudioSessionControl2 Session;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_SessionCreatedEventArgs(IAudioSessionControl2 newSession)
        {
            Session = newSession;
        }
        ~C_SessionCreatedEventArgs()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion
    }

    public class C_SessionCollection : IDisposable
    {
        IAudioSessionEnumerator _AudioSessionEnumerator;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_SessionCollection(IAudioSessionEnumerator realEnumerator)
        {
            _AudioSessionEnumerator = realEnumerator;
        }
        ~C_SessionCollection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioSessionEnumerator != null)
                    {
                        Marshal.ReleaseComObject(_AudioSessionEnumerator);
                        _AudioSessionEnumerator = null;
                    }
                }
            }
        }
        #endregion

        public C_AudioSessionControl2 this[int index]
        {
            get
            {
                IAudioSessionControl2 _Result;
                Marshal.ThrowExceptionForHR(_AudioSessionEnumerator.GetSession(index, out _Result));
                return new C_AudioSessionControl2(_Result);
            }
        }

        public int Count
        {
            get
            {
                int result;
                Marshal.ThrowExceptionForHR(_AudioSessionEnumerator.GetCount(out result));
                return result;
            }
        }
    }

    public class C_AudioSessionControl2 : IDisposable
    {
        IAudioSessionControl2 _AudioSessionControl2;
        internal C_AudioMeterInformation _AudioMeterInformation;
        internal C_SimpleAudioVolume _SimpleAudioVolume;
        private C_AudioSessionEvents _AudioSessionEvents;
        private volatile bool _IsDisposed = false;

        #region events
        public event EventHandler<C_StringEventArgs> OnDisplayNameChanged;
        public event EventHandler<C_StringEventArgs> OnIconPathChanged;
        public event EventHandler<C_SimpleVolumeChangedEventArgs> OnSimpleVolumeChanged;
        public event EventHandler<C_ChannelVolumeChangedEventArgs> OnChannelVolumeChanged;
        public event EventHandler<C_StateChangedEventArgs> OnStateChanged;
        #endregion

        #region Constructor/Destructor/Dispose
        internal C_AudioSessionControl2(IAudioSessionControl2 realAudioSessionControl2)
        {
            _AudioSessionControl2 = realAudioSessionControl2;

            var _meters_ = _AudioSessionControl2 as IAudioMeterInformation;
            var _volume_ = _AudioSessionControl2 as ISimpleAudioVolume;
            if (_meters_ != null) _AudioMeterInformation = new C_AudioMeterInformation(_meters_);
            if (_volume_ != null) _SimpleAudioVolume = new C_SimpleAudioVolume(_volume_);

            _AudioSessionEvents = new C_AudioSessionEvents(this);
            Marshal.ThrowExceptionForHR(_AudioSessionControl2.RegisterAudioSessionNotification(_AudioSessionEvents));
        }
        ~C_AudioSessionControl2()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioSessionEvents != null)
                    {
                        Marshal.ThrowExceptionForHR(_AudioSessionControl2.UnregisterAudioSessionNotification(_AudioSessionEvents));
                        _AudioSessionEvents.Dispose();
                        _AudioSessionEvents = null;
                    }

                    if (_AudioMeterInformation != null)
                    {
                        _AudioMeterInformation.Dispose();
                        _AudioMeterInformation = null;
                    }

                    if (_SimpleAudioVolume != null)
                    {
                        _SimpleAudioVolume.Dispose();
                        _SimpleAudioVolume = null;
                    }

                    if (_AudioSessionControl2 != null)
                    {
                        Marshal.ReleaseComObject(_AudioSessionControl2);
                        _AudioSessionControl2 = null;
                    }
                }
            }
        }
        #endregion

        internal void FireDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string e)
        {
            try
            {
                var _handler_ = OnDisplayNameChanged;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_StringEventArgs> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new C_StringEventArgs(e) });
                        else
                            _singleCast_(this, new C_StringEventArgs(e));
                    }
                }
            }
            catch
            { }
        }

        internal void FireOnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string e)
        {
            try
            {
                var _handler_ = OnIconPathChanged;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_StringEventArgs> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new C_StringEventArgs(e) });
                        else
                            _singleCast_(this, new C_StringEventArgs(e));
                    }
                }
            }
            catch
            { }
        }

        internal void FireSimpleVolumeChanged(float NewVolume, bool newMute)
        {
            try
            {
                var _handler_ = OnSimpleVolumeChanged;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_SimpleVolumeChangedEventArgs> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new C_SimpleVolumeChangedEventArgs(NewVolume, newMute) });
                        else
                            _singleCast_(this, new C_SimpleVolumeChangedEventArgs(NewVolume, newMute));
                    }
                }
            }
            catch
            { }
        }

        internal void FireChannelVolumeChanged(uint ChannelCount, IntPtr NewChannelVolumeArray, uint ChangedChannel)
        {
            try
            {
                var _volume_ = new float[ChannelCount];
                Marshal.Copy(NewChannelVolumeArray, _volume_, 0, (int)ChannelCount);

                var _handler_ = OnChannelVolumeChanged;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_ChannelVolumeChangedEventArgs> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new C_ChannelVolumeChangedEventArgs((int)ChannelCount, _volume_, (int)ChangedChannel) });
                        else
                            _singleCast_(this, new C_ChannelVolumeChangedEventArgs((int)ChannelCount, _volume_, (int)ChangedChannel));
                    }
                }
            }
            catch
            { }
        }

        internal void FireStateChanged(E_AudioSessionState NewState)
        {
            try
            {
                var _handler_ = OnStateChanged;
                if (_handler_ != null)
                {
                    foreach (EventHandler<C_StateChangedEventArgs> _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new C_StateChangedEventArgs(NewState) });
                        else
                            _singleCast_(this, new C_StateChangedEventArgs(NewState));
                    }
                }
            }
            catch
            { }
        }

        public C_AudioMeterInformation AudioMeterInformation
        {
            get
            {
                return _AudioMeterInformation;
            }
        }

        public C_SimpleAudioVolume SimpleAudioVolume
        {
            get
            {
                return _SimpleAudioVolume;
            }
        }

        public E_AudioSessionState State
        {
            get
            {
                E_AudioSessionState _res_;
                Marshal.ThrowExceptionForHR(_AudioSessionControl2.GetState(out _res_));
                return _res_;
            }
        }

        public string DisplayName
        {
            get
            {
                string _str_;
                Marshal.ThrowExceptionForHR(_AudioSessionControl2.GetDisplayName(out _str_));
                return _str_;
            }
        }

        public string IconPath
        {
            get
            {
                string _str_;
                Marshal.ThrowExceptionForHR(_AudioSessionControl2.GetIconPath(out _str_));
                return _str_;
            }
        }

        public string GetSessionIdentifier
        {
            get
            {
                string _str_;
                Marshal.ThrowExceptionForHR(_AudioSessionControl2.GetSessionIdentifier(out _str_));
                return _str_;
            }
        }

        public string GetSessionInstanceIdentifier
        {
            get
            {
                string _str_;
                Marshal.ThrowExceptionForHR(_AudioSessionControl2.GetSessionInstanceIdentifier(out _str_));
                return _str_;
            }
        }

        public uint GetProcessID
        {
            get
            {
                uint _pid_;
                Marshal.ThrowExceptionForHR(_AudioSessionControl2.GetProcessId(out _pid_));
                return _pid_;
            }
        }

        public bool IsSystemSoundsSession
        {
            get
            {
                return (_AudioSessionControl2.IsSystemSoundsSession() == 0);
            }
        }
    }

    public class C_StringEventArgs : EventArgs, IDisposable
    {
        public string Value;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_StringEventArgs(string Text)
        {
            Value = Text;
        }
        ~C_StringEventArgs()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion
    }

    public class C_SimpleVolumeChangedEventArgs : EventArgs, IDisposable
    {
        public float Volume;
        public bool Mute;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_SimpleVolumeChangedEventArgs(float newVolume, bool newMute)
        {
            Volume = newVolume;
            Mute = newMute;
        }
        ~C_SimpleVolumeChangedEventArgs()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion
    }

    public class C_ChannelVolumeChangedEventArgs : EventArgs, IDisposable
    {
        public int ChannelCount;
        public float[] Volume;
        public int ChangedChannel;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_ChannelVolumeChangedEventArgs(int channelCount, float[] newVolume, int changedChannel)
        {
            ChannelCount = channelCount;
            Volume = newVolume;
            ChangedChannel = changedChannel;
        }
        ~C_ChannelVolumeChangedEventArgs()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion
    }

    public class C_StateChangedEventArgs : EventArgs, IDisposable
    {
        public E_AudioSessionState State;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_StateChangedEventArgs(E_AudioSessionState newState)
        {
            State = newState;
        }
        ~C_StateChangedEventArgs()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion
    }

    public class C_SimpleAudioVolume : IDisposable
    {
        ISimpleAudioVolume _SimpleAudioVolume;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_SimpleAudioVolume(ISimpleAudioVolume realSimpleVolume)
        {
            _SimpleAudioVolume = realSimpleVolume;
        }
        ~C_SimpleAudioVolume()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_SimpleAudioVolume != null)
                    {
                        Marshal.ReleaseComObject(_SimpleAudioVolume);
                        _SimpleAudioVolume = null;
                    }
                }
            }
        }
        #endregion

        public float MasterVolume
        {
            get
            {
                float _ret_;
                Marshal.ThrowExceptionForHR(_SimpleAudioVolume.GetMasterVolume(out _ret_));
                return _ret_;
            }
            set
            {
                var _Empty_ = Guid.Empty;
                Marshal.ThrowExceptionForHR(_SimpleAudioVolume.SetMasterVolume(value, ref _Empty_));
            }
        }

        public bool Mute
        {
            get
            {
                bool _ret_;
                Marshal.ThrowExceptionForHR(_SimpleAudioVolume.GetMute(out _ret_));
                return _ret_;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_SimpleAudioVolume.SetMute(value, Guid.Empty));
            }
        }
    }

    internal class C_AudioSessionEvents : IAudioSessionEvents, IDisposable
    {
        private C_AudioSessionControl2 _Parent;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioSessionEvents(C_AudioSessionControl2 parent)
        {
            _Parent = parent;
        }
        ~C_AudioSessionEvents()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion

        [PreserveSig]
        public int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, Guid EventContext)
        {
            _Parent.FireDisplayNameChanged(NewDisplayName);
            return 0;
        }

        [PreserveSig]
        public int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, Guid EventContext)
        {
            _Parent.FireOnIconPathChanged(NewIconPath);
            return 0;
        }

        [PreserveSig]
        public int OnSimpleVolumeChanged(float NewVolume, bool newMute, Guid EventContext)
        {
            _Parent.FireSimpleVolumeChanged(NewVolume, newMute);
            return 0;
        }

        [PreserveSig]
        public int OnChannelVolumeChanged(uint ChannelCount, IntPtr NewChannelVolumeArray, uint ChangedChannel, Guid EventContext)
        {
            _Parent.FireChannelVolumeChanged(ChannelCount, NewChannelVolumeArray, ChangedChannel);
            return 0;
        }

        [PreserveSig]
        public int OnGroupingParamChanged(Guid NewGroupingParam, Guid EventContext)
        {
            return 0;
        }

        [PreserveSig]
        public int OnStateChanged(E_AudioSessionState NewState)
        {
            _Parent.FireStateChanged(NewState);
            return 0;
        }

        [PreserveSig]
        public int OnSessionDisconnected(E_AudioSessionDisconnectReason DisconnectReason)
        {
            return 0;
        }
    }

    internal class C_AudioSessionNotification : IAudioSessionNotification, IDisposable
    {
        private C_AudioSessionManager2 _Parent;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioSessionNotification(C_AudioSessionManager2 parent)
        {
            _Parent = parent;
        }
        ~C_AudioSessionNotification()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion

        [PreserveSig]
        public int OnSessionCreated(IAudioSessionControl2 NewSession)
        {
            _Parent.FireSessionCreated(NewSession);
            return 0;
        }
    }
    #endregion

    #region DeviceTopology
    public class C_DeviceTopology : IDisposable
    {
        private IDeviceTopology _DeviceTopology;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_DeviceTopology(IDeviceTopology realInterface)
        {
            _DeviceTopology = realInterface;
        }
        ~C_DeviceTopology()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_DeviceTopology != null)
                    {
                        Marshal.ReleaseComObject(_DeviceTopology);
                        _DeviceTopology = null;
                    }
                }
            }
        }
        #endregion

        public int GetConnectorCount
        {
            get
            {
                var _count_ = 0;
                Marshal.ThrowExceptionForHR(_DeviceTopology.GetConnectorCount(out _count_));
                return _count_;
            }
        }

        public C_Connector GetConnector(int index)
        {
            IConnector connector;
            Marshal.ThrowExceptionForHR(_DeviceTopology.GetConnector(index, out connector));
            return new C_Connector(connector);
        }

        public int GetSubunitCount
        {
            get
            {
                var _count_ = 0;
                Marshal.ThrowExceptionForHR(_DeviceTopology.GetSubunitCount(out _count_));
                return _count_;
            }
        }

        public C_Subunit GetSubunit(int index)
        {
            ISubunit _subUnit_;
            Marshal.ThrowExceptionForHR(_DeviceTopology.GetSubunit(index, out _subUnit_));
            return new C_Subunit(_subUnit_);
        }

        public C_Part GetPartById(int id)
        {
            IPart _part_;
            Marshal.ThrowExceptionForHR(_DeviceTopology.GetPartById(id, out _part_));
            return new C_Part(_part_);
        }

        public string GetDeviceId
        {
            get
            {
                string _id_;
                Marshal.ThrowExceptionForHR(_DeviceTopology.GetDeviceId(out _id_));
                return _id_;
            }
        }

        public C_PartsList GetSignalPath(C_Part from, C_Part to, bool rejectMixedPaths)
        {
            IPartsList _partList_;
            Marshal.ThrowExceptionForHR(_DeviceTopology.GetSignalPath((IPart)from, (IPart)to, rejectMixedPaths, out _partList_));
            return new C_PartsList(_partList_);
        }
    }

    public class C_Connector : IDisposable
    {
        private IConnector _Connector;
        private C_Part _Part;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_Connector(IConnector connector)
        {
            _Connector = connector;
        }
        ~C_Connector()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (_Part != null)
                    {
                        _Part.Dispose();
                        _Part = null;
                    }

                    if (_Connector != null)
                    {
                        Marshal.ReleaseComObject(_Connector);
                        _Connector = null;
                    }
                }
            }
        }
        #endregion

        public E_ConnectorType GetConnectorType
        {
            get
            {
                E_ConnectorType _type_;
                Marshal.ThrowExceptionForHR(_Connector.GetType(out _type_));
                return _type_;
            }
        }

        public E_DataFlow GetDataFlow
        {
            get
            {
                E_DataFlow _flow_;
                Marshal.ThrowExceptionForHR(_Connector.GetDataFlow(out _flow_));
                return _flow_;
            }
        }

        public void ConnecTo(C_Connector connectTo)
        {
            Marshal.ThrowExceptionForHR(_Connector.ConnectTo((IConnector)connectTo));
        }

        public void Disconnect()
        {
            Marshal.ThrowExceptionForHR(_Connector.Disconnect());
        }

        public bool IsConnected
        {
            get
            {
                bool _result_;
                Marshal.ThrowExceptionForHR(_Connector.IsConnected(out _result_));
                return _result_;
            }
        }

        public C_Connector GetConnectedTo
        {
            get
            {
                IConnector _connectedTo_;
                Marshal.ThrowExceptionForHR(_Connector.GetConnectedTo(out _connectedTo_));
                return new C_Connector(_connectedTo_);
            }
        }

        public string GetConnectorIdConnectedTo
        {
            get
            {
                string _id_;
                Marshal.ThrowExceptionForHR(_Connector.GetConnectorIdConnectedTo(out _id_));
                return _id_;
            }
        }

        public string GetDeviceIdConnectedTo
        {
            get
            {
                string _id_;
                Marshal.ThrowExceptionForHR(_Connector.GetDeviceIdConnectedTo(out _id_));
                return _id_;
            }
        }

        public C_Part GetPart
        {
            get
            {
                if (_Part == null)
                {
                    var _pUnk_ = Marshal.GetIUnknownForObject(_Connector);
                    IntPtr _ppv_;

                    Marshal.QueryInterface(_pUnk_, ref C_IIDs.IID_IPart, out _ppv_);
                    if (_ppv_ != IntPtr.Zero)
                        _Part = new C_Part((IPart)Marshal.GetObjectForIUnknown(_ppv_));
                    else
                        _Part = null;
                }
                return _Part;
            }
        }
    }

    public class C_Part : IDisposable
    {
        public event EventHandler OnPartNotification;

        private IPart _Part;

        private C_AudioVolumeLevel _AudioVolumeLevel;
        private C_AudioMute _AudioMute;
        private C_AudioPeakMeter _AudioPeakMeter;
        private C_AudioLoudness _AudioLoudness;

        private C_ControlChangeNotify _AudioVolumeLevelChangeNotification;
        private C_ControlChangeNotify _AudioMuteChangeNotification;
        private C_ControlChangeNotify _AudioPeakMeterChangeNotification;
        private C_ControlChangeNotify _AudioLoudnessChangeNotification;

        private C_PartsList partsListIncoming;
        private C_PartsList partsListOutgoing;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_Part(IPart part)
        {
            _Part = part;
        }
        ~C_Part()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    DisposeCtrlChangeNotify(ref _AudioLoudnessChangeNotification);
                    DisposeCtrlChangeNotify(ref _AudioMuteChangeNotification);
                    DisposeCtrlChangeNotify(ref _AudioPeakMeterChangeNotification);
                    DisposeCtrlChangeNotify(ref _AudioVolumeLevelChangeNotification);

                    if (_AudioVolumeLevel != null)
                    {
                        _AudioVolumeLevel.Dispose(); ;
                        _AudioVolumeLevel = null;
                    }

                    if (_AudioMute != null)
                    {
                        _AudioMute.Dispose(); ;
                        _AudioMute = null;
                    }

                    if (_AudioPeakMeter != null)
                    {
                        _AudioPeakMeter.Dispose(); ;
                        _AudioPeakMeter = null;
                    }

                    if (_AudioLoudness != null)
                    {
                        _AudioLoudness.Dispose(); ;
                        _AudioLoudness = null;
                    }

                    if (partsListIncoming != null)
                    {
                        partsListIncoming.Dispose(); ;
                        partsListIncoming = null;
                    }

                    if (partsListOutgoing != null)
                    {
                        partsListOutgoing.Dispose(); ;
                        partsListOutgoing = null;
                    }

                    if (_Part != null)
                    {
                        Marshal.ReleaseComObject(_Part);
                        _Part = null;
                    }
                }
            }
        }

        private void DisposeCtrlChangeNotify(ref C_ControlChangeNotify CCN)
        {
            if (CCN != null)
            {
                try
                {
                    if (CCN.IsAllocated)
                    {
                        Marshal.ThrowExceptionForHR(_Part.UnregisterControlChangeCallback(CCN));
                        CCN.Dispose();
                    }
                }
                catch { }
                CCN = null;
            }
        }
        #endregion

        internal void FireNotification()
        {
            try
            {
                var _handler_ = OnPartNotification;
                if (_handler_ != null)
                {
                    foreach (EventHandler _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, new EventArgs() });
                        else
                            _singleCast_(this, new EventArgs());
                    }
                }
            }
            catch
            { }
        }

        private void GetAudioVolumeLevel()
        {
            object _result_ = null;
            _Part.Activate(E_CLSCTX.ALL, ref C_IIDs.IID_IAudioVolumeLevel, out _result_);
            if (_result_ != null)
            {
                _AudioVolumeLevel = new C_AudioVolumeLevel(_result_ as IAudioVolumeLevel);
                _AudioVolumeLevelChangeNotification = new C_ControlChangeNotify(this);
                Marshal.ThrowExceptionForHR(_Part.RegisterControlChangeCallback(ref C_IIDs.IID_IAudioVolumeLevel, _AudioVolumeLevelChangeNotification));
            }
        }

        private void GetAudioMute()
        {
            object _result_ = null;
            _Part.Activate(E_CLSCTX.ALL, ref C_IIDs.IID_IAudioMute, out _result_);
            if (_result_ != null)
            {
                _AudioMute = new C_AudioMute(_result_ as IAudioMute);
                _AudioMuteChangeNotification = new C_ControlChangeNotify(this);
                Marshal.ThrowExceptionForHR(_Part.RegisterControlChangeCallback(ref C_IIDs.IID_IAudioMute, _AudioMuteChangeNotification));
            }
        }

        private void GetAudioPeakMeter()
        {
            object _result_ = null;
            _Part.Activate(E_CLSCTX.ALL, ref C_IIDs.IID_IAudioPeakMeter, out _result_);
            if (_result_ != null)
            {
                _AudioPeakMeter = new C_AudioPeakMeter(_result_ as IAudioPeakMeter);
                _AudioPeakMeterChangeNotification = new C_ControlChangeNotify(this);
                Marshal.ThrowExceptionForHR(_Part.RegisterControlChangeCallback(ref C_IIDs.IID_IAudioPeakMeter, _AudioPeakMeterChangeNotification));
            }
        }

        private void GetAudioLoudness()
        {
            object _result_ = null;
            _Part.Activate(E_CLSCTX.ALL, ref C_IIDs.IID_IAudioLoudness, out _result_);
            if (_result_ != null)
            {
                _AudioLoudness = new C_AudioLoudness(_result_ as IAudioLoudness);
                _AudioLoudnessChangeNotification = new C_ControlChangeNotify(this);
                Marshal.ThrowExceptionForHR(_Part.RegisterControlChangeCallback(ref C_IIDs.IID_IAudioLoudness, _AudioLoudnessChangeNotification));
            }
        }

        public string GetName
        {
            get
            {
                string _name_;
                Marshal.ThrowExceptionForHR(_Part.GetName(out _name_));
                return _name_;
            }
        }

        public int GetLocalId
        {
            get
            {
                int _id_;
                Marshal.ThrowExceptionForHR(_Part.GetLocalId(out _id_));
                return _id_;
            }
        }

        public string GetGlobalId
        {
            get
            {
                string _id_;
                Marshal.ThrowExceptionForHR(_Part.GetGlobalId(out _id_));
                return _id_;
            }
        }

        public E_PartType GetPartType
        {
            get
            {
                E_PartType _type_;
                Marshal.ThrowExceptionForHR(_Part.GetPartType(out _type_));
                return _type_;
            }
        }

        public Guid GetSubType
        {
            get
            {
                Guid _type_;
                Marshal.ThrowExceptionForHR(_Part.GetSubType(out _type_));
                return _type_;
            }
        }

        public string GetSubTypeName
        {
            get
            {
                var _subType_ = GetSubType;
                var _result_ = FindSubTypeIn(_subType_, typeof(C_KSNODETYPE));
                if (!string.IsNullOrEmpty(_result_)) return _result_;

                _result_ = FindSubTypeIn(_subType_, typeof(C_KSCATEGORY));
                if (!string.IsNullOrEmpty(_result_)) return _result_;

                return "UNDEFINED";
            }
        }

        private static string FindSubTypeIn(Guid findGuid, Type inClass)
        {
            var _fields_ = inClass.GetFields();
            foreach (var _field_ in _fields_)
                if ((Guid)_field_.GetValue(null) == findGuid)
                    return _field_.Name;

            return string.Empty;
        }

        public int GetControlInterfaceCount
        {
            get
            {
                var _count_ = 0;
                Marshal.ThrowExceptionForHR(_Part.GetControlInterfaceCount(out _count_));
                return _count_;
            }
        }

        public C_ControlInterface GetControlInterface(int index)
        {
            IControlInterface _controlInterface_;
            Marshal.ThrowExceptionForHR(_Part.GetControlInterface(index, out _controlInterface_));
            return new C_ControlInterface(_controlInterface_);
        }

        public C_PartsList EnumPartsIncoming
        {
            get
            {
                if (partsListIncoming == null)
                {
                    IPartsList _partsList_ = null;
                    _Part.EnumPartsIncoming(out _partsList_);
                    if (_partsList_ != null) partsListIncoming = new C_PartsList(_partsList_);
                }
                return partsListIncoming;
            }
        }

        public C_PartsList EnumPartsOutgoing
        {
            get
            {
                if (partsListOutgoing == null)
                {
                    IPartsList _partsList_ = null;
                    _Part.EnumPartsOutgoing(out _partsList_);
                    if (_partsList_ != null) partsListOutgoing = new C_PartsList(_partsList_);
                }
                return partsListOutgoing;
            }
        }

        public C_DeviceTopology GetTopologyObject
        {
            get
            {
                IDeviceTopology _deviceTopology_;
                Marshal.ThrowExceptionForHR(_Part.GetTopologyObject(out _deviceTopology_));
                return new C_DeviceTopology(_deviceTopology_);
            }
        }

        public C_AudioVolumeLevel AudioVolumeLevel
        {
            get
            {
                if (_AudioVolumeLevel == null)
                    GetAudioVolumeLevel();

                return _AudioVolumeLevel;
            }
        }

        public C_AudioMute AudioMute
        {
            get
            {
                if (_AudioMute == null)
                    GetAudioMute();

                return _AudioMute;
            }
        }

        public C_AudioPeakMeter AudioPeakMeter
        {
            get
            {
                if (_AudioPeakMeter == null)
                    GetAudioPeakMeter();

                return _AudioPeakMeter;
            }
        }

        public C_AudioLoudness AudioLoudness
        {
            get
            {
                if (_AudioLoudness == null)
                    GetAudioLoudness();

                return _AudioLoudness;
            }
        }
    }

    public class C_PartsList : IDisposable
    {
        private IPartsList _PartsList;
        private Dictionary<int, C_Part> _partsCache;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_PartsList(IPartsList partsList)
        {
            _PartsList = partsList;
            _partsCache = new Dictionary<int, C_Part>();
        }
        ~C_PartsList()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (_partsCache != null)
                    {
                        for (var _i_ = 0; _i_ < _partsCache.Count; _i_++)
                            _partsCache[_i_].Dispose();

                        _partsCache.Clear();
                        _partsCache = null;
                    }

                    if (_PartsList != null)
                    {
                        Marshal.ReleaseComObject(_PartsList);
                        _PartsList = null;
                    }
                }
            }
        }
        #endregion

        public int GetCount
        {
            get
            {
                var _count_ = 0;
                Marshal.ThrowExceptionForHR(_PartsList.GetCount(out _count_));
                return _count_;
            }
        }

        public C_Part GetPart(int index)
        {
            if (_partsCache.ContainsKey(index))
            {
                return _partsCache[index];
            }
            else
            {
                IPart _ipart_;
                Marshal.ThrowExceptionForHR(_PartsList.GetPart(index, out _ipart_));
                var _part_ = new C_Part(_ipart_);
                _partsCache.Add(index, _part_);
                return _part_;
            }
        }
    }

    internal class C_ControlChangeNotify : IControlChangeNotify, IDisposable
    {
        private C_Part _Parent;
        private GCHandle rcwHandle;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_ControlChangeNotify(C_Part parent)
        {
            _Parent = parent;
            rcwHandle = GCHandle.Alloc(this, GCHandleType.Normal);
        }
        ~C_ControlChangeNotify()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (rcwHandle.IsAllocated) rcwHandle.Free();
                }
            }
        }
        #endregion

        public bool IsAllocated
        {
            get { return rcwHandle.IsAllocated; }
        }

        [PreserveSig]
        public int OnNotify(uint dwSenderProcessId, ref Guid pguidEventContext)
        {
            if (System.Diagnostics.Process.GetCurrentProcess().Id != dwSenderProcessId)
                _Parent.FireNotification();
            return 0;
        }
    }

    public class C_Subunit : IDisposable
    {
        private ISubunit _Subunit;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_Subunit(ISubunit subunit)
        {
            _Subunit = subunit;
        }
        ~C_Subunit()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_Subunit != null)
                    {
                        Marshal.ReleaseComObject(_Subunit);
                        _Subunit = null;
                    }
                }
            }
        }
        #endregion
    }

    public class C_AudioVolumeLevel : C_PerChannelDbLevel
    {
        #region Constructor/Destructor/Dispose
        internal C_AudioVolumeLevel(IAudioVolumeLevel audioVolumeLevel)
            : base(audioVolumeLevel)
        { }
        #endregion
    }

    public class C_PerChannelDbLevel : IDisposable
    {
        private IPerChannelDbLevel _PerChannelDbLevel;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_PerChannelDbLevel(IPerChannelDbLevel perChannelDbLevel)
        {
            _PerChannelDbLevel = perChannelDbLevel;
        }
        ~C_PerChannelDbLevel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_PerChannelDbLevel != null)
                    {
                        Marshal.ReleaseComObject(_PerChannelDbLevel);
                        _PerChannelDbLevel = null;
                    }
                }
            }
        }
        #endregion

        public int GetChannelCount
        {
            get
            {
                uint _count_;
                Marshal.ThrowExceptionForHR(_PerChannelDbLevel.GetChannelCount(out _count_));
                return (int)_count_;
            }
        }

        public float GetLevel(int channel)
        {
            System.Threading.Thread.Sleep(5);
            var _level_ = 0F;
            try
            {
                Marshal.ThrowExceptionForHR(_PerChannelDbLevel.GetLevel((uint)channel, out _level_));
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(100);
            }
            return _level_;
        }

        public S_LevelRange GetLevelRange(int channel)
        {
            var _minLevel_ = 0F;
            var _maxLevel_ = 0F;
            var _stepping_ = 0F;
            System.Threading.Thread.Sleep(5);
            try
            {
                Marshal.ThrowExceptionForHR(_PerChannelDbLevel.GetLevelRange((uint)channel, out _minLevel_, out _maxLevel_, out _stepping_));
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(100);
            }
            return new S_LevelRange(_minLevel_, _maxLevel_, _stepping_);
        }

        public void SetLevel(int channel, float level)
        {
            Guid _eventContext_;
            Marshal.ThrowExceptionForHR(_PerChannelDbLevel.SetLevel((uint)channel, level, out _eventContext_));
        }

        public void SetLevelUniform(float level)
        {
            Guid _eventContext_;
            System.Threading.Thread.Sleep(5);
            try
            {
                Marshal.ThrowExceptionForHR(_PerChannelDbLevel.SetLevelUniform(level, out _eventContext_));
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }

    public class C_AudioMute : IDisposable
    {
        private IAudioMute _AudioMute;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioMute(IAudioMute audioMute)
        {
            _AudioMute = audioMute;
        }
        ~C_AudioMute()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioMute != null)
                    {
                        Marshal.ReleaseComObject(_AudioMute);
                        _AudioMute = null;
                    }
                }
            }
        }
        #endregion

        public bool Mute
        {
            get
            {
                bool _muted_;
                Marshal.ThrowExceptionForHR(_AudioMute.GetMute(out _muted_));
                return _muted_;
            }
            set
            {
                Guid _eventContext_;
                Marshal.ThrowExceptionForHR(_AudioMute.SetMute(value, out _eventContext_));
            }
        }
    }

    public class C_AudioPeakMeter : IDisposable
    {
        private IAudioPeakMeter _AudioPeakMeter;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioPeakMeter(IAudioPeakMeter audioPeakMeter)
        {
            _AudioPeakMeter = audioPeakMeter;
        }
        ~C_AudioPeakMeter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioPeakMeter != null)
                    {
                        Marshal.ReleaseComObject(_AudioPeakMeter);
                        _AudioPeakMeter = null;
                    }
                }
            }
        }
        #endregion

        public int GetChannelCount
        {
            get
            {
                int _count_;
                Marshal.ThrowExceptionForHR(_AudioPeakMeter.GetChannelCount(out _count_));
                return _count_;
            }
        }

        public float GetLevel(int channel)
        {
            var _level_ = 0F;
            Marshal.ThrowExceptionForHR(_AudioPeakMeter.GetLevel((uint)channel, out _level_));
            return _level_;
        }
    }

    public class C_AudioLoudness : IDisposable
    {
        private IAudioLoudness _AudioLoudness;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_AudioLoudness(IAudioLoudness audioLoudness)
        {
            _AudioLoudness = audioLoudness;
        }
        ~C_AudioLoudness()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_AudioLoudness != null)
                    {
                        Marshal.ReleaseComObject(_AudioLoudness);
                        _AudioLoudness = null;
                    }
                }
            }
        }
        #endregion

        public bool Enabled
        {
            get
            {
                bool _enabled_;
                Marshal.ThrowExceptionForHR(_AudioLoudness.GetEnabled(out _enabled_));
                return _enabled_;
            }
            set
            {
                Guid _eventContext_;
                Marshal.ThrowExceptionForHR(_AudioLoudness.SetEnabled(value, out _eventContext_));
            }
        }
    }

    public class C_ControlInterface : IDisposable
    {
        private IControlInterface _ControlInterface;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_ControlInterface(IControlInterface controlInterface)
        {
            _ControlInterface = controlInterface;
        }
        ~C_ControlInterface()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_ControlInterface != null)
                    {
                        Marshal.ReleaseComObject(_ControlInterface);
                        _ControlInterface = null;
                    }
                }
            }
        }
        #endregion

        public string GetName
        {
            get
            {
                string _name_;
                Marshal.ThrowExceptionForHR(_ControlInterface.GetName(out _name_));
                return _name_;
            }
        }

        public Guid GetId
        {
            get
            {
                Guid _id_;
                Marshal.ThrowExceptionForHR(_ControlInterface.GetID(out _id_));
                return _id_;
            }
        }
    }
    #endregion
    #endregion

    public class C_MMDeviceEnumerator : IDisposable
    {
        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class C_InternalMMDeviceEnumerator
        { }

        private IMMDeviceEnumerator _realEnumerator = new C_InternalMMDeviceEnumerator() as IMMDeviceEnumerator;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        public C_MMDeviceEnumerator()
        {
            if (Environment.OSVersion.Version.Major < 6)
                throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
        }
        ~C_MMDeviceEnumerator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_realEnumerator != null)
                    {
                        Marshal.ReleaseComObject(_realEnumerator);
                        _realEnumerator = null;
                    }
                }
            }
        }
        #endregion

        public C_MMDeviceCollection EnumerateAudioEndPoints(E_DataFlow dataFlow, E_DeviceState dwStateMask)
        {
            IMMDeviceCollection _result_;
            Marshal.ThrowExceptionForHR(_realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out _result_));
            return new C_MMDeviceCollection(_result_);
        }

        public C_MMDevice GetDefaultAudioEndpoint(E_DataFlow dataFlow, E_Role role)
        {
            IMMDevice _Device_ = null;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out _Device_));
            return new C_MMDevice(_Device_);
        }

        public static void SetDefaultAudioEndpoint(C_MMDevice device)
        {
            device.Selected = true;
        }

        public C_MMDevice GetDevice(string ID)
        {
            IMMDevice _Device_ = null;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDevice(ID, out _Device_));
            return new C_MMDevice(_Device_);
        }
    }

    public class C_MMDeviceCollection : IDisposable
    {
        private IMMDeviceCollection _MMDeviceCollection;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_MMDeviceCollection(IMMDeviceCollection parent)
        {
            _MMDeviceCollection = parent;
        }
        ~C_MMDeviceCollection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    if (_MMDeviceCollection != null)
                    {
                        Marshal.ReleaseComObject(_MMDeviceCollection);
                        _MMDeviceCollection = null;
                    }
                }
            }
        }
        #endregion

        public int Count
        {
            get
            {
                uint _result_;
                Marshal.ThrowExceptionForHR(_MMDeviceCollection.GetCount(out _result_));
                return (int)_result_;
            }
        }

        public C_MMDevice this[int index]
        {
            get
            {
                IMMDevice _result_;
                _MMDeviceCollection.Item((uint)index, out _result_);
                return new C_MMDevice(_result_);
            }
        }
    }

    public class C_MMDevice : IDisposable
    {
        #region Variables
        private IMMDevice _RealDevice;
        private C_PropertyStore _PropertyStore;
        private C_AudioMeterInformation _AudioMeterInformation;
        private C_AudioEndpointVolume _AudioEndpointVolume;
        private C_AudioSessionManager2 _AudioSessionManager2;
        private C_DeviceTopology _DeviceTopology;
        private volatile bool _IsDisposed = false;
        #endregion

        #region Init
        private void GetPropertyInformation()
        {
            IPropertyStore _propstore_;
            Marshal.ThrowExceptionForHR(_RealDevice.OpenPropertyStore(E_StgmAccess.STGM_READ, out _propstore_));
            _PropertyStore = new C_PropertyStore(_propstore_);
        }

        private void GetAudioSessionManager2()
        {
            object _result_;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref C_IIDs.IID_IAudioSessionManager2, E_CLSCTX.ALL, IntPtr.Zero, out _result_));
            _AudioSessionManager2 = new C_AudioSessionManager2(_result_ as IAudioSessionManager2);
        }

        private void GetAudioMeterInformation()
        {
            object _result_;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref C_IIDs.IID_IAudioMeterInformation, E_CLSCTX.ALL, IntPtr.Zero, out _result_));
            _AudioMeterInformation = new C_AudioMeterInformation(_result_ as IAudioMeterInformation);
        }

        private void GetAudioEndpointVolume()
        {
            object _result_;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref C_IIDs.IID_IAudioEndpointVolume, E_CLSCTX.ALL, IntPtr.Zero, out _result_));
            _AudioEndpointVolume = new C_AudioEndpointVolume(_result_ as IAudioEndpointVolume);
        }

        private void GetDeviceTopology()
        {
            object _result_;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref C_IIDs.IID_IDeviceTopology, E_CLSCTX.ALL, IntPtr.Zero, out _result_));
            _DeviceTopology = new C_DeviceTopology(_result_ as IDeviceTopology);
        }
        #endregion

        #region Properties
        public C_AudioSessionManager2 AudioSessionManager2
        {
            get
            {
                if ((_AudioSessionManager2 == null) || (_AudioSessionManager2.IsDisposed)) GetAudioSessionManager2();
                return _AudioSessionManager2;
            }
        }

        public C_AudioMeterInformation AudioMeterInformation
        {
            get
            {
                if (_AudioMeterInformation == null) GetAudioMeterInformation();
                return _AudioMeterInformation;
            }
        }

        public C_AudioEndpointVolume AudioEndpointVolume
        {
            get
            {
                if (_AudioEndpointVolume == null) GetAudioEndpointVolume();
                return _AudioEndpointVolume;
            }
        }

        public C_PropertyStore Properties
        {
            get
            {
                if (_PropertyStore == null) GetPropertyInformation();
                return _PropertyStore;
            }
        }

        public C_DeviceTopology DeviceTopology
        {
            get
            {
                if (_DeviceTopology == null) GetDeviceTopology();
                return _DeviceTopology;
            }
        }

        public string FriendlyName
        {
            get
            {
                if (_PropertyStore == null) GetPropertyInformation();
                if (_PropertyStore.Contains(C_PKEY.PKEY_DeviceInterface_FriendlyName))
                    return (string)_PropertyStore[C_PKEY.PKEY_DeviceInterface_FriendlyName].Value;
                else
                    return "Unknown";
            }
        }

        public string ID
        {
            get
            {
                try
                {
                    string _Result_;
                    Marshal.ThrowExceptionForHR(_RealDevice.GetId(out _Result_));
                    return _Result_;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public E_DataFlow DataFlow
        {
            get
            {
                E_DataFlow _Result_;
                (_RealDevice as IMMEndpoint).GetDataFlow(out _Result_);
                return _Result_;
            }
        }

        public E_DeviceState State
        {
            get
            {
                E_DeviceState _Result_;
                Marshal.ThrowExceptionForHR(_RealDevice.GetState(out _Result_));
                return _Result_;
            }
        }

        public bool Selected
        {
            get
            {
                return (new C_MMDeviceEnumerator()).GetDefaultAudioEndpoint(DataFlow, E_Role.eMultimedia).ID == ID;
            }
            set
            {
                if (value == true)
                    (new C_CPolicyConfigVistaClient()).SetDefaultDevie(ID);
            }
        }
        #endregion

        #region Constructor/Destructor/Dispose
        internal C_MMDevice(IMMDevice realDevice)
        {
            _RealDevice = realDevice;
        }
        ~C_MMDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (_PropertyStore != null)
                    {
                        _PropertyStore.Dispose();
                        _PropertyStore = null;
                    }

                    if (_AudioMeterInformation != null)
                    {
                        _AudioMeterInformation.Dispose();
                        _AudioMeterInformation = null;
                    }

                    if (_AudioEndpointVolume != null)
                    {
                        _AudioEndpointVolume.Dispose();
                        _AudioEndpointVolume = null;
                    }

                    if (_AudioSessionManager2 != null)
                    {
                        _AudioSessionManager2.Dispose();
                        _AudioSessionManager2 = null;
                    }

                    if (_DeviceTopology != null)
                    {
                        _DeviceTopology.Dispose();
                        _DeviceTopology = null;
                    }

                    if (_RealDevice != null)
                    {
                        Marshal.ReleaseComObject(_RealDevice);
                        _RealDevice = null;
                    }
                }
            }
        }
        #endregion
    }

    #region property store
    /// <summary>
    /// Property Store class, only supports reading properties at the moment.
    /// </summary>
    public class C_PropertyStore : IDisposable
    {
        private IPropertyStore _Store;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_PropertyStore(IPropertyStore store)
        {
            _Store = store;
        }
        ~C_PropertyStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;

                    if (_Store != null)
                    {
                        Marshal.ReleaseComObject(_Store);
                        _Store = null;
                    }
                }
            }
        }
        #endregion

        public int Count
        {
            get
            {
                int _Result_;
                Marshal.ThrowExceptionForHR(_Store.GetCount(out _Result_));
                return _Result_;
            }
        }

        public C_PropertyStoreProperty this[int index]
        {
            get
            {
                S_PropVariant _result_;
                var _key_ = Get(index);
                Marshal.ThrowExceptionForHR(_Store.GetValue(ref _key_, out _result_));
                return new C_PropertyStoreProperty(_key_, _result_);
            }
        }

        public bool Contains(S_PropertyKey testKey)
        {
            for (var _i_ = 0; _i_ < Count; _i_++)
            {
                var _key_ = Get(_i_);
                if (_key_.fmtid == testKey.fmtid && _key_.pid == testKey.pid)
                    return true;
            }
            return false;
        }

        public C_PropertyStoreProperty this[S_PropertyKey testKey]
        {
            get
            {
                S_PropVariant _result_;
                for (var _i_ = 0; _i_ < Count; _i_++)
                {
                    var _key_ = Get(_i_);
                    if (_key_.fmtid == testKey.fmtid && _key_.pid == testKey.pid)
                    {
                        Marshal.ThrowExceptionForHR(_Store.GetValue(ref _key_, out _result_));
                        return new C_PropertyStoreProperty(_key_, _result_);
                    }
                }
                return null;
            }
        }

        public S_PropertyKey Get(int index)
        {
            S_PropertyKey _key_;
            Marshal.ThrowExceptionForHR(_Store.GetAt(index, out _key_));
            return _key_;
        }

        public S_PropVariant GetValue(int index)
        {
            S_PropVariant _result_;
            var _key_ = Get(index);
            Marshal.ThrowExceptionForHR(_Store.GetValue(ref _key_, out _result_));
            return _result_;
        }
    }

    public class C_PropertyStoreProperty : IDisposable
    {
        private S_PropertyKey _PropertyKey;
        private S_PropVariant _PropValue;
        private volatile bool _IsDisposed = false;

        #region Constructor/Destructor/Dispose
        internal C_PropertyStoreProperty(S_PropertyKey key, S_PropVariant value)
        {
            _PropertyKey = key;
            _PropValue = value;
        }
        ~C_PropertyStoreProperty()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool ManagedCleanup)
        {
            if (ManagedCleanup)
            {
                if (!_IsDisposed)
                {
                    _IsDisposed = true;
                    // nothing to do
                }
            }
        }
        #endregion

        public S_PropertyKey Key
        {
            get
            {
                return _PropertyKey;
            }
        }

        public object Value
        {
            get
            {
                return _PropValue.Value;
            }
        }
    }
    #endregion
}
