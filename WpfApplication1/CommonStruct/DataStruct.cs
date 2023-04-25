using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using View.CommonEnum;

namespace View.CommonStruct
{
    public class DataStruct {

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TvRecMgr {
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x17)]
            public Byte[] a;
            public Byte sizeOflabel;
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x60)]
            public Byte[] title;

            /** コンストラクタ */
            public TvRecMgr(bool iniitialize) {
                a = new Byte[0x17];
                sizeOflabel = 0;
                title = new Byte[0x60];
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct UdffInfTbl {
            public Byte   id;                                           ///< +00
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x5)]
            public Byte[] time;
            public UInt32 mark;
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x78)]
            public Byte[] title;
            public UInt16 numOfFolder;      ///< フォルダ数(big endian)
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x00)]
            public UInt16[] folderNumber;
            /** コンストラクタ */
            public UdffInfTbl(bool iniitialize) {
                id = 0x48;
                time = new Byte[0x05];
                mark = 0;
                title = new Byte[0x78];
                numOfFolder = 0;
                folderNumber = new UInt16[0];
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct OpgrInfTbl {
            public UInt16 status;                                       ///< +0
            public Byte   id;                                           ///< +02
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x5)]
            public Byte[] mjdStartDateTime;                             ///< +03
            public UInt32 mark;                                         ///< +08
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x60)] 
            public Byte[] title;                                        ///< +0C
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x2)] 
            public Byte[] numberPdb;
            public UInt32 numberFlag;
            public UInt16 primarykey;
            public UInt16 b; ///< 不明
            public UInt32 c;
            public UInt32 d; ///<padding?
            public UInt32 e;
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0xf)]
            public Byte[] pudding;
            public Byte   mode;
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 0x8)]
            public Byte[] pudding2;
            public UInt32 recTime;


            /** コンストラクタ */
            public OpgrInfTbl(bool iniitialize) {
                mjdStartDateTime = new Byte[5];
                status = 0;
                id = 0x48;
                mark = 0;
                title = new Byte[0x60];
                numberPdb = new Byte[2];
                numberFlag = 0;
                primarykey = 0;
                b = 0;
                c = 0;
                d = 0;
                e = 0;
                this.pudding = new Byte[0xf];
                mode = 0;
                this.pudding2 = new Byte[0x8];
                recTime = 0;
                primarykey = 0;
            }
        }

        public struct IvdrFileList {
            /// <summary>
            /// ファイル名
            /// </summary>
            public string fileName;
            /// <summary>
            /// タイトル
            /// </summary>
            public string title;
            /// <summary>
            /// 作成日時
            /// </summary>
            public DateTime recordDateTime;
            /// <summary>
            /// 録画時間
            /// </summary>
            public TimeSpan durationTime;
            /// <summary>
            /// 録画モード
            /// </summary>
            public RecMode recordMode;
            /// <summary>
            /// ステータス
            /// </summary>
            public EventStatus status;
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="title"></param>
            /// <param name="recordDateTime"></param>
            /// <param name="durationTime"></param>
            /// <param name="recordMode"></param>
            public IvdrFileList(string fileName, string title,
                                DateTime recordDateTime, TimeSpan durationTime,
                                RecMode recordMode, EventStatus status) {
                this.fileName = fileName;
                this.title = title;
                this.recordDateTime = recordDateTime;
                this.durationTime = durationTime;
                this.recordMode = recordMode;
                this.status = status;
            }
        }

        public struct Folder {
            public string folder;
            public UInt16 fileNumber;
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="folder"></param>
            /// <param name="fileNumber"></param>
            public Folder(string folder, UInt16 fileNumber) {
                this.folder = folder;
                this.fileNumber = fileNumber;
            }
        }

        public struct UpnpDevice {
            /// <summary>
            /// メーカ名
            /// </summary>
            public string manufacturerName;
            /// <summary>
            /// モデル名
            /// </summary>
            public string modelName;
            /// <summary>
            /// モデル番号
            /// </summary>
            public string modelNumber;
        }


        public struct IvdrxSize {
            public string name;
            private int    ParSize;
            private double FreeSize;
            private double TotalSize;

            public double freesize {
                set {
                    this.FreeSize = freesize;
                    this.ParSize = (this.TotalSize == 0) ? 0 : (int)((this.FreeSize / this.TotalSize) * 100);
                }
                get {
                    return this.FreeSize;
                }
            }
            public double totalsize {
                set {
                    this.TotalSize = totalsize;
                    this.ParSize = (this.TotalSize == 0) ? 0 : (int)((this.FreeSize / this.TotalSize) * 100);
                }
                get {
                    return this.TotalSize;
                }
            }
            public int parsize {
                get {
                    return this.ParSize;
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="name"></param>
            /// <param name="freesize"></param>
            /// <param name="totalsize"></param>
            public IvdrxSize(string name, double freesize, double totalsize) {
                this.name = name;
                this.FreeSize = freesize;
                this.TotalSize = totalsize;
                this.ParSize = (totalsize == 0) ? 0 : (int)((freesize / totalsize) * 100);
            }
        }

        public struct Drivemap {
            public string drive;
            public string fileSystem;
            public string dlnaImage;
            public string modelName;
            public string manufacturerName;
            public string modelNumber;
            public X_Record scheduledRecording;
            private string Location;
            public DlnaSupport dlnaSupport;
            private string PresentationURL;
            private string IvdrControlUrl;
            private string CdirControlUrl;

            public string location{
                set {
                    this.Location = location; 
                }
                get {
                    return (this.Location == null ? string.Empty : this.Location);
                }
            }
            public string presentationURL{
                set {
                    this.PresentationURL = presentationURL; 
                }
                get {
                    return (this.PresentationURL == null ? string.Empty : this.PresentationURL);
                }
            }
            public string ivdrControlUrl {
                set {
                    this.IvdrControlUrl = ivdrControlUrl; 
                }
                get {
                    return (this.IvdrControlUrl == null ? string.Empty : this.IvdrControlUrl);
                }
            }
            public string cdirControlUrl {
                set {
                    this.CdirControlUrl = cdirControlUrl; 
                }
                get {
                    return (this.CdirControlUrl == null ? string.Empty : this.CdirControlUrl);
                }
            }
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="drive"></param>
            /// <param name="fileSystem"></param>
            public Drivemap(string drive, string fileSystem) {
                this.drive           = drive;
                this.fileSystem      = fileSystem;
                this.dlnaImage       = string.Empty;
                this.modelName       = string.Empty;
                this.manufacturerName = string.Empty;
                this.modelNumber      = string.Empty;
                this.PresentationURL  = string.Empty;
                this.scheduledRecording   = X_Record.NONE;
                this.dlnaSupport      = DlnaSupport.NONE;
                this.Location         = string.Empty;
                this.IvdrControlUrl   = string.Empty;
                this.CdirControlUrl   = string.Empty;
            }
            public Drivemap(string drive, string fileSystem,
                            string manufacturerName, string modelName,
                            string modelNumber) {
                this.drive           = drive;
                this.fileSystem      = fileSystem;
                this.dlnaImage       = string.Empty;
                this.modelName       = modelName;
                this.manufacturerName = manufacturerName;
                this.modelNumber      = modelNumber;
                this.PresentationURL  = string.Empty;
                this.scheduledRecording   = X_Record.NONE;
                this.dlnaSupport      = DlnaSupport.NONE;
                this.Location         = string.Empty;
                this.IvdrControlUrl   = string.Empty;
                this.CdirControlUrl   = string.Empty;
            }
            public Drivemap(string drive, string fileSystem, string dlnaUrl, 
                            string modelName, string manufacturerName,
                            string modelNumber, string presentationURL,
                            X_Record scheduledRecording, string location,
                            DlnaSupport dlnaSupport, string ivdrControlUrl,
                            string cdirControlUrl ) {
                this.drive            = drive;
                this.fileSystem       = fileSystem;
                this.dlnaImage        = (dlnaUrl == null ? string.Empty : dlnaUrl);
                this.modelName        = (modelName == null ? string.Empty : modelName);
                this.manufacturerName = (manufacturerName == null ? string.Empty : manufacturerName);
                this.modelNumber      = (modelNumber == null ? string.Empty : modelNumber);
                this.scheduledRecording   = scheduledRecording;
                this.dlnaSupport      = dlnaSupport;
                this.Location         = location;
                this.PresentationURL  = presentationURL;
                this.IvdrControlUrl   = ivdrControlUrl;
                this.CdirControlUrl   = cdirControlUrl;
            }
        }
    }
}
