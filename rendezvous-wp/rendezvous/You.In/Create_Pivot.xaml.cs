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
    public partial class Create_Pivot : PhoneApplicationPage
    {
        public static DateTime startTime = DateTime.Now;
        public static DateTime endTime = DateTime.Now.AddHours(3);
        public static DateTime startDate = DateTime.Now;
        public static DateTime endDate = endTime;
        static bool _isPublic = false;
        ApplicationBarIconButton save;
        ApplicationBarIconButton cancelled;
        public Create_Pivot()
        {
            InitializeComponent();

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            save = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            save.Text = "create";
            save.Click += new EventHandler(save_Click);

            cancelled = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancelled.Text = "cancel";
            cancelled.Click += new EventHandler(cancelled_Click);

            start_dateBox.Text = startDate.Date.ToShortDateString();
            end_dateBox.Text = endDate.Date.ToShortDateString();
            timePicker1.Value = startTime;
            end_timePicker.Value = endTime;
            isPublic.IsChecked = _isPublic;

            ApplicationBar.Buttons.Add(save);
            ApplicationBar.Buttons.Add(cancelled);
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
        }

        void save_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void txtbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



        private void start_dateBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/DatePicker.xaml?date="+startDate.Date.ToShortDateString(), UriKind.Relative));
        }

        private void end_dateBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/DatePicker2.xaml?date=" + endDate.Date.ToShortDateString(), UriKind.Relative));
        }

        private void timePicker1_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            startTime = (DateTime)timePicker1.Value;
        }

        private void end_timePicker_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            endTime = (DateTime)end_timePicker.Value;
        }

        private void isPublic_Checked(object sender, RoutedEventArgs e)
        {
            _isPublic = true;
        }

        private void isPublic_Unchecked(object sender, RoutedEventArgs e)
        {
            _isPublic = false;
        }

    }
}