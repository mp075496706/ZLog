/**
 * ==========================================
 * FileName：ZLog.cs
 * Author：jiaxin
 * CreatTime：2022-12-20 16:00:00
 * Desc：日志插件，主要用来保存Unity中的日志到本地。
 * ==========================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ZTools
{
    public class LogInfo
    {
        public string condition;
        public string stackTrace;
        public LogType logType;
        public string time;

        public LogInfo() { }

        public LogInfo(string condition,string stackTrace,LogType logType)
        {
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.logType = logType;
            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
        }
    }

    public class ZLog : MonoBehaviour
    {
        private static string logFileName;
        /// <summary>
        /// 日志文件名
        /// </summary>
        public static string LogFileName
        {
            get { return logFileName; }
        }

        private static string logFilePath;
        public static string LogFilePath
        {
            get { return logFilePath; }
        }

        /// <summary>
        /// 日志文件的全路径
        /// </summary>
        public static string LogFileFullPath
        {
            get { return logFilePath + "/" + logFileName; }
        }

        private static bool canWriteLog = false;
        /// <summary>
        /// 该项用来控制是否可以写入日志
        /// 如：上传日志文件的时候，不想要日志被写入，防止出现读写冲突，那么就设置此项为false;
        ///     上传操作完成后，重新设置此项为true即可。
        /// </summary>
        public static bool CanWriteLog
        {
            get { return canWriteLog; }
            set { canWriteLog = value; }
        }
        /// <summary>
        /// 是否正在写入日志，保证每次只有一个日志被写入，防止出现读写冲突。该项不需要手动设置，由程序自动控制。
        /// </summary>
        private bool writingLog = false;
        private bool firstEnable = true;
        private Queue<LogInfo> logInfos = new Queue<LogInfo>();
        private StreamWriter sw;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            logFilePath = Application.persistentDataPath + "/LogFile";
            logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            InvokeRepeating("CheckLogFileData", 0, 1);

            Application.logMessageReceivedThreaded += LogMessage;
        }

        private void Update()
        {
            SaveLogInfoToLocal();
        }

        void LogMessage(string condition, string stackTrace, LogType type)
        {
            logInfos.Enqueue(new LogInfo(condition, stackTrace, type));
        }

        void CheckLogFileData()
        {
            if (!logFileName.Equals(DateTime.Now.ToString("yyyy-MM-dd") + ".txt"))
            {
                logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            }

            if (!Directory.Exists(LogFilePath)) Directory.CreateDirectory(logFilePath);
            if (!File.Exists(LogFileFullPath))
            {
                canWriteLog = false;
                File.Create(LogFileFullPath).Close();
            }

            if (firstEnable)
            {
                firstEnable = false;
                Debug.Log("********************记录日志**********************");
            }
            canWriteLog = true;
        }

        void SaveLogInfoToLocal()
        {
            if (canWriteLog && logInfos.Count > 0 && !writingLog)
            {
                writingLog = true;
                LogInfo logInfo = logInfos.Dequeue();

                FileInfo fi = new FileInfo(LogFileFullPath);
                sw = fi.AppendText();   //在原文件后面追加内容     

                sw.WriteLine(logInfo.time + "  " + logInfo.logType + "  " + logInfo.condition + "  \n" + logInfo.stackTrace + "\n");
                sw.Close();
                sw.Dispose();

                writingLog = false;
            }
        }

        /// <summary>
        /// 强制写入日志到文件中
        /// 此方法会忽略canWriteLog、writingLog项
        /// 此方法仅在OnDestory方法使用时，由程序自动调用，请勿在正常使用时调用!!!
        /// </summary>
        /// <param name="logInfo"></param>
        void SaveLogInfoToLocal(LogInfo logInfo)
        {
            FileInfo fi = new FileInfo(LogFileFullPath);
            sw = fi.AppendText();   //在原文件后面追加内容     

            sw.WriteLine(logInfo.time + "  " + logInfo.logType + "  " + logInfo.condition + "  \n" + logInfo.stackTrace + "\n");
            sw.Close();
            sw.Dispose();
        }

        private void OnDestroy()
        {
            Debug.Log("********************结束记录**********************");

            //强制把日志队列中未写完的日志写入到本地
            List<LogInfo> logs = logInfos.ToList();
            for (int i = 0; i < logs.Count; i++)
            {
                SaveLogInfoToLocal(logs[i]);
            }
        }
    }
}
