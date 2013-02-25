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
    [DataContract(Name = "User", Namespace = "Paco.Application")]
    public class User : IComparable<User>
    {

        private string name;
        private long uid;
        private string pic_square;
        public User(string name, int uid)
        {
            this.name = name;
            this.uid = uid;
        }

        public User(JToken json)
        {
            this.name = (String)json["name"];
            this.uid = (long)json["uid"];
            this.pic_square = (String)json["pic_big"];
        }

        public User(JObject json)
        {
            JToken jtoken = (JToken)json;
            this.name = (String)json["name"];
            this.uid = long.Parse((String)json["id"]);
        }

        [DataMember(Name = "Name", EmitDefaultValue = true, IsRequired = true, Order = 2)]
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
        [DataMember(Name = "Uid", EmitDefaultValue = true, IsRequired = true, Order = 1)]
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

        [DataMember(Name = "Pic", EmitDefaultValue = true, IsRequired = true, Order = 3)]
        public String Pic
        {
            get
            {
                return pic_square;
            }
            set
            {
                pic_square = value;
            }
        }

        public int CompareTo(User obj)
        {
            return name.CompareTo(obj.name);                
        }

        public override string ToString()
        {
            return name;
        }

        public static string GetFirstNameKey(User person)
        {
            char key = char.ToLower(person.Name[0]);

            if (key < 'a' || key > 'z')
            {
                key = '#';
            }
            return key.ToString();
        }


    }  
}
