using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class SubBlock : Entity
    {
        public SubBlockCtrlEntityData Data { get; set; }

        public SubBlock(SubBlockCtrlEntityData data)
        {
            Data = data;
        }
    }
}
