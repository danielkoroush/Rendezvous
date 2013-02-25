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
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Collections.Generic;


namespace Paco.Application
{
    public class AppSettings
    {
        static IsolatedStorageSettings isolatedStore;

        public static readonly string FriendsKeyName = "friends";
        public static readonly string InviteesKeyName = "invitees";
        public static readonly string CurrentUserKeyName = "current_user";
        public static readonly string UserEventsKeyName = "user_events";
        private static readonly string OpenEventsKeyName = "open_events";
        private static readonly string RecentPlacesKeyName = "recent_places";
        private static readonly string CustomPlacesKeyName = "custom_places";
        private static readonly string RecentFriendsKeyName = "recent_friends";

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        static AppSettings()
        {
            try
            {
                // Get the settings for this application.
                isolatedStore = IsolatedStorageSettings.ApplicationSettings;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
            }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            try
            {
                // if new value is different, set the new value.
                if (isolatedStore[Key] != value)
                {
                    isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            catch (KeyNotFoundException)
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }
            catch (ArgumentException)
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }

            return valueChanged;
        }


        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;
            try
            {
                if (isolatedStore.Contains(Key))
                {
                    value = (valueType)isolatedStore[Key];
                }
                else
                {
                    value = defaultValue;
                }
            }
            catch (ArgumentException)
            {
                if (Debugger.IsAttached) Debugger.Break();
                value = defaultValue;
            }

            return value;
        }


        /// <summary>
        /// Save the settings.
        /// </summary>
        public static void Save()
        {
            isolatedStore.Save();
        }

        public static Dictionary<long, User> GetUsers(String key)
        {
            return GetValueOrDefault <Dictionary<long, User>>(key, null);
        }

        public static void SaveUsers(String key, Dictionary<long, User> value)
        {
            AddOrUpdateValue(key, value);
            Save();
        }

        public static User GetCurrentUserInfo()
        {
            return GetValueOrDefault<User>(CurrentUserKeyName, null);
        }

        public static void SaveCurrentUserInfo( User value)
        {
            AddOrUpdateValue(CurrentUserKeyName, value);
            Save();
        }

        public static Dictionary<long, Event> GetUserEventsInfo()
        {
            return GetValueOrDefault<Dictionary<long, Event>>(UserEventsKeyName, null);
        }

        public static List<Place> GetCustomPlaces()
        {
            return GetValueOrDefault<List<Place>>(CustomPlacesKeyName, null);
        }

        public static List<Place> GetRecentPlaces()
        {
            return GetValueOrDefault<List<Place>>(RecentPlacesKeyName, null);
        }

        public static void SaveUserEventsInfo(Dictionary<long,Event> value)
        {
            AddOrUpdateValue(UserEventsKeyName, value);
            //Save();
        }

        public static Dictionary<long, Event> GetOpenEventsInfo()
        {
            return GetValueOrDefault<Dictionary<long, Event>>(OpenEventsKeyName, null);
        }

        public static List<long> GetRecentFriends()
        {
            return GetValueOrDefault<List<long>>(RecentFriendsKeyName, null);
        }

        public static void SaveOpenEventsInfo(Dictionary<long, Event> value)
        {
            AddOrUpdateValue(OpenEventsKeyName, value);
            Save();
        }

        public static void SaveCustomPlaces(List<Place> value)
        {
            AddOrUpdateValue(CustomPlacesKeyName, value);
            Save();
        }

        public static void SaveRecentPlaces(List<Place> value)
        {
            AddOrUpdateValue(RecentPlacesKeyName, value);
            Save();
        }

        public static void SaveRecentFriends(List<long> value)
        {
            AddOrUpdateValue(RecentFriendsKeyName, value);
            Save();
        }

        public static void SaveInvitees(Dictionary<long, User> value)
        {
            AddOrUpdateValue(InviteesKeyName, value);
            Save();
        }

    }
}
