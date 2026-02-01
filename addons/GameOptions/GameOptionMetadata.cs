using Godot;
using Godot.Collections;

public static partial class GameOptionMetadata
{
    public static readonly Dictionary<string, Array<string>> DropDownOptions = new()
    {
        { "resolution", new Array<string> { "1280x720", "1920x1080", "2560x1440", "3840x2160" } },
    };

    public static readonly Dictionary<string, Variant> DropDownValues = new()
    {
        { "resolution", new Array<Variant> { new Vector2I(1280, 720), new Vector2I(1920, 1080), new Vector2I(2560, 1440), new Vector2I(3840, 2160) } }
    };

    public static readonly Dictionary<string, OptionDisplayType> DisplayTypes = new()
    {
        { "master_volume", OptionDisplayType.Slider },
        { "music_volume", OptionDisplayType.Slider },
        { "sfx_volume", OptionDisplayType.Slider },
        { "music_enabled", OptionDisplayType.CheckBox },
        { "sfx_enabled", OptionDisplayType.CheckBox },
        { "fullscreen", OptionDisplayType.CheckBox },
        { "resolution", OptionDisplayType.DropDown }
    };
}
