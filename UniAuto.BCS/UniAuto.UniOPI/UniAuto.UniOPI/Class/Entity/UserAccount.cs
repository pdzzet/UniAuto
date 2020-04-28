using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class UserAccount
    {

    }

    public class GroupUser
    {
        public string USER_ID { get; set; }
        public string CLIENT_KEY { get; set; }
        public string USER_NAME { get; set; }
        public string PASSWORD { get; set; }
        public string GROUP_ID { get; set; }
        public string ACTIVE { get; set; }
        public string E_MAIL { get; set; }
        public string LOCAL_NAME { get; set; }
        public string DEFAULT_FACTORY_NAME { get; set; }
        public string DEFAULT_AREA_NAME { get; set; }
        public string UACACTIVE { get; set; }
        public DateTime? TRX_DATETIME { get; set; }
        public string TRX_USER_ID { get; set; }
        public DateTime? ADD_DATETIME { get; set; }
        public string ADD_USER_ID { get; set; }
    }
}
