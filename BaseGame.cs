using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using SaveData = Godot.Collections.Dictionary<string, Godot.Variant>;

using static Logger;

public partial class BaseGame : Scene
{
    #region [Fields and Properties]
    [Export] public NotificationManager NotificationManager;
    [Export] public Camera2D Camera;

    [ExportGroup("Widgets")]
    [Export] public Dictionary<string, PackedScene> WidgetScenes;

    [ExportGroup("Game Settings")]
    [Export] public int gameVersion = 1;
    [Export] public float LoadingFadeDuration = 1.0f;

    public Control WidgetsNode => GetNode<Control>("%Widgets");
    protected CanvasLayer CanvasLayer => GetNode<CanvasLayer>("%CanvasLayer");
    protected CanvasLayer FixedCanvasLayer => GetNode<CanvasLayer>("%FixedCanvasLayer");
    protected Control UIContainer => GetNodeOrNull<Control>("%UIContainer");
    protected HUD HUD => UIContainer.GetNodeOrNull<HUD>("%HUD");

    public WidgetManager WidgetManager;
    protected SaveStateManager SaveStateManager;
    protected BaseGameInputManager GameInputManager;
    protected VariableManager VariableManager;

    public SelectionManager SelectionManager => GetNodeOrNull<SelectionManager>("%SelectionManager");
    protected TooltipManager TooltipManager => GetNodeOrNull<TooltipManager>("%TooltipManager");
    protected NavigationRegion2D NavigationRegion;
    public BaseGameStateManager GameStateManager;

    protected BaseGameStateManager.GameState LastGameState = BaseGameStateManager.GameState.None;

    //TODO: Input in InputManager auslagern

    public enum ControlState
    {
        None,
        Selecting,
        Dragging,
    }

    public ControlState CurrentControlState = ControlState.None;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        CreateManagers();

        GameStateManager.CurrentGameState = BaseGameStateManager.GameState.Loading;

        UISoundPlayer.Instance?.StopMusic();

        InitGame();
        InitSaveStateManager();
    }

    public override void _Process(double delta)
    {
        if (GameStateManager.CurrentGameState != BaseGameStateManager.GameState.Running)
            return;
    }

    public override async void _Input(InputEvent @event)
    {
        if (GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Paused)
            return;

        await GameInputManager.HandleGlobalInput(@event);
    }
    #endregion

    #region [Public]
    public virtual void InitGame(bool loadGame = false)
    {
        //TODO: Wird beim Laden eines Spiels doppelt aufgerufen!
        WidgetManager = new WidgetManager(this, WidgetsNode, WidgetScenes, FixedCanvasLayer.Scale.X);

        Input.MouseMode = DefaultMouseMode;

        GameStateManager.CurrentGameState = BaseGameStateManager.GameState.Running;

        Log("Game initialized.", "Game", LogTypeEnum.Framework);
    }

    public virtual void Uninit()
    {
        WidgetManager = null;
    }

    public override void Pause()
    {
        LastGameState = GameStateManager.CurrentGameState;
        GameStateManager.CurrentGameState = BaseGameStateManager.GameState.Paused;

        LastMouseMode = Input.MouseMode;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        Log("Game paused.", "Game", LogTypeEnum.Framework);
    }

    public override void Resume()
    {
        if (GameStateManager.CurrentGameState != BaseGameStateManager.GameState.Paused)
            return;

        Input.MouseMode = LastMouseMode;

        GameStateManager.CurrentGameState = LastGameState;

        Log("Game resumed.", "Game", LogTypeEnum.Framework);
    }

    public override async Task Close()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GameStateManager.CurrentGameState = BaseGameStateManager.GameState.Exiting;

        await base.Close();
    }

    public virtual async Task Restart()
    {
        // Uninit();
        await SceneManager.Instance.FadeOut(LoadingFadeDuration);
        await SceneManager.Instance.FadeIn(LoadingFadeDuration);

        // InitGame();

        Log("Game restarted.", "Game", LogTypeEnum.Framework);
    }
    #endregion

    #region [Lifecycle]
    protected virtual void CreateManagers()
    {
        GameStateManager = new BaseGameStateManager(this);
        VariableManager = new VariableManager();
    }

    protected void InitSaveStateManager()
    {
        SaveStateManager = new SaveStateManager(this);
        // SaveStateManager.SaveGameState(initialState, "init");
    }
    #endregion

    #region [Events]
    public override void OnWidgetOpened(string widgetName, Widget widgetInstance)
    {
        Log($"Widget opened: {widgetName}", LogTypeEnum.UI);
    }

    public override void OnWidgetClosed(string widgetName)
    {
        Log($"Widget closed: {widgetName}", LogTypeEnum.UI);
    }

    protected override void OnBackgroundInput(InputEvent @event)
    {
        // Prevent background input for actual game scene
    }
    #endregion

    #region [Utility]
    public virtual void EnableUIInput()
    {
        // HUD.EnableUIInput();
    }

    public virtual void DisableUInput()
    {
        // HUD.DisableUIInput();
    }
    #endregion

    #region [Saving and Loading]
    public virtual async Task LoadGame(string saveGameName = "savegame")
    {
        await SceneManager.Instance.FadeOut(LoadingFadeDuration);

        Uninit();
        var saveData = SaveStateManager.LoadGameState(saveGameName);
        Load(saveData);

        await SceneManager.Instance.FadeIn(LoadingFadeDuration);

        NotificationManager?.ShowNotification("Game loaded.");

        if (GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Paused)
            Resume();

        Log($"Game loaded from slot '{saveGameName}'.", "Game", LogTypeEnum.Framework);
    }

    public virtual void SaveGame(string saveGameName = "savegame")
    {
        var saveData = Save();
        SaveStateManager.SaveGameState(saveData, saveGameName);

        if (GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Paused)
            Resume();

        NotificationManager?.ShowNotification("Game saved.");
        Log($"Game saved to slot '{saveGameName}'.", "Game", LogTypeEnum.Framework);
    }

    public virtual void Load(SaveData saveData)
    {
        // StageManager.Instance.Load(saveData);
    }

    public virtual SaveData Save()
    {
        var saveData = new SaveData
        {
            // ["StageManager"] = StageManager.Instance.Save()
        };
        return saveData;
    }
    #endregion
}