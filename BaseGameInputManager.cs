using System.Threading.Tasks;
using Godot;

#nullable enable

public class BaseGameInputManager(BaseGame game, Camera2D camera)
{
    #region [Fields and Properties]
    protected BaseGame Game = game;
    protected Camera2D Camera = camera;

    protected Vector2 viewportMousePosition;
    #endregion

    #region [Godot]
    public virtual async Task HandleGlobalInput(InputEvent @event)
    {
        if (Game.GameStateManager.CurrentGameState == BaseGameStateManager.GameState.None ||
            Game.GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Loading ||
            Game.GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Exiting)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
        {
            switch (keyEvent.Keycode)
            {
                case Key.F5:
                    Game.SaveGame();
                    break;
                case Key.F7:
                    await Game.LoadGame();
                    break;
                case Key.F11:
                    _ = Game.WidgetManager.ToggleWidget("load", pauseGame: true);
                    break;
                case Key.F12:
                    _ = Game.WidgetManager.ToggleWidget("save", pauseGame: true);
                    break;
                case Key.Escape:
                    _ = Game.WidgetManager.ToggleWidget("options", pauseGame: true);
                    break;
            }
        }
    }
    #endregion
}