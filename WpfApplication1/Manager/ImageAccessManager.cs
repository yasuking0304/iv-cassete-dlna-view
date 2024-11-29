using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows.Interop;

namespace View.Manager {
    internal class ImageAccessManager {

        private const uint SHGSI_ICON      = 0x000000100;
        private const uint SHGSI_SMALLICON = 0x000000001;
        private const uint SIID_SHIELD     = 0x00000004D;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHSTOCKICONINFO {
            public int cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO sii);
    
        /// <summary>
        /// 権限昇格シールドアイコン取得
        /// </summary>
        /// <returns></returns>
        public static ImageSource GetSmallSHIELDIcon() {
            ImageSource retImage = null;
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
            sii.cbSize = Marshal.SizeOf(sii);

            SHGetStockIconInfo(SIID_SHIELD, SHGSI_ICON | SHGSI_SMALLICON, ref sii);
            if (sii.hIcon != IntPtr.Zero) {
                retImage = Imaging.CreateBitmapSourceFromHIcon(
                                        sii.hIcon,
                                        System.Windows.Int32Rect.Empty,
                                        BitmapSizeOptions.FromEmptyOptions()
                                    );
                retImage.Freeze();
            }
            return retImage;
        }
    }
}
