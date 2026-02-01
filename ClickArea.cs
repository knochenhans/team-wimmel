using Godot;

public partial class ClickArea : Area2D
{
    [Signal] public delegate void ClickedEventHandler();

    [Export] public string ID = "";
    [Export] public string DisplayName = "";
    [Export] public string Description = "";

    [Export] public string ActionID = "";
    [Export] public string TargetLocationID = "";
    [Export] public AudioStream SoundOnClick;
    [Export] public string CursorID = "";

    [Export] public float FadeInDuration = 1.0f;
    [Export] public float FadeOutDuration = 1.0f;
    [Export] public float ZoomDuration = 1.0f;
    [Export] public float ZoomAmount = 1.2f;
    [Export] public float PauseBeforeChange = 0.0f;

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        base._InputEvent(viewport, @event, shapeIdx);
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            EmitSignal(SignalName.Clicked);
    }
}
