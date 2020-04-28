using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormTraceSocket : Form
    {
        public static FormTraceSocket frmTraceSocket = null;
        public static int UIThreadID = 0;
        public static int PollThreadID = 0;
        public static long StatusReport = 0;
        public static long StatusReportReply = 0;
        public static long StatusRequest = 0;
        public static long StatusReply = 0;
        public static long SocketInQueue = 0;
        public static long SocketOutQueue = 0;

        public FormTraceSocket()
        {
            InitializeComponent();
        }

        private void FormTraceSocket_Load(object sender, EventArgs e)
        {
            UIThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtReport.Text = StatusReport.ToString();
            txtReportReply.Text = StatusReportReply.ToString();
            txtRequest.Text = StatusRequest.ToString();
            txtReply.Text = StatusReply.ToString();
            txtInQueue.Text = SocketInQueue.ToString();
            txtOutQueue.Text = SocketOutQueue.ToString();
            txtUIThreadID.Text = UIThreadID.ToString();
            txtPollThreadID.Text = PollThreadID.ToString();
        }
    }
}
