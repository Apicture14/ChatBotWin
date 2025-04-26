using FlaUI.Core.AutomationElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaUI;
using System.Data;
using FlaUI.Core.WindowsAPI;
using System.Diagnostics;
using ChatBotWin;

namespace ChatBot
{
    public class Handler
    {
        public static string version = "HANDLER VER 1";

        private Bot b;
        private Functions fus = new Functions();
        private List<Thread> tasks = new List<Thread>();
        public int taskCountAllowed = 3;
        public Handler(Bot b)
        {
            this.b = b;
        }
        public void HandleMessage(string msg,string user)
        {
            // b.ControlPanel.Log("Send");
            if (msg.StartsWith($"@{b.selfUserName}"))
            {
                long st=0, et=0;
                bool p = false;
                var t = msg.Substring(b.selfUserName.Length+2).Split(' ');
                string command = t[0];
                string[] args = t.Skip(1).ToArray();
                foreach (Functions.aFunc f in fus._funcs)
                {
                    if (command == f.command)
                    {
                        p = true;
                        st = DateTime.Now.Ticks;
                        if (args.Length == f.argc)
                        {
                            b.ControlPanel.Log($"Command {command} is invoked by {user}");
                            b.SendText($"Start to Process {msg} at 0s",false,"");
                            if (User_Control.VerifyPermission(user, f.permission))
                            {
                                switch (f.ret)
                                {
                                    case Consts.ReturnType.STRING:
                                        b.SendText(f.func(new Consts.commandInvoke() { user=user,args=args }),true,user);
                                        break;
                                    case Consts.ReturnType.IMAGE:
                                        b.AddImage(f.ofunc(new Consts.commandInvoke() { user = user, args = args }) as Image);
                                        break;
                                    case Consts.ReturnType.MIXED:
                                        Consts.MessageSequence ms = (Consts.MessageSequence)f.ofunc(new Consts.commandInvoke() { user = user, args = args });
                                        b.SendMS(ms);
                                        break;
                                }
                                
                            }
                            else
                            {
                                b.SendText($"访问受限\nPermission Denied({f.permission} Required)",true,user);
                            }
                            
                            break;
                        }
                        b.SendText("错误的命令:\n"+f.hint,true,user);
                    }
                }
                if (p)
                {
                    et = DateTime.Now.Ticks;
                    b.ControlPanel.Log($"Anwser in {(et - st) / 10000000.0}s");
                }
            }
        }
        
    }
}
