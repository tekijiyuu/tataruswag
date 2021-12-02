﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com



using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FFXIVTataruHelper
{
    public static class Logger
    {
        public static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> ConsoleLogQueue = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> ChatLogQueue = new ConcurrentQueue<string>();


        public static void WriteLog(string InputString,
        //[CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            WriteInnerLog(InputString, memberName, sourceLineNumber);
        }

        public static void WriteLog(object Input,
        //[CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            WriteInnerLog(Convert.ToString(Input), memberName, sourceLineNumber);
        }

        private static void WriteInnerLog(string InputString, string memberName, int sourceLineNumber)
        {
            string res = string.Empty;

            string time = DateTime.Now.ToString();

            res = time + Environment.NewLine;

            //res += sourceFilePath + Environment.NewLine;
            res += "Member name:" + memberName + Environment.NewLine;
            res += "Source Line Number: " + Convert.ToString(sourceLineNumber) + Environment.NewLine;

            res += InputString + Environment.NewLine;

            LogQueue.Enqueue(res);
        }

        public static void WriteConsoleLog(string InputString)
        {
            string res = "";

            string time = DateTime.Now.ToString();

            res = time + Environment.NewLine;
            res += InputString + Environment.NewLine;

            ConsoleLogQueue.Enqueue(res);
        }

        public static void WriteChatLog(string InputString)
        {
            string res = "";

            //string time = DateTime.Now.ToString();

            //res = time + Environment.NewLine;
            res += InputString;// + Environment.NewLine;

            ChatLogQueue.Enqueue(res);
        }
    }
}
