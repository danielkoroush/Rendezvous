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
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Paco.Application
{
    [DataContract(Name = "Event", Namespace = "Paco.Application")]
    public class Event : IComparable<Event>
    {
        private long id;
        private string name;
        private string host;
        private long start_time;
        private long end_time;
        private JToken venue;
        private string location;
        private string pic;
        private string description;
        private Dictionary<RSVP,List<User>> invitees;
        private RSVP rsvp;
        private Dictionary<long, RSVP> associatedFriends = new Dictionary<long, RSVP>();
        private bool _loaded_guests = false;

        public Event(JToken json)
        {
            rsvp = Paco.Application.RSVP.not_replied;
            this.id = (long)json["eid"];
            this.name = (String)json["name"];
            this.start_time = (long)json["start_time"];
            this.end_time = (long)json["end_time"];
            this.pic = (String)json["pic_big"];
            this.description = (String)json["description"];
            this.host = (String)json["host"];
            this.venue = json["venue"];
            this.location = (String)json["location"];
            invitees= new Dictionary<RSVP, List<User>>();
            associatedFriends = new Dictionary<long, RSVP>();
        }

        [DataMember(Name = "AssociatedFriends", EmitDefaultValue = true, IsRequired = true, Order = 12)]
        public Dictionary<long, RSVP> AssosicatedFriends
        {
            get
            {
                return associatedFriends;
            }
            set
            {
                associatedFriends = value;
            }
        }

        public void AddAssociateFriend(long id, string rsvp)
        {
            if (associatedFriends == null)
            {
                associatedFriends = new Dictionary<long, RSVP>();
            }
            if (!associatedFriends.ContainsKey(id)) associatedFriends.Add(id, parseFacbookResponse(rsvp));
        }

        public void AddAssociateFriend(long id, RSVP rsvp)
        {
            if (associatedFriends == null)
            {
                associatedFriends = new Dictionary<long, RSVP>();
            }
            if (!associatedFriends.ContainsKey(id)) associatedFriends.Add(id, rsvp);
        }

        public Dictionary<long,RSVP> GetAssociatedFriends()
        {
            return associatedFriends;
        }


        public void AddInvitees(User u, String response)
        {
            RSVP status = parseFacbookResponse(response);
            if (!invitees.ContainsKey(status))
            {
                List<User> list = new List<User>();
                list.Add(u);
                invitees.Add(status, list);
            }
            else
            {
                invitees[status].Add(u);
            }
            if (FacebookData.friends.ContainsKey(u.Uid) && !associatedFriends.ContainsKey(u.Uid))
            {
                associatedFriends.Add(u.Uid, status);
            }
        }


        private static RSVP parseFacbookResponse(String response)
        {
            if (response.Equals("attending", StringComparison.InvariantCultureIgnoreCase))
            {
                return RSVP.attending;
            }
            else if (response.Equals("declined", StringComparison.InvariantCultureIgnoreCase))
            {
                return RSVP.declined;
            }
            else if (response.Equals("unsure", StringComparison.InvariantCultureIgnoreCase))
            {
                return RSVP.maybe;
            }
            else
            {
                return RSVP.not_replied;
            }
        }

        [DataMember(Name = "RSVP", EmitDefaultValue = true, IsRequired = true, Order = 11)]
        public RSVP RSVP
        {
            get
            {
                return rsvp;
            }
            set
            {
                rsvp = value;
            }
        }


        [DataMember(Name = "Loaded", EmitDefaultValue = true, IsRequired = true, Order = 12)]
        public bool LoadedGuests
        {
            get
            {
                return _loaded_guests;
            }
            set
            {
                _loaded_guests = value;
            }
        }

        public void SetRSVP(String status)
        {
            if (status.Equals("attending", StringComparison.InvariantCultureIgnoreCase))
            {
                rsvp = RSVP.attending;
            }
            else if (status.Equals("declined", StringComparison.InvariantCultureIgnoreCase))
            {
                rsvp = RSVP.declined;
            }
            else if (status.Equals("unsure", StringComparison.InvariantCultureIgnoreCase))
            {
                rsvp = RSVP.maybe;
            }
            else
            {
                rsvp = RSVP.not_replied;
            }
        }
        /// <summary>
        /// Return the RSVP count for Accepted 
        /// </summary>
        /// <returns></returns>
        public int GetAcceptedCount()
        {
            if (!invitees.ContainsKey(RSVP.attending))
            {
                return 0;
            }
            return invitees[RSVP.attending].Count;
        }

        /// <summary>
        /// Returns the RSVP count for Declined
        /// </summary>
        /// <returns></returns>
        public int GetDeclinedCount()
        {
            if (!invitees.ContainsKey(RSVP.declined))
            {
                return 0;
            }
            return invitees[RSVP.declined].Count;
        }

        /// <summary>
        /// Returns the RSVP count for Unsure
        /// </summary>
        /// <returns></returns>
        public int GetUnsureCount()
        {
            if (!invitees.ContainsKey(RSVP.maybe))
            {
                return 0;
            }
            return invitees[RSVP.maybe].Count;
        }

        /// <summary>
        /// Return the RSVP count for No Reply
        /// </summary>
        /// <returns></returns>
        public int GetNoReponseCount()
        {
            if (!invitees.ContainsKey(RSVP.not_replied))
            {
                return 0;
            }
            return invitees[RSVP.not_replied].Count;
        }

        /// <summary>
        /// event id
        /// </summary>
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

        /// <summary>
        /// event name
        /// </summary>
        [DataMember(Name = "Name", EmitDefaultValue = true, IsRequired = true, Order = 2)]
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// event start time
        /// </summary>
        [DataMember(Name = "StartTime", EmitDefaultValue = true, IsRequired = true, Order = 3)]
        public long StartTime
        {
            get
            {
                return start_time;
            }
            set
            {
                start_time = value;
            }
        }

        /// <summary>
        /// event end time
        /// </summary>
        [DataMember(Name = "EndTime", EmitDefaultValue = true, IsRequired = true, Order = 4)]
        public long EndTime
        {
            get
            {
                return end_time;
            }
            set
            {
                end_time = value;
            }
        }

        [DataMember(Name = "Location", EmitDefaultValue = true, IsRequired = true, Order = 5)]
        public String Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        public List<User> GetAllInvitees(RSVP status)
        {
            if (invitees.ContainsKey(status))
            {
                return invitees[status];
            }
            return null;
        }


        [DataMember(Name = "Host", EmitDefaultValue = true, IsRequired = true, Order = 6)]
        public String Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
            }
        }

        [DataMember(Name = "Pic", EmitDefaultValue = true, IsRequired = true, Order = 7)]
        public String Picture
        {
            get
            {
                return pic;
            }
            set
            {
                pic = value;
            }
        }

        [DataMember(Name = "Description", EmitDefaultValue = true, IsRequired = true, Order = 8)]
        public String Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        [DataMember(Name = "Venue", EmitDefaultValue = true, IsRequired = true, Order = 9)]
        public String Venue
        {
            get
            {
                return venue.ToString();
            }
            set
            {
                venue = JObject.Parse(value);
            }
        }

        [DataMember(Name = "Invitees", EmitDefaultValue = true, IsRequired = true, Order = 10)]
        public Dictionary<RSVP,List<User>> Invitees
        {
            get
            {
                return invitees;
            }
            set
            {
                invitees = value;
            }
        }

        public String Venue_Street
        {
            get
            {
                try
                {
                    return (string)venue["street"];
                }catch (ArgumentNullException){
                    return String.Empty;
                }
            }           
        }

        public String Venue_City
        {
            get
            {
                try
                {
                    return (string)venue["city"];
                }
                catch (ArgumentNullException)
                {
                    return String.Empty;
                }
            }
        }

        public String Venue_State
        {
            get
            {
                try
                {
                    return (string)venue["state"];
                }
                catch (ArgumentNullException)
                {
                    return String.Empty;
                }
            }
        }

        public String Venue_Country
        {
            get
            {
                try
                {
                    return (string)venue["country"];
                }
                catch (ArgumentNullException)
                {
                    return String.Empty;
                }
            }
        }

        public String Venue_Latitude
        {
            get
            {
                try
                {
                    return ((float)venue["latitude"]).ToString();
                }
                catch (ArgumentNullException)
                {
                    return String.Empty;
                }
            }
        }

        public String Venue_Longitude
        {
            get
            {
                try
                {
                    return ((float)venue["longitude"]).ToString();
                }
                catch (ArgumentNullException)
                {
                    return String.Empty;
                }
            }
        }

        public String GetAddress()
        {
            StringBuilder result = new StringBuilder();
            if (!String.IsNullOrEmpty(Location))
            {
                result.AppendLine(Location);
            }
            if (venue!=null)
            {
                result.AppendLine(Venue_Street);
                result.AppendLine(String.Format("{0} {1}",Venue_City,Venue_State));
            }
            return result.ToString();
        }

        public override String ToString()
        {
            return String.Format("Name: {0} Guests: {1}", name, Invitees.Count);
        }

        public int CompareTo(Event obj)
        {
            if (obj == null) return -1;
            return start_time.CompareTo(obj.start_time);               
        }
    }
}
