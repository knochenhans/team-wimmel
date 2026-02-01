using Godot;
using System;

public partial class Outro : GameLocation
{
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            SceneManager.Instance.Quit();
        }
    }
}
