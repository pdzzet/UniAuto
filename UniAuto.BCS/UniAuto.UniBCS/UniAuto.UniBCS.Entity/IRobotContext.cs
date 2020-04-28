using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public interface IRobotContext
    {

        void AddParameter(string key, object obj);

        bool ContainsKey(string key);

        string GetReturnMessage();

        string GetReturnCode();

        void SetReturnMessage(string message);

        void SetReturnCode(string code);

        void SetReturnCodeAndMessage(string code, string message);

        object this[string key] {get;}

        DateTime CreateTime { get; }

        void Clear();
    }
}
