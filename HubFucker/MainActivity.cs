using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using HubCourseScheduleFucker;
using Felipecsl.GifImageViewLibrary;
using System.IO;
using Android.Widget;
using System.Collections.Generic;
using Environment = System.Environment;
using Jint.Native.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Android.Support.V7.Widget;
using Android.Bluetooth.LE;
using Android.Gestures;
using Xamarin.Essentials;
using Android.Support.V4.App;
using HubFucker.Resources.layout;
using Android.Content;
using Jint.Parser.Ast;

namespace HubFucker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        TextView progress;
        ProgressBar progressBar1;
        GifImageView myGIFImage;
        HubCourseScheduleFucker.HubFucker hubfucker;
        TextView tx;
        public static List<DailyLectures> lectures = new List<DailyLectures>();
        static string dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "hustcourses2020.1.json");
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        LectureListAdapter mAdapter;
        public static int day = DateTime.Now.DayOfYear - new DateTime(2020, 8, 31).DayOfYear;
        static event EventHandler<int> itemChanged;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            Android.Support.V7.App.ActionBarDrawerToggle toggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            tx = FindViewById<TextView>(Resource.Id.textView4);
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            progress = FindViewById<TextView>(Resource.Id.textView5);
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            LoadAsync();
        }
        async ValueTask LoadListAsync()
        {
            var stream = File.OpenRead(dataPath);
            lectures = await System.Text.Json.JsonSerializer
                .DeserializeAsync<List<DailyLectures>>(stream);
            await stream.DisposeAsync();
            // Instantiate the adapter and pass in its data source:
            mAdapter = new LectureListAdapter(lectures[day]);
            // Get our RecyclerView layout:
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            // Plug in the linear layout manager:
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            // Plug the adapter into the RecyclerView:
            mRecyclerView.SetAdapter(mAdapter);
            mRecyclerView.Visibility = ViewStates.Visible;
            tx.Text = $"第{lectures[day].Week}周，{lectures[day].DayOfWeek}";
            
            FindViewById<LinearLayout>(Resource.Id.linearLayout2).Visibility = ViewStates.Visible;
            FindViewById<Button>(Resource.Id.button1).Click += (o, e) => Prev();
            FindViewById<Button>(Resource.Id.button2).Click += (o, e) => Next();
            itemChanged += (o, e) => mAdapter.NotifyItemChanged(e);
            mAdapter.ItemClick += MAdapter_ItemClick;
            return;
        }
        public static void Update(int pos)
        {
            itemChanged.Invoke(null, pos);
            var s = System.Text.Json.JsonSerializer.Serialize(lectures);
            File.WriteAllTextAsync(dataPath, s);
        }

        private void MAdapter_ItemClick(object sender, int e)
        {
            var edit = new Intent(this, typeof(EditActivity));
            edit.PutExtra("course", e);
            StartActivity(edit);
        }

        async ValueTask LoadAsync()
        {
            if (File.Exists(dataPath))
            {
                await LoadListAsync();
                return;
            }
            FindViewById<LinearLayout>(Resource.Id.linearLayout1).Visibility = ViewStates.Visible;
            var getBttton = FindViewById<Button>(Resource.Id.getButton);
            getBttton.Click += GetButtonClick;
            var progressBar = FindViewById<ProgressBar>(Resource.Id.loadProgress);
            progressBar.Visibility = ViewStates.Visible;
            var code = FindViewById<TextInputEditText>(Resource.Id.code);
            hubfucker = new HubCourseScheduleFucker.HubFucker();
            var s = await hubfucker.GetValidationCodeGifAsync();

            myGIFImage = FindViewById<GifImageView>(Resource.Id.myGIFImage);
            var buffer = new byte[s.Length];
            await s.ReadAsync(buffer);

            progressBar.Visibility = ViewStates.Gone;
            code.Visibility = ViewStates.Visible;
            myGIFImage.SetBytes(buffer);
            myGIFImage.StartAnimation();
            getBttton.Visibility = ViewStates.Visible;
        }
        private async void GetButtonClick(object sender, EventArgs e)
        {
            FindViewById<LinearLayout>(Resource.Id.linearLayout1).Visibility = ViewStates.Gone;
            await Task.Run(async () =>
            {
                RunOnUiThread(() =>
                {
                    progress.Visibility = ViewStates.Visible;
                    progressBar1.Visibility = ViewStates.Visible;
                });
                try
                {
                    RunOnUiThread(() =>
                    {
                        progress.Text = "Logging in...This may take 20s";
                    });
                    await hubfucker.LoginAsync(FindViewById<TextInputEditText>(Resource.Id.stuId).Text,
                        FindViewById<TextInputEditText>(Resource.Id.passwd).Text,
                        FindViewById<TextInputEditText>(Resource.Id.code).Text);
                    RunOnUiThread(() =>
                    {
                        progress.Text = $"Fetching course data... {0}%";
                    });
                    for (int i = 0; i < 20; i++)
                    {
                        for (int j = 1; j < 8; j++)
                        {
                            var lects = await hubfucker.GetDailyLectureAsync(i + 1, (DayOfWeek)(j % 7));
                            RunOnUiThread(() =>
                            {
                                progress.Text = $"Fetching course data... {(i * 7 + j) / 1.4}%";
                            });
                            lectures.Add(new DailyLectures
                            {
                                DayOfWeek = (DayOfWeek)(j % 7),
                                Week = i + 1,
                                Lectures = lects
                            });
                        }
                    }
                    RunOnUiThread(async () =>
                    {
                        progress.Visibility = ViewStates.Gone;
                        progressBar1.Visibility = ViewStates.Gone;
                        var s = System.Text.Json.JsonSerializer.Serialize(lectures);
                        await File.WriteAllTextAsync(dataPath, s);
                        FindViewById<LinearLayout>(Resource.Id.linearLayout1).Visibility = ViewStates.Gone;
                        await LoadListAsync();
                        Toast.MakeText(this, "done", ToastLength.Long).Show();
                    });
                }
                catch (Exception)
                {
                    RunOnUiThread(async () =>
                    {
                        Toast.MakeText(this, $"Login Failed!", ToastLength.Long).Show();
                        progress.Visibility = ViewStates.Gone;
                        progressBar1.Visibility = ViewStates.Gone;
                        myGIFImage.Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.linearLayout1).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.linearLayout1).Visibility = ViewStates.Visible;
                        var getBttton = FindViewById<Button>(Resource.Id.getButton);
                        var progressBar = FindViewById<ProgressBar>(Resource.Id.loadProgress);
                        getBttton.Visibility = ViewStates.Gone;
                        progressBar.Visibility = ViewStates.Visible;
                        var code = FindViewById<TextInputEditText>(Resource.Id.code);
                        code.Visibility = ViewStates.Gone;
                        var s = await hubfucker.GetValidationCodeGifAsync();

                        myGIFImage = FindViewById<GifImageView>(Resource.Id.myGIFImage);
                        myGIFImage.StopAnimation();
                        var buffer = new byte[s.Length];
                        await s.ReadAsync(buffer);

                        progressBar.Visibility = ViewStates.Gone;
                        code.Visibility = ViewStates.Visible;
                        myGIFImage.SetBytes(buffer);
                        myGIFImage.StartAnimation();
                        myGIFImage.Visibility = ViewStates.Visible;
                        getBttton.Visibility = ViewStates.Visible;
                    });
                }
            });
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        //private void FabOnClick(object sender, EventArgs eventArgs)
        //{
        //    View view = (View) sender;
        //    Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
        //        .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        //}

        public void Prev()
        {
            try
            {
                CardViewHolder.dic.Clear();
                day--;
                mAdapter.lectures = lectures[day];
                mAdapter.NotifyDataSetChanged();
                tx.Text = $"第{lectures[day].Week}周，{lectures[day].DayOfWeek}";
            }
            catch (Exception)
            {
                day++;
                Toast.MakeText(this, "超出范围", ToastLength.Long).Show();
            }
        }
        public void Next()
        {
            try
            {
                CardViewHolder.dic.Clear();
                day++;
                mAdapter.lectures = lectures[day];
                mAdapter.NotifyDataSetChanged();
                tx.Text = $"第{lectures[day].Week}周，{lectures[day].DayOfWeek}";
            }
            catch (Exception)
            {
                day--;
                Toast.MakeText(this, "超出范围", ToastLength.Long).Show();
            }
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_camera)
            {
                try
                {
                    CardViewHolder.dic.Clear();
                    day = day - 7;
                    mAdapter.lectures = lectures[day];
                    mAdapter.NotifyDataSetChanged();
                    tx.Text = $"第{lectures[day].Week}周，{lectures[day].DayOfWeek}";
                }
                catch (Exception)
                {
                    day = day + 7;
                    Toast.MakeText(this, "超出范围", ToastLength.Long).Show();
                }
                // Handle the camera action
            }
            else if (id == Resource.Id.nav_gallery)
            {
                {
                    try
                    {
                        CardViewHolder.dic.Clear();
                        day = day + 7;
                        mAdapter.lectures = lectures[day];
                        mAdapter.NotifyDataSetChanged();
                        tx.Text = $"第{lectures[day].Week}周，{lectures[day].DayOfWeek}";
                    }
                    catch (Exception)
                    {
                        day = day - 7;
                        Toast.MakeText(this, "超出范围", ToastLength.Long).Show();
                    }
                }
            }
            else if (id == Resource.Id.nav_slideshow)
            {
                Browser.OpenAsync("https://github.com/Chronostasys/HubFucker");
            }
            else if (id == Resource.Id.nav_manage)
            {
                var pay = new Intent(this, typeof(PayActivity));
                StartActivity(pay);
                
            }
            //else if (id == Resource.Id.nav_share)
            //{

            //}
            //else if (id == Resource.Id.nav_send)
            //{

            //}

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer?.CloseDrawer(GravityCompat.Start);
            return false;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}

