namespace FPECallLog
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Windows;

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
        {
            CallGrid.ItemsSource = 
        }
    }
}