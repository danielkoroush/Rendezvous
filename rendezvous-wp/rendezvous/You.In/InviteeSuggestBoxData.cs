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
using Paco.Application;
using System.Collections;

namespace You.In
{
    public class InviteeSuggesetBoxData : IEnumerable<User>
    {
        public static List<User> _users = new List<User>();

        public static ICollection<User> Users
        {
            get
            {
                if (_users == null || _users.Count == 0)
                {
                    GetNames();
                }
                return (ICollection<User>)_users;
            }
        }

        /// <summary>
        /// Enumerates the Words property.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<User> GetEnumerator()
        {
            return InviteeSuggesetBoxData.Users.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the Words property.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return InviteeSuggesetBoxData.Users.GetEnumerator();
        }

        private static void GetNames()
        {
            if (FacebookData.friends != null)
            {
                foreach (User u in FacebookData.friends.Values)
                {
                    _users.Add(u);
                }
            }
            _users.Sort();
        }

        public static List<User> GetSortedFriendList()
        {
            if (_users == null || _users.Count==0)
            {
                GetNames();
            }
            return _users;
        }

    }
}
