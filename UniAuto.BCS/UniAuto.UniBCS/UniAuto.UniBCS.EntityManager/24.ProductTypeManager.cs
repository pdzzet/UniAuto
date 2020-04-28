using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Collections;
using UniAuto.UniBCS.DB;
using System.Data;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.EntityManager
{
    public class ProductTypeManager : EntityManager,IDataSource
    {
        private ProductTypeConstant _pRODUCTTYPES;

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

        protected override void AfterSelectDB(List<Entity.EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            Filenames.Add("ProductType.xml");
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(ProductTypeConstant);
        }

        protected override Entity.EntityFile NewEntityFile(string Filename)
        {
            return new ProductTypeConstant();
        }

        protected override void AfterInit(List<Entity.EntityData> entityDatas, List<Entity.EntityFile> entityFiles)
        {
            if (entityFiles.Count() > 0)
            {
                _pRODUCTTYPES = entityFiles[0] as ProductTypeConstant;
            }
            else
                _pRODUCTTYPES = new ProductTypeConstant();
        }

        public IList<ProductType> GetProductTypes()
        {
            return _pRODUCTTYPES.ProductTypeCollection;
        }

        public void UpdateLastUseTime(ProductType type)
        {
            type.LastUseTime = DateTime.Now;
            EnqueueSave(_pRODUCTTYPES);
        }

        public void AddNewProductType(ProductType type)
        {
            _pRODUCTTYPES.ProductTypeCollection.Add(type);

            _pRODUCTTYPES.ProductTypeCollection.Sort((a, b) =>
                {
                    if (a.Value > b.Value)
                        return 1;
                    else if (a.Value == b.Value)
                        return 0;
                    else
                        return -1;
                });
            EnqueueSave(_pRODUCTTYPES);
        }


        public void ReplaceNewProductType(Job job, List<ProductType> types)
        {
            List<ProductType> lst = types.OrderBy(p => p.LastUseTime).ToList<ProductType>();

            job.ProductType.Value = lst[0].Value;
            lst[0] = job.ProductType;
            lst[0].LastUseTime = DateTime.Now;
            EnqueueSave(_pRODUCTTYPES);
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("ProductTypeManager");
            return entityNames;
        }

        public DataTable GetDataTable(string entityName)
        {
            try
            {

                DataTable dt = new DataTable();

                ProductType file = new ProductType();

                DataTableHelp.DataTableAppendColumn(file, dt);


                foreach (ProductType entity in _pRODUCTTYPES.ProductTypeCollection )
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

             /// <summary>
        /// 找到Product Type
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public ProductType GetProductType(int v) {

            if(_pRODUCTTYPES.ProductTypeCollection.Count>0){

                ProductType productTyp =_pRODUCTTYPES.ProductTypeCollection.Find(p=>p.Value==v);
                return productTyp;
            }
            return null;
            
        }
        
    }
}
