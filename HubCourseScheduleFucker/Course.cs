namespace HubCourseScheduleFucker
{
    /// <summary>
    /// 不包含所有属性，只包含了要用的&我看的懂的
    /// 写hub系统的人太司马了，中文首字母全大写命名法，我尽力了
    /// </summary>
    public class Course
    {
        ///// <summary>
        ///// 学时
        ///// </summary>
        //public int? XS { get; set; }
        /// <summary>
        /// 星期
        /// </summary>
        public string XQ { get; set; }
        /// <summary>
        /// 上课的老师，司马玩意为什么这么起名
        /// </summary>
        public string XM { get; set; }
        /// <summary>
        /// 课程名（我已经死了
        /// </summary>
        public string KCMC { get; set; }
        /// <summary>
        /// 上课地点（根本看不懂这个名字，我按照数据猜的
        /// </summary>
        public string JSMC { get; set; }
    }
}
