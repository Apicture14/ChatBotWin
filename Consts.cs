using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ChatBot
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

    public struct CommandInvoke
    {
        public string user;
        public string[] args;
        public CommandInvoke(string user, string[] args)
        {
            this.user = user;
            this.args = args;
        }

    }
    public enum ReturnTypes
    {
        STRING = 0,
        IMAGE = 1,
        MSGSEQ = 2,
        VOID = 3
    }
    public struct ReturnData
    {
        public ReturnTypes type;
        public int code;
        public string msg;
        public object data;
        public double timecost;
        public ReturnData(ReturnTypes type, object data, int code = 0, string msg = "", double timecost = -1d)
        {
            this.type = type;
            this.data = data;
            this.code = code;
            this.msg = msg;
            this.timecost = timecost;
        }
    }
    public enum TaskState
    {
        Pending = 0,
        Processing = 1,
        Finished = 2
    }
    public struct TaskInfo
    {
        public string Command;
        public int Id;
        public int IdZ;
        public TaskState State;
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

            elem = new List<Tuple<ReturnTypes, object>>();
        }
        public List<Tuple<ReturnTypes,object>> elem;
        public int Count { get => elem.Count; }
        public int Add(ReturnTypes r,object o)
        {
            elem.Add(Tuple.Create(r,o));
            return elem.Count;
        }
    }
    
}
