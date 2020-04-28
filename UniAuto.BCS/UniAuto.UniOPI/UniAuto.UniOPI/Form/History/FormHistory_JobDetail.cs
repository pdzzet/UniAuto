using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormHistory_JobDetail : FormBase
    {
        DataGridViewRow RowData;
        public FormHistory_JobDetail(DataGridViewRow row)
        {
            InitializeComponent();

            RowData = row;

            
        }

        private void FormHistory_JobDetail_Load(object sender, EventArgs e)
        {
            try
            {
                int _offset = 0;
                int _len = 0;
                int _itemLen = 0;
                int _itemValue = 0;
                int _memoValue=0;
                string _memoDesc=string.Empty ;
                string _itemBinData = string.Empty;
                string _itemDesc = string.Empty;
                string _convertData = string.Empty;
                string _itemName = string.Empty;

                Dictionary<int,string> _dicItemList ;

                dgvData.Rows.Clear();

                txtLocalID.Text = RowData.Cells["NODEID"].Value.ToString();
                txtJobID.Text = RowData.Cells["JOBID"].Value.ToString();
                txtJobSeqNo.Text = RowData.Cells["JOBSEQNO"].Value.ToString();
                txtCstSeqNo.Text = RowData.Cells["CASSETTESEQNO"].Value.ToString();

                string _trackingData = RowData.Cells["TRACKINGDATA"].Value.ToString(); //UniTools.ReverseStr(RowData.Cells["TRACKINGDATA"].Value.ToString());
                string _EQPFlag = RowData.Cells["EQPFLAG"].Value.ToString();
                string _Inspjudgeddata = RowData.Cells["INSPJUDGEDDATA"].Value.ToString();
                 UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _selData = from job in _ctxBRM.SBRM_SUBJOBDATA
                              where job.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                              job.ITEMNAME == "TrackingData"
                              orderby job.SUBITEMLOFFSET
                              select job;

                foreach (SBRM_SUBJOBDATA _detail in _selData)
                {
                    int.TryParse(_detail.ITEMLENGTH.ToString(),out _itemLen);                   

                    if (_trackingData.Length < _itemLen ) _trackingData= _trackingData.PadRight(_itemLen,'0');

                    //取得item描述
                    _dicItemList = new Dictionary<int,string>();
                    string[] _tmp = _detail.MEMO.ToString().Split(',');
                    foreach (string _value in _tmp)
                    {
                        string[] _tmp2 = _value.Split(':');

                        if (_tmp2.Length < 2 ) continue ;

                        int.TryParse(_tmp2[0],out _memoValue);
                        _dicItemList.Add(_memoValue,_tmp2[1]);
                    }

                    _itemDesc = _detail.SUBITEMDESC.ToString();

                    if (int.TryParse(_detail.SUBITEMLOFFSET.ToString(),out _offset) == false ) continue ;
                    if (int.TryParse(_detail.SUBITEMLENGTH.ToString(), out _len) == false) continue;

                    _itemBinData = UniTools.ReverseStr(_trackingData.Substring(_offset, _len));
                    _itemValue = Convert.ToInt32(_itemBinData, 2);
                    _itemName = _detail.ITEMNAME;//add by sy.wu

                    if (_dicItemList.ContainsKey(_itemValue))
                    {
                        dgvData.Rows.Add(_itemDesc, string.Format("{0}:{1}", _itemValue, _dicItemList[_itemValue]), _itemName);
                    }
                    else
                    {
                        dgvData.Rows.Add(_itemDesc, _itemValue, _itemName);
                    }
                }

                #region   Inspjudgeddata  --add by sy.wu--
                var _selDataI = from job in _ctxBRM.SBRM_SUBJOBDATA
                           where job.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                           job.ITEMNAME == "Insp.JudgedData"
                           orderby job.ITEMNAME
                           select job;

                foreach (SBRM_SUBJOBDATA _detail in _selDataI)
                {
                    int.TryParse(_detail.ITEMLENGTH.ToString(), out _itemLen);



                    if (_Inspjudgeddata.Length < _itemLen) _Inspjudgeddata = _Inspjudgeddata.PadRight(_itemLen, '0');

                    //取得item描述
                    _dicItemList = new Dictionary<int, string>();
                    string[] _tmp = _detail.MEMO.ToString().Split(',');
                    foreach (string _value in _tmp)
                    {
                        string[] _tmp2 = _value.Split(':');

                        if (_tmp2.Length < 2) continue;

                        int.TryParse(_tmp2[0], out _memoValue);
                        _dicItemList.Add(_memoValue, _tmp2[1]);
                    }

                    _itemDesc = _detail.SUBITEMDESC.ToString();

                    if (int.TryParse(_detail.SUBITEMLOFFSET.ToString(), out _offset) == false) continue;
                    if (int.TryParse(_detail.SUBITEMLENGTH.ToString(), out _len) == false) continue;

                    _itemBinData = UniTools.ReverseStr(_Inspjudgeddata.Substring(_offset, _len));
                    _itemValue = Convert.ToInt32(_itemBinData, 2);
                    _itemName = _detail.ITEMNAME;
                    _itemValue.ToString();
                    if (_dicItemList.ContainsKey(_itemValue))
                    {
                        dgvData.Rows.Add(_itemDesc, string.Format("{0}:{1}", _itemValue, _dicItemList[_itemValue]), _itemName);
                    }
                    else
                    {
                        dgvData.Rows.Add(_itemDesc, _itemValue, _itemName);
                    }
                }
                #endregion

                #region   EQPFlag   --add by sy.wu--
                var _selDataE = from job in _ctxBRM.SBRM_SUBJOBDATA
                           where job.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                           job.ITEMNAME == "EQPFlag"
                           orderby job.ITEMNAME
                           select job;

                foreach (SBRM_SUBJOBDATA _detail in _selDataE)
                {
                    int.TryParse(_detail.ITEMLENGTH.ToString(), out _itemLen);



                    if (_EQPFlag.Length < _itemLen) _EQPFlag = _EQPFlag.PadRight(_itemLen, '0');

                    //取得item描述
                    _dicItemList = new Dictionary<int, string>();
                    string[] _tmp = _detail.MEMO.ToString().Split(',');
                    foreach (string _value in _tmp)
                    {
                        string[] _tmp2 = _value.Split(':');

                        if (_tmp2.Length < 2) continue;

                        int.TryParse(_tmp2[0], out _memoValue);
                        _dicItemList.Add(_memoValue, _tmp2[1]);
                    }

                    _itemDesc = _detail.SUBITEMDESC.ToString();

                    if (int.TryParse(_detail.SUBITEMLOFFSET.ToString(), out _offset) == false) continue;
                    if (int.TryParse(_detail.SUBITEMLENGTH.ToString(), out _len) == false) continue;

                    _itemBinData = UniTools.ReverseStr(_EQPFlag.Substring(_offset, _len));
                    _itemValue = Convert.ToInt32(_itemBinData, 2);
                    _itemName = _detail.ITEMNAME;

                    if (_dicItemList.ContainsKey(_itemValue))
                    {
                        dgvData.Rows.Add(_itemDesc, string.Format("{0}:{1}", _itemValue, _dicItemList[_itemValue]), _itemName);
                    }
                    else
                    {
                        dgvData.Rows.Add(_itemDesc, _itemValue, _itemName);
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

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
