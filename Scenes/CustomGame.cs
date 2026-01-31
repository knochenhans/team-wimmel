using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class CustomGame : BaseGame
{
    [Export] Dictionary<string, PackedScene> Locations = [];
    [Export] string StartingLocationID = "";

    AudioStreamPlayer2D SoundAudioStreamPlayer2D => GetNode<AudioStreamPlayer2D>("%SoundAudioStreamPlayer2D");

    GameLocation currentLocation;

    TextOutput TextOutput => GetNode<TextOutput>("%TextOutput");

    public enum CustomGameState
    {
        Active,
        Inactive,
    }

    public CustomGameState GameState = CustomGameState.Inactive;

    #region [Lifecycle]
    public override void InitGame(bool loadGame = false)
    {
        base.InitGame(loadGame);

        GameInputManager = new CustomGameInputManager(this, Camera);
        ChangeLocation(StartingLocationID);
    }
    #endregion

    #region [Public]
    public async void ChangeLocation(string locationID, float fadeInDuration = default, float fadeOutDuration = 1.0f)
    {
        if (Locations.TryGetValue(locationID, out PackedScene locationScene))
        {
            GameState = CustomGameState.Inactive;

            TextOutput.Clear();

            Camera.Position = Vector2.Zero;
            Camera.Zoom = Vector2.One;

            if (currentLocation != null)
            {
                currentLocation.LocationChanged -= ChangeLocation;
                currentLocation.ClickAreaMouseEntered -= OnClickAreaMouseEntered;
                currentLocation.ClickAreaMouseExited -= OnClickAreaMouseExited;
                currentLocation.QueueFree();
                currentLocation = null;
            }

            var locationInstance = locationScene.Instantiate();
            if (locationInstance is GameLocation location)
            {
                location.VariableManager = VariableManager;
                location.CursorManager = CursorManager;
                AddChild(location);

                location.SetCamera(Camera);
                currentLocation = location;
                location.LocationChanged += ChangeLocation;
                location.ClickAreaMouseEntered += OnClickAreaMouseEntered;
                location.ClickAreaMouseExited += OnClickAreaMouseExited;
                location.PlayBackgroundSound += OnPlaySound;
                location.PlayBackgroundMusic += OnPlayMusic;
                location.PlayMusic();
            }

            if (fadeInDuration == default)
                fadeInDuration = currentLocation.FadeInDuration;

            await SceneManager.Instance.FadeIn(fadeInDuration);
            GameState = CustomGameState.Active;
        }
        else
        {
            Logger.LogError($"Location with ID '{locationID}' not found in Locations dictionary.", "CustomGame", Logger.LogTypeEnum.Framework);
        }
    }
    #endregion

    #region [Events]
    public void OnClickAreaMouseEntered(ClickArea clickArea)
    {
        if (GameState != CustomGameState.Active)
            return;

        GD.Print($"Mouse Entered ClickArea: {clickArea.DisplayName} (ID: {clickArea.ID})");
        TextOutput.UpdateText(clickArea.Description);
    }

    public void OnClickAreaMouseExited(ClickArea clickArea)
    {
        if (GameState != CustomGameState.Active)
            return;

        GD.Print($"Mouse Exited ClickArea: {clickArea.DisplayName} (ID: {clickArea.ID})");
        TextOutput.Clear();
    }

    public void OnPlaySound(AudioStream audioStream)
    {
        if (audioStream == null)
            return;

        SoundAudioStreamPlayer2D.Stream = audioStream;
        SoundAudioStreamPlayer2D.Play();
    }

    public void OnPlayMusic(AudioStream audioStream)
    {
        if (audioStream == null)
            return;

        UISoundPlayer.Instance.StartOrKeepMusic(audioStream);
    }
    #endregion
}
