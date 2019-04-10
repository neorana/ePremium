using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Utilities
{
    public class Helper
    {
        public void CloseWindow(Window x)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            //  int count = Application.Current.Windows;
            foreach (Window w in Application.Current.Windows)
            {
                //Form f = Application.OpenForms[i];
                if (w.GetType().Assembly == currentAssembly && w == x)
                {
                    w.Close();
                }
            }
        }
        public List<T> Deserialize<T>(string path)
        {
            return JsonConvert.DeserializeObject<List<T>>(path);
        }

    }
}
