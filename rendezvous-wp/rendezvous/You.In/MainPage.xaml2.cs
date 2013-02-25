using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Paco.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using You.In;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;

namespace You.In
{
    public partial class MainPage : PhoneApplicationPage
    {

        private const string API_KEY = "ecfa9b40ad2ff0e84913fb17072c740c";
        private const string API_SECRET = "1cbac22e4a81106d0b51b1f56a8daee4";
        private Regex tokenRegex = new Regex(@"access_token=(?<token>[a-z0-9A-Z\._|-]+)");
        private readonly string FACEBOOK_CONNECT_URL_FORMAT = String.Format("http://www.facebook.com/dialog/oauth?client_id=137117509680418&redirect_uri=http://rend.ezvo.us/details/&display=wap&scope=publish_stream,create_event,user_events,friends_events,rsvp_event,offline_access");

        private string code = String.Empty;
        FQLCall fql;
        static bool _finished_friends = false;
        static bool _finished_upcoming = false;
        bool _loggingout = false;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            SupportedOrientations = SupportedPageOrientation.Portrait;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            webBrowser1.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(MainBrowser_Navigated);
            webBrowser1.Navigating += new EventHandler<NavigatingEventArgs>(webBrowser1_Navigating);
            webBrowser1.Source = (new Uri(FACEBOOK_CONNECT_URL_FORMAT));
        }


        #region Properties

        private bool isLoading;

        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;

                if (isLoading)
                {
                    loadingImage.Visibility = System.Windows.Visibility.Visible;
                    loadingProgress.Visibility = System.Windows.Visibility.Visible;
                    //  loadingText.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    loadingImage.Visibility = System.Windows.Visibility.Collapsed;
                    loadingProgress.Visibility = System.Windows.Visibility.Collapsed;
                    //  loadingText.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region Facebook tasks and calls

        private void StartLoadingData()
        {
            IsLoading = true;

            fql = new FQLCall();

            fql.UpcomingEventsFinished += new FQLCall.UpcomingEventsEventHandler(fql_UpcomingEventsFinished);
            fql.UpcomingEventsError += new FQLCall.UpcomingEventsErrorHandler(fql_UpcomingEventsError);
            fql.GetUpcomingEvents(AppSettings.TokenSetting);

            fql.FriendsFinished += new FQLCall.FriendsEventHandler(fql_FriendsFinished);
            fql.GetFriends(AppSettings.TokenSetting);
            foreach (FqlTaskType fqlTaskType in new FqlTaskType[] 
                    {
                       // FqlTaskType.GetCheckins,
                        FqlTaskType.GetUserInfo
                      //  FqlTaskType.GetFriends,
                    })
            {
                FqlTask task = new FqlTask(AppSettings.TokenSetting, fqlTaskType);
                ThreadPool.QueueUserWorkItem(task.DoSubtask);
            }
        }

        void fql_FriendsFinished(object sender, EventArgs e)
        {
            _finished_friends = true;
            if (_finished_upcoming && _finished_friends)
            {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            NavigationService.Navigate(new Uri("/Events_Page.xaml?fromLogin=1", UriKind.Relative));
                        }
                        catch (Exception) { }
                    });
            }
        }

        void fql_UpcomingEventsFinished(object sender, EventArgs e)
        {
            _finished_upcoming = true;
            if (_finished_upcoming && _finished_friends)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        NavigationService.Navigate(new Uri("/Events_Page.xaml?fromLogin=1", UriKind.Relative));
                    }
                    catch (Exception) { }
                });

            }
        }


        void fql_UpcomingEventsError(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (Debugger.IsAttached) Debugger.Break();

                this.IsLoading = false;
            });
        }

        #endregion

        #region Event Handlers

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppSettings.BackToLogin)
            {
                AppSettings.BackToLogin = false;
                throw new AppExitException();
            }
            else if (this.NavigationContext.QueryString.ContainsKey("logout"))
            {
                IsLoading = false;
                _loggingout = true;
                webBrowser1.Navigate(new Uri("http://m.facebook.com/logout.php?confirm=1"));
            }
            else
            {
                if (String.IsNullOrEmpty(AppSettings.TokenSetting))
                {
                    webBrowser1.Navigate(new Uri(FACEBOOK_CONNECT_URL_FORMAT));
                }
                else
                {
                    StartLoadingData();
                }
            }
        }

        private void hyperlinkButton1_Click(object sender, RoutedEventArgs e)
        {
            if (!webBrowser1.Source.AbsolutePath.Equals(FACEBOOK_CONNECT_URL_FORMAT))
            {
                webBrowser1.Visibility = Visibility.Visible;
                loginTxtBlock.Visibility = Visibility.Visible;
                webBrowser1.Navigate(new Uri(FACEBOOK_CONNECT_URL_FORMAT));
            }
        }

        #endregion

        #region Web browser event handlers

        void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            //  bool x = NetworkInterface.Networ
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                this.Dispatcher.BeginInvoke(() =>
{
    MessageBox.Show("Sorry we could not connect to the internet to run this application.");
    NavigationService.GoBack();
});

                return;
            }
            if (e.Uri.Host.Equals("rend.ezvo.us"))
            {
                webBrowser1.Visibility = Visibility.Collapsed;
                loginTxtBlock.Visibility = Visibility.Collapsed;
                IsLoading = true;
            }
        }

        void MainBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            if (!_loggingout)
            {

                if (e.Uri.AbsolutePath.Equals("/dialog/permissions.request"))
                {
                    webBrowser1.Height = 700;
                }
                else
                {
                    webBrowser1.Height = 400;
                }
                //only show the browser when the host is facebook
                if (e.Uri.Query.Contains("code=") && String.IsNullOrEmpty(code))
                {
                    code = e.Uri.Query.Substring(e.Uri.Query.IndexOf("code=") + 5);
                }

                if (!String.IsNullOrEmpty(code) && String.IsNullOrEmpty(AppSettings.TokenSetting) && !e.Uri.Query.Contains("client_secret="))
                {
                    string url = String.Format(@"https://graph.facebook.com/oauth/access_token?client_id={0}&display=touch&redirect_uri=http://rend.ezvo.us/details/&client_secret={1}&code={2}&scope=publish_stream,create_event&req_perms=publish_stream,create_event", API_KEY, API_SECRET, code);
                    webBrowser1.Navigate(new Uri(url));
                }
                else if (e.Uri.Query.Contains("client_secret"))
                {
                    string source = webBrowser1.SaveToString();
                    Match m = tokenRegex.Match(source);
                    AppSettings.TokenSetting = m.Groups["token"].Value;
                    //loginStack.Visibility = Visibility.Collapsed;
                    IsLoading = true;
                    StartLoadingData();
                }
                else
                {
                    if (String.IsNullOrEmpty(AppSettings.TokenSetting))
                    {
                        IsLoading = true;
                        //webBrowser1.Navigate(e.Uri);
                        //webBrowser1.Visibility = Visibility.Visible;
                       // loginStack.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                _loggingout = false;
                //webBrowser1.Visibility = Visibility.Collapsed;
                IsLoading = false;
            }
        }

        #endregion

        #region FQL Task

        enum FqlTaskType
        {
            GetUserInfo,
            //GetCheckins,
            GetFriends,
            GetUpcomingEvents
        }

        class FqlTask
        {
            // Signal this ManualResetEvent when the task is finished.
            internal ManualResetEvent Finished = new ManualResetEvent(false);
            string Token;
            FQLCall fql;
            FqlTaskType Type;
            internal FqlTask(String token, FqlTaskType type)
            {
                fql = new FQLCall();
                this.Token = token;
                this.Type = type;
            }

            void fql_Finished(object sender, EventArgs e)
            {
                Finished.Set();
            }

            private void SaveDataToIsolatedStorage(string isoFileName, string value)
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                StreamWriter sw = new StreamWriter(isoStore.OpenFile(isoFileName, FileMode.OpenOrCreate));
                sw.Write(value);
                sw.Close();
                IsolatedStorageSettings.ApplicationSettings["DataLastSave"] = DateTime.Now;
            }

            internal void DoSubtask(object state)
            {
                fql.UserInfoFinished += new FQLCall.UserInfoEventHandler(fql_Finished);
                fql.CheckinFinished += new FQLCall.CheckinEventHandler(fql_Finished);
                fql.Finished += new FQLCall.FBEventHandler(fql_Finished);

               // fql.SuggestionsFinished += new FQLCall.SuggestionsEventHandler(fql_Finished);
                fql.FriendsFinished += new FQLCall.FriendsEventHandler(fql_Finished);
                switch (Type)
                {
                    case FqlTaskType.GetUserInfo:
                        {
                            if (AppSettings.UserSetting == null)
                            {
                                fql.GetUserInfo(Token);
                            }
                            else
                            {
                                FacebookData.CurrentUser = AppSettings.UserSetting;
                                Finished.Set();
                            }
                            break;
                        }
                    //case FqlTaskType.GetCheckins:
                    //    {
                    //        //fql.GetCheckins(Token, Utility.ConvertUTtcToEpoch(DateTime.Now.AddDays(-10)));
                    //        break;
                    //    }
                    case FqlTaskType.GetFriends:
                        {
                            fql.GetFriends(Token);
                            break;
                        }
                    case FqlTaskType.GetUpcomingEvents:
                        {
                            fql.GetUpcomingEvents(Token);
                            break;
                        }
                }
            }
        }

        #endregion
    }
}