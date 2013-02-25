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

namespace Paco.Application
{
    public class FacebookData
    {
        public static Place choosenPlace;
        public static List<User> TaggedUsers = new List<User>();

        private static List<Place> _customPlaces = new List<Place>();
        public static List<Place> CustomPlaces
        {
            get
            {
                if (_customPlaces.Count == 0)
                {
                    _customPlaces = AppSettings.GetCustomPlaces();
                }
                if (_customPlaces != null)
                {
                    return _customPlaces;
                }
                _customPlaces = new List<Place>();
                return _customPlaces;
            }
            set
            {
                _customPlaces = value;
            }
        }
        public static List<Place> _recentPlaces = new List<Place>();
        public static List<Place> RecentPlaces
        {
            get
            {
                if (_recentPlaces.Count == 0)
                {
                    _recentPlaces = AppSettings.GetRecentPlaces();
                }
                if (_recentPlaces != null)
                {
                    return _recentPlaces;
                }
                _recentPlaces = new List<Place>();
                return _recentPlaces;
            }
            set
            {
                _recentPlaces = value;
            }
        }

        public static void AddRecentPlace(Place place)
        {
            foreach (Place p in _recentPlaces)
            {
                if (p.Name.Equals(place.Name) && p.address.Equals(place.address))
                {
                    return;
                }
            }
            if (_recentPlaces.Count > 10)
            {
                _recentPlaces.RemoveAt(0);
            }
            _recentPlaces.Add(place);
            AppSettings.SaveRecentPlaces(_recentPlaces);
        }

        public static void AddCustomPlace(Place place)
        {
            if (_customPlaces == null)
            {
                _customPlaces = new List<Place>();
            }
            if (_customPlaces.Count > 10)
            {
                _customPlaces.RemoveAt(0);
            }
            _customPlaces.Add(place);
            AppSettings.SaveCustomPlaces(_customPlaces);
        }

        //  public static Dictionary<long, Event> Open_Events = new Dictionary<long, Event>();
        private static bool _loadedFriends = false;
        public static bool LoadedFriends
        {
            set
            {
                _loadedFriends = value;
            }
            get
            {
                return _loadedFriends;
            }
        }

        private static bool _loadedCheckins;
        public static bool LoadedCheckins
        {
            set
            {
                _loadedCheckins = value;
            }
            get
            {
                return _loadedCheckins;
            }
        }

        private static bool _loadedSuggestions = false;
        public static bool LoadedSuggestions
        {
            set
            {
                _loadedSuggestions = value;
            }
            get
            {
                return _loadedSuggestions;
            }
        }

        private static bool _loadedEvents = false;
        public static bool LoadedEvents
        {
            set
            {
                _loadedEvents = value;
            }
            get
            {
                return _loadedEvents;
            }
        }

        public static List<long> choosenFriends;

        private static Dictionary<long, Checkin> _checkins = new Dictionary<long, Checkin>();
        public static Dictionary<long, Checkin> Checkins
        {
            get
            {
                return _checkins;
            }
            set
            {
                _checkins = value;
            }
        }

        private static Dictionary<long, User> _friends = new Dictionary<long, User>();
        public static Dictionary<long, User> friends
        {
            get
            {
                return _friends;
            }
            set
            {
                _friends = value;
            }
        }

        private static List<User> _friendslist = new List<User>();
        public static List<User> friendsList
        {
            get
            {
                return _friendslist;
            }
            set
            {
                _friendslist = value;
            }
        }

        private static User _currentUser;
        public static User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    return AppSettings.GetCurrentUserInfo();
                }
                else
                {
                    return _currentUser;
                }
            }
            set
            {
                _currentUser = value;
              //  AppSettings.SaveCurrentUserInfo(value);
            }
        }

        private static Dictionary<long, Event> _user_events = new Dictionary<long, Event>();
        public static Dictionary<long, Event> User_Events
        {
            get
            {
                return _user_events;
            }
            set
            {
                _user_events = value;
            }
        }

        private static Dictionary<long, Event> _open_events = new Dictionary<long, Event>();
        public static Dictionary<long, Event> Open_Events
        {
            get
            {
                //if (_loadedSuggestions && (_open_events == null))
                //{
                //    _open_events=AppSettings.GetOpenEventsInfo();
                //}
                return _open_events;
            }
            set
            {
                _open_events = value;
            }
        }

        private static Dictionary<long, User> _invitees = new Dictionary<long, User>();
        public static Dictionary<long, User> Invitees
        {
            get
            {
                return _invitees;
            }
            set
            {
                _invitees = value;
            }
        }

        public static List<User> GetSortedUsersList(List<User> Users)
        {
            if (Users == null) return null;
            List<User> results = new List<User>();
            List<User> friends = new List<User>();
            List<User> non_friends = new List<User>();
            foreach (User u in Users)
            {
                if (u.Uid != FacebookData.CurrentUser.Uid)
                {
                    if (FacebookData.friends.ContainsKey(u.Uid))
                    {
                        friends.Add(u);
                    }
                    else
                    {
                        non_friends.Add(u);
                    }
                }
            }
            friends.Sort();
            non_friends.Sort();
            results.InsertRange(0, friends);
            results.InsertRange(results.Count, non_friends);
            return results;
        }
    }
}
