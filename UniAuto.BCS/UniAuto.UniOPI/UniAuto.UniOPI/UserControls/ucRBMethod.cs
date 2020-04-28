using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucRBMethod : UserControl
    {
        public ucRBMethod()
        {
            InitializeComponent();
        }
       
        List<SBRM_ROBOT_METHOD_DEF> ObjEdit=new List<SBRM_ROBOT_METHOD_DEF>();//Method Def 的集合 
        string ItemID = string.Empty;
        string FunKey = string.Empty;

        string Desciption = string.Empty;

        //外部呼叫此UserControls時可以做事件處理
        public delegate void CboClickDelegate(object sender,EventArgs e);
        public event CboClickDelegate Select_cbo;

        //取得ObjectName
        public string GetObjectName()
        {
            if (cboObjectName.SelectedIndex == -1) return string.Empty;
            else
                return cboObjectName.SelectedItem.ToString();
        }

        //取得MethodName
        public string GetMethodName()
        {
            if (cboMethodName.SelectedIndex == -1) return string.Empty;
            else
                return ItemID;//ItemID = MethodName
        }

        public string GetFunName()
        {
            if (cboMethodName.SelectedIndex == -1) return string.Empty;
            else return FunKey;
        }

        //取得Description
        public string GetDescription()
        {
            if (string.IsNullOrEmpty(Desciption)) return string.Empty;
            else
                return Desciption;
        }

        //清楚Objection、MethodName選項
        public void ClearMethod()
        {            
            cboObjectName.Text = "";

            cboMethodName.DataSource = null;
        }

        //選取指定的ObjectName
        public void SetObjectName(string Name)
        {
            if (cboObjectName.DataSource != null)
                cboObjectName.SelectedItem = Name;
        }

        //選取指定的MethodName
        public void SetMethName(string Name)
        {
            if (cboMethodName.DataSource != null)
            {
                foreach (string _item in cboMethodName.Items)
                {
                    if (_item.Contains(Name) == true)
                    {
                        cboMethodName.SelectedItem = _item;
                        break;
                    }
                }
            }
        }

        //取得特定MethodType的MethodDEF資料，並設定ObjectName資料
        public void InitialRBMethod(string MethedType)
        {
            try
            {
                //取出特定MethodTYPE資料，並加以排序
                ObjEdit = ((from _obj in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_ROBOT_METHOD_DEF where _obj.METHODRULETYPE.Equals(MethedType) orderby _obj.OBJECTNAME, _obj.METHODNAME select _obj).Distinct().ToList());
                List<string> _objName = new List<string>();
                //第一筆為空，並將ObjEdit中的ObjectName取出來
                _objName.Add("");
                _objName.AddRange( (from _obj in ObjEdit  orderby _obj.OBJECTNAME select _obj.OBJECTNAME).Distinct().ToList());
                cboObjectName.DataSource = _objName;
                cboObjectName.SelectedIndex= 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //當選取ObejcetName的時候，將MethodName資料撈出來(FuncKey+MethodName)
        public void cboObjectName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboObjectName.SelectedItem.ToString() != string.Empty)
                {
                    List<string> _methodDef = new List<string>();
                    //第一筆為空，將ObjEdit內的MethodName、FuncKey資料抓出來
                    _methodDef.Add("");
                    var _method = (from _Method in ObjEdit orderby _Method.METHODNAME where _Method.OBJECTNAME.Equals(cboObjectName.SelectedItem.ToString()) select _Method).OrderBy(r=>r.FUNCKEY).Distinct().ToList();

                    foreach (var _funkeyJoinMethodName in _method)
                    {
                        _methodDef.Add("[" + _funkeyJoinMethodName.FUNCKEY + "]" + _funkeyJoinMethodName.METHODNAME);
                    }

                    cboMethodName.DataSource = _methodDef;
                    cboMethodName.SelectedIndex = 0;
                }
                else
                {
                    cboMethodName.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //當選取MethodName 將Description及ItemID值設定出來
        public void cboMethodName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboMethodName.SelectedValue == null) return;

                if (cboMethodName.SelectedItem.ToString() != string.Empty)
                {
                    string _cboItem = cboMethodName.SelectedItem.ToString();

                    //因為combobox的MethodName名稱有修改，所以要再把MethodName名稱撈出來
                    ItemID = _cboItem.Split(']')[1].ToString();
                    FunKey = _cboItem.Substring(1, 6);
                    var getContent = (from _obj in ObjEdit where _obj.METHODNAME.Equals(ItemID) && _obj.OBJECTNAME.Equals(cboObjectName.SelectedItem.ToString()) select _obj.DESCRIPTION).ToList();
                    Desciption = getContent[0].ToString();//只會有一筆資料，若這有兩筆以上表示DB有問題
                }
                else
                {
                    Desciption = string.Empty;
                    ItemID = string.Empty;
                    FunKey = string.Empty;
                }
                Select_cbo(sender, e);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SetEnable(bool IsEnable)
        {
            cboObjectName.Enabled = IsEnable;
            cboMethodName.Enabled = IsEnable;
        }
    }
}
