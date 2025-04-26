using FlaUI;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.AutomationElements.Infrastructure;
using System.Diagnostics;
using FlaUI.UIA3;
using FlaUI.Core.Definitions;
using Interop.UIAutomationClient;
using FlaUI.UIA3.EventHandlers;
using FlaUI.Core.Conditions;
using System.Windows;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using ChatBot;
using ChatBotWin;
using fListBox = FlaUI.Core.AutomationElements.ListBox;
using fApplication = FlaUI.Core.Application;
using fTextBox = FlaUI.Core.AutomationElements.TextBox;
using fButton = FlaUI.Core.AutomationElements.Button;
using Microsoft.VisualBasic.ApplicationServices;


public class Bot
{
    public static string version = "CONTROL VER 2";

    public fApplication WxApp;
    public UIA3Automation Automation = new UIA3Automation();
    public Window WxMainWindow;

    public fListBox SessionList;
    public ListBoxItem SessionOpened;
    
    public fListBox MessageList;
    public StructureChangedEventHandlerBase Handl;

    public fTextBox InputBox;
    public fButton ButtonSend;

    public Form1 ControlPanel;



    public string selfUserName = "";
    string LastSpeaker = "";
    string LastMessage = "";
    long LastTimeStamp = 0l;

    public Handler h;

    public Bot()
    {

    }

    public void Init(Form1 panel)
    {
        if (panel == null)
        {
            throw new Exception("");
        }

        ControlPanel = panel;

        Process[] p = Process.GetProcessesByName("WeChat");
        if (p.Length != 1)
        {
            throw new Exception("No or Multiple WeChat Instance!");
        }
        WxApp = fApplication.Attach(p[0].Id);
        WxMainWindow = WxApp.GetMainWindow(Automation);

        var User = WxMainWindow.FindFirstDescendant(c => c.ByName("Navigation")).FindFirstChild();

        if (User == null)
        {
            throw new Exception("Failed to get User");
        }
        else
        {
            ControlPanel.Log($"Successfully get User {User.Name}");
            selfUserName = User.Name;

            User_Control.LoadPermission();
        }


    }

    public void Start()
    {
        try{    
            MessageList = WxMainWindow.FindFirstDescendant(c => c.ByName("消息")).AsListBox();
            ButtonSend = WxMainWindow.FindFirstDescendant(c => c.ByName("Send (S)")).AsButton();
            InputBox = ButtonSend.Parent.Parent.Parent.FindFirstDescendant(c => c.ByControlType(ControlType.Edit)).AsTextBox();
            if (MessageList == null)
            {
                throw new Exception("Failed to Get Message List");
            }
            h = new Handler(3,3);
            h.Init(this);
            ControlPanel.Log($"Starts to Get {InputBox.Name} Msg");


            Handl = MessageList.Parent.RegisterStructureChangedEvent(FlaUI.Core.Definitions.TreeScope.Subtree, (e, p, o) =>
            {
                ListBoxItem msg = MessageList.Items.Last().AsListBoxItem();
                fButton user = msg.FindFirstChild().FindFirstChild(c => c.ByControlType(ControlType.Button)).AsButton();
                if (string.IsNullOrEmpty(user.Name))
                {
                    ControlPanel.Log($"(!)Empty Name Message: {msg.Name} Returned");
                    return;
                }



                if (msg.Name == LastMessage && user.Name == LastSpeaker && !msg.Name.StartsWith($"@{selfUserName} ."))
                {

                    return;
                }
                else
                {
                    ControlPanel.Log($"{DateTime.Now.ToShortTimeString()} {user.Name} {msg.Name}");
                    LastSpeaker = user.Name;
                    LastMessage = msg.Name;
                    LastTimeStamp = DateTime.Now.Ticks;
                    h.AcceptMessage(msg.Name, user.Name);
                }
            });
        }catch (Exception ex)
        {
            throw ex;
        }
    }

    public void AddText(string msg, bool startWithAt, string usr)
    {

        Thread t = new Thread(() => { Clipboard.SetText(startWithAt ? $"@{usr} {msg}" : msg); });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        InputBox.FocusNative();
        Thread.Sleep(100);
        Keyboard.TypeSimultaneously([FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V]);
        Thread.Sleep(500);
    }
    public void AddImage(Image img)
    {
        Thread t = new Thread(() =>
        {
            Clipboard.SetImage(img);
        }); 
        t.SetApartmentState (ApartmentState.STA);
        t.Start();
        t.Join();
        
        img.Dispose();
        InputBox.FocusNative();
        Thread.Sleep(100);
        Keyboard.TypeSimultaneously([VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V]);
        Thread.Sleep(100);
    }
    public void Send(bool DoubleTry=false)
    {
        ButtonSend.Click();

        if (!DoubleTry) { return; }
        Thread.Sleep(500);
        ButtonSend.Click();
    }
    public void SendText(string msg, bool startWithAt, string user)
    {
        AddText(msg, startWithAt, user);
        Send(false);
    }
    public void SendMS(MessageSequence ms)
    {
        foreach (var x in ms.elem)
        {
            switch (x.Item1)
            {
                case ReturnTypes.STRING:
                    AddText(x.Item2 as string, false, "");
                    break;
                case ReturnTypes.IMAGE:
                    AddImage(x.Item2 as Image);
                    break;
            }
           
        }
        Send();
        ms.elem.Clear();
    }

}
