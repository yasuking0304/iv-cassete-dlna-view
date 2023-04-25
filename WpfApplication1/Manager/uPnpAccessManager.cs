using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UPNPLib;
using View.CommonEnum;
using View.CommonStruct;
using View.Delegate;
using View.EventArgs;
using View.DidlLite;
using View.upnpLocation;

namespace View.Manager {

    class uPnpAccessManager {

        // インスタンス生成
        private static uPnpAccessManager instance = null;

        public static uPnpAccessManager GetInstance() {
            if (instance == null) {
                instance = new uPnpAccessManager();
            }
            return instance;
        }
        UpnpEventFinder upnpFinder = new UpnpEventFinder();

        // 中断フラグ
        private bool isSearchCanceled = false;
        private bool IsSearchCanceled {
            set {
                this.isSearchCanceled = value;
            }
            get {
                return this.isSearchCanceled;
            }
        }
        //
        private int Allcount = 0;

        /// <summary>
        /// DIDL xmlのデシリアライズ
        /// </summary>
        /// <param name="str">DIDL-Liteを格納した文字列</param>
        /// <returns>Xmlパースしたクラス</returns>
        private DIDLLite DeserializeDIDL(string str) {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(DIDLLite));
            Stream sr =  new MemoryStream(Encoding.Unicode.GetBytes(str.Replace("upnp:class", "upnp:cLass")));
            DIDLLite obj = null;
            try {
                obj = (DIDLLite)serializer.Deserialize(sr);
            } catch (Exception) {
            } finally {
                sr.Close();
            }
            return obj;
        }

        /// <summary>
        /// DIDL xmlのデシリアライズ
        /// </summary>
        /// <param name="str">DIDL-Liteを格納した文字列</param>
        /// <returns>Xmlパースしたクラス</returns>
        public RecordDestinationInfo DeserializeRecInfo(string str) {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(RecordDestinationInfo));
            Stream sr =  new MemoryStream(Encoding.UTF8.GetBytes(str));
            RecordDestinationInfo obj = null;
            try {
                obj = (RecordDestinationInfo)serializer.Deserialize(sr);
            } catch (Exception) {
            } finally {
                sr.Close();
            }
            return obj;
        }

        /// <summary>
        /// xmlのデシリアライズ(DIDL-Lite)
        /// </summary>
        /// <param name="str">DIDL-Liteを格納した文字列</param>
        /// <returns>Xmlパースしたクラス</returns>
        public RecordDestinations DeserializeRecs(string str) {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(RecordDestinations));
            Stream sr =  new MemoryStream(Encoding.UTF8.GetBytes(str));
            RecordDestinations obj = null;
            try {
                obj = (RecordDestinations)serializer.Deserialize(sr);
            } catch (Exception) {
            } finally {
                sr.Close();
            }
            return obj;
        }

        /// <summary>
        /// xmlのデシリアライズ(LocationURL)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public UPnPLocation DeserializeLocation(string str) {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(UPnPLocation));
            Stream sr =  new MemoryStream(Encoding.UTF8.GetBytes(str));
            UPnPLocation obj = null;
            try {
                obj = (UPnPLocation)serializer.Deserialize(sr);
            } catch (Exception) {
            } finally {
                sr.Close();
            }
            return obj;
        }
        
        /// <summary>
        /// xmlのデシリアライズ(SOAPコマンド)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public Envelope DeserializeEnvelope(string str) {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(Envelope));
            Stream sr =  new MemoryStream(Encoding.UTF8.GetBytes(str));
            Envelope obj = null;
            try {
                obj = (Envelope)serializer.Deserialize(sr);
            } catch (Exception) {
            } finally {
                sr.Close();
            }
            return obj;
        }
        
        /// <summary>
        /// SOAPコマンドでディスクサイズ取得
        /// </summary>
        /// <param name="url"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public List <DataStruct.IvdrxSize> GetDlnaDisksSizes(string url, EXEC_MODE mode) {
            List <DataStruct.IvdrxSize> sizelist = new List<DataStruct.IvdrxSize>{};
            Envelope objIvdrXml = null;
            try {
                if ( mode == EXEC_MODE.DLNA ) {
                    // X_HDLnkGetRecordDestinations -> X_HDLnkGetRecordDestinationInfo
                    RecordDestinations objRecsXml = null;
                    objIvdrXml = GetInstance().DeserializeEnvelope(
                                                    WebAccessManager.GetXRecordDestinations(url));
                    if ( objIvdrXml != null && objIvdrXml.Body.X_HDLnkGetRecordDestinationsResponse != null ) {
                        objRecsXml = GetInstance().DeserializeRecs(
                                                    objIvdrXml.Body.
                                                    X_HDLnkGetRecordDestinationsResponse.
                                                    RecordDestinationList);
                        foreach ( RecordDestination recslist in objRecsXml.RecordDestination) {
                            objIvdrXml = GetInstance().DeserializeEnvelope(
                                                    WebAccessManager.GetXRecordDestinationInfo(
                                                    url,
                                                    recslist.destID));
                            if ( objIvdrXml != null && objIvdrXml.Body.
                                                        X_HDLnkGetRecordDestinationInfoResponse != null ) {
                                RecordDestinationInfo objRecInfXml = GetInstance().DeserializeRecInfo(
                                                    objIvdrXml.Body.
                                                    X_HDLnkGetRecordDestinationInfoResponse.
                                                    RecordDestinationInfo);
                                sizelist.Add(new DataStruct.IvdrxSize( objRecInfXml.types,
                                                                        objRecInfXml.FreeSize,
                                                                        objRecInfXml.TotalSize));
                            }
                        }
                    }
                } else if ( mode == EXEC_MODE.IVDR ) {
                    // GetDriveInfo -> GetDriveInfo2
                    // ivdr1
                    objIvdrXml = GetInstance().DeserializeEnvelope(
                                                    WebAccessManager.GetDataIvdr_GetDriveInfo( url ));
                    if ( objIvdrXml != null ) {
                        sizelist.Add( new DataStruct.IvdrxSize("IVDR1",
                                                    objIvdrXml.Body.GetDriveInfoResponse.FreeSize,
                                                    objIvdrXml.Body.GetDriveInfoResponse.TotalSize));
                        // ivdr2
                        objIvdrXml = GetInstance().DeserializeEnvelope(
                                                    WebAccessManager.GetDataIvdr_GetDriveInfo2( url, 1 ));
                        if ( objIvdrXml != null ) {
                            sizelist.Add( new DataStruct.IvdrxSize("IVDR2",
                                                    objIvdrXml.Body.GetDriveInfoResponse.FreeSize,
                                                    objIvdrXml.Body.GetDriveInfoResponse.TotalSize));
                        }
                    }
                }
            } catch ( Exception ) {
                ;
            }
            return sizelist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="cdirUrl"></param>
        /// <param name="param"></param>
        public void GetUpnpRecordListCdirUrl(string deviceName, string cdirUrl, DlnaSupport param) {
            // 開始コード通知
            upnpFinder.AddEventsFileEvent();
            upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(EventStatus.ONSTART));
            upnpFinder.AddEventsFolderEvent();
            upnpFinder.OnFolderRaiseEvent(new FolderInfoEventArgs(EventStatus.ONSTART_GENRE));
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, evt) =>  {
                // ここはUIスレッドではない
                // DLNAライブチューナチェックを行っていないならチェックする
                //if ( (param & DlnaSupport.CHKLIVETUNER) == DlnaSupport.NONE ) { 
                //    bool? hasVideoBoradcast = UpnpCheckVideoBroadcast(deviceName, cdirUrl, false, null, 0);
                //}
                int totalcount = UpnpRecordListUrl(deviceName, cdirUrl, false, null, null, string.Empty, string.Empty);
            };
            // スレッド終了時処理。ここはUIスレッド
            bw.RunWorkerCompleted += (s, evt) =>  {
                // 終了コード通知
                upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(EventStatus.ONEND));
                upnpFinder.DeleteEventsFileEvent();
                upnpFinder.OnFolderRaiseEvent(new FolderInfoEventArgs(EventStatus.ONEND_GENRE));
                upnpFinder.DeleteEventsFolderEvent();
            };
            // 別スレッドを生成し実行
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        public void UpnpRecordContainerUrlInvoke(
                        string deviceName, string cdirUrl, string id, string parentId, string title) {
            this.IsSearchCanceled = false;
            upnpFinder.AddEventsFileEvent();
            upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(EventStatus.ONSTART));
            upnpFinder.AddEventsFolderEvent();
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, evt) =>  {
                // ここはUIスレッドではない
                bool refuse = (title.StartsWith("すべて")) ? true : false;
                int countRet = UpnpRecordListUrl(deviceName, cdirUrl, refuse,
                                                id, parentId, title, parentId);
            };
            // スレッド終了時処理。ここはUIスレッド
            bw.RunWorkerCompleted += (s, evt) =>  {
                upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(EventStatus.ONEND));
                upnpFinder.DeleteEventsFileEvent();
                upnpFinder.OnFolderRaiseEvent(new FolderInfoEventArgs(EventStatus.ONEND_GENRE));
                upnpFinder.DeleteEventsFolderEvent();
            };
            // 別スレッドを生成し実行
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// 検索キャンセル
        /// </summary>
        public void CanceledUpnpListInvoke() {
            IsSearchCanceled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpnpDlnaNameListInvoke() {
            upnpFinder.AddEventsDriveEvent();
            upnpFinder.OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(EventStatus.ONSTART));

            ///List<DataStruct.Drivemap> value = new List<DataStruct.Drivemap> {};
            BackgroundWorker bw = new BackgroundWorker();
            UPnPDevices devices = null;
            IUPnPDeviceDocumentAccess devicesAccess = null;
            UPnPLocation objLocationXml = null;
            bw.DoWork += (s, evt) =>  {
                // ここはUIスレッドではない
                UPnPDeviceFinder finder = new UPnPDeviceFinder();
                devices = finder.FindByType("urn:schemas-upnp-org:service:ContentDirectory:1", 1);
            };
            // スレッド終了時処理。ここはUIスレッド
            bw.RunWorkerCompleted += (s, evt) =>  {
                foreach (UPnPDevice dev in devices) {
                    string deviceLocationURL = string.Empty;
                    string ivdrControlURL = string.Empty;
                    string contDirControlURL = string.Empty;
                    try
                    {
                        X_Record ScheduledRecording = X_Record.NONE;
                        try {
                            devicesAccess = dev as IUPnPDeviceDocumentAccess;
                            deviceLocationURL = devicesAccess.GetDocumentURL();
                            objLocationXml = DeserializeLocation(
                                            WebAccessManager.GetWebSource(deviceLocationURL));
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
                           Debug.Print(e.ToString());
                        } finally {
                        }

                        upnpFinder.OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(
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
                                                         ))
                                                     );
                    } catch ( Exception ) {
                    } finally {
                    }
                }
                // devが検索されたデバイス
                upnpFinder.OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(EventStatus.ONEND));
                upnpFinder.DeleteEventsDriveEvent();
            ;
            };
            // 別スレッドを生成し実行
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="sevUrl"></param>
        /// <param name="recursion">再帰探索する場合:true</param>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private int UpnpRecordListUrl(string deviceName, string sevUrl, bool recursion,
                                      string id, string parentId, string title, string rootId) {
            Envelope objResponseXml = null;
            if ( !recursion ) this.Allcount = 0; 
            int count = 0;
            int countContain = 0;
            int startIndex = 0;
            int totalcount = 0;
            int itemtotalcount = 0;
            string dummyquery = "?dummy="+DateTime.Now.Ticks.ToString();
            string nowId = ( id == null || id.Trim() == string.Empty ) ? "0" : id.Trim();
            string parentNowId = string.Empty;
            object vOutActionArgs = new object();
            View.DidlLite.DIDLLite objDIDL = new DIDLLite { Container = null, Item = null };
            if (nowId != "0") {
                object[] parentParams =
                    new object[6] { nowId, "BrowseMetadata", "*", 0, 10, string.Empty };
                try {
                    objResponseXml = GetInstance().DeserializeEnvelope(
                                        WebAccessManager.GetBrowseList(sevUrl, parentParams));
                    if ( objResponseXml != null && objResponseXml.Body != null
                        && objResponseXml.Body.BrowseResponse != null ) {
                        objDIDL = DeserializeDIDL(objResponseXml.Body.BrowseResponse.Result);
                        if (objDIDL.Container != null) {
                            parentId = objDIDL.Container[0].parentID;
                        }
                    }
                } catch ( Exception ) {
                } finally {
                }
            }
            int setReadcount = 100;
            do {
                object[] inSearchParams =
                    new object[6] { nowId, "BrowseDirectChildren", "*", startIndex, setReadcount, string.Empty };
                objDIDL = new DIDLLite { Container = null, Item = null };

                try {
                    countContain = 0;
                    count = 0;
                    objResponseXml = GetInstance().DeserializeEnvelope(
                                        WebAccessManager.GetBrowseList(sevUrl, inSearchParams));
                    if ( objResponseXml != null && objResponseXml.Body != null
                        && objResponseXml.Body.BrowseResponse != null ) {
                        totalcount = objResponseXml.Body.BrowseResponse.TotalMatches;
                        objDIDL = DeserializeDIDL(objResponseXml.Body.BrowseResponse.Result);
                    }
                } catch ( Exception ) {
                    objDIDL = new DIDLLite { Container = null, Item = null };
                } finally {
                }
                if (objDIDL.Container != null && objDIDL.Item != null && startIndex == 0 && recursion == false) {
                    return UpnpRecordListUrl(deviceName, sevUrl, true, id, parentId, title, parentId);
                }
                if ( objDIDL.Container == null && objDIDL.Item == null && startIndex == 0 && recursion == false) {
                    upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(
                        deviceName,
                        "..",
                        parentId,
                        parentNowId,
                        "object.container",
                        sevUrl));
                    return 0;
                }
                if ( objDIDL.Container != null ) {
                    countContain = objDIDL.Container.Length;
                    setReadcount = countContain;
                    for ( int i=0; i< countContain; i++ ) {
                        DIDLContainer container = objDIDL.Container[i];
                        if ( !recursion ) {
                            if ( i == 0 && container.parentID != "0"  && startIndex == 0 ) {
                                upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(
                                    deviceName,
                                    "..",
                                    parentId,
                                    parentNowId,
                                    "object.container",
                                    sevUrl));
                            }
                            upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(
                                deviceName,
                                container.title,
                                container.id,
                                container.parentID,
                                container.cLass,
                                sevUrl));
                        }
                        //
                        if ( recursion ) {
                            totalcount+= UpnpRecordListUrl(
                                    deviceName, sevUrl, true, container.id, id, container.title, rootId);
                        }
                    }
                }
                if ( objDIDL.Item != null ) {
                    count = objDIDL.Item.Length;
                    itemtotalcount = totalcount;
                    for ( int i=0; i< count ;i++ ){
                        DIDLItem item = objDIDL.Item[i];
                        if (i == 0 && item.parentID != "0" && startIndex == 0) {
                            upnpFinder.OnFileRaiseEvent(new UpnpListInfoEventArgs(
                                deviceName,
                                "..",
                                rootId,
                                parentNowId,
                                "object.container",
                                sevUrl));
                        }
                        string genre = string.Empty;
                        if ( item.genre != null ) {
                            foreach ( string genr in item.genre ) {
                                // コンボボックスにジャンル追加
                                upnpFinder.OnFolderRaiseEvent(
                                    new FolderInfoEventArgs(genr) );
                                genre += (genre == string.Empty)
                                                ? genr : "\t\t\t\t" + genr;
                            }
                        }

                        UpnpListInfoEventArgs e = new UpnpListInfoEventArgs(
                            deviceName,
                            item.title,
                            item.date,
                            item.Res,
                            (item.longDescription != null) ? item.longDescription : item.description,
                            genre,
                            item.id,
                            parentId,
                            item.cLass,
                            this.Allcount + itemtotalcount,
                            sevUrl,
                            (item.channelName != null ) 
                                ? item.objectType + ',' + item.channelNr + ',' + item.channelName
                                : item.objectType + ',' + item.channelNr + ',' + string.Empty
                            );
                        upnpFinder.OnFileRaiseEvent(e);
                    }
                }
                startIndex += count + countContain;
                if ( this.IsSearchCanceled ) {
                    this.IsSearchCanceled = false;
                    break;
                }
            } while ( totalcount >  startIndex );
            this.Allcount += itemtotalcount;
            return totalcount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="sevUrl"></param>
        /// <param name="recursion"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool? UpnpCheckVideoBroadcast(string deviceName, string sevUrl,
                                             bool recursion, string id, int nestCounter) {
            if ( nestCounter > 10 ) {
                return null;
            }
            if ( this.IsSearchCanceled ) {
                this.IsSearchCanceled = false;
                return null;
            }
            Envelope objResponseXml = null;
            bool? isVideotuner = false;
            if ( !recursion ) {
                this.Allcount = 0;
                upnpFinder.AddEventsDriveEvent();
                upnpFinder.OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(deviceName, EventStatus.ON_CHKLIVETUNER));
                upnpFinder.DeleteEventsDriveEvent();
            }
            int count = 0;
            int startIndex = 0;
            int totalcount = 0;
            int itemtotalcount = 0;
            
            string nowId = ( id == null || id.Trim() == string.Empty ) ? "0" : id.Trim();
            string parentNowId = string.Empty;
            object vOutActionArgs = new object();
            View.DidlLite.DIDLLite objDIDL;
            do {
                object[] inSearchParams = new object[6] {
                    nowId,
                    "BrowseDirectChildren",
                    "*",
                    startIndex,
                    10,
                    string.Empty
                };
                objDIDL = new DIDLLite { Container = null, Item = null };

                try {
                    string dummyquery = "?"+DateTime.Now.Ticks.ToString();
                    objResponseXml = uPnpAccessManager.GetInstance().DeserializeEnvelope(
                                        WebAccessManager.GetBrowseList(sevUrl, inSearchParams));
                    if ( objResponseXml != null && objResponseXml.Body != null
                        && objResponseXml.Body.BrowseResponse != null ) {
                        totalcount = objResponseXml.Body.BrowseResponse.TotalMatches;
                        objDIDL = DeserializeDIDL(objResponseXml.Body.BrowseResponse.Result);
                    }
                } catch ( Exception ) {
                    objDIDL = new DIDLLite { Container = null, Item = null };
                } finally {
                }
                if ( objDIDL.Container == null && objDIDL.Item == null && startIndex == 0 ) {
                    return false;
                }
                if ( objDIDL.Container != null ) {
                    foreach ( DIDLContainer container in objDIDL.Container ) {
                        //
                        isVideotuner = UpnpCheckVideoBroadcast(deviceName, sevUrl,
                                                               true, container.id, nestCounter++);
                        if ( isVideotuner != false ) return isVideotuner;
                    }
                }
                if ( objDIDL.Item != null ) {
                    count = objDIDL.Item.Length;
                    itemtotalcount = totalcount;
                    foreach ( DIDLItem item in objDIDL.Item ){
                        if ( item.cLass.StartsWith("object.item.videoItem.videoBroadcast") ) {
                            upnpFinder.AddEventsDriveEvent();
                            upnpFinder.OnDriveRaiseEvent(new UpnpDriveInfoEventArgs(deviceName, EventStatus.ON_LIVETUNER));
                            upnpFinder.DeleteEventsDriveEvent();
                            return true;
                        } else {
                            return false;
                        }
                    }
                } else {
                    return false;
                }
                startIndex += count;
                if ( this.IsSearchCanceled ) {
                    this.IsSearchCanceled = false;
                    break;
                }
            } while ( totalcount > startIndex );
            this.Allcount += itemtotalcount;
            return isVideotuner;
        }
    }
}
