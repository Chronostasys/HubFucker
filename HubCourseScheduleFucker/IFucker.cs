using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HubCourseScheduleFucker
{
    public interface IFucker
    {
        ValueTask<List<Lecture>> GetDailyLectureAsync(int week, DayOfWeek dayOfWeek);
        ValueTask<Stream> GetValidationCodeGifAsync();
        ValueTask LoginAsync(string stuId, string passwd, string code);
    }
}