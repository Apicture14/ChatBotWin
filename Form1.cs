

using ChatBot;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using System.Runtime.InteropServices;

namespace ChatBotWin
{
    public partial class Form1 : Form
    {
        Bot bot = new Bot();
        Consts.MessageSequence ms = new Consts.MessageSequence();
        public Form1()
        {
            InitializeComponent();
        }

        public void Log(string text)
        {
            this.Invoke(delegate { this.RT_Log.AppendText(text + "\n"); });
        }

        private void Btn_Start_Click(object sender, EventArgs e)
        {
            bot.Init(this);
            bot.Start();
            Btn_Start.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            User_Control.SavePermission();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Pngs|*.png|Jpgs|*.jpg";



            if (o.ShowDialog() == DialogResult.OK)
            {
                ms.Add(Consts.ReturnType.IMAGE, Image.FromFile(o.FileName));

            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var x in ms.elem)
            {
                Clipboard.SetImage(x.Item2 as Image);
                if (bot != null)
                {
                    //bot.InputBox.DoubleClick();
                    bot.InputBox.FocusNative();
                    Thread.Sleep(100);
                    Keyboard.TypeSimultaneously([VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V]);
                    Thread.Sleep(100);
                }
            }
            ms.elem.Clear();
        }
    }
}
