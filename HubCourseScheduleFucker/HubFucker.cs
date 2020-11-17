using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using AngleSharp.Io.Cookie;
using AngleSharp.Io.Network;
using AngleSharp.Js;
using Jint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace HubCourseScheduleFucker
{
    public class HubFucker : IFucker
    {
        IBrowsingContext context;
        IDocument document;
        HttpClient client;
        bool first = true;
        HttpClientHandler handler;
        MyRequester requester;
        IDocument newdoc;
        Engine eng;

        public HubFucker()
        {
            var a = new DefaultHttpRequester();
            handler = new HttpClientHandler() { MaxAutomaticRedirections = 100}; //{ UseCookies = false };
            client = new HttpClient(handler);
            requester = new MyRequester();
            var config = Configuration.Default;
                //.WithRequester(requester)
                //.WithDefaultLoader(new LoaderOptions
                //{
                //    IsResourceLoadingEnabled = true,
                //})
                //.WithCookies().WithJs().WithEventLoop().WithMetaRefresh();

            //Create a new context for evaluating webpages with the given config
            context = BrowsingContext.New(config);
        }
        public async ValueTask<Stream> GetValidationCodeGifAsync()
        {
            var re = await client.GetAsync("https://pass.hust.edu.cn/cas/login?service=http%3A%2F%2Fhub.m.hust.edu.cn%2Fkcb%2Findex.jsp%3Fv%3D1");
            var html = await re.Content.ReadAsStringAsync();
            var desjs = await client.GetAsync("https://pass.hust.edu.cn/cas/comm/js/des.js");
            eng = new Engine().Execute(await desjs.Content.ReadAsStringAsync());

            if (first)
            {
                try
                {
                    document = await context.OpenAsync(ve => ve.Content(html));
                    
                    //document = await context.OpenAsync("http://hub.m.hust.edu.cn/kcb/index.jsp?v=1");
                    await document.WaitForReadyAsync();
                }
                catch (Exception e)
                {

                    throw e;
                }
                first = false;
            }
            //var c = context.GetCookie(new Url("https://pass.hust.edu.cn/cas/code"));
            var message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://pass.hust.edu.cn/cas/code");
            //message.Headers.Add("Cookie", c);
            var result = await client.SendAsync(message);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStreamAsync();
        }
        public async ValueTask LoginAsync(string stuId, string passwd, string code)
        {
            var lt = document.GetElementById("lt") as IHtmlInputElement;
            var enc = eng.GetValue("strEnc");
            var des = enc.Invoke($"{stuId + passwd + lt.Value}", "1", "2", "3").AsString();
            //var form = document.GetElementById("loginForm") as IHtmlFormElement;
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
            var passurl = new Uri("https://pass.hust.edu.cn/");
            var header = handler.CookieContainer.GetCookieHeader(passurl);
            var nc = $"cas_hash=; hust_cas_un={stuId}; {header}";
            handler.CookieContainer.SetCookies(passurl, nc);
            //var jsession = header.Split(';')[1].Split('=')[1];
            var u = "http://hub.m.hust.edu.cn/kcb/index.jsp?v=1";
            var url = new Url($"https://pass.hust.edu.cn/cas/login?service={HttpUtility.UrlEncode(u)}");
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(code), "code");
            form.Add(new StringContent(des), "rsa");
            form.Add(new StringContent($"{stuId.Length}"), "ul");
            form.Add(new StringContent($"{passwd.Length}"), "pl");
            form.Add(new StringContent($"{lt.Value}"), "lt");
            form.Add(new StringContent("e1s2"), "execution");
            form.Add(new StringContent("submit"), "_eventId");
            form.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            

            var msg = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
            msg.Content = form;
            msg.Headers.Add("Host", "pass.hust.edu.cn");
            msg.Headers.Add("Origin", "https://pass.hust.edu.cn");
            msg.Headers.Add("Sec-Fetch-Dest", "document");
            msg.Headers.Add("Sec-Fetch-Mode", "navigation");
            msg.Headers.Add("Sec-Fetch-Site", "same-origin");
            msg.Headers.Add("Sec-Fetch-User", "?1");
            msg.Headers.Add("Upgrade-Insecure-Requests", "1");
            msg.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Mobile Safari/537.36 Edg/86.0.622.56");
            msg.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            msg.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            msg.Headers.Add("Cookie", nc);

            msg.Headers.Referrer = new Uri("https://pass.hust.edu.cn/cas/login?service=http%3A%2F%2Fhub.m.hust.edu.cn%2Fkcb%2Findex.jsp%3Fv%3D1");


            client = new HttpClient(new HttpClientHandler { UseCookies = false });
            var submitResult = await client.SendAsync(msg);
            var rehtml = await submitResult.Content.ReadAsStringAsync();
            var he = handler.CookieContainer.GetCookieHeader(new Url("http://hub.m.hust.edu.cn"));

            //newdoc = await form.SubmitAsync(o);
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
