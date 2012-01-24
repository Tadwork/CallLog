namespace FPECallLog
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Data;
using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows.Data;
    #endregion

    /// <summary>
    ///   Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        
        /// <summary>
        ///   Initializes a new instance of the <see cref = "Window1" /> class.
        /// </summary>
        public Window1()
        {
            InitializeComponent();

            
        }
        /// <summary>
        ///   Handles the Click event of the buttonSpawn control.
        /// </summary>
        /// <param name = "sender">The source of the event.</param>
        /// <param name = "e">The <see cref = "System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        //private void buttonSpawn_Click(object sender, RoutedEventArgs e)
        //{
        //    string path = GetType().Assembly.Location;

        //    try
        //    {
        //        Process.Start(path, "those are the other application arguments");
        //    }
        //    catch (Exception exc)
        //    {
        //        MessageBox.Show(this, "An exception occurred while spawning another application" + Environment.NewLine + exc);
        //    }
        //}
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {DataContext = new CallViewModel();
        }

        private void List_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CallItem i= (CallItem)e.AddedItems[0];
            string path = "C:\\Users\\tzvi\\AppData\\Local\\Google\\Chrome\\Application\\chrome.exe";
            string args = "\"docs.google.com/a/friedmanpe.com/spreadsheet/viewform?pli=1&hl=en_US&formkey=dGhvdERPU0hjREV5S2ZSb0VIenVXN0E6MQ&entry_0=" + i.Name + "&entry_5=" +i.Phone +"&entry_8=Tzvi\"";
            Process.Start(path,args);
        }
       

       
    }
    public class CallViewModel
        {   private Database  db;
            private ICollectionView _callsView;

            public ICollectionView Calls
            {
                get { return _callsView; }
            }

            public CallViewModel()
            {
                UpdateCalls();
            }
            public void UpdateCalls(){
                var dbPath = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "Calls.db");
			    db = new Database (dbPath);
                IList<CallItem> calls = db.TodaysCalls();
                _callsView = CollectionViewSource.GetDefaultView(calls);
            }
        }
}