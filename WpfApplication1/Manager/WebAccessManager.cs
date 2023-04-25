using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.Cache;

namespace View.Manager {
    class WebAccessManager {
        // インスタンス生成
        private static uPnpAccessManager instance = null;

        public static uPnpAccessManager GetInstance() {
            if (instance == null) {
                instance = new uPnpAccessManager();
            }
            return instance;
        }
        private static string XML_ENVELOPE_HEAD =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>"  +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "  <s:Body>"
        ;
        private static string XML_ENVELOPE_FOOT =
            "  </s:Body>"  +
            "</s:Envelope>"
        ;

        private static string XML_BROWSER =
            XML_ENVELOPE_HEAD +
            "    <u:Browse xmlns:u=\"urn:schemas-upnp-org:service:ContentDirectory:1\">" +
            "      <ObjectID>{0}</ObjectID>" +
            "      <BrowseFlag>{1}</BrowseFlag>" +
            "      <Filter>{2}</Filter>" +
            "      <StartingIndex>{3}</StartingIndex>" +
            "      <RequestedCount>{4}</RequestedCount>" +
            "      <SortCriteria>{5}</SortCriteria>" +
            "    </u:Browse>" +
            XML_ENVELOPE_FOOT
        ;
        private static string XML_DRIVEINFO =
            XML_ENVELOPE_HEAD +
            "    <u:GetDriveInfo xmlns:u=\"urn:schemas-digion-com:service:X_Ivdr:1\" />" +
            XML_ENVELOPE_FOOT
        ;

        private static string XML_DRIVEINFO2 =
            XML_ENVELOPE_HEAD +
            "    <u:GetDriveInfo2 xmlns:u=\"urn:schemas-digion-com:service:X_Ivdr:1\">" +
            "      <DriveID>{0}</DriveID>" +
            "    </u:GetDriveInfo2>" +
            XML_ENVELOPE_FOOT
        ;
        private static string XML_HDLNKRECLIST =
            XML_ENVELOPE_HEAD +
            "    <u:X_HDLnkGetRecordDestinations xmlns:u=\"urn:schemas-upnp-org:service:ContentDirectory:1\"/>" +
            XML_ENVELOPE_FOOT
        ;
        private static string XML_HDLNKRECINFO =
            XML_ENVELOPE_HEAD +
            "    <u:X_HDLnkGetRecordDestinationInfo xmlns:u=\"urn:schemas-upnp-org:service:ContentDirectory:1\">" +
            "      <RecordDestinationID>{0}</RecordDestinationID>" +
            "    </u:X_HDLnkGetRecordDestinationInfo>" +
            XML_ENVELOPE_FOOT
        ;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static public string GetWebSource(string url) {
            string source = string.Empty;
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse webres = (HttpWebResponse)webreq.GetResponse();
            Stream st = webres.GetResponseStream();
            StreamReader sr = new StreamReader(st, Encoding.UTF8);
            source = sr.ReadToEnd();
            //閉じる
            sr.Close();
            st.Close();
            webres.Close();
        return source;
        }

        /// <summary>
        /// URLからIPアドレス取得
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static public string GetIPAddress(string url) {
            string ipaddr = string.Empty;
            string host   = Regex.Replace(url, "^.+?//", "" );
            host   = Regex.Replace(host,"/.+","");
            host   = Regex.Replace(host,":.+","");
            IPAddress[] adList = new IPAddress[] {};
            try {
                adList = Dns.GetHostAddresses(host);
            } finally {
                if ( adList.Length > 0 ) {
                    ipaddr = adList[0].ToString();
                }
            }
            return ipaddr;
        }

        /// <summary>
        /// URL合成
        /// </summary>
        /// <param name="url1"></param>
        /// <param name="url2"></param>
        /// <returns></returns>
        static public string GetAddUrl(string url1, string url2) {
            if (url2.Equals(string.Empty)) {
                return string.Empty;
            }
            string[] split = url2.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            string loadvalue = url1;
            string retvalue = string.Empty;
            if ( url2 != null ) {
                retvalue = url2;
                if ( retvalue.Substring(0,1).Equals("/") ) {
                    retvalue = url2.Substring(1);
                    string[] split1 = loadvalue.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                    loadvalue = split1[0] + "//" + split1[1] + "/"; 
                }
            }
//            if ( url1.IndexOf(split[0]) > 0) {
            if ( loadvalue.IndexOf(split[0]) > 0) {
                retvalue = loadvalue.Substring(0, loadvalue.IndexOf(split[0]) -1 )  + retvalue;

            } else if ( !url2.ToLower().StartsWith("http") ) {
                retvalue = loadvalue + retvalue;
            }
            return retvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="service"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        static private string GetParam(string url, string service, string method, string data) {
            string resulXmlFromWebService = string.Empty;
            if ( !url.Equals( string.Empty )) {
                try {
                    WebRequest webRequest = WebRequest.Create(url);
                    HttpWebRequest httpRequest = (HttpWebRequest)webRequest;
                    httpRequest.Method = "POST"; 
                    httpRequest.ContentType = "text/xml; charset=\"utf-8\"";
                    httpRequest.Headers.Add("SOAPAction: \""+ service + "#" + method + "\"");
                    httpRequest.ProtocolVersion = HttpVersion.Version11;
                    httpRequest.Accept = "text/xml";
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;
                    HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    httpRequest.CachePolicy = noCachePolicy;

                    Stream requestStream = httpRequest.GetRequestStream();          
                    StreamWriter streamWriter = new StreamWriter(requestStream, Encoding.ASCII);

                    StringBuilder soapRequest = new StringBuilder(data);

                    streamWriter.Write(soapRequest.ToString());             
                    streamWriter.Close();            
                    //Get the Response
                    WebResponse webResponse = httpRequest.GetResponse();
                    System.Threading.Thread.Sleep(30);
                    StreamReader srd = new StreamReader(webResponse.GetResponseStream(),new UTF8Encoding(false));
                    resulXmlFromWebService = srd.ReadToEnd();
                    //受信したデータを表示する
                    //Console.WriteLine(resulXmlFromWebService);
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            return resulXmlFromWebService;
        }


        static public string GetDataIvdr_GetDriveInfo(string url) {
            return GetParam(url, "urn:schemas-digion-com:service:X_Ivdr:1",
                            "GetDriveInfo",
                            XML_DRIVEINFO);
        }

        static public string GetDataIvdr_GetDriveInfo2(string url, int driveNumber) {
            return GetParam(url, "urn:schemas-digion-com:service:X_Ivdr:1",
                            "GetDriveInfo2",
                            string.Format(XML_DRIVEINFO2, driveNumber));
        }

        static public string GetXRecordDestinationInfo(string url, string device) {
            return GetParam(url, "urn:schemas-upnp-org:service:ContentDirectory:1",
                            "X_HDLnkGetRecordDestinationInfo",
                            string.Format(XML_HDLNKRECINFO, device));
        }

        static public string GetXRecordDestinations(string url) {
            return GetParam(url, "urn:schemas-upnp-org:service:ContentDirectory:1",
                            "X_HDLnkGetRecordDestinations",
                            XML_HDLNKRECLIST);
        }

        static public string GetBrowseList(string url, object[] data) {
            return GetParam(url, "urn:schemas-upnp-org:service:ContentDirectory:1",
                            "Browse",
                            string.Format(XML_BROWSER, data));
        }
        /// <summary>
        ///  URLイメージ画像取得
        /// </summary>
        /// <param name="url">URI</param>
        /// <returns></returns>
        static public ImageSource LoadImageFromURL( string url ) {
            if ( url == null || url.Trim().Length <= 0 ) {
                return null;
            }
            MemoryStream imgStream = new MemoryStream();
            ImageSource bmp = null;
            try {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                int buffSize = 65536; // 一度に読み込むサイズ
                WebRequest req = WebRequest.Create( url );
                BinaryReader reader = new BinaryReader( req.GetResponse().GetResponseStream() );
                try {
                     while ( true ) {
                        byte[] buff = new byte[ buffSize ];
                        int readBytes = reader.Read( buff, 0, buffSize );
                        if ( readBytes <= 0 ) {
                            break;
                        }
                        // バッファに追加
                        imgStream.Write( buff, 0, readBytes );
                    }
                } catch {
                    ;
                } finally {
                    reader.Close();
                }
                imgStream.Position = 0;
                BitmapDecoder decoder = BitmapDecoder.Create(
                                            imgStream,
                                            BitmapCreateOptions.PreservePixelFormat,
                                            BitmapCacheOption.OnDemand);

                bmp = new WriteableBitmap(decoder.Frames[0]);
                bmp.Freeze();
            } catch {
                ;
            } finally {
                imgStream.Close();
            }
            return bmp;
        }

        static public string GetServicesFromDevice(out int _port) {
            string responseString = string.Empty;
            _port = 0;
            try {
                IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, 1900);
                IPEndPoint MulticastEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

                Socket UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                UdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                UdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1500);
                UdpSocket.Bind(LocalEndPoint);
                UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                                            new MulticastOption(MulticastEndPoint.Address, IPAddress.Any));
                UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

                string requestString = "M-SEARCH * HTTP/1.1\r\n" +
                                       "HOST: 239.255.255.250:1900\r\n" +
                                       "ST: urn:schemas-upnp-org:service:ContentDirectory\r\n" +
                                       "MAN: \"ssdp:discover\"\r\n" +
                                       "MX: 1\r\n" +
                                       "\r\n";
                UdpSocket.SendTo(Encoding.UTF8.GetBytes(requestString), SocketFlags.None, MulticastEndPoint);

                byte[] ReceiveBuffer = new byte[64000];
                int ReceivedBytes = 0;
                while (true) {
                    if (UdpSocket.Available > 0){
                        ReceivedBytes = UdpSocket.Receive(ReceiveBuffer, SocketFlags.None);

                        if (ReceivedBytes > 0) {
                            Console.WriteLine(Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes));
                        }
                    }
                }
//                ReceivedBytes = UdpSocket.Receive(ReceiveBuffer, SocketFlags.None);
//                responseString = Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes);
            }

            catch (Exception ex) {
                Console.WriteLine("エラー発生！");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            if (responseString.Length == 0) {
                return string.Empty;
            }

            string location = string.Empty;
            string[] parts = responseString.Split(new string[] { System.Environment.NewLine },
                                                               StringSplitOptions.RemoveEmptyEntries
                                                 );

            foreach (string part in parts) {
                if (part.ToLower().StartsWith("location"))  {
                    location = part.Substring(part.IndexOf(':') + 1);
                    string port = location.Substring(location.LastIndexOf(':') + 1);
                    _port = int.Parse(port.Substring(0, port.IndexOf('/')));
                    break;
                }
            }
            if (location.Length == 0) {
                return string.Empty;
            }

            using (WebClient webClient = new WebClient())  {
                return webClient.DownloadString(location);
            }
        }
    }
}
