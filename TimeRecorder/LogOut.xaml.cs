using BusinessLayer;
using System;
using System.Windows;

namespace TimeRecorder
{
    /// <summary>
    /// Interaction logic for LogOut.xaml
    /// </summary>
    public partial class LogOut : Window
    {
        ProcessTime processTime = new ProcessTime();
        public LogOut()
        {
            InitializeComponent();
        }

        private void btnLogOutYes_Click(object sender, RoutedEventArgs e)
        {
            if (processTime.RecordTime(false))
            {
                MessageBox.Show("Log out has been successfully recorded!", "Log Out", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Something went wrong!..", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //MainWindow window = new MainWindow();
            //window.Show();
            this.Close();
        }

        private void btnLogOutNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }
    }
}
