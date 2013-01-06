    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Data;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows;
    using System.Windows.Input;
    using Sipek.Common.CallControl;
    using Sipek.Sip;
    using Sipek.Common;
    using Tako.GlobalHotKey;
    using WindowsInput;
    #endregion
namespace FPECallLog
{


    /// <summary>
    ///   Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Tako.GlobalHotKey.HotKeyProvider m_Provider;
               #region Properties
        // Get call manager instance
        CCallManager CallManager = CCallManager.Instance;
        private PhoneConfig _config = new PhoneConfig();
        internal PhoneConfig Config
        {
            get { return _config; }
        }

        // instance of incoming call
        IStateMachine incall = null;

        private bool registered = false;

        //private Contacts contacts = new Contacts();
       private NotifyIcon notifyIcon1 = new NotifyIcon();
        private ContextMenu contextMenu = new ContextMenu();
        
        #endregion
        /// <summary>
        ///   Initializes a new instance of the <see cref = "Window1" /> class.
        /// </summary>
        public Window1()
        {
            InitializeComponent();
            InitializeSip();
            InitializeNotifyIcon();
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
        this.m_Provider = new HotKeyProvider();
        this.m_Provider.HotKeyPressed += m_Provider_HotKeyPressed;
        var key1 = m_Provider.Register(ModifierKeys.None, Key.F6);
        }

        private void m_Provider_HotKeyPressed(object sender, HotKeyPressedEventArgs e)
        {
            if ( e.HotKey.Key == Key.F6)
            {
                //string oldtext = System.Windows.Clipboard.GetText();
                
                InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                System.Threading.Thread.Sleep(50);
                string numToCall = System.Windows.Clipboard.GetText();
                this.TxtNumToCall.Text = numToCall;
                
                //System.Windows.Clipboard.SetText(oldtext);
                
            }
            //else if (e.HotKey.ModifierKeys == (ModifierKeys.Alt | ModifierKeys.Control) && e.HotKey.Key == Key.F4)
            //{
            //  //  MessageBox.Show("ctrl+alt+f4");
            //}
        }

        
        private void List_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                CallItem i = (CallItem)e.AddedItems[0];
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = appdata + "\\Google\\Chrome\\Application\\chrome.exe";
               // MessageBox.Show(path);
                string args = "\"docs.google.com/a/friedmanpe.com/spreadsheet/viewform?pli=1&hl=en_US&formkey=dGhvdERPU0hjREV5S2ZSb0VIenVXN0E6MQ&entry_2=" + i.Name + "&entry_5=" + i.Phone + "&entry_7=Tzvi\"";
                Process.Start(path, args);
            }
            catch { 
            
            }
        }


        #region Methods
        private void InitializeNotifyIcon()
        {
            //this.WindowState = FormWindowState.Minimized;
            //this.Visible = false;
            //this.ShowInTaskbar = false;

            notifyIcon1.Icon = FPECallLog.Resources.Icon;
            notifyIcon1.Text = "SIP Notifier"; 
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += new EventHandler(NotifyIconDoubleClick);



            //contextMenu.MenuItems.Add(0,
            //    new MenuItem("Show/Hide", new System.EventHandler(NotifyIconDoubleClick)));
            //contextMenu.MenuItems.Add(1,
            //    new MenuItem("Exit", new System.EventHandler(notifyIcon1_Close_Click)));

            //notifyIcon1.ContextMenu = contextMenu;

            if (FPECallLog.Accounts.Default.HostName == "localhost")
            {
                notifyIcon1.BalloonTipTitle = "FPE Call Log";
                notifyIcon1.BalloonTipText = "Doubleclick this icon to set up your SIP account!";
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.ShowBalloonTip(30);
            }

        }

        private void InitializeSip()
        {
            if (registered == true || FPECallLog.Accounts.Default.HostName == "")
                return;

            ConnectionStatus.Content = "Connecting";
            // register callbacks
            CallManager.CallStateRefresh += new DCallStateRefresh(CallManager_CallStateRefresh);
            // ICallProxyInterface.CallStateChanged += new DCallStateChanged(CallManager_OnCallStateChanged);
            CallManager.IncomingCallNotification += new DIncomingCallNotification(CallManager_IncomingCallNotification);

            pjsipRegistrar.Instance.AccountStateChanged += new Sipek.Common.DAccountStateChanged(Instance_AccountStateChanged);

            // Inject VoIP stack engine to CallManager
            CallManager.StackProxy = pjsipStackProxy.Instance;

            // Inject configuration settings SipekSdk
            CallManager.Config = Config;
            pjsipStackProxy.Instance.Config = Config;
            pjsipRegistrar.Instance.Config = Config;

            // Initialize
            CallManager.Initialize();
            // register accounts...
            pjsipRegistrar.Instance.registerAccounts();

            registered = true;
        }

        private void DeinitializeSip()
        {
            if (registered == false)
                return;
            pjsipRegistrar.Instance.unregisterAccounts();
            CallManager.Shutdown();
            pjsipStackProxy.Instance.shutdown();
            ConnectionStatus.Content = "Disconnected";

            registered = false;
        }

        private void RestartSip()
        {
            DeinitializeSip();
            InitializeSip();
        }
        #endregion

        #region Callbacks
        void Instance_AccountStateChanged(int accountId, int accState)
        {
            // MUST synchronize threads
            if (!(App.Current.Dispatcher.Thread == System.Threading.Thread.CurrentThread))
                this.Dispatcher.BeginInvoke(new DAccountStateChanged(OnRegistrationUpdate), new object[] { accountId, accState });
            else
                OnRegistrationUpdate(accountId, accState);
        }


        void CallManager_CallStateRefresh(int sessionId)
        {
            // MUST synchronize threads
            if (!(App.Current.Dispatcher.Thread == System.Threading.Thread.CurrentThread))
                this.Dispatcher.BeginInvoke(new DCallStateRefresh(OnStateUpdate), new object[] { sessionId });
            else
                OnStateUpdate(sessionId);
        }

        /*void CallManager_OnCallStateChanged(int callId, ESessionState callState, string info)
        {
            // MUST synchronize threads
            if (InvokeRequired)
                this.BeginInvoke(new DCallStateChanged(OnCallStateChanged), new object[] { callId, callState, info });
            else
                OnCallStateChanged(callId, callState, info);
        }*/

        void CallManager_IncomingCallNotification(int sessionId, string number, string info)
        {
            // MUST synchronize threads
            if (!(App.Current.Dispatcher.Thread == System.Threading.Thread.CurrentThread))
                this.Dispatcher.BeginInvoke(new DIncomingCallNotification(OnIncomingCall), new object[] { sessionId, number, info });
            else
                OnIncomingCall(sessionId, number, info);
        }
        #endregion

        #region Synchronized callbacks
        private void OnRegistrationUpdate(int accountId, int accState)
        {
            string stateDescription = "";
            switch (accState)
            {
                case 200: stateDescription = "OK";
                    break;

                case 408: stateDescription = "Request Timeout";
                    break;

                case 503: stateDescription = "Service Unavailable";
                    break;

                case 401: stateDescription = "Unauthorized";
                    break;

                case 403: stateDescription = "Forbidden";
                    break;

                case -1:
                case 171100: stateDescription = "Login failed";

                    break;

                default: stateDescription = "Unknown";
                    break;
            }
            ConnectionStatus.Content = accState.ToString() + " - " + stateDescription;
        }

        private void OnStateUpdate(int sessionId)
        {
            string text = CallManager.getCall(sessionId).StateId.ToString();
            if (text == "NULL")
                text = "IDLE";

            CallStatus.Content = text;
        }

        /* private void OnCallStateChanged(int callId, ESessionState callState, string info)
        {
            IStateMachine state = CallManager.CallList[callId];

            number = state.CallingNumber;

            textBoxCallState.Text = number;
                        
            string contact = contacts.lookup(number);
            if (contact != "")
                number = contact + " (" + number + ")";

            notifyIcon1.BalloonTipTitle = "Eingehender Anruf";
            notifyIcon1.BalloonTipText = "Von: " + number + "\r\n" + info;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(30);
        } */

        private void OnIncomingCall(int sessionId, string number, string info)
        {
            incall = CallManager.getCall(sessionId);
            //string contact = contacts.lookup(number);
            //if (contact != "")
            //  number = contact + " (" + number + ")";

            CallStatus.Content = incall.StateId.ToString();
            // TODO put the code here to put the call in the DB
            Database  db =  new Database();
            string name = incall.CallingName;
            db.AddCall(name, number);
            DataContext = new CallViewModel();
            //textBoxLastCallNumber.Text = number;
            //textBoxLastCallDate.Text = DateTime.Now.ToString();

            notifyIcon1.BalloonTipTitle = "New Call";
            notifyIcon1.BalloonTipText = "From: " + name + " " + number + "\r\n" + info;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(30);

            /*
            // Send Busy
            ICallProxyInterface proxy = CallManager.StackProxy.createCallProxy();
            proxy.serviceRequest((int)EServiceCodes.SC_CFB, "");
            proxy.endCall();
             */
        }

        private int CallNumber(string number){

            return status;
        }
        #endregion

        #region Button Handlers
        private void NotifyIconDoubleClick(object sender, System.EventArgs e)
        {
            //if (this.ShowInTaskbar == true)
            //{
            //    this.ShowInTaskbar = false;
            //    this.t = false;
            //}
            //else
            //{
                FPECallLog.Accounts settings = FPECallLog.Accounts.Default;
                TxtServer.Text = settings.HostName;
                TxtAccount.Text = settings.UserName;
                TxtPassword.Text = settings.Password;
                BtnSave.IsEnabled = true;
                //this.ShowInTaskbar = true;
                //this.Visible = true;
                //this.BringToFront();
            //}

        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (TxtServer.Text == "")
            {
                System.Windows.MessageBox.Show("Server hostname must not be empty!");
                return;
            }
            BtnSave.IsEnabled = false;
            FPECallLog.Accounts settings = FPECallLog.Accounts.Default;

            settings.HostName = TxtServer.Text;
            settings.UserName = TxtAccount.Text;
            settings.Id = TxtAccount.Text;
            settings.Password = TxtPassword.Text;

            settings.Save();
            RestartSip();
        }

        private void linkLabelCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FPECallLog.Accounts settings = FPECallLog.Accounts.Default;

            TxtServer.Text = settings.HostName;
            TxtAccount.Text = settings.UserName;
            TxtPassword.Text = settings.Password;
            BtnSave.IsEnabled = false;
        }

        //private void notifyIcon1_Close_Click(object sender, EventArgs e)
        //{
        //    Close();
        //}

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    notifyIcon1.Visible = false;
        //    base.OnClosing(e);
        //}

        //private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    if (e.CloseReason == CloseReason.UserClosing)
        //    {
        //        e.Cancel = true;
        //        this.ShowInTaskbar = false;
        //        this.Visible = false;
        //    }
        //}
        private void AccountInfo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            
                if(null != BtnSave)BtnSave.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Database db = new Database();
            db.AddCall("test Name", "999-999-9999");
            DataContext = new CallViewModel();
        }

        private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
        {
            //implement call functionality
        }
     
        #endregion



       


       
    }
    public class CallViewModel
        {   private Database  db;
            private ICollectionView _callsView;
            //private string _server;
            //private string _account;
            //private string _password;
            //private string _callstate;
            //private string _regstate;

            //public string CallState
            //{
            //    get { return _callstate; }
            //    set { _callstate = value; }
            //}
            //public string Password
            //{
            //    get { return _password; }
            //    set { _password = value; }
            //}
            //public string RegistrationState
            //{
            //    get { return _regstate; }
            //    set { _regstate = value; }
            //}

            //public string Account
            //{
            //    get { return _account; }
            //    set { _account = value; }
            //}
            
            //public string Server
            //{
            //    get { return this._server; }
            //    set { this._server = value; }
            //}
            public ICollectionView Calls
            {
                get { return this._callsView; }
            }
   
            public CallViewModel()
            {
                UpdateCalls();
            }
            public void UpdateCalls(){
                //var dbPath = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "Calls.db");
			    db = new Database();
                IList<CallItem> calls = db.TodaysCalls();
                _callsView = CollectionViewSource.GetDefaultView(calls);
            }
        }


  
    }
