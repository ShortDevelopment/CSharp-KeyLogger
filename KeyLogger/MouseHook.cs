using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyLogger
{
    public class MouseHook : IDisposable
    {
        public bool LockAllClicks { get; set; }

        public MouseHook()
        {
            Initialize();
        }

        private LowLevelMouseProc callback;
        private IntPtr _hookID = IntPtr.Zero;
        private void Initialize()
        {
            callback = (int nCode, IntPtr wParam, IntPtr lParam) =>
            {
                HookStruct info = (HookStruct)Marshal.PtrToStructure(lParam, typeof(HookStruct));
                if (LockAllClicks && info.flags != LLKHF_INJECTED)
                    return (IntPtr)1;
                else
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
            };
            _hookID = SetHook(callback);
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (curProcess.MainModule)
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
            GC.KeepAlive(callback);
        }

        private const int LLKHF_INJECTED = 1;
        private const int WH_MOUSE_LL = 14;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 513,
            WM_LBUTTONUP,
            WM_MOUSEMOVE = 512,
            WM_MOUSEWHEEL = 522,
            WM_RBUTTONDOWN = 516,
            WM_RBUTTONUP
        }

        private struct Win32Point
        {
            public int x;
            public int y;
        }

        private struct HookStruct
        {
            public Win32Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #region API

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}
