using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace UniOPI
{
    public class PagedGridView : DataGridView
    {
        #region Fields
        private int _pageSize = 100;
        private BindingList<DataTable> _tables = null;
        private BindingSource _bs = null;
        private DataTable _source = null;
        #endregion

        #region Property
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }

        public DataTable Source
        {
            get
            {
                if (_source == null)
                    _source = new DataTable();

                return _source;
            }
        }

        public  BindingList<DataTable> Tables
        {
            get
            {
                if (_tables == null)
                    _tables = new BindingList<DataTable>();

                return _tables;
            }
            set 
            {
                _tables = value;
            }
        }
        #endregion
        
        public void SetPagedDataSource(DataTable dataTable, BindingNavigator bnav)
        {
            _bs = new BindingSource();
            _tables = new BindingList<DataTable>();
            _source = dataTable;

            DataTable dt = null;
            int counter = 1;
            foreach (DataRow dr in dataTable.Rows)
            {
                if (counter == 1)
                {
                    dt = dataTable.Clone();
                    _tables.Add(dt);
                }
                dt.Rows.Add(dr.ItemArray);
                if (PageSize < ++counter)
                {
                    counter = 1;
                }
            }
            bnav.BindingSource = _bs;
            _bs.DataSource = _tables;
            _bs.PositionChanged += bs_PositionChanged;
            bs_PositionChanged(_bs, EventArgs.Empty);
        }


        private void bs_PositionChanged(object sender, EventArgs e)
        {
            if (_bs.Position >= 0)
                this.DataSource = _tables[_bs.Position];
        }
    }
}
