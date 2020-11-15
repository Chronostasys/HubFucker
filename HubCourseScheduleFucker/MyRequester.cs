using AngleSharp.Io;
using System.Threading;
using System.Threading.Tasks;

namespace HubCourseScheduleFucker
{
    public class MyRequester:MyDefaultHttpRequester
    {
        protected override async Task<IResponse> PerformRequestAsync(Request request, CancellationToken cancel)
        {
            
            if (request.Address.Href.EndsWith("png")|| request.Address.Href.EndsWith("css"))
            {
                return new DefaultResponse();
            }
            else if (request.Address.Href.EndsWith("code"))
            {
                return new DefaultResponse();
            }
            var re = await base.PerformRequestAsync(request, cancel);
            return re;
        }
    }
}
