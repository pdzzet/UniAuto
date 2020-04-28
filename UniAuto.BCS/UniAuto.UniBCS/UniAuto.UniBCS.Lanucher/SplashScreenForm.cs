using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniAuto.UniBCS.Lanucher
{
    public delegate void UpdateLableTextEventHandler(string text);
    public partial class SplashScreenForm : Form
    {
        static SplashScreenForm splashScreen;

        Bitmap bitmap;

        public static SplashScreenForm SplashScreen
        {
            get
            {
                return splashScreen;
            }
            set
            {
                splashScreen = value;
            }
        }

        public SplashScreenForm()
        {
            this.InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            this.timer1.Enabled = true;
            this.label1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //Stream must be kept open for the lifetime of the bitmap
            //bitmap = new Bitmap(@"..\Image\SplashScreen.jpg");
            //this.ClientSize = new Size(bitmap.Width, bitmap.Height);//bitmap.Size;
            //using (Font font = new Font("Sans Serif", 24f))
            //{
            //    using (Graphics g = Graphics.FromImage(bitmap))
            //    {
            //        g.DrawString("Initial Workbench...", font, Brushes.Black, 100, 142);
            //    }
            //}
            //BackgroundImage = bitmap;
        }

        public static void ShowSplashScreen()
        {
            splashScreen = new SplashScreenForm();
            splashScreen.Show();
            splashScreen.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
            base.Dispose(disposing);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.label1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.Refresh();
        }

        private void SplashScreenForm_Load(object sender, EventArgs e)
        {
            
        }

        public void UpdateServerName(string msg)
        {
            try
            {
                label2.Text = string.Format(msg);
                this.Refresh();

            }
            catch(System.Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        public void UpdateInitInformation(string text)
        {
            try
            {
                //this.label7.Text += "\n";
                //this.label7.Text += text;
                if (this.transparentListBox1.Items.Count >5)
                {
                    this.transparentListBox1.Items.RemoveAt(0);
                }
                this.transparentListBox1.Items.Add(text);

                this.Refresh();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

       
    }
}
