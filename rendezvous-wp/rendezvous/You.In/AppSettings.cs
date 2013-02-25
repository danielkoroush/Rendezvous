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
using Paco.Application;

namespace You.In
{
    public static class AppSettings
    {
        static IsolatedStorageSettings isolatedStore;
        const string ToeknKeyName = "AccessToken";
        const string GeoLocationKeyName = "GeoLocation";
        const string UserKeyName = "CurrrentUser";
        const string ExceptionKeyName = "ExceptionStack";
        const User UserSettingDefault= null;
        const string ToeknSettingDefault = "";
        const bool GeoLocationSettingDefault = true;
        const string ExceptionSettingDefault = "";


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
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
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
                value = (valueType)isolatedStore[Key];
            }
            catch (KeyNotFoundException)
            {
                value = defaultValue;
            }
            catch (ArgumentException)
            {
                value = defaultValue;
            }

            return value;
        }


        /// <summary>
        /// Save the settings.
        /// </summary>
        private static void Save()
        {
            isolatedStore.Save();
        }


        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        public static String TokenSetting
        {
            get
            {
                return GetValueOrDefault<String>(ToeknKeyName, ToeknSettingDefault);
            }
            set
            {
                AddOrUpdateValue(ToeknKeyName, value);
                Save();
            }
        }

        public static bool GeoLocation
        {
            get
            {
                return GetValueOrDefault<bool>(GeoLocationKeyName, GeoLocationSettingDefault);
            }
            set
            {
                AddOrUpdateValue(GeoLocationKeyName, value);
                Save();
            }
        }

        public static bool BackToLogin
        {
            get;
            set;
        }

        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        public static String ExceptionSetting
        {
            get
            {
                return GetValueOrDefault<String>(ExceptionKeyName, ExceptionSettingDefault);
            }
            set
            {
                AddOrUpdateValue(ExceptionKeyName, value);
                Save();
            }
        }

        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        public static User UserSetting
        {
            get
            {
                return GetValueOrDefault<User>(UserKeyName, UserSettingDefault);
            }
            set
            {
                AddOrUpdateValue(UserKeyName, value);
                Save();
            }
        }

        public static void LogOut()
        {
            TokenSetting = String.Empty;
            GeoLocation = true;
            //FacebookData.Checkins = new Dictionary<long, Checkin>();
            FacebookData.choosenFriends = new List<long>();
            FacebookData.RecentPlaces = new List<Place>();
            FacebookData.choosenPlace = null;
            FacebookData.CurrentUser = null;           
            UserSetting = null;
            FacebookData.Invitees=new Dictionary<long,User>();
            FacebookData.LoadedFriends = false;
            FacebookData.friends = new Dictionary<long, User>();
            FacebookData.LoadedFriends = false;
            FacebookData.LoadedSuggestions = false;
            //FacebookData.LoadedCheckins = false;
            FacebookData.Open_Events = new Dictionary<long, Event>();
            FacebookData.User_Events = new Dictionary<long, Event>();
        }

    }
}
