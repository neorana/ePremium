using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Entities;
using Utilities;
using DataGrid = System.Windows.Controls.DataGrid;

namespace TimeRecorder
{
    /// <summary>
    /// Interaction logic for TimesheetView.xaml
    /// </summary>
    public partial class TimesheetView : Window
    {
        public TimesheetView()
        {
            InitializeComponent();
            SetDatasource();
        }

        //Objects needed
        // 1. datepicker for month / year (optional)
        // 2. datagrid for displaying records

        private void SetDatasource()
        {
            datagridTimeEntries.ItemsSource = Common.GetTimeEntries(Common.timeInEntriesPath);
            //foreach (DailyRecord item in datagridTimeEntries.)
            //{
            //    var row = datagridTimeEntries.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                

            //        row.Background = Brushes.YellowGreen;

               
            //}



        }
        private void DatagridTimeEntries_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
