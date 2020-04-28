using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.OpiSpec
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HelpAttribute : Attribute
    {
        protected string description;
        public string Description
        {
            get
            {
                return this.description;
            }
        }
        protected string version;
        public string Version
        {
            get
            {
                return this.version;
            }
            //if we ever want our attribute user to set this property, 
            //we must specify set method for it 
            set
            {
                this.version = value;
            }
        }
        public HelpAttribute(string Description_in)
        {
            this.description = Description_in;
            this.version = "No Version is defined for this class";
        }
    }
}
