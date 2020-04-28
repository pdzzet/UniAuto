using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormBCControlLastCommand : FormBase
    {

        string CommandType = string.Empty;

        public FormBCControlLastCommand(string commandType)
        {
            InitializeComponent();

            CommandType = commandType;

            switch (CommandType)
            {
                case "PROCESSPAUSE": 
                    lblCaption.Text = "Process Pause Last Command";
                    dgvData.Columns[colUnit.Name].Visible = true;
                    break ;
                case "TRANSFERSTOP": 
                    lblCaption.Text = "Transfer Stop Last Command";
                    dgvData.Columns[colUnit.Name].Visible = false;
                    break ;
                case "PROCESSSTOP": 
                    lblCaption.Text = "Process Stop Last Command";
                    dgvData.Columns[colUnit.Name].Visible = true;
                    break ;
                default :break ;
            }
        }

        private void FormBCControlLastCommand_Load(object sender, EventArgs e)
        {
            try
            {
               
                #region Send BCControlCommandInfoRequest

                BCControlCommandInfoRequest _trx = new BCControlCommandInfoRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.COMMANDTYPE = CommandType;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region BCControlCommandInfoReply

                string _respXml = _resp.Xml;

                BCControlCommandInfoReply _reply = (BCControlCommandInfoReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                SetData(_reply);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetData(BCControlCommandInfoReply Reply)
        {
            try
            {
                List<BCControlCommandInfoReply.ITEMc> _lstItems = Reply.BODY.ITEMLIST;

                dgvData.Rows.Clear();

                string _local = string.Empty;
                string _unit = string.Empty;
                string _unitKey = string.Empty ;
                string _command = string.Empty ;

                foreach (BCControlCommandInfoReply.ITEMc _item in _lstItems)
                {
                    _local = string.Empty;
                    _unit = string.Empty;
                    _command = string.Empty;

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_item.EQUIPMENTNO))
                        _local = string.Format("{0}-{1}", _item.EQUIPMENTNO, FormMainMDI.G_OPIAp.Dic_Node[_item.EQUIPMENTNO].NodeID);

                    switch (CommandType)
                    {
                        case "PROCESSPAUSE":
                            
                            #region Process Pause Command
                            if (_item.COMMAND =="1") _command = "1:Pause";
                            else if (_item.COMMAND =="2") _command = "2:Resume";
                            else _command = _item.COMMAND;

                            //NODENO(3) + UNITNO(2)
                            _unitKey = _item.EQUIPMENTNO.ToString().PadRight(3, ' ') + _item.UNITNO.PadLeft(2, '0');
                            if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(_unitKey))
                            {
                                _unit = string.Format("{0}-{1}", _item.UNITNO, FormMainMDI.G_OPIAp.Dic_Unit[_unitKey].UnitID);
                            }

                            break;
                            #endregion

                        case "TRANSFERSTOP":

                            #region Transfer Stop Command
                            if (_item.COMMAND =="1") _command = "1:Stop";
                            else if (_item.COMMAND =="2") _command = "2:Resume";
                            else _command = _item.COMMAND;

                            break;
                            #endregion

                        case "PROCESSSTOP":

                            #region Process Stop Command
                            if (_item.COMMAND =="1") _command = "1:Stop";
                            else if (_item.COMMAND =="2") _command = "2:Run";
                            else _command = _item.COMMAND;

                            //NODENO(3) + UNITNO(2)
                            _unitKey = _item.EQUIPMENTNO.ToString().PadRight(3, ' ') + _item.UNITNO.PadLeft(2, '0');
                            if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(_unitKey))
                            {
                                _unit = string.Format("{0}-{1}", _item.UNITNO, FormMainMDI.G_OPIAp.Dic_Unit[_unitKey].UnitID);
                            }
                            break ;
                            #endregion

                        default :
                            _command = string.Empty;
                            break;
                    }

                    dgvData.Rows.Add(_item.EQUIPMENTNO, _local, _item.UNITNO, _unit, _command);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
