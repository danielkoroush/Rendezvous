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
    public partial class SendMsg : PhoneApplicationPage
    {
        ApplicationBarIconButton sendMsg;
        public SendMsg()
        {
            InitializeComponent();


            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            sendMsg = new ApplicationBarIconButton(new Uri("/Images/appbar.send.rest.png", UriKind.Relative));
            sendMsg.Click += new EventHandler(sendMsg_Click);
            sendMsg.Text = "Send";
            ApplicationBar.Buttons.Add(sendMsg);
            ApplicationBarIconButton cancelled = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancelled.Text = "Cancel";
            cancelled.Click += new EventHandler(cancelled_Click);
            ApplicationBar.Buttons.Add(cancelled);
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        void sendMsg_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.UserPic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(this.NavigationContext.QueryString["friendPic"], UriKind.RelativeOrAbsolute));
           this.UserName.Text = this.NavigationContext.QueryString["friendName"];
           this.Subject.Text = this.NavigationContext.QueryString["eventName"];
           
        }
    }
}