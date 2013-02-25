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
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace You.In
{
    public partial class Settings : PhoneApplicationPage
    {                

        ApplicationBarIconButton create;        

        public Settings()
        {
            InitializeComponent();

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            create = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            create.Text = "ok";
            create.Click += new EventHandler(create_Click);
            ApplicationBar.Buttons.Add(create);
        }        

        void create_Click(object sender, EventArgs e)
        {
            if (locationToggle.IsChecked.Value)
            {
                AppSettings.GeoLocation = true;
            }
            else
            {
                AppSettings.GeoLocation = false;
            }
            this.Dispatcher.BeginInvoke(() =>
        {
            NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
        });
        }

        private void locationToggle_Checked(object sender, RoutedEventArgs e)
        {            
            AppSettings.GeoLocation = true;
        }

        private void locationToggle_Unchecked(object sender, RoutedEventArgs e)
        {            
            AppSettings.GeoLocation = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            locationToggle.IsChecked = AppSettings.GeoLocation;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.URL = "http://rend.ezvo.us/privacy.html";
            task.Show();
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "support@grizzlyapps.com";
            emailComposeTask.Body = "";
            emailComposeTask.Subject = "Feedback/Bug Report";
            emailComposeTask.Show();
        }
    }
}