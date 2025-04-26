using ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotWin
{
    internal static class Extensions
    {
        public static ReturnData AsReturn(this string s,double t = -1)
        {
            return new ReturnData(ReturnTypes.STRING, s, timecost: t);
        }
        public static ReturnData AsError(this string s,int code = -1,double t = -1)
        {
            return new ReturnData(ReturnTypes.STRING, "", code, s, t);
        }
        public static ReturnData AsReturn(this Image i, double t = -1)
        {
            return new ReturnData(ReturnTypes.IMAGE, i,timecost: t);
        }
        public static ReturnData AsReturn(this MessageSequence ms,double t = -1)
        {
            return new ReturnData(ReturnTypes.MSGSEQ, ms,timecost: t);
        }
        public static ReturnData AsError(this Exception ex,double t = -1)
        {
            return new ReturnData(ReturnTypes.STRING, "", -2, $"{ex.Message}{(ex.StackTrace==null?"":"\n"+ex.StackTrace)}", t);
        }
    }
}
