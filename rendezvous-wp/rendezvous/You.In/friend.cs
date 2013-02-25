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

namespace You.In
{
    public class friend
    {
        private string name;
        private string uid;
        public friend(string name, string uid)
        {
            this.name = name;
            this.uid = uid;
        }

        public string Name
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
        public string Uid
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
    }
}
