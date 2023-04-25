using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace View.DidlLite {
    // soap
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope {
        [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public SoapBody Body { get; set; }
    }

    public class SoapBody {
        [XmlElement(Namespace = "urn:schemas-digion-com:service:X_Ivdr:1")]
        public DriveInfoResponse GetDriveInfoResponse { get; set; }

        [XmlElement(Namespace = "urn:schemas-upnp-org:service:ContentDirectory:1")]
        public GetBrowseResponse BrowseResponse { get; set; }

        [XmlElement(Namespace = "urn:schemas-upnp-org:service:ContentDirectory:1")]
        public GetRecordInfoResponse X_HDLnkGetRecordDestinationInfoResponse { get; set; }

        [XmlElement(Namespace = "urn:schemas-upnp-org:service:ContentDirectory:1")]
        public GetRecordsResponse X_HDLnkGetRecordDestinationsResponse { get; set; }
    }

    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class GetBrowseResponse {
        [XmlElement("Result")]
        public string Result { get; set; }
        [XmlElement("NumberReturned")]
        public double NumberReturned { get; set; }
        [XmlElement("TotalMatches")]
        public int TotalMatches { get; set; }
        [XmlElement("UpdateID")]
        public double UpdateID { get; set; }
    }

    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class GetRecordInfoResponse {
        [XmlElement("RecordDestinationInfo")]
        public string RecordDestinationInfo { get; set; }
    }

    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class GetRecordsResponse {
        [XmlElement("RecordDestinationList")]
        public string RecordDestinationList { get; set; }
    }

    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class DriveInfoResponse {
        [XmlElement("IvdrDriveLetter")]
        public string IvdrDriveLetter { get; set; }
        [XmlElement("IvdrDriveType")]
        public string IvdrDriveType { get; set; }
        [XmlElement("TotalSize")]
        public double TotalSize { get; set; }
        [XmlElement("FreeSize")]
        public double FreeSize { get; set; }
    }

    //DIDL-Lite XmlParse
    [XmlRoot("RecordDestinations", Namespace = "urn:schemas-hdlnk-org")]
    public class RecordDestinations {
        [XmlElement("RecordDestination")]
        public RecordDestination[] RecordDestination { get; set; }
    }

    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class RecordDestination {
        [XmlAttribute("destID")]
        public string destID { get; set; }
        [XmlText]
        public string types { get; set; }
    }

    //DIDL-Lite XmlParse
    [XmlRoot("RecordDestinationInfo", Namespace = "urn:schemas-hdlnk-org")]
    public class RecordDestinationInfo {
        [XmlAttribute("totalCapacity")]
        public double TotalSize { get; set; }
        [XmlAttribute("availableCapacity")]
        public double FreeSize { get; set; }
        [XmlText]
        public string types { get; set; }
    }

    //DIDL-Lite XmlParse
    [XmlRoot("DIDL-Lite", Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/")]
    public class DIDLLite {
        [XmlElement("item")]
        public DIDLItem[] Item { get; set; }
        [XmlElement("container")]
        public DIDLContainer[] Container { get; set; }
    }

    public class DIDLContainer {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("parentID")]
        public string parentID { get; set; }

        [XmlAttribute("restricted")]
        public string restricted { get; set; }

        [XmlAttribute("childCount")]
        public string childCount { get; set; }

        [XmlElement(Namespace = "http://purl.org/dc/elements/1.1/")]
        public string title { get; set; }
        [XmlElement(Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string cLass { get; set; }
    }

    public class DIDLItem {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("parentID")]
        public string parentID { get; set; }

        [XmlAttribute("restricted")]
        public string restricted { get; set; }

        [XmlElement(Namespace = "http://purl.org/dc/elements/1.1/")]
        public string creator { get; set; }
        [XmlElement(Namespace = "http://purl.org/dc/elements/1.1/")]
        public string date { get; set; }
        [XmlElement(Namespace = "http://purl.org/dc/elements/1.1/")]
        public string title { get; set; }
        [XmlElement(Namespace = "http://purl.org/dc/elements/1.1/")]
        public string description { get; set; }
        [XmlElement(Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string cLass { get; set; }
        [XmlElement(Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string longDescription { get; set; }
        [XmlElement(Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string[] genre { get; set; }
        [XmlElement(Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string channelNr { get; set; }
        [XmlElement(Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string channelName { get; set; }
        [XmlElement(Namespace = "urn:schemas-arib-or-jp:elements-1-0/")]
        public string objectType { get; set; }

        [XmlElement("res")]
        public DIDLRes[] Res { get; set; }
    }

    public class DIDLRes {
        [XmlText]
        public string res { get; set; }

        [XmlAttribute("protocolInfo")]
        public string protocolInfo { get; set; }

        [XmlAttribute("duration")]
        public string duration { get; set; }

        [XmlAttribute("size")]
        public string size { get; set; }

        [XmlAttribute("resolution")]
        public string resolution { get; set; }
    }

}