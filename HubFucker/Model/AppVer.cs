using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HubFucker.Model
{
    public class AppVer
    {
        public string Version { get; set; }
        public string ApkUrl { get; set; }
        public List<string> Updates { get; set; }
    }
}