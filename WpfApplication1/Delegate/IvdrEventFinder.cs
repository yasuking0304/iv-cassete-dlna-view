using System;
using System.Windows;
using View.EventArgs;

namespace View.Delegate {

    class IvdrEventFinder {
        // Ivdrイベント通知用
        public delegate void LabelEventHandler(object sender, LabelInfoEventArgs e);
        public delegate void FileEventHandler(object sender, FileInfoEventArgs e);
        public delegate void FolderEventHandler(object sender, FolderInfoEventArgs e);

        public event LabelEventHandler  LabelInfo;
        public event FileEventHandler   FileInfo;
        public event FolderEventHandler FolderInfo;

        public virtual void OnLabelRaiseEvent(LabelInfoEventArgs e) {
            if (LabelInfo != null) {
                LabelInfo(this, e);
            }
        }
        public virtual void OnFileRaiseEvent(FileInfoEventArgs e) {
            if (FileInfo != null) {
                FileInfo(this, e);
            }
        }
        public virtual void OnFolderRaiseEvent(FolderInfoEventArgs e) {
            if (FolderInfo != null) {
                if ( e.GetTitle() == null || e.GetTitle().Trim() == string.Empty ) {
                    e.SetTitle( string.Empty );
                }
                FolderInfo(this, e);
            }
        }
        public void AddEventsLabelEvent() {
            LabelInfo += new LabelEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }
        public void DeleteEventsLabelEvent() {
            LabelInfo -= new LabelEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddEventsFileEvent() {
            FileInfo += new FileEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }
        public void DeleteEventsFileEvent() {
            FileInfo -= new FileEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddEventsFolderEvent() {
            FolderInfo += new FolderEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }
        public void DeleteEventsFolderEvent() {
            FolderInfo -= new FolderEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }

    }
}
