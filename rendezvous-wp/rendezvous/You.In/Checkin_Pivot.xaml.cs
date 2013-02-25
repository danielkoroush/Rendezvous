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
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Tasks;

namespace You.In
{
    public partial class Checkin_Pivot : PhoneApplicationPage
    {
        private long _id;
        Checkin _checkin;
        FQLCall fql = new FQLCall();
        Pushpin pin = new Pushpin();

        public Checkin_Pivot()
        {
            InitializeComponent();


        }


        void fql_TaggedUsersInfoFinished(object sender, EventArgs e)
        {
             FacebookData.TaggedUsers.Sort();
            this.Dispatcher.BeginInvoke(() =>
            {
             foreach (User u in FacebookData.TaggedUsers)
              {
                  listBox1.Items.Add(new ItemViewModel() { EventTitle = u.Name, PicSource = u.Pic });
             }
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            fql.TaggedUsersInfoFinished+=new FQLCall.
                TaggedUsersInfoEventHandler(fql_TaggedUsersInfoFinished); 
            if (this.NavigationContext.QueryString.ContainsKey("id"))
            {
                _id = long.Parse(this.NavigationContext.QueryString["id"]);
                _checkin = FacebookData.Checkins[_id];
                if (String.IsNullOrEmpty(_checkin.Message))
                    this.name.Visibility = Visibility.Collapsed;
                this.name.Text = _checkin.Message;
                this.CheckinPlace.Text = _checkin.PlaceName;
                this.CheckinTime.Text = Utility.ConvertEpochToUtc(_checkin.Time_Stamp).ToString("ddd, MMM dd @ ");
                this.CheckinTime.Text += Utility.ConvertEpochToUtc(_checkin.Time_Stamp).ToString("t");
                try
                {
                    this.UserName.Text = FacebookData.friends[_checkin.Uid].Name;
                    this.UserPic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(FacebookData.friends[_checkin.Uid].Pic, UriKind.RelativeOrAbsolute));
                }
                catch (KeyNotFoundException)
                {
                    this.UserName.Text = FacebookData.CurrentUser.Name;
                    this.UserPic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(FacebookData.CurrentUser.Pic, UriKind.RelativeOrAbsolute));
                }
               this.map1.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_checkin.Place.GetMapImage(), UriKind.RelativeOrAbsolute));
                String tags = _checkin.Tags;
                if (!String.IsNullOrEmpty(tags))
                {
                    fql.GetTagUsers(AppSettings.TokenSetting, tags);
                }
                else
                {

                    listBox1.Items.Add(new ItemViewModel() { EventTitle = "No Tags." });
                }
            }
        }

        private void map1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var task = new WebBrowserTask();
                task.URL = String.Format("maps:{0}%2C{1}", _checkin.Place.Lattitude, _checkin.Place.Longitude);
                task.Show();
            }
            catch (Exception e1)
            {
                //Could not get the geo cordinates for the checkin place.
                //to_remove
                MessageBox.Show(e1.Message);
            }
        }
    }
}