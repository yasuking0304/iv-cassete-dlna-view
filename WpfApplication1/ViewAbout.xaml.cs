using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using View.Manager;

namespace View
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class ViewAbout : Window {

        public ViewAbout() {
            InitializeComponent();
            App.Current.MainWindow.FontFamily =
                    new System.Windows.Media.FontFamily("Meiryo UI, MS UI Gothic");
        }
 
        private void WindowLoaded(object sender, RoutedEventArgs e) {
            IconHelper.RemoveIcon(this);
            Assembly asm = Assembly.GetExecutingAssembly();
            // バージョンの取得
            Version.Text = asm.GetName().Version.ToString();
            // Copyrightの取得
            foreach ( object asinfo in asm.GetCustomAttributes(true) ) {
                if ( asinfo is System.Reflection.AssemblyTitleAttribute ) {
                    AsmTitle.Text = ( asinfo as System.Reflection.AssemblyTitleAttribute ).Title;
                }
                if ( asinfo is System.Reflection.AssemblyCopyrightAttribute ) {
                    Copyright.Text = ( asinfo as System.Reflection.AssemblyCopyrightAttribute ).Copyright;
                }
            }
            ViewServiceStatus();
        }    

        private void ViewServiceStatus () {
            bool? sc = ServiceAccessManager.GetIvdrServiceStatus();
            if ( sc != null && sc == false ) {
                // Icon
                SmallSHIELD.Source = ImageAccessManager.GetSmallSHIELDIcon();
                LabelStartService.Visibility = Visibility.Visible;
            } else if ( sc == null || sc == true ) {
                // Icon
                SmallSHIELD.Source = null;
                LabelStartService.Visibility = Visibility.Hidden;
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e) {
            this.Close();
        }

        // HYperLink クリック
        private void OnClick(object sender, RoutedEventArgs e) {
            try {
                System.Diagnostics.Process.Start(MyUrl.Text);
            } catch {
            ;
            }
        }
        private void OnServiceStartClicked(object sender, RoutedEventArgs e) {
            Mouse.OverrideCursor = Cursors.Wait;
            System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += (s, evt) =>  {
                Thread.Sleep(10);
            };
            bw.RunWorkerCompleted += (s, evt) =>  {
                //管理者として自分自身を起動する
                System.Diagnostics.ProcessStartInfo psi =
                    new System.Diagnostics.ProcessStartInfo();
                //ShellExecuteを使う。デフォルトtrueなので、必要はない。
                psi.UseShellExecute = true;
                //自分自身のパスを設定する
                psi.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                //動詞に「runas」をつける
                psi.Verb = "runas";
                //引数
                psi.Arguments = "/netstart ivdrs_media_server";
                try {
                    //起動する
                    System.Diagnostics.Process prs = System.Diagnostics.Process.Start(psi);
                    prs.WaitForExit();
                }
                catch (System.ComponentModel.Win32Exception ex) {
                    //「ユーザーアカウント制御」ダイアログでキャンセルされたなどによって
                    //起動できなかった時
                    Console.WriteLine("起動しませんでした: " + ex.Message);
                }
                ViewServiceStatus();
                Mouse.OverrideCursor =null;
            };
            // 別スレッドを生成し実行
            bw.RunWorkerAsync();
        }

        internal static class IconHelper
        {
            [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hwnd, int index);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

            [DllImport("user32.dll")]
            static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

            const int GWL_STYLE = -16;
            const int WS_SYSMENU = 0x00080000;

            const int GWL_EXSTYLE = -20;
            const int WS_EX_DLGMODALFRAME = 0x0001;
            const int SWP_NOSIZE = 0x0001;
            const int SWP_NOMOVE = 0x0002;
            const int SWP_NOZORDER = 0x0004;
            const int SWP_FRAMECHANGED = 0x0020;
            const uint WM_SETICON = 0x0080;

            public static void RemoveIcon(Window window) {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                int extendedStyle = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, extendedStyle &= ~WS_SYSMENU);
		        SendMessage(hwnd, WM_SETICON, (IntPtr)0, IntPtr.Zero);
		        SendMessage(hwnd, WM_SETICON, (IntPtr)1, IntPtr.Zero);
                //SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            }
        }
    }
}
