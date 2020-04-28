using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    // 表單模式
    public enum FormMode
    {
        /// <summary> 
        /// 一般模式
        /// </summary>
        Normal,
        /// <summary> 
        /// 新增模式
        /// </summary>
        AddNew,
        /// <summary> 
        /// 修改模式
        /// </summary>
        Modify
    }

    public partial class FormBase : Form
    {
        protected FormMode _frmMode = FormMode.Normal;
       
        protected FormMode FormMode
        {
            get { return _frmMode; }
            set { _frmMode = value; }
        }

        public FormBase()
        {
            InitializeComponent();
            spcBase.SplitterDistance = 0;
            
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED (等同 cp.ExStyle = cp.ExStyle | 0x02000000)
                return cp;
            }
        }

        #region ConfirmPassword委派
        private delegate DialogResult InvokeDelegatePassword(Form parent, string msgCaption);
        #endregion

        #region 確認密碼
        public DialogResult ConfirmPassword(Form parent, string msgCaption)
        {
            InvokeDelegatePassword d = new InvokeDelegatePassword(ConfirmPassword);
            FormPasswordConfirm frm = new FormPasswordConfirm() { TopMost = true };
            frm.lblCaption.Text = msgCaption;

            if (parent.InvokeRequired)
            {
                return (DialogResult)parent.Invoke(d,new object[]{parent ,msgCaption});
            }

            DialogResult _result = frm.ShowDialog(parent);

            frm.Dispose();

            return _result;
        }
        #endregion

        #region ShowMessage

        public DialogResult QuectionMessage(Form parent, string msgCaption, string message)
        {
            try
            {
                FormQuectionMessage _frm = new FormQuectionMessage(msgCaption, message){ TopMost = true };

                DialogResult _reuslt = _frm.ShowDialog(parent);
                
                _frm.Dispose();

                return _reuslt;
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return System.Windows.Forms.DialogResult.No;
            }  
        }

        public void ShowMessage(Form parent, string msgCaption, string messageCode, string message, MessageBoxIcon showIcon, string stackTrace = "")
        {
            try
            {
                eOPIMessageType _opiType = eOPIMessageType.Information;

                switch (showIcon)
                {
                    case MessageBoxIcon.Error:
                        _opiType = eOPIMessageType.Error;
                        break;

                    case MessageBoxIcon.Warning:

                        _opiType = eOPIMessageType.Warning;
                        break;

                    default:
                        break;
                }

                OPIInfo.Q_OPIMessage.Enqueue(new OPIMessage(msgCaption, messageCode, message, _opiType));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }            
        }

        public void ShowMessage(Form parent, string msgCaption, Exception ex, MessageBoxIcon showIcon)
        {
            OPIInfo.Q_OPIMessage.Enqueue(new OPIMessage(msgCaption, ex));
        }

        public void ShowMessage(Form parent, string msgCaption, UniAuto.UniBCS.OpiSpec.Return OpiSpecReturn, MessageBoxIcon showIcon)
        {
            OPIInfo.Q_OPIMessage.Enqueue(new OPIMessage(msgCaption, OpiSpecReturn.RETURNCODE, OpiSpecReturn.RETURNMESSAGE));
        }

        #endregion

        #region 同步socket

        /// <summary>
        /// 同步socket
        /// </summary>
        /// <param name="trxid">Transaxtion ID</param>
        /// <param name="msgName">Message Name</param>
        /// <param name="xml">Trx Xml</param>
        /// <param name="SocketResponseTime">同步等待時間(毫秒)</param>
        /// <returns>MessageResponse</returns>
        public MessageResponse SendRequestResponse(string trxid, string msgName, string xml, int SocketResponseTime)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                MessageRequest _req = new MessageRequest();
                _req.TimeStamp = DateTime.Now;
                _req.Xml = xml;
                _req.TrxMsgName = msgName;
                _req.TrxId = trxid;
                _req.SessionId = FormMainMDI.G_OPIAp.SessionID;
                _req.RelationKey = trxid;

                if (SocketResponseTime == 0) SocketResponseTime = FormMainMDI.G_OPIAp.SocketResponseTime;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();//開始記時

                MessageResponse _rsp = FormMainMDI.SocketDriver.RequestRsponse(_req, SocketResponseTime);

                stopWatch.Stop();//結束記時

                TimeSpan ts = stopWatch.Elapsed;

                NLogManager.Logger.LogTrxWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("The elapsed time of transaction [{0}-({1})] is {2} Milliseconds", msgName, trxid, ts.TotalMilliseconds.ToString()));

                if (!_rsp.RetCode.Equals(0))
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", _rsp.RetMsg, MessageBoxIcon.Error);
                    this.Cursor = Cursors.Default;
                    return null;
                }
                else
                {
                    UniAuto.UniBCS.OpiSpec.Message bcs_message = Spec.CheckXMLFormat(_rsp.Xml);

                    //if (!Program.CheckBodyLineName(bcs_message.GetBody()))
                    string _msg = UniTools.CheckBodyLineName(bcs_message.GetBody());
                    if (_msg != string.Empty)
                    {
                        ShowMessage(this, bcs_message.HEADER.MESSAGENAME, "", _msg, MessageBoxIcon.Error);
                        return null;
                    }

                    if (!string.IsNullOrEmpty(bcs_message.RETURN.RETURNCODE) && bcs_message.RETURN.RETURNCODE != FormMainMDI.G_OPIAp.ReturnCodeSuccess)
                    {
                        ShowMessage(this, bcs_message.HEADER.MESSAGENAME, bcs_message.RETURN, MessageBoxIcon.Error);
                        return null;
                    }
                }

                return _rsp;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return null;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion


        public virtual void CheckChangeSave()
        {
 
        }
    }        
}
