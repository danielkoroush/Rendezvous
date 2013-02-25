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
using System.Runtime.Serialization;

namespace Paco.Application
{
    [DataContract(Name = "Checkin", Namespace = "Paco.Application")]
    public class Checkin : IComparable<Checkin>
    {
        public Checkin(long id, long uid, long timestamp)
        {
            this.id = id;
            this.uid = uid;
            this.timestamp = timestamp;
        }

        public Checkin(long id, long uid, long timestamp, Place p, JToken tags)
        {
            this.id = id;
            this.uid = uid;
            this.timestamp = timestamp;
            this._place = p;
            this.placename = p.Name;
            String temp = tags.ToString().Replace("[",String.Empty);
            temp = temp.Replace("]",String.Empty);
            this._tags = temp;
        }
        
        private String _tags;
        [DataMember(Name = "Tags", EmitDefaultValue = true, IsRequired = true, Order = 2)]
        public String Tags
        {
            get
            {
                return _tags;
            }
            set 
            {
                _tags = value;
            }
        }

        private long id;
                [DataMember(Name = "ID", EmitDefaultValue = true, IsRequired = true, Order = 1)]
        public long ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        private String placename;
        [DataMember(Name = "Name", EmitDefaultValue = true, IsRequired = true, Order = 3)]
        public String PlaceName
        {
            set
            {
                placename = value;
            }
            get
            {
                return placename;
            }
        }
        
        private Place _place;
        [DataMember(Name = "Place", EmitDefaultValue = true, IsRequired = true, Order = 4)]
        public Place Place
        {
            get
            {
                return _place;
            }
            set 
            {
                _place = value;
            }
        }


        public long uid;
        [DataMember(Name = "UID", EmitDefaultValue = true, IsRequired = true, Order = 5)]
        public long Uid
        {
            get
            {
                return uid;
            }
            set
            {
                uid = value;
            }
        }
        
        private long page_id;
        [DataMember(Name = "Page_ID", EmitDefaultValue = true, IsRequired = true, Order = 6)]
        public long Page_Id
        {
            get
            {
                return page_id;
            }
            set
            {
                page_id = value;
            }
        }
        
        private long post_id;
        [DataMember(Name = "Post_ID", EmitDefaultValue = true, IsRequired = true, Order = 7)]
        public long Post_Id
        {
            get
            {
                return post_id;
            }
            set
            {
                post_id = value;
            }
        }
        
        private long timestamp;
        [DataMember(Name = "Time", EmitDefaultValue = true, IsRequired = true, Order = 8)]
        public long Time_Stamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
            }
        }
        
        private string message;
        [DataMember(Name = "Message", EmitDefaultValue = true, IsRequired = true, Order = 9)]
        public String Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }

        public override string ToString()
        {
            return uid + "    " + Utility.ConvertEpochToUtc(timestamp);
        }

        public int CompareTo(Checkin obj)
        {
           return obj.timestamp.CompareTo(timestamp);
        }
    }
}
