using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using View.CommonEnum;
using View.Manager;
using View.EventArgs;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace View
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {

        // Win32 APIのインポート
        [DllImport("user32.dll", SetLastError = true )]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam); 

        internal enum DeviceType : uint {
            DBT_DEVTYP_OEM = 0x00000000,
            DBT_DEVTYP_DEVNODE = 0x00000001,
            DBT_DEVTYP_VOLUME = 0x00000002,
            DBT_DEVTYP_PORT = 0x00000003,
            DBT_DEVTYP_NET = 0x00000004
        }

        internal enum VolumeChangeFlags : ushort {
            DBTF_MEDIA = 0x0001,
            DBTF_NET = 0x0002
        }

        // メッセージ構造体
        [StructLayout(LayoutKind.Sequential)]
        internal struct DEV_BROADCAST_VOLUME {
            public uint dbcv_size;
            public DeviceType dbcv_devicetype;
            public uint dbcv_reserved;
            public uint dbcv_unitmask;
            public VolumeChangeFlags dbcv_flags;
        } 

        private static System.Threading.Mutex _instanceMutex = null;
        private static App instance = null;
        private static System.IO.StreamWriter sw = null;
        private static System.Diagnostics.Process p = null;
        private static String driveLetter = null;
        private static IpcClient client;

        public static App GetInstance() {
            if (instance == null) {
                instance = new App();
            }
            return instance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnStartup(object sender, StartupEventArgs e) {
            if ( e.Args.Length >= 2 ) {
                if ( e.Args[0] == "/diskpart" ) {
                    ComMain(e.Args);
                    Application.Current.Shutdown();
                    return;
                }
                if (e.Args[0] == "/chkdsk") {
                    ComMainChkdsk(e.Args);
                    Application.Current.Shutdown();
                    return;
                }
                if ( e.Args[0] == "/netstart" && e.Args[1] == "ivdrs_media_server" ) {
                    bool? sv = ServiceAccessManager.SetIvdrServiceStart();
                    if ( sv != null && sv == false ) {
                        string scNameMessage = App.Current.Resources["SC_IVDR_NAME"].ToString()
                                             + App.Current.Resources["SC_STARTFAIL"].ToString();
                        MessageBox.Show( scNameMessage
                                       , App.Current.Resources["SC_STARTFAIL_TITLE"].ToString()
                                       , MessageBoxButton.OK
                                       , MessageBoxImage.Error);
                    }
                    Application.Current.Shutdown();
                    return;
                }
            }
            // check that there is only one instance of the control panel running...
            bool createdNew;
            _instanceMutex = new System.Threading.Mutex(true, @"Global\IvCasseteDlnaView", out createdNew);
            if (!createdNew) {
                _instanceMutex = null;
                Application.Current.Shutdown();
                return;
            }
        }

        public void OnExit(object sender, ExitEventArgs e) {          
            if(_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
        }

        public static void DoEvents() {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public static object ExitFrames(object f) {
            ((DispatcherFrame)f).Continue = false;
   
            return null;
        }

        private void ComMain(string[] args) {
            //インスタンス接続
            instance = this;
            //
            //Processオブジェクトを作成
            p = new System.Diagnostics.Process();

            //入力できるようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;

            //非同期で出力を読み取れるようにする
            p.StartInfo.RedirectStandardOutput = true;
            // cmd.exeの設定
            p.StartInfo.FileName =
                System.Environment.GetEnvironmentVariable("ComSpec");
            p.StartInfo.CreateNoWindow = true;

            //起動
            p.Start();

            //非同期で出力の読み取りを開始
            p.BeginOutputReadLine();
            // ドライブレターの設定
            driveLetter = args[1];
            //入力のストリームを取得
            using (sw = p.StandardInput) {
                sw.WriteLine(@"chcp 437");
                //「diskpart」を実行する
                sw.WriteLine(@"diskpart");
                //「readonly属性解除」を実行する
                sw.WriteLine(@"select volume " + driveLetter);
                sw.WriteLine(@"attribute disk");
                sw.WriteLine(@"attribute disk clear readonly");
                //終了する
                sw.WriteLine(@"exit");
                //終了する
                sw.WriteLine(@"exit");
            }
            sw.Close();
        
            p.WaitForExit();
            p.Close();
            Console.ReadLine();
        }

        private void ComMainChkdsk(string[] args) {

            MainWindow wnd = new MainWindow();
            wnd.CheckDiskStartMessageIVDR();


            //インスタンス接続
            instance = this;

            //Processオブジェクトを作成
            p = new System.Diagnostics.Process();

            //入力できるようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            //非同期で出力を読み取れるようにする
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            // cmd.exeの設定
            p.StartInfo.FileName =
                System.Environment.GetEnvironmentVariable("ComSpec");
            p.StartInfo.CreateNoWindow = true;

            //非同期で出力の読み取りを開始
            p.OutputDataReceived += new DataReceivedEventHandler(processDataReceived);
            p.ErrorDataReceived += new DataReceivedEventHandler(processDataReceived);
            //起動
            p.Start();
            // 出力開始
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            client = new IpcClient();
            // ドライブレターの設定
            driveLetter = args[1];

            //入力のストリームを取得
            sw = p.StandardInput;
            sw.WriteLine(@"chcp 437");
            //「diskpart」を実行する
            sw.WriteLine(@"diskpart");
            sw.WriteLine(@"select volume " + driveLetter);
            sw.WriteLine(@"attribute disk");
            Thread.Sleep(1000);
            while (!p.HasExited) {
                Thread.Sleep(1000);
            }
            p.WaitForExit();
            p.Close();
        }

        private void processDataReceived(object sender, DataReceivedEventArgs e) {
            String line = e.Data.Trim();
            if (line == "") {
                return;
            }
            // diskpart 
            if (line.IndexOf("Microsoft DiskPart version") != -1) {
                //diskpartがロードされた
                client.remoteObject.sendData("ロック確認を開始します。");
            }
            // diskpart 
            if (line.IndexOf("Current Read-only State : No") != -1) {
                //diskpartを終了する
                sw.WriteLine(@"exit");
                client.remoteObject.sendData("ロックはされていませんでした。");
            }
            // diskpart 
            if (line.IndexOf("Current Read-only State : Yes") != -1) {
                //remoteObj.Message = "DISKPART";
                sw.WriteLine(@"attribute disk clear readonly");
                // diskpartを終了する
                sw.WriteLine(@"exit");
                client.remoteObject.sendData( "ロックを解除しました。");
            }
            // diskpart exit
            if (line.IndexOf("Leaving DiskPart...") != -1) {
                Thread.Sleep(2000);
                // 残り容量チェック
                sw.WriteLine(@"fsutil volume diskfree " + driveLetter);
                client.remoteObject.sendData("ロック処理を終了中です...");
            }
            // fsutil volume diskfree 成功
            if (line.IndexOf("Total free bytes") != -1 && line.IndexOf(" 0 (  0.0 KB)") == -1) {
                // コマンドプロンプトを終了する
                sw.WriteLine(@"exit");
                sw.Close();
                client.remoteObject.sendData("チェック処理を終了します。");
            }
            // fsutil volume diskfree 失敗?
            if (line.IndexOf("Total free bytes") != -1 && line.IndexOf(" 0 (  0.0 KB)") != -1) {
                // chkdsk起動
                sw.WriteLine(@"chkdsk /f " + driveLetter);
                client.remoteObject.sendData("disk調査処理を開始します。");
            }
            // chkdsk /f
            if (line.IndexOf("The type of the file system is") != -1 && line.IndexOf("UDF.") != -1) {
                client.remoteObject.sendData("このドライブはiVDRです。");
            }
            if (line.IndexOf("The type of the file system is") != -1 && line.IndexOf("UDF.") == -1) {
                client.remoteObject.sendData("このドライブはiVDRではありません。");
            }
            if (line.IndexOf("CHKDSK is verifying ICBs") != -1) {
                client.remoteObject.sendData("disk調査(1/5)");
            }
            if (line.IndexOf("CHKDSK is looking for orphan ICBs") != -1) {
                client.remoteObject.sendData("disk調査(2/5)");
            }
            if (line.IndexOf("CHKDSK is verifying ICB links") != -1) {
                client.remoteObject.sendData("disk調査(3/5)");
            }
            if (line.IndexOf("CHKDSK is verifying link counts and parent entries") != -1) {
                client.remoteObject.sendData("disk調査(4/5)");
            }
            if (line.IndexOf("CHKDSK is verifying object size for ICBs") != -1) {
                client.remoteObject.sendData("disk調査(5/5)");
            }
            if (line.IndexOf("file system and found no problems") != -1) {
                client.remoteObject.sendData("disk調査結果、問題はありませんでした。");
            }
            if (line.IndexOf("file system and found problems") != -1) {
                client.remoteObject.sendData("disk回復処理を行います。30分から1時間かかる場合もあります。");
            }
            if (line.IndexOf("allocation units available on disk") != -1) {
                client.remoteObject.sendData("disk調査処理を終了します。");
                Thread.Sleep(2000);
                // コマンドプロンプトを終了する
                sw.WriteLine(@"exit");
                sw.Close();
                client.remoteObject.sendData("チェック処理を終了します。");
            }
        }
    }
    class IpcClient {
        public IpcRemoteObject remoteObject;

        public IpcClient() {
            // クライアントチャンネル生成
            IpcClientChannel channel = new IpcClientChannel();

            // チャンネル登録
            ChannelServices.RegisterChannel(channel, true);

            // リモートオブジェクト取得
            remoteObject = Activator.GetObject(typeof(IpcRemoteObject),
                "ipc://ivdr/ipc") as IpcRemoteObject;
        }
    }
}
