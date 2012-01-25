﻿//"C:\Users\tzvi\Documents\call-log\x86 project\bin\x86\Debug\FPECallLog.exe" /n "%N" /p "%P"


namespace FPECallLog
{
    #region Namespaces
    using System;
    using System.Collections;
    using System.Windows;
    using Custom.Windows;
    using System.Data;
    using NDesk.Options;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App : InstanceAwareApplication
    {
        public Database db;
        /// <summary>
        ///   Initializes a new instance of the <see cref = "App" /> class.
        /// </summary>
        /// <exception cref = "System.InvalidOperationException">More than one instance of the <see cref = "System.Windows.Application" /> class is created per <see cref = "System.AppDomain" />.</exception>
        public App()
                : base(ApplicationInstanceAwareness.Host)
        {
            //NOTE: Change the awareness in the default constructor to switch from global awareness and local awareness!
      
        }

        //This function is evaluated only if the GuidAttribute is not defines for the assemlby, so there is no need for a compile switch...
        //It's just here to point the developer attentin on this fact! :)
#if !USE_ASSEMBLY_GUID
        /// <summary>
        /// Called when the the application <see cref="Guid"/> has to be generated.
        /// </summary>
        /// <returns>
        /// The <see cref="Guid"/> used to identify the application univocally.
        /// </returns>
        /// <remarks>
        /// 	<para>If the entry assembly is decorated with a <see cref="System.Runtime.InteropServices.GuidAttribute"/>, this function is ignored.</para>
        /// 	<para>Special care must be taken when overriding this method.
        /// <para>First of all, <c>do not call the base implementation</c>, since it just throws an <see cref="UndefinedApplicationGuidException"/> to inform the developer that something is missing.</para>
        /// 		<para>Moreover, the method must return a <see cref="Guid"/> value which is <c>constant</c>, since it is used to mark univocally the application.</para>
        /// 		<para>The encouraged approach to mark an application univocally, is marking the entry assembly with a proper <see cref="System.Runtime.InteropServices.GuidAttribute"/>; this method should be used only if such method is impractical or not possible.</para>
        /// 	</para>
        /// </remarks>
        /// <exception cref="UndefinedApplicationGuidException">If the function has not been properly overridden or the base implementation has been invoked in a <see cref="InstanceAwareApplication"/> derived class.</exception>
        protected override Guid GenerateApplicationGuid()
        {
            return new Guid("A2048947-1DBF-492b-AADF-3DCFC8B24801");
        }
#endif
        public DataTable Calls = new DataTable("Call List");
        /// <summary>
        ///   Raises the <see cref = "Application.Startup" /> event.
        /// </summary>
        /// <param name = "e">The <see cref = "System.Windows.StartupEventArgs" /> instance containing the event data.</param>
        /// <param name = "isFirstInstance">If set to <c>true</c> the current instance is the first application instance.</param>
        protected override void OnStartup(StartupEventArgs e, bool isFirstInstance)
        {
            base.OnStartup(e, isFirstInstance);

            if (!isFirstInstance)
            {

                //    Window window = MainWindow;
                //const string message = "I am not the first application, and I'm going to shutdown!";
                //const string title = "TestApplication - Next instance";
                //if (window != null)
                //    MessageBox.Show(window, message, title);
                //else
                //    MessageBox.Show(message, title);

                Shutdown(1);
            }
            else { 
                      StartDB();
                       AddCallFromArgs(e.Args);  
            }
        }
        private void StartDB(){
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Calls.db");
            db = new Database(dbPath);
        }
        private void AddCallFromArgs(string[] args)
        {
            //if (args.Length == 2)
            //{
            //    db.AddCall(args[0], args[1]);
            //}
            bool show_help = false;
            string name = "unknown";
            string phone = "000000000";

            var p = new OptionSet() {
    { "n|name=" , "the {NAME} of caller.",
       n => name = n },
   { "p|phone=", "the {Phone} of caller.",
       d => phone = d },
    //{ "h|help",  "show this message and exit", 
    //   v => show_help = v != null },
            };
            List<string> extra;

            try
            {
                extra = p.Parse(args);
                db.AddCall(name, phone);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        /// <summary>
        /// Raises the <see cref="Custom.Windows.InstanceAwareApplication.StartupNextInstance"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Custom.Windows.StartupNextInstanceEventArgs"/> instance containing the event data.</param>
        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {

            e.BringToForeground = false;
            if (e.Args.Length > 0)
            {
                StartDB();
                AddCallFromArgs(e.Args);
                Window1 w = (Window1)App.Current.MainWindow;

                w.DataContext = new CallViewModel();
            }
            base.OnStartupNextInstance(e);
            

            //
            //string message = "Another instance of this application was started";
            //const string title = "TestApplication - First instance";
            //if (e.Args.Length > 0)
            //    message += Environment.NewLine + "args:" + Environment.NewLine + EnumerableToString(e.Args);

            //if (window != null)
            //    MessageBox.Show(window, message, title);
            //else
            //    MessageBox.Show(message, title);
        }

        /// <summary>
        ///   Converts an enumerable in a string.
        /// </summary>
        /// <param name = "enumerable">The enumerable.</param>
        /// <returns>The string containing the enumerable items.</returns>
        private string EnumerableToString(IEnumerable enumerable)
        {
            string result = "";

            foreach (object obj in enumerable)
            {
                if (obj != null)
                    result += obj + Environment.NewLine;
            }

            return result;
        }
    }
}