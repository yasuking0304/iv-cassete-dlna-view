using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace View.Manager {
    class ServiceAccessManager {

        public static bool? GetIvdrServiceStatus() {
            bool? retValue = null;
            try {
                ServiceController sc = new ServiceController(
                        App.Current.Resources["SC_IVDR_NAME"].ToString(), ".");
                if ( sc.Status == ServiceControllerStatus.Stopped ) {
                    retValue = false;
                } else if ( sc.Status == ServiceControllerStatus.Running ) {
                    retValue = true;
                }
            } catch {
                retValue = null;
            }
            return retValue;
        }

        public static bool? SetIvdrServiceStart() {
            bool? retValue = null;
            try {
                ServiceController sc = new ServiceController(
                            App.Current.Resources["SC_IVDR_NAME"].ToString(), ".");
                if ( sc.Status == ServiceControllerStatus.Stopped ) {
                    // 30秒でタイムアウト
                    TimeSpan ts = new TimeSpan(0,0,30);
                    sc.Start();
                    sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, ts);
                }
                if ( sc.Status == ServiceControllerStatus.Running ) {
                    retValue = true;
                }
            } catch (System.ServiceProcess.TimeoutException) {
                retValue = false;
            } catch {
                retValue = null;
            }
            return retValue;
        }
    }
}
