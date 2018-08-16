﻿using System.ComponentModel;

namespace WHampson.Gta3CarGenEditor.Models
{
    public enum GamePlatform
    {
        [Description("Android")]
        Android,

        [Description("iOS")]
        IOS,

        [Description("PC")]
        PC,

        [Description("PlayStation 2")]
        PS2,

        [Description("Xbox")]
        Xbox
    };
}
