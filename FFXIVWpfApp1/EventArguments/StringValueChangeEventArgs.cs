﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;

namespace FFXIVTataruHelper.EventArguments
{
    public class StringValueChangeEventArgs : TatruEventArgs
    {
        public string OldString { get; internal set; }

        public string NewString { get; internal set; }

        internal StringValueChangeEventArgs(Object sender) : base(sender) { }
    }
}
