using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Layouts;
using NLog.Config;
using System.Xml;

namespace UniOPI
{
    public static class NLogManager
    {
        private enum nLogLevel
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
        public static class Logger
        {
            private const int CLASS_NAME_LENGTH = 30;
            private const int METHOD_NAME_LENGTH = 50;
            #region 之後刪掉
            //public static void LogErrorWrite(string loggerName, string className, string methodName, Exception ex)
            //{
            //    WriteLog(nLogLevel.Error, className, methodName, ex.ToString());
            //}
            //public static void LogErrorWrite(string loggerName, string className, string methodName, string msg)
            //{
            //    WriteLog(nLogLevel.Error, className, methodName, msg);
            //}
            //public static void LogErrorWrite(string loggerName, string className, string methodName, string msg, Exception ex)
            //{
            //    msg = string.Format("{0}{1}",msg ,ex.ToString());
            //    WriteLog(nLogLevel.Error, className, methodName, msg);
            //}
            //public static void LogInfoWrite(string loggerName, string className, string methodName, string msg)
            //{
            //    WriteLog(nLogLevel.Info, className, methodName, msg);
            //}
            //public static void LogWarnWrite(string loggerName, string className, string methodName, string msg) 
            //{
            //    WriteLog(nLogLevel.Warn, className, methodName, msg);
            //}
            //public static void LogTrxWrite(string loggerName, string msg)
            //{
            //    string className=string.Empty ;
            //    string methodName = string.Empty;
            //    WriteLog(nLogLevel.Trace, className, methodName, msg);
            //}
            #endregion
            private const string logTraceTo_fileName = "D:/UnicomLog/{ServerName}/OPI/${date:format=yyyyMMdd}/Trace/OPI_Trace_${date:format=HH}.txt";
            private const string logTraceTo_archiveFileName = "D:/UnicomLog/{ServerName}/OPI/${date:format=yyyyMMdd}/Trace/OPI_Trace_${date:format=HH}.{#####}.txt";
            private const string logErrorTo_fileName = "D:/UnicomLog/{ServerName}/OPI/${date:format=yyyyMMdd}/Error/OPI_Error_${date:format=HH}.txt";
            private const string logErrorTo_archiveFileName = "D:/UnicomLog/{ServerName}/OPI/${date:format=yyyyMMdd}/Error/OPI_Error_${date:format=HH}.{#####}.txt";
            private const string logInfoTo_fileName = "D:/UnicomLog/{ServerName}/OPI/${date:format=yyyyMMdd}/Info/OPI_Info_${date:format=HH}.txt";
            private const string logInfoTo_archiveFileName = "D:/UnicomLog/{ServerName}/OPI/${date:format=yyyyMMdd}/Info/OPI_Info_${date:format=HH}.{#####}.txt";

            public static void ReplaceServerName(string serverName)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("UniOPI.exe.config");
                var xmlNodes = doc.SelectNodes("configuration/nlog/targets");

                if (xmlNodes != null)
                {
                    foreach (XmlNode ns in xmlNodes[0].ChildNodes)
                    {
                        if(ns.NodeType!=XmlNodeType.Element)
                            continue;
                        if (string.IsNullOrEmpty(ns.Attributes["name"].Value))
                            continue;

                        switch (ns.Attributes["name"].Value)
                        {
                            case "TraceLog":
                                ns.Attributes["fileName"].Value = logTraceTo_fileName;
                                ns.Attributes["archiveFileName"].Value = logTraceTo_archiveFileName;
                                break;
                            case "ErrorLog":
                                ns.Attributes["fileName"].Value = logErrorTo_fileName;
                                ns.Attributes["archiveFileName"].Value = logErrorTo_archiveFileName;
                                break;
                            case "InfoLog":
                                ns.Attributes["fileName"].Value = logInfoTo_fileName;
                                ns.Attributes["archiveFileName"].Value = logInfoTo_archiveFileName;
                                break;
                        }
                    }
                }

                {
                    XmlNodeList xnlTargets = doc.SelectNodes("configuration/nlog/targets/target");
                    foreach (XmlNode xnTarget in xnlTargets)
                    {
                        foreach (XmlAttribute xa in xnTarget.Attributes)
                        {
                            xa.Value = xa.Value.Replace("{ServerName}", serverName);
                        }
                    }
                }

                doc.Save("UniOPI.exe.Config");
            }
            
            public static void LogErrorWrite(string className, string methodName, Exception ex)
            {
                string msg = string.Format("{0} stackTrace:[{1}]", ex.Message, ex.StackTrace);
                WriteLog(nLogLevel.Error, className, methodName, msg);
            }
            public static void LogErrorWrite(string className, string methodName, string msg)
            {
                WriteLog(nLogLevel.Error, className, methodName, msg);
            }
            public static void LogInfoWrite(string className, string methodName, string msg)
            {
                WriteLog(nLogLevel.Info, className, methodName, msg);
            }
            public static void LogWarnWrite(string className, string methodName, string msg)
            {
                WriteLog(nLogLevel.Warn, className, methodName, msg);
            }
            public static void LogTrxWrite(string className, string methodName, string msg)
            {
                WriteLog(nLogLevel.Trace, className, methodName, msg);
            }
 
            private static void ConvertLogInfo(ref string sourceName, ref string methodName)
            {
                sourceName = sourceName.PadRight(CLASS_NAME_LENGTH);
                if (!methodName.Contains("()")) methodName=string.Format("{0}{1}",methodName,"()");
                methodName = methodName.PadRight(METHOD_NAME_LENGTH);
            }
            private static void WriteLog(nLogLevel  logLevel, string className, string methodName, string msg)
            {
                ConvertLogInfo(ref className, ref methodName);
                NLog.Logger logger = LogManager.GetCurrentClassLogger();
                string formatMsg = string.Format("[{0}]{1}-{2}",className ,methodName ,msg);
                switch (logLevel)
                { 
                    case nLogLevel.Error :
                        logger.Error(formatMsg);
                        break;
                    case nLogLevel.Debug:
                        logger.Debug(formatMsg);
                        break;
                    case nLogLevel.Fatal:
                        logger.Fatal(formatMsg);
                        break;
                    case nLogLevel.Info:
                        logger.Info(formatMsg);
                        break;
                    case nLogLevel.Trace:
                        logger.Trace(formatMsg);
                        break;
                    case nLogLevel.Warn:
                        logger.Trace(formatMsg);
                        break;
                }
                
            }

        }
    }
}
