using System.ComponentModel;

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
        PlayStation2,

        [Description("PS2 (Australia)")]
        PlayStation2AU,

        [Description("PS2 (Japan)")]
        PlayStation2JP,

        [Description("Xbox")]
        Xbox
    };
}
