using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormMaterialStatus : FormBase
    {
        string[] Material_POL = {"POL_1","RWK"};
        //string[] Material_CELL = { "ODF", "PIL"};
        //string[] Material_CF = { "FCMPH_TYPE1", "FCSPH_TYPE1", "FCRPH_TYPE1", "FCGPH_TYPE1", "FCBPH_TYPE1", "FCOPH_TYPE1" };
        //string[] Material_Comm = { "PCS", "CUT_1", "RWT", "PHL_EDGEEXP", "PHL_TITLE" };

        public FormMaterialStatus()
        {
            InitializeComponent();
        }

        private void FormMaterialStatus_Load(object sender, EventArgs e)
        {
            try
            {
                grbMaterial_CF.Dock = DockStyle.Fill;
                grbMaterial_CELL.Dock = DockStyle.Fill;
                grbMaterial_POL.Dock = DockStyle.Fill;
                grbMaterial_Comm.Dock = DockStyle.Fill;

                switch (FormMainMDI.G_OPIAp.CurLine.FabType)
                {
                    case "CF":
                        grbMaterial_CF.Visible = true; //grbMaterial_CF.Visible = (Array.IndexOf(Material_CF, FormMainMDI.G_OPIAp.CurLine.LineType) != -1 ? true : false);
                        break;
                    case "ARRAY":
                        grbMaterial_Comm.Visible = true;  //grbMaterial_Comm.Visible = (Array.IndexOf(Material_Comm, FormMainMDI.G_OPIAp.CurLine.LineType) != -1 ? true : false);
                        break;
                    case "CELL":
                        if (Array.IndexOf(Material_POL, FormMainMDI.G_OPIAp.CurLine.LineType) >= 0) grbMaterial_POL.Visible = true;
                        else grbMaterial_CELL.Visible = true ;  //grbMaterial_CELL.Visible = (Array.IndexOf(Material_CELL, FormMainMDI.G_OPIAp.CurLine.LineType) != -1 ? true : false); 
                        break;
                    default :
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                string _xml = string.Empty;
                string _err = string.Empty;
                string _respXml = string.Empty;

                #region Material Status Information

                switch (FormMainMDI.G_OPIAp.CurLine.FabType)
                {
                    case "CF":

                        #region CF Photo Line PR (Coater) / Mask (Aligner)

                        #region CFMaterialStatusRequest
                        CFMaterialStatusRequest _cfMaterialStatusRequest = new CFMaterialStatusRequest();
                        _cfMaterialStatusRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _cfMaterialStatusRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                        _xml = _cfMaterialStatusRequest.WriteToXml();

                        MessageResponse _cfMaterialStatusResp = this.SendRequestResponse(_cfMaterialStatusRequest.HEADER.TRANSACTIONID, _cfMaterialStatusRequest.HEADER.MESSAGENAME, _cfMaterialStatusRequest.WriteToXml(), FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                        if (_cfMaterialStatusResp == null) return;

                        #endregion

                        #region CFMaterialStatusReply

                        _respXml = _cfMaterialStatusResp.Xml;

                        CFMaterialStatusReply _cfMaterialStatusReply = (CFMaterialStatusReply)Spec.CheckXMLFormat(_respXml);

                        #region Update Data
                        RefreshDataGridView(_cfMaterialStatusReply);
                        #endregion

                        #endregion

                        break;

                        #endregion
                        
                    case "ARRAY":
                        
                        #region Common Material

                        #region MaterialStatusReportRequest
                        MaterialStatusReportRequest _materialStatusRequest = new MaterialStatusReportRequest();
                        _materialStatusRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _materialStatusRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                        _xml = _materialStatusRequest.WriteToXml();

                        MessageResponse _materialStatusResp = this.SendRequestResponse(_materialStatusRequest.HEADER.TRANSACTIONID, _materialStatusRequest.HEADER.MESSAGENAME, _materialStatusRequest.WriteToXml(), FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                        if (_materialStatusResp == null) return;

                        #endregion

                        #region MaterialStatusReply

                        _respXml = _materialStatusResp.Xml;

                        MaterialStatusReportReply _materialStatusReply = (MaterialStatusReportReply)Spec.CheckXMLFormat(_respXml);

                        #region Update Data
                        RefreshDataGridView(_materialStatusReply);
                        #endregion

                        #endregion                    

                        break;

                        #endregion
                        
                    case "CELL":

                        #region Cell Material Status

                        if (Array.IndexOf(Material_POL, FormMainMDI.G_OPIAp.CurLine.LineType) >= 0)
                        {
                            #region Cell POL_1 Line Cartridge (POL_1)

                            #region POLMaterialStatusRequest
                            POLMaterialStatusRequest _POLMaterialStatusRequest = new POLMaterialStatusRequest();
                            _POLMaterialStatusRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                            _POLMaterialStatusRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                            _xml = _POLMaterialStatusRequest.WriteToXml();

                            MessageResponse _POL_1MaterialStatusResp = this.SendRequestResponse(_POLMaterialStatusRequest.HEADER.TRANSACTIONID, _POLMaterialStatusRequest.HEADER.MESSAGENAME, _POLMaterialStatusRequest.WriteToXml(), FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                            if (_POL_1MaterialStatusResp == null) return;
                            #endregion

                            #region POL MaterialStatusReply

                            _respXml = _POL_1MaterialStatusResp.Xml;

                            POLMaterialStatusReply _POL_1MaterialStatusReply = (POLMaterialStatusReply)Spec.CheckXMLFormat(_respXml);

                            #region Update Data
                            RefreshDataGridView(_POL_1MaterialStatusReply);
                            #endregion

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Cell Line common item -- PI Line PIC, ODF Line SDP & LCD & SUV

                            #region CellMaterialStatusRequest
                            CellMaterialStatusRequest _cellMaterialStatusRequest = new CellMaterialStatusRequest();
                            _cellMaterialStatusRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                            _cellMaterialStatusRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                            _xml = _cellMaterialStatusRequest.WriteToXml();

                            MessageResponse _pipMaterialStatusResp = this.SendRequestResponse(_cellMaterialStatusRequest.HEADER.TRANSACTIONID, _cellMaterialStatusRequest.HEADER.MESSAGENAME, _cellMaterialStatusRequest.WriteToXml(), FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                            if (_pipMaterialStatusResp == null) return;

                            #endregion

                            #region PIPMaterialStatusReply

                            _respXml = _pipMaterialStatusResp.Xml;

                            CellMaterialStatusReply _cellMaterialStatusReply = (CellMaterialStatusReply)Spec.CheckXMLFormat(_respXml);

                            #region Update Data
                            RefreshDataGridView(_cellMaterialStatusReply);
                            #endregion

                            #endregion

                            #endregion
                        }

                        break;

                        #endregion

                    default:
                        break;
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private string GetMaterialStatus(string MaterialStatus)
        {
            switch (MaterialStatus)
            {
                case "1": return "1：Mount ";
                case "2": return "2：Dismount";
                case "3": return "3：In-Use";
                case "4": return "4：Prepare";
                case "5": return "5：Empty";
                default: return string.Format("{0}:Unknown", MaterialStatus);
            }
        }

        private string GetCompleteStatus(string CompleteStatus)
        {
            switch (CompleteStatus)
            {
                case "1": return "1：Normal End ";
                case "2": return "2：Abnormal End";
                default: return string.Format("{0}:Unknown", CompleteStatus);
            }
        }

        private void RefreshDataGridView(CFMaterialStatusReply MaterialStatusReply)
        {
            try
            {
                string _localName = string.Empty;
                string _err = string.Empty;
            
                dgvData_CF.Rows.Clear();

                foreach (CFMaterialStatusReply.EQUIPMENTc _reply in MaterialStatusReply.BODY.EQUIPMENTLIST)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_reply.EQUIPMENTNO))
                    {
                        _localName = string.Format("{0}-{1}-{2}", FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeNo,
                                                                    FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeID,
                                                                    FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeName);

                        foreach (CFMaterialStatusReply.MATERIALc _material in _reply.MATERIALLIST)
                        {
                            //local , unit no, operator id, material status, material value, slot no, material id
                            dgvData_CF.Rows.Add(_localName, _material.UNITNO, _material.OPERATIONERID, GetMaterialStatus(_material.MATERIALSTATUS), _material.MATERIALVALUE, _material.SLOTNO, _material.MATERIALID);
                        }
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text , "", string.Format("Local [{0}] is not exist", _reply.EQUIPMENTNO), MessageBoxIcon.Error);
                    }
                }  
            
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView(CellMaterialStatusReply MaterialStatusReply)
        {
            try
            {
                string _localName = string.Empty;
                string _err = string.Empty;

                dgvData_CELL.Rows.Clear();

                foreach (CellMaterialStatusReply.EQUIPMENTc _reply in MaterialStatusReply.BODY.EQUIPMENTLIST)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_reply.EQUIPMENTNO))
                    {
                        _localName = string.Format("{0}-{1}-{2}", FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeNo,
                                                                    FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeID,
                                                                    FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeName);

                        foreach (CellMaterialStatusReply.MATERIALc _material in _reply.MATERIALLIST)
                        {
                            //local , unit no, recipe ID, material ID, material Status, position, weight, operator id
                            dgvData_CELL.Rows.Add(_localName, _material.UNITNO, _material.RECIPEID, _material.MATERIALID, GetMaterialStatus(_material.MATERIALSTATUS), _material.UV_MASK_USE_COUNT, _material.OPERATIONERID);
                        }
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Local [{0}] is not exist", _reply.EQUIPMENTNO), MessageBoxIcon.Error);
                    }
                }
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
       
        private void RefreshDataGridView(POLMaterialStatusReply MaterialStatusReply)
        {
            try
            {
                string _localName = string.Empty;
                string _err = string.Empty;

                dgvData_POL.Rows.Clear();

                foreach (POLMaterialStatusReply.EQUIPMENTc _reply in MaterialStatusReply.BODY.EQUIPMENTLIST)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_reply.EQUIPMENTNO))
                    {
                        _localName = string.Format("{0}-{1}-{2}", FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeNo,
                                                                    FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeID,
                                                                    FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeName);

                        foreach (POLMaterialStatusReply.MATERIALc _material in _reply.MATERIALLIST)
                        {
                            //local , unit no,port id,Recipe ID,material ID,  material Status, Lot No#01, Lot ID#01,Lot No#01 Count, Lot No#02,Lot ID#02,Lot No#02 Count,
                            //Lot No#03,Lot ID#03,Lot No#03 Count,Lot No#04,Lot ID#04,Lot No#04 Count,Lot No#05,Lot ID#05,Lot No#05 Count ,operator id 
                            dgvData_POL.Rows.Add(_localName, _material.UNITNO,_material.PORTID,_material.RECIPEID, _material.MATERIALID, GetMaterialStatus(_material.MATERIAL_STATUS),
                                _material.LOTNO01, _material.LOTID01, _material.COUNT01, _material.LOTNO02, _material.LOTID01, _material.COUNT02, _material.LOTNO03, _material.LOTID03, _material.COUNT03,
                                _material.LOTNO04, _material.LOTID04, _material.COUNT04, _material.LOTNO05, _material.LOTID05, _material.COUNT05, _material.OPERATIONERID);
                        }
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text , "", string.Format("Local [{0}] is not exist", _reply.EQUIPMENTNO), MessageBoxIcon.Error);
                    }
                }
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void RefreshDataGridView(MaterialStatusReportReply MaterialStatusReply)
        {
            try
            {
                string _localName = string.Empty;
                string _err = string.Empty;

                dgvData_Comm.Rows.Clear();

                foreach (MaterialStatusReportReply.EQUIPMENTc _reply in MaterialStatusReply.BODY.EQUIPMENTLIST)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_reply.EQUIPMENTNO))
                    {
                        _localName = string.Format("{0}-{1}-{2}", FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeNo,
                                                                  FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeID,
                                                                  FormMainMDI.G_OPIAp.Dic_Node[_reply.EQUIPMENTNO].NodeName);

                        foreach (MaterialStatusReportReply.MATERIALc _material in _reply.MATERIALLIST)
                        {
                            dgvData_Comm.Rows.Add(_localName,_material.MATERIALTYPE, _material.MATERIALNAME, _material.SLOTNO, _material.MATERIALSTATUS);
                        }
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Local [{0}] is not exist", _reply.EQUIPMENTNO), MessageBoxIcon.Error);
                    }
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
