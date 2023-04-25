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
using UPNPLib;

namespace View.EventArgs {

    public class UpnpListInfoEventArgs {

        /// <summary>
        /// デバイス名
        /// </summary>
        string deviceName = string.Empty;
        /// <summary>
        /// タイトル
        /// </summary>
        string title = string.Empty;
        /// <summary>
        /// 録画日時
        /// </summary>
        DateTime? recordDateTime = null;
        /// <summary>
        /// 録画時間
        /// </summary>
        TimeSpan? durationTime = null;
        /// <summary>
        /// ジャンル
        /// </summary>
        string genre = string.Empty;
        /// <summary>
        /// イメージURI
        /// </summary>
        string imageUri = string.Empty;
        /// <summary>
        /// 説明
        /// </summary>
        string description = string.Empty;
        /// <summary>
        /// ファイルに紐づく固有の番号
        /// </summary>
        string id = string.Empty;
        /// <summary>
        /// ファイルに紐づく固有の番号
        /// </summary>
        string parentId = string.Empty;
        /// <summary>
        /// ファイル総数
        /// </summary>
        int totalCount = -1;
        /// <summary>
        /// ビットマップ
        /// </summary>
        public System.Windows.Media.Imaging.BitmapSource bmp = null;
        /// <summary>
        /// イベント状態
        /// </summary>
        EventStatus eventStatus = EventStatus.ONSTART;
        /// <summary>
        /// 
        /// </summary>
        string classes = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        string serviceUrl = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        string channel = string.Empty;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ss"></param>

        // フォルダ用コンストラクタ
        public UpnpListInfoEventArgs(string deviceName,
                                     string title, string id, string parentId,
                                     string classes, string serviceUrl) {
            // デバイス名
            this.deviceName = deviceName;
            // タイトル
            if ( title != null && title.Trim() != string.Empty) {
                this.title = LanguageConverter.GetInstance().GetAribtext2Hirabun(title);
            } else {
                this.title = string.Empty;
            }
            // ファイル固有番号
            if ( id == null || id.Trim() == string.Empty || id == "0" ) {
                this.id      = string.Empty;
            } else {
                this.id      = id;
            }
            this.parentId = (parentId != null) ? parentId.Trim() : string.Empty;
            this.eventStatus = EventStatus.ONDATA_FOLDER;
            this.recordDateTime = DateTime.Now;
            this.durationTime = null;
            this.genre = string.Empty;
            this.imageUri = string.Empty;
            this.bmp = null;
            this.description = string.Empty;
            this.totalCount = -1;
            this.classes = classes;
            this.serviceUrl = serviceUrl;
            this.channel = string.Empty;
        }

        // 番組リスト用コンストラクタ
        public UpnpListInfoEventArgs(string deviceName,
                                     string title, string recordDateTime, DidlLite.DIDLRes[] resList,
                                     string description, string genre,
                                     string id, string parentId, string classes,
                                     int totalcount,
                                     string serviceUrl, string channel) {
            DateTime workrecordDateTime = new DateTime();
            TimeSpan workdurationTime = new TimeSpan();
            this.recordDateTime =  null;
            this.durationTime = null;
            this.imageUri = string.Empty;
            this.classes = classes;
            this.channel = LanguageConverter.GetInstance().GetAribtext2Hirabun(channel);
            try {
                // デバイス名
                this.deviceName = deviceName;
                // タイトル
                if ( title != null && title.Trim() != string.Empty) {
                    this.title = LanguageConverter.GetInstance().GetAribtext2Hirabun(title);
                } else {
                    this.title = string.Empty;
                }
                // ファイル固有番号
                this.id = ( id == null || id.Trim() == string.Empty || id == "0" ) ? string.Empty : id;
                this.parentId = (parentId != null) ? parentId.Trim() : string.Empty;
                this.eventStatus = EventStatus.ONDATA_ITEM;
                if ( DateTime.TryParse(recordDateTime, out workrecordDateTime) ) {
                    this.recordDateTime = workrecordDateTime;
                }
                if ( resList != null ) {
                    foreach ( DidlLite.DIDLRes res in resList ) {
                        if ( res != null && res.duration != null && res.duration != string.Empty ) {
                            if ( TimeSpan.TryParse( res.duration, out workdurationTime ) ) {
                                this.durationTime = workdurationTime;
                            }
                        }
                        if ( res != null && res.protocolInfo != null
                                         && ( res.protocolInfo.IndexOf(":DLNA.ORG_PN=PNG_TN") > 0
                                           || res.protocolInfo.IndexOf(":DLNA.ORG_PN=JPEG_TN") > 0) ) {
                            this.imageUri = res.res;
                        }
                    }
                }
                this.genre = genre;
                this.bmp = null;
                this.description = description;
                this.totalCount = totalcount;
                this.serviceUrl = serviceUrl;
            } catch (Exception) {
            ;
            } finally {
            }
        }

        public UpnpListInfoEventArgs(EventStatus eof) {
            this.deviceName = string.Empty;
            this.title = string.Empty;
            this.recordDateTime = null;
            this.durationTime = null;
            this.imageUri = string.Empty;
            this.id = string.Empty;
            this.bmp = null;
            this.description = string.Empty;
            this.eventStatus = eof;
            this.totalCount = -1;
            this.classes = string.Empty;
            this.serviceUrl = string.Empty;
            this.channel = string.Empty;
        }
        
        public void Set(string recordDateTime, DidlLite.DIDLRes[] resList,
                                     string description, string genre) {
            DateTime workrecordDateTime = new DateTime();
            TimeSpan workdurationTime = new TimeSpan();
            this.recordDateTime = null;
            this.durationTime = null;
            if ( DateTime.TryParse(recordDateTime, out workrecordDateTime) ) {
                this.recordDateTime = workrecordDateTime;
            }
            if ( resList != null ) {
                foreach (DidlLite.DIDLRes res in resList ) {
                    if ( res.duration != null && res.duration != string.Empty ) {
                        if (TimeSpan.TryParse(res.duration, out workdurationTime) ) {
                            this.durationTime = workdurationTime;
                        }
                        break;
                    }
                }
            }
            this.genre = genre;
            this.description = description;
        }

        public string GetDeviceName() {
            return deviceName;
        }

        public string GetTitle() {
            return title;
        }

        public DateTime? GetRecordDatTime() {
            return recordDateTime;
        }

        public TimeSpan? GetRecDurationTime() {
            return durationTime;
        }

        public string GetImageUri() {
            return imageUri;
        }

        public void SetImageUri(System.Windows.Media.Imaging.BitmapSource bmp) {
            this.bmp = bmp;
        }

        public string GetDescription() {
            if ( description != null && description.Trim() != string.Empty ) {
                return LanguageConverter.GetInstance().GetAribtext2Hirabun(description);
            } else {
                return string.Empty;
            }
        }

        public string GetGenre() {
            if ( genre == null || genre.Trim() == string.Empty ) {
                return Application.Current.TryFindResource("LBL_GENRE_UNKNOWN") as string;
            } else {
                return genre;
            }
        }

        public string GetId() {
            return id;
        }

        public int GetTotalCount() {
            return totalCount;
        }

        public string GetParentId() {
            return parentId;
        }

        public EventStatus GetEventStatus() {
            return eventStatus;
        }

        public string GetClasses() {
            return classes;
        }

        public string GetUPnPServiceUrl() {
            return serviceUrl;
        }

        public string GetChannel() {
            return channel;
        }
    }
}
