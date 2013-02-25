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

namespace Paco.Application
{
    [DataContract(Name = "Place", Namespace = "Paco.Application")]
    public class Place
    {
        private string name;
        private string id;
        private string category;
        private JToken location;

        [DataMember(Name = "Address", EmitDefaultValue = true, IsRequired = true, Order = 7)]
        public String address = String.Empty;

        private float _lattitude;
        [DataMember(Name = "Lattitude", EmitDefaultValue = true, IsRequired = true, Order = 3)]
        public float Lattitude
        {
            get
            {
                return _lattitude;
            }
            set 
            {
                _lattitude = value;
            }
        }

        private float _longitude;
        [DataMember(Name = "Longitude", EmitDefaultValue = true, IsRequired = true, Order = 2)]
        public float Longitude
        {
            get
            {
                return _longitude;
            }
            set 
            {
                _longitude = value;
            }
        }

        public Place(String name, float latitude, float longitude, String address)
        {
            this.name = name;
            if (latitude!=0) _lattitude = latitude;
            if (longitude != 0) _longitude = longitude;
            this.address = address;
        }

        public Place(JToken json)
        {
            name = (String) json["name"];
            id = (String)json["id"];
            category = (String)json["category"];
            location = json["location"];
        }

        public Place(JObject json)
        {
            name= (String)json["name"];
            id = ((float)json["page_id"]).ToString();
            _lattitude=((float)json["latitude"]);
            _longitude=((float)json["longitude"]);
        }

        [DataMember(Name = "Name", EmitDefaultValue = true, IsRequired = true, Order = 1)]
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

        [DataMember(Name = "ID", EmitDefaultValue = true, IsRequired = true, Order = 4)]
        public string ID
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

        [DataMember(Name = "Category", EmitDefaultValue = true, IsRequired = true, Order = 5)]
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
            }
        }

        //[DataMember(Name = "Location", EmitDefaultValue = true, IsRequired = true, Order = 6)]
        //public String Location
        //{
        //    get
        //    {               
        //        return location.ToString();
        //    }
        //    set { }
        //}
        public string ToString()
        {
            return name;
        }

        private double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

        public double GetDistance(double lat2, double lon2)
        {
            Longitude=float.Parse(GetLongitude());
            Lattitude=float.Parse(GetLatitude());
            double R = 3960;
            double dLat = this.toRadian(lat2 - Lattitude);
            double dLon = this.toRadian(lon2 - Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(this.toRadian(Lattitude)) * Math.Cos(this.toRadian(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;
            return d;  
        }

        public String GetDistanceString(double lat2, double lon2)
        {
            try
            {
                Longitude = float.Parse(GetLongitude());
                Lattitude = float.Parse(GetLatitude());
                double R = 3960;
                double dLat = this.toRadian(lat2 - Lattitude);
                double dLon = this.toRadian(lon2 - Longitude);
                double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(this.toRadian(Lattitude)) * Math.Cos(this.toRadian(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
                double d = R * c;
                return d.ToString("0.000");
            }
            catch (NullReferenceException)
            {
                return "N/A";
            }
        }

        public String GetLatitude()
        {
            float latitude = (float)location["latitude"];
            return latitude.ToString();
        }

        public String GetLongitude()
        {
            float longitude = (float)location["longitude"];
            return longitude.ToString();
        }

        public String GetCity()
        {
            try
            {
                return (String)(location["city"]);
            }
            catch (NullReferenceException)
            {
                return String.Empty;
            }
        }

        public String GetState()
        {
            try
            {
                return (String)(location["state"]);
            }
            catch (NullReferenceException)
            {
                return String.Empty;
            }
        }

        public String GetStreet()
        {
            try
            {
                return (String)(location["street"]);
            }
            catch (NullReferenceException)
            {
                return String.Empty;
            }
        }
        public String GetMapUrl()
        {
            float latitude = (float)location["latitude"];
            float longitude = (float)location["longitude"];
            return String.Format("http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{0},{1}/15?mapSize=550,420&pp={0},{1};34&mapVersion=v1&key=AhGs2lwlgYWijQdXshst6ZGVLD1Gm3oVUpiO8CEajIV2sAYerOB1UgP7TV1D4mNl", latitude, longitude);                 
        }

        public String GetMapImage()
        {
            return String.Format("http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{0},{1}/16?mapSize=600,200&pp={0},{1};34&mapVersion=v1&key=AhGs2lwlgYWijQdXshst6ZGVLD1Gm3oVUpiO8CEajIV2sAYerOB1UgP7TV1D4mNl", _lattitude, _longitude);                
        }

        public static void GetLongLat(string location, AsyncCallback CityResponseCallback)
        {
            //if you wanna reverse lookup, use this http://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&sensor=true
            Uri geoCodeUri = new Uri(String.Format("http://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=true", location));
            HttpWebRequest request = WebRequest.Create(geoCodeUri) as HttpWebRequest;
            request.Method = "POST";
            request.BeginGetResponse(CityResponseCallback, request);
        }
        public static void GeoCodeResponseCallback(string json, out string longitude, out string latitude)
        {
            ///String json;
            longitude = "";
            latitude = "";
            try
            {                    
                longitude = JArray.Parse(JObject.Parse(json)["results"].ToString())[0]["geometry"]["location"]["lat"].ToString();
                latitude = JArray.Parse(JObject.Parse(json)["results"].ToString())[0]["geometry"]["location"]["lng"].ToString();
                    #region expectedresponse
                    /*{
  "status": "OK",
  "results": [ {
    "types": [ "street_address" ],
    "formatted_address": "18270-18290 NE 97th Way, Redmond, WA 98052, USA",
    "address_components": [ {
      "long_name": "18270-18290",
      "short_name": "18270-18290",
      "types": [ "street_number" ]
    }, {
      "long_name": "NE 97th Way",
      "short_name": "NE 97th Way",
      "types": [ "route" ]
    }, {
      "long_name": "Redmond",
      "short_name": "Redmond",
      "types": [ "locality", "political" ]
    }, {
      "long_name": "East Seattle",
      "short_name": "East Seattle",
      "types": [ "administrative_area_level_3", "political" ]
    }, {
      "long_name": "King",
      "short_name": "King",
      "types": [ "administrative_area_level_2", "political" ]
    }, {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    }, {
      "long_name": "98052",
      "short_name": "98052",
      "types": [ "postal_code" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.6868213,
        "lng": -122.0970018
      },
      "location_type": "RANGE_INTERPOLATED",
      "viewport": {
        "southwest": {
          "lat": 47.6836641,
          "lng": -122.1001555
        },
        "northeast": {
          "lat": 47.6899594,
          "lng": -122.0938602
        }
      },
      "bounds": {
        "southwest": {
          "lat": 47.6868093,
          "lng": -122.0975481
        },
        "northeast": {
          "lat": 47.6868142,
          "lng": -122.0964676
        }
      }
    }
  }, {
    "types": [ "neighborhood", "political" ],
    "formatted_address": "Education Hill, Redmond, WA, USA",
    "address_components": [ {
      "long_name": "Education Hill",
      "short_name": "Education Hill",
      "types": [ "neighborhood", "political" ]
    }, {
      "long_name": "Redmond",
      "short_name": "Redmond",
      "types": [ "locality", "political" ]
    }, {
      "long_name": "East Seattle",
      "short_name": "East Seattle",
      "types": [ "administrative_area_level_3", "political" ]
    }, {
      "long_name": "King",
      "short_name": "King",
      "types": [ "administrative_area_level_2", "political" ]
    }, {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.6882764,
        "lng": -122.1127284
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 47.6728430,
          "lng": -122.1407111
        },
        "northeast": {
          "lat": 47.7028750,
          "lng": -122.0917000
        }
      },
      "bounds": {
        "southwest": {
          "lat": 47.6728430,
          "lng": -122.1407111
        },
        "northeast": {
          "lat": 47.7028750,
          "lng": -122.0917000
        }
      }
    }
  }, {
    "types": [ "postal_code" ],
    "formatted_address": "Redmond, WA 98052, USA",
    "address_components": [ {
      "long_name": "98052",
      "short_name": "98052",
      "types": [ "postal_code" ]
    }, {
      "long_name": "Redmond",
      "short_name": "Redmond",
      "types": [ "locality", "political" ]
    }, {
      "long_name": "East Seattle",
      "short_name": "East Seattle",
      "types": [ "administrative_area_level_3", "political" ]
    }, {
      "long_name": "King",
      "short_name": "King",
      "types": [ "administrative_area_level_2", "political" ]
    }, {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.6800079,
        "lng": -122.1213836
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 47.6270580,
          "lng": -122.1644590
        },
        "northeast": {
          "lat": 47.7333080,
          "lng": -122.0783080
        }
      },
      "bounds": {
        "southwest": {
          "lat": 47.6270580,
          "lng": -122.1644590
        },
        "northeast": {
          "lat": 47.7333080,
          "lng": -122.0783080
        }
      }
    }
  }, {
    "types": [ "locality", "political" ],
    "formatted_address": "Redmond, WA, USA",
    "address_components": [ {
      "long_name": "Redmond",
      "short_name": "Redmond",
      "types": [ "locality", "political" ]
    }, {
      "long_name": "East Seattle",
      "short_name": "East Seattle",
      "types": [ "administrative_area_level_3", "political" ]
    }, {
      "long_name": "King",
      "short_name": "King",
      "types": [ "administrative_area_level_2", "political" ]
    }, {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.6739881,
        "lng": -122.1215120
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 47.6270219,
          "lng": -122.1645100
        },
        "northeast": {
          "lat": 47.7171189,
          "lng": -122.0367330
        }
      },
      "bounds": {
        "southwest": {
          "lat": 47.6270219,
          "lng": -122.1645100
        },
        "northeast": {
          "lat": 47.7171189,
          "lng": -122.0367330
        }
      }
    }
  }, {
    "types": [ "administrative_area_level_3", "political" ],
    "formatted_address": "East Seattle, WA, USA",
    "address_components": [ {
      "long_name": "East Seattle",
      "short_name": "East Seattle",
      "types": [ "administrative_area_level_3", "political" ]
    }, {
      "long_name": "King",
      "short_name": "King",
      "types": [ "administrative_area_level_2", "political" ]
    }, {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.7221712,
        "lng": -122.1270532
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 47.4228389,
          "lng": -122.2761370
        },
        "northeast": {
          "lat": 47.7770729,
          "lng": -121.9182420
        }
      },
      "bounds": {
        "southwest": {
          "lat": 47.4228389,
          "lng": -122.2761370
        },
        "northeast": {
          "lat": 47.7770729,
          "lng": -121.9182420
        }
      }
    }
  }, {
    "types": [ "administrative_area_level_2", "political" ],
    "formatted_address": "King, Washington, USA",
    "address_components": [ {
      "long_name": "King",
      "short_name": "King",
      "types": [ "administrative_area_level_2", "political" ]
    }, {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.5480339,
        "lng": -121.9836029
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 47.0844570,
          "lng": -122.5410680
        },
        "northeast": {
          "lat": 47.7803280,
          "lng": -121.0657090
        }
      },
      "bounds": {
        "southwest": {
          "lat": 47.0844570,
          "lng": -122.5410680
        },
        "northeast": {
          "lat": 47.7803280,
          "lng": -121.0657090
        }
      }
    }
  }, {
    "types": [ "administrative_area_level_1", "political" ],
    "formatted_address": "Washington, USA",
    "address_components": [ {
      "long_name": "Washington",
      "short_name": "WA",
      "types": [ "administrative_area_level_1", "political" ]
    }, {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 47.7510741,
        "lng": -120.7401386
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 45.5435410,
          "lng": -124.8489740
        },
        "northeast": {
          "lat": 49.0024940,
          "lng": -116.9155800
        }
      },
      "bounds": {
        "southwest": {
          "lat": 45.5435410,
          "lng": -124.8489740
        },
        "northeast": {
          "lat": 49.0024940,
          "lng": -116.9155800
        }
      }
    }
  }, {
    "types": [ "country", "political" ],
    "formatted_address": "United States",
    "address_components": [ {
      "long_name": "United States",
      "short_name": "US",
      "types": [ "country", "political" ]
    } ],
    "geometry": {
      "location": {
        "lat": 37.0902400,
        "lng": -95.7128910
      },
      "location_type": "APPROXIMATE",
      "viewport": {
        "southwest": {
          "lat": 18.7763000,
          "lng": 170.5957000
        },
        "northeast": {
          "lat": 71.5388000,
          "lng": -66.8850749
        }
      },
      "bounds": {
        "southwest": {
          "lat": 18.7763000,
          "lng": 170.5957000
        },
        "northeast": {
          "lat": 71.5388000,
          "lng": -66.8850749
        }
      }
    }
  } ]
}
*/
                    #endregion                
            }
            catch (Exception e) 
            {
                longitude = String.Empty;
                latitude = String.Empty;
            }
        }
        public String WebRequestMethod()
        {
            String result = String.Empty;
            result = String.Format("\"location\":\"{0}\"",name);
            string street = (String)location["street"];
            if (!String.IsNullOrEmpty(street))
            {
                result += String.Format(",\"street\":\"{0}\"", street);
            }            
            string city = (String)location["city"];            
            if (!String.IsNullOrEmpty(city))
            {
                result += String.Format(",\"city\":\"{0}\"", city);
            }
            try
            {
                float latitude = (float)location["latitude"]; 
                float longitude = (float)location["longitude"];
                if (latitude != null && longitude != null)
                {
                    string bing_map = String.Format("Bing Map:\\nhttp://www.bing.com/maps/?q={0},{1}", latitude, longitude);
                    string google_map = String.Format("Google Map:\\nhttp://maps.google.com/maps/api/staticmap?size=512x512&maptype=roadmap&markers={0},{1}&sensor=false&zoom=15", latitude, longitude);
                    string yelp = String.Format("Yelp:\\nhttp://www.yelp.com/search?find_desc={2}&ns=1&GeoPoint={0},{1}", latitude, longitude,name.Replace(" ","+"));
                    result += String.Format(",\"description\":\"{0}\\n\\n{1}\\n{2}\"", bing_map,google_map,yelp);
                }
            }
            catch (ArgumentNullException)
            {
                //latitude and longitude for the place doesn't exist
            }
            return result;
        }

        public String GetAddress()
        {
            String address = String.Empty;
            try
            {
                address = String.Format("{0}{3}{1} {2}", location["street"], location["city"], location["state"],Environment.NewLine);
                address = address.Replace("\"", string.Empty);
            }
            catch (NullReferenceException)
            {
            }
            return address;
        }

    }
}
