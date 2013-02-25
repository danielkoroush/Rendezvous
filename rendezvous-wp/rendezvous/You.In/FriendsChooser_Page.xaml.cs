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
using Paco.Application;
using Microsoft.Phone.Shell;

namespace You.In
{
    public partial class FriendsChooser_Page : PhoneApplicationPage
    {
        private FQLCall fqlCall = new FQLCall();

        public FriendsChooser_Page()
        {               
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

         
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!FacebookData.LoadedFriends)
            {
                fqlCall.Finished += new FQLCall.FBEventHandler(fqlCall_Finished);
                fqlCall.GetFriends(AppSettings.TokenSetting);
            }  
        }

        void fqlCall_Finished(object sender, EventArgs e)
        {

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (FacebookData.choosenFriends==null) FacebookData.choosenFriends = new List<long>();
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        void accepted_Click(object sender, EventArgs e)
        {
            String parameter = string.Empty;
            parameter = "invitees=" + FacebookData.choosenFriends.Count;
        }

        private void Stack_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                long uid = long.Parse((sender as StackPanel).Name);
              
            }
            catch (Exception e1)
            {
                string x = e1.Message;
            }
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                long uid = long.Parse((sender as StackPanel).Name);
            }
            catch (Exception e1)
            {
                string x = e1.Message;
            }
        }

        private void StackPanel_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            long uid = long.Parse((sender as StackPanel).Name);
        }

    }
}