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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Paco.Application
{
    public class Deal : IComparable<Deal>
    {
        string id;
        string small_image = "/Images/event_thumb.png";
        string large_image;
        DateTime end_time;
        string title;
        string detail;
        string about;
        JArray options;
        JToken merchent;
        string dealUrl;

        public Deal(JToken json)
        {
            this.id = (String)json["id"];
            this.small_image = (String)json["sidebarImageUrl"];
            this.title = (String)json["announcementTitle"];
            this.large_image = (String)json["largeImageUrl"];
            this.id = (String)json["id"];
            this.end_time = DateTime.Parse((String)json["endAt"]);
            this.detail = Convert((String)json["highlightsHtml"]);
            this.about = Convert((String)json["pitchHtml"]);
            this.options = (JArray)json["options"];
            this.merchent = (JToken)json["merchant"];
            this.dealUrl = (string)json["dealUrl"];
        }

        public Deal(string title)
        {
            this.title = title;
        }

        public string DealUrl
        {
            get
            {
                return this.dealUrl;
            }
        }

        public JToken Merchent
        {
            get
            {
                return merchent;
            }
        }

        public JArray Options
        {
            get
            {
                return options;
            }
        }

        public String Detail
        {
            get
            {
                return detail;
            }
        }

        public String About
        {
            get
            {
                return about;
            }
        }

        public String Id
        {
            get
            {
                return id;
            }
        }

        public String Title
        {
            get
            {
                return title;
            }
        }

        public String Large_Image
        {
            get
            {
                return large_image;
            }
        }

        public String Small_Image
        {
            get
            {
                return small_image;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return end_time;
            }
        }

        public static string Convert(string input)
        {
            string returnString = Regex.Replace(input, "<[^>]*>", "");
            // Decode HTML entities
            returnString = HttpUtility.HtmlDecode(returnString);
            return returnString;
        }

        public int CompareTo(Deal obj)
        {
            if (obj == null) return -1;
            return end_time.CompareTo(obj.end_time);
        }
    }
}
