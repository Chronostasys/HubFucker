﻿using Jint;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HubCourseScheduleFucker
{
    public class HubFucker : IFucker
    {
        HttpClient client;
        bool first = true;
        HttpClientHandler handler;
        Engine eng;
        Regex ltReg = new Regex(@"id=""lt""[\sa-zA-Z0-9=""\-]*");

        public HubFucker()
        {
            handler = new HttpClientHandler() { MaxAutomaticRedirections = 100};
            client = new HttpClient(handler);
        }
        public async ValueTask<Stream> GetValidationCodeGifAsync()
        {
            //抓取加密脚本，放入js引擎
            var desjs = await client.GetAsync("https://pass.hust.edu.cn/cas/comm/js/des.js");
            eng = new Engine().Execute(await desjs.Content.ReadAsStringAsync());

            if (first)
            {
                try
                {
                }
                catch (Exception e)
                {

                    throw e;
                }
                first = false;
            }
            //获取二维码
            var message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://pass.hust.edu.cn/cas/code");
            var result = await client.SendAsync(message);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStreamAsync();
        }
        public async ValueTask LoginAsync(string stuId, string passwd, string code)
        {
            var re = await client.GetAsync("https://pass.hust.edu.cn/cas/login?service=http%3A%2F%2Fhub.m.hust.edu.cn%2Fkcb%2Findex.jsp%3Fv%3D1");
            var html = await re.Content.ReadAsStringAsync();
            var ltval = ltReg.Match(html).Value.Split("value=")[1];
            var lt = ltval.Split('"')[1];

            var enc = eng.GetValue("strEnc");
            var des = enc.Invoke($"{stuId + passwd + lt}", "1", "2", "3").AsString();
            var passurl = new Uri("https://pass.hust.edu.cn/");
            var header = handler.CookieContainer.GetCookieHeader(passurl);
            var nc = $"cas_hash=; hust_cas_un={stuId}; {header}";
            handler.CookieContainer.SetCookies(passurl, nc);
            var u = "http://hub.m.hust.edu.cn/kcb/index.jsp?v=1";
            var url = new Uri($"https://pass.hust.edu.cn/cas/login?service={HttpUtility.UrlEncode(u)}");

            //application/x-www-form-urlencoded 需要用stringcontent设置
            var content = new StringContent($"code={code}&rsa={des}&ul={stuId.Length}&pl={passwd.Length}&lt={lt}&execution=e1s1&_eventId=submit");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");


            var msg = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
            msg.Content = content;
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

            msg.Headers.Referrer = new Uri("https://pass.hust.edu.cn/cas/login?service=http%3A%2F%2Fhub.m.hust.edu.cn%2Fkcb%2Findex.jsp%3Fv%3D1");


            var submitResult = await client.SendAsync(msg);
            //由于scheme从https变为http，需要手动重定向
            var fin = await client.GetAsync(submitResult.Headers.Location);
            fin.EnsureSuccessStatusCode();

        }
        public async ValueTask<List<Lecture>> GetDailyLectureAsync(int week, DayOfWeek dayOfWeek)
        {
            //实际上hub只使用日期来获取星期几，所以time没必要是真实日期
            DateTime time = DateTime.Parse("2021-3-1");
            var less = dayOfWeek + 7 - time.DayOfWeek;
            time = time.AddDays(less);
            var timeSlug = time.ToString("yyyy-MM-dd");
            var req = $"http://hub.m.hust.edu.cn/kcb/todate/JsonCourse.action?sj={timeSlug}&zc={week}";

            //application/x-www-form-urlencoded 需要用stringcontent设置
            //维护hub的傻逼们把2021第一个学期叫做2020 2
            var content = new StringContent("xqh=20202");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var switchTermMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://hub.m.hust.edu.cn/kcb/todate/XqJsonCourse.action");
            switchTermMessage.Content = content;
            await client.SendAsync(switchTermMessage);


            var message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, req);
            var result = await client.SendAsync(message);
            var s = await result.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<List<Lecture>>(s);
        }
    }
}
