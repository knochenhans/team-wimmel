using System;
using System.Threading.Tasks;
using Godot;
using static Logger;

public partial class GameLocation : Node
{
    #region [Fields and Properties]
    [Export] public string ID = "";
    [Export] public string DisplayName = "";
    [Export] public float FadeInDuration = 1.0f;
    [Export] public float FadeOutDuration = 1.0f;

    [Signal] public delegate void ClickAreaClickedEventHandler(ClickArea clickArea);
    [Signal] public delegate void ClickAreaMouseEnteredEventHandler(ClickArea clickArea);
    [Signal] public delegate void ClickAreaMouseExitedEventHandler(ClickArea clickArea);
    [Signal] public delegate void LocationChangedEventHandler(string locationID, float fadeInDuration = 1.0f, float fadeOutDuration = 1.0f);
    [Signal] public delegate void PlaySoundEventHandler(AudioStream audioStream);

    Node2D ClickAreaSpawnersNode => GetNode<Node2D>("%ClickAreas");
    Sprite2D BackgroundNode => GetNode<Sprite2D>("%BackgroundSprite2D");
    AudioStreamPlayer MusicPlayer => GetNode<AudioStreamPlayer>("%MusicPlayer");
    Camera2D Camera;
    public VariableManager VariableManager;
    public CursorManager CursorManager;
    ClickArea CurrentHoverClickArea = null;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        base._Ready();

        foreach (var spawner in ClickAreaSpawnersNode.GetChildren())
        {
            if (spawner is ClickAreaSpawner clickAreaSpawner)
            {
                var clickArea = clickAreaSpawner.SpawnClickArea();
                ClickAreaSpawnersNode.AddChild(clickArea);
                ClickAreaSpawnersNode.RemoveChild(clickAreaSpawner);

                clickArea.Clicked += () => OnClickAreaClicked(clickArea);
                clickArea.MouseEntered += () => OnClickMouseEntered(clickArea);
                clickArea.MouseExited += () => OnClickMouseExited(clickArea);
            }
        }

        VariableManager.Increment($"scene.{ID}.visits");
        FadeInMusic();
    }
    #endregion

    #region [Public]
    public void SetCamera(Camera2D camera)
    {
        Camera = camera;

        var backgroundSize = BackgroundNode.Texture.GetSize() * BackgroundNode.Scale;

        // Zoom to fit the background (preserve aspect ratio)
        var viewportSize = GetViewport().GetVisibleRect().Size;
        if (backgroundSize.X <= 0 || backgroundSize.Y <= 0 || viewportSize.X <= 0 || viewportSize.Y <= 0)
        {
            Camera.Zoom = Vector2.One;
            return;
        }

        var scaleX = viewportSize.X / backgroundSize.X;
        var scaleY = viewportSize.Y / backgroundSize.Y;
        var scale = Math.Min(scaleX, scaleY);
        Camera.Zoom = new Vector2(scale, scale);
    }

    public void FadeInMusic()
    {
        MusicPlayer.VolumeDb = -80.0f;
        MusicPlayer.Play();
        _ = TweenVolumeTo(MusicPlayer, 0.0f, FadeInDuration);
    }
    #endregion

    #region [Events]
    public async void OnClickAreaClicked(ClickArea clickArea)
    {
        GD.Print($"Clicked on ClickArea: {clickArea.DisplayName} (ID: {clickArea.ID})");

        EmitSignal(SignalName.ClickAreaClicked, clickArea);

        UISoundPlayer.Instance.PlaySound("click1");
        EmitSignal(SignalName.PlaySound, clickArea.SoundOnClick);

        if (string.IsNullOrEmpty(clickArea.TargetLocationID))
            return;

        await ChangeLocation(clickArea);
    }

    private async Task ChangeLocation(ClickArea clickArea)
    {
        Vector2 targetPosition = Camera.GetGlobalMousePosition();
        var zoomTask = TweenZoomAtPoint(clickArea.ZoomDuration, targetPosition, Camera.Zoom.X * clickArea.ZoomAmount);
        var fadeTask = SceneManager.Instance.FadeOut(clickArea.FadeOutDuration);

        await Task.Delay((int)(clickArea.PauseBeforeChange * 1000));

        var volumeTask = TweenVolumeTo(MusicPlayer, -80.0f, clickArea.FadeOutDuration);
        await Task.WhenAll(zoomTask, fadeTask, volumeTask);

        EmitSignal(SignalName.LocationChanged, clickArea.TargetLocationID, clickArea.FadeInDuration, clickArea.FadeOutDuration);

        CursorManager.ResetMouseCursor();
    }

    public void OnClickMouseEntered(ClickArea clickArea)
    {
        GD.Print($"Mouse Entered ClickArea: {clickArea.DisplayName} (ID: {clickArea.ID})");

        if (CurrentHoverClickArea != clickArea && CurrentHoverClickArea != null)
        {
            DeactivateClickArea(CurrentHoverClickArea);
        }
        CurrentHoverClickArea = clickArea;
        ActivateClickArea(clickArea);
    }

    public void OnClickMouseExited(ClickArea clickArea)
    {
        GD.Print($"Mouse Exited ClickArea: {clickArea.DisplayName} (ID: {clickArea.ID})");

        if (CurrentHoverClickArea == clickArea)
        {
            CurrentHoverClickArea = null;
            DeactivateClickArea(clickArea);
        }
    }
    #endregion

    #region [Utility]
    private void ActivateClickArea(ClickArea clickArea)
    {
        EmitSignal(SignalName.ClickAreaMouseEntered, clickArea);

        UISoundPlayer.Instance.PlaySound("hover");
        CursorManager.SetMouseCursor(clickArea.ActionID);
    }

    private void DeactivateClickArea(ClickArea clickArea)
    {
        EmitSignal(SignalName.ClickAreaMouseExited, clickArea);
        CursorManager.ResetMouseCursor();
    }

    public async Task TweenZoomAtPoint(
    float duration,
    Vector2 targetWorldPos,
    float targetZoom = 1.0f,
    Tween.TransitionType transitionType = Tween.TransitionType.Linear,
    Tween.EaseType easeType = Tween.EaseType.InOut)
    {
        if (Camera == null)
            return;

        Vector2 startZoom = Camera.Zoom;
        Vector2 endZoom = new(targetZoom, targetZoom);

        Vector2 startPos = Camera.Position;

        // Compute compensated target position
        Vector2 endPos =
            targetWorldPos +
            ((startPos - targetWorldPos) * (startZoom.X / targetZoom));

        var tcs = new TaskCompletionSource();

        var tween = Camera.CreateTween();
        tween.SetParallel(true);

        tween.TweenProperty(Camera, "zoom", endZoom, duration)
            .SetTrans(transitionType)
            .SetEase(easeType);

        tween.TweenProperty(Camera, "position", endPos, duration)
            .SetTrans(transitionType)
            .SetEase(easeType);

        tween.Finished += () => tcs.SetResult();
        await tcs.Task;
    }

    public async Task TweenVolumeTo(
    AudioStreamPlayer audioPlayer,
    float targetVolumeDb,
    float duration,
    Tween.TransitionType transitionType = Tween.TransitionType.Linear,
    Tween.EaseType easeType = Tween.EaseType.InOut)
    {
        if (audioPlayer == null)
            return;
        var tcs = new TaskCompletionSource();
        var tween = audioPlayer.CreateTween();
        tween.TweenProperty(audioPlayer, "volume_db", targetVolumeDb, duration)
            .SetTrans(transitionType)
            .SetEase(easeType);
        tween.Finished += () => tcs.SetResult();
        await tcs.Task;
    }
    #endregion

    #region [Saving and Loading]

    #endregion
}
