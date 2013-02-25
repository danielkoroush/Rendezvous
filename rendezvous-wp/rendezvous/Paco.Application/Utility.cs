using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using Delay;

namespace Paco.Application
{
    public class Utility
    {

        public static String ConvertUTtcToEpoch(DateTime UtcTime)
        {
            double d = (UtcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return (String.Format("{0:##}", (UtcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds));
        }


        public static String GetCurrentTimeInEpoch()
        {
            return (String.Format("{0:##}", (double)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds));
        }


        public static DateTime ConvertEpochToUtc(double epoch)
        {
            long baseTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            long tickResolution = 10000000;
            long e = long.Parse(epoch.ToString());
            long epochTicks = (e * tickResolution) + baseTicks;
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            DateTime utcTime = new DateTime(epochTicks, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Local);
        }

        public static void GetWebException(WebException ex)
        {
            // To help with debugging, we grab the exception stream to get full error details
            StreamReader errorStream = null;
            try
            {
                errorStream = new StreamReader(ex.Response.GetResponseStream());
                Console.WriteLine(errorStream.ReadToEnd());
            }
            finally
            {
                if (errorStream != null) errorStream.Close();
            }
        }

        /// <summary>
        /// deprecated. use itemviewmodel
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static Canvas CreateEventItem(User u)
        {
            Canvas c = new Canvas();
            c.Height = 115;
            c.VerticalAlignment = VerticalAlignment.Top;
            c.HorizontalAlignment = HorizontalAlignment.Left;
            c.Margin = new Thickness(0, 0, 0, 0);

            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Height = 105;
            border.Width = 105;
            border.BorderThickness = new Thickness(1, 1, 1, 1);


            Image i = new Image();
            i.Height = 95;
            i.Width = 95;
            //  i.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(u.Pic, UriKind.RelativeOrAbsolute));
            RectangleGeometry rec = new RectangleGeometry();
            rec.Rect = new Rect(0, 0, 95, 95);
            i.Clip = rec;
            LowProfileImageLoader.SetUriSource(i, new Uri(u.Pic, UriKind.RelativeOrAbsolute));
            border.Child = i;

            TextBlock name = new TextBlock();
            name.Text = u.Name;
            name.FontSize = 30;
            name.FontFamily = new FontFamily("Segoe WP Light");
            name.HorizontalAlignment = HorizontalAlignment.Right;
            name.VerticalAlignment = VerticalAlignment.Top;
            name.Height = 43;
            name.Width = 313;
            name.Margin = new Thickness(110, 0, 0, 0);

            c.Children.Add(border);
            c.Children.Add(name);

            return c;
        }
        /// <summary>
        /// deperecated... use ItemViewModel
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static Canvas CreateUserItem(User u)
        {
            Canvas c = new Canvas();
            c.Height = 87;
            c.VerticalAlignment = VerticalAlignment.Top;
            c.HorizontalAlignment = HorizontalAlignment.Left;
            c.Margin = new Thickness(0, 0, 0, 0);

            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Height = 81;
            border.Width = 81;
            border.BorderThickness = new Thickness(1, 1, 1, 1);

            Image i = new Image();
            i.Height = 80;
            i.Width = 80;
            try
            {
                //i.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(u.Pic, UriKind.RelativeOrAbsolute));
                LowProfileImageLoader.SetUriSource(i, new Uri(u.Pic, UriKind.RelativeOrAbsolute));
            }
            catch (ArgumentNullException)
            {
                LowProfileImageLoader.SetUriSource(i, new Uri("http://static.ak.fbcdn.net/rsrc.php/zl/r/05ikEGl-CqS.png", UriKind.RelativeOrAbsolute));
                //i.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("http://static.ak.fbcdn.net/rsrc.php/zl/r/05ikEGl-CqS.png", UriKind.RelativeOrAbsolute));                
            }

            RectangleGeometry rec = new RectangleGeometry();
            rec.Rect = new Rect(0, 0, 80, 80);
            i.Clip = rec;

            border.Child = i;

            TextBlock name = new TextBlock();
            name.Text = u.Name;
            name.FontSize = 30;
            name.FontFamily = new FontFamily("Segoe WP Light");
            name.HorizontalAlignment = HorizontalAlignment.Right;
            name.VerticalAlignment = VerticalAlignment.Center;
            name.Height = 43;
            name.Width = 313;
            name.Margin = new Thickness(92, 0, 0, 0);

            TextBlock id = new TextBlock();
            id.Text = u.Uid.ToString();
            id.Visibility = Visibility.Collapsed;
            if (FacebookData.friends.ContainsKey(u.Uid))
            {
                // Create a blue and a black Brush
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Orange;
                name.Foreground = brush;
            }

            c.Children.Add(border);
            c.Children.Add(name);
            c.Children.Add(id);

            return c;
        }
        public static Canvas CreateCheckinItem(Checkin checkin, User u)
        {
            Canvas c = new Canvas();
            c.Height = 112;
            c.VerticalAlignment = VerticalAlignment.Top;
            c.HorizontalAlignment = HorizontalAlignment.Left;
            c.Margin = new Thickness(0, 0, 0, 0);

            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Height = 100;
            border.Width = 100;
            border.BorderThickness = new Thickness(1, 1, 1, 1);

            Image i = new Image();
            i.Height = 90;
            i.Width = 90;
            // i.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(u.Pic, UriKind.RelativeOrAbsolute));
            RectangleGeometry rec = new RectangleGeometry();
            rec.Rect = new Rect(0, 0, 100, 173);
            i.Clip = rec;
            LowProfileImageLoader.SetUriSource(i, new Uri(u.Pic, UriKind.RelativeOrAbsolute));
            border.Child = i;

            TextBlock name = new TextBlock();
            name.Text = u.Name;
            name.FontSize = 30;
            name.FontWeight = FontWeights.Bold;
            name.FontFamily = new FontFamily("Segoe WP Light");
            name.HorizontalAlignment = HorizontalAlignment.Right;
            name.VerticalAlignment = VerticalAlignment.Top;
            name.Height = 30;
            name.Width = 313;
            name.Margin = new Thickness(129, 4, 0, 0);

            TextBlock place = new TextBlock();
            place.Text = checkin.PlaceName;
            place.FontSize = 25;
            // place.FontWeight = FontWeights.Bold;
            place.FontFamily = new FontFamily("Segoe WP Light");
            place.HorizontalAlignment = HorizontalAlignment.Right;
            place.VerticalAlignment = VerticalAlignment.Top;
            place.Height = 30;
            place.Width = 313;
            place.Margin = new Thickness(129, 39, 0, 0);

            TextBlock time = new TextBlock();
            time.Text = Utility.ConvertEpochToUtc(checkin.Time_Stamp).ToString("ddd, MMM dd");
            time.FontSize = 20;
            // time.FontWeight = FontWeights.Bold;
            time.FontFamily = new FontFamily("Segoe WP Light");
            time.HorizontalAlignment = HorizontalAlignment.Right;
            time.VerticalAlignment = VerticalAlignment.Top;
            time.Height = 30;
            time.Width = 313;
            time.Margin = new Thickness(129, 65, 0, 0);

            TextBlock id = new TextBlock();
            id.Text = checkin.ID.ToString();
            id.Visibility = Visibility.Collapsed;

            c.Children.Add(id);
            c.Children.Add(border);
            c.Children.Add(name);
            c.Children.Add(place);
            c.Children.Add(time);

            return c;
        }


    }
}
