using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
    public enum eRecipeCheckResult
    {
        OK,
        NG,
        CIMOFF,
        TIMEOUT,
        NOCHECK,
        MACHINENG,
        RECIPELENNG,
        NOMACHINE, //沒有這個機台
        ZERO //RECIPEID ='00'
    }

    public enum eRecipeCheckCommandType
    {
        BCS,
        MES,
        OPI,
    }

    public enum eRecipeCheckCommandState
    {
        WaitForEQ,
        WaitForMES,
        Finish,
    }

    //20150711 Add by Frank
    public enum eModifyFlag
    {
        Create = 1,
        Modify = 2,
        Delete = 3
    }
    /// <summary>
    /// Recipe Check Object
    /// </summary>
    public class RecipeCheckInfo
    {
        private string _eqpNo = string.Empty;
        private string _eqpName = string.Empty;
        private string _eqpID = string.Empty;

        public string EqpID
        {
            get { return _eqpID; }
            set { _eqpID = value; }
        }

        public string EqpName
        {
            get { return _eqpName; }
            set { _eqpName = value; }
        }
        private bool _isFinish;
        private bool _isSend;
        private string _recipeID;
        private string _recipeNo;
        private DateTime _createTime = DateTime.Now;
        private int _mode;//Auto Manual;
        private int _portNo;
        private string _trxId;
        private string _lineRecipeName;
        private string _mesVALICODEtext;
        private string _eventcomment;
        private string _recipeVersiom; //cc.kuang add for t3 20150701

        public string LineRecipeName
        {
            get { return _lineRecipeName; }
            set { _lineRecipeName = value; }
        }

        public string MESVALICODEText
        {
            get { return _mesVALICODEtext; }
            set { _mesVALICODEtext = value; }
        }

        private IDictionary<string, string> _parameters;

        public IDictionary<string, string> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }
        

        
        private eRecipeCheckResult _result=eRecipeCheckResult.NG;

        /// <summary>
        /// EQP No
        /// </summary>
        public string EQPNo
        {
            get { return _eqpNo; }
            set { _eqpNo = value; }
        }

        /// <summary>
        /// Write to EQP Flag
        /// </summary>
        public bool IsSend
        {
            get { return _isSend; }
            set { _isSend = value; }
        }
       

        public bool IsFinish
        {
            get { return _isFinish; }
            set { _isFinish = value; }
        }

        public string EventComment
        {
            get { return _eventcomment; }
            set { _eventcomment = value; }
        }

        public string RecipeID
        {
            get { return _recipeID; }
            set { _recipeID = value; }
        }

        public string RecipeNo
        {
            get { return _recipeNo; }
            set { _recipeNo = value; }
        }

        public DateTime CreateTime
        {
            get { return _createTime; }
            set { _createTime = value; }
        }

        public int Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        public int PortNo
        {
            get { return _portNo; }
            set { _portNo = value; }
        }
        public string TrxId
        {
            get { return _trxId; }
            set { _trxId = value; }
        }
        public string RecipeVersion
        {
            get { return _recipeVersiom; }
            set { _recipeVersiom = value; }
        }

        /// <summary>
        /// Recipe Check Result
        /// </summary>
        public eRecipeCheckResult Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public RecipeCheckInfo(string eqpNo, string eqpID, int mode, int portNo, string recipeID, string lineRecipeName, bool send = false, bool finish = false)
        {
            _eqpNo = eqpNo;
            _eqpID = eqpID;
            _recipeID = recipeID;
            _isSend = send;
            _isFinish = finish;
            _mode = mode;
            _portNo = portNo;
            _lineRecipeName = lineRecipeName;
            _parameters = new Dictionary<string, string>();
        }
        public RecipeCheckInfo(string eqpNo,int mode,int portNo,string recipeID,string lineRecipeName ,bool send=false,bool finish=false)
        {
            _eqpNo = eqpNo;
            _recipeID = recipeID;
            _isSend = send;
            _isFinish = finish;
            _mode = mode;
            _portNo = portNo;
            _lineRecipeName = lineRecipeName;
            _parameters=new Dictionary<string,string>();
        }
        public RecipeCheckInfo(string eqpNo, int mode, int portNo, string recipeID, bool send = false, bool finish = false)
        {
            _eqpNo = eqpNo;
            _recipeID = recipeID;
            _isSend = send;
            _isFinish = finish;
            _mode = mode;
            _portNo = portNo;
            _lineRecipeName = "";
            _parameters = new Dictionary<string, string>();
        }

        public RecipeCheckInfo()
        {
            _parameters = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            
            return string.Format("EquipmentNo={0},PortNO={1},Mode={2},RecipeID={3},Result={4},CreateTime={5}.",
                        EQPNo,PortNo,Mode,RecipeID,Result,CreateTime.ToString("yyyyMMddHHmmssfff"));
        }
    }

    public class RecipeCheckCommand
    {
        private string _trxID;
        private eRecipeCheckCommandType _commandType;

        private eRecipeCheckResult _result=eRecipeCheckResult.NG;

        private bool _isFinish=false;

        private bool _isSend = false;

        public bool IsSend
        {
            get { return _isSend; }
            set { _isSend = value; }
        }

        public bool IsFinish
        {
            get { return _isFinish; }
            set { _isFinish = value; }
        }

        public eRecipeCheckResult Result
        {
            get { return _result; }
            set { _result = value; }
        }
        
        public string TrxID
        {
            get { return _trxID; }
            set { _trxID = value; }
        }
        
        
        

        public eRecipeCheckCommandType CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        public RecipeCheckCommand(string trxId,eRecipeCheckCommandType commandType)
        {
            _trxID=trxId;
            _commandType=commandType;
        }

        public override string ToString()
        {
            return string.Format("TrxID={0},Command Type={1}.", _trxID, _commandType.ToString());
        }
    }

    //Recipe DB (儲存所做過的所有ppid 以LOCAL ,REMOTE ,OFFLINE的型式分別存放)
    //public class RECIPE : EntityData
    //{
    //    public RECIPE Data { get; set; }

    //    public RECIPE(RECIPE data)
    //    {
    //        Data = data;
    //    }
    //}

}
