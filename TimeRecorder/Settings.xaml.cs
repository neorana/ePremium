using Entities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Utilities;

namespace TimeRecorder
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public ReadOnlyCollection<TimeZoneInfo> TimeZoneList;
        MainWindow mainView = new MainWindow();

        public Settings()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //insert timezones
            ReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();
            //this.DataContext = TimeZones;
            foreach (TimeZoneInfo z in TimeZones)
            {
                cmbTimeZone.Items.Add(z.DisplayName);
            }

            //insert sample shifts
            cmbShift.Items.Insert(0, "DAY");
            cmbShift.Items.Insert(1, "MID");
            cmbShift.Items.Insert(2, "NIGHT");

            //insert start time
            for (int i = 0; i < 24; i++)
            {
                DateTime tm = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, i, 0, 0);
                cmbStartTime.Items.Insert(i, tm.ToShortTimeString());
            }

            var hasSchedSetting = Common.HasScheduleSetting();
            if (hasSchedSetting)
            {
                //retrieve for editing
                var path = Common.commonSettingsPath;
                string[] json = Common.GetLinesofJson(path);
                ScheduleSetting scheduleSetting = JsonConvert.DeserializeObject<ScheduleSetting>(json[json.Length - 1]);

                txtTeamManager.Text = scheduleSetting.TeamManager.ToString();
                txtActivity.Text = scheduleSetting.Activity.ToString();
                isAutoSubmit.IsChecked = scheduleSetting.AutoSubmit;
                cmbTimeZone.SelectedValue = scheduleSetting.TimeZone;
                if (scheduleSetting.Shift == "DAY")
                {
                    cmbShift.SelectedIndex = 0;
                }
                else if (scheduleSetting.Shift == "MID")
                {
                    cmbShift.SelectedIndex = 1;
                }
                else
                {
                    cmbShift.SelectedIndex = 2;
                }

                cmbStartTime.SelectedValue = scheduleSetting.StartTime;
                cmbTimeZone.SelectedValue = scheduleSetting.TimeZone;

            }
            else
            {

                cmbTimeZone.SelectedIndex = 1;


                cmbShift.SelectedIndex = 0;


                cmbStartTime.SelectedIndex = 0;
            }


        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtActivity.Text.Equals(string.Empty))
            {
                MessageBox.Show("Activity setup is required", "Setting", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                try
                {
                    ScheduleSetting ss = new ScheduleSetting();
                    ss.AutoSubmit = isAutoSubmit.IsChecked.Value;
                    ss.TeamManager = txtTeamManager.Text;
                    ss.Shift = cmbShift.SelectedValue.ToString();
                    ss.TimeZone = cmbTimeZone.SelectedValue.ToString();
                    ss.StartTime = cmbStartTime.SelectedValue.ToString();
                    ss.Activity = txtActivity.Text;
                    ss.DateCreated = DateTime.UtcNow;

                    if (BusinessLayer.SetupSetting.SaveSettings(ss))
                    {
                        MessageBox.Show("Settings has been successfully saved. Boom Beybi! :)", "Setting", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong...", "Setting", MessageBoxButton.OK, MessageBoxImage.Error);

                    }

                    mainView.Show();
                    this.Close();

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }



        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainView.Show();
        }
    }
}
