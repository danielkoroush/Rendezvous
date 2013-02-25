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
    public class RecentFriends : List<FriendsInGroup>
    {
        private static readonly string Groups = "#abcdefghijklmnopqrstuvwxyz";

        private Dictionary<int, User> _personLookup = new Dictionary<int, User>();

        public RecentFriends()
        {
            try
            {
                List<long> recent = AppSettings.GetRecentFriends();
                List<User> people = new List<User>();
                if (recent != null)
                {
                    foreach (long uid in recent)
                    {
                        if (FacebookData.friends.ContainsKey(uid))
                        {
                            people.Add(FacebookData.friends[uid]);
                        }
                    }
                }
                // FacebookData.RecentUsers;
                people.Sort();

                Dictionary<string, FriendsInGroup> groups = new Dictionary<string, FriendsInGroup>();

                foreach (char c in Groups)
                {
                    FriendsInGroup group = new FriendsInGroup(c.ToString());
                    this.Add(group);
                    groups[c.ToString()] = group;
                }

                foreach (User person in people)
                {
                    groups[User.GetFirstNameKey(person)].Add(person);
                }
            }
            catch (Exception e)
            {
                String x = e.Message;
            }
        }

    }
}
