# Design
## 基本运作原理
基础思路就是通过模拟用户在浏览器上的正常登录流程，
来骗过学校的认证系统，从而抓取课程表信息。  
下面的所有说明皆以华科的Hub系统为例  
## Hub系统基本结构
Hub在几年前，实现了所谓的“统一身份认证平台”，
说白了就是一个很垃圾的openid授权。  
所以未登录用户查询课表的最短流程是：  
尝试获取课表->跳转至身份认证域名认证->
认证成功，携带token跳转回源域名的授权用endpoint->
跳转回课表页面  
> 授权endpoint没有response body，它唯一的作用是
> 通过header来设置用户的身份cookie

所以，我们需要通过编程来完美复现这个过程。  
> **Note**  
> 以下内容以阅读者具备较熟练的浏览器使用知识为前提。
> 如果还不了解这方面知识，请先看[这篇文章](https://www.limfx.pro/ReadArticle/724/ru-he-zheng-que-shi-yong-liu-lan-qi)
### 登录过程破解
登录界面域名是`pass.hust.edu.cn`，我们通过浏览器工具，
可以轻松发现它的登录信息界面是个form。  
然后我们查看页面源代码，可以看到hub的前端没使用复杂的前端框架，
只用了jQuery，而且文件名没有做哈希。直接就能看到有个
`login.standar.js`文件，里头有个`login`函数，
用脚想都知道这个是在登陆时调用的。。。  
可以加断点实验，你会发现确实如此。  
然后你会发现它调用了一个加密函数`strEnc`，而旁边就
是一个叫des的文件，里边第一个函数就是这个。容易看出
这里是使用了一个triple des加密算法（虽然不知道为啥
hub的程序员把变量命名为rsa）。  
可以看到它把学号加密码加`lt`一起放进去加密了，然后
放回表单。加密的密钥是'1'，'2'，'3'。  
此时我们回去分析表单，可以看到表单下方有一大排隐藏的input标签
其中的`lt`明显是用来放antiforgery token的。  
> 没听说过antiforgery token？没关系，它在这里没那么重要  

其它的位置，code对应验证码，ul对应学号长度，pl对应
密码长度。  
所以我们如果要骗过hub，就需要填满一张一模一样的表单
然后提交给他。我们为了验证我们上方的分析，可以先
在浏览器理手动登录一次，然后查看请求的记录信息。  
仔细分析后可以发现，有几个需要注意的地方：
- 请求的cookie是更改过的，在前方加了
	两个值`hust_cas_un`和`cas_hash`，第一个对应的是学号
	的值，第二个值不重要，可以置空。我猜应该是选中的html
	标签的id。
- contenttype是`application/x-www-form-urlencoded`
	这和传统的`multipart/form-data`不一样，虽然也是
	form，但是它没有boundary，详细差别可以见[这里](https://stackoverflow.com/questions/3508338/what-is-the-boundary-in-multipart-form-data)
分析到这里，就可以尝试去欺骗hub系统了  
## 代码实现
原计划直接使用AngleSharp来模拟浏览器（因为AngleSharp本身就是
一个小浏览器），但是因为c#的httpclient有个特殊的设计（见下方），
导致在hub系统里用会出问题。而且完全用AngleSharp性能也不好  
最终我使用了AngleSharp来提取form理各个隐藏的值，使用Jint
引擎来调用Hub系统的那个des加密函数，然后用HttpClient
手动发送请求来解决问题。  
> 特殊设计  
> c#中的HttpClient不允许自动从https连接重定向到http连接
> （出于安全考虑），而AngleSharp底层大量使用了HttpClient，
> 导致用AngleSharp认证成功后无法自动跳转到设置Cookie的
> 页面


这一部分源代码见[这里](../HubCourseScheduleFucker/HubFucker.cs)


