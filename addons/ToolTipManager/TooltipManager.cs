using Godot;
using Godot.Collections;

public partial class TooltipManager : Node
{
    [Export] PackedScene TooltipScene;
    [Export] Node TooltipParentNode;

    [Export] public Vector2 TooltipOffset = new(0, 0);
    [Export] public double TooltipDelay = 0.5;

    public Array<ProgressBarEntry> ProgressBarEntries = [];

    Tooltip Tooltip;
    int ShowRequestId;
    bool followMouse = true;
    Vector2 requestedMousePos;

    public override void _Ready()
    {
        Tooltip = TooltipScene?.Instantiate<Tooltip>();
        Tooltip.ProgressBarEntries = ProgressBarEntries;
        TooltipParentNode.AddChild(Tooltip);
        Tooltip.Visible = false;
        Tooltip.MouseFilter = Control.MouseFilterEnum.Ignore;
        UpdateTooltipPosition();
    }

    public void RegisterProgressBar(string id, string displayName, Color color)
    {
        ProgressBarEntries.Add(new ProgressBarEntry(id, displayName, color));
        Tooltip.AddProgressBar(id, displayName, color);
    }

    public async void ShowTooltip(string text, Vector2 position = default, bool showProgressBars = false, bool followMouse = true)
    {
        int currentRequest = ++ShowRequestId;
        this.followMouse = followMouse;

        await ToSignal(GetTree().CreateTimer(TooltipDelay), "timeout");

        requestedMousePos = position == default
        ? Tooltip.GetGlobalMousePosition()
        : position;

        if (text?.Length == 0)
            return;

        if (currentRequest != ShowRequestId)
            return;

        Tooltip.ShowProgressBars(showProgressBars);
        Tooltip.UpdateText(text);
        Tooltip.ResetSize();
        if (position == default)
            Tooltip.UpdatePosition(requestedMousePos + TooltipOffset);
        else
            Tooltip.UpdatePosition(position + TooltipOffset);
        await Tooltip.FadeIn();
    }

    public override void _Input(InputEvent @event)
    {
        if (!Tooltip.Visible || !followMouse)
            return;

        if (@event is InputEventMouseMotion)
            UpdateTooltipPosition();
    }

    void UpdateTooltipPosition()
    {
        Tooltip.Position = Tooltip.GetGlobalMousePosition() + TooltipOffset;
    }

    public void HideTooltip()
    {
        ShowRequestId++;
        Tooltip.Visible = false;
    }

    public void UpdateProgressBar(string name, float value) => Tooltip.UpdateProgressBar(name, value);
}