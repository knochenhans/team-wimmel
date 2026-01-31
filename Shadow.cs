using Godot;
using System;

public partial class Shadow : Area2D
{
    public override void _Input(InputEvent @event)
    {
        // if (@event is InputEventMouseButton mouseEvent)
        // {
        //     if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        //     {
        //         QueueFree();
        //     }
        // }

        // discard the event so it doesn't propagate to other nodes
        @event.Dispose();

        GD.Print("Input event received in Shadow");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // if (@event is InputEventMouseButton mouseEvent)
        // {
        //     if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        //     {
        //         QueueFree();
        //     }
        // }

        // discard the event so it doesn't propagate to other nodes
        @event.Dispose();

        // GD.Print("Unhandled input event received in Shadow");
    }

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
    }

    public void OnMouseEntered()
    {
        // QueueFree();
    }
}
