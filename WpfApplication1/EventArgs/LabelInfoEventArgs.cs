using System;
using System.Text;
using View.CommonStruct;
using View.DataConverter.LanguageConverter;

namespace View.EventArgs {

    public class LabelInfoEventArgs {

        string title = string.Empty;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ss"></param>
        public LabelInfoEventArgs(CommonStruct.DataStruct.TvRecMgr indata) {
            this.title = LanguageConverter.GetInstance().GetJis2Unicode(indata.title, indata.title.Length);
            if ( this.title.Equals(null) ) {
                this.title = string.Empty;
            }
        }

        public LabelInfoEventArgs(bool dummydata) {
            this.title = string.Empty;
        }

        public string GetTitle(){
            return title;
        }
    }
}
