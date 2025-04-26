using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ChatBot
{
    public class Consts
    {
        public struct GeoSearchResult
        {
            public string info { get; set; }
            public string status { get; set; }
            public string count { get; set; }
            public JsonArray geocodes { get; set; }
        }
        public struct WeatherResult
        {
            public string info { get; set; }
            public string status { get; set; }
            public JsonArray lives{ get; set; }
            public JsonArray forecast{ get; set; }
        }
        public struct Hitokoto
        {
            public int id { get; set; }
            public string uuid { get; set; }
            public string hitokoto { get; set; }
            public string type { get; set; }
            public string from { get; set; }
            public string from_who { get; set; }
            public string creator { get; set; }
            public int length { get; set; }

        }

        public struct commandInvoke
        {
            public string user { get; set; }
            public string[] args { get; set; }
        }
        public enum ReturnType
        {
            STRING = 0,
            IMAGE = 1,
            MIXED = 2
        }
        public struct PixivImage
        {
            public string name;
            public string author;
            public int id;
            public int pages;
            public List<Image> img;
        }
        
        public struct MessageSequence
        {
            public MessageSequence()
            {

                elem = new List<Tuple<ReturnType, object>>();
            }
            public List<Tuple<ReturnType,object>> elem;
            public int Count { get => elem.Count; }
            public int Add(ReturnType r,object o)
            {
                elem.Add(Tuple.Create(r,o));
                return elem.Count;
            }
        }
    }
}
