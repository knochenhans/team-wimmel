using Godot;

public partial class ProgressBarEntry(string id, string displayName, Color color) : GodotObject
{
    public string ID = id;
    public string DisplayName = displayName;
    public Color Color = color;
    public ProgressBar ProgressBarControl;
}
