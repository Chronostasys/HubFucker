using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using AngleSharp.Io.Cookie;
using AngleSharp.Io.Network;
using AngleSharp.Js;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HubCourseScheduleFucker
{
    public class HubFucker
    {
        IBrowsingContext context;
        IDocument document;
        HttpClient client;
        bool first = true;
        HttpClientHandler handler;
        MyRequester requester;
        IDocument newdoc;
        public static HttpStatusCode httpStatus;
        public HubFucker()
        {
            var a = new DefaultHttpRequester();
            handler = new HttpClientHandler { UseCookies = false};
            client = new HttpClient(handler);
            requester = new MyRequester();
            var config = Configuration.Default.WithRequester(requester)
                .WithDefaultLoader(new LoaderOptions 
                {
                    IsResourceLoadingEnabled = true ,
                })
                .WithCookies().WithJs().WithEventLoop().WithMetaRefresh();

            //Create a new context for evaluating webpages with the given config
            context = BrowsingContext.New(config);
        }
        public async ValueTask<Stream> GetValidationCodeGifAsync()
        {
            if (first)
            {
                try
                {
                    document = await context.OpenAsync("http://hub.m.hust.edu.cn/kcb/index.jsp?v=1");
                    await document.WaitForReadyAsync();
                }
                catch (Exception e)
                {

                    throw e;
                }
                first = false;
            }
            var c = context.GetCookie(new Url("https://pass.hust.edu.cn/cas/code"));
            var message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://pass.hust.edu.cn/cas/code");
            message.Headers.Add("Cookie", c);
            var result = await client.SendAsync(message);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStreamAsync();
        }
        public async ValueTask LoginAsync(string stuId, string passwd, string code)
        {
            await document.WaitForReadyAsync();
            var u = document.GetElementById("un") as IHtmlInputElement;
            var p = document.GetElementById("pd") as IHtmlInputElement;
            var codeElement = document.GetElementById("code") as IHtmlInputElement;
            u.Value = stuId;
            p.Value = passwd;
            codeElement.Value = code;
            var lt = document.GetElementById("lt") as IHtmlInputElement;
            var des = document.ExecuteScript($"strEnc('{stuId+passwd+lt.Value}' , '1' , '2' , '3')");
            var desE = document.GetElementById("rsa") as IHtmlInputElement;
            desE.Value = des as string;
            var form = document.GetElementById("loginForm") as IHtmlFormElement;
            var o = new
            {
                code = code,
                rsa = des,
                ul = stuId.Length,
                pl = passwd.Length,
                lt = lt.Value,
                execution = "e1s1",
                _eventId = "submit"
            };
            newdoc = await form.SubmitAsync(o);
            await newdoc.WaitForReadyAsync();
            
        }
        public async ValueTask<List<Lecture>> GetDailyLectureAsync(int week, DayOfWeek dayOfWeek)
        {
            DateTime time = DateTime.Now;
            var less = dayOfWeek + 7 - time.DayOfWeek;
            time = time.AddDays(less);
            var timeSlug = time.ToString("yyyy-MM-dd");
            var req = $"http://hub.m.hust.edu.cn/kcb/todate/JsonCourse.action?sj={timeSlug}&zc={week}";
            
            var message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, req);
            message.Headers.Add("Cookie", newdoc.Cookie);
            var result = await client.SendAsync(message);
            return await JsonSerializer.DeserializeAsync<List<Lecture>>(await result.Content.ReadAsStreamAsync());
        }
    }
}
