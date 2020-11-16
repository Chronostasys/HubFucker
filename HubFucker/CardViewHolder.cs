using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using HubCourseScheduleFucker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HubFucker
{
    public class CardViewHolder : RecyclerView.ViewHolder
    {
        public TextView Teacher { get; private set; }
        public TextView Caption { get; private set; }
        public TextView Pos { get; private set; }
        CardView card;
        Color[] colors = new[] { Color.LightYellow, Color.LightPink, Color.LightCyan, Color.LightGray, Color.SkyBlue, Color.LightSalmon };
        Random ran = new Random();
        public static Dictionary<string, Color> dic = new Dictionary<string, Color>();

        public void SetBackGround()
        {
            var found = dic.TryGetValue(Caption.Text, out var c);
            if (found)
            {
                card.SetCardBackgroundColor(c);
                return;
            }
            var choice = ran.Next(0, 6);
            while (dic.Values.Contains(colors[choice]))
            {
                choice = ran.Next(0, 6);
            }
            card.SetCardBackgroundColor(colors[choice]);
            dic.Add(Caption.Text, colors[choice]);
        }

        public CardViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            // Locate and cache view references:
            Teacher = itemView.FindViewById<TextView>(Resource.Id.textViewTeacher);
            Caption = itemView.FindViewById<TextView>(Resource.Id.textView);
            Pos = itemView.FindViewById<TextView>(Resource.Id.textViewPos);
            card = itemView.FindViewById<CardView>(Resource.Id.card);
            itemView.Click += (o, e) => listener(base.LayoutPosition);

        }
    }
    public class LectureListAdapter : RecyclerView.Adapter
    {
        public DailyLectures lectures;
        public event EventHandler<int> ItemClick;

        public LectureListAdapter(DailyLectures _lectures)
        {
            lectures = _lectures;
        }
        public override int ItemCount => lectures.Lectures.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CardViewHolder vh = holder as CardViewHolder;

            vh.Teacher.Text = lectures.Lectures[position].kc[0].XM;

            vh.Caption.Text = lectures.Lectures[position].kc[0].KCMC;

            vh.Pos.Text = lectures.Lectures[position].kc[0].JSMC;
            vh.SetBackGround();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.card, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            CardViewHolder vh = new CardViewHolder(itemView, (e)=>ItemClick?.Invoke(this, e));
            return vh;
        }
    }
}