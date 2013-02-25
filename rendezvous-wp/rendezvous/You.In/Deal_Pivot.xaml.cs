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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace You.In
{
    public partial class Deal_Pivot : PhoneApplicationPage
    {
        Deal deal;
        ApplicationBarIconButton facebook;
        Dictionary<String, String> locations = new Dictionary<string, string>();
        int id;
        public static string CompanyName;
        public static StringBuilder Description;

        public Deal_Pivot()
        {
            InitializeComponent();

            Visibility v = (Visibility)Resources["PhoneLightThemeVisibility"];

            if (v != System.Windows.Visibility.Visible)
            {
                Uri uri = new Uri("/Images/powered_by_groupon2.png", UriKind.Relative); // Resource 
                System.Windows.Media.Imaging.BitmapImage imgSource = new System.Windows.Media.Imaging.BitmapImage(uri);
                grouponLogo.Source = imgSource;
            }

            CompanyName = string.Empty;
            Description = new StringBuilder();

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            facebook = new ApplicationBarIconButton(new Uri("/Images/facebook_icon.png", UriKind.Relative));
            facebook.Text = "create event";
            facebook.Click += new EventHandler(facebook_Click);
            facebook.IsEnabled = true;

            ApplicationBar.Buttons.Add(facebook);
        }

        void facebook_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Create3.xaml?deal=" + id, UriKind.Relative));
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                id = int.Parse(this.NavigationContext.QueryString["id"]);
                deal = Groupon.Deals[id];
                this.title.Text = deal.Title;
                image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(deal.Large_Image, UriKind.RelativeOrAbsolute));
                this.about.Text = deal.About;
                this.detail.Text = deal.Detail;
                this.endTime.Text = deal.EndTime.ToString();
                this.buyUrl.NavigateUri = new Uri(deal.DealUrl);
                this.buyUrl.Content = "click here to go to deal's offical website";
                ParseOptions();
                AddCompanyInfo();
            }
            catch (Exception) { }
        }

        private void AddCompanyInfo()
        {
            this.company.Text = (string)this.deal.Merchent["name"];
            CompanyName = this.company.Text;
            string url = (string)this.deal.Merchent["websiteUrl"];
            this.companyUrl.NavigateUri = new Uri(url);
            this.companyUrl.Content = url;
        }

        private void ParseOptions()
        {
            this.optionsPanel.Children.Clear();
            Description = new StringBuilder();
            Description.AppendLine("Purchase Options:");
            foreach (JToken token in deal.Options)
            {
                bool isSoldOut = (bool)token["isSoldOut"];
                if (!isSoldOut)
                {
                    JToken price = (JToken)token["price"];
                    JToken value = (JToken)token["value"];
                    AddOptions((string)price["formattedAmount"], (string)value["formattedAmount"]);
                    ParseLocation((JArray)token["redemptionLocations"]);
                    Description.AppendLine((string)token["buyUrl"]);
                }
            }
        }

        private void ParseLocation(JArray jarray)
        {
            this.locationStack.Children.Clear();
            Description.AppendLine("Locations:");
            foreach (JToken json in jarray)
            {
                StringBuilder sb = new StringBuilder();
                string address = (string)json["streetAddress1"];
                //  if (!locations.ContainsKey(address))
                //{
                sb.AppendLine(address);
                sb.Append((string)json["streetAddress2"]);
                sb.AppendLine(string.Format("{0}, {1}, {2}", (string)json["city"], (string)json["state"], (string)json["postalCode"]));
                string phone = (string)json["phoneNumber"];
                if (!string.IsNullOrEmpty(phone))
                {
                    sb.AppendLine(string.Format("phone: {0}", phone));
                }
                string geo = String.Format("{0},{1}", (float)json["lat"], (float)json["lng"]);
                //     locations.Add(address, sb.ToString());
                Description.AppendLine(sb.ToString());
                AddLocation(sb.ToString(), geo);
                //   }
            }
        }

        private void AddLocation(string address, string geo)
        {
            TextBlock addressBox = new TextBlock()
            {
                TextWrapping = TextWrapping.NoWrap,
                Width = 400,
                Tag = geo,
                Text = address,
                Margin = new Thickness(-14, 0, 0, 0),
                Style = (Style)Application.Current.Resources["PhoneTextTitle3Style"]
            };
            addressBox.MouseLeftButtonDown += new MouseButtonEventHandler(addressBox_MouseLeftButtonDown);
            this.locationStack.Children.Add(addressBox);
        }

        void addressBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock t = (TextBlock)sender;
            NavigationService.Navigate(new Uri(string.Format("/map.xaml?address={0}&geo={1}" , t.Text,t.Tag), UriKind.Relative));
        }

        private void AddOptions(string price, string value)
        {
            Description.AppendLine(string.Format("Price : {0} , Value : {1}", price, value));
            Canvas c = new Canvas()
            {
                Height = 40,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Width = 453,
                Margin = new Thickness(-5, 5, 0, 0),
            };
            TextBlock priceBox = new TextBlock()
            {
                TextWrapping = TextWrapping.NoWrap,
                Width = 250,
                Text = price,
                Margin = new Thickness(16, 0, 0, 0),
                Style = (Style)Application.Current.Resources["PhoneTextTitle3Style"]
            };
            TextBlock valueBox = new TextBlock()
            {
                TextWrapping = TextWrapping.NoWrap,
                Width = 150,
                Text = value,
                Margin = new Thickness(273, 0, 0, 0),
                Style = (Style)Application.Current.Resources["PhoneTextTitle3Style"]
            };
            c.Children.Add(priceBox);
            c.Children.Add(valueBox);
            this.optionsPanel.Children.Add(c);
        }
    }
}