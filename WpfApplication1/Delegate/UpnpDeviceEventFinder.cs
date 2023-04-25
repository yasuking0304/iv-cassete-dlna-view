using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using UPNPLib;
using View.CommonEnum;
using View.CommonStruct;
using View.EventArgs;
using View.Manager;
using View.upnpLocation;
using System.ComponentModel;

namespace View.Delegate {
    class UpnpDeviceEventFinder : IUPnPDeviceFinderCallback {

        int DeviceFindID = 0;
        UPnPDeviceFinder finder = new UPnPDeviceFinder();
        UPnPLocation objLocationXml = null;

        #region IUPnPDeviceFinderCallback

        public void DeviceAdded(int lFindData, UPnPDevice dev) {
            // デバイス追加
            IUPnPDeviceDocumentAccess devicesAccess = null;
            string deviceLocationURL = string.Empty;
            string ivdrControlURL = string.Empty;
            string contDirControlURL = string.Empty;
            try {
                X_Record ScheduledRecording = X_Record.NONE;
                try {
                    devicesAccess = dev as IUPnPDeviceDocumentAccess;
                    deviceLocationURL = devicesAccess.GetDocumentURL();
                    string dd = WebAccessManager.GetWebSource(deviceLocationURL);
                    objLocationXml = uPnpAccessManager.GetInstance().DeserializeLocation(dd);
                    contDirControlURL = WebAccessManager.GetAddUrl(
                                            deviceLocationURL,
                                            objLocationXml.Device.GetContentDirectoryControlURL);
                    if ( objLocationXml.Device.IsXivdr ) {
                        ivdrControlURL = WebAccessManager.GetAddUrl(
                                            deviceLocationURL,
                                            objLocationXml.Device.GetXIvdrControlURL);
                    }

                    if ( objLocationXml.Device.IsScheduledRecording ) {
                        ScheduledRecording = X_Record.SCHEDULEDRECORDING1;
                    } else if ( objLocationXml.Device.IsScheduledRecording2 ) {
                        ScheduledRecording = X_Record.SCHEDULEDRECORDING2;
                    }
                    if ( objLocationXml.Device.IsXscheduledRecordingExt ) {
                        ScheduledRecording = X_Record.X_SCHEDULEDRECORDINGEXT;
                    }
                    if ( objLocationXml.Device.IsXscheduledRecording ) {
                        ScheduledRecording = X_Record.X_SCHEDULEDRECORDING;
                    }
                } catch ( Exception e ) {
                ;
                } finally {
                }
                OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(
                                    new DataStruct.Drivemap(
                                        dev.FriendlyName,
                                        dev.UniqueDeviceName,
                                        WebAccessManager.GetAddUrl(
                                            deviceLocationURL,
                                            objLocationXml.Device.GetIcoURL()),
                                        dev.ModelName,
                                        dev.ManufacturerName,
                                        dev.ModelNumber,
                                        dev.PresentationURL,
                                        ScheduledRecording,
                                        deviceLocationURL,
                                        objLocationXml.Device.GetDlnaSupport,
                                        ivdrControlURL,
                                        contDirControlURL
                                        ), EventStatus.ONADD_DEVICE
                                        )
                                    );
            } catch ( Exception ) {
            } finally {
            }
            // オブジェクトを取り除く
            Marshal.ReleaseComObject(dev);
        }

        public void DeviceRemoved(int lFindData, string bstrUDN) {
            // デバイスが削除
               OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(
                                    new DataStruct.Drivemap(
                                        string.Empty,
                                        bstrUDN,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        X_Record.NONE,
                                        string.Empty,
                                        DlnaSupport.NONE,
                                        string.Empty,
                                        string.Empty
                                        ), EventStatus.ONDEL_DEVICE
                                        )
                                    );
         ;
        }

        public void SearchComplete(int lFindData) {
            // 検索完了
            OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(EventStatus.ONEND_DEVICE));
        ;
        }

        #endregion

        // Upnpイベント通知用
        public delegate void DriveEventHandler(object sender, UpnpDriveInfoEventArgs e);
        public event DriveEventHandler  DriveInfo;

        public virtual void OnDriveRaiseEvent(UpnpDriveInfoEventArgs e) {
            if (DriveInfo != null) {
                DriveInfo(this, e);
            }
        }
        public void AddUpnpDeviceNotifyEvent() {
            if ( DriveInfo == null ) {
                DriveInfo += new DriveEventHandler(View.MainWindow.GetInstance().RaiseEvent);
            }
        }
        public void DeleteUpnpDeviceNotifyEvent() {
            while (DriveInfo != null) {
                DriveInfo -= new DriveEventHandler(View.MainWindow.GetInstance().RaiseEvent);
            };
        }

        /// <summary>
        /// リスナ通知開始
        /// </summary>
        public void FindDeviceAsync() {
            DeviceFindID = finder.CreateAsyncFind("urn:schemas-upnp-org:service:ContentDirectory:1", 0, this);
            finder.StartAsyncFind(DeviceFindID);
        }

        /// <summary>
        /// リスナ通知解除
        /// </summary>
        public void CancelFindDeviceAsync() {
            DeleteUpnpDeviceNotifyEvent();
            if ( DeviceFindID != 0 ) {
                finder.CancelAsyncFind(DeviceFindID);
            }
        }
    }
}
