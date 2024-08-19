using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace View.Manager {
    class ServiceAccessManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void SetCursorPos(int X, int Y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private struct POINT
        {
            public int x;
            public int y;
        }
        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;

        public static bool? GetIvdrServiceStatus()
        {
            bool? retValue = null;
            try
            {
                ServiceController sc = new ServiceController(
                        App.Current.Resources["SC_IVDR_NAME"].ToString(), ".");
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    retValue = false;
                }
                else if (sc.Status == ServiceControllerStatus.Running)
                {
                    retValue = true;
                }
            }
            catch
            {
                retValue = null;
            }
            return retValue;
        }

        public static bool? SetIvdrServiceStart()
        {
            bool? retValue = null;
            try
            {
                ServiceController sc = new ServiceController(
                            App.Current.Resources["SC_IVDR_NAME"].ToString(), ".");
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    // 30秒でタイムアウト
                    TimeSpan ts = new TimeSpan(0, 0, 30);
                    sc.Start();
                    sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, ts);
                }
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    retValue = true;
                }
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                retValue = false;
            }
            catch
            {
                retValue = null;
            }
            return retValue;
        }

        public static bool? RestartIvdrTrayTool()
        {
            bool? retValue = null;
            Process[] ps =
                Process.GetProcessesByName("rhdmunplug");

            /// マウスカーソルの位置を記憶
            GetCursorPos(out POINT point);
            foreach (Process p in ps)
            {
                string path = p.MainModule.FileName;
                // 強制終了する
                p.Kill();

                //プロセスが終了するまで最大10秒待機する
                p.WaitForExit(10000);
                //プロセスが終了したか確認する
                if (p.HasExited)
                {
                    retValue = true;
                    TaskTrayCleanning();
                }
                else
                {
                    retValue = false;
                }
                if (retValue is true)
                {
                    // プロセス再起動
                    _ = Process.Start(path);
                }
            }
            /// カーソルの位置復元
            SetCursorPos(point.x, point.y);
            return retValue;
        }

        /// <summary>
        /// タスクトレイに残ったアイコンの残骸を消す(マウスオーバーすると消える性質を利用)
        /// </summary>
        private static void TaskTrayCleanning()
        {
            IntPtr hwndTaskBar = FindWindow("Shell_TrayWnd", null);
            IntPtr hwndTray = FindWindowEx(hwndTaskBar, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr hwndSysPager = FindWindowEx(hwndTray, IntPtr.Zero, "SysPager", null);
            /// タスクトレイに対してマウスオーバー縦断
            if (GetWindowRect(hwndSysPager, out RECT rect))
            {
                for (int x = rect.Left; x < rect.Right; x += 24)
                {
                    for (int y = rect.Top; y < rect.Bottom; y += 24)
                    {
                        SetCursorPos(x + 8, y + 8);
                        Thread.Sleep(1);
                    }
                }
            }
            IntPtr hwndOverflow = FindWindow("NotifyIconOverflowWindow", null);
            IntPtr hwndButton = FindWindowEx(hwndTray, IntPtr.Zero, "Button", null);
            /// オーバーレイタスクトレイ画面を開く
            if (hwndOverflow == IntPtr.Zero || !IsWindowVisible(hwndOverflow))
            {
                if (GetWindowRect(hwndButton, out rect))
                {
                    SetCursorPos(rect.Left + 8, rect.Top + 8);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);  /// マウスの左ボタンクリックイベント
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    Thread.Sleep(1);
                }
            }
            /// オーバーレイタスクトレイ画面に対してマウスオーバー縦断
            hwndOverflow = FindWindow("NotifyIconOverflowWindow", null);
            if (GetWindowRect(hwndOverflow, out rect))
            {
                for (int x = rect.Left; x < rect.Right; x += 24)
                {
                    for (int y = rect.Top; y < rect.Bottom; y += 24)
                    {
                        SetCursorPos(x + 8, y + 8);
                        Thread.Sleep(1);
                    }
                }
            }
            /// オーバーレイタスクトレイ画面を閉じる
            if (GetWindowRect(hwndButton, out rect))
            {
                SetCursorPos(rect.Left + 8, rect.Top + 8);
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);  /// マウスの左ボタンクリックイベント
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                Thread.Sleep(1);
            }
        }
    }
}
