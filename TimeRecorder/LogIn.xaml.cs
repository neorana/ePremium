using BusinessLayer;
using System.Timers;
using System.Windows;
using Utilities;
using System.Windows.Threading;



namespace TimeRecorder
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        ProcessTime processTime = new ProcessTime();
        public LogIn()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += delegate { DragMove(); };
        }

        private void btnLogInYes_Click(object sender, RoutedEventArgs e)
        {
            if (processTime.RecordTime(true))
            {
                MessageBox.Show("Login has been successfully recorded!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Something went wrong!..", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Close();
        }

        private void btnLogInNo_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
        }
    }
}
