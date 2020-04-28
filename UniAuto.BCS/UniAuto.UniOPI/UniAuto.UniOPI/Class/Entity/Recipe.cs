using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class Recipe
    {
        public string RecipeName { get; set; }
        public string PPID { get; set; }
        //public string RecipeType { get; set; }

        public bool AllEQ_RecipeCheck_OK { get; set; }  //是否所有eq recipe check都已回覆且OK

        //public Dictionary<string,RecipeCheckEQ> LocalRecipeCheck { get; set; }  //加入所有機台，並取得對應的recipe no & recipe check

        public List<RecipeCheckEQ> LocalRecipeCheck { get; set; }  //加入所有機台，並取得對應的recipe no & recipe check

        public string ServerName { get; set; }  //for cross line 判斷用

        public Recipe(string serverName, string recipeName, string ppid)
        {

            ServerName = serverName;

            RecipeName = recipeName;

            //L2:recipeno;L3:recipeno;L4:recipeno.....
            PPID = ppid;

            string[] _nodeRecipe = PPID.Split(';');

            LocalRecipeCheck = new List<RecipeCheckEQ>();

            for (int i = 0; i < _nodeRecipe.Length; i++)
            {
                //NodeNo:RecipeNo
                string[] _data = _nodeRecipe[i].Split(':');

                Node _node = null;

                #region Local line Node 
                if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_data[0]))
                {
                    _node = FormMainMDI.G_OPIAp.Dic_Node[_data[0]];
                }
                #endregion

                if (_node == null) continue;

                RecipeCheckEQ _check = new RecipeCheckEQ();

                _check.LocalNo = _data[0];
                _check.RecipeNo = _data[1];
                _check.NeedRecipeCheck = _node.RecipeRegisterCheck;
                _check.RecipeCheckResult = string.Empty;

                #region 內容都為0 則不需做recipe check
                if (_check.RecipeNo == _node.DefaultRecipeNo) _check.RecipeByPass = true;
                else _check.RecipeByPass = false;
                #endregion
                        
                LocalRecipeCheck.Add(_check);

                ////有一個node需做recipe check => 將EQ recipe check return OK 改為 false
                //if (_check.NeedRecipeCheck) AllEQ_RecipeCheck_OK = false;
                AllEQ_RecipeCheck_OK = false;
            }            
        }


        public void SetRecipeCheckResult(string localNo, string RecipeNo, string Result, string ResultMsg)
        {
            bool _allResultOK = true;

            RecipeCheckEQ _check = LocalRecipeCheck.Find(r => r.LocalNo.Equals(localNo) && r.RecipeNo.Equals(RecipeNo));

            if (_check == null) return;

            _check.RecipeCheckResult = Result;
            _check.ResultMsg = ResultMsg;


            foreach (RecipeCheckEQ _node in LocalRecipeCheck)
            {
                //if (_node.NeedRecipeCheck && _node.RecipeByPass == false)
                //{
                    if (_node.RecipeCheckResult == "NG")
                    {
                        _allResultOK = false;
                        break;
                    }
                //}
            }

            AllEQ_RecipeCheck_OK = _allResultOK;
        }

    }

    public class RecipeCheckEQ
    {
        public string LocalNo { get; set; }
        public string RecipeNo { get; set; }
        public bool NeedRecipeCheck { get; set; }  //是否需做recipe check
        public bool RecipeByPass { get; set; }     //是否為by pass ( recipe no = 00)
        public string RecipeCheckResult { get; set; }  //recipe check 回覆結果 OK / NG / NONE
        public string ResultMsg { get; set; }  //NG,NONE 原因  -- RecipeRegisterValidationReturnReport  1. OK: / 2. NG: NG, MACHINENG, NOMACHINE, RECIPELENNG, TIMEOUT / 3. NONE: ZERO, CIMOFF, NOCHECK
    }

    public class ProcessLine
    {
        public string ServerName { get; set; }
        public string ProductSpecName { get; set; }
        public string ProductSpecVer { get; set; }
        public string ProductOwner { get; set; }
        public string OwnerID { get; set; }
        public string BCProductType { get; set; }
        public string BCProductID { get; set; }
        public string CarrierSetCode { get; set; }
        public string LineRecipeName { get; set; }
        public string LinePPID { get; set; }

        public Dictionary<string, List<RecipeCheckEQ>> LocalRecipeCheck { get; set; }
    }

}
