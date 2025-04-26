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
using System.Collections.Concurrent;

namespace ChatBot
{
    public class Handler
    {
        public static string version = "HANDLER VER 2";
        public bool Inited = false;

        private Bot b;
        private Functions fus;
        public int MaxTaskAccept = 3;
        public int MaxTaskRun = 2;
        private int TaskProcessing { get => tasks.Count; }
        private SemaphoreSlim semaphore;
        private ConcurrentQueue<Task<ReturnData>> tasks = new ConcurrentQueue<Task<ReturnData>>();

        public List<TaskInfo> taskStates = new List<TaskInfo>(); 
        public int historyCount = 10;
        private int NextTaskId = 0;

        
        public Handler(int taskRun, int taskAccept)
        {
            this.MaxTaskRun = taskRun;
            this.MaxTaskAccept = taskAccept;
            this.semaphore = new SemaphoreSlim(MaxTaskRun,MaxTaskRun);
        }
        public void Init(Bot bot)
        {
            this.b = bot;
            this.fus = new Functions(b, this);
            this.Inited = true;
        }
        public void ParseCommand(string msg,out string command,out string[] args)
        {
            var t = msg.Substring(b.selfUserName.Length + 2).Split(' ');
            command = t[0];
            args = t.Skip(1).ToArray();
        }
        public void AcceptMessage(string msg,string user)
        {
            if (!this.Inited)
            {
                throw new Exception("Handler Not Inited");
            }
            if (msg.StartsWith($"@{b.selfUserName}"))
            {
                string command;
                string[] args;
                ParseCommand(msg,out command,out args);
                foreach (Functions.CommandInfo f in fus._funcs)
                {
                    if (command == f.Command)
                    {
                        if (args.Length < f.MinArgCount || args.Length > f.MaxArgCount)
                        {
                            b.SendText("错误的命令:\n" + f.Hint, true, user);
                        }
                        else 
                        { 
                            b.ControlPanel.Log($"Command {command} is invoked by {user}");
                            if (tasks.Count >= MaxTaskAccept)
                            {
                                b.SendText($"无法接受命令,队列已满({MaxTaskAccept})", false, "");
                                return;
                            }
                            Task<ReturnData> t = Task.Run<ReturnData>
                            (() =>
                            {
                                Stopwatch st = new Stopwatch();
                                try
                                {
                                    st.Start();
                                    semaphore.Wait();
                                    ReturnData d = f.Function(new CommandInvoke(user, args));
                                    st.Stop();
                                    d.timecost = st.Elapsed.TotalSeconds;
                                    return d;
                                }
                                catch (Exception e)
                                {
                                    st.Stop();
                                    return e.AsError(st.Elapsed.TotalSeconds);
                                }
                                finally
                                {
                                    st.Reset();
                                    semaphore.Release();
                                }
                            });
                            tasks.Enqueue(t);
                            /*
                            if (taskStates.Count == historyCount)
                            {
                                taskStates.Remove(taskStates.Find((x)=>x.State == TaskState.Finished));
                            }
                            */
                            taskStates.Add(new TaskInfo { Command=$"{user} {msg.Replace($"@{b.selfUserName} ","")}",Id=t.Id,IdZ=NextTaskId,State=TaskState.Pending});
                            NextTaskId++;

                            ProcessMessage();
                            return;
                        }
                        
                    }
                }
            }
        }
        public async void ProcessMessage()
        {
            ReturnData d;
            try{    
                while (tasks.TryDequeue(out var t))
                {
                    
                    int i = taskStates.FindIndex((x)=>x.Id == t.Id);
                    if (i != -1) { TaskInfo x = taskStates.Find((x)=>x.Id==t.Id);x.State = TaskState.Processing; taskStates[i] = x; }

                    d = await t;
                    if (d.code != 0)
                    {
                        b.SendText(d.msg, false, "");
                    }
                    else
                    {
                        switch (d.type)
                        {
                            case ReturnTypes.STRING:
                                b.SendText(d.data as string, false, "");
                                break;
                            case ReturnTypes.IMAGE:
                                b.AddImage(d.data as Image);
                                b.Send();
                                break;
                            case ReturnTypes.MSGSEQ:
                                b.SendMS((MessageSequence)d.data);
                                break;
                            case ReturnTypes.VOID:
                                break;
                        }
                        if  (d.timecost != -1d)
                        {
                            b.SendText($"Operation Finished in {d.timecost}s",false,"");
                        }
                        taskStates.Remove(taskStates.Find((x)=>x.Id == t.Id));
                        //taskStates.Find((x)=>x == )
                    }
                }
            }catch (Exception e)
            {
                b.SendText($"Error Occur When Depacking Return:\n{e.Message}\n{e.StackTrace}",false,"");
            }
        }
    }
}
