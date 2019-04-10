using Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Threading;
using Utilities;

namespace TimeRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string CaptionLogOut = "Log Out"; //test for push in githubs
        const string CaptionLogIn = "Log In";

        public bool IsExitFromSystemTray = false;

        DailyRecord record = new DailyRecord();
        DispatcherTimer timer = new DispatcherTimer();

        System.Windows.Forms.NotifyIcon MyNotifyIcon = new System.Windows.Forms.NotifyIcon();
        System.Windows.Forms.MenuItem SystemTrayMenu = new System.Windows.Forms.MenuItem();

        Mutex m;


        public MainWindow()
        {

            bool isnew;
            m = new Mutex(true, "TimeRecorderApp", out isnew);
            if (!isnew)
            {
                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            }
            InitializeComponent();
            //prevent multiple launching apps
            //    String thisprocessname = Process.GetCurrentProcess().ProcessName;

            //if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            //    return;
            //end of multiple launching

            var login = Common.GetLatestLogIn();

            //For system tray
            MyNotifyIcon = new System.Windows.Forms.NotifyIcon();


            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/TimeRecorder;component/Resources/epremium.ico")).Stream;
            MyNotifyIcon.Icon = new System.Drawing.Icon(iconStream);

            SystemTrayMenu.Text = "Exit";
            SystemTrayMenu.Click += SystemTrayMenu_Click;
            var contextMenu1 = new System.Windows.Forms.ContextMenu();
            contextMenu1.MenuItems.AddRange(
                    new System.Windows.Forms.MenuItem[] { SystemTrayMenu });

            MyNotifyIcon.ContextMenu = contextMenu1;

            MyNotifyIcon.MouseDoubleClick +=
                new System.Windows.Forms.MouseEventHandler
                    (MyNotifyIcon_MouseDoubleClick);


            timer.Tick += (s, e) => lblCurrentDateTime.Content = DateTime.Now.ToString("dddd, MMMM dd, yyyy hh:mm:ss tt");
            //timer.Tick += (s, e) => lblTest.Content = DateTime.Now.ToString("hh:mm tt");
            timer.Tick += new EventHandler(MyTimer_Tick);
            timer.Interval = TimeSpan.FromSeconds(1);

            timer.Start();

            DataContext = this;

        }

        private void SystemTrayMenu_Click(object sender, EventArgs e)
        {
            IsExitFromSystemTray = true;

            if (!Common.HasLogoutToday())
            {
                LogOut window = new LogOut();
                window.Show();
            }

            this.Close();
        }

        //For system tray
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app ";
                MyNotifyIcon.ShowBalloonTip(400);
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }


        //For system tray
        void MyNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }


        private void SnoozeLogin()
        {
            if (!Common.HasLoginToday())
            {
                if (Common.GetCurrentSetting().StartTime != DateTime.Now.ToString("hh:mm tt")
                    && (DateTime.Now.Minute % 30) == 0
                    && Properties.Settings.Default.SnoozeTime != DateTime.Now.ToString("hh:mm tt"))
                {
                    SnoozeSetup();
                }
            }
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //added logic for prompt start time
            //solution: get the start time set in settings then compare with current date and time
            lblTest.Content = DateTime.Now.ToString("hh:mm tt");
            var startTimesetting = Common.GetCurrentSetting().StartTime;
            var substringHour = lblElapseTime.Content.ToString().Substring(0, 2);

            if (bbtnLogOut.Content.ToString() == CaptionLogIn)
            {
                //prompt Login
                if (lblTest.Content.ToString() == startTimesetting)
                {
                    if (Properties.Settings.Default.SnoozeTime != DateTime.Now.ToString("hh:mm tt"))
                    {
                        SnoozeSetup();
                    }
                }
                else
                {
                    SnoozeLogin();
                }

            }
            else
            {
                //prompt Logout End of Day
                var defaultElapse = "00:00:00";
                {
                    if (lblElapseTime.Content.ToString() != defaultElapse &&
                        substringHour == "09") //nine hours elapse time from login
                    {
                        if (Properties.Settings.Default.SnoozeTime != DateTime.Now.ToString("hh:mm tt"))
                        {
                            Properties.Settings.Default.SnoozeTime = DateTime.Now.ToString("hh:mm tt");
                            Properties.Settings.Default.Save();
                            LogOut window = new LogOut();
                            window.Show();
                            this.Close();
                            timer.Stop();
                        }

                    }
                }
            }
        }

        private void SnoozeSetup()
        {
            Properties.Settings.Default.SnoozeTime = DateTime.Now.ToString("hh:mm tt");
            Properties.Settings.Default.Save();

            Common.CloseWindow<LogIn>("LogIn");

            LogIn window = new LogIn();
            window.Topmost = true;
            window.Activate();
            window.Show();
            this.Close();
            timer.Stop();
        }

        void clockTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(UpdateCurrentDateTime));
        }
        public string CurrentDate
        {
            get { return (string)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }
        public string CurrentTime
        {
            get { return (string)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public string CurrentDateTime
        {
            get { return (string)GetValue(CurrentDateTimeProperty); }
            set { SetValue(CurrentDateTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty CurrentTimeProperty =
           DependencyProperty.Register("CurrentTime", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty CurrentDateTimeProperty =
          DependencyProperty.Register("CurrentDateTime", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        private void UpdateCurrentDateTime()
        {
            CurrentDate = DateTime.Now.ToString("D");
            CurrentTime = DateTime.Now.ToString("t");
            CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy hh:mm tt");
        }
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            if (bbtnLogOut.Content.ToString() == CaptionLogOut)
            {
                LogOut window = new LogOut();
                window.Show();
                this.Close();
            }
            else
            {
                LogIn window = new LogIn();
                window.Show();
                this.Close();
            }

        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings window = new Settings();
            window.Show();
            this.Close();
        }
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            // Call upload method here
            var result = (System.Windows.Forms.MessageBox.Show($"{Common.GetTimesheetCoverage()} timesheet." +
                                                            $"\nThis will be posted to ePremium site.\nDo you want to proceed?",
                "Upload", System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning));

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                var uploadresult = Common.UploadTimesheet();
                if (uploadresult)
                {

                    MessageBox.Show($"Timesheet for {Common.GetTimesheetCoverage()} successfully uploaded");
                    System.Diagnostics.Process.Start("http://epremium.internal.towerswatson.com");
                }
            }
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            LogIn window = new LogIn();
            window.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Common.HasLoginToday())
            {
                var login = Common.GetLatestLogIn();
                var logout = Common.GetLatestTimeEntryLine().TimeOut;
                if (login.Date == DateTime.Now.Date && logout == DateTime.MinValue)
                {
                    lblMainWindowTimeIn.Content = login.ToString("hh:mm tt");

                    timer.Tick += (o, args) =>
                    {
                        lblElapseTime.Content = (login - DateTime.Now).ToString(@"hh\:mm\:ss");
                    };
                    timer.Start();
                    bbtnLogOut.IsEnabled = true;
                    bbtnLogOut.Content = CaptionLogOut;
                }
                else
                {
                    lblMainWindowTimeIn.Content = login.ToString("hh:mm tt");
                    lblCaptionElapse.Content = "Your Time Out:";
                    lblElapseTime.Content = logout.ToString("hh:mm: tt");
                    bbtnLogOut.IsEnabled = false;
                }
            }
            else
            {
                //consider night shift
                var login = Common.GetLatestLogIn();
                if (Common.GetCurrentShiftSetting() == "NIGHT")
                {
                    if ((DateTime.Now - login).TotalDays <= 1)
                    {
                        bbtnLogOut.Content = CaptionLogOut;
                    }
                    else
                    {
                        bbtnLogOut.Content = CaptionLogIn;
                    }
                }



            }
        }

        private void FormMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsExitFromSystemTray)
            {
                this.WindowState = WindowState.Minimized;
                e.Cancel = true;
            }
        }

        private void FormMain_Closed(object sender, EventArgs e)
        {

        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            TimesheetView ts1 = new TimesheetView();
            ts1.Show();
            this.Close();
        }
    }

}