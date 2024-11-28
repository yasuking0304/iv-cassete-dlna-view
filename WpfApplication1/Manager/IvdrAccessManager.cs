using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using View.CommonEnum;
using View.CommonStruct;
using View.EventArgs;
using View.DataConverter.EndianConverter;
using View.DataConverter.StringConverter;
using View.DataConverter.LanguageConverter;
using View.Delegate;

namespace View.Manager {

    internal class IvdrAccessManager {

        // インスタンス生成
        private static IvdrAccessManager instance = null;

        public static IvdrAccessManager GetInstance() {
            if (instance == null) {
                instance = new IvdrAccessManager();
            }
            return instance;
        }
        IvdrEventFinder ivdrFinder = new IvdrEventFinder();

        /// <summary>
        /// ラベル名取得
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        public string GetIvdrLabel(string drive) {
            ivdrFinder.AddEventsLabelEvent();
            string path = drive + (Application.Current.TryFindResource("TVREC_MGR") as string);
            CommonStruct.DataStruct.TvRecMgr tvRecMgr = new CommonStruct.DataStruct.TvRecMgr(true);
            LabelInfoEventArgs lblargs = new LabelInfoEventArgs(true);
            BinaryReader fs = null;
            try {
                fs = new BinaryReader(
                           File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                tvRecMgr.a = fs.ReadBytes(tvRecMgr.a.Length);
                tvRecMgr.sizeOflabel = fs.ReadByte();
                tvRecMgr.title = fs.ReadBytes(tvRecMgr.title.Length);
                // イベント通知
                lblargs = new LabelInfoEventArgs(tvRecMgr);
                ivdrFinder.OnLabelRaiseEvent(lblargs);
            
            } catch (DirectoryNotFoundException) {
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }
                tvRecMgr.sizeOflabel = 0;
            
            } catch (Exception e) {
                fs.Close();
                fs = null;
            
            } finally {
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }
            }
            ivdrFinder.DeleteEventsLabelEvent();
            return lblargs.GetTitle();
        }

        /// <summary>
        /// ivdrフォルダ一覧取得
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        public List<DataStruct.Folder> GetIvdrFolderList(string drive) {
            ivdrFinder.AddEventsFolderEvent();
            List<DataStruct.Folder> value = new List<DataStruct.Folder> {};
            string path = drive + (Application.Current.TryFindResource("UDFF_INF_TBL") as string);
            CommonStruct.DataStruct.UdffInfTbl UdffInfTbl = new CommonStruct.DataStruct.UdffInfTbl(true);
            BinaryReader fs = null;
            try {
                fs = new BinaryReader(
                           File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                // イベント通知(ivdrのフォルダ専用)
                FolderInfoEventArgs folderArgs = new FolderInfoEventArgs(EventStatus.ONSTART);
                ivdrFinder.OnFolderRaiseEvent(folderArgs);
                for (; ; ) {
                    try {
                        UInt16 num = 0;
                        UdffInfTbl.id = fs.ReadByte();
                        UdffInfTbl.time = fs.ReadBytes(UdffInfTbl.time.Length);
                        UdffInfTbl.mark = fs.ReadUInt32();
                        UdffInfTbl.title = fs.ReadBytes(UdffInfTbl.title.Length);
                        UdffInfTbl.numOfFolder = fs.ReadUInt16();
                        // イベント通知
                        folderArgs = new FolderInfoEventArgs(UdffInfTbl);
                        ivdrFinder.OnFolderRaiseEvent(folderArgs);
                        // リスト追加
                        string folderTitle = folderArgs.GetTitle();
                        for (uint i = 0; i < folderArgs.GetFolderNumber(); i++) {
                            folderArgs.folderCode[i] = fs.ReadUInt16();
                            num = (UInt16)folderArgs.GetFolderCode(i);
                            value.Add(new DataStruct.Folder(folderTitle, num));
                        }
                    }
                    catch (System.IO.EndOfStreamException) {
                        break;
                    }
                }
            } catch (DirectoryNotFoundException) {
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }
            } catch (UnauthorizedAccessException) {
                fs.Close();
                fs = null;
            } catch (Exception e) {
                fs.Close();
                fs = null;
            } finally {
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }
            }
            // 完了イベント通知後、イベント通知解除
            ivdrFinder.OnFolderRaiseEvent(new FolderInfoEventArgs(EventStatus.ONEND));
            ivdrFinder.DeleteEventsFolderEvent();
            return value;
        }
        public void GetIvdrList(string drive) {
            // ファイル情報取得
            ivdrFinder.AddEventsFileEvent();
            System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            string path = drive + (Application.Current.TryFindResource("OPGR_INF_TBL") as string);
            CommonStruct.DataStruct.OpgrInfTbl b = new CommonStruct.DataStruct.OpgrInfTbl(true);
            BinaryReader fs = null;
            try {
                fs = new BinaryReader(
                            File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                // 開始コード通知
                FileInfoEventArgs eofArgs = new FileInfoEventArgs(EventStatus.ONSTART);
                ivdrFinder.OnFileRaiseEvent(eofArgs);
                bw.DoWork += (s, evt) =>  {
                    // ここはUIスレッドではない
                    try {
                        for ( ; ; ) {
                            try {
                                b.status = fs.ReadUInt16();
                                b.id = fs.ReadByte();
                                b.mjdStartDateTime = fs.ReadBytes(b.mjdStartDateTime.Length);
                                b.mark = fs.ReadUInt32();
                                b.title = fs.ReadBytes(b.title.Length);
                                b.numberPdb = fs.ReadBytes(b.numberPdb.Length);;
                                b.numberFlag = fs.ReadUInt16();
                                b.primarykey = fs.ReadUInt16();
                                b.b = fs.ReadUInt16(); ///< 不明
                                b.c = fs.ReadUInt32();
                                b.d = fs.ReadUInt32(); ///<padding?
                                b.e = fs.ReadUInt32();
                                b.pudding = fs.ReadBytes(b.pudding.Length);
                                b.mode = fs.ReadByte();
                                b.pudding2 = fs.ReadBytes(b.pudding2.Length);
                                b.recTime = fs.ReadUInt32();

                                FileInfoEventArgs args = new FileInfoEventArgs(b);
                                try {
                                    using (Stream stream = new FileStream(drive + args.GetImageUri(),
                                                                    FileMode.Open, FileAccess.Read)) {
                                        BitmapDecoder decoder = BitmapDecoder.Create(
                                                                    stream,
                                                                    BitmapCreateOptions.None,
                                                                    BitmapCacheOption.OnLoad);

                                        args.bmp = new WriteableBitmap(decoder.Frames[0]);
                                        args.bmp.Freeze();
                                        stream.Close();
                                    }
                                } catch (Exception) {
                                    ;
                                }
                                // イベント通知
                                ivdrFinder.OnFileRaiseEvent(args);

                            } catch (System.IO.EndOfStreamException) {
                            // ファイルの終わり
                                break;
                            }
                        }

                    } catch (DirectoryNotFoundException) {
                        if ( fs != null ) {
                            fs.Close();
                            fs = null;
                        }
                    } catch (UnauthorizedAccessException) {
                        fs.Close();
                        fs = null;

                    } catch (Exception e) {
                        fs.Close();
                        fs = null;

                    } finally {
                        if ( fs != null ) {
                            fs.Close();
                            fs = null;
                        }
                    }
                };            // スレッド終了時処理。ここはUIスレッド
                bw.RunWorkerCompleted += (s, evt) =>  {
                    // 終了コード通知
                    ivdrFinder.OnFileRaiseEvent(new FileInfoEventArgs(EventStatus.ONEND));
                    ivdrFinder.DeleteEventsFileEvent();
                };
                // 別スレッドを生成し実行
                bw.RunWorkerAsync();
                return;
            } catch {
                if ( fs != null ) {
                    fs.Close();
                }
                fs = null;
                ivdrFinder.DeleteEventsFileEvent();
            } finally {
            }
        }
    }
}
