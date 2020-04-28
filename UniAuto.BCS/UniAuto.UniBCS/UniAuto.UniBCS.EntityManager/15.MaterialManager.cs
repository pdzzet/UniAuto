using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;

namespace UniAuto.UniBCS.EntityManager
{
    public class MaterialManager : EntityManager, IDataSource
    {
        private Dictionary<string, MaterialEntity> _entities = new Dictionary<string, MaterialEntity>(); //NodeNo+UnitNo+MaterialPort+MaterialID
        //20141220 cy:增加for mask的集合
        private Dictionary<string, MaterialEntity> _entitiesMask = new Dictionary<string, MaterialEntity>(); //key:NodeNo+MaterialSlotNo+MaterialType
        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.XML;
        }
        protected override string GetSelectHQL()
        {
            return string.Empty;
        }
        protected override Type GetTypeOfEntityData()
        {
            return null;
        }
        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            Filenames.Add("*.xml");
        }
        protected override Type GetTypeOfEntityFile()
        {
            return typeof(MaterialEntity);
        }
        protected override EntityFile NewEntityFile(string Filename)
        {
            return new MaterialEntity();
        }
        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            foreach (EntityFile entity_file in entityFiles)
            {
                MaterialEntity material_entity = entity_file as MaterialEntity;
                if (material_entity != null)
                {
                    //20141220 cy:增加mask type的集合
                    if (material_entity.EQType == eMaterialEQtype.MaskEQ)
                    {
                        string mask_key = string.Format("{0}_{1}_{2}", material_entity.NodeNo, material_entity.MaterialSlotNo, material_entity.MaterialType);
                        if (!_entitiesMask.ContainsKey(mask_key))
                        {
                            _entitiesMask.Add(mask_key, material_entity);
                        }
                    }
                    else
                    {
                        string material_key = string.Format("{0}_{1}_{2}_{3}", material_entity.NodeNo, material_entity.UnitNo, material_entity.MaterialPort, material_entity.MaterialID);
                        if (!_entities.ContainsKey(material_key))
                        {
                            _entities.Add(material_key, material_entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 將Material加入Dictionary
        /// Mount Material
        /// </summary>
        /// <param name="Job">若Unit No有重複, 會覆蓋</param>
        public void AddMaterial(MaterialEntity material)
        {
            lock (_entities)
            {
                string materialKey = string.Format("{0}_{1}_{2}_{3}", material.NodeNo, material.UnitNo, material.MaterialPort, material.MaterialID);
                if (!_entities.ContainsKey(materialKey))
                {
                    _entities.Add(materialKey, material);
                }
                else
                {
                    _entities.Remove(materialKey);
                    _entities.Add(materialKey, material);
                }
                //material.SetFilename(string.Format("{0}.xml", materialKey));
                material.SetFilename(string.Format("{0}.{1}", materialKey,GetFileExtension()));
                EnqueueSave(material);
            }
        }
        
        /// <summary>
        /// 將Material從Dictionary移除
        /// </summary>
        /// <param name="material"></param>
        public void DeleteMaterial(MaterialEntity material)
        {
            lock (_entities)
            {
                string materialKey = string.Format("{0}_{1}_{2}_{3}", material.NodeNo, material.UnitNo, material.MaterialPort, material.MaterialID);
                if (_entities.ContainsKey(materialKey))
                {
                    material.WriteFlag = false;
                    _entities.Remove(materialKey);
                    EnqueueSave(material);
                }
            }
        }

        /// <summary>
        /// 將Mask加入Dictionary
        /// </summary>
        /// <param name="material"></param>
        public void AddMask(MaterialEntity material)
        {
            lock (_entitiesMask)
            {
                string materialKey = string.Format("{0}_{1}_{2}", material.NodeNo, material.MaterialSlotNo, material.MaterialType);
                if (!_entitiesMask.ContainsKey(materialKey))
                {
                    _entitiesMask.Add(materialKey, material);
                }
                else
                {
                    _entitiesMask.Remove(materialKey);
                    _entitiesMask.Add(materialKey, material);
                }
                material.SetFilename(string.Format("{0}.xml", materialKey));
                EnqueueSave(material);
            }
        }

        public void DeleteMask(MaterialEntity material)
        {
            lock (_entitiesMask)
            {
                string materialKey = string.Format("{0}_{1}_{2}", material.NodeNo, material.MaterialSlotNo, material.MaterialType);
                if (_entities.ContainsKey(materialKey))
                {
                    material.WriteFlag = false;
                    _entities.Remove(materialKey);
                    EnqueueSave(material);
                }
            }
        }

        /// <summary>
        /// 取得line Material 以List 方式傳回
        /// </summary>
        /// <returns>Job List</returns>
        public List<MaterialEntity> GetMaterials()
        {
            List<MaterialEntity> ret = null;
            lock (_entities)
            {
                ret = _entities.Values.ToList();
            }
            return ret;
        }
        /// <summary>
        /// 取得Line Mask, 以List方式傳回
        /// </summary>
        /// <returns></returns>
        public List<MaterialEntity> GetMasks()
        {
            List<MaterialEntity> ret = null;
            lock (_entitiesMask)
            {
                ret = _entitiesMask.Values.ToList();
            }
            return ret;
        }

        /// <summary>
        /// 取得單一Material,以Slot No取得
        /// </summary>
        /// <param name="EQP No"></param>
        /// <param name="slotNo"></param>
        /// <returns>MaterialEntity</returns>
        public MaterialEntity GetMaterialBySlot(string eqpNo, string slotNo)
        {
            try
            {
                foreach (KeyValuePair<string, MaterialEntity> kvp in _entities)
                {
                    if (kvp.Value.NodeNo.ToUpper() == eqpNo.ToUpper() && kvp.Value.MaterialSlotNo == slotNo)
                        return kvp.Value;
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }
        public MaterialEntity GetMaskBySlot(string eqpNo, string slotNo)
        {
            try
            {
                string maskKey = string.Format("{0}_{1}_MASK", eqpNo, slotNo);
                if (_entitiesMask.ContainsKey(maskKey))
                    return _entitiesMask[maskKey];
                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public MaterialEntity GetMaskBySlot(string eqpNo, string slotNo, string type)
        {
            try
            {
                string maskKey = string.Format("{0}_{1}_{2}", eqpNo, slotNo, type);
                if (_entitiesMask.ContainsKey(maskKey))
                    return _entitiesMask[maskKey];
                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public MaterialEntity GetMaterialByName(string eqpNo, string name)
        {
            try
            {
                foreach (KeyValuePair<string, MaterialEntity> kvp in _entities)
                {
                    if (kvp.Value.NodeNo.ToUpper() == eqpNo.ToUpper() && kvp.Value.MaterialID == name)
                        return kvp.Value;
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }
        public MaterialEntity GetMaskByName(string eqpNo, string name)
        {
            try
            {
                foreach (KeyValuePair<string, MaterialEntity> kvp in _entitiesMask)
                {
                    if (kvp.Value.NodeNo.ToUpper() == eqpNo.ToUpper() && kvp.Value.MaterialID == name)
                        return kvp.Value;
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public List<MaterialEntity> GetMasksByName(string eqpNo, string name)
        {
            try
            {
                List<MaterialEntity> ret = new List<MaterialEntity>();
                foreach (KeyValuePair<string, MaterialEntity> kvp in _entitiesMask)
                {
                    if (kvp.Value.NodeNo.ToUpper() == eqpNo.ToUpper() && kvp.Value.MaterialID == name)
                        ret.Add(kvp.Value);
                }
                return ret;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

		public MaterialEntity GetMaterialByKey(string key) {
			try {				
				if (this._entities.ContainsKey(key)) {
					return this._entities[key];
				}
				return null;
			} catch (Exception ex) {
				Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				return null;
			}
		}

        /// <summary>
        ///將Material從Dictionary去掉
        /// UnMount Material
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public bool UnMountMaterial(MaterialEntity material)
        {
            try
            {
                string materialKey = string.Format("{0}_{1}_{2}_{3}", material.NodeNo, material.UnitNo, material.MaterialPort, material.MaterialID);
                if (_entities.ContainsKey(materialKey))
                {
                    _entities.Remove(materialKey);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        public bool UnMountMask(MaterialEntity material)
        {
            try
            {
                string maskKey = string.Format("{0}_{1}_{2}", material.NodeNo, material.MaterialSlotNo, material.MaterialType);
                if (_entitiesMask.ContainsKey(maskKey))
                {
                    _entitiesMask.Remove(maskKey);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public bool UnMountMaterial(string eqpNo, string unitNo, string materialPort, string materialID)
        {
            try
            {
                string materialKey = string.Format("{0}_{1}_{2}_{3}", eqpNo, unitNo, materialPort, materialID);
                if (_entities.ContainsKey(materialKey))
                {
                    _entities.Remove(materialKey);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        public bool UnMountMask(string eqpNo, string slotNo)
        {
            try
            {
                string maskKey = string.Format("{0}_{1}_MASK", eqpNo, slotNo);
                if (_entitiesMask.ContainsKey(maskKey))
                {
                    _entitiesMask.Remove(maskKey);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public bool UnMountMask(string eqpNo, string slotNo, string type)
        {
            try
            {
                string maskKey = string.Format("{0}_{1}_{2}", eqpNo, slotNo, type);
                if (_entitiesMask.ContainsKey(maskKey))
                {
                    _entitiesMask.Remove(maskKey);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// MaterialHistory 
        /// </summary>
        /// <param name="equipmentID"></param>
        /// <param name="unitNo"></param>
        /// <param name="currentMaterial"></param>
        /// <param name="oldMaterialId"></param>
        public void RecordMaterialHistory(string equipmentID, string unitNo, MaterialEntity currentMaterial, string oldMaterialId, string transactionID,string permitCode="",string count="0")
        {
            try
            {
                MATERIALHISTORY materialHistory = new MATERIALHISTORY()
                {
                    UPDATETIME = DateTime.Now,
                    NODEID = equipmentID,
                    UNITNO = unitNo,
                    MATERIALID = currentMaterial.MaterialID,
                    MATERIALCOUNT = count,
                    MATERIALSTATUS = currentMaterial.MaterialStatus.ToString(),
                    MATERIALTYPE = currentMaterial.MaterialType,
                    SLOTNO = currentMaterial.MaterialSlotNo,
                    OPERATORID = currentMaterial.OperatorID,
                    PERMITCODE = permitCode,
                    OLDMATERIALID = oldMaterialId,
                    TRANSACTIONID = transactionID
                };
                HibernateAdapter.SaveObject(materialHistory);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //Add By yangzhenteng/20171024/Only For Cell-PIC
        public void RecordMaterialHistory_PIC(string equipmentID, string unitNo, MaterialEntity currentMaterial, string oldMaterialId, string transactionID, string recipeID, string materialWeight)
        {
            try
            {
                MATERIALHISTORY materialHistory = new MATERIALHISTORY()
                {
                    UPDATETIME = DateTime.Now,
                    NODEID = equipmentID,
                    UNITNO = unitNo,
                    MATERIALID = currentMaterial.MaterialID,
                    MATERIALCOUNT = materialWeight,
                    MATERIALSTATUS = currentMaterial.MaterialStatus.ToString()+"_W",
                    MATERIALTYPE = currentMaterial.MaterialType,
                    SLOTNO = currentMaterial.MaterialSlotNo,
                    OPERATORID = currentMaterial.OperatorID,
                    PERMITCODE = recipeID,
                    OLDMATERIALID = oldMaterialId,
                    TRANSACTIONID = transactionID
                };
                HibernateAdapter.SaveObject(materialHistory);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("MaterialManager");
          
            return entityNames;
        }

   


        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                MaterialEntity file = new MaterialEntity();
                DataTableHelp.DataTableAppendColumn(file, dt);

                List<MaterialEntity> materials = GetMaterials();
                foreach (MaterialEntity entity in materials)
                {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(entity, dr);
                    dt.Rows.Add(dr);
                }
                List<MaterialEntity> masks = GetMasks();
                foreach (MaterialEntity entity in masks)
                {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(entity, dr);
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                return null;
            }
        }
    }
}
