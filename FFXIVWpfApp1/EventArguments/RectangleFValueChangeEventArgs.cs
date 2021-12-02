﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Drawing;

namespace FFXIVTataruHelper.EventArguments
{
    public class RectangleDValueChangeEventArgs : TatruEventArgs
    {
        public RectangleD OldValue { get; internal set; }

        public RectangleD NewValue { get; internal set; }

        internal RectangleDValueChangeEventArgs(Object sender) : base(sender) { }
    }
}
