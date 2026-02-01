using Godot;

public enum OptionDisplayType
{
    Slider,
    SpinBox,
    CheckBox,
    DropDown
}

[GlobalClass]
public partial class OptionMetadata : Resource
{
    [Export] public string DisplayName = "";
    [Export] public float Min = 0.0f;
    [Export] public float Max = 1.0f;
    [Export] public OptionDisplayType DisplayType = OptionDisplayType.Slider;
    // [Export] public Variant.Type DataType = Variant.Type.Float;
}
