using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatBot;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using ChatBotWin;
using static ChatBot.Consts;
using System.Net;

namespace ChatBot
{
    public class Functions
    {

        public static string version = "FUNCTIONS VER 3";
        NetUtils netUtils = new NetUtils();
        Bot bot = null;

        private static Bot bot = null;
        private static Handler handler = null;
        
        public struct CommandInfo
        {
            public string Command;
            public int MinArgCount;
            public int MaxArgCount;
            public ReturnTypes ReturnType = ReturnTypes.STRING;
            public Func<CommandInvoke, ReturnData> Function;
            //public Func<commandInvoke, object> ofunc;
            public string Hint;
            public User_Control.Permissions Permission = User_Control.Permissions.USER;
            public CommandInfo()
            {

            }
        }
        private CommandInfo _help = new CommandInfo()
        {
            Command = ".help",
            MinArgCount = 0,
            MaxArgCount = 0,
            Function = (b) =>
            {
                return ".time 获取时间\n.weather <地点> 获取天气\n.hitokoto 获取一言\n.bingpic 获取bing每日壁纸\n.wordref <word> 查询英文单词\n.pixrank <rank> 按照排名查找插画\n.pixid <id> 按照id查找插画"
                        .AsReturn();
            }
        };

        private CommandInfo _time = new CommandInfo()
        {
            Command = ".time",
            Hint = ".time",
            MinArgCount = 0,
            MaxArgCount = 0,
            Function = (a) => { return (DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss\n") + DateTime.Now.DayOfWeek.ToString()).AsReturn(); }
        };
        private CommandInfo _weather = new CommandInfo()
        {
            
            Command = ".weather",
            Hint = ".weather 地名\n快捷：\n0 湘潭\n1 长沙\n2 株洲",
            MinArgCount = 1,
            MaxArgCount = 1,
            Function = (b) =>
            {
                string[] a = b.args;
                try
                {
                    switch (a[0])
                    {
                        case "0":
                            a[0] = "湘潭";
                            break;
                        case "1":
                            a[0] = "长沙市";
                            break;
                        case "2":
                            a[0] = "株洲";
                            break;
                    }
                    
                    NetUtils.Resp r = NetUtils.Get($"https://restapi.amap.com/v3/geocode/geo?address={a[0]}&key={NetUtils.aMap_key}");
                    GeoSearchResult gr = JsonSerializer.Deserialize<GeoSearchResult>(r.ResponseText);
                    if (gr.status == "1")
                    {
                        if (gr.count == "0")
                        {
                            return "无结果".AsReturn();
                        }
                        if (!gr.geocodes[0].AsObject()["city"].ToString().Contains(a[0]) && !gr.geocodes[0].AsObject()["district"].ToString().Contains(a[0]))
                        {
                            string s = "无匹配,相似结果:\n";
                            foreach (var x in gr.geocodes)
                            {
                                s += $"{x["adcode"]}|{x["city"]}|{x["district"]}\n";
                            }
                            return s.AsReturn();
                        }
                        else
                        {
                            string s = "";
                            r = NetUtils.Get($"https://restapi.amap.com/v3/weather/weatherInfo?city={gr.geocodes[0]["adcode"]}&key={NetUtils.aMap_key}");
                            WeatherResult ww = JsonSerializer.Deserialize<WeatherResult>(r.ResponseText);
                            JsonObject d = ww.lives[0].AsObject();

                            s += $"{d["province"]}, {d["city"]}, {d["district"]}, {d["adcode"]}\n";
                            s += $"{d["weather"]}\n气温:{d["temperature_float"]}\n湿度:{d["humidity_float"]}\n";
                            s += $"{d["winddirection"]}风 {d["windpower"]}级\n";
                            s += $"更新于:{d["reporttime"]}";

                            return s.AsReturn();
                        }
                    }
                    else
                    {
                        return new ReturnData(ReturnTypes.STRING,"Operation Failed",-1,gr.info);
                    }
                    

                }catch (Exception ex)
                {
                    return ex.AsError();
                }
            }
        };

        private CommandInfo _hitokoto = new CommandInfo()
        {
            Command = ".hitokoto",
            Hint = ".hitokoto",
            MinArgCount = 0,
            MaxArgCount = 0,
            Function = (b) =>
            {
                try{    
                string s = "";
                NetUtils.Resp r = NetUtils.Get("https://v1.hitokoto.cn/");
                    Hitokoto h = JsonSerializer.Deserialize<Hitokoto>(r.ResponseText);
                s += h.hitokoto + "\n";
                s += $"--{(string.IsNullOrEmpty(h.from_who) ? h.from : h.from_who)} {(string.IsNullOrEmpty(h.from_who) ? "" : h.from)} 类型:{h.type}";
                    return s.AsReturn();
                }catch (Exception ex)
                {
                    return ex.AsError();
            }
            }
        };

        private CommandInfo _wordref = new CommandInfo()
        {
            Command = ".wordref",
            MinArgCount = 1,
            MaxArgCount = 1,
            Function = (b) =>
            {
                try{    
                string[] a = b.args;
                NetUtils.Resp r = NetUtils.Get($"https://www.wordreference.com/enzh/{a[0]}");
                HtmlDocument d = new HtmlDocument();
                d.LoadHtml(r.ResponseText);

                HtmlNode NodeNotFound = d.DocumentNode.SelectSingleNode("//*[@id=\"noEntryFound\"]");
                if (NodeNotFound != null)
                {
                        return NodeNotFound.InnerText.AsReturn();
                }

                string Result = "";
                HtmlNode NodeWord = d.DocumentNode.SelectSingleNode("//*[@id=\"articleHead\"]/h1");
                HtmlNode NodePronounce = d.DocumentNode.SelectSingleNode("//*[@id=\"pronunciation_widget\"]/div");
                HtmlNode NodeMeans = d.DocumentNode.SelectSingleNode("//*[@id=\"articleWRD\"]/table[1]");

                Result += NodeWord.InnerHtml + "\n";

                string s = "";
                foreach (HtmlNode x in NodePronounce.ChildNodes)
                {
                    s += x.InnerText;
                }
                string[] ss = s.Split("/");
                string type = s.Substring(0, 2);
                Result += $"{type}:/{ss[1]}/ {(type == "UK" ? "US" : "UK")}:/{ss[3]}/\n";

                foreach (HtmlNode x in NodeMeans.ChildNodes)
                {
                    //Console.WriteLine(x.InnerHtml);
                    if (x.Attributes["id"] != null)
                    {
                        Result += $"{x.FirstChild.FirstChild.InnerHtml}({x.FirstChild.FirstChild.NextSibling.NextSibling.InnerHtml}) {x.FirstChild.NextSibling.NextSibling.FirstChild.InnerText.Replace("SCSimplified Chinese ", "")}\n";
                        //Console.WriteLine(x.Attributes["id"]);
                    }
                }
                Console.WriteLine(Result);
                    return Result.AsReturn();
                }catch (Exception ex)
                {
                    return ex.AsError();
            }
            }
        };

        public CommandInfo _luck = new CommandInfo()
        {
            Command = ".luck",
            Hint = ".luck",
            MinArgCount = 0,
            MaxArgCount = 0,
            Permission = User_Control.Permissions.USER,
            Function = (b) =>
            {
                string u = b.user;
                return (Math.Abs(u.GetHashCode() * DateTime.Now.Month * DateTime.Now.Day) % 100 + 1).ToString().AsReturn();
            }
        };

        public CommandInfo _bingpic = new CommandInfo()
        {
            Command = ".bingpic",
            Hint = ".bingpic",
            Permission = User_Control.Permissions.USER,
            ReturnType = ReturnTypes.MSGSEQ,
            Function = (b) =>
            {
                try{
                    MessageSequence ms = new MessageSequence();

                string picBase = "https://cn.bing.com";
                string linkInfo = "https://raw.onmicrosoft.cn/Bing-Wallpaper-Action/main/data/zh-CN_all.json";

                JsonObject j = JsonSerializer.Deserialize<JsonObject>(NetUtils.Get(linkInfo).ResponseText);
                JsonObject pic = j["data"].AsArray()[0].AsObject();

                if (!(j["message"].ToString() == "ok"))
                {
                        ms.Add(ReturnTypes.STRING, j["message"]);
                        return ms.AsReturn();
                }

                    ms.Add(ReturnTypes.STRING, $"Last Update:{j["LastUpdate"]}\n");
                    ms.Add(ReturnTypes.STRING, $"Lang:{j["Language"]}\n");
                    ms.Add(ReturnTypes.IMAGE, Image.FromStream(NetUtils.Get($"{picBase}{pic["url"]}",RawStream:true).RawStream));
                    ms.Add(ReturnTypes.STRING, $"From:{pic["copyright"]}\n");
                    ms.Add(ReturnTypes.STRING, $"{pic["startdate"]}");
                
                    return ms.AsReturn();
                }catch (Exception e)
                {
                    return e.AsError();
                }
            }
        };

        private CommandInfo _pixiv_rank = new CommandInfo()
        {
            Command = ".pixrank",
            MinArgCount = 1,
            MaxArgCount = 1,
            Hint = ".pixrank rank (1~50)",
            Permission = User_Control.Permissions.USER,
            ReturnType = ReturnTypes.MSGSEQ,
            Function = (b) =>
            {
                MessageSequence ms = new MessageSequence();
                string cookie_str = "first_visit_datetime_pc=2024-09-05%2019%3A06%3A05; p_ab_id=2; p_ab_id_2=0; p_ab_d_id=1845953874; yuid_b=MSBpNlE; __utmz=235335808.1725530769.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); privacy_policy_agreement=7; privacy_policy_notification=0; a_type=0; b_type=0; __utmc=235335808; _gid=GA1.2.1504319134.1745390523; device_token=87a3d7f04f6182f9eaf58ff0d8c43ad6; c_type=28; first_visit_datetime=2025-04-23%2015%3A45%3A23; webp_available=1; login_ever=yes; _lr_geo_location_state=TXG; _lr_geo_location=TW; _ga_3WKBFJLFCP=GS1.1.1745390723.1.1.1745391082.0.0.0; _gcl_au=1.1.650255852.1745391171; cc1=2025-04-23%2019%3A23%3A12; __utma=235335808.1308468258.1725530769.1745390505.1745403792.6; PHPSESSID=80665076_IBwGIAvviw9OZNe1J1BmQHbU1dMY4fuQ; _ga_MZ1NL4PHH0=GS1.1.1745405612.4.1.1745405630.0.0.0; __utmv=235335808.|2=login%20ever=yes=1^3=plan=normal=1^9=p_ab_id=2=1^10=p_ab_id_2=0=1^20=webp_available=yes=1; __utmt=1; _ga=GA1.1.1308468258.1725530769; FCNEC=%5B%5B%22AKsRol8ZtFUWFsmNhReXT5WNWVkC_cBvFisKjDzjlSJ4LNd2OZjvovi551PLpb7o7Xj-mwSCHGDqqBwEwXRg_TpdcJtIYbG29ZNnuQSovR3LciIy1YLWMfkT78dirpmyt5T44rgfbaSI8DHGrsWpDHE4kBMxthUXMg%3D%3D%22%5D%5D; __cf_bm=H8FD6qbhR2xbjsfV6DCph0beMTszFJaEbNehUUPA5ms-1745405865-1.0.1.1-1qCZuAZhFiIbwhhBouYX7ZvCvUgiadZR3VyVGKB._9Ktugv0L6zVML5L2bwWud2K_K02oVji2UymTxOsV_LLE9WkojK8MxJUK9Z.35ZMtoo276OwxJv2iusCNY14ww4b; _ga_75BBYNYN9J=GS1.1.1745403791.7.1.1745405864.0.0.0; __utmb=235335808.15.10.1745403792; cf_clearance=mUXul3W7BwoJRNwfZktNeDuKAScjeWAEAJlW9jPMTx4-1745405865-1.2.1.1-2t_WTPp0RdvLYLrPEMuP1aLwWoHSR7pRTgkRpOYGytJhyZh54olcPmvtblrHDRxm3I2_KELyYfgkiYSL4TTn4RboyfkkYCMzhwEPBJGVKDdJOh7X.lEkHcwNqZSJ963bwdteSn7HLf_w9z4DPt1d.v2RJFCLtS0yBJyelhSOXLvEOGxbONNbL8BeBtY0.6hmI6l6mTfDikB_aMwyS1_NHicpAd866DszF784IXzkYM62T1AMehFb96XoGz3MJVot9JJfyf9s.bAhPitrvkIA06dfTCcaRokOmslgE6Fr5m0WDn23s8XioxjR.W0k8qWzbaD4WGaFlkKB.lOwS411O2.PHWOGW2uk1kbCQy5mesg";
                CookieContainer cookie_container = NetUtils.Str2CookieContainer(cookie_str,".pixiv.net");
                try
                {
                    PixivUtils p = new PixivUtils(cookie_container);

                    PixivImage pi = p.Id2Image(p.Rank2Id(int.Parse(b.args[0])));

                    ms.Add(ReturnTypes.STRING, $"{pi.name} {pi.author}\n{pi.id} page:{pi.pages}");
                    foreach (Image i in pi.img)
                    {
                        ms.Add(ReturnTypes.IMAGE, i);
                    }

                    return ms.AsReturn();
                }
                catch (Exception e) 
                {
                    return e.AsError();
                }
            }
        };

        private CommandInfo _pixiv_id = new CommandInfo()
        {
            Command = ".pixid",
            MinArgCount = 1,
            MaxArgCount= 1,
            Hint = ".pixid id",
            Permission = User_Control.Permissions.USER,
            ReturnType = ReturnTypes.MSGSEQ,
            Function = (b) =>
            {
                MessageSequence ms = new MessageSequence();
                string cookie_str = "first_visit_datetime_pc=2024-09-05%2019%3A06%3A05; p_ab_id=2; p_ab_id_2=0; p_ab_d_id=1845953874; yuid_b=MSBpNlE; __utmz=235335808.1725530769.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); privacy_policy_agreement=7; privacy_policy_notification=0; a_type=0; b_type=0; __utmc=235335808; _gid=GA1.2.1504319134.1745390523; device_token=87a3d7f04f6182f9eaf58ff0d8c43ad6; c_type=28; first_visit_datetime=2025-04-23%2015%3A45%3A23; webp_available=1; login_ever=yes; _lr_geo_location_state=TXG; _lr_geo_location=TW; _ga_3WKBFJLFCP=GS1.1.1745390723.1.1.1745391082.0.0.0; _gcl_au=1.1.650255852.1745391171; cc1=2025-04-23%2019%3A23%3A12; __utma=235335808.1308468258.1725530769.1745390505.1745403792.6; PHPSESSID=80665076_IBwGIAvviw9OZNe1J1BmQHbU1dMY4fuQ; _ga_MZ1NL4PHH0=GS1.1.1745405612.4.1.1745405630.0.0.0; __utmv=235335808.|2=login%20ever=yes=1^3=plan=normal=1^9=p_ab_id=2=1^10=p_ab_id_2=0=1^20=webp_available=yes=1; __utmt=1; _ga=GA1.1.1308468258.1725530769; FCNEC=%5B%5B%22AKsRol8ZtFUWFsmNhReXT5WNWVkC_cBvFisKjDzjlSJ4LNd2OZjvovi551PLpb7o7Xj-mwSCHGDqqBwEwXRg_TpdcJtIYbG29ZNnuQSovR3LciIy1YLWMfkT78dirpmyt5T44rgfbaSI8DHGrsWpDHE4kBMxthUXMg%3D%3D%22%5D%5D; __cf_bm=H8FD6qbhR2xbjsfV6DCph0beMTszFJaEbNehUUPA5ms-1745405865-1.0.1.1-1qCZuAZhFiIbwhhBouYX7ZvCvUgiadZR3VyVGKB._9Ktugv0L6zVML5L2bwWud2K_K02oVji2UymTxOsV_LLE9WkojK8MxJUK9Z.35ZMtoo276OwxJv2iusCNY14ww4b; _ga_75BBYNYN9J=GS1.1.1745403791.7.1.1745405864.0.0.0; __utmb=235335808.15.10.1745403792; cf_clearance=mUXul3W7BwoJRNwfZktNeDuKAScjeWAEAJlW9jPMTx4-1745405865-1.2.1.1-2t_WTPp0RdvLYLrPEMuP1aLwWoHSR7pRTgkRpOYGytJhyZh54olcPmvtblrHDRxm3I2_KELyYfgkiYSL4TTn4RboyfkkYCMzhwEPBJGVKDdJOh7X.lEkHcwNqZSJ963bwdteSn7HLf_w9z4DPt1d.v2RJFCLtS0yBJyelhSOXLvEOGxbONNbL8BeBtY0.6hmI6l6mTfDikB_aMwyS1_NHicpAd866DszF784IXzkYM62T1AMehFb96XoGz3MJVot9JJfyf9s.bAhPitrvkIA06dfTCcaRokOmslgE6Fr5m0WDn23s8XioxjR.W0k8qWzbaD4WGaFlkKB.lOwS411O2.PHWOGW2uk1kbCQy5mesg";
                CookieContainer cookie_container = NetUtils.Str2CookieContainer(cookie_str, ".pixiv.net");
                try
                {
                    PixivUtils p = new PixivUtils(cookie_container);

                    PixivImage pi = p.Id2Image(int.Parse(b.args[0]));

                    ms.Add(ReturnTypes.STRING, $"{pi.name} {pi.author}\n{pi.id} page:{pi.pages}");

                    foreach (Image i in pi.img)
                    {
                        ms.Add(ReturnTypes.IMAGE, i);
                    }
                    return ms.AsReturn();
                }
                catch (Exception e)
                {
                    return e.AsError();
                }
            }
        };

        private CommandInfo __version = new CommandInfo()
        {
            Command = "/version",
            Hint = "/version",
            MinArgCount = 0,
            MaxArgCount = 0,
            Permission = User_Control.Permissions.ADMIN,
            Function = (a) =>
            {
                return $"{Bot.version}\n{Handler.version}\n{Functions.version}\n{User_Control.version}"
                        .AsReturn();
            }
        };

        private CommandInfo __setp = new CommandInfo()
        {
            Command = "/setp",
            Hint = "/setp usr permission",
            MinArgCount = 2,
            Permission = User_Control.Permissions.ADMIN,
            Function = (b) =>
            {
                string[] a = b.args;
                try
                {
                    if (int.Parse(a[1]) > 2 || int.Parse(a[1]) < 0)
                    {
                        return $"无效的参数{a[1]}".AsError();
                    }
                    else
                    {
                        User_Control.user_permssions.Add(a[0], (User_Control.Permissions)(int.Parse(a[1])));
                        User_Control.SavePermission();
                        return "success".AsReturn();
                    }
                }
                catch(Exception ex)
                {
                    return ex.AsError();
                }
            }
        };

        private CommandInfo __delay = new CommandInfo()
        {
            Command = "/delay",
            MinArgCount = 1,
            MaxArgCount = 1,
            Permission = User_Control.Permissions.ADMIN,
            ReturnType = ReturnTypes.VOID,
            Function = (b) =>
            {
                if (int.TryParse(b.args[0], out int t))
                {
                    Thread.Sleep(t);
                }
                return "success".AsReturn();
            }
        };

        private CommandInfo __tasks = new CommandInfo()
        {
            Command = "/tasks",
            MinArgCount = 0,
            MaxArgCount = 0,
            Permission = User_Control.Permissions.ADMIN,
            Function = (b) =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var x in handler.taskStates)
                {
                    sb.Append($"{x.IdZ} {x.Id} {x.Command} {x.State}\n");
                }
                return (sb.ToString().Remove(sb.Length-1).AsReturn());
            }
        };


        public List<CommandInfo> _funcs = new List<CommandInfo>();

        public Functions(Bot b,Handler h)
        {
            if(b == null || h==null)
            {
                throw new ArgumentNullException();
            }
            else
        {
                bot = b;
                handler = h;
            }
            _funcs.Add(_help);
            _funcs.Add(_time);
            _funcs.Add(_weather);
            _funcs.Add(_hitokoto);
            _funcs.Add(_wordref);
            _funcs.Add(__version);
            _funcs.Add(__setp);
            _funcs.Add(_luck);
            _funcs.Add(_bingpic);
            _funcs.Add(_pixiv_rank);
            _funcs.Add(_pixiv_id);
            _funcs.Add(__tasks);
            _funcs.Add(__delay);
        }
    }
}
