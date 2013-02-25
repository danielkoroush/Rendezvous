using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using System.IO.IsolatedStorage;
using Paco.Application;
using System.Threading;

namespace You.In
{
    public partial class App : Application
    {
        private static MainViewModel viewModel = null;
        // Declare the dataObject variable as a public member of the Application class.
        // Real applications will use a more complex data structure, such as an XML
        // document. The only requirement is that the object be serializable.
        public string AppDataObject;
        static string _error_msg = String.Empty;
        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            if (AppSettings.UserSetting != null)
            {
                FacebookData.CurrentUser = AppSettings.UserSetting;
            }
        }

 

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (String.IsNullOrEmpty(AppSettings.ExceptionSetting))
            {
                //send the exception from previous session
            }

            if (PhoneApplicationService.Current.State.ContainsKey("userEvents"))
            {
                FacebookData.User_Events = PhoneApplicationService.Current.State["userEvents"] as Dictionary<long, Event>;
                if (FacebookData.User_Events != null)
                {
                    FacebookData.LoadedEvents = true;
                }
            }
            if (PhoneApplicationService.Current.State.ContainsKey("openEvents"))
            {
                FacebookData.Open_Events = PhoneApplicationService.Current.State["openEvents"] as Dictionary<long, Event>;
                if (FacebookData.Open_Events != null)
                {
                    FacebookData.LoadedSuggestions = true;
                }
            }

            if (PhoneApplicationService.Current.State.ContainsKey("friends"))
            {
                FacebookData.friends = PhoneApplicationService.Current.State["friends"] as Dictionary<long, User>;
                if (FacebookData.friends != null)
                {
                    FacebookData.LoadedFriends = true;
                }
            }
            if (PhoneApplicationService.Current.State.ContainsKey("user"))
            {
                FacebookData.CurrentUser = PhoneApplicationService.Current.State["user"] as User;
                AppSettings.UserSetting = FacebookData.CurrentUser;
            }
            if (PhoneApplicationService.Current.State.ContainsKey("recent"))
            {
                FacebookData.RecentPlaces = PhoneApplicationService.Current.State["recent"] as List<Place>;                
            }
            if (PhoneApplicationService.Current.State.ContainsKey("custom"))
            {
                FacebookData.CustomPlaces = PhoneApplicationService.Current.State["custom"] as List<Place>;                
            }
            if (PhoneApplicationService.Current.State.ContainsKey("suggestionsCount"))
            {
                try
                {
                    FQLCall._friends_processed = (int)PhoneApplicationService.Current.State["suggestionsCount"];
                }
                catch (Exception)
                {
                    FQLCall._friends_processed = 0;
                }
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
               // Store it in the State dictionary.
                if (FacebookData.User_Events!=null && FacebookData.User_Events.Count > 0) { PhoneApplicationService.Current.State["userEvents"] = FacebookData.User_Events; }
                if (FacebookData.Open_Events!=null) {
                    PhoneApplicationService.Current.State["openEvents"] = FacebookData.Open_Events;
                    PhoneApplicationService.Current.State["suggestionsCount"] = FQLCall._friends_processed;
                }
                
                if (FacebookData.friends!=null && FacebookData.friends.Count > 0) { PhoneApplicationService.Current.State["friends"] = FacebookData.friends; }
                if (FacebookData.CurrentUser != null) { PhoneApplicationService.Current.State["user"] = FacebookData.CurrentUser; }
                if (FacebookData.RecentPlaces != null && FacebookData.RecentPlaces.Count>0) { PhoneApplicationService.Current.State["recent"] = FacebookData.RecentPlaces; }
                if (FacebookData.CustomPlaces != null && FacebookData.CustomPlaces.Count > 0) { PhoneApplicationService.Current.State["custom"] = FacebookData.CustomPlaces; }                               
            // Also store it in Isolated Storage, in case the application is never reactivated.

        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            if (FacebookData.CurrentUser != null)
            {
                AppSettings.UserSetting = FacebookData.CurrentUser;
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                //System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!(e.ExceptionObject is AppExitException))
            {
                e.Handled = false;
                try
                {
                    _error_msg = e.ExceptionObject.Message + "\n" + e.ExceptionObject.StackTrace;
                    string header;
                    if (FacebookData.CurrentUser != null)
                    {
                        header = String.Format("id={0}&time={1}&dump={2}", FacebookData.CurrentUser.Uid, DateTime.Now.ToShortDateString(), _error_msg);
                    }
                    else
                    {
                        header = String.Format("id={0}&time={1}&dump={2}", "NULL", DateTime.Now.ToShortDateString(), _error_msg);
                    }
                     HttpWebRequest request3 = WebRequest.Create("http://ezvo.us/reporting.php?" +header ) as HttpWebRequest;
                    request3.BeginGetResponse(new AsyncCallback(PostErrorRequestCallback), request3);
                    //MessageBox.Show("It looks like we've encountered an error on our side :(\nWe're looking into it and should have a fix shortly.");

                }
                catch (Exception) { }
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }

            }
        }

        private void PostErrorRequestCallback(IAsyncResult ar)
        {           
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }


}