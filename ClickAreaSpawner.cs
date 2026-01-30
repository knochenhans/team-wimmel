using Godot;
using System;

[GlobalClass]
public partial class ClickAreaSpawner : Polygon2D
{
    [Export] public string ID = "";
    [Export] public string DisplayName = "";
    [Export(PropertyHint.MultilineText)] public string Description = "";

    [ExportCategory("Action")]
    [Export] public string ActionID = "look";
    [Export] public string TargetLocationID = "";

    [ExportCategory("Transition")]
    [Export] public float FadeInDuration = 1.0f;
    [Export] public float FadeOutDuration = 1.0f;
    [Export] public float ZoomDuration = 1.0f;
    [Export] public float ZoomAmount = 1.2f;

    public ClickArea SpawnClickArea()
    {
        var clickArea = new ClickArea
        {
            ID = ID,
            Name = ID,
            DisplayName = DisplayName,
            Description = Description,
            ActionID = ActionID,
            TargetLocationID = TargetLocationID,
            FadeInDuration = FadeInDuration,
            FadeOutDuration = FadeOutDuration,
            ZoomDuration = ZoomDuration,
            ZoomAmount = ZoomAmount,
            Position = Position,
            RotationDegrees = RotationDegrees,
            Scale = Scale
        };

        var collisionPolygon = new CollisionPolygon2D
        {
            Polygon = Polygon
        };
        clickArea.AddChild(collisionPolygon);
        return clickArea;
    }
}
