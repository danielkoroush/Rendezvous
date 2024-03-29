﻿ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using SamplePhoneClient.ServiceReference1;

namespace SamplePhoneClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var client = new ServiceClient();
            client.AddUserActivityAsync(Guid.NewGuid().ToString(), Guid.Empty, "SomeActionOnPhone", "Some Comment from Phone");
        }
    }
}