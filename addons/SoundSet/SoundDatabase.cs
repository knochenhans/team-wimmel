using Godot;
using Godot.Collections;

#nullable enable

[GlobalClass]
public partial class SoundDatabase : Node
{
    [Export] public SoundDatabaseResource? SoundDatabaseResource;
    [Export] public string FallbackSoundSetID = string.Empty;

    public Dictionary<string, SoundSetResource> SoundSets = [];

    public override void _Ready()
    {
        // Initialize the Sounds dictionary by populating it with sounds from the SoundDatabaseResource
        if (SoundDatabaseResource?.SoundSets != null)
        {
            foreach (var soundSet in SoundDatabaseResource.SoundSets)
            {
                if (soundSet != null)
                    SoundSets[soundSet.ID] = soundSet;
            }
        }
    }

    public SoundSetResource? GetSoundSet(string id)
    {
        if (SoundSets.TryGetValue(id, out var soundSet))
            return soundSet;

        if (SoundSets.TryGetValue(FallbackSoundSetID, out var fallbackSoundSet))
            return fallbackSoundSet;

        Logger.LogWarning($"No sound set found for ID '{id}' and no valid fallback set.", Logger.LogTypeEnum.Audio);
        return null;
    }

    public static SoundDatabase CreateFromResource(SoundDatabaseResource resource, string fallbackSoundSetID = "")
    {
        var database = new SoundDatabase
        {
            SoundDatabaseResource = resource,
            FallbackSoundSetID = fallbackSoundSetID
        };

        if (resource != null)
        {
            if (resource.SoundSets == null)
            {
                Logger.LogWarning($"SoundDatabaseResource '{resource.ID}' has no SoundSets assigned.", Logger.LogTypeEnum.Audio);
                return database;
            }

            foreach (var soundSet in resource.SoundSets)
            {
                if (soundSet != null)
                    database.SoundSets[soundSet.ID] = soundSet;
            }
        }

        return database;
    }
}
