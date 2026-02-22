using Godot;

[Tool, GlobalClass]
public partial class CursorSetResource : Resource
{
    [Export] public Texture2D Texture = null!;
    [Export] public Vector2 Hotspot = Vector2.Zero;
}