using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HubFucker.Resources.layout
{
    [Activity(Label = "EditActivity")]
    public class EditActivity : AppCompatActivity
    {
        int pos;
        EditText cn;
        EditText lo;
        EditText te;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_edit);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            ////Toolbar will now take on default actionbar characteristics
            //SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            pos = Intent.GetIntExtra("course", -1);
            var course = MainActivity.lectures[MainActivity.day].Lectures[pos];
            cn = FindViewById<EditText>(Resource.Id.editTextCN);
            te = FindViewById<EditText>(Resource.Id.editTextT);
            lo = FindViewById<EditText>(Resource.Id.editTextL);
            FindViewById<Button>(Resource.Id.saveEdit).Click += EditActivity_Click; ;
            cn.Text = course.kc[0].KCMC;
            te.Text = course.kc[0].XM;
            lo.Text = course.kc[0].JSMC;
            // Create your application here
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                Finish();

            return base.OnOptionsItemSelected(item);
        }

        private void EditActivity_Click(object sender, EventArgs e)
        {
            var course = MainActivity.lectures[MainActivity.day].Lectures[pos];
            course.kc[0].KCMC = cn.Text;
            course.kc[0].XM = te.Text;
            course.kc[0].JSMC = lo.Text;
            MainActivity.Update(pos);
            OnBackPressed();
        }
    }
}