using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRealJobCount : FormBase
    {
        public FormRealJobCount()
        {
            InitializeComponent();
        }

        private void FormRealJobCount_Load(object sender, EventArgs e)
        {
            try
            {
                dgvData.Rows.Clear();
                int _totalCnt_All = 0;
                int _totalCnt_Node = 0;
                int _tftCnt = 0;
                int _cfCnt = 0;
                int _dummyCnt = 0;
                int _throughDummy = 0;
                int _thicknessDummy=0;
                int _uvMask = 0;
                int _unassembledTFTJobCount = 0;//sy add 20160826
                int _iTODummyJobCount = 0;//sy add 20160826
                int _nIPDummyJobCount = 0;//sy add 20160826
                int _metalOneDummyJobCount = 0;//sy add 20160826
                
                #region Load Node Information
                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {

                    _tftCnt = _tftCnt + _node.TFTJobCount;
                    _cfCnt  = _cfCnt + _node.CFJobCount;
                    _dummyCnt = _dummyCnt + _node.DummyJobCount;
                    _throughDummy = _throughDummy +_node.ThroughDummyJobCount ;
                    _thicknessDummy= _thicknessDummy + _node.ThicknessDummyJobCount;
                    _uvMask = _uvMask + _node.UVMASKJobCount;
                    _unassembledTFTJobCount = _unassembledTFTJobCount + _node.UnassembledTFTJobCount;//sy add 20160826
                    _iTODummyJobCount = _iTODummyJobCount + _node.ITODummyJobCount;//sy add 20160826
                    _nIPDummyJobCount = _nIPDummyJobCount + _node.NIPDummyJobCount;//sy add 20160826
                    _metalOneDummyJobCount = _metalOneDummyJobCount + _node.MetalOneDummyJobCount;//sy add 20160826
                    _totalCnt_Node = _node.TFTJobCount + _node.CFJobCount + _node.DummyJobCount + _node.ThroughDummyJobCount + _node.ThicknessDummyJobCount + _node.UVMASKJobCount
                        + _node.UnassembledTFTJobCount + _node.ITODummyJobCount + _node.NIPDummyJobCount + _node.MetalOneDummyJobCount;//sy add 20160826
                    _totalCnt_All = _totalCnt_All + _totalCnt_Node;

                    dgvData.Rows.Add(_node.NodeNo, string.Format("{0}-{1}", _node.NodeNo, _node.NodeID), _node.TFTJobCount, _node.CFJobCount, _node.DummyJobCount, _node.ThroughDummyJobCount, _node.ThicknessDummyJobCount,
                        _node.UnassembledTFTJobCount, _node.ITODummyJobCount, _node.NIPDummyJobCount, _node.MetalOneDummyJobCount, _node.UVMASKJobCount, _totalCnt_Node);//sy add 20160826
                }

                dgvData.Rows.Add("Total", "Total Count", _tftCnt, _cfCnt, _dummyCnt, _throughDummy, _thicknessDummy, _uvMask, _totalCnt_All);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                string _localNo = string.Empty;
                int _totalCnt_All = 0;
                int _totalCnt_Node = 0;
                int _tftCnt = 0;
                int _cfCnt = 0;
                int _dummyCnt = 0;
                int _throughDummy = 0;
                int _thicknessDummy = 0;
                int _uvMask = 0;
                int _unassembledTFTJobCount = 0;//sy add 20160826
                int _iTODummyJobCount = 0;//sy add 20160826
                int _nIPDummyJobCount = 0;//sy add 20160826
                int _metalOneDummyJobCount = 0;//sy add 20160826

                #region Load Node Information
                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    _tftCnt = _tftCnt + _node.TFTJobCount;
                    _cfCnt = _cfCnt + _node.CFJobCount;
                    _dummyCnt = _dummyCnt + _node.DummyJobCount;
                    _throughDummy = _throughDummy + _node.ThroughDummyJobCount;
                    _thicknessDummy = _thicknessDummy + _node.ThicknessDummyJobCount;
                    _uvMask = _uvMask + _node.UVMASKJobCount;
                    _unassembledTFTJobCount = _unassembledTFTJobCount + _node.UnassembledTFTJobCount;//sy add 20160826
                    _iTODummyJobCount = _iTODummyJobCount + _node.ITODummyJobCount;//sy add 20160826
                    _nIPDummyJobCount = _nIPDummyJobCount + _node.NIPDummyJobCount;//sy add 20160826
                    _metalOneDummyJobCount = _metalOneDummyJobCount + _node.MetalOneDummyJobCount;//sy add 20160826
                    _totalCnt_Node = _node.TFTJobCount + _node.CFJobCount + _node.DummyJobCount + _node.ThroughDummyJobCount + _node.ThicknessDummyJobCount + _node.UVMASKJobCount
                        + _node.UnassembledTFTJobCount + _node.ITODummyJobCount + _node.NIPDummyJobCount + _node.MetalOneDummyJobCount;//sy add 20160826
                    _totalCnt_All = _totalCnt_All + _totalCnt_Node;

                    foreach (DataGridViewRow _row in dgvData.Rows)
                    {
                        if (_row.Cells[colLocalNo.Name].Value.ToString() == _node.NodeNo)
                        {
                            _row.Cells[colTFTCount.Name].Value = _node.TFTJobCount.ToString();
                            _row.Cells[colCFCount.Name].Value = _node.CFJobCount.ToString();
                            _row.Cells[colDummyCount.Name].Value = _node.DummyJobCount.ToString();    
                            _row.Cells[colThroughCount.Name].Value = _node.ThroughDummyJobCount.ToString(); 
                            _row.Cells[colThicknessCount.Name].Value = _node.ThicknessDummyJobCount.ToString();
                            _row.Cells[colUVMaskCount.Name].Value = _node.UVMASKJobCount.ToString();
                            _row.Cells[colUnassembledTFTCount.Name].Value = _node.UnassembledTFTJobCount.ToString();//sy add 20160826
                            _row.Cells[colITOCount.Name].Value = _node.ITODummyJobCount.ToString();//sy add 20160826
                            _row.Cells[colNIPCount.Name].Value = _node.NIPDummyJobCount.ToString();//sy add 20160826
                            _row.Cells[colMetalOneCount.Name].Value = _node.MetalOneDummyJobCount.ToString(); //sy add 20160826
                            _row.Cells[colTotalCount.Name].Value = _totalCnt_Node.ToString(); 

                            break;
                        }
                    }                    
                }

                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    if (_row.Cells[colLocalNo.Name].Value.ToString() == "Total")
                    {
                        _row.Cells[colTFTCount.Name].Value = _tftCnt.ToString();
                        _row.Cells[colCFCount.Name].Value = _cfCnt.ToString();
                        _row.Cells[colDummyCount.Name].Value = _dummyCnt.ToString();
                        _row.Cells[colThroughCount.Name].Value = _throughDummy.ToString();
                        _row.Cells[colThicknessCount.Name].Value = _thicknessDummy.ToString();
                        _row.Cells[colUVMaskCount.Name].Value = _uvMask.ToString();
                        _row.Cells[colUnassembledTFTCount.Name].Value = _unassembledTFTJobCount.ToString();//sy add 20160826
                        _row.Cells[colITOCount.Name].Value = _iTODummyJobCount.ToString();//sy add 20160826
                        _row.Cells[colNIPCount.Name].Value = _nIPDummyJobCount.ToString();//sy add 20160826
                        _row.Cells[colMetalOneCount.Name].Value = _metalOneDummyJobCount.ToString();//sy add 20160826
                        _row.Cells[colTotalCount.Name].Value = _totalCnt_All.ToString();

                        break;
                    }
                } 
                #endregion
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
                string _err = string.Empty;

                AllEquipmentStatusRequest _trx = new AllEquipmentStatusRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                string _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text, "", "Reload Equipment Information  Send to BC Success !", MessageBoxIcon.Information);  
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }      
    }
}
