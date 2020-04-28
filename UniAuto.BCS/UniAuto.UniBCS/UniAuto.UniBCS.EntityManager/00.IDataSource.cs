using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;

namespace UniAuto.UniBCS.EntityManager
{
    public interface IDataSource
    {
        DataTable GetDataTable(string entityName);

        IList<string> GetEntityNames();
    }

    public static class DataTableHelp
    {
        public static void DataTableAppendColumn(object obj, DataTable dataTable)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType != typeof(ICollection))
                    dataTable.Columns.Add(prop.Name, typeof(string));
            }
        }

        public static void DataRowAssignValue(object obj, DataRow dataRow)
        {

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType != typeof(ICollection))
                {
                    object val = prop.GetValue(obj, null);
                    if (val != null)
                    {
                        dataRow[prop.Name] = val.ToString();
                    }
                    else
                    {
                        dataRow[prop.Name] = "";
                    }
                }
            }
        }

    }
}
