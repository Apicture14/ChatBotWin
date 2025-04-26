namespace ChatBotWin
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            RT_Log = new RichTextBox();
            Btn_Start = new Button();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // RT_Log
            // 
            RT_Log.Location = new Point(0, 0);
            RT_Log.Name = "RT_Log";
            RT_Log.Size = new Size(1042, 470);
            RT_Log.TabIndex = 0;
            RT_Log.Text = "";
            // 
            // Btn_Start
            // 
            Btn_Start.Location = new Point(12, 476);
            Btn_Start.Name = "Btn_Start";
            Btn_Start.Size = new Size(131, 40);
            Btn_Start.TabIndex = 1;
            Btn_Start.Text = "Start";
            Btn_Start.UseVisualStyleBackColor = true;
            Btn_Start.Click += Btn_Start_Click;
            // 
            // button1
            // 
            button1.Location = new Point(409, 476);
            button1.Name = "button1";
            button1.Size = new Size(131, 40);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(674, 476);
            button2.Name = "button2";
            button2.Size = new Size(131, 40);
            button2.TabIndex = 3;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1044, 589);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(Btn_Start);
            Controls.Add(RT_Log);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox RT_Log;
        private Button Btn_Start;
        private Button button1;
        private Button button2;
    }
}
