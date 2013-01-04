    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Data;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows.Data;
    using System.Windows.Forms;
    using Sipek.Common.CallControl;
    using Sipek.Sip;
    using Sipek.Common;
    #endregion
namespace FPECallLog
{


    /// <summary>
    ///   Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
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
        private void InitializeWindow()
        {
            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.ShowInTaskbar = false;

            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Text = "SIP Notifier";   // Eigenen Text einsetzen
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += new EventHandler(NotifyIconDoubleClick);



            contextMenu.MenuItems.Add(0,
                new MenuItem("Show/Hide", new System.EventHandler(NotifyIconDoubleClick)));
            contextMenu.MenuItems.Add(1,
                new MenuItem("Exit", new System.EventHandler(notifyIcon1_Close_Click)));

            notifyIcon1.ContextMenu = contextMenu;

            if (SIP_Notifier.Accounts.Default.HostName == "localhost")
            {
                notifyIcon1.BalloonTipTitle = "SIP Notifier";
                notifyIcon1.BalloonTipText = "Doubleclick this icon to set up your SIP account!";
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.ShowBalloonTip(30);
            }

        }

        private void InitializeSip()
        {
            if (registered == true || SIP_Notifier.Accounts.Default.HostName == "")
                return;

            textBoxRegState.Text = "Connecting";
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
            textBoxRegState.Text = "Disconnected";

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
            if (InvokeRequired)
                this.BeginInvoke(new DAccountStateChanged(OnRegistrationUpdate), new object[] { accountId, accState });
            else
                OnRegistrationUpdate(accountId, accState);
        }


        void CallManager_CallStateRefresh(int sessionId)
        {
            // MUST synchronize threads
            if (InvokeRequired)
                this.BeginInvoke(new DCallStateRefresh(OnStateUpdate), new object[] { sessionId });
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
            if (InvokeRequired)
                this.BeginInvoke(new DIncomingCallNotification(OnIncomingCall), new object[] { sessionId, number, info });
            else
                OnIncomingCall(sessionId, number, info);
        }
        #endregion

        #region Synhronized callbacks
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
            textBoxRegState.Text = accState.ToString() + " - " + stateDescription;
        }

        private void OnStateUpdate(int sessionId)
        {
            string text = CallManager.getCall(sessionId).StateId.ToString();
            if (text == "NULL")
                text = "IDLE";

            textBoxCallState.Text = text;
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

            textBoxCallState.Text = incall.StateId.ToString();
            textBoxLastCallNumber.Text = number;
            textBoxLastCallDate.Text = DateTime.Now.ToString();

            notifyIcon1.BalloonTipTitle = "New Call";
            notifyIcon1.BalloonTipText = "From: " + number + "\r\n" + info;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(30);

            /*
            // Send Busy
            ICallProxyInterface proxy = CallManager.StackProxy.createCallProxy();
            proxy.serviceRequest((int)EServiceCodes.SC_CFB, "");
            proxy.endCall();
             */
        }

        #endregion

        #region Button Handlers
        private void NotifyIconDoubleClick(object sender, System.EventArgs e)
        {
            if (this.ShowInTaskbar == true)
            {
                this.ShowInTaskbar = false;
                this.Visible = false;
            }
            else
            {
                SIP_Notifier.Accounts settings = SIP_Notifier.Accounts.Default;
                textBoxHostName.Text = settings.HostName;
                textBoxUserName.Text = settings.UserName;
                textBoxPassword.Text = settings.Password;
                buttonSave.Enabled = false;
                this.ShowInTaskbar = true;
                this.Visible = true;
                //this.BringToFront();
            }

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (textBoxHostName.Text == "")
            {
                MessageBox.Show("Server hostname must not be empty!");
                return;
            }
            buttonSave.Enabled = false;
            SIP_Notifier.Accounts settings = SIP_Notifier.Accounts.Default;

            settings.HostName = textBoxHostName.Text;
            settings.UserName = textBoxUserName.Text;
            settings.Id = textBoxUserName.Text;
            settings.Password = textBoxPassword.Text;

            settings.Save();
            RestartSip();
        }

        private void linkLabelCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SIP_Notifier.Accounts settings = SIP_Notifier.Accounts.Default;

            textBoxHostName.Text = settings.HostName;
            textBoxUserName.Text = settings.UserName;
            textBoxPassword.Text = settings.Password;
            buttonSave.Enabled = false;
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

        //private void textBoxHostName_TextChanged(object sender, EventArgs e)
        //{
        //    buttonSave.Enabled = true;
        //}

        //private void textBoxUserName_TextChanged(object sender, EventArgs e)
        //{
        //    buttonSave.Enabled = true;
        //}

        //private void textBoxPassword_TextChanged(object sender, EventArgs e)
        //{
        //    buttonSave.Enabled = true;
        //}
        #endregion
       
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