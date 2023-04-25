using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using View.CommonEnum;

namespace View.DataConverter.LanguageConverter {
    public class LanguageConverter {
        /// <summary>
        ///  初期値
        /// </summary>
        private static LanguageConverter instance = null;

        private static CODE_SET[] CodeG = new CODE_SET[4];
        private static GCODE_SET[] LockingGL = new GCODE_SET[2];
        private static GCODE_SET[] LockingGR = new GCODE_SET[2];

        /// <summary>
        /// ARIB JIS to UNICODE
        /// </summary>
        private string[,] GSetCode ={ {
        " ", "!", "\"","#", "$", "%", "&", "'" ,"(", ")", "*", "+", ",", "-", ".", "/",
        "0", "1", "2"," 3", "4", "5", "6", "7" ,"8", "9", ":", ";", "<", "=", ">", "?",
        "@", "A", "B"," C", "D", "E", "F", "G" ,"H", "I", "J", "K", "L", "M", "N", "O",
        "P", "Q", "R"," S", "T", "U", "V", "W" ,"X", "Y", "Z", "[", "\\","]" ,"^" ,"_",
        "`", "a", "b"," c", "d", "e", "f", "g" ,"h", "i", "j", "k", "l", "m", "n", "o",
        "p", "q", "r"," s", "t", "u", "v", "w" ,"x", "y", "x", "{", "|", "}", "~"  }
        ,{
        "　","！","”","＃","＄","％","＆","’","（","）","＊","＋","，","－","．","／",
        "０","１","２","３","４","５","６","７","８","９","：","；","＜","＝","＞","？",
        "＠","Ａ","Ｂ","Ｃ","Ｄ","Ｅ","Ｆ","Ｇ","Ｈ","Ｉ","Ｊ","Ｋ","Ｌ","Ｍ","Ｎ","Ｏ",
        "Ｐ","Ｑ","Ｒ","Ｓ","Ｔ","Ｕ","Ｖ","Ｗ","Ｘ","Ｙ","Ｚ","［","￥","］","＾","＿",
        "‘","ａ","ｂ","ｃ","ｄ","ｅ","ｆ","ｇ","ｈ","ｉ","ｊ","ｋ","ｌ","ｍ","ｎ","ｏ",
        "ｐ","ｑ","ｒ","ｓ","ｔ","ｕ","ｖ","ｗ","ｘ","ｙ","ｚ","｛","｜","｝","￣" }
        ,{
        "　","ぁ","あ","ぃ","い","ぅ","う","ぇ","え","ぉ","お","か","が","き","ぎ","く",
        "ぐ","け","げ","こ","ご","さ","ざ","し","じ","す","ず","せ","ぜ","そ","ぞ","た",
        "だ","ち","ぢ","っ","つ","づ","て","で","と","ど","な","に","ぬ","ね","の","は",
        "ば","ぱ","ひ","び","ぴ","ふ","ぶ","ぷ","へ","べ","ぺ","ほ","ぼ","ぽ","ま","み",
        "む","め","も","ゃ","や","ゅ","ゆ","ょ","よ","ら","り","る","れ","ろ","ゎ","わ",
        "ゐ","ゑ","を","ん","　","　","　","ヽ","ヾ","ー","。","「","」","、","・"}
        ,{
        "　","ァ","ア","ィ","イ","ゥ","ウ","ェ","エ","ォ","オ","カ","ガ","キ","ギ","ク",
        "グ","ケ","ゲ","コ","ゴ","サ","ザ","シ","ジ","ス","ズ","セ","ゼ","ソ","ゾ","タ",
        "ダ","チ","ヂ","ッ","ツ","ヅ","テ","デ","ト","ド","ナ","ニ","ヌ","ネ","ノ","ハ",
        "バ","パ","ヒ","ビ","ピ","フ","ブ","プ","ヘ","ベ","ペ","ホ","ボ","ポ","マ","ミ",
        "ム","メ","モ","ャ","ヤ","ュ","ユ","ョ","ヨ","ラ","リ","ル","レ","ロ","ヮ","ワ",
        "ヰ","ヱ","ヲ","ン","ヴ","ヵ","ヶ","ヽ","ヾ","ー","。","「","」","、","・"}
        };

        /// <summary>
        /// ARIB領域(1-3) 感嘆符など
        /// JIS CODE 0x7D21～0x7D7B
        /// </summary>
        private string[] ARIB_ADD_X7D ={
        "(月)",	"(火)",	"(水)",	"(木)",	"(金)",	"(土)",	"(日)",	"(祝)",
		"㍾",	"㍽",	"㍼",	"㍻",	"№",	"℡",	"(〒)",	"○",
		"[本]",	"[三]",	"[二]",	"[安〕","[点]",	"[打〕","[盗]",	"[勝]",
		"[敗]",	"[Ｓ]",	"[投]",	"[捕］","[一]",	"[二］","[三]",	"[遊]",
		"[左]",	"[中]",	"[右]",	"[指］","[走]",	"[打］","㍑",	"㎏",
		"Hz",	"ha",	"km",	"k㎡",  "hPa",	"・",	"・",	"1/2",
		"0/3",	"1/3",	"2/3",	"1/4",	"3/4",	"1/5",	"2/5",	"3/5",
		"4/5",	"1/6",	"5/6",	"1/7",	"1/8",	"1/9",	"1/10",	"晴れ",
		"曇り",	"雨",	"雪",	"△",	"▲",	"▽",	"▼",	"◆",
		"・",	"・",	"・",	"◇",	"◎",	"!!",	"!?",	"曇/晴",
		"雨",	"雨",	"雪",	"大雪",	"雷",	"雷雨",	"　",	"・",
		"・",	"♪",	"℡"};
        /// <summary>
        /// ARIB領域(1-3g) 感嘆符など
        /// JIS CODE 0x7D21～0x7D7B
        /// </summary>
        private string[] ARIB_ADD_X7DG ={
        "(月)",	"(火)",	"(水)",	"(木)",	"(金)",	"(土)",	"(日)",	"(祝)",
		"㍾",	"㍽",	"㍼",	"㍻",	"№",	"℡",	"(〒)",	"○",
		"[本]",	"[三]",	"[二]",	"[安〕","[点]",	"[打〕","[盗]",	"[勝]",
		"[敗]",	"[Ｓ]",	"[投]",	"[捕］","[一]",	"[二］","[三]",	"[遊]",
		"[左]",	"[中]",	"[右]",	"[指］","[走]",	"[打］","㍑",	"㎏",
		"Hz",	"ha",	"km",	"k㎡",  "hPa",	"・",	"・",	"1/2",
		"0/3",	"1/3",	"2/3",	"1/4",	"3/4",	"1/5",	"2/5",	"3/5",
		"4/5",	"1/6",	"5/6",	"1/7",	"1/8",	"1/9",	"1/10",	"晴れ",
		"曇り",	"雨",	"雪",	"△",	"▲",	"▽",	"▼",	"◆",
		"・",	"・",	"・",	"◇",	"◎",	"!!",	"\uE2F0","曇/晴",
		"雨",	"雨",	"雪",	"大雪",	"雷",	"雷雨",	"　",	"・",
		"・",	"♪",	"℡"};

        private string[] ARIB_ADD_X7DU ={
        "㈪",	"㈫",	"㈬",	"㈭",	"㈮",	"㈯",	"㈰",	"㈷",
		"㍾",	"㍽",	"㍼",	"㍻",	"№",	"℡",	"〶",	"○",
		"🉀",	"🉁",	"🉂",	"🉃",   "🉄",	"🉅",   "🉆",	"🉇",
		"🉈",	"🄪",	"🈧",	"🈨",   "🈩",	"🈔",   "🈪",	"🈫",
		"🈬",	"🈭",	"🈮",	"🈯",   "🈰",	"🈱",   "㍑", 	"㎏",
		"Hz",	"ha",	"km",	"㎢",   "㍱",	"・",	"・",	"½",
		"↉",	"⅓",	"⅔",	"¼",	"¾",	"⅕",	"⅖",	"⅗",
		"⅘",	"⅙",	"⅚",	"⅐",	"⅛",	"⅑",	"⅒",	"☀",
		"☁",	"☂",	"☃",	"△",	"▲",	"▽",	"▼",	"◆",
		"・",	"・",	"・",	"◇",	"◎",	"!!",	"!?"    ,"⛅",
		"☔",	"⛆",	"☃",	"⛇",	"⚡",	"⛈",	"　",	"・",
		"・",	"♪",	"℡"};

        /// <summary>
        /// ARIB領域(1-1) 四角囲み文字など
        /// JISコード 0x7A50～0x7A74
        /// </summary>
        private string[] ARIB_ADD_X7A ={
        "[HV]","[SD]", "[P]",  "[W]",   "[MV]", "[手]", "[字]", "[双]",
        "[デ]","[S]",  "[二]", "[多]",  "[解]", "[SS]", "[B]",  "[N]",
        "■",  "●",   "[天]", "[交]",  "[映]", "[無]", "[料]", "[鍵]",
        "[前]","[後]", "[再]", "[新]",  "[初]", "[終]", "[生]", "[販]",
        "[声]","[吹]", "[PPV]","(秘)", "ほか"};
        /// ARIB領域(1-1g 外字版) 四角囲み文字など
        /// JISコード 0x7A50～0x7A74
        /// </summary>
        private string[] ARIB_ADD_X7AG = {
        "\uE0F8","\uE0F9","\uE0FA","\uE0FB", "\uE0FC","\uE0FD","\uE0FE","\uE0FF",
        "\uE180","\uE181","\uE182","\uE183", "\uE184","\uE185","\uE186","\uE187",
        "■"    ,"●"    ,"\uE18A","\uE18B", "\uE18C","\uE18D","\uE18E","\uE18F",
        "\uE190","\uE191","\uE192","\uE193", "\uE194","\uE195","\uE196","\uE197",
        "\uE198","\uE199","\uE19A","\uE19B", "\uE19C"
        };
        /// <summary>
        /// ARIB領域(2-3 UNICODE6.x版) 四角囲み文字など
        /// JISコード 0x7A50～0x7A74
        /// </summary>
        private string[] ARIB_ADD_X7AU = {
        "\U0001F14A","\U0001F14C","\U0001F13F","\U0001F146", "\U0001F14B","\U0001F210","\U0001F211","\U0001F212",
        "\U0001F213","\U0001F142","\U0001F214","\U0001F215", "\U0001F216","\U0001F14D","\U0001F131","\U0001F13D",
        "■","●",   "\U0001F217","\U0001F218","\U0001F219", "\U0001F21A","\U0001F21B","\u26BF"    ,"\U0001F21C",
        "\U0001F21D","\U0001F21E","\U0001F21F","\U0001F220", "\U0001F221","\U0001F222","\U0001F223","\U0001F224",
        "\U0001F225","\U0001F14E","\u3299"    ,"\U0001F200"
        };

        /// <summary>
        /// ARIB領域(1-4) 丸囲み文字、ギリシャ文字
        /// JISコード 0x7E21～0x7E7D
        /// </summary>
        private string[] ARIB_ADD_X7E = {
               "Ⅰ", "Ⅱ",  "Ⅲ",   "Ⅳ",  "Ⅴ",  "Ⅵ",  "Ⅶ",
        "Ⅷ",  "Ⅸ", "Ⅹ",  "XI",   "XII", "⑰",  "⑱",  "⑲",
        "⑳",  "(1)","(2)", "(3)",  "(4)", "(5)", "(6)", "(7)",
        "(8)", "(9)","(10)","(11)", "(12)","(21)","(22)","(23)",
        "(24)","(A)","(B)", "(C)",  "(D)", "(E)", "(F)", "(G)",
        "(H)", "(I)","(J)", "(K)",  "(L)", "(M)", "(N)", "(O)",
        "(P)", "(Q)","(R)", "(S)",  "(T)", "(U)", "(V)", "(W)",
        "(X)", "(Y)","(Z)", "(25)", "(26)","(27)","(28)","(29)",
        "(30)","①", "②",  "③",   "④",  "⑤",  "⑥",  "⑦",
        "⑧",  "⑨", "⑩",  "⑪",   "⑫",  "⑬",  "⑭",  "⑮",
        "⑯",  "①", "②",  "③",   "④",  "⑤",  "⑥",  "⑦",
        "⑧",  "⑨", "⑩",  "⑪",   "⑫",  "(31)"};

        /// <summary>
        /// ARIB領域(2-4 UNICODE6.x版) 丸囲み文字、ギリシャ文字
        /// JISコード 0x7E21～0x7E70
        /// </summary>
        private string[] ARIB_ADD_X7EU = {
            "Ⅰ", "Ⅱ", "Ⅲ",                              "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ",
        "Ⅷ", "Ⅸ", "Ⅹ", "\u216A",                          "\u216B","⑰", "⑱", "⑲",
        "⑳","\u2776","\u2777","\u2778",                     "\u2779","\u277A","\u277B","\u277C",
        "\u277D","\u277E","\u277F","\u24EB",                 "\u24EC","\u3251","\u3252","\u3253",
        "\u3254",    "\U0001F110","\U0001F111","\U0001F112", "\U0001F113","\U0001F114","\U0001F115","\U0001F116",
        "\U0001F117","\U0001F118","\U0001F119","\U0001F11A", "\U0001F11B","\U0001F11C","\U0001F11D","\U0001F11E",
        "\U0001F11F","\U0001F120","\U0001F121","\U0001F122", "\U0001F123","\U0001F124","\U0001F125","\U0001F126",
        "\U0001F127","\U0001F128","\U0001F129","\u3255",     "\u3256","\u3257","\u3258","\u3259",
        "\u325A","①","②","③",                             "④","⑤","⑥","⑦",
        "⑧","⑨","⑩","⑪",                                 "⑫","⑬","⑭","⑮",
        "⑯"};
        /// <summary>
        /// Blankコード
        /// </summary>
        private string BLANK = "□";
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static LanguageConverter GetInstance() {
            if (instance == null) {
                instance = new LanguageConverter();
            }
            return instance;
        }

        /// <summary>
        /// 生のARIBコードをテキストに変換する
        /// </summary>
        /// <param name="instr"></param>
        /// <returns></returns>
        public string GetAribtext2Hirabun(string instr) {
            string outtemp = string.Empty;
            if ( instr != null && instr.Trim() != string.Empty ) {
                outtemp = instr;
            
                for (int i = 0; i<ARIB_ADD_X7AG.Length; i++) {
                    outtemp = outtemp.Replace(ARIB_ADD_X7AG[i], ARIB_ADD_X7A[i]);
                }
                for (int i = 0; i<ARIB_ADD_X7DG.Length; i++) {
                    outtemp = outtemp.Replace(ARIB_ADD_X7DG[i], ARIB_ADD_X7D[i]);
                }
                outtemp = outtemp.Replace("〜", "～");
                outtemp = outtemp.Replace("―", "ー");
                outtemp = outtemp.Replace("\u203c", "!!");
                outtemp = outtemp.Replace("\u2049", "!?");
                outtemp = outtemp.Replace("\ua027", "”");
                outtemp = outtemp.Replace("\ua00f", "~");
            }
            return outtemp;
        }

        /// <summary>
        /// 文字検索
        /// </summary>
        /// <param name="title">タイトル文字列</param>
        /// <param name="description">番組詳細文字列</param>
        /// <param name="searchWord">検索文字列</param>
        /// <returns></returns>
        public bool IsSearchString(string title, string description, string searchWord) {
            bool retValue = false;
            bool addkey   = false;
            string keyword = string.Empty;
            string[] m_keywords = new string[] {};
            if ( searchWord.Trim() == string.Empty ) {
                return true;
            }
            string[] keywords = Strings.StrConv(GetAribtext2Hirabun( searchWord.ToUpper())
                    ,VbStrConv.Wide, 0).Trim().Split( new string[] {  Strings.StrConv((" "),
                                                                      VbStrConv.Wide) },
                                                                      StringSplitOptions.RemoveEmptyEntries);
            string titleWide = Strings.StrConv(title.ToUpper(), VbStrConv.Wide, 0);
            string descriptWide = Strings.StrConv(description.ToUpper(), VbStrConv.Wide, 0);
            foreach ( string keywordX in keywords ) {
                if ( keywordX.Substring(0, 1) == "－" ) {
                    if ( keywordX.Substring(1).Trim() != string.Empty ) {
                        m_keywords.CopyTo(m_keywords = new string[m_keywords.Length + 1], 0);
                        m_keywords[m_keywords.Length - 1] = keywordX.Substring(1).Trim();
                    }
                    continue;
                }
                if ( keywordX.Substring(0, 1) == "＋" ) {
                    if ( keywordX.Substring(1).Trim() != string.Empty ) {
                        keyword = keywordX.Substring(1).Trim();
                        addkey   = true;
                    }
                } else { 
                    if ( keywordX.Trim() != string.Empty ) {
                        keyword = keywordX.Trim();
                        addkey   = true;
                    }
                }
                if ( keyword == string.Empty ) {
                    continue;
                } else if ( titleWide.Contains(keyword) ) {
                    retValue = true;
                } else if ( descriptWide.Contains(keyword) ) {
                    retValue = true;
                }
            }
            //
            if ( !addkey ) {
                retValue = true;
            }
            foreach ( string keywordX in m_keywords ) {
                if ( keywordX == string.Empty ) {
                    continue;
                } else if ( titleWide.Contains(keywordX) ) {
                    retValue = false;
                    break;
                } else if ( descriptWide.Contains(keywordX) ) {
                    retValue = false;
                    break;
                }
            }
            return retValue;
        }

        /// <summary>
        /// テキスト組み合わせで書かれた外字をUNICODE6.xに置換する
        /// </summary>
        /// <param name="instr"></param>
        /// <returns></returns>
        public string Gettext2Unicode6(string instr) {
            string outtemp = string.Empty;
            if ( instr != null && instr.Trim() != string.Empty ) {
                outtemp = instr;

                for (int i = 0; i<ARIB_ADD_X7A.Length-1; i++) {
                    outtemp = outtemp.Replace(ARIB_ADD_X7A[i], ARIB_ADD_X7AU[i]);
                }
                for (int i = 0; i< ARIB_ADD_X7E.Length; i++) {
                    outtemp = outtemp.Replace(ARIB_ADD_X7E[i], ARIB_ADD_X7EU[i]);
                }
            }
            return outtemp;
        }

        /// <summary>
        /// ARIB JISコードをUNICODEに変換する
        /// </summary>
        /// <param name="indata">ARIB JISコードが格納されたバイト配列</param>
        /// <param name="indataSize">変換サイズ</param>
        /// <returns>UNICODE文字列</returns>
        public string GetJis2Unicode(byte[] indata, int indataSize) {
            string outtemp = string.Empty;
            int i = 0;
            // 4.3 初期化(ARIB TR-B14)
            bool IsZenkaku = true;
            bool IsZen2Han = false;
            LockingGL = new GCODE_SET[] {
                            GCODE_SET.G0,
                            GCODE_SET.NULL
                            };
            LockingGR = new GCODE_SET[] {
                            GCODE_SET.G2,
                            GCODE_SET.NULL
                            };
            CodeG = new CODE_SET[] {
                            CODE_SET.KANJI,
                            CODE_SET.ALPHANUMERIC,
                            CODE_SET.HIRAGANA,
                            CODE_SET.KATAKANA
                            };            
            
            try {
                for (; i < indataSize ;) {
                    byte data = indata[i++];
                    if (data == 0x1b) {
                    // [ESC]シーケンス
                        data = indata[i++];
                        if (data == 0x24) {
                        // 2バイトGセット指示
                            data = indata[i++];
                            switch(data){
                                case 0x29   : DesignationGSET(GCODE_SET.G1, indata[i++]); break;     ///< 2バイトGセット G1
                                case 0x2a   : DesignationGSET(GCODE_SET.G2, indata[i++]); break;     ///< 2バイトGセット G2
                                case 0x2b   : DesignationGSET(GCODE_SET.G3, indata[i++]); break;     ///< 2バイトGセット G3
                                default		: DesignationGSET(GCODE_SET.G0, data);        break;     ///< 2バイトGセット G0
                            }
                            IsZenkaku = true;
                            continue;
                        } else if (data >= 0x28 && data<= 0x2b) {
                        // 1バイトGセット指示
                            switch(data){
                                case 0x28   : DesignationGSET(GCODE_SET.G0, indata[i++]); break;     ///< 1バイトGセット G0
                                case 0x29   : DesignationGSET(GCODE_SET.G1, indata[i++]); break;     ///< 1バイトGセット G1
                                case 0x2a   : DesignationGSET(GCODE_SET.G2, indata[i++]); break;     ///< 1バイトGセット G2
                                case 0x2b   : DesignationGSET(GCODE_SET.G3, indata[i++]); break;     ///< 1バイトGセット G3
                                default		: break;
                            }
                            IsZenkaku = true;
                            continue;
                        } else if (data == 0x6f || data == 0x6e || (data >= 0x7c && data <= 0x7e)) {
                        // 呼び出し制御 LS2, LS3
                            InvokeGSET(data);
                            continue;
                        }
                    } else if (data == 0x20) {
                    // 半角スペース
                        outtemp += " ";
                        continue;
                    } else if (data == 0x88 || data == 0x89) {
                    // SSZ or MSZ. 2バイトコードのまま次回から半角
                        IsZen2Han = true;
                        continue;
                    } else if (data == 0x8a) {
                    // NSZ
                        IsZen2Han = false;
                        continue;
                    } else if ((data == 0x0e) || (data == 0x0f) || (data == 0x19) || (data == 0x1d) ) {
                    // 呼び出し制御 LS1, LS0, SS2, SS3
                        InvokeGSET(data);
                        continue;
                    }
                    if (data == 0x00) {
                        break;
                    }
                    if ((data > 0x20 && data < 0x7f) || (data > 0xa0 && data < 0xff)) {
                        CODE_SET mode = CODE_SET.NULL;
                        if ( (data & 0x80) == 0 ) {
                        // シングルシフトを考慮してグラフィックセット取得
                            mode =( (LockingGL[1] == GCODE_SET.NULL)
                                        ? CodeG[(int)LockingGL[0]] : CodeG[(int)LockingGL[1]]);
                        } else {
                            data &= 0x7f;
                            // グラフィックセット取得
                            mode = CodeG[(int)LockingGR[0]];
                        }
                        // プロポーショナルコード変換
                        if (mode == CODE_SET.PROP_ALPHANUMERIC) mode = CODE_SET.ALPHANUMERIC;
                        if (mode == CODE_SET.PROP_HIRAGANA)     mode = CODE_SET.HIRAGANA;
                        if (mode == CODE_SET.PROP_KATAKANA)     mode = CODE_SET.KATAKANA;
                        //
                        if (mode == CODE_SET.ALPHANUMERIC || mode == CODE_SET.HIRAGANA || mode == CODE_SET.KATAKANA) {
                        // 1バイト文字
                            if ( IsZen2Han ) {
                            // 2バイト全角→半角変換
                                outtemp += GSetCode[0, data - 0x20];
                            } else if  (IsZenkaku == true) {
                                outtemp += GSetCode[(int)mode+1, data - 0x20];
                            } else {
                                outtemp += GSetCode[0, data - 0x20];
                            }
                        }
                        if (mode == CODE_SET.KANJI || mode == CODE_SET.JIS_KANJI_PLANE_1
                            || mode == CODE_SET.JIS_KANJI_PLANE_2 || mode == CODE_SET.ADDITIONAL_SYMBOLS) {
                            byte lowdata = (byte)(indata[i++] & 0x7f);
                            if ( IsZen2Han ) {
                            // 2バイト全角→半角変換
                                outtemp += GetZen2hanCode(data, lowdata);
                            } else {
                            // 漢字領域
                                if ( data < 0x7a ) {
                                    outtemp += GetZenkakuCode(data, lowdata);
                                } else {
                                    outtemp += GetAdditionalCode(data, lowdata);
                                }
                            }
                        }
                    }
                    // シングルシフトONであれば解除する
                    if (LockingGL[1] != GCODE_SET.NULL) {
                        SingleShiftGL(GCODE_SET.NULL);
                    }
                }
            } catch (Exception e) {
                outtemp =outtemp;
            }
            return outtemp;

        }

        /// <summary>
        /// JIS to SJIS変換(Low byte取得)
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        static public  byte lowJis2Sjis(byte high, byte low) {
            byte value = (byte)low;
            if ((high & 1) != 0) {
               value += 0x1f;
                if (value >= 0x7f) {
                    value++;
                }
            } else {
                value += 0x7e;
            }
            return value;
        }

        /// <summary>
        /// JIS to SJIS変換(High byte取得)
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        static public  byte highJis2Sjis(byte high, byte low) {
            byte value = (byte)((high - 0x21) >> 1);
            if (value >= 0x1f) {
                value += 0xc1;
            } else {
                value += 0x81;
            }
            return value;
        }

        /// <summary>
        /// SJIS to JIS変換(Low byte取得)
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        static public  byte lowSjis2Jis(byte high, byte low) {
            byte value = (byte)low;
            if (value >= 0x7f) {
                value--;
            }
            if (value >= 0x9e) {
                value -=0x7d;
            } else {
                value -=0x1f;
            }
            return value;
        }

        /// <summary>
        /// SJIS to JIS変換(High byte取得)
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        static public  byte highSjis2Jis(byte high, byte low) {
            byte value = (byte)high;
            if (value <= 0x9f) {
                value -= 0x71;
            } else {
                value -= 0xb1;
            }
            value*=2;
            value++;
            if(low>=0x9e){
                value++;
            }
            return value;
        }
        
        /// <summary>
        /// 全角変換
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        static public string GetZenkakuCode(byte high, byte low) {
            byte highValue = (byte)((high - 0x21) >> 1);
            if (highValue >= 0x1f) {
                highValue += 0xc1;
            } else {
                highValue += 0x81;
            }

            byte lowValue = (byte)low;
            if ((high & 1) != 0) {
                lowValue += 0x1f;
                if (lowValue >= 0x7f) {
                    lowValue++;
                }
            } else {
                lowValue += 0x7e;
            }
            Byte[] temp = new Byte[3] {highValue, lowValue, 0};
            return System.Text.Encoding.GetEncoding(932).GetString(temp).Split('\0')[0];
        }
        /// <summary>
        /// 全角 to 半角変換
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        static public string GetZen2hanCode(byte high, byte low) {
            byte highValue = (byte)((high - 0x21) >> 1);
            if (highValue >= 0x1f) {
                highValue += 0xc1;
            } else {
                highValue += 0x81;
            }

            byte lowValue = (byte)low;
            if ((high & 1) != 0) {
               lowValue += 0x1f;
                if (lowValue >= 0x7f) {
                    lowValue++;
                }
            } else {
                lowValue += 0x7e;
            }
             Byte[] temp = new Byte[3] {highValue, lowValue, 0};
            string str = System.Text.Encoding.GetEncoding(932).GetString(temp).Split('\0')[0];
            // バッククォート、グレイヴアクセントの変換
            if ( str == "′" || str=="´") {
                str = "’";
            }
            return Strings.StrConv(str, VbStrConv.Narrow);
        }

        /// <summary>
        /// ARIB外字変換
        /// </summary>
        private string GetAdditionalCode(byte high, byte low) {
            string outtemp = string.Empty;
            if (high == 0x7a) {
            // ARIB 四角囲み文字など
                if ((low >= 0x50) && (low - 0x50) < ARIB_ADD_X7A.Length) {
                    outtemp = ARIB_ADD_X7A[(low - 0x50)];
                } else {
                    outtemp = BLANK;
                }
            } else if (high == 0x7d) {
            // ARIB 感嘆符
                if ((low >= 0x21) && (low - 0x21) < ARIB_ADD_X7D.Length) {
                    outtemp = ARIB_ADD_X7D[(low - 0x21)];
                } else {
                    outtemp = BLANK;
                }
            } else if (high == 0x7e) {
            // ARIB 丸囲み文字など
                if ((low >= 0x21) && (low - 0x21) < ARIB_ADD_X7E.Length) {
                    outtemp = ARIB_ADD_X7E[(low - 0x21)];
                } else {
                    outtemp = BLANK;
                }
            }
            return outtemp;
        }

        private void LockingShiftGL(GCODE_SET byIndexG) {
	        // LSx
            LockingGL[0] = byIndexG;
        }

        private void LockingShiftGR(GCODE_SET byIndexG) {
	        // LSxR
	        LockingGR[0] = byIndexG;
        }

        private void SingleShiftGL(GCODE_SET byIndexG) {
	        // SSx
            LockingGL[1] = byIndexG;
        }

        private bool InvokeGSET(byte byCode) {
            // 呼び出し制御でGセットを割り当てる
            switch(byCode){
                case 0x0e   : LockingShiftGL(GCODE_SET.G1);                         return true;    ///< 呼び出し LS0
                case 0x0f   : LockingShiftGL(GCODE_SET.G0);                         return true;    ///< 呼び出し LS1
                case 0x19   : SingleShiftGL(GCODE_SET.G2);                          return true;    ///< 呼び出し SS2
                case 0x1d   : SingleShiftGL(GCODE_SET.G3);                          return true;    ///< 呼び出し SS3
                case 0x6e   : LockingShiftGL(GCODE_SET.G2);                         return true;    ///< 呼び出し LS2
                case 0x6f   : LockingShiftGL(GCODE_SET.G3);                         return true;    ///< 呼び出し LS3
                case 0x7c   : LockingShiftGR(GCODE_SET.G3);                         return true;    ///< 呼び出し LS3R
                case 0x7d   : LockingShiftGR(GCODE_SET.G2);                         return true;    ///< 呼び出し LS2R
                case 0x7e   : LockingShiftGR(GCODE_SET.G1);                         return true;    ///< 呼び出し LS1R
		        default		: return false;		///< 不明なグラフィックセット
		    }
        }
        ///
        private bool DesignationGSET(GCODE_SET byIndexG, byte byCode) {
	        // Gのグラフィックセットを割り当てる
	        switch(byCode){
		        case 0x42	: CodeG[(int)byIndexG] = CODE_SET.KANJI;				return true;	///< Kanji
		        case 0x4A	: CodeG[(int)byIndexG] = CODE_SET.ALPHANUMERIC;		    return true;	///< Alphabet+numeric
		        case 0x30	: CodeG[(int)byIndexG] = CODE_SET.HIRAGANA;		    	return true;	///< Hiragana
		        case 0x31	: CodeG[(int)byIndexG] = CODE_SET.KATAKANA;	    		return true;	///< Katakana
		        case 0x32	: CodeG[(int)byIndexG] = CODE_SET.MOSAIC_A;			    return true;	///< Mosaic A
		        case 0x33	: CodeG[(int)byIndexG] = CODE_SET.MOSAIC_B;		    	return true;	///< Mosaic B
		        case 0x34	: CodeG[(int)byIndexG] = CODE_SET.MOSAIC_C;	    		return true;	///< Mosaic C
		        case 0x35	: CodeG[(int)byIndexG] = CODE_SET.MOSAIC_D;			    return true;	///< Mosaic D
		        case 0x36	: CodeG[(int)byIndexG] = CODE_SET.PROP_ALPHANUMERIC;	return true;	///< Proportional Alphabet+numeric
		        case 0x37	: CodeG[(int)byIndexG] = CODE_SET.PROP_HIRAGANA;		return true;	///< Proportional Hiragana
		        case 0x38	: CodeG[(int)byIndexG] = CODE_SET.PROP_KATAKANA;		return true;	///< Proportional Katakana
		        case 0x49	: CodeG[(int)byIndexG] = CODE_SET.JIS_X0201_KATAKANA;   return true;	///< JIS X 0201 Katakana
		        case 0x39	: CodeG[(int)byIndexG] = CODE_SET.JIS_KANJI_PLANE_1;	return true;	///< JIS compatible Kanji Plane 1
		        case 0x3A	: CodeG[(int)byIndexG] = CODE_SET.JIS_KANJI_PLANE_2;	return true;	///< JIS compatible Kanji Plane 2
		        case 0x3B	: CodeG[(int)byIndexG] = CODE_SET.ADDITIONAL_SYMBOLS;   return true;	///< Additional symbols
		        default		: return false;		///< 不明なグラフィックセット
		    }
        }
    }
}
