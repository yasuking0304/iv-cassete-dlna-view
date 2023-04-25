using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using View.CommonEnum;

namespace View.upnpLocation {
    //
    [XmlRoot("root", Namespace = "urn:schemas-upnp-org:device-1-0")]
    public class UPnPLocation {
        public string URLBase { get; set; }

        [XmlElement("device")]
        public DeviceItem Device { get; set; }
    }

    public class DeviceItem {
        public string UDN { get; set; }

        public string friendlyName { get; set; }

        public string manufacturer { get; set; }

        public string modelName { get; set; }

        public string modelNumber { get; set; }

        public string serialNumber { get; set; }

        public string modelURL { get; set; }

        public string presentationURL { get; set; }

        [XmlElement(Namespace = "urn:schemas-dlna-org:device-1-0")]
        public string X_DLNACAP { get; set; }

        [XmlElement(Namespace = "urn:schemas-skyperfectv-co-jp:device-1-0")]
        public string X_SPTVCAP { get; set; }

        [XmlElement(Namespace = "urn:schemas-jlabs-or-jp:device-1-0")]
        public string X_JLABSCAP { get; set; }

        [XmlElement(Namespace = "urn:schemas-toshiba-co-jp:movemetadata-1-0/")]
        public string X_move_support { get; set; }

        [XmlElement(Namespace = "urn:schemas-panasonic-com:vli")]
        public XIpplTVList X_IPPLTV_LIST { get; set; }

        public class XIpplTVList {
            public string X_IPPLTV_VERSION { get; set; }
        }

        [XmlElement("iconList")]
        public IconList IconList { get; set; }

        [XmlElement("serviceList")]
        public ServiceList ServiceList { get; set; }

        /// <summary>
        /// Ivdrサービス実装の確認
        /// </summary>
        public bool IsXivdr {
            get {
                bool retValue = false;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-digion-com:service:X_Ivdr:")) { 
                        retValue = true;
                        break;
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GetXIvdrControlURL {
            get {
                string retValue = string.Empty;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-digion-com:service:X_Ivdr:")) { 
                        retValue = ServiceList.Service[i].controlURL;
                        break;
                    }
                }
                return retValue;
            }
        }

        public string GetContentDirectoryControlURL {
            get {
                string retValue = string.Empty;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-upnp-org:service:ContentDirectory:")) { 
                        retValue = ServiceList.Service[i].controlURL;
                        break;
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// SCHEDULEDRECORDINGサービス実装の確認
        /// </summary>
        public bool IsScheduledRecording {
            get {
                bool retValue = false;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-upnp-org:service:ScheduledRecording:1")) { 
                        retValue = true;
                        break;
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// SCHEDULEDRECORDING2サービス実装の確認
        /// </summary>
        public bool IsScheduledRecording2 {
            get {
                bool retValue = false;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-upnp-org:service:ScheduledRecording:2")) { 
                        retValue = true;
                        break;
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// X_SCHEDULEDRECORDINGサービス実装の確認
        /// </summary>
        public bool IsXscheduledRecording {
            get {
                bool retValue = false;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-xsrs-org:service:X_ScheduledRecording:")) { 
                        retValue = true;
                        break;
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// X_SCHEDULEDRECORDINGEXTサービス実装の確認
        /// </summary>
        public bool IsXscheduledRecordingExt {
            get {
                bool retValue = false;
                for ( int i=0; i< ServiceList.Service.Length ; i++ ) {
                    if (ServiceList.Service[i].serviceType.StartsWith("urn:schemas-xsrs-org:service:X_ScheduledRecordingExt:")) { 
                        retValue = true;
                        break;
                    }
                }
                return retValue;
            }
        }

        public string GetIcoURL() {
            string retValue = GetIconURL("image/png", 120, 120, 24);

            if ( retValue.Equals(string.Empty) ) {
                retValue = GetIconURL("image/jpeg", 120, 120, 24);
            }
            if ( retValue.Equals(string.Empty) ) {
                retValue = GetIconURL("image/png", 48, 48, 24);
            }
            if ( retValue.Equals(string.Empty) ) {
                retValue = GetIconURL("image/jpeg", 48, 48, 24);
            }
            return retValue;
        }

        public string GetIconURL(string mimetype, Int32 width, Int32 height, Int32 depth)  {
            string retValue = string.Empty;
            if ( IconList != null ) {
                foreach (Icon ico in IconList.Icon) {
                    if (   ico.mimetype.Equals(mimetype) && ico.width == width
                        && ico.height == height && ico.depth == depth ) {
                        retValue = ico.url;
                        break;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// DLNAサービス群フラグ
        /// </summary>
        public DlnaSupport GetDlnaSupport {
            get {
                DlnaSupport retValue = DlnaSupport.NONE;
                retValue |= ( IsXdlnacapAvUpload == true   ) ? DlnaSupport.DLNACAPAVUPLOAD : 0;
                retValue |= ( IsXdlnacapDtcpMove == true   ) ? DlnaSupport.DLNACAPDTCPMOVE : 0;
                retValue |= ( IsXdlnacapDtcpCopy == true   ) ? DlnaSupport.DLNACAPDTCPCOPY : 0;
                retValue |= ( IsXjlabscapUploadRec == true ) ? DlnaSupport.JLABSCAPUPLOADREC : 0;
                retValue |= ( IsXjlabscapMove == true      ) ? DlnaSupport.JLABSCAPMOVE : 0;
                retValue |= ( IsXsptvcapRec == true        ) ? DlnaSupport.SPTVCAPREC : 0;
                retValue |= ( IsXsptvcapMove == true       ) ? DlnaSupport.SPTVCAPMOVE : 0;
                retValue |= ( IsRegzacapMove == true       ) ? DlnaSupport.REGZACAPMOVE : 0;
                retValue |= ( IsLiveTunerPanasonic == true ) ? DlnaSupport.LIVETUNER | DlnaSupport.CHKLIVETUNER : 0;
                return retValue;
            }
        }

        /// <summary>
        /// DLNAサービス実装の確認(ダビング受け型ムーブイン)
        /// </summary>
        public bool IsXdlnacapAvUpload {
            get {
                bool retValue = false;
                if ( X_DLNACAP != null ) {
                    string[] tmpString = X_DLNACAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("av-upload")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// DLNAサービス実装の確認(ダウンロード型ムーブアウト)
        /// </summary>
        public bool IsXdlnacapDtcpMove {
            get {
                bool retValue = false;
                if ( X_DLNACAP != null ) {
                    string[] tmpString = X_DLNACAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("dtcp-move")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// DLNAサービス実装の確認(ダウンロード型コピーアウト)
        /// </summary>
        public bool IsXdlnacapDtcpCopy {
            get {
                bool retValue = false;
                if ( X_DLNACAP != null ) {
                    string[] tmpString = X_DLNACAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("dtcp-copy")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// JLABSサービス実装の確認(CATV ダビングアウト)
        /// </summary>
        public bool IsXjlabscapMove {
            get {
                bool retValue = false;
                if ( X_JLABSCAP != null ) {
                    string[] tmpString = X_JLABSCAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("upload_dub")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// JLABSサービス実装の確認(CATV LAN録画)
        /// </summary>
        public bool IsXjlabscapUploadRec {
            get {
                bool retValue = false;
                if ( X_JLABSCAP != null ) {
                    string[] tmpString = X_JLABSCAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("upload_rec")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// SPTVサービス実装の確認(スカパーLAN録画)
        /// </summary>
        public bool IsXsptvcapRec {
            get {
                bool retValue = false;
                if ( X_SPTVCAP != null ) {
                    string[] tmpString = X_SPTVCAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("rec-")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }
        /// <summary>
        /// SPTVサービス実装の確認(スカパー ムーブアウト)
        /// </summary>
        public bool IsXsptvcapMove {
            get {
                bool retValue = false;
                if ( X_SPTVCAP != null ) {
                    string[] tmpString = X_SPTVCAP.ToLower().Split(',');
                    for ( int i=0; i< tmpString.Length ; i++ ) {
                        if (tmpString[i].StartsWith("move-")) { 
                            retValue = true;
                            break;
                        }
                    }
                }
                return retValue;
            }
        }
        /// <summary>
        /// ライブチューナ(panasonic方式)サービス実装の確認
        /// </summary>
        public bool IsLiveTunerPanasonic {
            get {
                bool retValue = false;
                if ( X_IPPLTV_LIST != null ) {
                    if (X_IPPLTV_LIST.X_IPPLTV_VERSION != null ) { 
                        retValue = true;
                    }
                }
                return retValue;
            }
        }
        /// <summary>
        /// RegzaTVサービス実装の確認
        /// </summary>
        public bool IsRegzacapMove {
            get {
                bool retValue = false;
                if ( X_move_support != null ) {
                    retValue = true;
                }
                return retValue;
            }
        }
    }

    public class IconList {
        [XmlElement("icon")]
        public Icon[] Icon { get; set; }
    }

    public class Icon {
        public string mimetype { get; set; }

        public Int32 width { get; set; }
        
        public Int32 height { get; set; }
        
        public Int32 depth { get; set; }
        
        public string url { get; set; }
    }

    public class ServiceList {
        [XmlElement("service")]
        public Service[] Service { get; set; }
    }

    public class Service {
        public string serviceType { get; set; }

        public string SCPDURL { get; set; }

        public string controlURL { get; set; }

        public string eventSubURL { get; set; }

    }
}
