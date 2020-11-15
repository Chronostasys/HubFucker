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

namespace HubFucker.Resources.layout
{
    [Activity(Label = "PayActivity")]
    public class PayActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_pay);
            var img = FindViewById<ImageView>(Resource.Id.imageView1);
            img.SetImageResource(Resource.Drawable.pay);
            // Create your application here
        }
    }
}