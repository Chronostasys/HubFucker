using System;
using System.Collections.Generic;

namespace HubCourseScheduleFucker
{
    /// <summary>
    /// 这个里边的名称都是小写，course里全是大写，
    /// 我开始觉得写hub的人根本没学过编程，可能只是上了个幼儿编程兴趣班
    /// </summary>
    public class Lecture
    {
        /// <summary>
        /// 类似序号的一个东西
        /// </summary>
        public string jcx { get; set; }
        /// <summary>
        /// 又是我看不懂的代码，为什么这里需要列表？？
        /// </summary>
        public List<Course> kc { get; set; }
    }
    public class DailyLectures
    {
        public int Week { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public List<Lecture> Lectures { get; set; }
    }
}
