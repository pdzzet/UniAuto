using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Threading;
using System.Reflection;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniRCS.Core
{
    public partial class RobotCoreService :AbstractRobotService
    {
        /// <summary>
        /// 設定是否啟用Robot Service ,True表示啟用
        /// </summary>
        protected bool _robotServiceIsEnable;

        /// <summary>
        /// 設定是否啟用Robot Service ,True表示啟用
        /// </summary>
        public bool IsEnable
        {
            get
            {
                return _robotServiceIsEnable;
            }
            set
            {
                _robotServiceIsEnable = value;
            }
        }

        /// <summary>
        /// 判斷RCS是否開始做Initial
        /// </summary>
        private bool _startRCSInitial;

        /// <summary>
        /// RCS Initial Thread ,當Initial執行完畢之後會結束這個Thread
        /// </summary>
        private Thread _RCSInitial;

        /// <summary>
        /// RCS要控制Robot的list
        /// </summary>
        List<Robot> _controlRobotList = null;

        //Function List ============================================================================================================================

        public override bool Init()
        {
            if (_robotServiceIsEnable == true)
            {
                //設定可以開始做Initial
                _startRCSInitial = true;

                _RCSInitial = new Thread(new ThreadStart(RCSInitial));
                _RCSInitial.IsBackground = true;
                _RCSInitial.Start();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Destroy()
        {
            _startRCSInitial = false;
        }

        private void RCSInitial()
        {
            string strlog = string.Empty;

            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RCS Initial Start", "L1");
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            while (_startRCSInitial)
            {
                Thread.Sleep(300);

                try
                {

                    //BCS狀態為Run之後才可以繼續執行
                    if (Workbench.State != eWorkbenchState.RUN) continue;

                    #region [ Get All Robot Entitys and Start Robot MainProc by Robot entity ]

                    if (_controlRobotList == null)
                    {
                        _controlRobotList = ObjectManager.RobotManager.GetRobots();
                        if (_controlRobotList == null)
                        {
                            continue;
                        }

                        Thread robotMain;

                        foreach (Robot procRobot in _controlRobotList)
                        {
                            robotMain = new Thread(new ParameterizedThreadStart(RobotInitial));
                            robotMain.IsBackground = true;
                            robotMain.Start(procRobot);
                        }
                        _startRCSInitial = false;
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }

            }

            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RCS Initial Finish", "L1");
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        }

        /// <summary>
        /// Initial 各Robot的參數與設定
        /// </summary>
        /// <param name="objRobot"></param>
        private void RobotInitial(object objRobot)
        {
            string strlog = string.Empty;

            try
            {

                Robot curRobot = (Robot)objRobot;

                #region [ Check Robot Entity Exist ]

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot Entity! ", "L1");
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Initial Start", "L1", curRobot.Data.ROBOTNAME);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ Robot Initial 需要Reload的參數(byLine byMode) ][ Wait_Proc_0001 ]


                #endregion

                //Robot Initial完畢之後開始啟用RobotMainProcess

                RobotMainProcess(curRobot);

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

    }
}
