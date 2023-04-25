using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using View.CommonStruct;
using View.CommonEnum;
using View.DataConverter.StringConverter;
using View.DataConverter.LanguageConverter;

namespace View.EventArgs {

    public class UpnpDriveInfoEventArgs {
        /// <summary>
        /// LOCATION
        /// </summary>
        string location;
        /// <summary>
        /// upnp名
        /// </summary>
        string drive;
        /// <summary>
        /// uniqueDeviceName
        /// </summary>
        string fileSystem;
        /// <summary>
        /// DlnaイメージアイコンURI
        /// </summary>
        string dlnaImage;
        /// <summary>
        /// モデル名
        /// </summary>
        string modelName;
        /// <summary>
        /// 製造メーカ名
        /// </summary>
        string manufacturerName;
        /// <summary>
        /// モデル名
        /// </summary>
        string modelNumber;
        /// <summary>
        /// 提供URI
        /// </summary>
        string presentationURL;
        /// <summary>
        /// 録画機能
        /// </summary>
        X_Record scheduledRecording;
        /// <summary>
        /// イベント状態
        /// </summary>
        EventStatus eventStatus = EventStatus.ONSTART;
        /// <summary>
        /// フラグ
        /// </summary>
        DlnaSupport dlnaSupport = DlnaSupport.NONE;
        /// <summary>
        ///  ivdrサービスURL
        /// </summary>
        string ivdrControlURL = string.Empty;
        /// <summary>
        ///  contentDirectoryサービスURL
        /// </summary>
        string contentDirectoryControlURL = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ss"></param>
        public UpnpDriveInfoEventArgs(CommonStruct.DataStruct.Drivemap indata) {
            this.drive      = indata.drive;
            this.fileSystem = indata.fileSystem;
            this.dlnaImage  = indata.dlnaImage;
            this.modelName  = indata.modelName;
            this.manufacturerName   = indata.manufacturerName;
            this.modelNumber        = indata.modelNumber;
            this.presentationURL    = indata.presentationURL;
            this.scheduledRecording = indata.scheduledRecording;
            this.eventStatus        = EventStatus.ONDATA;
            this.location    = indata.location;
            this.dlnaSupport = indata.dlnaSupport;
            this.ivdrControlURL = indata.ivdrControlUrl;
            this.contentDirectoryControlURL = indata.cdirControlUrl;
        }

        public UpnpDriveInfoEventArgs(CommonStruct.DataStruct.Drivemap indata, EventStatus status) {
            this.drive      = indata.drive;
            this.fileSystem = indata.fileSystem;
            this.dlnaImage  = indata.dlnaImage;
            this.modelName  = indata.modelName;
            this.manufacturerName   = indata.manufacturerName;
            this.modelNumber        = indata.modelNumber;
            this.presentationURL    = indata.presentationURL;
            this.scheduledRecording = indata.scheduledRecording;
            this.eventStatus        = status;
            this.location    = indata.location;
            this.dlnaSupport = indata.dlnaSupport;
            this.ivdrControlURL = indata.ivdrControlUrl;
            this.contentDirectoryControlURL = indata.cdirControlUrl;
        }

        public UpnpDriveInfoEventArgs(EventStatus eof) {
            this.location   = null;
            this.drive      = null;
            this.fileSystem = null;
            this.dlnaImage  = null;
            this.modelName  = null;
            this.manufacturerName   = null;
            this.modelNumber        = null;
            this.presentationURL    = null;
            this.scheduledRecording = X_Record.NONE;
            this.eventStatus        = eof;
            this.dlnaSupport = DlnaSupport.NONE;
            this.ivdrControlURL = string.Empty;
            this.contentDirectoryControlURL = string.Empty;
        }

        public UpnpDriveInfoEventArgs(string drive, EventStatus eof) {
            this.location   = null;
            this.drive      = drive;
            this.fileSystem = null;
            this.dlnaImage  = null;
            this.modelName  = null;
            this.manufacturerName   = null;
            this.modelNumber        = null;
            this.presentationURL    = null;
            this.scheduledRecording = X_Record.NONE;
            this.eventStatus        = eof;
            this.dlnaSupport = DlnaSupport.NONE;
            this.ivdrControlURL = string.Empty;
            this.contentDirectoryControlURL = string.Empty;
        }

        public string GetLocation() {
            return (location == null) ? string.Empty : location;
        }

        public string GetDriveName() {
            return (drive == null) ? string.Empty : drive;
        }

        public string GetFileSystem() {
            return (fileSystem == null) ? string.Empty : fileSystem;
        }

        public string GetDlnaImage() {
            return (dlnaImage == null) ? string.Empty : dlnaImage;
        }

        public string GetModelName() {
            return (modelName == null) ? string.Empty : modelName;
        }

        public string GetManufacturerName() {
            return (manufacturerName == null) ? string.Empty : manufacturerName;
        }

        public string GetModelNumber() {
            return (modelNumber == null) ? string.Empty : modelNumber;
        }

        public string GePresentationURL() {
            return (presentationURL == null) ? string.Empty : presentationURL;
        }

        public  X_Record GetScheduledRecording() {
            return scheduledRecording;
        }

        public EventStatus GetEventStatus() {
            return eventStatus;
        }

        public DlnaSupport GetDlnaSupport() {
            return dlnaSupport;
        }

        public string GetIvdrControlURL() {
            return ivdrControlURL;
        }

        public string GetContentDirectoryControlURL() {
            return contentDirectoryControlURL;
        }



        public DataStruct.Drivemap GetUpnpDriveInfo() {
        return ( new DataStruct.Drivemap(drive,
                                         fileSystem,
                                         dlnaImage,
                                         modelName,
                                         manufacturerName,
                                         modelNumber,
                                         presentationURL,
                                         scheduledRecording,
                                         location,
                                         dlnaSupport,
                                         ivdrControlURL,
                                         contentDirectoryControlURL
                                        ));
        }

    }
}
