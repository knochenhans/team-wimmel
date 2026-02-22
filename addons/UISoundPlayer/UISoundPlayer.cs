using Godot;

using static Logger;

public partial class UISoundPlayer : Node
{
    [Export] public SimpleSoundSetResource UISoundResource;

    [ExportGroup("Music")]
    [Export] public AudioStream DefaultMusic;
    [Export] public float MusicVolumeDb = 0.0f;

    private AudioStreamPlayer AudioStreamPlayer => GetNode<AudioStreamPlayer>("AudioStreamPlayer");
    private AudioStreamPlayer MusicPlayer => GetNode<AudioStreamPlayer>("MusicPlayer");
    public static UISoundPlayer Instance { get; private set; }

    public override void _EnterTree()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            LogError("Duplicate UISoundPlayer instance detected, destroying the new one.", "UISoundPlayer", LogTypeEnum.Audio);
            QueueFree();
            return;
        }

        Instance = this;
    }

    public void PlaySound(string soundName)
    {
        if (UISoundResource == null || !UISoundResource.Sounds.TryGetValue(soundName, out AudioStream sound))
        {
            LogWarning($"Sound '{soundName}' not found in UISoundResource.", "UISoundPlayer", LogTypeEnum.Audio);
            return;
        }

        if (sound != null)
        {
            AudioStreamPlayer.Stream = sound;
            AudioStreamPlayer.Play();
        }
        else
        {
            LogWarning($"AudioStream for '{soundName}' is null.", "UISoundPlayer", LogTypeEnum.Audio);
        }
    }

    public void StartOrKeepMusic()
    {
        AudioStream musicToPlay = DefaultMusic;
        if (musicToPlay != null)
        {
            if (musicToPlay != MusicPlayer.Stream)
            {
                MusicPlayer.Stream = musicToPlay;
                MusicPlayer.VolumeDb = MusicVolumeDb;
                MusicPlayer.Play();
            }
        }
        else
        {
            LogWarning("DefaultMusic is not set.", "UISoundPlayer", LogTypeEnum.Audio);
        }
    }

    public void StartOrKeepMusic(AudioStream music)
    {
        if (music == null)
        {
            StopMusic();
            return;
        }

        AudioStream musicToPlay = music ?? DefaultMusic;
        if (musicToPlay != null)
        {
            if (musicToPlay != MusicPlayer.Stream || !MusicPlayer.Playing)
            {
                MusicPlayer.Stream = musicToPlay;
                MusicPlayer.VolumeDb = MusicVolumeDb;
                MusicPlayer.Play();
            }
        }
        else
        {
            LogWarning("DefaultMusic is not set.", "UISoundPlayer", LogTypeEnum.Audio);
        }
    }

    public void StopMusic()
    {
        MusicPlayer.Stop();
    }
}
