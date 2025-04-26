using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Hreq = System.Net.HttpWebRequest;
using Hresp = System.Net.HttpWebResponse;
using System.Text.Encodings.Web;
using System.Web;
using System.Net;

namespace ChatBot
{

    public class NetUtils
    {
        public const string aMap_key = "2f22b16d60964da88e8642d19bbb6d5c";
        public struct Resp
        {
            public string SourceLink;
            public int StatusCode;
            public Stream RawStream;
            public string ResponseText;
            public Resp Assert()
            {
                if (this.StatusCode != 200)
                {
                    throw new HttpRequestException($"{StatusCode} "+ResponseText);
                }
                return this;
            }
        }
        public const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36 Edg/135.0.0.0";
        public static long GetTimeStamp()
        {
            var d = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return d;
        }

        public static CookieContainer Str2CookieContainer(string pairs,string domain=null)
        {
            CookieContainer cc = new CookieContainer();
            foreach (string pair in pairs.Split("; "))
            {
                if (!string.IsNullOrEmpty(pair))
                {
                    string[] parts = pair.Split("=");
                    cc.Add(new Cookie(parts[0], parts[1],"/",domain));
                }
            }
            return cc;
        }

        public static Resp Get(string url,int timeout=10000,CookieContainer cc = null,bool keep_alive = false,bool RawStream = false)
        {
            
            try{
                Hreq req = Hreq.Create(url) as Hreq;
                req.Method = "GET";
                req.UserAgent = UserAgent;
                req.KeepAlive = keep_alive;
                req.Timeout = timeout;
                if (cc != null) { req.CookieContainer = cc; }
                Hresp resp = req.GetResponse() as Hresp;
           
                if (RawStream)
                {
                    return new Resp
                    {
                        StatusCode = (int)resp.StatusCode,
                        RawStream = resp.GetResponseStream()
                    };
                }

                string s = new StreamReader(resp.GetResponseStream()).ReadToEnd();

                return new Resp()
                {
                    SourceLink = resp.ResponseUri.ToString(),
                    StatusCode = (int)resp.StatusCode,
                    ResponseText = s,
                    
                };
            }catch (Exception ex)
            {
                return new Resp
                {
                    ResponseText = ex.Message,
                    StatusCode = -1
                    
                };
            }
        }
        public static Resp Post(string url,string data,bool keep_alive)
        {
            Hreq req = Hreq.Create(url) as Hreq;
            req.Method = "POST";
            req.UserAgent = UserAgent;
            req.KeepAlive= keep_alive;

            Stream s = req.GetRequestStream();
            s.Write(Encoding.UTF8.GetBytes(data));
            s.Close();

            Hresp resp = req.GetResponse() as Hresp;
            string ss = new StreamReader(resp.GetResponseStream()).ReadToEnd();

            return new Resp()
            {
                SourceLink = resp.ResponseUri.ToString(),
                StatusCode = (int)resp.StatusCode,
                ResponseText = ss
            };
        }
    }
}
