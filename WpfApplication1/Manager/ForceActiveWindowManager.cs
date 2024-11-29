using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace View.Manager
{
    internal class ForceActiveWindowManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(
            IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AttachThreadInput(
            int idAttach, int idAttachTo, bool fAttach);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(
            uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        private const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        private const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        private const uint SPIF_SENDCHANGE = 0x2;

        /// <summary>
        /// 画面を強制的にアクティブ化、最小化解除
        /// </summary>
        /// <param name="handle">フォームハンドル</param>
        /// <param name="window">Window</param>
        public static void ForceActiveWindow(IntPtr handle, Window window)
        {
            // タスクバーが点滅すると無反応で結果にfalseが返却されるので何度かリトライ
            for (int i = 0; i < 3; i++)
            {
                if (ForceActive(handle))
                {
                    break;
                }
            }
            window.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Windowフォーム画面を強制的にアクティブ化
        /// </summary>
        /// <param name="handle">フォームハンドル</param>
        /// <returns>true: 成功, false: 失敗</returns>
        private static bool ForceActive(IntPtr handle)
        {
            IntPtr dummy = IntPtr.Zero;
            IntPtr timeout = IntPtr.Zero;

            // フォアグラウンドウィンドウのスレッドIDを取得
            int foregroundID = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            // 目的のウィンドウを作成したスレッドIDを取得
            int targetID = GetWindowThreadProcessId(handle, out _);
            // スレッドのインプット状態を連結
            AttachThreadInput(targetID, foregroundID, true);
            // 現在のウィンドウの切り替え時間を保存
            _ = SystemParametersInfo(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, timeout, 0);
            // ウィンドウの切り替え時間を 0ms にする
            _ = SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, dummy, SPIF_SENDCHANGE);
            // ウィンドウをフォアグラウンドに持ってくる
            bool isSuccess = SetForegroundWindow(handle);
            // 切り替え時間を戻す
            _ = SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, timeout, SPIF_SENDCHANGE);
            // スレッドのインプット状態を解除
            _ = AttachThreadInput(targetID, foregroundID, false);
            return isSuccess;
        }
    }
}
