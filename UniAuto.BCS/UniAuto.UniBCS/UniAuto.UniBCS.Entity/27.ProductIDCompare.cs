using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    public class ProductIDConstant : EntityFile
    {
        public List<ProductID> ProductIDCollection = new List<ProductID>();
    }

    [Serializable]
    public class ProductID
    {
        ArrayList _items = new ArrayList();
        int _value = 0;
        DateTime _lastUseTime = DateTime.Now;

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public DateTime LastUseTime
        {
            get { return _lastUseTime; }
            set { _lastUseTime = value; }
        }

        public ArrayList Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public ProductID()
        {
            _lastUseTime = DateTime.Now;
        }

        public ProductID(string[] strs)
        {
            _items.AddRange(strs);
        }

        public ProductID(List<string> strs)
        {
            _items.AddRange(strs);
        }

        public void SetItemData(List<string> strs)
        {
            _items.AddRange(strs);
        }

        public void SetItemData(string[] strs)
        {
            _items.AddRange(strs);
        }

        public override Boolean Equals(System.Object obj)
        {

            if (obj == null)
                return false;
            bool ok = (obj is ProductID);
            if (!ok)
                return ok;

            ProductID other = obj as ProductID;

            if (other.Items.Count != this.Items.Count)
                return false;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ToString() != other.Items[i].ToString())
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override System.String ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in _items)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        //public static bool operator ==(ProductID productID, string[] other)
        //{
        //    ProductID otherProduct = new ProductID(other);
        //    return productID.Equals(otherProduct);
        //}

        //public static bool operator !=(ProductID productID, string[] other)
        //{
        //    ProductID otherProduct = new ProductID(other);
        //    return productID.Equals(otherProduct);
        //}

        //public static bool operator ==(ProductID type, ProductID other)
        //{
        //    return type.Equals(other);
        //}

        //public static bool operator !=(ProductID type, ProductID other)
        //{
        //    return type.Equals(other);
        //}


    }
}
