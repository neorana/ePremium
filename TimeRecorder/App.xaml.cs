using BusinessLayer;
using System.Windows;
using Utilities;


namespace TimeRecorder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ProcessTime processTime = new ProcessTime();
        public void Application_Startup(object sender, StartupEventArgs e)
        {

            if (Common.HasScheduleSetting())
            {
                if (processTime.IsNeedToPromptLogIn())
                {
                    LogIn loginWindow = new LogIn();
                    loginWindow.Show();
                }
                else
                {
                    MainWindow mainView = new MainWindow();
                    mainView.Show();
                }


            }
            else
            {
                //MainWindow mainView = new MainWindow();
                //mainView.Show();

                Settings settingsWindow = new Settings();
                settingsWindow.Show();
            }
        }
    }
}
