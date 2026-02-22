using Godot;

public partial class Background : Control
{
    public SelectionManager selectionManager;

    public override void _GuiInput(InputEvent @event) => selectionManager?.HandleInputEvent(@event);
}
