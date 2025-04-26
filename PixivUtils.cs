using ChatBot;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Net.Http;
using Hreq = System.Net.HttpWebRequest;
using Hresp = System.Net.HttpWebResponse;
using static ChatBot.NetUtils;

namespace ChatBotWin
{
    public class PixivUtils
    {
        

        public const string RankLink = "https://www.pixiv.net/ranking.php?mode=daily&content=illust";
        public const string PicBase = "https://www.pixiv.net/ajax/illust/";

        public CookieContainer cc = null;
        
        public PixivUtils(CookieContainer cc)
        {
            this.cc = cc;
        }

        public int Rank2Id(int rank)
        {
            if (rank > 50 || rank < 0)
            {
                return -1;
            }

            HtmlAgilityPack.HtmlDocument d = new HtmlAgilityPack.HtmlDocument();
            NetUtils.Resp r = NetUtils.Get("https://www.pixiv.net/ranking.php?content=illust", 16000, cc).Assert();
            d.LoadHtml(r.ResponseText);

            // HtmlNode ndate = d.DocumentNode.SelectSingleNode("//*[@id=\"wrapper\"]/div[1]/div/div[2]/div/nav[2]/ul/li[2]/a");
            HtmlNode nlist = d.DocumentNode.SelectSingleNode("//*[@id=\"wrapper\"]/div[1]/div/div[3]/div[1]");
            HtmlNode npic = nlist.ChildNodes[rank - 1];

            return int.Parse(npic.Attributes["data-id"].Value);
        }

        public Consts.PixivImage Id2Image(int id)
        {
            string link = PicBase + id.ToString();
            string jt = NetUtils.Get(link, 10000, cc).Assert().ResponseText;
            JsonObject j = JsonSerializer.Deserialize<JsonObject>(jt);

            if ((bool)j["error"])
            {
                throw new Exception(j["message"].AsValue().ToString());
            }

            j = j["body"].AsObject();
            Consts.PixivImage img = new Consts.PixivImage();

            img.name = j["illustTitle"].ToString();
            img.author = j["userName"].ToString();
            img.id = int.Parse(j["id"].AsValue().ToString());
            img.img = new List<Image>();
            img.pages = int.Parse(j["pageCount"].ToString());

            if (img.pages > 1)
            {
                j = JsonSerializer.Deserialize<JsonObject>(NetUtils.Get(PicBase + j["id"].ToString() + "/pages").ResponseText);
                if ((bool)j["error"])
                {
                    throw new Exception(j["message"].AsValue().ToString());
                }
                foreach (JsonObject x in j["body"].AsArray())
                {
                    Hreq preq = Hreq.Create(x["urls"]["original"].ToString().Replace("i.pximg.net", "i.pixiv.re")) as Hreq;
                    preq.UserAgent = NetUtils.UserAgent;
                    preq.Referer = "https://www.pixiv.net/";
                    preq.CookieContainer = cc;
                    preq.Accept = "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8";
                    Hresp presp = preq.GetResponse() as Hresp;
                    if (presp.StatusCode == HttpStatusCode.OK)
                    {
                        img.img.Add(Image.FromStream(presp.GetResponseStream()));
                    }
                    else
                    {
                        throw new HttpRequestException($"{presp.StatusCode}");
                    }
                }
                return img;
            }

            Hreq req = Hreq.Create(j["urls"]["original"].ToString()) as Hreq;
            req.UserAgent = NetUtils.UserAgent;
            req.Referer = "https://www.pixiv.net/";
            req.CookieContainer = cc;
            req.Accept = "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8";
            Hresp resp = req.GetResponse() as Hresp;
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                img.img.Add(Image.FromStream(resp.GetResponseStream()));
                return img;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
