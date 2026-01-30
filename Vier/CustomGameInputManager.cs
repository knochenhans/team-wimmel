using System.Threading.Tasks;

using Godot;
using Godot.Collections;

public class CustomGameInputManager(CustomGame game, Camera2D camera) : BaseGameInputManager(game, camera)
{
    public override async Task HandleGlobalInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
        {
            // var currentStageScene = StageManager.Instance.CurrentStageScene;

            switch (keyEvent.Keycode)
            {
                case Key.D:
                    if (game.GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Running)
                    {
                        // game.PlayerEntity.StatManager.SetStat("health", 0);

                        // await StageManager.Instance.RestartCurrentStage();
                    }
                    break;
                case Key.R:
                    if (game.GameStateManager.CurrentGameState == BaseGameStateManager.GameState.Running)
                        await game.Restart();
                    break;
                case Key.Enter:
                    await game.GameStateManager.ExitToScene("menu", new Variant());
                    break;
            }

            await base.HandleGlobalInput(@event);
        }
    }
}