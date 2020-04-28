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
    public class ProductIDManager : EntityManager, IDataSource
    {
        private ProductIDConstant _ProductIDS;

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
            Filenames.Add("ProductID.xml");
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(ProductIDConstant);
        }

        protected override Entity.EntityFile NewEntityFile(string Filename)
        {
            return new ProductIDConstant();
        }

        protected override void AfterInit(List<Entity.EntityData> entityDatas, List<Entity.EntityFile> entityFiles)
        {
            if (entityFiles.Count() > 0)
            {
                _ProductIDS = entityFiles[0] as ProductIDConstant;
            }
            else
                _ProductIDS = new ProductIDConstant();
        }

        public IList<ProductID> GetProductIDs()
        {
            return _ProductIDS.ProductIDCollection;
        }

        public void UpdateLastUseTime(ProductID type)
        {
            type.LastUseTime = DateTime.Now;
            EnqueueSave(_ProductIDS);
        }

        public void AddNewProductID(ProductID type)
        {
            _ProductIDS.ProductIDCollection.Add(type);

            _ProductIDS.ProductIDCollection.Sort((a, b) =>
            {
                if (a.Value > b.Value)
                    return 1;
                else if (a.Value == b.Value)
                    return 0;
                else
                    return -1;
            });
            EnqueueSave(_ProductIDS);
        }


        public void ReplaceNewProductID(Job job, List<ProductID> types)
        {
            List<ProductID> lst = types.OrderBy(p => p.LastUseTime).ToList<ProductID>();

            job.ProductID.Value = lst[0].Value;
            lst[0] = job.ProductID;
            lst[0].LastUseTime = DateTime.Now;
            EnqueueSave(_ProductIDS);
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("PlanManager");
            return entityNames;
        }

        public DataTable GetDataTable(string entityNames)
        {
            try
            {

                DataTable dt = new DataTable();

                ProductID file = new ProductID();

                DataTableHelp.DataTableAppendColumn(file, dt);


                foreach (ProductID entity in _ProductIDS.ProductIDCollection)
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
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ProductID GetProductID(int value) {
            if (_ProductIDS.ProductIDCollection.Count > 0) {

                ProductID productID = _ProductIDS.ProductIDCollection.Find(p => p.Value == value);
                return productID;
            }
            return null;
        }
    }
}
