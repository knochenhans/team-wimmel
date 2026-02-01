using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class ChaosraumGeräte : GameLocation
{
    [Export] public Dictionary<string, AudioStream> Loops = [];

    Dictionary<string, AudioStreamPlayer2D> AudioPlayers = [];
    Dictionary<string, ClickArea> ClickAreas = [];
    Dictionary<string, bool> LoopStates = [];

    public override void _Ready()
    {
        base._Ready();

        foreach (var clickArea in GetNode<Node2D>("%ClickAreas").GetChildren())
        {
            if (clickArea is ClickArea ca)
                ClickAreas.Add(ca.ID, ca);
        }

        foreach (var loop in Loops)
        {
            var audioPlayer = new AudioStreamPlayer2D
            {
                Stream = loop.Value,
                Autoplay = true,
                VolumeDb = 6.0f,
                PanningStrength = 2.0f,
            };
            AudioPlayers.Add(loop.Key, audioPlayer);
            LoopStates.Add(loop.Key, true);
            AddChild(audioPlayer);

            audioPlayer.GlobalPosition = GetClickAreaCenter(ClickAreas[loop.Key]);
        }
    }

    public override async void OnClickAreaClicked(ClickArea clickArea)
    {
        AudioPlayers[clickArea.ID].Playing = false;
        LoopStates[clickArea.ID] = false;

        base.OnClickAreaClicked(clickArea);

        if (!IsAnyLoopPlaying())
        {
            await ChangeLocation("chaosraum_spieluhr_offen_suche", FadeInDuration, FadeOutDuration);
        }
    }

    private bool IsAnyLoopPlaying()
    {
        return LoopStates.Values.Any(state => state);
    }

    private Vector2 GetClickAreaCenter(ClickArea clickArea)
    {
        var collisionPolygon = clickArea.GetChildren().OfType<CollisionPolygon2D>().First();
        var points = collisionPolygon.Polygon;
        Vector2 center = Vector2.Zero;
        foreach (var point in points)
        {
            center += point;
        }
        center /= points.Length;
        return clickArea.ToGlobal(center);
    }

    public override void PlayMusic()
    {
        // UISoundPlayer.Instance.StartOrKeepMusic(MusicStream);
    }
}
