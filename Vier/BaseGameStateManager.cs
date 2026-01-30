using System.Threading.Tasks;

using Godot;

using static Logger;

using InitData = Godot.Collections.Dictionary<string, Godot.Variant>;


public class BaseGameStateManager(BaseGame game)
{
    BaseGame Game = game;

    public enum GameState
    {
        None,
        Loading,
        Running,
        Paused,
        GameOver,
        Exiting
    }

    protected GameState currentGameState = GameState.None;
    public GameState CurrentGameState
    {
        get => currentGameState;
        set
        {
            if (currentGameState != value)
            {
                Log($"Game state changed from {currentGameState} to {value}.", "Game", LogTypeEnum.Framework);
                currentGameState = value;
                StateChanged(value);
            }
        }
    }

    protected async void StateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Loading:
                break;
            case GameState.Running:
                // StageManager.Instance.UnpauseCurrentStage();
                break;
            case GameState.Paused:
                // StageManager.Instance.PauseCurrentStage();
                break;
            case GameState.GameOver:
                await Game.Restart();
                break;
        }
    }

    public async Task ExitToScene(string sceneName, Variant result)
    {
        CurrentGameState = GameState.Paused;

        await SceneManager.Instance.ChangeToScene(sceneName, new InitData()
        {
            { "GameResult", result }
        });
    }
}