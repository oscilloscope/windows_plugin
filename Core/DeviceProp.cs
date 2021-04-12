using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pGina.Core
{
    class DeviceProp
    {

        const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);


        public static string[] ScreenSize()
        {

            string[] screenSize = new string[2];

            foreach (Screen screen in Screen.AllScreens)
            {
                DEVMODE dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);

                //Console.WriteLine($"Device: {screen.DeviceName}");
                //Console.WriteLine($"Real Resolution: {dm.dmPelsWidth}x{dm.dmPelsHeight}");
                //Console.WriteLine($"Virtual Resolution: {screen.Bounds.Width}x{screen.Bounds.Height}");
                //Console.WriteLine("Device name: " + screen.DeviceName);
                //Console.WriteLine();

                screenSize[0] = dm.dmPelsWidth.ToString();
                screenSize[1] = dm.dmPelsHeight.ToString();

            }
            return screenSize;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

    }


}
