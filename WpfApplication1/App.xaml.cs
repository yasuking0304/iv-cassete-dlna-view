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

        private static Mutex _instanceMutex = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnStartup(object sender, StartupEventArgs e) {
            if ( e.Args.Length >= 2 ) {
                if (e.Args[0] == "/chkdsk") {
                    DiskMaintenanceManager diskMan = new DiskMaintenanceManager();
                    diskMan.Repair(e.Args[1]);
                    Current.Shutdown();
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
                    Current.Shutdown();
                    return;
                }
            }
            // check that there is only one instance of the control panel running...
            bool createdNew;
            _instanceMutex = new System.Threading.Mutex(true, @"Global\IvCasseteDlnaView", out createdNew);
            if (!createdNew) {
                _instanceMutex = null;
                Current.Shutdown();
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
    }
}
