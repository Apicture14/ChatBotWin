using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;

namespace ChatBotWin
{
    public static class WinApi
    {
        public const uint GMEM_MOVABLE = 0x0002;
        public const uint CF_BITMAP = 2;
        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(uint flag,uint size);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalFree(IntPtr hMem);
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint type,IntPtr hMem);

        public static void ClearBoard()
        {
            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            CloseClipboard();

            if (GetLastError() != 0)
            {
                throw new Exception(GetLastError().ToString());
            } 
        }
        public static bool CopyImage(Image image)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return false;
            }
            EmptyClipboard();
            SetClipboardData(CF_BITMAP, new Bitmap(image).GetHbitmap());
            CloseClipboard();
            return true;
        }
        public static void CopyToClipboardViaWinAPI(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap(); // 返回 HBITMAP 句柄
            if (hBitmap == IntPtr.Zero)
            {
                Console.WriteLine("GetHbitmap failed!");
                return;
            }

            if (!OpenClipboard(IntPtr.Zero))
            {
                Console.WriteLine("OpenClipboard failed!");
                return;
            }

            try
            {
                EmptyClipboard();

                // 注意：这里不能删除 hBitmap！SetClipboardData 后系统拥有它。
                IntPtr result = SetClipboardData(CF_BITMAP, hBitmap);

                if (result == IntPtr.Zero)
                {
                    Console.WriteLine("SetClipboardData failed!");
                }
            }
            finally
            {
                CloseClipboard();
            }
        }
    }
}
