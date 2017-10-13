using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace VolumeManager
{
    public class C_GlobalHotKey : IDisposable
    {
        /* HowTo use :
         
        C_GlobalHotKey hotkey;

        hotkey = new C_GlobalHotKey();
        hotkey.HotkeyPressed += hotkey_HotkeyPressed;
        hotkey.RegisterGlobalHotKey( (int) Keys.F11, WindowsConsts.MOD_CONTROL, this.Handle );
        hotkey.UnregisterGlobalHotKey();

        protected override void WndProc (ref Message m)
        {
             C_GlobalHotKey.ProcessWndProc(ref m);
             base.WndProc(ref m);
        }
         
         */

        /// <summary>Handle of the current process</summary>
        private IntPtr _Handle;
        private volatile bool _IsDisposed = false;

        public C_GlobalHotKey()
        {
            _Handle = Process.GetCurrentProcess().Handle;
        }
        ~C_GlobalHotKey()
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
                    UnregisterGlobalHotKey();
                }
            }
        }

        public event EventHandler HotkeyPressed;

        protected virtual void OnHotkeyPressed(EventArgs e)
        {
            try
            {
                var _handler_ = HotkeyPressed;
                if (_handler_ != null)
                    foreach (EventHandler _singleCast_ in _handler_.GetInvocationList())
                    {
                        System.ComponentModel.ISynchronizeInvoke syncInvoke = _singleCast_.Target as System.ComponentModel.ISynchronizeInvoke;
                        if ((syncInvoke != null) && (syncInvoke.InvokeRequired))
                            syncInvoke.Invoke(_singleCast_, new object[] { this, e });
                        else
                            _singleCast_(this, e);
                    }
            }
            catch
            { }
        }

        /// <summary>The ID for the hotkey</summary>
        public short HotkeyID { get; private set; }

        /// <summary>Register the hotkey</summary>
        public bool RegisterGlobalHotKey(int hotkey, int modifiers, IntPtr handle)
        {
            UnregisterGlobalHotKey();
            _Handle = handle;
            return RegisterGlobalHotKey(hotkey, modifiers);
        }

        /// <summary>Register the hotkey</summary>
        public bool RegisterGlobalHotKey(int hotkey, int modifiers)
        {
            UnregisterGlobalHotKey();

            try
            {
                // use the GlobalAddAtom API to get a unique ID (as suggested by MSDN)
                var _HotkeyID_ = NativeMethods.GlobalAddAtom(Guid.NewGuid().ToString());
                if (_HotkeyID_ == 0)
                    throw new Exception($"Unable to generate unique hotkey ID. Error: {Marshal.GetLastWin32Error().ToString()}");
                else
                    HotkeyID = _HotkeyID_;

                // register the hotkey, throw if any error
                if (!NativeMethods.RegisterHotKey(_Handle, HotkeyID, (uint)modifiers, (uint)hotkey))
                    throw new Exception($"Unable to register hotkey. Error: {Marshal.GetLastWin32Error().ToString()}");

                lock (_Instances)
                    _Instances.Add(this);

                return true;
            }
            catch (Exception ex)
            {
                // clean up if hotkey registration failed
                Dispose();
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>Unregister the hotkey</summary>
        public void UnregisterGlobalHotKey()
        {
            if (HotkeyID != 0)
            {
                NativeMethods.UnregisterHotKey(_Handle, HotkeyID);
                // clean up the atom list
                NativeMethods.GlobalDeleteAtom(HotkeyID);
                HotkeyID = 0;
                lock (_Instances)
                    _Instances.Remove(this);
            }
        }

        private static volatile List<C_GlobalHotKey> _Instances = new List<C_GlobalHotKey>();
        public static void ProcessWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WindowsConsts.WM_HOTKEY:
                    {
                        lock (_Instances)
                            foreach (var _Instance_ in _Instances)
                                if ((short)m.WParam == _Instance_.HotkeyID)
                                {
                                    _Instance_.OnHotkeyPressed(null);
                                    return;
                                }
                        break;
                    }
                default:
                    return;
            }
        }
    }

    public static partial class WindowsConsts
    {
        public const int MOD_ALT = 1;
        public const int MOD_CONTROL = 2;
        public const int MOD_SHIFT = 4;
        public const int MOD_WIN = 8;

        public const int WM_HOTKEY = 0x312;

        public const int WM_NCHITTEST = 0x0084;
        public const int HTCAPTION = 2;
    }

    public static partial class NativeMethods
    {
        /// <summary> The RegisterHotKey function defines a system-wide hot key </summary>
        /// <param name="hwnd">Handle to the window that will receive WM_HOTKEY messages  generated by the hot key.</param>
        /// <param name="id">Specifies the identifier of the hot key.</param>
        /// <param name="fsModifiers">Specifies keys that must be pressed in combination with the key  specified by the 'vk' parameter in order to generate the WM_HOTKEY message.</param>
        /// <param name="vk">Specifies the virtual-key code of the hot key</param>
        /// <returns><c>true</c> if the function succeeds, otherwise <c>false</c></returns>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ms646309(VS.85).aspx"/>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        internal static extern int UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern short GlobalDeleteAtom(short nAtom);
    }
}
