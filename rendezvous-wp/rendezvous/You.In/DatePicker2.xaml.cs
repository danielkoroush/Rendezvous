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

namespace You.In
{
    public partial class DatePicker2 : PhoneApplicationPage
    {
        ApplicationBarIconButton send;
        ApplicationBarIconButton cancel;
        public DatePicker2()
        {
            InitializeComponent();
            send = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            send.Text = "publish";
            send.Click += new EventHandler(send_Click);

            cancel = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancel.Text = "cancel";
            cancel.Click += new EventHandler(cancel_Click);

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(send);
            ApplicationBar.Buttons.Add(cancel);
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("date"))
            {
                Cal.SelectedDate = DateTime.Parse(this.NavigationContext.QueryString["date"]);
            }
        }

        void cancel_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Create3.xaml", UriKind.Relative));
        }

        void send_Click(object sender, EventArgs e)
        {
            Create3.endDate= Cal.SelectedDate;
            NavigationService.Navigate(new Uri("/Create3.xaml", UriKind.Relative));
        }

    }
}