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
using Android.Support.V7.View.Menu;
using Android.Content.PM;
using System.Net.Http;
using HubFucker.Model;

namespace HubFucker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", Icon ="@drawable/ic_launcher_logo", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        IMenuItem current;
        TextView progress;
        ProgressBar progressBar1;
        GifImageView myGIFImage;
        IFucker fucker;
        TextView tx;
        public static List<DailyLectures> lectures = new List<DailyLectures>();
        static string dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "hustcourses2021.1.json");
        string apkPath => Path.Combine(
            GetExternalFilesDir(null).Path,
            "hubfucker.apk");
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        LectureListAdapter mAdapter;
        public static int day = (DateTime.Now < new DateTime(2021, 3, 1) ? 0 : (DateTime.Now - new DateTime(2021, 3, 1)).Days);
        static event EventHandler<int> itemChanged;
        string[] days = new[] { "星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
        NavigationView navigationView;
        HttpClient client = new HttpClient();
        DateTime uncheckTime = DateTime.MinValue;
        string uncheckSavePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "uncheckTime.txt");
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
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            progress = FindViewById<TextView>(Resource.Id.textView5);
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            LoadAsync();
            CheckUpdate();
        }
        async ValueTask CheckUpdate()
        {
            try
            {
                if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage)!=Permission.Granted
                    || CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Permission.Granted
                    || CheckSelfPermission(Manifest.Permission.InstallPackages) != Permission.Granted)
                {
                    RequestPermissions(new[] { Manifest.Permission.InstallPackages, Manifest.Permission.ReadExternalStorage,
                        Manifest.Permission.WriteExternalStorage }, 1);
                }
                if (File.Exists(uncheckSavePath))
                {
                    uncheckTime = DateTime.Parse(File.ReadAllText(uncheckSavePath));
                }
                if (uncheckTime.Year == DateTime.Now.Year && (DateTime.Now.DayOfYear - uncheckTime.DayOfYear) < 7)
                {
                    return;
                }
                var re = await client.GetStreamAsync(
                    "https://cdn.limfx.pro/hubfucker/version.json");
                var remoteVers = await System.Text.Json.JsonSerializer.DeserializeAsync<List<AppVer>>(re);
                var localVers = await System.Text.Json.JsonSerializer.DeserializeAsync<List<AppVer>>(Assets.Open("version.json"));
                if (remoteVers[0].version == localVers[0].version)
                {
                    //Toast.MakeText(this, "已经是最新版", ToastLength.Long).Show();
                    return;
                }
                var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                var msg = "更新版本的HubFucker已发布，你想现在安装它吗？\n";
                for (int i = 0; i < remoteVers.Count - localVers.Count; i++)
                {
                    var item = remoteVers[i];
                    msg += $"V{item.version}\n";
                    foreach (var update in item.updates)
                    {
                        msg += $" -{update}\n";
                    }
                }
                builder
                    .SetMessage(msg)
                    .SetPositiveButton("是", (o, args) =>
                    {
                        Toast.MakeText(this, "下载已在后台开始，会在完成时自动尝试安装", ToastLength.Long).Show();
                        InstallAsync(remoteVers[0].apkUrl);
                    })
                    .SetNegativeButton("否", (o, args) =>
                    {
                    })
                    .SetNeutralButton("最近不再提示", (o, args) =>
                    {
                        uncheckTime = DateTime.Now;
                        File.WriteAllTextAsync(uncheckSavePath, uncheckTime.ToString());
                    })
                    .Create()
                    .Show();
            }
            catch (Exception)
            {

                Toast.MakeText(this, "由于网络问题，检查更新失败。推荐使用华科校园网", ToastLength.Long).Show();
            }
        }
        async ValueTask InstallAsync(string src)
        {
            try
            {
                var s = await client.GetStreamAsync(src);
                var fs = File.Create(apkPath);
                await s.CopyToAsync(fs);
                await fs.FlushAsync();
                await fs.DisposeAsync();
                await s.DisposeAsync();
                //if (!File.Exists(apkPath))
                //{
                //}
                Intent install = new Intent(Intent.ActionView);
                install.AddFlags(ActivityFlags.ClearTask);

                install.AddFlags(ActivityFlags.GrantReadUriPermission);
                var f = new Java.IO.File(apkPath);
                var uri = FileProvider.GetUriForFile(this,
                    PackageName + ".provider", f);
                install.SetDataAndType(uri, "application/vnd.android.package-archive");
                StartActivity(install);
                return;
            }
            catch (Exception)
            {
                Toast.MakeText(this, "因为网络问题，更新下载失败。推荐使用华科校园网", ToastLength.Long).Show();
            }
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
            ShowDailyCourse(true);
            
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
            fucker = new HubCourseScheduleFucker.HubFucker();
            var s = await fucker.GetValidationCodeGifAsync();

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
                    await fucker.LoginAsync(FindViewById<TextInputEditText>(Resource.Id.stuId).Text,
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
                            var lects = await fucker.GetDailyLectureAsync(i + 1, (DayOfWeek)(j % 7));
                            RunOnUiThread(() =>
                            {
                                progress.Text = $"Fetching course data... {(int)((i * 7 + j) / 1.4)}%";
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
                        var s = await fucker.GetValidationCodeGifAsync();

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
                day--;
                ShowDailyCourse();
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
                day++;
                ShowDailyCourse();
            }
            catch (Exception)
            {
                day--;
                Toast.MakeText(this, "超出范围", ToastLength.Long).Show();
            }
        }
        void ShowDailyCourse(bool init = false)
        {
            if (navigationView.CheckedItem?.ItemId== Resource.Id.nav_share)
            {
                current = navigationView.CheckedItem;
            }
            if (day!= DateTime.Now.DayOfYear - new DateTime(2021, 3, 1).DayOfYear)
            {
                //current?.SetChecked(false);
                navigationView.CheckedItem?.SetChecked(false);
            }
            else
            {
                current?.SetChecked(true);
                
                navigationView.SetCheckedItem(Resource.Id.nav_share);
            }

            if (!init)
            {
                CardViewHolder.dic.Clear();
                mAdapter.lectures = lectures[day];
                mAdapter.NotifyDataSetChanged();
            }
            tx.Text = $"第{lectures[day].Week}周，{days[(int)lectures[day].DayOfWeek]}";
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            if (id == Resource.Id.nav_camera)
            {
                try
                {
                    day = day - 7;
                    ShowDailyCourse();
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
                        day = day + 7;
                        ShowDailyCourse();
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
            else if (id == Resource.Id.nav_share)
            {
                current = item;
                var prevDay = day;
                day = DateTime.Now.DayOfYear - new DateTime(2020, 3, 1).DayOfYear;
                try
                {
                    ShowDailyCourse();
                }
                catch (Exception)
                {
                    day = prevDay;
                    Toast.MakeText(this, "超出范围", ToastLength.Long).Show();
                }
                drawer?.CloseDrawer(GravityCompat.Start);
                return true;
            }
            //else if (id == Resource.Id.nav_send)
            //{

            //}

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

