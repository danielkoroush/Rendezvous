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
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;

namespace You.In
{
    public partial class map : PhoneApplicationPage
    {
        public map()
        {
            InitializeComponent();
            this.map1.CredentialsProvider = new ApplicationIdCredentialsProvider("AhGs2lwlgYWijQdXshst6ZGVLD1Gm3oVUpiO8CEajIV2sAYerOB1UgP7TV1D4mNl");
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.address.Text = this.NavigationContext.QueryString["address"];
            string geo = this.NavigationContext.QueryString["geo"];
            this.pushpin.Content = Deal_Pivot.CompanyName;

            string [] temp = geo.Split(',');
            GeoCoordinate geo1 = new GeoCoordinate(double.Parse(temp[0]),double.Parse(temp[1]));
            this.map1.Center = geo1;
            this.pushpin.Location = geo1;            
        }
    }
}