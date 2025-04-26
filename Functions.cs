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

        
        public struct aFunc
        {
            public string command;
            public int argc;
            public Consts.ReturnType ret = Consts.ReturnType.STRING;
            public Func<Consts.commandInvoke, string> func;
            public Func<Consts.commandInvoke, object> ofunc;
            public string hint;
            public User_Control.Permissions permission = User_Control.Permissions.USER;
            public aFunc()
            {

            }
        }
        private aFunc _help = new aFunc()
        {
            command = ".help",
            func = (b) =>
            {
                return ".time 获取时间\n.weather <地点> 获取天气\n.hitokoto 获取一言\n.bingpic 获取bing每日壁纸\n.wordref <word> 查询英文单词\n.pixrank <rank> 按照排名查找插画\n.pixid <id> 按照id查找插画";
            }
        };

        private aFunc _time = new aFunc()
        {
            command = ".time",
            hint = ".time",
            argc = 0,
            func = (a) => { return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss\n") + DateTime.Now.DayOfWeek.ToString(); }
        };
        private aFunc _weather = new aFunc()
        {
            
            command = ".weather",
            hint = ".weather 地名\n快捷：\n0 湘潭\n1 长沙\n2 株洲",
            argc = 1,
            func = (b) =>
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
                    Consts.GeoSearchResult gr = JsonSerializer.Deserialize<Consts.GeoSearchResult>(r.ResponseText);
                    if (gr.status == "1")
                    {
                        if (gr.count == "0")
                        {
                            return "无结果";
                        }
                        if (!gr.geocodes[0].AsObject()["city"].ToString().Contains(a[0]) && !gr.geocodes[0].AsObject()["district"].ToString().Contains(a[0]))
                        {
                            string s = "无匹配,相似结果:\n";
                            foreach (var x in gr.geocodes)
                            {
                                s += $"{x["adcode"]}|{x["city"]}|{x["district"]}\n";
                            }
                            return s;
                        }
                        else
                        {
                            string s = "";
                            r = NetUtils.Get($"https://restapi.amap.com/v3/weather/weatherInfo?city={gr.geocodes[0]["adcode"]}&key={NetUtils.aMap_key}");
                            Consts.WeatherResult ww = JsonSerializer.Deserialize<Consts.WeatherResult>(r.ResponseText);
                            JsonObject d = ww.lives[0].AsObject();

                            s += $"{d["province"]}, {d["city"]}, {d["district"]}, {d["adcode"]}\n";
                            s += $"{d["weather"]}\n气温:{d["temperature_float"]}\n湿度:{d["humidity_float"]}\n";
                            s += $"{d["winddirection"]}风 {d["windpower"]}级\n";
                            s += $"更新于:{d["reporttime"]}";

                            return s;
                        }
                    }
                    else
                    {
                        return "Operation Failed " + gr.info;
                    }
                    

                }catch (Exception ex)
                {
                    return ex.Message+ex.StackTrace;
                }
            }
        };

        private aFunc _hitokoto = new aFunc()
        {
            command = ".hitokoto",
            hint = ".hitokoto",
            func = (b) =>
            {
                string s = "";
                NetUtils.Resp r = NetUtils.Get("https://v1.hitokoto.cn/");
                Consts.Hitokoto h = JsonSerializer.Deserialize<Consts.Hitokoto>(r.ResponseText);
                s += h.hitokoto + "\n";
                s += $"--{(string.IsNullOrEmpty(h.from_who) ? h.from : h.from_who)} {(string.IsNullOrEmpty(h.from_who) ? "" : h.from)} 类型:{h.type}";
                return s;
            }
        };

        private aFunc _wordref = new aFunc()
        {
            command = ".wordref",
            argc = 1,
            func = (b) =>
            {
                string[] a = b.args;
                NetUtils.Resp r = NetUtils.Get($"https://www.wordreference.com/enzh/{a[0]}");
                HtmlDocument d = new HtmlDocument();
                d.LoadHtml(r.ResponseText);

                HtmlNode NodeNotFound = d.DocumentNode.SelectSingleNode("//*[@id=\"noEntryFound\"]");
                if (NodeNotFound != null)
                {
                    return NodeNotFound.InnerText;
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
                return Result;
            }
        };

        public aFunc _luck = new aFunc()
        {
            command = ".luck",
            hint = ".luck",
            permission = User_Control.Permissions.USER,
            func = (b) =>
            {
                string u = b.user;
                return (Math.Abs(u.GetHashCode() * DateTime.Now.Month * DateTime.Now.Day) % 100 + 1).ToString();
            }
        };

        public aFunc _bingpic = new aFunc()
        {
            command = ".bingpic",
            hint = ".bingpic",
            permission = User_Control.Permissions.USER,
            ret = Consts.ReturnType.MIXED,
            ofunc = (b) =>
            {
                Consts.MessageSequence ms = new MessageSequence();

                string picBase = "https://cn.bing.com";
                string linkInfo = "https://raw.onmicrosoft.cn/Bing-Wallpaper-Action/main/data/zh-CN_all.json";

                JsonObject j = JsonSerializer.Deserialize<JsonObject>(NetUtils.Get(linkInfo).ResponseText);
                JsonObject pic = j["data"].AsArray()[0].AsObject();

                if (!(j["message"].ToString() == "ok"))
                {
                    ms.Add(Consts.ReturnType.STRING, j["message"]);
                    return ms;
                }

                ms.Add(Consts.ReturnType.STRING, $"Last Update:{j["LastUpdate"]}\n");
                ms.Add(Consts.ReturnType.STRING, $"Lang:{j["Language"]}\n");
                ms.Add(Consts.ReturnType.IMAGE, Image.FromStream(NetUtils.Get($"{picBase}{pic["url"]}",RawStream:true).RawStream));
                ms.Add(Consts.ReturnType.STRING, $"From:{pic["copyright"]}\n");
                ms.Add(Consts.ReturnType.STRING, $"{pic["startdate"]}");
                
                return ms;
            }
        };

        public aFunc _pixiv_rank = new aFunc()
        {
            command = ".pixrank",
            argc = 1,
            hint = ".pixrank rank (1~50)",
            permission = User_Control.Permissions.USER,
            ret = Consts.ReturnType.MIXED,
            ofunc = (b) =>
            {
                Consts.MessageSequence ms = new Consts.MessageSequence();
                string cookie_str = "first_visit_datetime_pc=2024-09-05%2019%3A06%3A05; p_ab_id=2; p_ab_id_2=0; p_ab_d_id=1845953874; yuid_b=MSBpNlE; __utmz=235335808.1725530769.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); privacy_policy_agreement=7; privacy_policy_notification=0; a_type=0; b_type=0; __utmc=235335808; _gid=GA1.2.1504319134.1745390523; device_token=87a3d7f04f6182f9eaf58ff0d8c43ad6; c_type=28; first_visit_datetime=2025-04-23%2015%3A45%3A23; webp_available=1; login_ever=yes; _lr_geo_location_state=TXG; _lr_geo_location=TW; _ga_3WKBFJLFCP=GS1.1.1745390723.1.1.1745391082.0.0.0; _gcl_au=1.1.650255852.1745391171; cc1=2025-04-23%2019%3A23%3A12; __utma=235335808.1308468258.1725530769.1745390505.1745403792.6; PHPSESSID=80665076_IBwGIAvviw9OZNe1J1BmQHbU1dMY4fuQ; _ga_MZ1NL4PHH0=GS1.1.1745405612.4.1.1745405630.0.0.0; __utmv=235335808.|2=login%20ever=yes=1^3=plan=normal=1^9=p_ab_id=2=1^10=p_ab_id_2=0=1^20=webp_available=yes=1; __utmt=1; _ga=GA1.1.1308468258.1725530769; FCNEC=%5B%5B%22AKsRol8ZtFUWFsmNhReXT5WNWVkC_cBvFisKjDzjlSJ4LNd2OZjvovi551PLpb7o7Xj-mwSCHGDqqBwEwXRg_TpdcJtIYbG29ZNnuQSovR3LciIy1YLWMfkT78dirpmyt5T44rgfbaSI8DHGrsWpDHE4kBMxthUXMg%3D%3D%22%5D%5D; __cf_bm=H8FD6qbhR2xbjsfV6DCph0beMTszFJaEbNehUUPA5ms-1745405865-1.0.1.1-1qCZuAZhFiIbwhhBouYX7ZvCvUgiadZR3VyVGKB._9Ktugv0L6zVML5L2bwWud2K_K02oVji2UymTxOsV_LLE9WkojK8MxJUK9Z.35ZMtoo276OwxJv2iusCNY14ww4b; _ga_75BBYNYN9J=GS1.1.1745403791.7.1.1745405864.0.0.0; __utmb=235335808.15.10.1745403792; cf_clearance=mUXul3W7BwoJRNwfZktNeDuKAScjeWAEAJlW9jPMTx4-1745405865-1.2.1.1-2t_WTPp0RdvLYLrPEMuP1aLwWoHSR7pRTgkRpOYGytJhyZh54olcPmvtblrHDRxm3I2_KELyYfgkiYSL4TTn4RboyfkkYCMzhwEPBJGVKDdJOh7X.lEkHcwNqZSJ963bwdteSn7HLf_w9z4DPt1d.v2RJFCLtS0yBJyelhSOXLvEOGxbONNbL8BeBtY0.6hmI6l6mTfDikB_aMwyS1_NHicpAd866DszF784IXzkYM62T1AMehFb96XoGz3MJVot9JJfyf9s.bAhPitrvkIA06dfTCcaRokOmslgE6Fr5m0WDn23s8XioxjR.W0k8qWzbaD4WGaFlkKB.lOwS411O2.PHWOGW2uk1kbCQy5mesg";
                CookieContainer cookie_container = NetUtils.Str2CookieContainer(cookie_str,".pixiv.net");
                try
                {
                    PixivUtils p = new PixivUtils(cookie_container);

                    PixivImage pi = p.Id2Image(p.Rank2Id(int.Parse(b.args[0])));

                    ms.Add(Consts.ReturnType.STRING, $"{pi.name} {pi.author}\n{pi.id} page:{pi.pages}");
                    foreach (Image i in pi.img)
                    {
                        ms.Add(Consts.ReturnType.IMAGE, i);
                    }

                    return ms;
                }
                catch (Exception e) 
                {
                    ms.Add(Consts.ReturnType.STRING,e.Message);
                    return ms;
                }
            }
        };

        public aFunc _pixiv_id = new aFunc()
        {
            command = ".pixid",
            argc = 1,
            hint = ".pixid id",
            permission = User_Control.Permissions.USER,
            ret = Consts.ReturnType.MIXED,
            ofunc = (b) =>
            {
                Consts.MessageSequence ms = new Consts.MessageSequence();
                string cookie_str = "first_visit_datetime_pc=2024-09-05%2019%3A06%3A05; p_ab_id=2; p_ab_id_2=0; p_ab_d_id=1845953874; yuid_b=MSBpNlE; __utmz=235335808.1725530769.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); privacy_policy_agreement=7; privacy_policy_notification=0; a_type=0; b_type=0; __utmc=235335808; _gid=GA1.2.1504319134.1745390523; device_token=87a3d7f04f6182f9eaf58ff0d8c43ad6; c_type=28; first_visit_datetime=2025-04-23%2015%3A45%3A23; webp_available=1; login_ever=yes; _lr_geo_location_state=TXG; _lr_geo_location=TW; _ga_3WKBFJLFCP=GS1.1.1745390723.1.1.1745391082.0.0.0; _gcl_au=1.1.650255852.1745391171; cc1=2025-04-23%2019%3A23%3A12; __utma=235335808.1308468258.1725530769.1745390505.1745403792.6; PHPSESSID=80665076_IBwGIAvviw9OZNe1J1BmQHbU1dMY4fuQ; _ga_MZ1NL4PHH0=GS1.1.1745405612.4.1.1745405630.0.0.0; __utmv=235335808.|2=login%20ever=yes=1^3=plan=normal=1^9=p_ab_id=2=1^10=p_ab_id_2=0=1^20=webp_available=yes=1; __utmt=1; _ga=GA1.1.1308468258.1725530769; FCNEC=%5B%5B%22AKsRol8ZtFUWFsmNhReXT5WNWVkC_cBvFisKjDzjlSJ4LNd2OZjvovi551PLpb7o7Xj-mwSCHGDqqBwEwXRg_TpdcJtIYbG29ZNnuQSovR3LciIy1YLWMfkT78dirpmyt5T44rgfbaSI8DHGrsWpDHE4kBMxthUXMg%3D%3D%22%5D%5D; __cf_bm=H8FD6qbhR2xbjsfV6DCph0beMTszFJaEbNehUUPA5ms-1745405865-1.0.1.1-1qCZuAZhFiIbwhhBouYX7ZvCvUgiadZR3VyVGKB._9Ktugv0L6zVML5L2bwWud2K_K02oVji2UymTxOsV_LLE9WkojK8MxJUK9Z.35ZMtoo276OwxJv2iusCNY14ww4b; _ga_75BBYNYN9J=GS1.1.1745403791.7.1.1745405864.0.0.0; __utmb=235335808.15.10.1745403792; cf_clearance=mUXul3W7BwoJRNwfZktNeDuKAScjeWAEAJlW9jPMTx4-1745405865-1.2.1.1-2t_WTPp0RdvLYLrPEMuP1aLwWoHSR7pRTgkRpOYGytJhyZh54olcPmvtblrHDRxm3I2_KELyYfgkiYSL4TTn4RboyfkkYCMzhwEPBJGVKDdJOh7X.lEkHcwNqZSJ963bwdteSn7HLf_w9z4DPt1d.v2RJFCLtS0yBJyelhSOXLvEOGxbONNbL8BeBtY0.6hmI6l6mTfDikB_aMwyS1_NHicpAd866DszF784IXzkYM62T1AMehFb96XoGz3MJVot9JJfyf9s.bAhPitrvkIA06dfTCcaRokOmslgE6Fr5m0WDn23s8XioxjR.W0k8qWzbaD4WGaFlkKB.lOwS411O2.PHWOGW2uk1kbCQy5mesg";
                CookieContainer cookie_container = NetUtils.Str2CookieContainer(cookie_str, ".pixiv.net");
                try
                {
                    PixivUtils p = new PixivUtils(cookie_container);

                    PixivImage pi = p.Id2Image(int.Parse(b.args[0]));

                    ms.Add(Consts.ReturnType.STRING, $"{pi.name} {pi.author}\n{pi.id} page:{pi.pages}");

                    foreach (Image i in pi.img)
                    {
                        ms.Add(Consts.ReturnType.IMAGE, i);
                    }
                    return ms;
                }
                catch (Exception e)
                {
                    ms.Add(Consts.ReturnType.STRING, e.Message);
                    return ms;
                }
            }
        };

        public aFunc __version = new aFunc()
        {
            command = "/version",
            hint = "/version",
            permission = User_Control.Permissions.ADMIN,
            func = (a) =>
            {
                return $"{Bot.version}\n{Handler.version}\n{Functions.version}\n{User_Control.version}";
            }
        };

        public aFunc __setp = new aFunc()
        {
            command = "/setp",
            hint = "/setp usr permission",
            argc = 2,
            permission = User_Control.Permissions.ADMIN,
            func = (b) =>
            {
                string[] a = b.args;
                try
                {
                    if (int.Parse(a[1]) > 2 || int.Parse(a[1]) < 0)
                    {
                        return $"无效的参数{a[1]}";
                    }
                    else
                    {
                        User_Control.user_permssions.Add(a[0], (User_Control.Permissions)(int.Parse(a[1])));
                        User_Control.SavePermission();
                        return "success";
                    }
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }
            }
        };


        public List<aFunc> _funcs = new List<aFunc>();

        public Functions()
        {
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
        }
    }
}
