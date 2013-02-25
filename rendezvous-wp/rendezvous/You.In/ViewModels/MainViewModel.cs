using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Paco.Application;

namespace You.In
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }

        public static Event newUserEvent;

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }
        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get
            {
                return isDataLoaded;
            }
            set
            {
                isDataLoaded = value;
            }
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData(Dictionary<long,Event> events)
        {
            this.Items.Clear();
            List<Event> temp_list = new List<Event>();
            foreach (long eid in events.Keys)
            {
                if (events[eid] != null)
                {
                    temp_list.Add(events[eid]);
                    //  Event temp = events[eid]; 5
                }
            }
            temp_list.Sort();
            foreach (Event temp in temp_list)
            {
                ItemViewModel tmp = new ItemViewModel() { EventTitle = temp.Name, EventDate = Utility.ConvertEpochToUtc(temp.StartTime).ToString("ddd, MMM dd"), EventTime = Utility.ConvertEpochToUtc(temp.StartTime).ToString("t"), Eid = temp.ID, PicSource = temp.Picture };
                tmp.EventAddress = (temp.Host!=null) ?(temp.Host.Length > 30) ? temp.Host.Substring(0, 26) + "..." : temp.Host : string.Empty;
                if (temp.RSVP == RSVP.attending)
                {
                    tmp.EventConfirmed = new Uri("/Images/accept.png", UriKind.Relative).ToString();

                }
                else if (temp.RSVP == RSVP.declined)
                {
                    tmp.EventConfirmed = new Uri("/Images/declined.png", UriKind.Relative).ToString();
                    
                }
                else if (temp.RSVP == RSVP.maybe)
                {
                    tmp.EventConfirmed = new Uri("/Images/maybe.png", UriKind.Relative).ToString();
                    
                }
                else if (temp.RSVP == RSVP.not_replied)
                {
                    tmp.EventConfirmed = new Uri("/Images/noreply.png", UriKind.Relative).ToString();
                }

                this.Items.Add(tmp);
            }

            if (Items.Count < 1)
            {
                ItemViewModel tmp = new ItemViewModel() { EventTitle = "No Upcoming Events", EventAddress = "Please use below buttons to create one", PicSource = new Uri("/Images/event_thumb.png", UriKind.Relative).ToString() };
                Items.Add(tmp);
            }

            this.isDataLoaded = true;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}