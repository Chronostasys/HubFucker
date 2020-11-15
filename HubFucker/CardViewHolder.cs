using Android.App;
using Android.Content;
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

        public CardViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references:
            Teacher = itemView.FindViewById<TextView>(Resource.Id.textViewTeacher);
            Caption = itemView.FindViewById<TextView>(Resource.Id.textView);
            Pos = itemView.FindViewById<TextView>(Resource.Id.textViewPos);

        }
    }
    public class LectureListAdapter : RecyclerView.Adapter
    {
        public DailyLectures lectures;

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
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.card, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            CardViewHolder vh = new CardViewHolder(itemView);
            return vh;
        }
    }
}