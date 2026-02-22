using Godot;
using SaveData = Godot.Collections.Dictionary<string, Godot.Variant>;
using Game.Utils;

public class SaveStateManager(BaseGame game)
{
    public BaseGame Game = game;
    private SaveData SaveData = [];

    public virtual void SaveGameState(SaveData saveData, string name = "savegame")
    {
        saveData["Version"] = Game.gameVersion;

        if (Game.Camera != null)
        {
            if (Game.Camera is Camera2D camera2D)
                saveData["CameraPosition"] = camera2D.Position;
            // else if (Game.Camera is Camera3D camera3D)
            //     saveData["CameraPosition"] = camera3D.Position;
        }

        SaveGameStateToFile(saveData, name);
    }

    public virtual SaveData LoadGameState(string name = "savegame")
    {
        SaveData = LoadGameStateFromFile(name);

        if (SaveData == null)
            return null;

        // Check if the save file version matches the current game version
        if (!SaveData.TryGetValue("Version", out var version) || (int)version != Game.gameVersion)
        {
            Logger.LogError($"Save file version mismatch. Expected {Game.gameVersion}, got {(int)version}.", Logger.LogTypeEnum.Framework);
            return null;
        }

        // Load camera position
        SaveData.TryGetValue("CameraPosition", out var cameraPosition);
        if (Game.Camera != null)
        {
            if (Game.Camera is Camera2D camera2D)
                camera2D.Position = GameUtils.ParseVector2(cameraPosition.ToString());
            // else if (Game.Camera is Camera3D camera3D)
            //     camera3D.Position = GameUtils.ParseVector3(cameraPosition.ToString());
        }

        return SaveData;
    }

    public static bool IsSaveFileExists(string name = "savegame")
    {
        return FileAccess.FileExists($"user://{name}.save");
    }

    protected static SaveData LoadGameStateFromFile(string name = "savegame")
    {
        if (!FileAccess.FileExists($"user://{name}.save"))
            return null;

        using var saveFile = FileAccess.Open($"user://{name}.save", FileAccess.ModeFlags.Read);
        if (saveFile == null)
        {
            Logger.LogError($"Failed to open save file {name}.", Logger.LogTypeEnum.Framework);
            return null;
        }

        var jsonString = saveFile.GetAsText();
        var saveData = (SaveData)Json.ParseString(jsonString);
        if (saveData == null)
        {
            Logger.LogError($"Failed to parse save data from {name}.", Logger.LogTypeEnum.Framework);
            return null;
        }

        return saveData;
    }

    protected static void SaveGameStateToFile(SaveData saveData, string name = "savegame")
    {
        using var saveFile = FileAccess.Open($"user://{name}.save", FileAccess.ModeFlags.Write);
        if (saveFile != null)
        {
            var jsonString = Json.Stringify(saveData, indent: "\t");
            saveFile.StoreString(jsonString);
            Logger.Log($"Game state saved to user://{name}.save", Logger.LogTypeEnum.Framework);
        }
        else
        {
            Logger.LogError($"Failed to open save file {name} for writing.", Logger.LogTypeEnum.Framework);
        }
    }
}