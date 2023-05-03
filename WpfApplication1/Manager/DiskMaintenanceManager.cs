using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace View.Manager
{
    class DiskMaintenanceManager
    {
        private static System.IO.StreamWriter sw = null;
        private static System.Diagnostics.Process p = null;
        private static IpcClientManager client;
        private static string driveLetter = null;

        public void Repair(string driveNumber)
        {

            MainWindow wnd = new MainWindow();
            wnd.CheckDiskStartMessageIVDR();


            //Processオブジェクトを作成
            p = new Process();

            //入力できるようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            //非同期で出力を読み取れるようにする
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            // cmd.exeの設定
            p.StartInfo.FileName =
                System.Environment.GetEnvironmentVariable("ComSpec");
            p.StartInfo.CreateNoWindow = true;

            //非同期で出力の読み取りを開始
            p.OutputDataReceived += new DataReceivedEventHandler(ProcessDataReceived);
            p.ErrorDataReceived += new DataReceivedEventHandler(ProcessDataReceived);
            //起動
            p.Start();
            // 出力開始
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            client = new IpcClientManager();
            // ドライブレターの設定
            driveLetter = driveNumber;

            //入力のストリームを取得
            sw = p.StandardInput;
            sw.WriteLine(@"chcp 437");
            //「diskpart」を実行する
            sw.WriteLine(@"diskpart");
            sw.WriteLine(@"select volume " + driveLetter);
            sw.WriteLine(@"attribute disk");
            Thread.Sleep(1000);
            while (!p.HasExited)
            {
                Thread.Sleep(1000);
            }
            p.WaitForExit();
            p.Close();
        }

        private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data.Trim();
            if (line == "")
            {
                return;
            }
            // diskpart 
            if (line.IndexOf("Microsoft DiskPart version") != -1)
            {
                //diskpartがロードされた
                client.remoteObject.sendData("ロック確認を開始します。");
            }
            // diskpart 
            if (line.IndexOf("Current Read-only State : No") != -1)
            {
                //diskpartを終了する
                sw.WriteLine(@"exit");
                client.remoteObject.sendData("ロックはされていませんでした。");
            }
            // diskpart 
            if (line.IndexOf("Current Read-only State : Yes") != -1)
            {
                //remoteObj.Message = "DISKPART";
                sw.WriteLine(@"attribute disk clear readonly");
                // diskpartを終了する
                sw.WriteLine(@"exit");
                client.remoteObject.sendData("ロックを解除しました。");
            }
            // diskpart exit
            if (line.IndexOf("Leaving DiskPart...") != -1)
            {
                Thread.Sleep(2000);
                // 残り容量チェック
                sw.WriteLine(@"fsutil volume diskfree " + driveLetter);
                client.remoteObject.sendData("ロック処理を終了中です...");
            }
            // fsutil volume diskfree 成功
            if (line.IndexOf("Total free bytes") != -1 && line.IndexOf(" 0 (  0.0 KB)") == -1)
            {
                // コマンドプロンプトを終了する
                sw.WriteLine(@"exit");
                sw.Close();
                client.remoteObject.sendData("チェック処理を終了します。");
            }
            // fsutil volume diskfree 失敗?
            if (line.IndexOf("Total free bytes") != -1 && line.IndexOf(" 0 (  0.0 KB)") != -1)
            {
                // chkdsk起動
                sw.WriteLine(@"chkdsk /f " + driveLetter);
                client.remoteObject.sendData("disk調査処理を開始します。");
            }
            // chkdsk /f
            if (line.IndexOf("The type of the file system is") != -1 && line.IndexOf("UDF.") != -1)
            {
                client.remoteObject.sendData("このドライブはiVDRです。");
            }
            if (line.IndexOf("The type of the file system is") != -1 && line.IndexOf("UDF.") == -1)
            {
                client.remoteObject.sendData("このドライブはiVDRではありません。");
            }
            if (line.IndexOf("CHKDSK is verifying ICBs") != -1)
            {
                client.remoteObject.sendData("disk調査(1/5)");
            }
            if (line.IndexOf("CHKDSK is looking for orphan ICBs") != -1)
            {
                client.remoteObject.sendData("disk調査(2/5)");
            }
            if (line.IndexOf("CHKDSK is verifying ICB links") != -1)
            {
                client.remoteObject.sendData("disk調査(3/5)");
            }
            if (line.IndexOf("CHKDSK is verifying link counts and parent entries") != -1)
            {
                client.remoteObject.sendData("disk調査(4/5)");
            }
            if (line.IndexOf("CHKDSK is verifying object size for ICBs") != -1)
            {
                client.remoteObject.sendData("disk調査(5/5)");
            }
            if (line.IndexOf("file system and found no problems") != -1)
            {
                client.remoteObject.sendData("disk調査結果、問題はありませんでした。");
            }
            if (line.IndexOf("file system and found problems") != -1)
            {
                client.remoteObject.sendData("disk回復処理を行います。30分から1時間かかる場合もあります。");
            }
            if (line.IndexOf("allocation units available on disk") != -1)
            {
                client.remoteObject.sendData("disk調査処理を終了します。");
                Thread.Sleep(2000);
                // コマンドプロンプトを終了する
                sw.WriteLine(@"exit");
                sw.Close();
                client.remoteObject.sendData("チェック処理を終了します。");
            }
        }
    }
}
