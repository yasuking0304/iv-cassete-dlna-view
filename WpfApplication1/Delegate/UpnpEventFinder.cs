using System;
using System.Runtime.InteropServices;
using System.Windows;
using View.EventArgs;
using UPNPLib;
 
namespace View.Delegate {

    class UpnpEventFinder  {

        // Upnpイベント通知用
        public delegate void DriveEventHandler(object sender, UpnpDriveInfoEventArgs e);
        public delegate void FileEventHandler(object sender, UpnpListInfoEventArgs e);
        public delegate void FolderEventHandler(object sender, FolderInfoEventArgs e);

        public event DriveEventHandler  DriveInfo;
        public event FileEventHandler   FileInfo;
        public event FolderEventHandler FolderInfo;

        public virtual void OnDriveRaiseEvent(UpnpDriveInfoEventArgs e) {
            if (DriveInfo != null) {
                DriveInfo(this, e);
            }
        }
        public virtual void OnFileRaiseEvent(UpnpListInfoEventArgs e) {
            if (FileInfo != null) {
                FileInfo(this, e);
            }
        }
        public virtual void OnFolderRaiseEvent(FolderInfoEventArgs e) {
            if (FolderInfo != null) {
                if ( e.GetTitle() == null || e.GetTitle().Trim() == string.Empty ) {
                    e.SetTitle( Application.Current.TryFindResource("LBL_GENRE_UNKNOWN") as string );
                }
                FolderInfo(this, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddEventsDriveEvent() {
            DriveInfo += new DriveEventHandler(View.MainWindow.GetInstance().RaiseEvent);
        }
        public void DeleteEventsDriveEvent() {
            DriveInfo -= new DriveEventHandler(View.MainWindow.GetInstance().RaiseEvent);
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
