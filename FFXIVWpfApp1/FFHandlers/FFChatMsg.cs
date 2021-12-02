﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;

namespace FFXIVTataruHelper.FFHandlers
{
    public struct FFChatMsg
    {
        public string Text { get; internal set; }
        public string Code { get; internal set; }
        public DateTime TimeStamp { get; internal set; }

        public FFChatMsg(string text, string code, DateTime timeStamp)
        {
            Text = text;
            Code = code;
            TimeStamp = timeStamp;
        }

        public FFChatMsg(FFChatMsg msg)
        {
            Text = msg.Text;
            Code = msg.Code;
            TimeStamp = msg.TimeStamp;
        }
    }
}
