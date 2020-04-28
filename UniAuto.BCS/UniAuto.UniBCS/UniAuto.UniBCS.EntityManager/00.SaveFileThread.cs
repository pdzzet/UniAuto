using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using Newtonsoft.Json;

namespace UniAuto.UniBCS.EntityManager
{
	public class SaveFileThread
	{
        public class Item
        {
            public EntityManager.Info _info = null;
            public EntityFile _file = null;
            public Item(EntityManager.Info info, EntityFile file)
            {
                _info = info;
                _file = file;
            }
        }

        private static int _run = 0;
        private static Thread _thread = null;
        private static Queue<Item> _saveQ = new Queue<Item>();
        private static readonly string _loggerName = "Service";

        /// <summary>
        /// 由Spring呼叫, 讀取DB資料並實例化EntityData及啟動Thread
        /// </summary>
        public static void Init()
        {
            if (_run == 0)
            {
                _thread = new Thread(new ThreadStart(ThreadFunc));
                _thread.IsBackground = true;
                _run++;
                _thread.Start();
            }
            else
                _run++;
        }

        /// <summary>
        /// 由Spring或測試程式呼叫, 終止Thread
        /// </summary>
        public static void Destroy()
        {
            _run--;
            if (_run == 0)
            {
                if (_thread != null)
                {
                    _thread.Join();
                    _thread = null;
                }
            }
        }

        /// <summary>
        /// 將EntityFile放入Queue, 由Thread存檔
        /// </summary>
        /// <param name="file"></param>
        public static void EnqueueSave(Item item)
        {
            if (_run > 0)
            {
                lock (_saveQ)
                {
                    _saveQ.Enqueue(item);
                }
            }
        }

        /// <summary>
        /// 從Queue取出EntityFile並寫檔
        /// </summary>
        private static void ThreadFunc()
        {
            List<Item> save_list = new List<Item>();
            Stopwatch stop_watch = new Stopwatch();
            const int THREAD_SLEEP = 300;
            while (_run > 0)
            {
                try
                {
                    List<Item> temp_list = new List<Item>();
                    lock (_saveQ)
                    {
                        while (_saveQ.Count > 0)
                        {
                            Item item = _saveQ.Dequeue();
                            temp_list.Add(item);
                        }
                    }
                    foreach(Item item in temp_list)
                    {
                        for (int i = save_list.Count - 1; i >= 0; i--)
                        {
                            if (save_list[i]._info.DataFilePath == item._info.DataFilePath &&
                                save_list[i]._file.GetFilename() == item._file.GetFilename())
                            {
                                save_list.RemoveAt(i);//同一個檔案不需重複寫檔, 只需將最新的資料寫入檔案
                            }
                        }
                        save_list.Add(item);
                    }
                    
                    if (save_list.Count > 0)
                    {
                        stop_watch.Restart();
                        foreach (Item item in save_list)
                        {
                            if (item._file.WriteFlag)
                            {
                                WriteFile(item);
                            }
                            else
                            {
                                DeleteFile(item);
                            }
                        }
                        stop_watch.Stop();
                        save_list.Clear();
                        //Queue已被清空, Sleep一下等待Queue累積
                        //只要Queue裡有資料, 就必須把資料寫完才能離開迴圈
                        Thread.Sleep(THREAD_SLEEP);
                    }
                    else
                    {
                        if (_run > 0)
                            Thread.Sleep(THREAD_SLEEP);
                        else
                            break;
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(_loggerName, _loggerName, "ThreadFunc()", ex);
                }
            }
        }

        /// <summary>
        /// 將EntityFile序列化後存檔
        /// </summary>
        /// <param name="File"></param>
        private static void WriteFile(Item item)
        {
            try
            {
                if (item._file.GetFilename() != string.Empty)
                {
                    if (!Directory.Exists(item._info.DataFilePath))
                    {
                        Directory.CreateDirectory(item._info.DataFilePath);
                    }
                    //先將檔案寫入暫存檔, 寫完後重命名, 避免檔案寫一半時BC就被終止.
                    //留下寫一半的壞檔, BC重啟後就無法讀取
                    string fname = string.Format("{0}{1}", item._info.DataFilePath, item._file.GetFilename());
                    string tmp_fname = string.Format("{0}{1}.tmp", item._info.DataFilePath, item._file.GetFilename());
                    if (item._info.FileType == EntityManager.FILE_TYPE.BIN)
                    {
                        #region 寫入暫存檔
                        {
                            byte[] data_bytes = BinarySerialize(item._file);
                            //using (FileStream fs = System.IO.File.OpenWrite(string.Format("{0}{1}", DataFilePath, file.GetFilename())))
                            using (FileStream fs = System.IO.File.Open(tmp_fname, FileMode.Create))
                            {
                                byte[] int_bytes = BitConverter.GetBytes(data_bytes.Length);
                                fs.Write(int_bytes, 0, int_bytes.Length);
                                fs.Write(data_bytes, 0, data_bytes.Length);
                                fs.Flush();
                            }
                        }
                        #endregion
                        #region 重命名
                        {
                            #region 已下File.Delete, 但File.Move時仍拋Exception, 因為File.Delete需要時間
                            //if (File.Exists(fname))
                            //    File.Delete(fname);
                            //File.Move(tmp_fname, fname);//System.IO.IOException: 當檔案已存在時，無法建立該檔案。
                            #endregion

                            //改以迴圈檢查, 並在File.Delete後睡一下
                            if (File.Exists(fname))
                                File.Delete(fname);
                            while (File.Exists(fname))
                                Thread.Sleep(1);
                            File.Move(tmp_fname, fname);
                        }
                        #endregion
                    }
                    else if (item._info.FileType == EntityManager.FILE_TYPE.XML)
                    {
                        #region 寫入暫存檔
                        {
                            string xml = XmlSerialize(item._file);
                            //using (FileStream fs = System.IO.File.OpenWrite(string.Format("{0}{1}", DataFilePath, file.GetFilename())))
                            using (FileStream fs = System.IO.File.Open(tmp_fname, FileMode.Create))
                            {
                                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                                {
                                    sw.Write(xml);
                                    sw.Flush();
                                    fs.Flush();
                                }
                            }
                        }
                        #endregion
                        #region 重命名
                        {
                            #region 已下File.Delete, 但File.Move時仍拋Exception, 因為File.Delete需要時間
                            //if (File.Exists(fname))
                            //    File.Delete(fname);
                            //File.Move(tmp_fname, fname);//System.IO.IOException: 當檔案已存在時，無法建立該檔案。
                            #endregion

                            //改以迴圈檢查, 並在File.Delete後睡一下
                            if (File.Exists(fname))
                                File.Delete(fname);
                            while (File.Exists(fname))
                                Thread.Sleep(1);
                            File.Move(tmp_fname, fname);
                        }
                        #endregion
                    } else if (item._info.FileType == EntityManager.FILE_TYPE.JSON) {

                        #region 寫入暫存檔
                        string json = JsonSerialize(item._file);
                        using (FileStream fs = System.IO.File.Open(tmp_fname, FileMode.Create)) {
                            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                                sw.Write(json);
                                sw.Flush();
                                fs.Flush();
                            }
                        }
                        #endregion
                        #region 重命名
                        {
                            #region 已下File.Delete, 但File.Move時仍拋Exception, 因為File.Delete需要時間
                            //if (File.Exists(fname))
                            //    File.Delete(fname);
                            //File.Move(tmp_fname, fname);//System.IO.IOException: 當檔案已存在時，無法建立該檔案。
                            #endregion

                            //改以迴圈檢查, 並在File.Delete後睡一下
                            if (File.Exists(fname))
                                File.Delete(fname);
                            while (File.Exists(fname))
                                Thread.Sleep(1);
                            File.Move(tmp_fname, fname);
                        }
                        #endregion
                    }
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(_loggerName, _loggerName, string.Format("WriteFile()-ThreadId=[{0}]", Thread.CurrentThread.ManagedThreadId), ex);
            }
        }

        /// <summary>
        /// Delete Entity File
        /// </summary>
        /// <param name="file"> EntityFile object</param>
        private static void DeleteFile(Item item)
        {
            try
            {
                if (!string.IsNullOrEmpty(item._file.GetFilename()))
                {
                    string path = Path.Combine(item._info.DataFilePath, item._file.GetFilename());
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        NLogManager.Logger.LogInfoWrite(_loggerName, _loggerName, "DeleteFile()",
                                    string.Format("Delete file name={0}.", path));
                    }
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(_loggerName, _loggerName, "DeleteFile()", ex);
            }
        }

        /// <summary>
        /// Binary序列化
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        private static byte[] BinarySerialize(EntityFile entityFile)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, entityFile);
            return ms.ToArray();
        }

        /// <summary>
        /// Xml序列化
        /// </summary>
        /// <param name="entityFile"></param>
        /// <returns></returns>
        private static string XmlSerialize(EntityFile entityFile)
        {
            StringWriterUTF8 sw = new StringWriterUTF8();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            settings.Encoding = Encoding.UTF8;
            settings.IndentChars = "    ";
            settings.OmitXmlDeclaration = true;//移除xmlns:q1="http://www.w3.org/2001/XMLSchema"

            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd

            XmlWriter writer = XmlWriter.Create(sw, settings);
            XmlSerializer serializer = new XmlSerializer(entityFile.GetType());
            serializer.Serialize(writer, entityFile, names);
            writer.Close();
            return sw.ToString();//XML序列化
        }

        private static string JsonSerialize(EntityFile entityFile) {

            return JsonConvert.SerializeObject(entityFile);
        }
    }
}
