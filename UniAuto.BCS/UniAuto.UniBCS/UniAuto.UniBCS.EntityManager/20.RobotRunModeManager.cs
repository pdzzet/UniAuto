using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using System.IO;
using System.Data;
using UniAuto.UniBCS.Log;
using System.Reflection;

namespace UniAuto.UniBCS.EntityManager
{
    //public class RobotRunModeManager : EntityManager, IDataSource
    //{
    //    private Dictionary<string, RobotRunMode> _entities = new Dictionary<string, RobotRunMode>();

    //    private List<EntityData> _tempEntityDatas = null;

    //    #region 繼承EntityManager

    //    public override EntityManager.FILE_TYPE GetFileType()
    //    {
    //        return FILE_TYPE.BIN;
    //    }

    //    protected override string GetSelectHQL()
    //    {
    //        return string.Format("from RobotRunModeEntityData where SERVERNAME = '{0}'", BcServerName);
    //    }

    //    protected override Type GetTypeOfEntityData()
    //    {
    //        return typeof(RobotRunModeEntityData);
    //    }

    //    //產生序列化文件
    //    protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
    //    {
    //        Filenames = new List<string>();
    //        _tempEntityDatas = EntityDatas;

    //        foreach (EntityData entity_data in EntityDatas)
    //        {
    //            RobotRunModeEntityData robot_entity_data = entity_data as RobotRunModeEntityData;
    //            if (robot_entity_data != null)
    //            {
    //                //以ServerName+RUNMODENAME為檔名
    //                string file_name = string.Format("{0}_{1}.bin", robot_entity_data.SERVERNAME, robot_entity_data.RUNMODENAME);
    //                Filenames.Add(file_name);
    //            }
    //        }

    //    }

    //    protected override Type GetTypeOfEntityFile()
    //    {
    //        return typeof(RobotRunModeEntityFile);
    //    }

    //    //確認檔案是否存在,不存在就新建
    //    protected override EntityFile NewEntityFile(string Filename)
    //    {
    //        return new RobotRunModeEntityFile();
            
    //    }

    //    //以RunModeName 存放在Dic
    //    protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
    //    {
    //        string mapKey = string.Empty;

    //        foreach (EntityData entity_data in entityDatas)
    //        {
    //            RobotRunModeEntityData robotRunMode_entity_data = entity_data as RobotRunModeEntityData;

    //            if (robotRunMode_entity_data != null)
    //            {
    //                foreach (EntityFile entity_file in entityFiles)
    //                {
    //                    RobotRunModeEntityFile robotRunMode_entity_file = entity_file as RobotRunModeEntityFile;

    //                    if (robotRunMode_entity_file != null)
    //                    {

    //                        string fextname = robotRunMode_entity_file.GetFilename();
    //                        string fname = Path.GetFileNameWithoutExtension(fextname);
    //                        //Check Dic Key //以ServerName+RUNMODENAME為檔名
    //                        mapKey = string.Format("{0}_{1}", robotRunMode_entity_data.SERVERNAME, robotRunMode_entity_data.RUNMODENAME);

    //                        if (string.Compare(mapKey, fname, true) == 0)
    //                            _entities.Add(mapKey, new RobotRunMode(robotRunMode_entity_data, robotRunMode_entity_file));
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public List<RobotRunMode> GetRobotRunModes()
    //    {
    //        List<RobotRunMode> ret = new List<RobotRunMode>();
    //        foreach (RobotRunMode entity in _entities.Values)
    //        {
    //            ret.Add(entity);
    //        }
    //        return ret;
    //    }

    //    //提供給外部取得Dt做Show在BC上
    //    public System.Data.DataTable GetDataTable()
    //    {
    //        try
    //        {
    //            DataTable dt = new DataTable();
    //            RobotRunModeEntityData data = new RobotRunModeEntityData();
    //            RobotRunModeEntityFile file = new RobotRunModeEntityFile();
    //            DataTableHelp.DataTableAppendColumn(data, dt);
    //            DataTableHelp.DataTableAppendColumn(file, dt);

    //            List<RobotRunMode> robotRunMode_entities = GetRobotRunModes();

    //            foreach (RobotRunMode entity in robotRunMode_entities)
    //            {
    //                DataRow dr = dt.NewRow();
    //                DataTableHelp.DataRowAssignValue(entity.Data, dr);
    //                DataTableHelp.DataRowAssignValue(entity.File, dr);
    //                dt.Rows.Add(dr);
    //            }
    //            return dt;
    //        }
    //        catch (System.Exception ex)
    //        {
    //            NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            return null;
    //        }

    //    }

    //    #endregion

    //}
}
