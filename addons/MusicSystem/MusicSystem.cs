using Godot;
using Godot.Collections;

using SaveData = Godot.Collections.Dictionary<string, Godot.Variant>;

public partial class MusicSystem : AudioStreamPlayer
{
    #region [Fields and Properties]
    [Export] Array<TensionMusicGroup> TensionMusicGroups = [];

    Dictionary<int, Array<TensionMusicClip>> tensionMusicClips = [];

    AudioStreamPlaybackInteractive Playback;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        foreach (TensionMusicGroup group in TensionMusicGroups)
            tensionMusicClips[group.TensionLevel] = group.Clips;

        if (GetStream() == null)
        {
            Logger.LogWarning("No AudioStream assigned to MusicSystem. Please assign an AudioStream in the inspector.", "MusicSystem", Logger.LogTypeEnum.Audio);
            return;
        }
        if (Playing)
            Playback = GetStreamPlayback() as AudioStreamPlaybackInteractive;
    }
    #endregion

    #region [General Logic]
    public void SwitchToClipByName(string clipName)
    {
        Playback = GetStreamPlayback() as AudioStreamPlaybackInteractive;
        Playback?.SwitchToClipByName(clipName);
        Logger.Log($"Switched to music clip: {clipName}", "MusicSystem", Logger.LogTypeEnum.Audio);
    }

    public void SwitchToTensionLevel(int tensionLevel)
    {
        if (!tensionMusicClips.TryGetValue(tensionLevel, out Array<TensionMusicClip> clips) || clips.Count == 0)
        {
            Logger.LogWarning($"No music clips found for tension level {tensionLevel}.", "MusicSystem", Logger.LogTypeEnum.Audio);
            return;
        }

        var totalProbability = 0.0f;

        foreach (var clip in clips)
            totalProbability += clip.Probability;

        var randomValue = GD.Randf() * totalProbability;
        float cumulativeProbability = 0.0f;
        AudioStreamInteractive streamInteractive = Stream as AudioStreamInteractive;

        foreach (var clip in clips)
        {
            cumulativeProbability += clip.Probability;
            if (randomValue <= cumulativeProbability)
            {
                string name = ClipToName(clip);

                if (!Playing)
                {
                    for (int i = 0; i < streamInteractive.ClipCount; i++)
                    {
                        if (streamInteractive.GetClipName(i) == name)
                        {
                            streamInteractive.InitialClip = i;
                            Play();
                            return;
                        }
                    }
                }
                else
                {
                    SwitchToClipByName(name);
                }

                return;
            }
        }
    }
    #endregion

    #region [Utility]
    private static string ClipToName(TensionMusicClip clip, bool capitalizeWords = true)
    {
        var name = clip.Clip.ResourcePath.GetFile().GetBaseName();

        var matches = MyRegex().Matches(name);
        var words = new System.Collections.Generic.List<string>();
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            var value = m.Value;
            if (capitalizeWords)
                value = char.ToUpper(value[0]) + value[1..];
            words.Add(value);
        }

        if (words.Count == 0) words.Add(name);
        return string.Join(" ", words);
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"[A-Z]?[a-z]+|\d+")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
    #endregion

    #region [Saving and Loading]
    public SaveData Save()
    {
        var saveData = new SaveData
        {
            // ["MusicSystem"] = new SaveData
            // {

            // }
        };
        return saveData;
    }

    public void Load(SaveData saveData)
    {

    }
    #endregion
}