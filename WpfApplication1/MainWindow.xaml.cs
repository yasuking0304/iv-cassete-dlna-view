using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using View.CommonEnum;
using View.CommonStruct;
using View.DataConverter.LanguageConverter;
using View.DataConverter.StringConverter;
using View.Delegate;
using View.Manager;
using View.EventArgs;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace View
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, int bRevert);

        [DllImport("user32.dll")]
        private static extern int AppendMenu(
                  IntPtr hMenu, int Flagsw, int IDNewItem, string lpNewItem);

        private const int MF_SEPARATOR = 0x0800;
        private const int MENU_ABOUT = 100;

        private List<DataStruct.Drivemap> driveletter = new List<DataStruct.Drivemap> { };

        private List<DataStruct.Drivemap> dlnaletter = new List<DataStruct.Drivemap> { };

        private List<DataStruct.Folder> folder = new List<DataStruct.Folder> { };

        private static MainWindow instance = null;

        private List<Customer> ivdrFileList = new List<Customer> { };

        private static bool? CopyButtonLongClicked = null;
        private static System.ComponentModel.BackgroundWorker bwcopy = null;

        private List<Customer> dlnaFileList = new List<Customer> { };
        private List<Customer> dlnaFileListbackup = new List<Customer> { };
        private EXEC_MODE ExecMode = EXEC_MODE.NONE;
        private UpnpDeviceEventFinder eventFinder = new UpnpDeviceEventFinder();
        private DispatcherTimer timer = new DispatcherTimer();
        private IpcRemoteObject remoteObject;
        private Process prs;

        private string searchKeyword = string.Empty;
        public string SearchKeyword
        {
            set
            {
                this.searchKeyword = value;
            }
            get
            {
                return this.searchKeyword;
            }
        }

        private string selectFolder = string.Empty;
        public string SelectedFolder
        {
            set
            {
                this.selectFolder = value;
            }
            get
            {
                return this.selectFolder;
            }
        }

        private bool isDiskLockCheck = false;
        private bool IsDiskLockCheck
        {
            set
            {
                this.isDiskLockCheck = value;
            }
            get
            {
                return this.isDiskLockCheck;
            }
        }

        private bool? isshowPanel = null;
        private bool? IsShowPanel
        {
            set
            {
                this.isshowPanel = value;
            }
            get
            {
                return this.isshowPanel;
            }
        }
        private bool IsShowPanelFixed
        {
            get
            {
                return (this.isshowPanel == null) ? false : (bool)this.isshowPanel;
            }
        }

        public static MainWindow GetInstance()
        {
            if (instance == null)
            {
                instance = new MainWindow();
            }
            return instance;
        }

        public MainWindow()
        {
            InitializeComponent();
            instance = this;

            App.Current.MainWindow.FontFamily = new System.Windows.Media.FontFamily("Meiryo UI, MS UI Gothic");
            App.Current.MainWindow.Title =
                Application.Current.TryFindResource("PG_TITLE") as string + " - non titled";
            Repair.Text = Application.Current.TryFindResource("LBL_COMFIRMATION_OF_UNLOCK_CHECK") as string;
            // Driveletter初期化
            InitDriveCombobox();
            // 録画モード ComboBox初期化
            //CreateFolder2CombBox();
            ProgressDiscSpace.Visibility = Visibility.Hidden;
            // イベント登録する
            DriveComboBox.SelectionChanged += new SelectionChangedEventHandler(
                                                                DriveComboBoxSelectionChanged);

            FolderComboBox.SelectionChanged += new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);
            RefreshDrive();
        }
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // システムイベント通知の追加
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            // 設定メニューを追加
            IntPtr menu = GetSystemMenu(new WindowInteropHelper(this).Handle, 0);
            AppendMenu(menu, MF_SEPARATOR, 0, null);
            AppendMenu(menu, 0, MENU_ABOUT, TryFindResource("MENU_ABOUT") as string);
            // upnp状態変化イベント通知の追加
            eventFinder.FindDeviceAsync();
            // ボタンに関するイベント通知の追加
            CopyButton.AddHandler(Button.MouseLeftButtonDownEvent,
                                new MouseButtonEventHandler(CopyButtonButtonDowned), true);
            CopyButton.AddHandler(Button.MouseLeftButtonUpEvent,
                                new MouseButtonEventHandler(CopyButtonButtonUpped), true);
            // キーボード操作に関するイベント通知の追加
            AddHandler(Control.KeyUpEvent, new KeyEventHandler(ListBox1KeyUp), true);
            // 時間表示用タイマー
            timer.Tick += OnTimerTick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            this.IpcServer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerTick(object sender, System.EventArgs e) {
            string realtime = View.DataConverter.StringConverter.StringConverter.
                                     GetRecordTimeStringConverter(DateTime.Now, false);
            if (RealTime.Text != realtime) {
                if (IsShowPanel != null && DriveComboBox != null) {
                    string drive = DriveComboBox.SelectedValue.ToString();
                    DataStruct.Drivemap dlnadrive = dlnaletter.Find(delegate (DataStruct.Drivemap bnumber) {
                        return bnumber.drive.Equals(drive);
                    });
                    showDiskSize(dlnadrive);
                    ShowInfoWindows(this.IsShowPanelFixed);
                }
                RealTime.Text = realtime;
                //dlnaletter.Clear();
                //eventFinder.CancelFindDeviceAsync();
                //eventFinder.AddUpnpDeviceNotifyEvent();
                //eventFinder.FindDeviceAsync();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            eventFinder.CancelFindDeviceAsync();
            CopyButton.RemoveHandler(Button.MouseLeftButtonDownEvent,
                                new MouseButtonEventHandler(CopyButtonButtonDowned));
            CopyButton.RemoveHandler(Button.MouseLeftButtonUpEvent,
                                new MouseButtonEventHandler(CopyButtonButtonUpped));
            RemoveHandler(Control.KeyUpEvent, new KeyEventHandler(ListBox1KeyUp));

            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.RemoveHook(new HwndSourceHook(WndProc));
            timer.Stop();
            timer.Tick -= OnTimerTick;
            base.OnClosing(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RaiseEvent(object sender, RoutedEventArgs e)
        {
            //返されたデータを取得し表示
            Debug.Print(e.ToString());
            ;
        }

        public void RaiseEvent(object sender, LabelInfoEventArgs e)
        {
            //返されたデータを取得し表示
            string title = e.GetTitle();
            if (title == string.Empty)
            {
                title = "non titled";
            }
            App.Current.MainWindow.Title = TryFindResource("PG_TITLE") as string + " - " + title;
            Debug.Print(e.ToString());
            ;
        }

        public void RaiseEvent(object sender, FolderInfoEventArgs e)
        {
            //返されたデータを取得し表示
            if (e.GetEventStatus() == EventStatus.ONSTART)
            {
                // 開始コード通知(「すべて」を先に追加)
                FolderComboBox.SelectionChanged -= new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);
                FolderComboBox.Items.Clear();
                FolderComboBox.Items.Add(TryFindResource("ALL") as string);
                FolderComboBox.SelectedIndex = 0;
                folder.Clear();
                FolderComboBox.SelectionChanged += new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);
            }
            else if (e.GetEventStatus() == EventStatus.ONDATA)
            {
                // リスト追加コード通知
                FolderComboBox.SelectionChanged -= new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);
                if (FolderComboBox.Items.Count == 1)
                {
                    FolderComboBox.Items.Add(TryFindResource("FOLDER_UNSET") as string);
                }
                FolderComboBox.Items.Add(e.GetTitle());
                FolderComboBox.SelectionChanged += new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);

            }
            else if (e.GetEventStatus() == EventStatus.ONEND)
            {
                // 完了コード通知
                ;
            }
            else if (e.GetEventStatus() == EventStatus.ONDATA_GENRE)
            {
                // リスト追加コード通知
                DataStruct.Folder first = folder.Find(delegate (DataStruct.Folder bnumber)
                {
                    return bnumber.folder.Equals(e.GetTitle());
                });
                if (first.folder == null)
                {
                    folder.Add(new DataStruct.Folder(e.GetTitle(), 0));
                }
            }
            else if (e.GetEventStatus() == EventStatus.ONEND_GENRE)
            {
                // 完了コード通知
                FolderComboBox.SelectionChanged -= new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);
                for (int i = 0; i < folder.Count; i++)
                {
                    FolderComboBox.Items.Add(folder[i].folder);
                }
                dlnaFileListbackup.Clear();
                dlnaFileListbackup.AddRange(dlnaFileList);
                FolderComboBox.SelectionChanged += new SelectionChangedEventHandler(
                                                                FolderComboBoxSelectionChanged);
            }
            else
            {
                ;
            }
        ;
        }

        private void SetValue(string param)
        {
            textCounter.Text = param;
            Thread.Sleep(10);
        }

        public void RaiseEvent(object sender, UpnpListInfoEventArgs e)
        {

            if (e.GetEventStatus() == EventStatus.ONSTART)
            {
                // 開始コード通知
                dlnaFileList.Clear();
                dlnaFileList = new List<Customer> { };
                Dispatcher.Invoke(DispatcherPriority.Normal
                                , new Action<string>(SetValue)
                                , string.Empty);
                ChangeDlnaList();
                ShowInfoWindows(true);
                DriveComboBox.IsEnabled = false;
                FolderComboBox.IsEnabled = false;           /// RecSearch, SearchDoneButton, ToggleLeftButton, ToggleRightButton
                CopyButton.IsEnabled = false;
            }
            else if (e.GetEventStatus() == EventStatus.ONEND)
            {
                // 終了コード通知
                ChangeDlnaList();
                HideInfoWindows();
                Dispatcher.Invoke(DispatcherPriority.Normal
                                , new Action<string>(SetValue)
                                , string.Empty);
                DriveComboBox.IsEnabled = true;
                if (IsDlnaItemMode())
                {
                    FolderComboBox.IsEnabled = true;        /// RecSearch, SearchDoneButton, ToggleLeftButton, ToggleRightButton
                    CopyButton.IsEnabled = true;
                }
            }
            else if (e.GetEventStatus() == EventStatus.ONDATA_FOLDER)
            {
                //
                Customer folderfirst = dlnaFileList.Find(delegate (Customer bnumber)
                {
                    return bnumber.Id == e.GetId();
                });
                if (folderfirst != null)
                {
                    return;
                }

                dlnaFileList.Add(new Customer()
                {
                    Productname = e.GetDeviceName(),
                    Filename = e.GetId(),
                    Title = e.GetTitle(),
                    SortRecordDatTime = StringConverter.GetRecordDatTimeStringConverter(
                                                            e.GetRecordDatTime(), true),
                    RecordDate = string.Empty,
                    RecordTime = string.Empty,
                    DurationTime = string.Empty,
                    DurationTimeNormal = string.Empty,
                    Description = string.Empty,
                    RecordMode = string.Empty,
                    ImageSource = e.bmp,
                    Status = string.Empty,
                    Folder = string.Empty,
                    Weeks = string.Empty,
                    Id = e.GetId(),
                    ParentId = e.GetParentId(),
                    Classes = e.GetClasses(),
                    serviceUrl = e.GetUPnPServiceUrl(),
                    Channel = e.GetChannel(),
                });
            }
            else if (e.GetEventStatus() == EventStatus.ONDATA_ITEM)
            {
                //
                dlnaFileList.Add(new Customer()
                {
                    Productname = e.GetDeviceName(),
                    Filename = e.GetId(),
                    Title = e.GetTitle(),
                    SortRecordDatTime = StringConverter.GetRecordDatTimeStringConverter(
                                                            e.GetRecordDatTime(), true),
                    RecordDate = StringConverter.GetRecordDateStringConverter(
                                                            e.GetRecordDatTime()),
                    RecordTime = StringConverter.GetRecordTimeStringConverter(
                                                            e.GetRecordDatTime(), false),
                    DurationTime = StringConverter.GetRecDurationTimeStringCoverter(
                                                            e.GetRecDurationTime(), false),
                    DurationTimeNormal = StringConverter.GetRecDurationTimeStringCoverter(
                                                            e.GetRecDurationTime(), true),
                    RecordMode = string.Empty,
                    Status = string.Empty,
                    Folder = string.Empty,
                    Weeks = (e.GetRecordDatTime() == null)
                            ? string.Empty : ((DateTime)e.GetRecordDatTime()).DayOfWeek.ToString().ToUpper(),
                    Genre = e.GetGenre(),
                    Id = e.GetId(),
                    Description = e.GetDescription(),
                    ParentId = e.GetParentId(),
                    Classes = e.GetClasses(),
                    ImageSource = e.bmp,
                    FullAddress = e.GetImageUri(),
                    serviceUrl = e.GetUPnPServiceUrl(),
                    Channel = e.GetChannel(),
                });
                Dispatcher.Invoke(DispatcherPriority.Normal
                                       , new Action<string>(SetValue)
                                       , StringConverter.GetNumOfListStringConverter2(
                                                       dlnaFileList.Count() - 1,
                                                       e.GetTotalCount())
                                 );
            }
        }

        public void RaiseEvent(object sender, UpnpDriveInfoEventArgs e)
        {
            if (e.GetEventStatus() == EventStatus.ONSTART)
            {
                // 開始コード通知
                eventFinder.DeleteUpnpDeviceNotifyEvent();
                dlnaletter.Clear();
            }
            else if (e.GetEventStatus() == EventStatus.ONDATA)
            {
                dlnaletter.Add(e.GetUpnpDriveInfo());

            }
            else if (e.GetEventStatus() == EventStatus.ONEND)
            {
                for (int i = 0; i < dlnaletter.Count; i++)
                {
                    if (DriveComboBox.Items.IndexOf(dlnaletter[i].drive) == -1)
                    {
                        DriveComboBox.Items.Add(dlnaletter[i].drive);
                    }
                }
                SetFooter();
                eventFinder.AddUpnpDeviceNotifyEvent();
            }
            else if (e.GetEventStatus() == EventStatus.ONADD_DEVICE)
            {
                if (dlnaletter.IndexOf(e.GetUpnpDriveInfo()) == -1)
                {
                    dlnaletter.Add(e.GetUpnpDriveInfo());
                    for (int i = 0; i < dlnaletter.Count; i++)
                    {
                        if (DriveComboBox.Items.IndexOf(dlnaletter[i].drive) == -1)
                        {
                            DriveComboBox.Items.Add(dlnaletter[i].drive);
                        }
                    }
                }
            }
            else if (e.GetEventStatus() == EventStatus.ONDEL_DEVICE)
            {
                for (int i = dlnaletter.Count - 1; i > 0; i--)
                {
                    if (dlnaletter[i].fileSystem == e.GetFileSystem())
                    {
                        DriveComboBox.Items.Remove(dlnaletter[i].drive);
                        dlnaletter.RemoveAt(i);
                        break;
                    }
                }

            }
            else if (e.GetEventStatus() == EventStatus.ONEND_DEVICE)
            {
                List<string> dlnaList = new List<string> { };
                for (int i = 0; i < dlnaletter.Count; i++)
                {
                    dlnaList.Add(dlnaletter[i].drive);
                }
                for (int i = DriveComboBox.Items.Count - 1; i > 0; i--)
                {
                    string x = DriveComboBox.Items[i].ToString();
                    if (dlnaList.IndexOf(DriveComboBox.Items[i].ToString()) == -1)
                    {
                        DriveComboBox.Items.RemoveAt(i);
                    }
                }

            }
            else if (e.GetEventStatus() == EventStatus.ON_CHKLIVETUNER)
            {
                // 特殊コード
                SetDlnaSupportLivetuner(e.GetDriveName(), DlnaSupport.CHKLIVETUNER);

            }
            else if (e.GetEventStatus() == EventStatus.ON_LIVETUNER)
            {
                // 特殊コード
                SetDlnaSupportLivetuner(e.GetDriveName(), DlnaSupport.LIVETUNER);
            }
            else
            {
            }
        }
        /// <summary>
        /// DLNAライブチューナの結果反映
        /// </summary>
        /// <param name="drive"></param>
        /// <param name="param"></param>
        private void SetDlnaSupportLivetuner(string drive, DlnaSupport param)
        {
            for (int i = 0; i < dlnaletter.Count; i++)
            {
                if (dlnaletter[i].drive.Equals(drive))
                {
                    DataStruct.Drivemap dmap = dlnaletter[i];
                    dmap.dlnaSupport |= param;
                    dlnaletter[i] = dmap;
                    Dispatcher.Invoke(DispatcherPriority.Normal
                                    , new Action<DlnaSupport>(SetValue)
                                    , dmap.dlnaSupport);
                    break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        private void SetValue(DlnaSupport param)
        {
            //this.DlnaLiveTuner.DataContext = new { LiveTunerSupport = (param & DlnaSupport.LIVETUNER) };
        }


        public void RaiseEvent(object sender, FileInfoEventArgs e)
        {
            //返されたデータを取得し表示
            if (e.GetEventStatus() == EventStatus.ONSTART)
            {
                // 開始コード通知
                ivdrFileList.Clear();
                ivdrFileList = new List<Customer> { };
                NumOfListTextBlock.Text = string.Empty;
                SearchKeyword = RecSearch.Text;
                SelectedFolder = (FolderComboBox.SelectedIndex > 0)
                                ? FolderComboBox.SelectedValue.ToString() : string.Empty;
                ShowInfoWindows(true);
                this.DataContext = new { IsDlnaMode = DlnaSupport.NODLNA, };
                DriveComboBox.IsEnabled = false;
                FolderComboBox.IsEnabled = false;       /// RecSearch, SearchDoneButton, ToggleLeftButton, ToggleRightButton

                IvdrDiscSpace1.Visibility = Visibility.Hidden;
                LabelIvdrTotalSize1.Text = string.Empty;
                LabelIvdrFreeSize1.Text = string.Empty;
                PanelCloseButton.Visibility = Visibility.Hidden;

                IvdrDiscSpace2.Visibility = Visibility.Hidden;
                LabelIvdrTotalSize2.Text = string.Empty;
                LabelIvdrFreeSize2.Text = string.Empty;

            }
            else if (e.GetEventStatus() == EventStatus.ONEND)
            {
                // 終了コード通知
                // タイトル順ソート
                ivdrFileList.Sort(delegate (Customer mca1, Customer mca2)
                {
                    return string.Compare(mca1.Title, mca2.Title);
                });
                // 日付順逆ソート
                if (ToggleRightButton.IsChecked == true)
                {
                    ivdrFileList.Sort(delegate (Customer mca1, Customer mca2)
                    {
                        return string.Compare(mca2.SortRecordDatTime, mca1.SortRecordDatTime);
                    });
                }
                NumOfListTextBlock.Text = StringConverter.GetNumOfListStringConverter(ivdrFileList.Count);
                ChangeIvdr();
                DriveComboBox.IsEnabled = true;
                FolderComboBox.IsEnabled = true;        /// RecSearch, SearchDoneButton, ToggleLeftButton, ToggleRightButton
                return;
            }
            else
            {
                // リスト追加コード通知
                string folderTitle = string.Empty;
                if (SearchKeyword != string.Empty)
                {
                    if (!LanguageConverter.GetInstance().IsSearchString(e.GetTitle(), string.Empty, SearchKeyword))
                    {
                        return;
                    }
                }
                if (SelectedFolder != string.Empty)
                {
                    string folderunset = TryFindResource("FOLDER_UNSET") as string;

                    if (SelectedFolder == folderunset)
                    {
                        DataStruct.Folder folderfirst = folder.Find(delegate (DataStruct.Folder bnumber)
                        {
                            return bnumber.fileNumber == e.GetPrimaryKey();
                        });
                        if (folderfirst.folder != null)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (folder.IndexOf(new DataStruct.Folder(
                                        SelectedFolder, e.GetPrimaryKey())) == -1)
                        {
                            return;
                        }
                        folderTitle = SelectedFolder;
                    }
                }
                else
                {
                    DataStruct.Folder first = folder.Find(delegate (DataStruct.Folder bnumber)
                    {
                        return bnumber.fileNumber.Equals(e.GetPrimaryKey());
                    });
                    folderTitle = first.folder;
                }
                string dummy = e.GetRecordDatTime().ToString();
                //loadimage.
                ivdrFileList.Add(new Customer()
                {
                    Filename = e.GetFileName(),
                    Title = e.GetTitle(),
                    SortRecordDatTime = StringConverter.GetRecordDatTimeStringConverter(
                                                            e.GetRecordDatTime(), true),
                    RecordDate = StringConverter.GetRecordDateStringConverter(
                                                            e.GetRecordDatTime()),
                    RecordTime = StringConverter.GetRecordTimeStringConverter(
                                                            e.GetRecordDatTime(), false),
                    DurationTime = StringConverter.GetRecDurationTimeStringCoverter(
                                                            e.GetRecDurationTime(), false),
                    DurationTimeNormal = StringConverter.GetRecDurationTimeStringCoverter(
                                                            e.GetRecDurationTime(), true),
                    RecordMode = e.GetRecordMode().ToString(),
                    ImageSource = e.bmp,
                    Status = e.GetStatus(),
                    Folder = folderTitle,
                    Weeks = e.GetRecordDatTime().DayOfWeek.ToString().ToUpper(),
                    Id = string.Empty,
                    ParentId = string.Empty,
                    serviceUrl = string.Empty
                });
            }
            Debug.Print(e.ToString());
        }

        // HYperLink クリック
        private void DlnaInfoOnClick(object sender, RoutedEventArgs e)
        {
            if (IsShowPanel == null)
            {
                string drive = DriveComboBox.SelectedValue.ToString();
                DataStruct.Drivemap dlnadrive = dlnaletter.Find(delegate (DataStruct.Drivemap bnumber)
                {
                    return bnumber.drive.Equals(drive);
                });
                showDiskSize(dlnadrive);
                ShowInfoWindows(false);
            }
            else if (IsShowPanel == false)
            {
                HideInfoWindows();
            }
        }

        // HYperLink クリック
        private void PresentationOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Presentation.Text);
            }
            catch
            {
                ;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="waitmode"></param>
        private void ShowInfoWindows(bool waitmode)
        {
            PanelWait.Visibility = Visibility.Visible;
            BlurEffect be = new BlurEffect();
            be.Radius = 7;
            be.KernelType = KernelType.Gaussian;
            listBox1.Effect = be;
            listBox3.Effect = be;
            if (waitmode)
            {
                ProgressWait.Visibility = Visibility.Visible;
                WaitText.Visibility = Visibility.Visible;
                IsShowPanel = true;
            }
            else
            {
                ProgressWait.Visibility = Visibility.Hidden;
                WaitText.Visibility = Visibility.Hidden;
                IsShowPanel = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HideInfoWindows()
        {
            BlurEffect be = new BlurEffect();
            be.Radius = 0;
            be.KernelType = KernelType.Gaussian;
            listBox1.Effect = be;
            listBox3.Effect = be;
            PanelWait.Visibility = Visibility.Hidden;
            IsShowPanel = null;
        }

        private void OnRepairClicked(object sender, RoutedEventArgs e) {
            if (Mouse.OverrideCursor == Cursors.Wait) {
                return;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            //管理者として自分自身を起動する
            prs = new System.Diagnostics.Process();
            ProcessStartInfo psi = prs.StartInfo;
            //ShellExecuteを使う。デフォルトtrueなので、必要はない。
            psi.UseShellExecute = true;
            //自分自身のパスを設定する
            psi.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
            //動詞に「runas」をつける
            psi.Verb = "runas";
            //引数
            psi.Arguments = "/chkdsk " + DriveComboBox.Text;
            try
            {
                prs.EnableRaisingEvents = true;
                prs.Exited += (sdr, ev) => {
                    prs.Close();
                    Dispatcher.InvokeAsync((Action)(() => {
                        RefreshIvdrDrive();
                        Mouse.OverrideCursor = null;
                    }));
                };
                //起動する
                prs.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                //「ユーザーアカウント制御」ダイアログでキャンセルされたなどによって
                //起動できなかった時
                Console.WriteLine("起動しませんでした: " + ex.Message);
                Mouse.OverrideCursor = null;
            }
        }
 
        public void ChangeDlnaList()
        {

            if ((dlnaFileList.Count == 1 && dlnaFileList[0].RecordDate == string.Empty)
              || (dlnaFileList.Count > 1 && dlnaFileList[1].RecordDate == string.Empty))
            {
                if (this.ExecMode != EXEC_MODE.DLNA_CONTENTS)
                {
                    this.ExecMode = EXEC_MODE.DLNA_CONTENTS;
                    this.Panel1.Visibility = Visibility.Hidden;
                    this.Panel3.Visibility = Visibility.Visible;
                    this.NumOfList.Text = string.Empty;
                    listBox3.MouseDoubleClick += new MouseButtonEventHandler(
                                                                    ListBox1SelectionChanged);
                }
                dlnaFileListbackup.AddRange(dlnaFileList);
                this.listBox3.DataContext = dlnaFileList;
                this.listBox3.SelectedIndex = -1;
                this.RecSearch.Text = string.Empty;

            }
            else if (dlnaFileList.Count == 1
                  || (dlnaFileList.Count > 1 && dlnaFileList[1].RecordDate != string.Empty))
            {
                for (int iLoop = 0; iLoop < dlnaFileList.Count; iLoop++)
                {
                    dlnaFileList[iLoop].ImageSource = (System.Windows.Media.Imaging.BitmapSource)
                                        WebAccessManager.LoadImageFromURL(dlnaFileList[iLoop].FullAddress);

                }
                if (dlnaFileList.Count > 1 && dlnaFileList[1].ClassStartsWith.Equals("object.item"))
                {
                    // バックアップ
                    dlnaFileListbackup.Clear();
                    dlnaFileListbackup.AddRange(dlnaFileList);
                    // ..を取り除く
                    dlnaFileList.Remove(dlnaFileListbackup[0]);
                    // タイトル順ソート
                    dlnaFileList.Sort(delegate (Customer mca1, Customer mca2)
                    {
                        return string.Compare(mca1.Title, mca2.Title);
                    });
                    // 日付順逆ソート
                    if (ToggleRightButton.IsChecked == true)
                    {
                        dlnaFileList.Sort(delegate (Customer mca1, Customer mca2)
                        {
                            return string.Compare(mca2.SortRecordDatTime, mca1.SortRecordDatTime);
                        });
                    }
                    // ..追加
                    dlnaFileList.Insert(0, dlnaFileListbackup[0]);
                }
                if (this.ExecMode != EXEC_MODE.DLNA)
                {
                    this.ExecMode = EXEC_MODE.DLNA;
                    this.Panel1.Visibility = Visibility.Hidden;
                    this.Panel3.Visibility = Visibility.Visible;
                    this.NumOfList.Text = StringConverter.GetNumOfListStringConverter(dlnaFileList.Count - 1);
                    ;
                    listBox3.MouseDoubleClick += new MouseButtonEventHandler(
                                                                    ListBox1SelectionChanged);
                }
                this.listBox3.DataContext = dlnaFileList;
                this.listBox3.SelectedIndex = -1;
                this.RecSearch.Text = string.Empty;
            }
            // デバイスが失われている場合、処理を中止
            if (DriveComboBox.SelectedIndex == -1)
            {
                return;
            }
            string drive = DriveComboBox.SelectedValue.ToString();
            DataStruct.Drivemap dlnafirst = dlnaletter.Find(delegate (DataStruct.Drivemap bnumber)
            {
                return bnumber.drive.Equals(drive);
            });
            if (dlnafirst.drive != null)
            {
                // dlnaモード
                App.Current.MainWindow.Title =
                        TryFindResource("PG_TITLE") as string + " - " + dlnafirst.drive;
                DriveMode.Visibility = System.Windows.Visibility.Hidden;
                DlnaMode.Visibility = System.Windows.Visibility.Visible;
                PanelCloseButton.Visibility = Visibility.Visible;
                DlnaIcon.Source = WebAccessManager.LoadImageFromURL(dlnafirst.dlnaImage);
                showDiskSize(dlnafirst);
                //
                Presentation.Text = dlnafirst.presentationURL;
                DlnaInfo.Text = dlnafirst.drive;
                this.DataContext = new
                {
                    IsDlnaMode = dlnafirst.dlnaSupport,
                    Recording = dlnafirst.scheduledRecording.ToString(),
                    DlnaAvUploadSupport = dlnafirst.dlnaSupport & DlnaSupport.DLNACAPAVUPLOAD,
                    DlnaDtcpMoveSupport = dlnafirst.dlnaSupport & DlnaSupport.DLNACAPDTCPMOVE,
                    DlnaDtcpCopySupport = dlnafirst.dlnaSupport & DlnaSupport.DLNACAPDTCPCOPY,
                    JLabsMoveSupport = dlnafirst.dlnaSupport & DlnaSupport.JLABSCAPMOVE,
                    JLabsUploadRecSupport = dlnafirst.dlnaSupport & DlnaSupport.JLABSCAPUPLOADREC,
                    SptvMoveSupport = dlnafirst.dlnaSupport & DlnaSupport.SPTVCAPMOVE,
                    SptvRecSupport = dlnafirst.dlnaSupport & DlnaSupport.SPTVCAPREC,
                    RegzaRecSupport = dlnafirst.dlnaSupport & DlnaSupport.REGZACAPMOVE,
                    ModelName = dlnafirst.modelName,
                    ModelNumber = dlnafirst.modelNumber,
                    ImageDlna = WebAccessManager.LoadImageFromURL(dlnafirst.dlnaImage),
                    Manufacturer = dlnafirst.manufacturerName,
                    Modelname = dlnafirst.modelName
                };
                //this.DlnaLiveTuner.DataContext = new { LiveTunerSupport = (dlnafirst.dlnaSupport & DlnaSupport.LIVETUNER) };
            }
            Thread.Sleep(10);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dlnaname"></param>
        private void showDiskSize(DataStruct.Drivemap dlnaname)
        {
            List<DataStruct.IvdrxSize> disksSizes = new List<DataStruct.IvdrxSize> { };
            if (!dlnaname.ivdrControlUrl.Equals(string.Empty))
            {
                disksSizes = uPnpAccessManager.GetInstance().GetDlnaDisksSizes(
                                                            dlnaname.ivdrControlUrl,
                                                            EXEC_MODE.IVDR);
            }
            else if ((dlnaname.dlnaSupport & (DlnaSupport.JLABSCAPUPLOADREC
                                              | DlnaSupport.JLABSCAPMOVE | DlnaSupport.SPTVCAPMOVE
                                              | DlnaSupport.SPTVCAPREC)) != DlnaSupport.NONE)
            {
                // X_HDLnkGetRecordDestinationInfo
                disksSizes = uPnpAccessManager.GetInstance().GetDlnaDisksSizes(
                                                            dlnaname.cdirControlUrl,
                                                            EXEC_MODE.DLNA);
            }
            if (disksSizes.Count > 0)
            {
                IvdrDiscSpace1.Visibility = Visibility.Visible;
                IvdrDiscSpace1.Value = disksSizes[0].parsize;
                LabelIvdrTotalSize1.Text = disksSizes[0].name + " " + string.Format(
                                        TryFindResource("SIZE_OF_DISC") as string,
                                        (decimal)((uint)(disksSizes[0].totalsize / (1000 * 1000 * 100))) / 10);
                Iv1RecTime_comp_non.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[0].totalsize / (1000 * 1000 * 10)) / 764)));
                Iv1RecTime_comp_4th.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[0].totalsize / (1000 * 1000 * 10)) / 271)));
                LabelIvdrFreeSize1.Text = string.Format(
                                        TryFindResource("FREE_SPACE") as string,
                                        (decimal)((uint)(disksSizes[0].freesize / (1000 * 1000 * 100))) / 10);
                Iv1FreeTime_comp_non.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[0].freesize / (1000 * 1000 * 10)) / 764)));
                Iv1FreeTime_comp_4th.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[0].freesize / (1000 * 1000 * 10)) / 271)));
            }
            else
            {
                IvdrDiscSpace1.Visibility = Visibility.Hidden;
                LabelIvdrTotalSize1.Text = string.Empty;
                LabelIvdrFreeSize1.Text = string.Empty;
            }
            if (disksSizes.Count > 1)
            {
                IvdrDiscSpace2.Visibility = Visibility.Visible;
                IvdrDiscSpace2.Value = disksSizes[1].parsize;
                LabelIvdrTotalSize2.Text = disksSizes[1].name + " " + string.Format(
                                        TryFindResource("SIZE_OF_DISC") as string,
                                        (decimal)((uint)(disksSizes[1].totalsize / (1000 * 1000 * 100))) / 10);
                Iv2RecTime_comp_non.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[1].totalsize / (1000 * 1000 * 10)) / 764)));
                Iv2RecTime_comp_4th.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[1].totalsize / (1000 * 1000 * 10)) / 271)));
                LabelIvdrFreeSize2.Text = string.Format(
                                        TryFindResource("FREE_SPACE") as string,
                                        (decimal)((uint)(disksSizes[1].freesize / (1000 * 1000 * 100))) / 10);
                Iv2FreeTime_comp_non.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[1].freesize / (1000 * 1000 * 10)) / 764)));
                Iv2FreeTime_comp_4th.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((disksSizes[1].freesize / (1000 * 1000 * 10)) / 271)));
            }
            else
            {
                IvdrDiscSpace2.Visibility = Visibility.Hidden;
                LabelIvdrTotalSize2.Text = string.Empty;
                LabelIvdrFreeSize2.Text = string.Empty;
            }
        }

        public void ChangeIvdr()
        {
            if (this.ExecMode != EXEC_MODE.IVDR)
            {
                this.ExecMode = EXEC_MODE.IVDR;
                this.Panel1.Visibility = Visibility.Visible;
                this.Panel3.Visibility = Visibility.Hidden;
                listBox3.MouseDoubleClick -= new MouseButtonEventHandler(
                                                                ListBox1SelectionChanged);
            }
            HideInfoWindows();
            this.listBox1.DataContext = ivdrFileList;
            this.listBox1.SelectedIndex = -1;
            Thread.Sleep(10);
        }
        private bool IsDlnaItemMode()
        {
            bool isdlnaDlnaMode = false;
            for (int iCount = 0; dlnaFileList.Count > iCount; iCount++)
            {
                if (dlnaFileList[iCount].ClassStartsWith.Equals("object.item"))
                {
                    isdlnaDlnaMode = true;
                    break;
                }
            }
            return isdlnaDlnaMode;
        }

        /// <summary>
        /// フォルダコンボボックスの要素追加
        /// </summary>
        private void CreateFolder2CombBox()
        {
            // iVDRアダプターが存在しない場合
            if (DriveComboBox.SelectedIndex == -1)
            {
                return;
            }
            string drive = DriveComboBox.SelectedValue.ToString();
            // フォルダ情報取得
            folder = IvdrAccessManager.GetInstance().GetIvdrFolderList(drive);
        }

        /// <summary>
        /// ivdrリスト情報取得
        /// </summary>
        private void ReLoadIvdrList()
        {
            string drive = DriveComboBox.SelectedValue.ToString();
            RefreshIvdrDrive(true);
            // ラベル取得
            string title = IvdrAccessManager.GetInstance().GetIvdrLabel(drive);
            // ファイル情報取得
            IvdrAccessManager.GetInstance().GetIvdrList(drive);
        }

        /// <summary>
        /// Ivdrモードのドライブ情報取得
        /// </summary>
        private void RefreshIvdrDrive()
        {
            RefreshIvdrDrive(false);
        }
        /// <summary>
        /// Ivdrモードのドライブ情報取得
        /// </summary>
        private void RefreshIvdrDrive(bool LockCheckOn)
        {
            if (DriveComboBox.Items.Count == 0)
            {
                this.listBox1.DataContext = ivdrFileList;
                this.listBox1.SelectedIndex = -1;
                ProgressDiscSpace.Value = 0;
                LabelFreeDisc.Text = string.Empty;
                return;
            }
            string drive = DriveComboBox.SelectedValue.ToString();
            string folderTitle = string.Empty;
            DriveMode.Visibility = System.Windows.Visibility.Visible;
            DlnaMode.Visibility = System.Windows.Visibility.Hidden;
            DriveInfo cdrive = new DriveInfo(drive);
            DataStruct.Drivemap driveinfo = driveletter.Find(delegate (DataStruct.Drivemap bnumber)
            {
                return bnumber.drive.Equals(drive);
            });
            try
            {
                ProgressDiscSpace.Value = ((double)cdrive.TotalFreeSpace / (double)cdrive.TotalSize) * 100;
                LabelSizeOfDisc.Text = string.Format(
                                        TryFindResource("SIZE_OF_DISC") as string,
                                        (decimal)((uint)(cdrive.TotalSize / (1000 * 1000 * 100))) / 10);
                ProgressDiscSpace.Visibility = Visibility.Visible;
                RecSearch.Visibility = Visibility.Visible;
                SearchDoneButton.Visibility = Visibility.Visible;
                FolderComboBox.Visibility = Visibility.Visible;
                LabelSizeOfDisc.Visibility = Visibility.Visible;
                NumOfListTextBlock.Visibility = Visibility.Visible;
                // ivdrの残量が0か、ロック判定された場合
                if (cdrive.TotalFreeSpace == 0 && LockCheckOn == true)
                {
                    LabelFreeDisc.Visibility = Visibility.Hidden;
                    LabelRepairDisk.Visibility = Visibility.Visible;
                    SmallSHIELD.Visibility = Visibility.Visible;
                    SmallSHIELD.Source = ImageAccessManager.GetSmallSHIELDIcon();

                    DriveModelTitle.Text = TryFindResource("LBL_DLNA_MODELNAME") as string;
                    DriveModelName.Text = driveinfo.manufacturerName + " " + driveinfo.modelName;
                    DriveModelNumberTitle.Text = TryFindResource("LBL_DLNA_MODELNUMBER") as string;

                    return;
                }
                DriveModelTitle.Text = TryFindResource("LBL_DLNA_MODELNAME") as string;
                DriveModelName.Text = driveinfo.manufacturerName + " " + driveinfo.modelName;
                DriveModelNumberTitle.Text = TryFindResource("LBL_DLNA_MODELNUMBER") as string;
                uint revnumber = 0;
                if (uint.TryParse(driveinfo.modelNumber, out revnumber))
                {
                    ;
                }
                // RHDM-UシリーズのF/Wが古い場合
                if (DriveModelName.Text == "I-O DATA RHDM-U" && (revnumber < 1918)) {
                    DriveModelNumber.Text = driveinfo.modelNumber +
                                            TryFindResource("SC_REQUIRING_UPDATE") as string;
                    DriveModelNumber.Foreground = System.Windows.Media.Brushes.Red;
                } else {
                    DriveModelNumber.Text = driveinfo.modelNumber;
                    DriveModelNumber.Foreground = System.Windows.Media.Brushes.DarkBlue;
                }

                LabelFreeDisc.Visibility = Visibility.Visible;
                LabelRepairDisk.Visibility = Visibility.Hidden;
                SmallSHIELD.Visibility = Visibility.Hidden;
                LabelFreeDisc.Text = string.Format(
                                    TryFindResource("FREE_SPACE") as string,
                                    (decimal)((uint)(cdrive.TotalFreeSpace / (1000 * 1000 * 100))) / 10);

                DvRecTime_comp_non.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((cdrive.TotalSize / (1000 * 1000 * 10)) / 764)));
                DvRecTime_comp_4th.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((cdrive.TotalSize / (1000 * 1000 * 10)) / 271)));
                DvFreeTime_comp_non.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((cdrive.TotalFreeSpace / (1000 * 1000 * 10)) / 764)));
                DvFreeTime_comp_4th.Text = string.Format(TryFindResource("SIZE_OF_TIME") as string,
                                        ((uint)((cdrive.TotalFreeSpace / (1000 * 1000 * 10)) / 271)));
            } catch (Exception) {
                ProgressDiscSpace.Visibility = Visibility.Hidden;
                RecSearch.Visibility = Visibility.Hidden;
                SearchDoneButton.Visibility = Visibility.Hidden;
                FolderComboBox.Visibility = Visibility.Hidden;
                LabelSizeOfDisc.Visibility = Visibility.Hidden;
                LabelFreeDisc.Visibility = Visibility.Hidden;
                LabelRepairDisk.Visibility = Visibility.Hidden;
                SmallSHIELD.Visibility = Visibility.Hidden;
                NumOfListTextBlock.Visibility = Visibility.Hidden;
                return;
            }
        }

        private void SearchDoneButtonClicked(object sender, RoutedEventArgs e)
        {
            FolderComboBoxSelect(FolderComboBox.Text);
            e.Handled = true;
        }

        private void DriveComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DriveComboBox.SelectedIndex == -1)
            {
                return;
            }
            RaiseEvent(this, new FolderInfoEventArgs(EventStatus.ONSTART));
            string drive = e.AddedItems[0].ToString();
            DataStruct.Drivemap drivefirst = driveletter.Find(delegate (DataStruct.Drivemap bnumber)
            {
                return bnumber.drive.Equals(drive);
            });
            DataStruct.Drivemap dlnafirst = dlnaletter.Find(delegate (DataStruct.Drivemap bnumber)
            {
                return bnumber.drive.Equals(drive);
            });
            if (drivefirst.drive != null)
            {
                CreateFolder2CombBox();
                ReLoadIvdrList();
                e.Handled = true;
            }
            else if (dlnafirst.drive != null)
            {
                uPnpAccessManager.GetInstance().GetUpnpRecordListCdirUrl(
                                    dlnafirst.drive, dlnafirst.cdirControlUrl, dlnafirst.dlnaSupport);
                e.Handled = true;
            }
        }

        private void FolderComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            FolderComboBoxSelect(e.AddedItems[0].ToString());
            e.Handled = true;
        }

        private void RecSearchKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FolderComboBoxSelect(FolderComboBox.Text);
                e.Handled = true;
            }
        }

        private void ListBox1SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            Customer id = (Customer)(((ListBox)sender).SelectedItem);
            if (id != null)
            {
                if (!id.ClassStartsWith.Equals("object.container"))
                {
                    return;
                }
                RaiseEvent(this, new FolderInfoEventArgs(EventStatus.ONSTART));
                uPnpAccessManager.GetInstance().UpnpRecordContainerUrlInvoke(
                                                id.Productname, id.serviceUrl, id.Id, id.ParentId, id.Title);
                e.Handled = true;
            }
        }

        private void ListBox1KeyUp(object sender, KeyEventArgs e)
        {
            Customer id = null;
            if (e.Key != Key.Back || RecSearch.IsFocused == true)
            {
                return;
            }
            if (listBox3.Visibility == Visibility.Visible && listBox3.Items.Count > 0)
            {
                id = (Customer)listBox3.Items[0];
                if (id.Title != "..")
                {
                    return;
                }
            }
            RaiseEvent(this, new FolderInfoEventArgs(EventStatus.ONSTART));
            uPnpAccessManager.GetInstance().UpnpRecordContainerUrlInvoke(
                                                id.Productname, id.serviceUrl, id.Id, id.ParentId, id.Title);
            e.Handled = true;
        }

        private void ListBox2SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            Customer id = (Customer)(((ListBox)sender).SelectedItem);
            if (id != null)
            {
                RaiseEvent(this, new FolderInfoEventArgs(EventStatus.ONSTART));
                uPnpAccessManager.GetInstance().UpnpRecordContainerUrlInvoke(
                                                id.Productname, id.serviceUrl, id.Id, id.ParentId, id.Title);
                e.Handled = true;
            }
        }

        private void FolderComboBoxSelect(string selectItem)
        {
            string drive = DriveComboBox.SelectedValue.ToString();
            DataStruct.Drivemap drivefirst = driveletter.Find(delegate (DataStruct.Drivemap bnumber)
            {
                return bnumber.drive.Equals(drive);
            });
            DataStruct.Drivemap dlnafirst = dlnaletter.Find(delegate (DataStruct.Drivemap bnumber)
            {
                return bnumber.drive.Equals(drive);
            });
            if (drivefirst.drive != null)
            {
                ReLoadIvdrList();
            }
            else if (dlnafirst.drive != null)
            {
                if (dlnaFileListbackup.Count == 0)
                {
                    return;
                }
                // 絞って返す
                this.listBox3.DataContext = null;
                this.listBox3.SelectedIndex = -1;
                Thread.Sleep(10);
                dlnaFileList.Clear();
                foreach (Customer dlnaFile in dlnaFileListbackup)
                {
                    if (dlnaFile.ClassStartsWith.Equals("object.container"))
                    {
                        continue;
                    }
                    else if (dlnaFile.Genre == null)
                    {
                        return;
                    }
                    else if (dlnaFile.Genre.IndexOf(selectItem) >= 0
                           || FolderComboBox.SelectedIndex == 0)
                    {
                        string folderTitle = string.Empty;
                        if (LanguageConverter.GetInstance().IsSearchString(
                                        dlnaFile.Title, dlnaFile.Description, RecSearch.Text))
                        {
                            dlnaFileList.Add(dlnaFile);
                        }
                    }
                }
                // タイトル順ソート
                dlnaFileList.Sort(delegate (Customer mca1, Customer mca2)
                {
                    return string.Compare(mca1.Title, mca2.Title);
                });
                // 日付順逆ソート
                if (ToggleRightButton.IsChecked == true)
                {
                    dlnaFileList.Sort(delegate (Customer mca1, Customer mca2)
                    {
                        return string.Compare(mca2.SortRecordDatTime, mca1.SortRecordDatTime);
                    });
                }
                // コンテナ追加
                for (int i = 0; dlnaFileListbackup.Count > i; i++)
                {
                    if (dlnaFileListbackup[i].ClassStartsWith.Equals("object.container"))
                    {
                        dlnaFileList.Insert(i, dlnaFileListbackup[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                this.NumOfList.Text = StringConverter.GetNumOfListStringConverter(dlnaFileList.Count - 1);
                this.listBox3.DataContext = dlnaFileList;
                this.listBox3.SelectedIndex = -1;
            }
        }

        private void PlayButtonClicked(object sender, RoutedEventArgs e)
        {

            //           Customer item = (Customer)listBox1.SelectedItem;
            //           string path = driveletter[0] + "\\iVDR_TVR\\" + item.Filename;
            //           Process.Start("C:\\Program Files\\VideoLAN\\VLC\\vlc.exe", path);
            // ファイル情報取得
            string path = "C:\\Users\\serina\\Desktop\\PROG0027.AVS.m2ts";
            //            string path = "C:\\Users\\serina\\Desktop\\PROG0035.m2ts";
            BinaryReader fs = null;
            fs = null;
            long count = 0;
            long a;
            long b;
            byte c;
            byte d;
            byte[] pud = new byte[0xb6];
            try
            {
                fs = new BinaryReader(
                           File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                for (; ; )
                {
                    try
                    {
                        a = fs.ReadUInt32();
                        b = fs.ReadUInt32();
                        c = fs.ReadByte();
                        d = fs.ReadByte();
                        if (d == 0x7f)
                        {
                            count++;
                        }
                        pud = fs.ReadBytes(pud.Length);
                    }
                    catch (System.IO.EndOfStreamException)
                    {
                        fs.Close();
                        fs = null;
                        break;
                    }
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
            }
            //MessageBox(
        }

        private void RefreshDrive()
        {
            System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += (s, evt) =>
            {
                Thread.Sleep(100);
            };
            // スレッド終了時処理
            bw.RunWorkerCompleted += (s, evt) =>
            {
                InitDriveCombobox();
            };
            // 別スレッドを生成し実行
            bw.RunWorkerAsync();
        }

        private void InitDriveCombobox()
        {
            DriveComboBox.IsEnabled = false;
            DriveComboBox.Items.Clear();
            // Driveletter初期化
            // VID_04BB&PID_0A11='I-O_DATA/RHDM-U'  IODATA RHDM-U series
            // VID_04BB&PID_0A14='I-O_DATA/RHDM-UT' IODATA RHDM-UT series
            // VID_152D&PID_0539='JMicron/JMS539'   Maxell M-VDRS-ADP
            // VID_152D&PID_2339='JMicron/JM20339'  HGST   iP1000zx-ADP
            driveletter = Manager.DriveManager.GetUsbStorName(
                                "VID_04BB&PID_0A11,VID_04BB&PID_0A14,VID_152D&PID_0539,VID_152D&PID_2339");
            //dlnaletter = Manager.DriveManager.GetDlnaName();
            // Drive ComboBox初期化
            for (int i = 0; i < driveletter.Count; i++)
            {
                DriveComboBox.Items.Add(driveletter[i].drive);
            }
            SetFooter();
            // 時間差で取得
            uPnpAccessManager.GetInstance().UpnpDlnaNameListInvoke();
        }

        private void SetFooter()
        {
            if (DriveComboBox.Items.Count > 0)
            {
                ProgressDiscSpace.Visibility = Visibility.Visible;
                RecSearch.Visibility = Visibility.Visible;
                SearchDoneButton.Visibility = Visibility.Visible;
                FolderComboBox.Visibility = Visibility.Visible;
                LabelSizeOfDisc.Visibility = Visibility.Visible;
                LabelFreeDisc.Visibility = Visibility.Visible;
                NumOfListTextBlock.Visibility = Visibility.Visible;
                DriveComboBox.SelectedIndex = 0;
            }
            else
            {
                ProgressDiscSpace.Visibility = Visibility.Hidden;
                RecSearch.Visibility = Visibility.Hidden;
                SearchDoneButton.Visibility = Visibility.Hidden;
                FolderComboBox.Visibility = Visibility.Hidden;
                LabelSizeOfDisc.Visibility = Visibility.Hidden;
                LabelFreeDisc.Visibility = Visibility.Hidden;
                NumOfListTextBlock.Visibility = Visibility.Hidden;
            }
        }
        private void CopyButtonButtonDowned(object sender, RoutedEventArgs e)
        {
            if (bwcopy != null)
            {
                return;
            }
            bwcopy = new System.ComponentModel.BackgroundWorker();
            CopyButtonLongClicked = false;
            bwcopy.DoWork += (s, evt) =>
            {
                Thread.Sleep(1000);
            };
            // スレッド終了時処理
            bwcopy.RunWorkerCompleted += (s, evt) =>
            {
                if (CopyButtonLongClicked != null)
                {
                    CopyButton.Content = TryFindResource("LBL_COPYBUTTON_SAVEMODE");
                    CopyButtonLongClicked = true;
                }
                bwcopy = null;
            };
            // 別スレッドを生成し実行
            bwcopy.RunWorkerAsync();
        }

        private void CopyButtonButtonUpped(object sender, RoutedEventArgs e)
        {
            CopyButtonLongClicked = null;
            if (CopyButton.Content != TryFindResource("LBL_COPYBUTTON_COPYMODE"))
            {
                CopyButton.Content = TryFindResource("LBL_COPYBUTTON_COPYMODE");
            }
        }

        private void PanelCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            if (IsShowPanel == false)
            {
                HideInfoWindows();
            }
            else
            {
                uPnpAccessManager.GetInstance().CanceledUpnpListInvoke();
            }
        }

        private void CopyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (CopyButton.Content == TryFindResource("LBL_COPYBUTTON_SAVEMODE"))
            {
                if (ExecMode == EXEC_MODE.IVDR)
                {
                    Manager.ClipBoardManager.SaveClipBoardIvdr(ivdrFileList);
                }
                else if (ExecMode == EXEC_MODE.DLNA)
                {
                    Manager.ClipBoardManager.SaveClipBoardDlna(dlnaFileList);
                }
            }
            else
            {
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                source.RemoveHook(new HwndSourceHook(WndProc));
                if (ExecMode == EXEC_MODE.IVDR)
                {
                    Manager.ClipBoardManager.SetClipBoardIvdr(ivdrFileList);
                }
                else if (ExecMode == EXEC_MODE.DLNA)
                {
                    Manager.ClipBoardManager.SetClipBoardDlna(dlnaFileList);
                }
                else if (ExecMode == EXEC_MODE.DLNA_CONTENTS)
                {
                    Manager.ClipBoardManager.SetClipBoardDlna(dlnaFileList);
                }
                source.AddHook(new HwndSourceHook(WndProc));
            }
        }

        private void ToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (listBox3 != null && listBox3.Items.Count > 0 && listBox3.Visibility == Visibility.Visible)
            {
                FolderComboBoxSelect(FolderComboBox.Text);
            }
            else if (listBox1 != null && listBox1.Items.Count > 0 && listBox1.Visibility == Visibility.Visible)
            {
                FolderComboBoxSelect(FolderComboBox.Text);
            }
            e.Handled = true;
        }

        private static void OpenAboutDialog()
        {
            ViewAbout wnd = new ViewAbout();
            wnd.Owner = Window.GetWindow(GetInstance());
            wnd.ShowDialog();
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg,
                                      IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (uint)WM.DEVICECHANGE)
            {
                if ((DBT)wParam == DBT.DEVICEARRIVAL)
                {
                    instance.GetType().InvokeMember("RefreshDrive",
                                        System.Reflection.BindingFlags.InvokeMethod |
                                        System.Reflection.BindingFlags.NonPublic |
                                        System.Reflection.BindingFlags.Instance,
                                        null,
                                        instance,
                                        null);
                    handled = true;
                }
            }
            if (msg == (uint)WM.SYSCOMMAND)
            {
                if (wParam.ToInt32() == MENU_ABOUT)
                {
                    OpenAboutDialog();
                }
            }
            return IntPtr.Zero;
        }

        public void CheckDiskStartMessageIVDR() {
            Repair.Text = Application.Current.TryFindResource("LBL_RUNNING_CHECKDSK_IVDR") as string;
        }
        public void CheckDiskStartMessageOther() {
            Repair.Text = Application.Current.TryFindResource("LBL_RUNNING_CHECKDSK_OTHER") as string;
        }

        private void IpcServer() {
            // IPC Channel作成
             Hashtable properties = new Hashtable();
             properties.Add("portName", "ivdr");
             properties.Add("authorizedGroup", "Users");
            IpcServerChannel ipcChannel = new IpcServerChannel(properties, null);

            // チャンネル登録
            ChannelServices.RegisterChannel(ipcChannel, true);

            // リモートオブジェクト生成
            remoteObject = new IpcRemoteObject();
            // イベントハンドラ設定
            remoteObject.getData += new IpcRemoteObject.CallEventHandler(View.MainWindow.GetInstance().getData);
            RemotingServices.Marshal(remoteObject, "ipc", typeof(IpcRemoteObject));
        }

        /**
         * イベントハンドラに設定するメソッド
         * メッセージが送られるとこれが呼ばれる。
         * 
         **/
        private void getData(IpcRemoteObject.IpcRemoteObjectEventArg e) {
            try {
                Dispatcher.InvokeAsync(
                    (Action)(() =>
                {
                    Repair.Text = e.command;
                }));
            } catch(Exception ex) {
                MessageBox.Show(ex.ToString());
            }
           // GetInstance().Repair.Text = e.command;
        }

    }
    // Converter
    [ValueConversion(typeof(bool?), typeof(bool?))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool?))
                return false;
            return (value != null) ? !(bool?)value : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool?))
                return false;

            return (value != null) ? !(bool?)value : null;
        }

        #endregion
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class StringSplitConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                return string.Empty;
            string[] keywords = ((string)value).Trim().Split(new string[] { "\t" },
                                                              StringSplitOptions.RemoveEmptyEntries);
            return (keywords.Length != 0) ? keywords[0] : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                return string.Empty;
            string[] keywords = ((string)value).Trim().Split(new string[] { "\t" },
                                                              StringSplitOptions.RemoveEmptyEntries);
            return (keywords.Length != 0) ? keywords[0] : string.Empty;
        }

        #endregion
    }

    // Binding
    public class Customer
    {
        public string Productname { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string Folder { get; set; }
        public string SortRecordDatTime { get; set; }
        public string RecordDate { get; set; }
        public string Weeks { get; set; }
        public string RecordTime { get; set; }
        public string DurationTime { get; set; }
        public string DurationTimeNormal { get; set; }
        public string Description { get; set; }
        public string FullAddress { get; set; }
        public string RecordMode { get; set; }
        public string Genre { get; set; }
        public string Status { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string serviceUrl { get; set; }
        public BitmapSource ImageSource { get; set; }
        public string Classes { get; set; }
        public string ClassStartsWith {
            get {
                if (Classes != null && Classes.StartsWith("object.item")) {
                    return "object.item";
                }
                else if (Classes != null && Classes.StartsWith("object.container")) {
                    return "object.container";
                }
                else {
                    return string.Empty;
                }
            }
        }
        public string Channel { get; set; }
    }

}

