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

    public class FileInfoEventArgs {

        /// <summary>
        /// ファイル名
        /// </summary>
        string fileName = string.Empty;
        /// <summary>
        /// タイトル
        /// </summary>
        string title = string.Empty;
        /// <summary>
        /// 作成日時
        /// </summary>
        DateTime recordDateTime = new DateTime();
        /// <summary>
        /// 録画時間
        /// </summary>
        TimeSpan durationTime = new TimeSpan();
        /// <summary>
        /// 録画モード
        /// </summary>
        RecMode recordMode = RecMode.DIGITAL;
        /// <summary>
        /// イメージURI
        /// </summary>
        string imageUri = string.Empty;
        /// <summary>
        /// ステータス(番組ロックなど)
        /// </summary>
        string status = string.Empty;
        /// <summary>
        /// ファイルに紐づく固有の番号
        /// </summary>
        ushort primarykey = 0;
        ushort id = 0;
        /// <summary>
        /// ビットマップ
        /// </summary>
        public BitmapSource bmp = null;
        ///
        /// イベント状態
        EventStatus eventStatus = EventStatus.ONSTART;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ss"></param>
        public FileInfoEventArgs(CommonStruct.DataStruct.OpgrInfTbl indata) {
            // 番組ロック情報
            if ((indata.status & 0x80) != 0) {
                this.status = "\u00CF";         ///< 鍵マーク
            }

            //日付の構築(MJD:修正ユリウス日)
            long ui = ((indata.mjdStartDateTime[0]) << 8) + indata.mjdStartDateTime[1];
            DateTime tm = new DateTime(1858, 11, 17);
            tm = tm.AddDays(ui);
            // 1904以前になった場合、2038桁あふれとみなし、過年する
            if ( tm.Year < 1904 ) {
                tm.AddYears(2038 - 1858);
            }
            // 時間の構築(BCD形式)
            int hour = ((indata.mjdStartDateTime[2] & 0x0f0)>>4) * 10 + (indata.mjdStartDateTime[2] & 0x0f);
            int min  = ((indata.mjdStartDateTime[3] & 0x0f0)>>4) * 10 + (indata.mjdStartDateTime[3] & 0x0f);
            int sec  = ((indata.mjdStartDateTime[4] & 0x0f0)>>4) * 10 + (indata.mjdStartDateTime[4] & 0x0f);
            //
            tm = tm.AddHours(hour);
            tm = tm.AddMinutes(min);
            tm = tm.AddSeconds(sec);
            this.recordDateTime = tm;
            // 録画時間(BCD形式)
            hour = (((int)indata.recTime & 0xf0)>>4) * 10      + ((int)indata.recTime & 0x0f);
            min  = (((int)indata.recTime & 0xf000)>>12) * 10   + (((int)indata.recTime & 0x0f00) >> 8);
            sec  = (((int)indata.recTime & 0xf00000)>>20) * 10 + (((int)indata.recTime & 0x0f0000) >> 16);
            this.durationTime = new TimeSpan(hour, min, sec);

            // 録画モード
            this.recordMode = RecMode.DIGITAL;
            int recMode = (int)indata.mode;
            if (recMode == 4) {
                this.recordMode = RecMode.ANALOG;
            } else if (recMode == 0x10) {
                this.recordMode = RecMode.DIGITAL;
            } else if (recMode == 0x12) {
                this.recordMode = RecMode.DIGITAL;
            } else if (recMode == 0x41) {
                this.recordMode = RecMode.DIGITAL;
            } else {
                ;
            }
            // ファイル名(Packed binary)
            this.fileName = string.Format(
                        (Application.Current.TryFindResource("FILE_NAME") as string),
                        indata.numberPdb[0],indata.numberPdb[1]);
            if ((indata.numberFlag & 0x20) == 0x20) {
                // 画像名
                this.imageUri = string.Format(
                        (Application.Current.TryFindResource("IMAGE_NAME") as string), indata.primarykey);
            } else {
                this.imageUri = string.Empty;
            }
            // ファイル固有番号
            this.primarykey = indata.primarykey;
            this.id         = indata.id;
            // タイトル
            string instr =  LanguageConverter.GetInstance().GetJis2Unicode(indata.title, indata.title.Length);
            Byte[] test = new Byte[0x80];
            int offset = 0;
            //try {
            //    int iret = LanguageConverter.GetInstance().ConvUnicode2Jis(instr, test, 0x60, offset); 
            //} catch (Exception e) {
            //    offset = offset;
            //}
            this.title = instr;
            //this.title = LanguageConverter.GetInstance().Gettext2Unicode6(instr);
            this.eventStatus = EventStatus.ONDATA;
            this.bmp = null;
        }

        public FileInfoEventArgs(EventStatus eof) {
            this.fileName = string.Empty;
            this.title = string.Empty;
            this.recordDateTime = new DateTime();
            this.durationTime = new TimeSpan();
            this.recordMode = RecMode.DIGITAL;
            this.imageUri = string.Empty;
            this.status = string.Empty;
            this.primarykey = 0;
            this.id = 0;
            this.bmp = null;
            this.eventStatus = eof;
        }
        public string GetFileName() {
            return fileName;
        }

        public string GetTitle() {
            return title;
        }

        public DateTime GetRecordDatTime() {
            return recordDateTime;
        }

        public TimeSpan GetRecDurationTime() {
            return durationTime;
        }

        public RecMode GetRecordMode() {
            return recordMode;
        }

        public string GetImageUri() {
            return imageUri;
        }

        public string GetStatus() {
            return status;
        }

        public ushort GetPrimaryKey() {
            return primarykey;
        }

        public ushort GetId() {
            return id;
        }

        public EventStatus GetEventStatus() {
            return eventStatus;
        }
    }
}
