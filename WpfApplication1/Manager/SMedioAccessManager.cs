using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using View.CommonStruct;
using View.EventArgs;

namespace View.Manager {
    class SMedioAccessManager {

        // インスタンス生成
        private static SMedioAccessManager instance = null;

        public static SMedioAccessManager GetInstance() {
            if (instance == null) {
                instance = new SMedioAccessManager();
            }
            return instance;
        }
        
        /// <summary>
        /// レジストリ読み込み(string)
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetRegistryString(RegistryKey hkey, string key, string value) {
            RegistryKey regkey = hkey.OpenSubKey(key, false);
            //キーが存在しないときは '' が返される
            if (regkey == null) return string.Empty;

            //文字列を読み込む
            string stringValue = (string)regkey.GetValue(value);
            //閉じる
            regkey.Close();
            return stringValue;
        }
        /// <summary>
        /// 一覧取得
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        public List<DataStruct.Folder> GetsMedioFolderList() {
            List<DataStruct.Folder> value = new List<DataStruct.Folder> {};
            string path = GetRegistryString(
                Registry.CurrentUser,
                "Software\\sMedio\\DubbingService\\DubbingService",
                "LastPathAddedToVideoLibrary");
            try
            {
                string[] videoFiles = Directory.GetFiles(path, "*.dtcp.info");
                string tvRecData = "";
                foreach (string file in videoFiles)
                {
                    using (StreamReader st = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
                    {
                        tvRecData = st.ReadToEnd();
                    }
                    // イベント通知
                }

            }
            catch (Exception)
            {
                ;
            }
            return value;
        }
    }
}
