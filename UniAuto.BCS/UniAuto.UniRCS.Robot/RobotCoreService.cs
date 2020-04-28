using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Threading;
using System.Reflection;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;

// 在 RobotCoreService 需完成以下
// 1. Message Mapping 需要的 Method
// 2. 被其他 Service Invoke 的 Method
// 3. Reload DLL同步控制
// 

namespace UniAuto.UniRCS.RobotService
{
    public class RobotCoreService : AbstractService
    {
        /// <summary>
        /// 獨立執行緒, 執行 Robot DLL
        /// </summary>
        private Thread m_Thread = null;

        /// <summary>
        /// 
        /// </summary>
        private ReaderWriterLockSlim m_RWL = new ReaderWriterLockSlim();

        /// <summary>
        /// 
        /// </summary>
        private AbstractRobotProcess m_RobotProcess = null;

        /// <summary>
        /// 決定 Robot DLL 載入方式, 由Spring讀Config並給值
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Robot DLL Filename, 由Spring讀Config並給值
        /// </summary>
        public string RobotDLLFilename { get; set; }
        
        public override bool Init()
        {
            return true;
        }

        public void Destroy()
        {
            m_RWL.Dispose();
        }

        public FrmRobot.DLLInfo ReadDLLInfo()
        {
            FrmRobot.DLLInfo ret = new FrmRobot.DLLInfo();
            string fname = string.Format(@".\{0}", RobotDLLFilename);
            FileInfo info = new FileInfo(fname);
            ret.Filename = RobotDLLFilename;
            ret.Exists = info.Exists;
            if (info.Exists)
            {
                FileVersionInfo version_info = FileVersionInfo.GetVersionInfo(fname);
                ret.Version = version_info.FileVersion;
            }
            return ret;
        }

        public string LoadDLL()
        {
            string err_msg = string.Empty;
            try
            {
                string fname = string.Format(@".\{0}", RobotDLLFilename);
                FileInfo info = new FileInfo(fname);
                if (info.Exists)
                {
                    Assembly assembly = null;
                    if (Debug)
                    {
                        //載入Rule DLL檔，可在Debug時停中斷點
                        //因DLL檔已載入，故更新Robot DLL前必需關閉BCS主程式
                        assembly = Assembly.Load(fname);
                    }
                    else
                    {
                        //讀取Robot DLL的資料後釋放DLL檔，將讀到的資料動態載入程式碼
                        //因DLL檔已經釋放，故可以在不關閉BCS主程式的情況下更新Robot DLL
                        //只需將Robot DLL檔案覆蓋，重新LoadDLL即可
                        //因DLL檔已釋放，故無法在Debug時停中斷點
                        byte[] buf = System.IO.File.ReadAllBytes(fname);
                        assembly = Assembly.Load(buf);
                    }

                    Type robot_process_type = null;
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type == typeof(AbstractRobotProcess))
                        {
                            if (robot_process_type == null)
                                robot_process_type = type;
                            else
                                throw new Exception("");
                        }
                    }

                    if (robot_process_type != null)
                    {
                        m_RobotProcess = Activator.CreateInstance(robot_process_type) as AbstractRobotProcess;
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
            }
            return err_msg;
        }
    }
}
