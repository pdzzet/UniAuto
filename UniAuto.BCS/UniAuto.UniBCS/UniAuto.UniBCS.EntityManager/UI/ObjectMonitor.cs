using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

namespace UniAuto.UniBCS.EntityManager.UI
{
    public partial class ObjectMonitor : Form
    {
        public static BindingFlags m_BindingFlags = BindingFlags.Default;
        public ObjectMonitor()
        {
            InitializeComponent();
        }

        public object SelectObject
        {
            get;
            set;

        }

        public string ObjectName { get; set; }

        //private void Refresh(string rootKey)
        //{
        //    try
        //    {
        //        TreeNode mainRoot = new TreeNode();
        //        mainRoot.Nodes.Add(ObjectName);
        //        ObjRefresh(SelectObject);
        //        TreeNode clearNode = null;
        //        AddTreeviewNode(clearNode, true);
        //        for (int i = 0; i < mainRoot.Nodes.Count; ++i)
        //        {
        //            AddTreeviewNode(mainRoot.Nodes[i], false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write(ex);
        //    }
        //}

        /// <summary>
        /// 搜尋物件各欄位變數(PS:屬性是欄位與方法的綜合體)
        /// </summary>
        /// <param name="container">物件要存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="cls">要搜尋的物件實體</param>
        /// <param name="key">該物件實體命名</param>
        private void SearchObjData(Dictionary<string, object> container, TreeNode root, object cls, string key)
        {
            m_BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

            checkClass(container, root, cls, key);
        }
        /// <summary>
        /// 查詢指定物件的欄位資料
        /// </summary>
        /// <param name="key">依key來決定要找詢的物件</param>
        public void ObjRefresh(object obj)
        {
            try
            {
                    TreeNode clearNode = null;
                    AddTreeviewNode(clearNode, true);

                    // 暫存的泛型資料容器
                    Dictionary<string, object> nowDataContainer = new Dictionary<string, object>();
                    nowDataContainer.Clear();

                    TreeNode nowNodeData = new TreeNode();
                   
                    nowNodeData.Nodes.Add(ObjectName);
                   
                    SearchObjData(nowDataContainer, nowNodeData.LastNode, obj, ObjectName);
                  
                    //treeViewMonitor.Nodes.Add(nowNodeData.LastNode);
                    AddTreeviewNode(nowNodeData.LastNode, false);

                    nowDataContainer.Clear();

                

            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                //MessageBox.Show(ex.Message, "ObjRefresh Exception", MessageBoxButtons.OK);
            }
          
        }
        /// <summary>
        /// 解析泛型資料型態
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="container"></param>
        /// <param name="node"></param>
        /// <param name="data"></param>
        /// <param name="preName"></param>
        public static void GetDictionaryContents<TKey, TValue>(Dictionary<string, object> container, TreeNode node, IDictionary<TKey, TValue> data, string preName)
        {
            if (data == null) return;
            foreach (var pair in data)
            {
                string tempName = preName + @"\" + pair.Key.ToString();

                if (pair.Value.GetType().IsClass && pair.Value.GetType() != typeof(string))
                {
                    node.Nodes.Add(pair.Key.ToString() + "(ValueType : " + pair.Value.GetType().ToString() + ")");
                    //checkClass(container, node.Nodes[node.Nodes.Count - 1], (object)pair.Value, tempName);
                    if (!(pair.Value.GetType().IsSecurityTransparent)) 
                        checkClass(container, node.Nodes[node.Nodes.Count - 1], (object)pair.Value, tempName);
                }
                else
                {
                    node.Nodes.Add(pair.Key.ToString() + " = " + pair.Value.ToString());
                }
            }
        }

        private void Init()
        {
           
        }

        private void ObjectMonitor_Load(object sender, EventArgs e)
        {
            ObjRefresh(SelectObject);
        }

        public delegate void AddTreeviewCallBack(TreeNode node, bool isReset);
        public void AddTreeviewNode(TreeNode node, bool isReset)
        {
            if (treeViewMonitor.InvokeRequired)
            {
                AddTreeviewCallBack setProCallback = new AddTreeviewCallBack(AddTreeviewNode);
                treeViewMonitor.Invoke(setProCallback, new object[] { node, isReset });
            }
            else
            {
                if (isReset)
                {
                    treeViewMonitor.Nodes.Clear();
                }
                else
                {
                    if (node != null) treeViewMonitor.Nodes.Add(node);
                }
            }
        }

        #region Static Method
        /// <summary>
        /// 解析陣列內各元素的欄位資料
        /// </summary>
        /// <param name="container">物件存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="data">陣列內各元素欄位</param>
        /// <param name="cls"></param>
        /// <param name="preName"></param>
        private static void checkArray(Dictionary<string, object> container, TreeNode root, FieldInfo data, object cls, string preName)
        {
            try
            {

                Array arr = data.GetValue(cls) as Array;
                if (arr == null)
                {
                    root.Text = data.Name + " = NULL";
                    return;
                }
                else { ; }

                ArrayList arrayList = new ArrayList(arr);
                if (data.FieldType == typeof(string[]))
                {
                    for (int i = 0; i < arrayList.Count; ++i)
                    {
                        root.Nodes.Add(data.Name + "[" + i + "] = " + arrayList[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < arrayList.Count; ++i)
                    {
                        if (arrayList[i].GetType().IsClass)
                        {
                            string tempName = preName + @"\" + data.Name + "[" + i + "]";
                            root.Nodes.Add(data.Name + "[" + i + "]");
                            if (arrayList[i].GetType().IsSecurityTransparent) continue;
                            checkClass(container, root.Nodes[root.Nodes.Count - 1], arrayList[i], tempName);
                        }
                        else
                        {
                            root.Nodes.Add(data.Name + "[" + i + "] = " + arrayList[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        /// <summary>
        /// 解析陣列物件實體
        /// </summary>
        /// <param name="container">物件存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="cls">物件實體</param>
        /// <param name="preName">物件實體的名稱</param>
        private static void checkArray(Dictionary<string, object> container, TreeNode root, object cls, string preName)
        {
            try
            {


                if (!container.ContainsKey(preName)) container.Add(preName, cls);

                Array arr = cls as Array;
                if (arr == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }
                ArrayList arrayList = new ArrayList(arr);
                preName = preName.Substring(preName.LastIndexOf(@"\") + 1);
                for (int i = 0; i < arrayList.Count; ++i)
                {
                    if (arrayList[i].GetType().IsClass)
                    {
                        if (arrayList[i].GetType() == typeof(string))
                        {
                            root.Nodes.Add(preName + "[" + i + "] = " + arrayList[i]);
                        }
                        else
                        {
                            root.Nodes.Add(preName + "[" + i + "]");
                            if (arrayList[i].GetType().IsSecurityTransparent) continue;
                            checkClass(container, root.LastNode, arrayList[i], arrayList[i].ToString() + "[" + i + "]");
                        }
                    }
                    else
                    {
                        root.Nodes.Add(preName + "[" + i + "] = " + arrayList[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        /// <summary>
        /// 解析泛型容器List物件實體
        /// </summary>
        /// <param name="container">物件存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="cls">物件實體</param>
        /// <param name="preName">物件實體的名稱</param>
        private static void checkList(Dictionary<string, object> container, TreeNode root, object cls, string preName)
        {
            try
            {
                if (cls == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }

                if (!container.ContainsKey(preName)) container.Add(preName, cls);

                IList lst = cls as IList;
                string modifyListName = preName.Substring(preName.LastIndexOf(@"\") + 1);

                for (int i = 0; i < lst.Count; ++i)
                {
                    if (lst[i].GetType().IsClass)
                    {
                        if (lst[i].GetType() == typeof(string))
                        {
                            root.Nodes.Add(modifyListName + "[" + i + "] = " + lst[i]);
                        }
                        else
                        {
                            string lstSubName = preName + "\\" + modifyListName + "[" + i + "]";
                            root.Nodes.Add(modifyListName + "[" + i + "]");
                            if (lst[i].GetType().IsSecurityTransparent) continue;
                            checkClass(container, root.LastNode, lst[i], lstSubName);
                        }
                    }
                    else
                    {
                        root.Nodes.Add(modifyListName + "[" + i + "] = " + lst[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private static void checkQueue(Dictionary<string, object> container, TreeNode root, object cls, string preName)
        {
            try
            {
                if (cls == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }

                if (!container.ContainsKey(preName)) container.Add(preName, cls);

                IEnumerable queData = cls as IEnumerable;
                string modifyQueueName = preName.Substring(preName.LastIndexOf(@"\") + 1);
                int queId = 0;
                foreach (var value in queData)
                {
                    if (value.GetType().IsClass)
                    {
                        if (value.GetType() == typeof(string))
                        {
                            root.Nodes.Add(modifyQueueName + "[" + queId + "] = " + value.ToString());
                        }
                        else
                        {
                            string queSubName = preName + "\\" + modifyQueueName + "[" + queId + "]";

                            //container.Add(queSubName, value);
                            root.Nodes.Add(modifyQueueName + "[" + queId + "]");
                            if (value.GetType().IsSecurityTransparent) continue;
                            checkClass(container, root.LastNode, value, queSubName);
                        }
                    }
                    else
                    {
                        root.Nodes.Add(modifyQueueName + "[" + queId + "] = " + value.ToString());
                    }
                    queId += 1;
                    //Console.WriteLine(value.GetType().IsClass);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        /// <summary>
        /// 查詢泛型容器內含資料
        /// </summary>
        /// <param name="container">物件存放的泛型容器</param>
        /// <param name="root"></param>
        /// <param name="cls"></param>
        private static void checkDictionary(Dictionary<string, object> container, TreeNode root, object cls, string preName)
        {
            if (cls == null)
            {
                root.Text = preName + " = NULL";
                return;
            }
            else { ; }

            foreach (Type iType in cls.GetType().GetInterfaces())
            {
               
                typeof(ObjectMonitor).GetMethod("GetDictionaryContents").MakeGenericMethod(iType.GetGenericArguments()).Invoke(null, new object[] { container, root, cls, preName });
                //GetDictionaryContents()
                break;
            }
        }

        /// <summary>
        /// 搜尋出指定物件所有公開及靜態欄位
        /// </summary>
        /// <param name="container">物件要存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="cls">要搜尋的物件實體</param>
        /// <param name="preName">所屬何物件的命名</param>
        private static void checkClass(Dictionary<string, object> container, TreeNode root, object cls, string preName)
        {
            try
            {
                if (cls == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }

                container.Add(preName, cls);

                Type clsBaseType = cls.GetType().BaseType;
                Type clsNowType;

                if (clsBaseType != typeof(object) && clsBaseType != typeof(Form))
                {
                    if (clsBaseType == typeof(Array))
                    {
                        // 查詢已記綠的陣列物件
                        checkArray(container, root, cls, preName);
                    }
                    else
                    {
                        if (cls.GetType().IsGenericType) {
                            switch (cls.GetType().Name) {
                                case "List`1":
                                case "IList`1":
                                    checkList(container, root, cls, preName);
                                    break;
                                case "Queue`1":
                                    checkQueue(container, root, cls, preName);
                                    break;
                                case "Dictionary`2":
                                case "IDictionary`2":
                                case "SerializableDictionary`2":
                                    checkDictionary(container, root, cls, "");
                                    break;
                                default:
                                    break;
                            }
                        } else {
                            // 先搜尋上層父類別(不包括object類別)的資料
                            m_BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                            root.Nodes.Add("base");
                            if (clsBaseType != null) {
                                checkBaseClass(container, root.LastNode, cls, clsBaseType, preName);
                            } else { ; }
                        }
                    }
                }
                else
                {
                    // 查詢已記綠的泛型物件
                    if (cls.GetType().IsGenericType)
                    {
                        switch (cls.GetType().Name)
                        {
                            case "List`1":
                            case "IList`1":
                                checkList(container, root, cls, preName);
                                break;
                            case "Queue`1":
                                checkQueue(container, root, cls, preName);
                                break;
                            case "Dictionary`2":
                            case "IDictionary`2":
                            case "SerializableDictionary`2":
                                checkDictionary(container, root, cls, "");
                                break;
                            default:
                                break;
                        }
                    }
                    else { ; }
                }
                clsNowType = cls.GetType();
                m_BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] deriveData = clsNowType.GetFields(m_BindingFlags);
                checkDeriveClass(container, root, cls, deriveData, preName);
                checkPropertiesData(container, clsNowType, root, cls, preName);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        /// <summary>
        /// 搜尋最上層父類別所有欄位資料
        /// </summary>
        /// <param name="container">物件要存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="cls">要搜尋的物件實體</param>
        /// <param name="baseType">遞迴上一層的類別型態</param>
        /// <param name="preName">遞迴上一層的類別實體名稱</param>
        private static void checkBaseClass(Dictionary<string, object> container, TreeNode root, object cls, Type baseType, string preName)
        {
            try
            {
                if (cls == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }

                Type nowBaseType = baseType.BaseType;

                if (nowBaseType != typeof(object))
                {
                    m_BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                    root.Nodes.Add("base");
                    checkBaseClass(container, root.LastNode, cls, nowBaseType, preName);

                    checkPropertiesData(container, nowBaseType, root.LastNode, cls, preName);
                }
                else
                {
                    ; // nothing //
                }
                m_BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] baseData = baseType.GetFields(m_BindingFlags);
                checkDeriveClass(container, root, cls, baseData, preName);

                checkPropertiesData(container, baseType, root, cls, preName);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        /// <summary>
        /// 解析衍生類別的所有公開、靜態欄位資料
        /// </summary>
        /// <param name="container">物件要存放的泛型容器</param>
        /// <param name="root">資料顯現要放存的節點</param>
        /// <param name="cls">要搜尋的物件實體</param>
        /// <param name="data">物件實體中所有公開、靜態欄位的資料</param>
        /// <param name="preName">遞迴上一層的類別實體名稱</param>
        private static void checkDeriveClass(Dictionary<string, object> container, TreeNode root, object cls, FieldInfo[] data, string preName)
        {
            try
            {
                if (cls == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }

                for (int i = 0; i < data.Length; i++)
                {
                    // 欄位為陣列型態
                    if (data[i].FieldType.IsArray)
                    {
                        root.Nodes.Add(data[i].Name);
                        string arrName = preName + @"\" + data[i].Name;
                        container.Add(arrName, data[i].GetValue(cls));
                        TreeNode arrNode = root.Nodes[root.Nodes.Count - 1];
                        checkArray(container, arrNode, data[i], cls, arrName);
                    }
                    // 欄位為類別型態(需注意string也是為類別型態)
                    else if (data[i].FieldType.IsClass)
                    {
                        if (data[i].FieldType == typeof(string))
                        {
                            root.Nodes.Add(data[i].Name + " = " + data[i].GetValue(cls));
                        }
                        else
                        {
                            // 是否為泛型資料型態
                            if (data[i].FieldType.IsGenericType)
                            {
                                root.Nodes.Add(data[i].Name);
                                string nodeName = preName + @"\" + data[i].Name;
                                if (data[i].FieldType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    checkList(container, root.LastNode, data[i].GetValue(cls), nodeName);
                                }
                                else if (data[i].FieldType.GetGenericTypeDefinition() == typeof(Queue<>))
                                {
                                    checkQueue(container, root.LastNode, data[i].GetValue(cls), nodeName);
                                }
                                else if (data[i].FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    container.Add(nodeName, data[i].GetValue(cls));
                                    foreach (Type iType in data[i].GetValue(cls).GetType().GetInterfaces())
                                    {
                                        typeof(ObjectMonitor).GetMethod("GetDictionaryContents").MakeGenericMethod(iType.GetGenericArguments()).Invoke(null, new object[] { container, root.Nodes[root.Nodes.Count - 1], data[i].GetValue(cls), nodeName });
                                        break;
                                    }
                                }
                                else
                                {
                                    ;// MessageBox.Show("型別無法判別", "checkDeriveClass Exception", MessageBoxButtons.OK);
                                }
                            }
                            else
                            {
                                if (data[i].FieldType.IsSecurityTransparent) continue;

                                string tempName = preName + @"\" + data[i].Name;
                                root.Nodes.Add(data[i].Name);
                                checkClass(container, root.LastNode, data[i].GetValue(cls), tempName);
                            }
                        }
                    }
                    else
                    {
                        root.Nodes.Add(data[i].Name + " = " + data[i].GetValue(cls));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private static void checkPropertiesData(Dictionary<string, object> container, Type t, TreeNode root, object cls, string preName)
        {
            try
            {
                if (cls == null)
                {
                    root.Text = preName + " = NULL";
                    return;
                }
                else { ; }

                PropertyInfo[] myPropertyInfo = t.GetProperties(m_BindingFlags);
                if (t.IsGenericType) return;

                for (int i = 0; i < myPropertyInfo.Length; i++)
                {
                    PropertyInfo myPropInfo = myPropertyInfo[i];
                    string temp = preName + @"\" + myPropInfo.Name;
                    if (myPropInfo.PropertyType.IsArray)
                    {
                        root.Nodes.Add(myPropInfo.Name);
                        if (myPropInfo.GetValue(cls, null) == null)
                        {
                            continue;
                        }
                        else
                        {
                            checkArray(container, root.LastNode, myPropInfo.GetValue(cls, null), temp);
                        }
                    }
                    else
                    {
                        if (myPropInfo.PropertyType.IsClass || myPropInfo.PropertyType.IsGenericType)
                        {
                            if (myPropInfo.PropertyType != typeof(string))
                            {
                                switch (myPropInfo.PropertyType.Name)
                                {
                                    case "List`1":
                                    case "IList`1":
                                        root.Nodes.Add(myPropInfo.Name);
                                        checkList(container, root.LastNode, myPropInfo.GetValue(cls, null), temp);
                                        break;
                                    case "Queue`1":
                                        root.Nodes.Add(myPropInfo.Name);
                                        checkQueue(container, root.LastNode, myPropInfo.GetValue(cls, null), temp);
                                        break;
                                    case "Dictionary`2":
                                    case "IDictionary`2":
                                    case "SerializableDictionary`2":
                                        root.Nodes.Add(myPropInfo.Name);
                                        // 利用搜尋的method來找尋所有欄位
                                        container.Add(temp, myPropInfo.GetValue(cls, null));
                                        checkDictionary(container, root.LastNode, myPropInfo.GetValue(cls, null), temp);
                                        break;
                                    case "Object":
                                        root.Nodes.Add(myPropInfo.Name + " = " + myPropInfo.GetValue(cls, null));
                                        break;
                                    default:
                                        if (myPropInfo.PropertyType.IsSecurityTransparent) continue;
                                        root.Nodes.Add(myPropInfo.Name);
                                        checkClass(container, root.LastNode, myPropInfo.GetValue(cls, null), temp);
                                        break;
                                }
                            }
                            else
                            {
                                TreeNode newNode = new TreeNode(myPropInfo.Name + " = " + myPropInfo.GetValue(cls, null));
                                newNode.Tag = myPropInfo.GetValue(cls, null);
                                root.Nodes.Add(newNode);
                            }
                        }
                        else
                        {
                            //if ((myPropInfo.PropertyType.IsSecurityTransparent) && (myPropInfo.PropertyType != typeof(Boolean))) continue;
                            if (myPropInfo.PropertyType.IsSecurityTransparent && myPropInfo.PropertyType.BaseType != typeof(System.ValueType)) continue;
                            root.Nodes.Add(myPropInfo.Name + " = " + myPropInfo.GetValue(cls, null));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }
        #endregion

        #region Event
     
        private void treeViewMonitor_ItemDrag(object sender, ItemDragEventArgs e)
        {
            try
            {
                TreeNode source = (TreeNode)e.Item;
                // 只拖曳可展開的TreeNode
                if (source.GetNodeCount(true) > 0 && !(source.Text.Equals("base")))
                {
                    string temp = source.FullPath;
                    string[] sArray = Regex.Split(temp, @"\\", RegexOptions.IgnoreCase);
                    int subLenaddr = 0;
                    string path = string.Empty;
                    for (int i = 0; i < sArray.Length; ++i)
                    {
                        subLenaddr = sArray[i].IndexOf('(');
                        if (subLenaddr >= 0)
                        {
                            sArray[i] = sArray[i].Substring(0, subLenaddr);
                        }
                        else { ; }

                        path += sArray[i].Trim();
                        if (i == (sArray.Length - 1)) break;
                        path += "\\";
                    }
                    // 濾掉base
                    path = path.Replace("base\\", string.Empty);
                    treeViewMonitor.DoDragDrop(path, DragDropEffects.Move | DragDropEffects.Copy);
                }
                else { ; }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }
        #endregion

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ObjRefresh(SelectObject);
        }

    }
}
