using System;
using System.IO;
using System.Text;
using View;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Win32;

namespace View.Manager
{
    class ClipBoardManager {
        static public void SetClipBoardIvdr(List<Customer> ivdrlist) {
            string split = "\t";
            // タイトル
            string getText = string.Empty;
            string text = "\"title\"" + split + "\"startdatetime\"" + split + "\"duration\""
                        + split + "\"filename\"" + split + "\"foldername\"\r\n";

            // itemデータ
            for( int i=0; i<ivdrlist.Count; i++) {
                text += "\"" + ivdrlist[i].Title + "\"" + split
                      + "\"" + ivdrlist[i].SortRecordDatTime + "\"" + split
                      + "\"" + ivdrlist[i].DurationTimeNormal + "\"" + split
                      + "\"" + ivdrlist[i].Filename + "\"" + split
                      + "\"" + ivdrlist[i].Folder  + "\"\r\n";
            }
            Clipboard.SetDataObject(text, true);
        }
        static public void SetClipBoardDlna(List<Customer> ivdrlist) {
            string split = "\t";
            // タイトル
            string getText = string.Empty;
            string text = "\"title\"" + split + "\"startdatetime\"" + split + "\"duration\""
                        + split + "\"id\"" + split + "\"genre\"" + split + "\"description\""
                        + split + "\"channel\"\r\n";

            // itemデータ
            for( int i=0; i<ivdrlist.Count; i++) {
                if ( ivdrlist[i].Classes.StartsWith("object.container") ) {
                    continue;
                }
                string[] genreList = ivdrlist[i].Genre.Split(new char[] { '\t'});
                string genre = string.Empty;
                for ( int igene=0 ; igene <genreList.Length; igene++ ) {
                    if (  genreList[igene].Trim() != string.Empty ) {
                        genre += ( genre == string.Empty) ? genreList[igene] : ";" + genreList[igene];
                    }
                }

                text += "\"" + ivdrlist[i].Title + "\"" + split
                      + "\"" + ivdrlist[i].SortRecordDatTime + "\"" + split
                      + "\"" + ivdrlist[i].DurationTimeNormal + "\"" + split
                      + "\"" + ivdrlist[i].Filename + "\"" + split
                      + "\"" + genre  + "\"" + split
                      + "\"" + ivdrlist[i].Description + "\"" + split
                      + "\"" + ivdrlist[i].Channel  + "\"\r\n";
            }
            Clipboard.SetDataObject(text, true);
        }

        static public void SaveClipBoardIvdr(List<Customer> ivdrlist) {
            string split = "\t";
            // タイトル
            string getText = string.Empty;
            string text = "\"title\"" + split + "\"startdatetime\"" + split + "\"duration\""
                        + split + "\"filename\"" + split + "\"foldername\"\r\n";

            // itemデータ
            for( int i=0; i<ivdrlist.Count; i++) {
                text += "\"" + ivdrlist[i].Title + "\"" + split
                      + "\"" + ivdrlist[i].SortRecordDatTime + "\"" + split
                      + "\"" + ivdrlist[i].DurationTimeNormal + "\"" + split
                      + "\"" + ivdrlist[i].Filename + "\"" + split
                      + "\"" + ivdrlist[i].Description  + "\"\r\n";
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.Filter = "Tab区切りファイル|*.txt";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true) {
                using (Stream fileStream = saveFileDialog.OpenFile())
                using (StreamWriter sr = new StreamWriter(fileStream)) {
                    sr.Write(text);
                }
            }
        }
        static public void SaveClipBoardDlna(List<Customer> ivdrlist) {
            string split = "\t";
            // タイトル
            string getText = string.Empty;
            string text = "\"title\"" + split + "\"startdatetime\"" + split + "\"duration\""
                        + split + "\"id\"" + split + "\"genre\"" + split + "\"description\""
                        + split + "channel\r\n";

            // itemデータ
            for( int i=0; i<ivdrlist.Count; i++) {
                if ( ivdrlist[i].Classes.StartsWith("object.container") ) {
                    continue;
                }
                string[] genreList = ivdrlist[i].Genre.Split(new char[] { '\t'});
                string genre = string.Empty;
                for ( int igene=0 ; igene <genreList.Length; igene++ ) {
                    if (  genreList[igene].Trim() != string.Empty ) {
                        genre += ( genre == string.Empty) ? genreList[igene] : ";" + genreList[igene];
                    }
                }

                text += "\"" + ivdrlist[i].Title + "\"" + split
                      + "\"" + ivdrlist[i].SortRecordDatTime + "\"" + split
                      + "\"" + ivdrlist[i].DurationTimeNormal + "\"" + split
                      + "\"" + ivdrlist[i].Filename + "\"" + split
                      + "\"" + genre  + "\"" + split
                      + "\"" + ivdrlist[i].Description + "\"" + split
                      + "\"" + ivdrlist[i].Channel  + "\"\r\n";
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.Filter = "Tab区切りファイル|*.txt";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true) {
                using (Stream fileStream = saveFileDialog.OpenFile())
                using (StreamWriter sr = new StreamWriter(fileStream)) {
                    sr.Write(text);
                }
            }
        }
    }
}
