using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using View.CommonEnum;
using View.CommonStruct;
using View.DataConverter.EndianConverter;
using View.DataConverter.LanguageConverter;

namespace View.EventArgs {

    public class FolderInfoEventArgs  {

        string title = string.Empty;
        ushort   folderNumber = 0;
        public ushort[] folderCode = new ushort[] {};
        /// イベント状態
        EventStatus eventStatus = EventStatus.ONSTART;
        /// <summary>
        /// ivdr専用 フォルダ コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ss"></param>
        public FolderInfoEventArgs(CommonStruct.DataStruct.UdffInfTbl indata) {
            this.title = LanguageConverter.GetInstance().GetJis2Unicode(indata.title, indata.title.Length);
            this.folderNumber = EndianConverter.IntConverter(indata.numOfFolder);
            this.folderCode = new ushort[this.folderNumber];
            this.eventStatus = EventStatus.ONDATA;
        }

        /// <summary>
        /// uPnp専用 ジャンル コンストラクタ
        /// </summary>
        /// <param name="title"></param>
        public FolderInfoEventArgs(string title) {
            this.title = title;
            this.folderNumber = 0;
            this.folderCode = new ushort[] {};
            this.eventStatus = EventStatus.ONDATA_GENRE;
        }

        public FolderInfoEventArgs(EventStatus status) {
            this.title = string.Empty;
            this.folderNumber = 0;
            this.folderCode = new ushort[] {};
            this.eventStatus = status;
        }

        public string GetTitle(){
            return title;
        }

        public void SetTitle(string title){
            this.title = title;
        }

        public uint GetFolderNumber(){
            return folderNumber;
        }

        public ushort GetFolderCode(uint byIndex) {
            return (ushort)(EndianConverter.IntConverter(folderCode[byIndex]) & 0x7fff);
        }

        public EventStatus GetEventStatus() {
            return eventStatus;
        }
    }
}
