using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class TCOVN_PL_ITO_RobotParam
    {
        private Job _Job01 = null;
        private DefineNormalRobotCmd _Job01_Cmd01 = null;
        private DefineNormalRobotCmd _Job01_Cmd02 = null;

        private Job _Job02 = null;
        private DefineNormalRobotCmd _Job02_Cmd01 = null;
        private DefineNormalRobotCmd _Job02_Cmd02 = null;

        public Job Job01
        {
            get { return _Job01; }
            set { _Job01 = value; }
        }
        public DefineNormalRobotCmd Job01_Cmd01
        {
            get { return _Job01_Cmd01; }
            set { _Job01_Cmd01 = value; }
        }
        public DefineNormalRobotCmd Job01_Cmd02
        {
            get { return _Job01_Cmd02; }
            set { _Job01_Cmd02 = value; }
        }

        public Job Job02
        {
            get { return _Job02; }
            set { _Job02 = value; }
        }
        public DefineNormalRobotCmd Job02_Cmd01
        {
            get { return _Job02_Cmd01; }
            set { _Job02_Cmd01 = value; }
        }
        public DefineNormalRobotCmd Job02_Cmd02
        {
            get { return _Job02_Cmd02; }
            set { _Job02_Cmd02 = value; }
        }
    }

    /// <summary>
    /// 用來控制 OVN SD 的 Buffer Cooler, 能夠 Get Get 或 Put Put 取放兩片, 避免 PLC Refresh 資料時間差的問題
    /// </summary>
    public class TCOVN_SD_RobotParam
    {
        // OVN SD 的 Buffer Cooler, 要可以 Get Get 或 Put Put 取放兩片
        // 但是 Buffer Cooler 各自有兩個 1 Slot 的 Stage
        // 所以 Buffer Cooler 要 Get Get 或 Put Put 就必須兩個 Stage 同時 SendReady 或 ReceiveAble
        // 為了避免 PLC Refresh 資料時間差的問題, 一個 Stage 的 SendReady 或 ReceiveAble ON 時就要等另一個 Stage 是否也要 SendReady 或 RecevieAble
        // 等待有 Timeout, Timeout 設定在 Config\Agent\IO\Parameter
        public const string STAGE11 = "11";
        public const string STAGE12 = "12";
        public const string STAGE13 = "13";
        public const string STAGE14 = "14";

        private Dictionary<string, DateTime> m_SendReadyOnTimes = new Dictionary<string, DateTime>();

        private Dictionary<string, DateTime> m_ReceiveAbleOnTimes = new Dictionary<string, DateTime>();

        public int TimeoutMS { get; set; }

        public TCOVN_SD_RobotParam()
        {
            ResetDateTime();
        }

        /// <summary>
        /// 將Stage SendReady ReceiveAble Bit On的時間歸零
        /// </summary>
        public void ResetDateTime()
        {
            if (m_SendReadyOnTimes.Count == 0)
            {
                m_SendReadyOnTimes.Add(STAGE11, DateTime.MinValue);
                m_SendReadyOnTimes.Add(STAGE12, DateTime.MinValue);
                m_SendReadyOnTimes.Add(STAGE13, DateTime.MinValue);
                m_SendReadyOnTimes.Add(STAGE14, DateTime.MinValue);
            }
            else
            {
                m_SendReadyOnTimes[STAGE11] = DateTime.MinValue;
                m_SendReadyOnTimes[STAGE12] = DateTime.MinValue;
                m_SendReadyOnTimes[STAGE13] = DateTime.MinValue;
                m_SendReadyOnTimes[STAGE14] = DateTime.MinValue;
            }

            if (m_ReceiveAbleOnTimes.Count == 0)
            {
                m_ReceiveAbleOnTimes.Add(STAGE11, DateTime.MinValue);
                m_ReceiveAbleOnTimes.Add(STAGE12, DateTime.MinValue);
            }
            else
            {
                m_ReceiveAbleOnTimes[STAGE11] = DateTime.MinValue;
                m_ReceiveAbleOnTimes[STAGE12] = DateTime.MinValue;
            }
        }

        /// <summary>
        /// 是否為OVN SD的Buffer Stage
        /// </summary>
        /// <param name="StageID"></param>
        /// <returns></returns>
        public bool IsBufferStage(string StageID)
        {
            return (StageID == STAGE11 || StageID == STAGE12);
        }

        /// <summary>
        /// 是否為OVN SD的Cooler Stage
        /// </summary>
        /// <param name="StageID"></param>
        /// <returns></returns>
        public bool IsCoolerStage(string StageID)
        {
            return (StageID == STAGE13 || StageID == STAGE14);
        }

        /// <summary>
        /// 設定Stage SendReady Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <param name="Dt"></param>
        public void SetSendReadyOnTime(string StageID, DateTime Dt)
        {
            if (m_SendReadyOnTimes.ContainsKey(StageID))
                m_SendReadyOnTimes[StageID] = Dt;
        }

        /// <summary>
        /// 設定Stage ReceiveAble Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <param name="Dt"></param>
        public void SetReceiveAbleOnTime(string StageID, DateTime Dt)
        {
            if (m_ReceiveAbleOnTimes.ContainsKey(StageID))
                m_ReceiveAbleOnTimes[StageID] = Dt;
        }

        /// <summary>
        /// 取得Stage SendReady Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <returns></returns>
        public DateTime GetSendReadyOnTime(string StageID)
        {
            if (m_SendReadyOnTimes.ContainsKey(StageID))
                return m_SendReadyOnTimes[StageID];
            return DateTime.MinValue;
        }

        /// <summary>
        /// 取得Stage ReceiveAble Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <returns></returns>
        public DateTime GetReceiveAbleOnTime(string StageID)
        {
            if (m_ReceiveAbleOnTimes.ContainsKey(StageID))
                return m_ReceiveAbleOnTimes[StageID];
            return DateTime.MinValue;
        }

        /// <summary>
        /// 檢查Buffer是否可以Get Get, 或者Timeout只能Get
        /// </summary>
        /// <returns>true:表示可以Get Get或已經Timeout只能Get;  false表示不可以Get Get或還沒Timeout</returns>
        public bool CheckBufferGetGet()
        {
            bool ret = false;
            if (m_SendReadyOnTimes[STAGE11] != DateTime.MinValue || m_SendReadyOnTimes[STAGE12] != DateTime.MinValue)
            {
                if (m_SendReadyOnTimes[STAGE11] != DateTime.MinValue && m_SendReadyOnTimes[STAGE12] != DateTime.MinValue)
                {
                    DateTime last = (m_SendReadyOnTimes[STAGE11] > m_SendReadyOnTimes[STAGE12]) ? m_SendReadyOnTimes[STAGE11] : m_SendReadyOnTimes[STAGE12];
                    if ((DateTime.Now - last).TotalMilliseconds <= eRobotCommonConst.ROBOT_MAIN_PROCESS_SLEEP)
                    {
                        // Stage11 SendReady ON 著, 但 Stage12 的 SendReady OFF
                        // 在 Timeout 之前 Stage11 會一直等待 Stage12, 因此不會把 Job 加到 StageCanControlJobList
                        // 此時迴圈跑到 Stage12, Stage12 SendReady 突然 ON, 雖然 Stage11,Stage12 此時同時 ON 著
                        // 但不能直接把 Stage12 Job 加到 StageCanControlJobList 裡
                        // 因為 Stage11 先放棄加 Job, 若 Stage12 突然 ON 那麼 Stage12 也要跟著放棄加 Job
                        // 否則 Select Rule 結束後得到 Stage12 的一片 Job, 就會馬上下 RobotCommand
                        // ----------------------
                        // 最近一次 SendReady ON 的時間若與現在時間太接近, 則放棄加 Job, 等待下一次迴圈
                        // ret = true;
                    }
                    else
                        ret = true;
                }
                else if (m_SendReadyOnTimes[STAGE11] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_SendReadyOnTimes[STAGE11]).TotalMilliseconds >= TimeoutMS);
                else if (m_SendReadyOnTimes[STAGE12] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_SendReadyOnTimes[STAGE12]).TotalMilliseconds >= TimeoutMS);
            }
            return ret;
        }

        /// <summary>
        /// 檢查Buffer是否可以Put Put, 或者Timeout只能Put
        /// </summary>
        /// <returns>true:表示可以Put Put或已經Timeout只能Put;  false表示不可以Put Put或還沒Timeout</returns>
        public bool CheckBufferPutPut()
        {
            bool ret = false;
            if (m_ReceiveAbleOnTimes[STAGE11] != DateTime.MinValue || m_ReceiveAbleOnTimes[STAGE12] != DateTime.MinValue)
            {
                if (m_ReceiveAbleOnTimes[STAGE11] != DateTime.MinValue && m_ReceiveAbleOnTimes[STAGE12] != DateTime.MinValue)
                {
                    DateTime last = (m_ReceiveAbleOnTimes[STAGE11] > m_ReceiveAbleOnTimes[STAGE12]) ? m_ReceiveAbleOnTimes[STAGE11] : m_ReceiveAbleOnTimes[STAGE12];
                    if ((DateTime.Now - last).TotalMilliseconds <= eRobotCommonConst.ROBOT_MAIN_PROCESS_SLEEP)
                    {
                        // Stage11 ReceiveAble ON 著, 但 Stage12 的 ReceiveAble OFF
                        // 在 Timeout 之前 Stage11 會一直等待 Stage12, 因此不會把 Slot 加到 curUDRQ_SlotList
                        // 此時迴圈跑到 Stage12, Stage12 ReceiveAble 突然 ON, 雖然 Stage11,Stage12 此時同時 ON 著
                        // 但不能直接把 Stage12 Slot 加到 curUDRQ_SlotList 裡
                        // 因為 Stage11 先放棄加 Slot, 若 Stage12 突然 ON 那麼 Stage12 也要跟著放棄加 Slot
                        // 否則 Select Rule 結束後得到 Stage12 的UDRQ Slot, 就會馬上下 RobotCommand
                        // ----------------------
                        // 最近一次 ReceiveAble ON 的時間若與現在時間太接近, 則放棄加 Slot, 等待下一次迴圈
                        // ret = true;
                    }
                    else
                        ret = true;
                }
                else if (m_ReceiveAbleOnTimes[STAGE11] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_ReceiveAbleOnTimes[STAGE11]).TotalMilliseconds >= TimeoutMS);
                else if (m_ReceiveAbleOnTimes[STAGE12] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_ReceiveAbleOnTimes[STAGE12]).TotalMilliseconds >= TimeoutMS);
            }
            return ret;
        }

        /// <summary>
        /// 檢查Cooler是否可以Get Get, 或者Timeout只能Get
        /// </summary>
        /// <returns>true:表示可以Get Get或已經Timeout只能Get;  false表示不可以Get Get或還沒Timeout</returns>
        public bool CheckCoolerGetGet()
        {
            bool ret = false;
            if (m_SendReadyOnTimes[STAGE13] != DateTime.MinValue || m_SendReadyOnTimes[STAGE14] != DateTime.MinValue)
            {
                if (m_SendReadyOnTimes[STAGE13] != DateTime.MinValue && m_SendReadyOnTimes[STAGE14] != DateTime.MinValue)
                {
                    DateTime last = (m_SendReadyOnTimes[STAGE13] > m_SendReadyOnTimes[STAGE14]) ? m_SendReadyOnTimes[STAGE13] : m_SendReadyOnTimes[STAGE14];
                    if ((DateTime.Now - last).TotalMilliseconds <= eRobotCommonConst.ROBOT_MAIN_PROCESS_SLEEP)
                    {
                        // Stage13 SendReady ON 著, 但 Stage14 的 SendReady OFF
                        // 在 Timeout 之前 Stage13 會一直等待 Stage14, 因此不會把 Job 加到 StageCanControlJobList
                        // 此時迴圈跑到 Stage14, Stage14 SendReady 突然 ON, 雖然 Stage13,Stage14 此時同時 ON 著
                        // 但不能直接把 Stage14 Job 加到 StageCanControlJobList 裡
                        // 因為 Stage13 先放棄加 Job, 若 Stage14 突然 ON 那麼 Stage14 也要跟著放棄加 Job
                        // 否則 Select Rule 結束後得到 Stage14 的一片 Job, 就會馬上下 RobotCommand
                        // ----------------------
                        // 最近一次 SendReady ON 的時間若與現在時間太接近, 則放棄加 Job, 等待下一次迴圈
                        // ret = true;
                    }
                    else
                        ret = true;
                }
                else if (m_SendReadyOnTimes[STAGE13] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_SendReadyOnTimes[STAGE13]).TotalMilliseconds >= TimeoutMS);
                else if (m_SendReadyOnTimes[STAGE14] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_SendReadyOnTimes[STAGE14]).TotalMilliseconds >= TimeoutMS);
            }
            return ret;
        }
    }

    /// <summary>
    /// 用來控制 FCSRT 的 VCR Stage, 能夠 Get Get 或 Put Put 取放兩片, 避免 PLC Refresh 資料時間差的問題
    /// </summary>
    public class FCSRT_RobotParam
    {
        // FCSRT 的 VCR Stage, 要可以 Get Get 或 Put Put 取放兩片
        // 但是 VCR Stage 有兩個, 各自是 1 Slot 的 Stage
        // 所以 VCR Stage 要 Get Get 或 Put Put 就必須兩個 Stage 同時 SendReady 或 ReceiveAble
        // 為了避免 PLC Refresh 資料時間差的問題, 一個 Stage 的 SendReady 或 ReceiveAble ON 時就要等另一個 Stage 是否也要 SendReady 或 RecevieAble
        // 等待有 Timeout, Timeout 設定在 Config\Agent\IO\Parameter
        public const string STAGE11 = "11";
        public const string STAGE12 = "12";

        private Dictionary<string, DateTime> m_SendReadyOnTimes = new Dictionary<string, DateTime>();

        private Dictionary<string, DateTime> m_ReceiveAbleOnTimes = new Dictionary<string, DateTime>();

        public int TimeoutMS { get; set; }

        public FCSRT_RobotParam()
        {
            ResetDateTime();
        }

        /// <summary>
        /// 將Stage SendReady ReceiveAble Bit On的時間歸零
        /// </summary>
        public void ResetDateTime()
        {
            if (m_SendReadyOnTimes.Count == 0)
            {
                m_SendReadyOnTimes.Add(STAGE11, DateTime.MinValue);
                m_SendReadyOnTimes.Add(STAGE12, DateTime.MinValue);
            }
            else
            {
                m_SendReadyOnTimes[STAGE11] = DateTime.MinValue;
                m_SendReadyOnTimes[STAGE12] = DateTime.MinValue;
            }

            if (m_ReceiveAbleOnTimes.Count == 0)
            {
                m_ReceiveAbleOnTimes.Add(STAGE11, DateTime.MinValue);
                m_ReceiveAbleOnTimes.Add(STAGE12, DateTime.MinValue);
            }
            else
            {
                m_ReceiveAbleOnTimes[STAGE11] = DateTime.MinValue;
                m_ReceiveAbleOnTimes[STAGE12] = DateTime.MinValue;
            }
        }

        /// <summary>
        /// 設定Stage SendReady Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <param name="Dt"></param>
        public void SetSendReadyOnTime(string StageID, DateTime Dt)
        {
            if (m_SendReadyOnTimes.ContainsKey(StageID))
                m_SendReadyOnTimes[StageID] = Dt;
        }

        /// <summary>
        /// 設定Stage ReceiveAble Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <param name="Dt"></param>
        public void SetReceiveAbleOnTime(string StageID, DateTime Dt)
        {
            if (m_ReceiveAbleOnTimes.ContainsKey(StageID))
                m_ReceiveAbleOnTimes[StageID] = Dt;
        }

        /// <summary>
        /// 取得Stage SendReady Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <returns></returns>
        public DateTime GetSendReadyOnTime(string StageID)
        {
            if (m_SendReadyOnTimes.ContainsKey(StageID))
                return m_SendReadyOnTimes[StageID];
            return DateTime.MinValue;
        }

        /// <summary>
        /// 取得Stage ReceiveAble Bit On的時間
        /// </summary>
        /// <param name="StageID"></param>
        /// <returns></returns>
        public DateTime GetReceiveAbleOnTime(string StageID)
        {
            if (m_ReceiveAbleOnTimes.ContainsKey(StageID))
                return m_ReceiveAbleOnTimes[StageID];
            return DateTime.MinValue;
        }

        /// <summary>
        /// 檢查VCR Stage是否可以Get Get, 或者Timeout只能Get
        /// </summary>
        /// <returns>true:表示可以Get Get或已經Timeout只能Get;  false表示不可以Get Get或還沒Timeout</returns>
        public bool CheckVCRStageGetGet()
        {
            bool ret = false;
            if (m_SendReadyOnTimes[STAGE11] != DateTime.MinValue || m_SendReadyOnTimes[STAGE12] != DateTime.MinValue)
            {
                if (m_SendReadyOnTimes[STAGE11] != DateTime.MinValue && m_SendReadyOnTimes[STAGE12] != DateTime.MinValue)
                {
                    DateTime last = (m_SendReadyOnTimes[STAGE11] > m_SendReadyOnTimes[STAGE12]) ? m_SendReadyOnTimes[STAGE11] : m_SendReadyOnTimes[STAGE12];
                    if ((DateTime.Now - last).TotalMilliseconds <= eRobotCommonConst.ROBOT_MAIN_PROCESS_SLEEP)
                    {
                        // Stage11 SendReady ON 著, 但 Stage12 的 SendReady OFF
                        // 在 Timeout 之前 Stage11 會一直等待 Stage12, 因此不會把 Job 加到 StageCanControlJobList
                        // 此時迴圈跑到 Stage12, Stage12 SendReady 突然 ON, 雖然 Stage11,Stage12 此時同時 ON 著
                        // 但不能直接把 Stage12 Job 加到 StageCanControlJobList 裡
                        // 因為 Stage11 先放棄加 Job, 若 Stage12 突然 ON 那麼 Stage12 也要跟著放棄加 Job
                        // 否則 Select Rule 結束後得到 Stage12 的一片 Job, 就會馬上下 RobotCommand
                        // ----------------------
                        // 最近一次 SendReady ON 的時間若與現在時間太接近, 則放棄加 Job, 等待下一次迴圈
                        // ret = true;
                    }
                    else
                        ret = true;
                }
                else if (m_SendReadyOnTimes[STAGE11] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_SendReadyOnTimes[STAGE11]).TotalMilliseconds >= TimeoutMS);
                else if (m_SendReadyOnTimes[STAGE12] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_SendReadyOnTimes[STAGE12]).TotalMilliseconds >= TimeoutMS);
            }
            return ret;
        }

        /// <summary>
        /// 檢查VCR Stage是否可以Put Put, 或者Timeout只能Put
        /// </summary>
        /// <returns>true:表示可以Put Put或已經Timeout只能Put;  false表示不可以Put Put或還沒Timeout</returns>
        public bool CheckVCRStagePutPut()
        {
            bool ret = false;
            if (m_ReceiveAbleOnTimes[STAGE11] != DateTime.MinValue || m_ReceiveAbleOnTimes[STAGE12] != DateTime.MinValue)
            {
                if (m_ReceiveAbleOnTimes[STAGE11] != DateTime.MinValue && m_ReceiveAbleOnTimes[STAGE12] != DateTime.MinValue)
                {
                    DateTime last = (m_ReceiveAbleOnTimes[STAGE11] > m_ReceiveAbleOnTimes[STAGE12]) ? m_ReceiveAbleOnTimes[STAGE11] : m_ReceiveAbleOnTimes[STAGE12];
                    if ((DateTime.Now - last).TotalMilliseconds <= eRobotCommonConst.ROBOT_MAIN_PROCESS_SLEEP)
                    {
                        // Stage11 ReceiveAble ON 著, 但 Stage12 的 ReceiveAble OFF
                        // 在 Timeout 之前 Stage11 會一直等待 Stage12, 因此不會把 Slot 加到 curUDRQ_SlotList
                        // 此時迴圈跑到 Stage12, Stage12 ReceiveAble 突然 ON, 雖然 Stage11,Stage12 此時同時 ON 著
                        // 但不能直接把 Stage12 Slot 加到 curUDRQ_SlotList 裡
                        // 因為 Stage11 先放棄加 Slot, 若 Stage12 突然 ON 那麼 Stage12 也要跟著放棄加 Slot
                        // 否則 Select Rule 結束後得到 Stage12 的UDRQ Slot, 就會馬上下 RobotCommand
                        // ----------------------
                        // 最近一次 ReceiveAble ON 的時間若與現在時間太接近, 則放棄加 Slot, 等待下一次迴圈
                        // ret = true;
                    }
                    else
                        ret = true;
                }
                else if (m_ReceiveAbleOnTimes[STAGE11] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_ReceiveAbleOnTimes[STAGE11]).TotalMilliseconds >= TimeoutMS);
                else if (m_ReceiveAbleOnTimes[STAGE12] != DateTime.MinValue)
                    ret = ((DateTime.Now - m_ReceiveAbleOnTimes[STAGE12]).TotalMilliseconds >= TimeoutMS);
            }
            return ret;
        }
    }

    public class SorterMode_RobotParam
    {
        public enum NEED_TO_CALL_CST_SERVICE
        {
            /// <summary>初始
            /// 
            /// </summary>
            NONE = 0,
            /// <summary>需要呼叫CassetteService
            /// 
            /// </summary>
            NEED_TO_CALL = 1,
            /// <summary>有一片Job找到Unloader, 不需要呼叫CassetteService
            /// 
            /// </summary>
            ONE_JOB_MATCH = 2
        }
        /// <summary>
        /// 以Job Grade尋找Unloader卻找不到時, 呼叫CassetteService
        /// </summary>
        public NEED_TO_CALL_CST_SERVICE NeedToCallCassetteService { get; set; }

        /// <summary>
        /// CassetteService只能呼叫一次, true表示可以呼叫, false表示已經呼叫過
        /// </summary>
        public bool EnableCallCassetteService { get; set; }

        /// <summary>
        /// 預設以Grade OK的Job優先
        /// </summary>
        public const string DEFAULT_FIRST_PRIORITY_GRADE = "OK";

        /// <summary>
        /// 上一個取出的Grade, 相同Grade的Job優先處理
        /// </summary>
        public string LastGrade { get; set; }

        public SorterMode_RobotParam()
        {
            NeedToCallCassetteService = NEED_TO_CALL_CST_SERVICE.NONE;
            EnableCallCassetteService = true;
            LastGrade = DEFAULT_FIRST_PRIORITY_GRADE;//預設以Grade OK的Job優先
        }
    }

    public class JobSendToSameEQ_RobotParam
    {
        /// <summary>
        /// false表示檢測機可收就可以派Job; true表示同CST Job要進同一台檢測機
        /// 在RobotMain迴圈前暫存Robot.File.curRobotSameEQFlag, 在迴圈裡使用暫存值而不使用即時值
        /// </summary>
        public bool SameEQFlag { get; set; }

        public JobSendToSameEQ_RobotParam()
        {
            SameEQFlag = false;
        }
    }

    /// <summary>
    /// 鎖定EQ Stage, 以盡量塞滿某一Stage為優先
    /// </summary>
    public class FixTargetStage_RobotParam
    {
        public enum TIMEOUT_STATE
        {
            /// <summary>
            /// 狀態歸零, 重新檢查是否Timeout
            /// </summary>
            CLEAR = 0,

            /// <summary>
            /// 尚未Timeout, 繼續等待FixStage的RecevieAble
            /// </summary>
            WAIT_FOR_TIMEOUT = 1,

            /// <summary>
            /// 尚未Timeout, 但FixStage已發出ReceiveAble
            /// </summary>
            RECEIVE_ABLE = 2,

            /// <summary>
            /// 已經Timeout
            /// </summary>
            TIMEOUT = 3
        }

        /// <summary>
        /// 開始鎖定的時間, Robot放完片且恢復IDLE之後開始計算
        /// </summary>
        public DateTime FixDateTime { get; set; }

        /// <summary>
        /// 鎖定後目標Stage必須在Timeout時間內發RecevieAble, 否則解除鎖定
        /// </summary>
        public int TimeoutMS { get; set; }

        /// <summary>
        /// 鎖定的Stage. 當STAGEID為空字串時, STAGE先發RecvAble就先處理. 在Robot放片的時候給值, 當STAGEID有值, FixDateTime才有意義, 會等待STAGE發RecvAble
        /// </summary>
        public string STAGEID { get; set; }

        /// <summary>
        /// Robot放完片後為true, 等Robot Idle時就要false並且記FixDateTime
        /// </summary>
        public bool SetFixDateTime { get; set; }

        /// <summary>
        /// FilterRule是By Job呼叫, 為了避免Slot1時未Timeout而等待FixStage, Slot20卻已Timeout而直接出片的問題
        /// </summary>
        public TIMEOUT_STATE TimeoutState { get; set; }

        public FixTargetStage_RobotParam()
        {
            FixDateTime = DateTime.MinValue;
            TimeoutMS = 0;
            STAGEID = string.Empty;
            TimeoutState = TIMEOUT_STATE.CLEAR;
            SetFixDateTime = false;
        }
    }

    public class eRobotContextParameter
    {

        public const string StageCanControlJobList = "StageCanControlJobList";
        public const string ArmCanControlJobList = "ArmCanControlJobList";
        //public const string BeforeOrderByJobList = "BeforeOrderByJobList";
        //public const string AfterOrderByJobList = "AfterOrderByJobList";
        public const string CurRobotEntity = "CurRobotEntity";
        public const string CurRobotAllStageListEntity = "CurRobotAllStageListEntity";
        public const string CurJobEntity = "CurJobEntity";
        public const string OrderByAction = "OrderByAction";
        public const string LoadJobArmNo_For_1Arm_1Job = "LoadJobArmNo_For_1Arm_1Job";
        public const string UnloadJobArmNo_For_1Arm_1Job = "UnloadJobArmNo_For_1Arm_1Job";
        public const string TargetStageID = "TargetStageID";
        public const string TargetSlotNo = "TargetSlotNo";
        //public const string StepStageLDRQList = "StepStageLDRQList";
        public const string StepCanUseStageList = "StepCanUseStageList";
        //20160802
        public const string NextStepCanUseStageList = "NextStepCanUseStageList";

        public const string Is2ndCmdCheckFlag = "Is2ndCmdCheckFlag";
        public const string Define_1stNormalRobotCommandInfo = "Define_1stNormalRobotCommandInfo";
        public const string Define_2ndNormalRobotCommandInfo = "Define_2ndNormalRobotCommandInfo";

        public const string StageSelectInfo = "StageSelectInfo";
        public const string Afrer1stOrderByCheckFlag = "Is1stOrderByCheckFlag";
        public const string AfrerOrderByResultInfo = "AfrerOrderByResultInfo";

        public const string DefineNormalRobotCmd = "DefineNormalRobotCmd";

        public const string RouteStepByPassGotoStepNo = "RouteStepByPassGotoStepNo";
        public const string RouteStepJumpGotoStepNo = "RouteStepJumpGotoStepNo";


        public const string DRYNodeNo = "L4"; //GlobalAssemblyVersion v1.0.0.26-20151016, added by dade
        public const string DRYIFReceiveAbleSignal = "ReceiveAbleSignal"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY
        public const string DRYIFReceiveType = "ReceiveType"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY
        public const string DRYStageStatus = "StageStatus"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY
        public const string DRYKeptReceiveType = "KeptReceiveType"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY
        public const string DRYPutPriorityUDRQ = "DRY_PutPriority_UDRQ"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY

        public const string TCOVN_PL_ITO_RobotParam = "TCOVN_PL_ITO_RobotParam";// for TCOVN PL ITO
        public const string TCOVN_SD_RobotParam = "TCOVN_SD_RobotParam";// for TCOVN SD
        public const string FCSRT_RobotParam = "FCSRT_RobotParam";// for FCSRT_TYPE1
        public const string JobSendToSameEQ_RobotParam = "JobSendToSameEQ_RobotParam";// for Array, 一條LINE裡有多台同型檢測機時, 同CST的Job要進同一台檢測機
        public const string FixTargetStage_RobotParam = "FixTargetStage_RobotParam";// for Array OVN ITO PL, 鎖定目標優先餵片. TCOVN_ITO跟PL, 盡量塞滿某一座烤箱, 當A,B座烤箱同時LDRQ, 那麼以鎖定目標優先餵片.
        public const string SorterMode_RobotParam = "SorterMode_RobotParam";//for CF Sorter Mode, 判斷是否該呼叫CassetteService退CST

        //20151027 add for 2 Job
        public const string Cur1stJob_CommandInfo = "Cur1stJob_CommandInfo";

        //20160115 add for 1Arm 2Job Get 2nd Cmd
        public const string Cur1stSlotBlock_CommandInfo = "Cur1stSlotBlock_CommandInfo";

        public const string CanUsePreFetchFlag = "PreFetch";
        public const string CanUseWaitFrontFlag = "WaitFront";
        public const string CanUseRTCFlag = "RTC";

        public const string CanUsePutReadyFlag = "PutReady";
        public const string CanUseGetReadyFlag = "GetReady";

        public const string MPLCLocalNo = "L1";

        public const string RTC_WAIT_TIME = "RTC_WAIT_TIME";

        //20160525
        public const string EQPRTC_WAIT_TIME = "EQPRTC_WAIT_TIME";

        public const string UDRQ_JOB_FORECAST_CHECK = "UDRQ_JOB_FORECAST_CHECK";

        public const string DRYLastFetchProcessType = "FetchProcessType";

        //20151216 add for Cell Special 1Arm 2Job
        public const string StageCanControlSlotBlockInfoList = "StageCanControlSlotBlockInfoList";
        public const string ArmCanControlSlotBlockInfoList = "ArmCanControlSlotBlockInfoList";

        //20151227 add for Cell Special 1Arm 2Job
        public const string CurSlotBlockInfoEntity = "CurSlotBlockInfoEntity";

        public const string PREFETCH_DELAY_TIME = "PREFETCH_DELAY_TIME";
        
        public const string IsType2Flag = "IsType2Flag"; //2016/1/6

        public const string IsRecvDelayTimeNGFlag = "IsRecvDelayTimeNGFlag"; //2016/1/25 use in ELA line

        public const string IsRecipeNGFlag = "IsRecipeNGFlag"; //2016/2/4 use in ELA line

        public const string EquipmentNo = "EquipmentNo";//2016/02/04 use in CRP line

        public const string UnitNo = "UnitNo";//Yang,2016/08/21

        public const string chambermode = "chambermode";//Yang,2016/08/21


        public eRobotContextParameter()
        {
        }

    }

    public class eJob_CheckFail_Reason
    {
        //Job Check Fail 以J開頭後接9碼
        public const string Get_CstSlotExistJob_Route_Is_Fail = "J000000001";
        public const string Get_CstSlotExistJob_CurStepNo_OutofMaxStepNo = "J000000002";
        public const string Get_CstSlotExistJob_CheckFetchOut_Condition_Fail = "J000000003";
        //20160812 下robot commnad前,在強制判斷是不是FromCST(Both),避免進到預取流程,因為沒Filter,所以回錯CST
        public const string Job_FromCST_To_TargetCST_Is_Fail = "J000000004";
        //20160815 Get的stage與Put的stage不能一樣,避免同stage取又放
        public const string Job_FromStage_And_TargetStage_Are_Same_Fail = "J000000005";
        public const string Job_PreFetch_TargetStage_Is_Cannot_Cassette_Fail = "J000000006";
        public const string Job_PreFetch_TargetStage_Is_Clean_Out_Fail = "J000000007"; //add by yang 20161003

    }

    public class eRobot_CheckFail_Reason
    {
        //Robot Check Fail 以R開頭後接9碼
        public const string CAN_ISSUE_CMD_ROBOT_STATUS_FAIL = "R000000001";
        public const string CAN_ISSUE_CMD_ROBOTCMD_STATUS_FAIL = "R000000002";
        public const string CAN_ISSUE_CMD_FIND_ROBOT_EQP_FAIL = "R000000003";
        public const string CAN_ISSUE_CMD_ROBOT_EQP_CIM_ON_FAIL = "R000000004";
        public const string CAN_ISSUE_CMD_CMD_REPLY_BIT_OFF_FAIL = "R000000005";
        public const string GET_CAN_CONTROL_JOB_FAIL = "R000000006";
        public const string CAN_ISSUE_CMD_NO_COMMAND_ON_ROBOT_FAIL = "R000000007";
        public const string CAN_ISSUE_CMD_ROBOT_EQP_OPERATIONMODE_IS_MANUAL = "R000000008";
        public const string CAN_ISSUE_CMD_ROBOT_FETCHPORT_WAIT_FIRSTGLASSCHECK = "R000000009";
        public const string CAN_ISSUE_CMD_ROBOT_SELECTRULE_IS_NULL = "R000000010";
        public const string CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_STATUS_IS_ABORTING = "R000000011";
        public const string IO_PARAMETERS_PORT_CAN_FETCH_OUT_CHECKFLAG_IS_DISABLE = "R000000024";//不可出片只可收片,Port Can Fetch Out Check,it is cycle stop Fetch Out,match IO/Parameters:PortCanFetchOutCheckFlag,Deng,20190828
        public const string CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_INDEXER_STATUS_IS_DOWN = "R000000018";   //Watson Add 20151216 For AOH800
        //20160720
        public const string DOWNSTREAM_SLOTNO_IS_ZERO = "R000000019";
        public const string UPSTREAM_SLOTNO_IS_ZERO = "R000000020";
        public const string DOWNSTREAM_SLOTNO_IS_MISMATCH = "R000000021";
        public const string UPSTREAM_SLOTNO_IS_MISMATCH = "R000000022";

        //20151124 add for 1Arm 2Job Select
        public const string SELECT_PORT_CAN_NOT_GET_PORT_ENTITY = "R000000012";
        public const string SELECT_PORT_IS_NOT_ENABLE = "R000000013";
        public const string SELECT_PORT_IS_DOWN = "R000000014";
        public const string SELECT_PORT_CASSETTE_TYPE_IS_RANDOM = "R000000015";
        public const string SELECT_PORT_CST_FETCHOUT_MODE_IS_NOT_SEQUENCE = "R000000016";
        public const string SELECT_PORT_TYPE_IS_BOTH = "R000000017";
        public const string SELECT_PORT_CASSETTESTATUS_NOT_WAITFORPROCESS_INPROCESS = "R000000023";
    }

    public class eRobotCommonConst
    {
        public const int LOG_SERVICE_LENGTH = 30;
        public const int LOG_FUNCTION_LENGTH = 60;
        public const int LOG_FAILCODE_LENGTH = 60;

        /// <summary>
        /// 300ms, 必須是毫秒
        /// </summary>
        public const int ROBOT_MAIN_PROCESS_SLEEP = 300;

        /// <summary>发送RobotCommand EQ Reply 超时 TimerID
        /// 
        /// </summary>
        public const string ROBOT_CONTROL_COMMAND_TIMEOUT = "RobotControlCommandTimeout";

        /// <summary>发送CellSpecialRobotCommand EQ Reply 超时 TimerID
        /// 
        /// </summary>
        public const string CELL_SPECIAL_ROBOT_CONTROL_COMMAND_TIMEOUT = "CellSpecialRobotControlCommandTimeout";

        /// <summary>发送RobotCommandResultReport EQ Reply 超时 TimerID
        /// 
        /// </summary>
        public const string ROBOT_COMMAND_RESULT_REPORT_TIMEOUT = "RobotCommandResultReportTimeout";

        /// <summary>发送CellSpecialRobotCommandResultReport EQ Reply 超时 TimerID
        /// 
        /// </summary>
        public const string CELL_SPECIAL_ROBOT_COMMAND_RESULT_REPORT_TIMEOUT = "CellSpecialRobotCommandResultReportTimeout";

        public const string DB_FUNCTION_IS_ENABLE = "Y";
        public const string DB_FUNCTION_IS_DISABLE = "N";
        public const string DB_ORDER_BY_ASC = "ASC";
        public const string DB_ORDER_BY_DESC = "DESC";

        public const string ROBOT_HOME_STAGEID = "00";

        public const string ROBOT_TABLE_NAME = "SBRM_ROBOT";
        public const string ROBOT_ROBOT_METHOD_DEF_TABLE_NAME = "SBRM_ROBOT_METHOD_DEF";
        public const string ROBOT_PROC_RESULT_HANDLE_TABLE_NAME = "SBRM_ROBOT_PROC_RESULT_HANDLE";
        public const string ROBOT_ROUTE_CONDITION_TABLE_NAME = "SBRM_ROBOT_ROUTE_CONDITION";
        public const string ROBOT_ROUTE_MST_TABLE_NAME = "SBRM_ROBOT_ROUTE_MST";
        public const string ROBOT_ROUTE_STEP_TABLE_NAME = "SBRM_ROBOT_ROUTE_STEP";
        public const string ROBOT_RULE_FILTER_TABLE_NAME = "SBRM_ROBOT_RULE_FILTER";
        public const string ROBOT_RULE_JOB_SELECT_TABLE_NAME = "SBRM_ROBOT_RULE_JOB_SELECT";
        public const string ROBOT_RULE_ORDERBY_TABLE_NAME = "SBRM_ROBOT_RULE_ORDERBY";
        public const string ROBOT_STAGE_TABLE_NAME = "SBRM_ROBOT_STAGE";
        public const string ROBOT_RULE_STAGE_SELECT_TABLE_NAME = "SBRM_ROBOT_RULE_STAGE_SELECT";
        //20151113 Modity Table Name by DB
        public const string ROBOT_ROUTE_STEP_BYPASS_TABLE_NAME = "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS";// "SBRM_ROBOT_ROUTE_STEP_BYPASS";
        public const string ROBOT_ROUTE_STEP_JUMP_TABLE_NAME = "SBRM_ROBOT_RULE_ROUTESTEP_JUMP";//"SBRM_ROBOT_ROUTE_STEP_JUMP";

        //for Detail Log Mode Start and End Entity
        public const char MODE_START_CHAR = 'V';
        public const int MODE_START_CHAR_LENGTH = 200;
        public const char MODE_END_CHAR = '^';
        public const int MODE_END_CHAR_LENGTH = 150;

        //for Detail Log Rule Select Start and End Entity
        public const char RULE_SELECT_START_CHAR = '*';
        public const int RULE_SELECT_START_CHAR_LENGTH = 100;
        public const char RULE_SELECT_END_CHAR = '*';
        public const int RULE_SELECT_END_CHAR_LENGTH = 80;
        public const char ALL_RULE_SELECT_START_CHAR = '=';
        public const int ALL_RULE_SELECT_START_CHAR_LENGTH = 150;
        public const char ALL_RULE_SELECT_END_CHAR = '=';
        public const int ALL_RULE_SELECT_END_CHAR_LENGTH = 100;

        //for Detail Log Rule StageSelect Start and End Entity
        public const char RULE_STAGESELECT_START_CHAR = '*';
        public const int RULE_STAGESELECT_START_CHAR_LENGTH = 100;
        public const char RULE_STAGESELECT_END_CHAR = '*';
        public const int RULE_STAGESELECT_END_CHAR_LENGTH = 80;
        public const char ALL_RULE_STAGESELECT_START_CHAR = '=';
        public const int ALL_RULE_STAGESELECT_START_CHAR_LENGTH = 150;
        public const char ALL_RULE_STAGESELECT_END_CHAR = '=';
        public const int ALL_RULE_STAGESELECT_END_CHAR_LENGTH = 100;

        //for Detail Log Rule RouteStepByPass Start and End Entity
        public const char RULE_ROUTESTEPBYPASS_START_CHAR = '*';
        public const int RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH = 100;
        public const char RULE_ROUTESTEPBYPASS_END_CHAR = '*';
        public const int RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH = 80;
        public const char ALL_RULE_ROUTESTEPBYPASS_START_CHAR = '=';
        public const int ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH = 150;
        public const char ALL_RULE_ROUTESTEPBYPASS_END_CHAR = '=';
        public const int ALL_RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH = 100;

        //for Detail Log Rule RouteStepJump Start and End Entity
        public const char RULE_ROUTESTEPJUMP_START_CHAR = '*';
        public const int RULE_ROUTESTEPJUMP_START_CHAR_LENGTH = 100;
        public const char RULE_ROUTESTEPJUMP_END_CHAR = '*';
        public const int RULE_ROUTESTEPJUMP_END_CHAR_LENGTH = 80;
        public const char ALL_RULE_ROUTESTEPJUMP_START_CHAR = '=';
        public const int ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH = 150;
        public const char ALL_RULE_ROUTESTEPJUMP_END_CHAR = '=';
        public const int ALL_RULE_ROUTESTEPJUMP_END_CHAR_LENGTH = 100;

        //for Detail Log Rule Filter Start and End Entity
        public const char RULE_FILTER_START_CHAR = '*';
        public const int RULE_FILTER_START_CHAR_LENGTH = 100;
        public const char RULE_FILTER_END_CHAR = '*';
        public const int RULE_FILTER_END_CHAR_LENGTH = 80;
        public const char ALL_RULE_FILTER_START_CHAR = '=';
        public const int ALL_RULE_FILTER_START_CHAR_LENGTH = 150;
        public const char ALL_RULE_FILTER_END_CHAR = '=';
        public const int ALL_RULE_FILTER_END_CHAR_LENGTH = 100;

        //for Detail Log Rule OrderBy Start and End Entity
        public const char RULE_ORDERBY_START_CHAR = '*';
        public const int RULE_ORDERBY_START_CHAR_LENGTH = 100;
        public const char RULE_ORDERBY_END_CHAR = '*';
        public const int RULE_ORDERBY_END_CHAR_LENGTH = 80;

        public const string DB_SUBJOBDATA_ITEM_TRACKINGDATA = "TrackingData";
        public const string LOG_Check_1stCmd_Desc = "Check 1st Command";
        public const string LOG_Check_2ndCmd_Desc = "Check 2nd Command";

        public const string ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME = "RobotCommandActiveTimeout";
        public const string ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_CONSTANT_KEY = "ROBOT_CMD_WAITACTIVE_TIMEOUT";
        public const string ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME = "RobotCommandRT2Timeout";
        public const string ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_CONSTANT_KEY = "ROBOT_CMD_RT2_TIMEOUT";

        //20151026 add for Setting Both Job can Store Another SlotNo
        public const string BOTHPORTJOB_CAN_STORETO_NOTSOURCESLOTNO_CONSTANT_KEY = "ROBOT_BOTHPORTJOB_CAN_STORETO_NOTSOURCESLOTNO";

        public const int ROBOT_ROUTE_COMPLETE_STEPNO = 65535;

        public const string DB_SETTING_TRACKINGDATE_LEN1_ON = "1";
        public const string DB_SETTING_TRACKINGDATE_LEN2_ON = "3";

        //20151110 add Stage HighestPriority 
        public const string ROBOT_STAGE_HIGTEST_PRIORITY = "99";
        public const string ROBOT_STAGE_LOWEST_PRIORITY = "01";

        //20151124 add for 1Arm2Job Use
        public const string ROBOT_ARM_FRONT_LOCATION = "01";
        public const string ROBOT_ARM_BACK_LOCATION = "02";

        //20151124 add 定義一個Column有多少個Slot以便運算
        /// <summary>
        /// 100
        /// </summary>
        public const int CELL_1ARM2JOB_ONE_COLUMN_COUNT = 100;

        //20151209 add for Froce Return CST Without LDRQ TimeOut
        public const string ROBOT_FORCE_RETURN_CST_WITHOUT_LDRQ_TIMEOUT_CONSTANT_KEY = "ROBOT_FORCE_RETURN_CST_WITHOUT_LDRQ_TIMEOUT";

        //20160104 Add for MQC TTP One Route Could Start Cassette Command
        public const string ROBOT_ROUTE_NOUSE_NOCHECK = "ROBOT_ROUTE_NOUSE_NOCHECK";

        //20160108 add for Cell MX/EMP Grade
        public const string PORT_MX_GRADE = "MX";
        public const string PORT_EMP_GRADE = "EM"; //20160114 Modfiy 只有兩碼EM

        public eRobotCommonConst()
        {
        }
    }

    public class eRobotSelectJob_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為SE開頭後接8碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "SE00000000";
        public const string NG_curRobot_Is_Null = "SE00000001";
        public const string NG_curBcsJob_Is_Null = "SE00000002";
        public const string NG_Get_RobotStageList_Is_Null = "SE00000003";
        public const string NG_Get_RobotStageList_Is_Empty = "SE00000004";
        public const string NG_Get_Robot_Is_Not_Cell_Special = "SE00000005";
        public const string NG_Get_StageCanControlSlotBlockInfoList_Is_Null = "SE00000006";
        public const string NG_Get_ArmCanControlSlotBlockInfoList_Is_Null = "SE00000007";

    }

    public class eRobotSelectJob_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eJobFilter_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為FE開頭後接8碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "FE00000000";
        public const string NG_curRobot_Is_Null = "FE00000001";
        public const string NG_curBcsJob_Is_Null = "FE00000002";
        public const string NG_Job_Location_IsNot_Robot = "FE00000003";
        public const string NG_Job_Location_Is_Robot = "FE00000004";
        public const string NG_ArmJob_StepAction_Is_Fail = "FE00000005";
        public const string NG_NotArmJob_StepAction_Is_Fail = "FE00000006";
        public const string NG_RobotArmType_IsNot_1Arm1Job = "FE00000007";
        public const string NG_ArmJob_RouteUseArm_GlassNotExist = "FE00000008";
        public const string NG_StageJob_RouteUseArm_GlassExist = "FE00000009";
        public const string NG_StageJob_RouteUseArm_Setting_Fail = "FE00000010";
        public const string NG_Job_Get_RouteStep_Fail = "FE00000011";
        public const string NG_DecodeTrackingData_Fail = "FE00000012";
        public const string NG_ArmJob_CheckTrackingData_Fail = "FE00000013";
        public const string NG_StageJob_CheckTrackingData_Fail = "FE00000014";
        public const string NG_Get_Stage_Is_Null = "FE00000015";
        public const string NG_Get_LDRQStageList_Is_Fail = "FE00000016";
        public const string NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail = "FE00000017";
        public const string NG_NoSet_2ndCmdCheckFlag_Fail = "FE00000018";
        public const string NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail = "FE00000019";
        public const string NG_Chek_NextStep_FilterCondition_Fail = "FE00000020";
        public const string NG_RecipeCheck_NoAnyStage_Receive = "FE00000021"; //Watson 20150922 Add For RecipeCheck No Stage can receive.
        public const string NG_Get_EQP_Is_Null = "FE00000022"; //Watson 20150930 Add For No EQP can receive.
        public const string NG_FetchOutSampingFlag_Is_Fail = "FE00000023";
        public const string NG_FetchOutEqpHaveGlass_Is_Fail = "FE00000023-1"; //add qiumin 20170830 for ATS400
        public const string NG_PortStoreByTurnFlagAndAngle_Is_Fail = "FE00000023-2";  //add qiumin 20170830 FOR ATS400
        public const string NG_JobOrTargetStageNotInChangerPlan = "FE00000024";
        public const string NG_PlanStatusIsNotReadyOrStart = "FE00000024-1";
        public const string NG_CanNotGetAnyChangerPlan = "FE00000024-2";
        public const string NG_JobNotInChangerPlan = "FE00000024-3";
        public const string NG_TargetStageNotInChangerPlan = "FE00000024-4";
        public const string NG_CVDFetchGlassProportionalRule_Fail = "FE00000025";  //Watson 20151015 Add For CVD
        public const string NG_Get_LINE_Is_Null = "FE00000026";  //Watson 20151016 Add For CVD
        public const string NG_Chek_NextStep_RouteStepByPassCondition_Fail = "FE00000027";
        public const string NG_Chek_NextStep_RouteStepJumpCondition_Fail = "FE00000028";
        public const string NG_Get_curRobot_Line_Is_Fail = "FE00000029";
        public const string NG_PortFetchOutNotFroceCleanOut_Is_Fail = "FE00000030";
        public const string NG_curCST_Is_Null = "FE00000031";
        public const string NG_PortFetchOutFirstGlassCheck_Is_Fail = "FE00000032";
        public const string NG_curPort_Is_Null = "FE00000033";
        public const string NG_DRY_RouteInfo_Is_Null = "FE00000034"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY
        public const string NG_DRY_PrcessType_Mismatch = "FE00000035"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY
        public const string NG_DRY_ProcessType_Different = "FE00000036"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY 
        public const string NG_DRY_Get_ReceiveType_Is_Null = "FE00000037"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY 
        public const string NG_DRY_Get_ReceiveType_Is_Fail = "FE00000038"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY 
        public const string NG_DRY_ReceiveType_Is_Zero = "FE00000039";  //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY 
        public const string NG_DRY_Check_ReceiveType_Is_Fail = "FE00000040"; //GlobalAssemblyVersion v1.0.0.26-20151021, for DRY 
        public const string NG_CoolRunRemainCount_Is_Fail = "FE00000041"; //Brent 20151029 Add for Cool Run Remain Count 
        public const string NG_No_Arm_Can_Use = "FE00000042";
        public const string NG_No_Macth_Port_Mode = "FE00000043";//Filter_Cell_PDR_JobJudgePortMode
        public const string NG_Job_Get_Route_Fail = "FE00000044";
        public const string NG_No_Match_Port_Grade = "FE00000045";
        public const string NG_DRY_STAGE_NOT_FOUND = "FE00000046";
        public const string NG_DRY_STAGETYPE_NOT_PORT = "FE00000047";
        public const string NG_Get_CheckNoStopStageList_Is_Null = "FE00000048";
        public const string NG_Get_CheckNoStopStageList_StepAction_Not_PUT = "FE00000049";
        public const string NG_Get_CheckNoStopStageList_Is_Fail = "FE00000050";
        public const string NG_RobotArmType_IsNot_1Arm2Job = "FE00000051";
        public const string NG_JobJudgeAndEQPFlagIsNotMatch = "FE00000052";
        public const string NG_SlotBlockInfo_Is_Null = "FE00000053";
        public const string NG_SlotBlockInfo_Job_Is_Null = "FE00000054";
        public const string NG_SlotBlockInfo_FrontBack_LocationStageID_Is_Different = "FE00000055";
        public const string NG_SlotBlockInfo_FrontBack_Action_Is_Different = "FE00000056";
        public const string NG_SlotBlockInfo_FrontBack_UseArm_Is_Different = "FE00000057";
        public const string NG_ThroughMode = "FE00000058";
        public const string NG_Current_PortRouteAndJobRouteIsNotMatch = "FE00000059";
        public const string NG_Get_Current_PortRoute = "FE00000060";
        public const string NG_ELACleanStageReceiveDelayTimeCheck = "FE00000061";
        public const string NG_ELACleanStageReceiveDelayTimeCheck_ModeNotMatch = "FE00000062";
        public const string NG_ELACleanStageReceiveDelayTimeCheck_StatusAllDown = "FE00000063";
        public const string NG_curSlotBlockInfo_Is_Null = "FE00000064";
        public const string NG_JobProcessTypeEQRunModeCheck = "FE00000065";
        public const string NG_REQ_IS_Satisfied = "FE00000066";
        public const string NG_CheckStepAction_Is_Error = "FE00000067";
        public const string NG_Job_Get_LineByServerName_Fail = "FE00000068";
        public const string NG_Job_Get_LineUnloaderDispatchRule_Is_Empty = "FE00000069";
        public const string NG_Job_CheckUnloaderDispatchRuleByJobGrade_Fail = "FE00000070";
        public const string NG_Normal_MQCMode = "FE00000071";
        //20160118 add for Check SlotBlockInfo Front/Back Job RouteCondition
        public const string NG_SlotBlockInfo_FrontBackJob_RouteID_NotMatch = "FE00000072";
        public const string NG_SlotBlockInfo_FrontBackJob_CurStepID_NotMatch = "FE00000073";
        public const string NG_SlotBlockInfo_FrontBackJob_NextStepID_NotMatch = "FE00000074";        

        public const string NG_DRY_PROCESSTYPEBLOCK_NOT_FOUND = "FE00000075";
        public const string NG_DRY_PROCESSTYPEBLOCK_IS_EMPTY = "FE00000076";
        public const string NG_DRY_RECEIVETYPE_PROCESSTYPEBLOCK_MISMATCH = "FE00000077";
        public const string NG_Job_CheckUnloaderDispatchRuleByEQPFlag_Fail = "FE00000078";
        public const string NG_Job_CheckUnloaderDispatchRuleBySettingCode_Fail = "FE00000079";
        public const string NG_Job_CheckUnloaderDispatchRuleByAssignment_Fail = "FE00000080";
        public const string NG_Job_CheckUnloaderDispatchRuleByJudge_Fail = "FE00000081";
        public const string NG_ELAProcessRecipeNotMatch = "FE00000082";

        public const string NG_SlotBlockInfo_FrontBackJob_SamplingFlag_NotMatch = "FE00000083";
        public const string NG_SlotBlockInfo_StageType_Is_Not_Port = "FE00000084";
        public const string NG_GetStageRecipeGroupNoList_Is_Empty = "FE00000085";
        public const string NG_GetStageRecipeGroupNoList_Is_Mismatch = "FE00000086";
        //20160318 add for Check Front/Back DCS and Sorflag for CCSOR FlagMode
        public const string NG_SlotBlockInfo_FrontBackJob_EQPFlag_NotMatch = "FE00000087";
        public const string NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetEQP_Fail = "FE00000088";
        public const string NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetLine_Fail = "FE00000089";
        public const string NG_ProductTypeCheck_Fail = "FE00000090";
        public const string NG_DailyCheck_Fail = "FE00000091";
        public const string NG_FixTargetNotTimeOut = "FE00000092";
        //20160606
        public const string NG_Job_CheckUnloaderDispatchRuleBySameJobGrade_Fail = "FE00000093";
        //20160615
        public const string NG_PortFetchOutNotAbnormalForceCleanOut_Is_Fail = "FE00000094";
        public const string NG_No_Unloading_Port = "FE00000095";
        public const string NG_JudgeNG_No_Macth_UnloadingPort = "FE00000096";

        public const string NG_No_Can_Use_Chamber = "FE00000097";//20160819
        public const string NG_Filter_DRYMixNoFetchOutRule = "FE00000099";//20160825

        public const string NG_Job_MisMatchChamberMode = "FE00000098";
        public const string NG_Cassette_WaitingforFirstGlassCheck = "FE00000100";

        public const string NG_PDRNotRequest_CEMRequest = "FE00000101";
        public const string NG_ELAEQPTypeMismatch = "FE00000102";//201701017
        public const string NG_ELADelayTime= "FE00000103";//20180719 qiumin 

    }

    public class eStageType
    {
        public const string PORT = "PORT";
        public const string EQUIPMENT = "EQUIPMENT";
    }

    public class eJobFilter_ReturnMessage
    {
        public const string OK_Message = "";
    }

    public class eJobOrderBy_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為O(Eng,Not Zero)E開頭後接8碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "OE00000000";
        public const string NG_BeforeOrderStageList_Is_Null = "OE00000001";
        public const string NG_BeforeOrderStageList_Is_Empty = "OE00000002";
        public const string NG_OrderByAction_Is_Null = "OE00000003";
        public const string NG_OrderByAction_Is_illgal = "OE00000004";
        public const string NG_GetAfter1stOrderByFlag_Is_Fail = "OE00000005";
        public const string NG_GetDefineNormalRobotCmd = "OE00000006";
        public const string NG_GetCurBcsJob = "OE00000007";
        public const string NG_GetChangerPlan = "OE00000008";
        public const string NG_GetPort = "OE00000009";
        public const string NG_ChangerPlanCassetteID = "OE00000010";
        public const string NG_JobExistenceSlotMismatch = "OE00000011";
        public const string NG_JobAlreadyExistInTargetSlot = "OE00000012";
        public const string NG_SourcePortNotBothPort = "OE00000013";
        public const string NG_OriginalCassetteOriginalSlotIsNotEmpty = "OE00000014";
        public const string NG_UnloadDispatch_No_TargetStage = "OE00000015";
        public const string NG_Select_No_TargetStage = "OE00000016";
        public const string NG_SortGradeTargetPort = "OE00000017";
        public const string NG_TargetStageNotEmptySlot = "OE00000018";
        public const string NG_targetPositonOrTargetSlotNoIsillegal = "OE00000019";
        public const string NG_No_Match_Port_Grade = "OE00000020";
        public const string NG_SelectMAC_No_TargetStage = "OE00000021";
    }

    public class eJobOrderBy_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eRouteCondition_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為RE開頭後接8碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "RE00000000";
        //20160111 add
        public const string NG_curRobot_Is_Null = "RE00000001";
        public const string NG_curRobot_DockEQP_Is_Null = "RE00000002";
        public const string NG_curRobot_DockLine_Is_Null = "RE00000003"; 
        //20160204 add
        public const string EquipmentNo_Is_Error = "RE00000004"; 
    }

    public class eRouteCondition_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eProcResultAction_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為AE開頭後接8碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "AE00000000";
        public const string NG_curRobot_Is_Null = "AE00000001";
        public const string NG_curBcsJob_Is_Null = "AE00000002";
        public const string NG_Check_curBcsJob_Loction_Not_RobotArm_Fail = "AE00000003";
        public const string NG_Get_RouteInfo_Fail = "AE00000004";
        public const string NG_JOB_CurStep_Action_IsNot_GET = "AE00000005";
        public const string NG_Get_CurLoadArmNo_Fail = "AE00000006";
        public const string NG_Get_CurUnloadArmNo_Fail = "AE00000007";
        public const string NG_Check_curBcsJob_Loction_On_RobotArm_Fail = "AE00000008";
        public const string NG_JOB_CurStep_Action_IsNot_PUT_EX = "AE00000009";
        public const string NG_TargetStage_Is_Null = "AE00000010";
        public const string NG_TargetSlotNo_Is_Null = "AE00000011";
        public const string NG_Check_curBcsJob_ProcessType_Not_RobotArm_ProcessType = "AE00000012";

    }

    public class eProcResultAction_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eJobStageSelect_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為SS開頭後接8碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "SS00000000";
        public const string NG_curRobot_Is_Null = "SS00000001";
        public const string NG_curBcsJob_Is_Null = "SS00000002";
        public const string NG_curJobStageSelectInfo_Is_Null = "SS00000003";
        //public const string NG_Job_Location_Is_Robot = "FE00000004";
        //public const string NG_ArmJob_StepAction_Is_Fail = "FE00000005";
        //public const string NG_NotArmJob_StepAction_Is_Fail = "FE00000006";
        public const string NG_RobotArmType_IsNot_1Arm1Job = "SS00000007";
        //public const string NG_ArmJob_RouteUseArm_GlassNotExist = "FE00000008";
        //public const string NG_StageJob_RouteUseArm_GlassExist = "FE00000009";
        //public const string NG_StageJob_RouteUseArm_Setting_Fail = "FE00000010";
        public const string NG_Job_Get_AfterStageSelectStep_Fail = "SS00000011";
        //public const string NG_DecodeTrackingData_Fail = "FE00000012";
        //public const string NG_ArmJob_CheckTrackingData_Fail = "FE00000013";
        //public const string NG_StageJob_CheckTrackingData_Fail = "FE00000014";
        //public const string NG_Get_Stage_Is_Null = "FE00000015";
        public const string NG_Get_curStageSelectCanUseStageList_Is_Fail = "SS00000015";
        public const string NG_Get_CheckStepStageList_Is_Fail = "SS00000016";
        //public const string NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail = "FE00000017";
        //public const string NG_NoSet_2ndCmdCheckFlag_Fail = "FE00000018";
        //public const string NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail = "FE00000019";
        //public const string NG_Chek_NextStep_FilterCondition_Fail = "FE00000020";
        //public const string NG_RecipeCheck_NoAnyStage_Receive = "FE00000021"; //Watson 20150922 Add For RecipeCheck No Stage can receive.
    }

    public class eJobStageSelect_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eJobRouteStepByPass_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為RSPS開頭後接6碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "RSPS000000";
        public const string NG_curRobot_Is_Null = "RSPS000001";
        public const string NG_curBcsJob_Is_Null = "RSPS000002";
        public const string NG_curJobStageSelectInfo_Is_Null = "RSPS000003";
        public const string NG_RobotArmType_IsNot_1Arm1Job = "RSPS000004";
        public const string NG_Job_Get_AfterStageSelectStep_Fail = "RSPS000005";
        public const string NG_Get_curStageSelectCanUseStageList_Is_Fail = "RSPS000006";
        public const string NG_Get_CheckStepStageList_Is_Fail = "RSPS000007";
        public const string NG_Get_CurStepRoute_Is_Fail = "RSPS000008";
        public const string NG_RecipeIsByPass = "RSPS000009";
        public const string NG_NoSet_2ndCmdCheckFlag_Fail = "RSPS000010";
        public const string NG_RecipeIsByPass_GotoStepNo_Is_Fail = "RSPS000011";
        //20160525
        public const string NG_RecipeIsByPass_Get_curRobot_Line_Is_Fail = "RSPS000012";
        public const string NG_RecipeIsByPass_GotoStepRoute_Is_Fail = "RSPS000013";
    }

    public class eJobRouteStepByPass_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eJobRouteStepJump_ReturnCode
    {
        //Return Code 為10碼 ,Return NG為RSJ開頭後接7碼
        public const string Result_Is_OK = "0000000000";
        public const string NG_Exception = "RSJ0000000";
        public const string NG_curRobot_Is_Null = "RSJ0000001";
        public const string NG_curBcsJob_Is_Null = "RSJ0000002";
        public const string NG_curJobStageSelectInfo_Is_Null = "RSJ0000003";
        public const string NG_RobotArmType_IsNot_1Arm1Job = "RSJ0000004";
        public const string NG_Job_Get_AfterStageSelectStep_Fail = "RSJ0000005";
        public const string NG_Get_curStageSelectCanUseStageList_Is_Fail = "RSJ0000006";
        public const string NG_Get_CheckStepStageList_Is_Fail = "RSJ0000007";
        public const string NG_Get_CurStepRoute_Is_Fail = "RSJ0000008";
        public const string NG_RecipeIsByPass = "RSJ0000009";
        public const string NG_NoSet_2ndCmdCheckFlag_Fail = "RSJ0000010";
        public const string NG_ForceCleanOut_GotoStepNo_Is_Fail = "RSJ0000011";
        public const string NG_Get_curRobot_Line_Is_Fail = "RSJ0000012";
        public const string NG_Get_JumpGotoStepRoute_Is_Fail = "RSJ0000013";
        public const string NG_ForceCleanOutJumpNewStepID = "RSJ0000014";
        public const string NG_EQPFlag_SubItem_Is_Null = "RSJ0000015";
        public const string NG_TTPDailyCheck_GotoStepNo_Is_Fail = "RSJ0000016";
        public const string NG_TTPDailyCheckJumpNewStepID = "RSJ0000017";
        public const string NG_VCRNG_GotoStepNo_Is_Fail = "RSJ0000018";
        public const string NG_VCRDisable_GotoStepNo_Is_Fail = "RSJ0000019";
        public const string NG_ELABackUp_GotoStepNo_Is_Fail = "RSJ0000020";
        public const string NG_ELAMQC_GotoStepNo_Is_Fail = "RSJ0000021";
        public const string NG_VCRNotDisable_GotoStepNo_Is_Fail = "RSJ0000022";
        public const string NG_RouteStep_Is_Null = "RSJ0000023";
        public const string NG_Route_Is_Null = "RSJ0000023";
        public const string NG_ELAOverQTime_GotoStepNo_Is_Fail = "RSJ0000024";
        public const string NG_ForceReturnCSTWithoutLDRQ_GotoStepNo_Is_Fail = "RSJ0000025";
        public const string NG_GAP_SortMode_GotoStepNo_Is_Fail = "RSJ0000026";
        public const string NG_GAPSortModeCheckJumpNewStepID = "RSJ0000027";
        //20160525
        public const string NG_EQPWaitRTC_Is_Fail = "RSJ0000028";
        public const string NG_EQPWaitRTC_GotoStepNo_Is_Fail = "RSJ0000029";
        public const string NG_CVD_GotoStepNo_Is_Fail = "RSJ00000030";//Yang
        public const string NG_EQPWaitRTCJumpNewStepID = "RSJ0000031";
        public const string NG_ForceReturnCSTWithDailyCheck_GotoStepNo_Is_Fail = "RSJ0000032";// qiumin 2016/12/12
        public const string NG_curBcsJobTurnFlag_Is_Null = "RSJ0000033"; // qiumin 2017/9/4
    }

    public class eJobRouteStepJump_ReturnMessage
    {
        public const string OK_Message = "";

    }

    public class eRobotStageStatus
    {
        //每次確認時的狀態
        public const string INIT_CHECK = "INITIAL";

        public const string SEND_OUT_READY = "UDRQ";

        public const string RECEIVE_READY = "LDRQ";

        public const string NO_REQUEST = "NOREQ";

        //20141218 add for Both Port
        public const string SEND_OUT_AND_RECEIVE_READY = "UDRQ_LDRQ";

        public eRobotStageStatus()
        {
        }
    }

    public class ePortJobUDRQReason
    {

        /// <summary>
        /// CSTSeq>0 , JobSeq>0 ,Exist=2,In RobotJob WIP
        /// </summary>
        public const string REASON_OK = "0";

        /// <summary>
        /// CSTSeq>0 , JobSeq>0 ,Exist=2,
        /// </summary>
        public const string JOB_NOT_INWIP = "1";

        /// <summary>
        /// CSTSeq>=0 , JobSeq>=0 ,Exist=2,有料無帳
        /// </summary>
        public const string JOBINFO_NOT_EXIST_JOB_EXIST = "2";

        /// <summary>
        /// CSTSeq>0 , JobSeq>0 ,Exist=1,有帳無料
        /// </summary>
        public const string JOBINFO_EXIST_JOB_NOT_EXIST = "3";

        /// <summary>
        /// //CSTSeq=0 , JobSeq=0 ,Exist=1
        /// </summary>
        public const string IS_EMPTY_SLOT = "4";

        public const string IS_EXCEPTION = "5";

        public const string OTHERS = "7";

        public const string CANNOT_FIND_ROUTE = "6";

        public ePortJobUDRQReason()
        {
        }
    }

    public class ePortSlotExistInfo
    {
        public const int JOB_EXIST = 1;
        public const int JOB_NO_EXIST = 0;
    }

    public class eLoaderPortSendOutStatus
    {

        public const string PORT_IN_PROCESS = "1";
        public const string PORT_WAIT_PROCESS = "2";
        public const string NOT_IN_PORT = "3";

        public eLoaderPortSendOutStatus()
        {
        }
    }

    public class eRepairPriority //Added by zhangwei 20161010 REP 机台IR 优先级高于RP
    {

        public const string INK_REPAIR = "2";
        public const string NORMAL_REPAIR = "1";
        public const string OTHER = "0";

        public eRepairPriority()
        {
        }
    }
    public class eRobotStageCSTType
    {
        //當Stage Type為'PORT'使用
        //'SEQUENCE': 代表Sequence Cassette，只能由下由上抽片，由上往下放片. 
        //'RANDOM':代表可以Randon抽片和放片.
        //其它值填入''.

        public const string WIRE_CST = "SEQUENCE";
        public const string RANDOM_CST = "RANDOM";
        public const string IS_NOT_PORT = "";

        public eRobotStageCSTType()
        {
        }
    }

    public class eUnloadPortReceiveStatus
    {
        //當同時有Unload and Borh Port可以收片時,定義優先考慮Both(以作回原CST依據) 
        public const string BOTH_PORT_IN_ABORTING = "1";
        public const string BOTH_PORT_IN_PROCESS = "2";
        public const string BOTH_PORT_WAIT_PROCESS = "3";
        public const string ULD_PORT_IN_PROCESS = "4";
        public const string ULD_PORT_WAIT_PROCESS = "5";
        public const string OTHERS = "6";

        public eUnloadPortReceiveStatus()
        {
        }
    }

    public class eRobot_ControlCommandStatus
    {
        public const string EMPTY = "EMPTY";

        public const string CREATE = "CREATE";

        public const string EQREPLY_OK = "EQREPLY_OK";

        public const string COMPLETE = "COMPLETE";

        public const string CANCEL = "CLEAR";

        public eRobot_ControlCommandStatus()
        {
        }
    }

    public class eRobot_RunMode
    {
        /// <summary>
        /// SEMI
        /// </summary>
        public const string SEMI_MODE = "SEMI";

        /// <summary>
        /// AUTO
        /// </summary>
        public const string AUTO_MODE = "AUTO";

        public eRobot_RunMode()
        {
        }

    }

    public class eRobot_SameEQFlag
    {
        /// <summary>
        /// Y
        /// </summary>
        public const string YES = "Y";
        /// <summary>
        /// N
        /// </summary>
        public const string NO = "N";
    }

    public class eRobot_HoldStatus
    {

        public const string RELEASE_STATUS = "0";

        public const string HOLD_STATUS = "1";

        public eRobot_HoldStatus()
        {
        }

    }

    /// <summary> SBRM_ROBOT_ROUTE_STEP中的RobotAction設定 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
    ///
    /// </summary>
    public class eRobot_DB_CommandAction
    {
        public const string ACTION_GET = "GET";
        public const string ACTION_PUT = "PUT";
        public const string ACTION_PUTREADY = "PUTREADY";
        public const string ACTION_GETREADY = "GETREADY";
        public const string ACTION_EXCHANGE = "EXCHANGE";
        public const string ACTION_GETPUT = "GETPUT";
        public const string ACTION_MULTI_GET = "MULTI_GET";
        public const string ACTION_MULTI_PUT = "MULTI_PUT";
        public const string ACTION_RTC_PUT = "RTC_PUT"; //20151229_001
        public const string ACTION_RECIPEGROUPEND_PUT = "RECIPEGROUPEND_PUT";  //20160511
        public const string ACTION_MULTIRECIPEGROUPEND_PUT = "MULTIRECIPEGROUPEND_PUT";  //20160511

        public eRobot_DB_CommandAction()
        {
        }

    }


    public class eRobot_Trx_CommandAction
    {
        //0: None
        //1: Put          //2: Get          //4: Exchange
        //8: Put Ready    //16: Get Ready   //32: Get/Put
        //64: Multi-Put   //128:Multi-Get   //256: RTC Put
        public const int ACTION_NONE = 0;
        public const int ACTION_PUT = 1;
        public const int ACTION_GET = 2;
        public const int ACTION_EXCHANGE = 4;
        public const int ACTION_PUTREADY = 8;
        public const int ACTION_GETREADY = 16;
        public const int ACTION_GETPUT = 32;
        public const int ACTION_MULTI_PUT = 64;
        public const int ACTION_MULTI_GET = 128;
        public const int ACTION_RTC_PUT = 256; //20151229_001
        public const int ACTION_RECIPEGROUPEND_PUT = 512;  //20160511
        public const int ACTION_MULTIRECIPEGROUPEND_PUT = 1024;  //20160511

        public eRobot_Trx_CommandAction()
        {
        }

    }

    public class eRobot_ArmSelect
    {
        public const int NONE = 0;
        public const int UPPER = 1;
        public const int LOWER = 2;
        public const int BOTH = 3;
    }

    public class eRobot_ControlCommand
    {
        public const int NONE = 0;
        public const int PUT = 1;
        public const int GET = 2;
        public const int EXCHANGE = 4;
        public const int PUT_READY = 8;
        public const int GET_READY = 16;
        public const int GET_PUT = 32;
        public const int MULTI_PUT = 64;
        public const int MULTI_GET = 128;
        public const int RTC_PUT = 256; //20151229_001
        public const int RECIPEGROUPEND_PUT = 512;  //20160511
        public const int MULTIRECIPEGROUPEND_PUT = 1024;  //20160511
    }

    public class eRobot_RouteProcessStatus
    {
        public const string INIT = "INIT";

        public const string WAIT_PROC = "WAIT";

        public const string INPROCESS = "PROCESS";

        public const string COMPLETE = "COMPLETE";

        public eRobot_RouteProcessStatus()
        {
        }
    }

    public class eLinekSignalDirect
    {
        public const string UPSTREAM = "UPSTREAM";
        public const string DOWNSTREAM = "DOWNSTREAM";

        public eLinekSignalDirect()
        {

        }
    }

    public class eRobotStageType
    {
        //DB Define
        //'PORT': for Cassette Port.
        //'STAGE': for Indexer inside Stage(such as VCR Table, Turn Table…)
        //'FIXBUFFER': for Indexer Fix Buffer.
        //'EQUIPMENT': for downstream or upstream Equipment.
        public const string PORT = "PORT";
        public const string STAGE = "STAGE";
        public const string FIXBUFFER = "FIXBUFFER";
        public const string EQUIPMENT = "EQUIPMENT";
        public const string ROBOTARM = "ROBOTARM";

        public eRobotStageType()
        {

        }
    }

    public class eDBRobotUseArmCode
    {
        //DB定義 
        //'UP':Upper Arm 
        //'LOW':Lower Arm
        //'ANY':Any Arm
        //'ALL':All Arm
        public const string UPPER_ARM = "UP";
        public const string LOWER_ARM = "LOW";

        /// <summary> All為定義上下Arm 同時作(BOTH)
        ///
        /// </summary>
        public const string ALL_ARM = "ALL";

        /// <summary> Any為定義任何一Arm即可
        /// 
        /// </summary>
        public const string ANY_ARM = "ANY";

        public const string NO_ARM_SELECT = "";// string.Empty;

        //20151228 add For Cell Special SPEC定義
        //0: None               //1: Upper/Left Arm    //2: Lower/Left Arm   //3: Left Both Arm 
        //4: Upper/Right Arm    //8: Lower/Right Arm   //12: Right Both Arm  //5: Upper Both Arm 10: Lower Both Arm
        public const string CELL_SPECIAL_UPPER_LEFT_ARM01 = "UP_LEFT";
        public const string CELL_SPECIAL_LOWER_LEFT_ARM02 = "LOW_LEFT";
        public const string CELL_SPECIAL_UPPER_RIGHT_ARM03 = "UP_RIGHT";
        public const string CELL_SPECIAL_LOWER_RIGHT_ARM04 = "LOW_RIGHT";
        public const string CELL_SPECIAL_BOTH_LEFT = "BOTH_LEFT";
        public const string CELL_SPECIAL_BOTH_RIGHT = "BOTH_RIGHT";
        public const string CELL_SPECIAL_BOTH_UPPER = "BOTH_UPPER";
        public const string CELL_SPECIAL_BOTH_LOWER = "BOTH_LOWER";

        public eDBRobotUseArmCode()
        {
        }

    }

    public class eSendToOPIMsgType
    {
        public const string NormalType = "Info";
        public const string AlarmType = "Error";
        public const string WarningType = "Warn";

        public eSendToOPIMsgType()
        {
        }

    }


    //public class eCVDProportionalRuleType
    //{
    //    public const string Normal = "1";
    //    public const string MQC = "2";
    //}

    // GlobalAssemblyVersion v1.0.0.26-20151015, added by dade
    /// <summary> Array shop / DRY line / DRY EQP 支持的 Receive Type (defined in Process Type of Job Data)
    /// DRY line (Array shop)
    ///    1. MQC Mode
    ///    2. PS Mode
    ///    3. GE Mode
    ///    4. ILD Mode
    ///    5. SD Mode
    ///    6. PV Mode
    ///    7. ASH Mode
    ///    8. PLN Mode
    ///
    ///    0. Product (NOREQ) 
    /// </summary>
    public class eDRYReceiveType
    {
        public const string _PRODUCT = "0";
        public const string _MQC = "1";
        public const string _PS = "2";
        public const string _GE = "3";
        public const string _ILD = "4";
        public const string _SD = "5";
        public const string _PV = "6";
        public const string _ASH = "7";
        public const string _PLN = "8";
    }

    public class eInvokeOPIFunction
    {
        public const string SendToOPI_RealTimeRobotCommandInfo = "RobotRealTimeRobotCommandReport";


    }

    public class eParameterXMLConstant
    {
        public const string TTP_DAILYCHECK_TRX = "TTP_DAILYCHECK_TRX";
        public const string CF_TTP_DAILYCHECK_TRX = "CF_TTP_DAILYCHECK_TRX";

        public const string CVD_LL1CLEANOUT_TRX = "CVD_LL1CLEANOUT_TRX";  
        public const string CVD_LL2CLEANOUT_TRX = "CVD_LL2CLEANOUT_TRX";  //Yang
    }

    public class eConstantXML
    {
        public const string CVD_ProportionalRule_Force = "CVD_ProportionalRule_Force";
    }

    //Watson Add 20151020
    public class eTTPEQPRunMode
    {
        public const string AGING_UNKNOW = "";
        public const string AGING_ENABLE = "Aging Enable";
        public const string AGING_DISABLE = "Aging Disable";
    }


    public class eTTPDailCheckGlassCSTSEQ
    {
        public const int Array_CSTSEQ = 5000;
        public const int CF_CSTSEQ_Max = 61000 ;
        public const int CF_CSTSEQ_Min = 60000;
    }


    public class eRobotStage_ExchangeType
    {
        public const string EXCHANGE = "EXCHANGE";
        public const string GETPUT = "GETPUT";
        public const string NOSUPPORT = "";
        //20160106 add for 新需求:如MAC设备中有Turn的基板，MAC不能要求Exchange，BC不能下达Exchange Command给Robot;
        public const string MAC_EXCHANGE = "MAC_EXCHANGE";

    }

    public class eRobotStage_RobotInterfaceType
    {
        public const string NORMAL = "NORMAL";
        //以下For MultiSlot
        public const string MULTI_SINGLE = "MULTI";
        public const string MULTI_DAUL = "BOTH";
        public const string GETGET_PUTPUT = "BOTHGETPUT";
    }

    //20151030 add for Robot UIService Retrun Code
    public class eRobotUIService_RetrunErrCode
    {
        public const string GET_JOB_FAIL = "R000001";
        public const string CHANGE_STEP_FAIL = "R000002";
        public const string GET_ROBOT_FAIL = "R000003";
        public const string RUNMODE_NOT_SEMI_STEP_CHNAGE_FAIL = "R000004";
        public const string ROBOTWIPCREATE_FAIL = "R000005";
    }

    //20151107 add Robot Step Type
    public class eRobotRouteStepRule
    {

        public const string ONLY = "ONLY";
        public const string SELECT = "SELECT";
        //public const string TRACKING = "TRACKING";
        //public const string SEQUENCE = "SEQUENCE";
        public const string ULDDISPATCH = "ULDDISPATCH";


        public eRobotRouteStepRule()
        {
        }
    }

    //20151209 add for Froce Retrun CST Without LDRQ Status
    public class eRobot_FroceRetrunCSTWithoutLDRQStatus
    {

        public const string IS_READY = "READY";

        public const string IS_START = "START";

        public const string IS_NOTCHECK = "NOTCHECK";

        public eRobot_FroceRetrunCSTWithoutLDRQStatus()
        {
        }
    }

    //20151227 add for Cell Special Slot Block JobExist Status
    public class eRobot_SlotBlock_JobsExistStatus
    {

        public const string FRONT_BACK_EMPTY = "FRONT_BACK_EMPTY";

        public const string FRONT_BACK_EXIST = "FRONT_BACK_EXIST";

        public const string FRONT_EMPTY_BACK_EXIST = "FRONT_EMPTY_BACK_EXIST";

        public const string FRONT_EXIST_BACK_EMPTY = "FRONT_EXIST_BACK_EMPTY";

        public eRobot_SlotBlock_JobsExistStatus()
        {
        }
    }

    //20151228 add for Cell Special Arm SlotNo
    /// <summary> UpperLeft = Arm#01, LowerLeft = Arm#02, UpperRight = Arm#03, LowerRight = Arm#04
    /// 
    /// </summary>
    public class eCellSpecialArmSlotNo
    {
        public const int Unknown = 0;
        public const int UpperLeft_Front = 1;
        public const int UpperLeft_Back = 2;
        public const int LowerLeft_Front = 3;
        public const int LowerLeft_Back = 4;
        public const int UpperRight_Front = 5;
        public const int UpperRight_Back = 6;
        public const int LowerRight_Front = 7;
        public const int LowerRight_Back = 8;

    }

    //Watson Add 20151229
    public class eCellQCREqpRunMode
    {
        public const string EQPRun_UNKNOW = "";
        public const string NORMAL = "NORMAL";
        public const string FLAGMODE = "FLAGMODE";
    }

    //20160108 add for Cell Unloader Dispatch Rule Match Grade Priority
    public class eCellUnloaderDispatchRuleMatchGradePriority
    {
        //Priority MATCH>EMP>MIX
        public const string IS_MATCH_PRIORITY = "1";
        public const string IS_EMP_PRIORITY = "2";
        public const string IS_MX_PRIORITY = "3";
        private const string UNOCHECK_PRIORITY = "9";

    }

    /// <summary>
    ///  Watson Add 20160202 Add GAP Run Mode Descrption
    ///  <item key ="1" value= "GAPANDSORTERMODE"/>
    ///  <item key ="2" value= "GMIANDSORTERMODE"/>
    /// </summary>
    public class eCellGAPEQPRunMode
    {
        public const string GAPANDSORTERMODE = "GAPANDSORTERMODE";
        public const string GMIANDSORTERMODE = "GMIANDSORTERMODE";
        public const string GAPANDGMIMODE = "GAPANDGMIMODE";
    }

    public class eCellPDREQPRunMode
    {
        public const string PDRANDSORTERMODE = "PDRANDSORTERMODE";
        public const string CEMANDSORTERMODE = "CEMANDSORTERMODE";
        public const string PDRANDCEMMODE = "PDRANDCEMMODE";
    }
}
