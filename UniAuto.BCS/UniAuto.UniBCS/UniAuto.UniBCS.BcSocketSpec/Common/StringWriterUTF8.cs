using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.BcSocketSpec
{
    public class StringWriterUTF8 : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
