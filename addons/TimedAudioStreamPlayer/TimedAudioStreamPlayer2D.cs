using System;
using Godot;
using Godot.Collections;
using static Logger.LogTypeEnum;

[GlobalClass]
public partial class TimedAudioStreamPlayer2D : AudioStreamPlayer2D
{
    [Export] public TimedAudioStreamPlayerResource TimedAudioStreamPlayerResource;
    [Export] public SoundSetResource DefaultSoundSet;
    [Export] public Array<SoundSetResource> SoundSets = [];

    public double BaseDelay = 0.0;
    public double RandomDelayAdded = 0.0;
    public double MaxDelay = 0.0;
    public double MinDelay = 0.0;

    private Dictionary<string, SoundSetResource> soundSetDict = [];

    private bool delayActive = false;

    enum State
    {
        Idle,
        LoopPlaying,
    }

    private State currentState = State.Idle;
    private int loopToken = 0;

    private SoundSetResource currentSoundSet;
    public SoundSetResource CurrentSoundSet
    {
        get => currentSoundSet;
        set
        {
            if (currentSoundSet == value)
                return;

            currentSoundSet = value;

            if (currentSoundSet != null)
            {
                VolumeDb = currentSoundSet.VolumeOffset;
                PitchScale *= currentSoundSet.PitchOffset;

                if (TimedAudioStreamPlayerResource == null)
                {
                    Logger.LogError("No TimedAudioStreamPlayerResource assigned to TimedAudioStreamPlayer2D.", Audio);
                    return;
                }

                var randomizer = new AudioStreamRandomizer
                {
                    RandomVolumeOffsetDb = TimedAudioStreamPlayerResource.RandomVolumeAdded,
                    RandomPitch = TimedAudioStreamPlayerResource.RandomPitch
                };

                if (currentSoundSet.SoundSet == null || currentSoundSet.SoundSet.Count == 0)
                {
                    Logger.LogError("CurrentSoundSet has no sounds assigned.", Audio);
                    return;
                }

                for (int i = 0; i < currentSoundSet.SoundSet.Count; i++)
                {
                    if (currentSoundSet.SoundSet[i].AudioStream is AudioStream stream)
                        randomizer.AddStream(i, stream, 1.0f / currentSoundSet.SoundSet.Count);
                }

                Stream = randomizer;
            }

            // if (currentState == State.LoopPlaying)
            // {
            //     loopToken++;

            //     currentState = State.Idle;

            //     StartLoop();
            // }
        }
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        if (TimedAudioStreamPlayerResource == null)
        {
            Logger.LogError("No TimedAudioStreamPlayerResource assigned to TimedAudioStreamPlayer2D.", Audio);
            return;
        }

        Stream = new AudioStreamRandomizer();
    }

    public override void _Ready()
    {
        base._Ready();
        BuildSoundSetDict();

        if (TimedAudioStreamPlayerResource == null)
        {
            Logger.LogError("No TimedAudioStreamPlayerResource assigned to TimedAudioStreamPlayer2D.", Audio);
            return;
        }

        TimedAudioStreamPlayerResource.Init();

        BaseDelay = TimedAudioStreamPlayerResource.BaseDelay;
        RandomDelayAdded = TimedAudioStreamPlayerResource.RandomDelayAdded;
        MaxDelay = TimedAudioStreamPlayerResource.MaxDelay;
        MinDelay = TimedAudioStreamPlayerResource.MinDelay;

        if (BaseDelay > 0.0f || RandomDelayAdded > 0.0f || MinDelay > 0.0f)
        {
            if (TimedAudioStreamPlayerResource.Autostart)
                StartLoop();
        }

        if (Stream is AudioStreamRandomizer randomizer)
        {
            randomizer.RandomVolumeOffsetDb = TimedAudioStreamPlayerResource.RandomVolumeAdded;
            randomizer.RandomPitch = TimedAudioStreamPlayerResource.RandomPitch;
        }
    }

    public void Play(string soundSetID = "")
    {
        SetCurrentSoundSetByID(soundSetID);
        Play();
    }

    public async void Play()
    {
        if (TimedAudioStreamPlayerResource == null)
        {
            Logger.LogError("No TimedAudioStreamPlayerResource assigned to TimedAudioStreamPlayer2D.", Audio);
            return;
        }

        if (TimedAudioStreamPlayerResource.AlwaysRespectBaseDelay)
        {
            if (IsPlaying() || delayActive)
                return;
        }

        if (CurrentSoundSet == null)
        {
            CurrentSoundSet = DefaultSoundSet;

            if (CurrentSoundSet == null)
            {
                Logger.LogError("No sound set assigned to TimedAudioStreamPlayer2D.", Audio);
                return;
            }
        }

        base.Play();

        if (TimedAudioStreamPlayerResource.AlwaysRespectBaseDelay)
        {
            await ToSignal(this, AudioStreamPlayer2D.SignalName.Finished);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            delayActive = true;
            await ToSignal(GetTree().CreateTimer(GetNewDelay()), Timer.SignalName.Timeout);
            delayActive = false;
        }
    }

    public new void Stop()
    {
        if (TimedAudioStreamPlayerResource?.FinishOnStop == true)
            return;

        base.Stop();
    }

    public void SetCurrentSoundSetByID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            CurrentSoundSet = DefaultSoundSet;
            return;
        }

        if (soundSetDict.TryGetValue(id, out var soundSet))
            CurrentSoundSet = soundSet;
        else
            Logger.LogWarning($"No sound set with ID '{id}' found in TimedAudioStreamPlayer2D.", Audio);
    }

    private void BuildSoundSetDict()
    {
        // Initialize sound set dictionary
        soundSetDict.Clear();
        foreach (var soundSet in SoundSets)
        {
            if (soundSet != null && !string.IsNullOrEmpty(soundSet.ID))
            {
                if (!soundSetDict.ContainsKey(soundSet.ID))
                    soundSetDict[soundSet.ID] = soundSet;
                else
                    Logger.LogWarning($"Duplicate sound set ID '{soundSet.ID}' found in TimedAudioStreamPlayer2D.", Audio);
            }
            else
            {
                Logger.LogWarning("Sound set with null or empty ID found in TimedAudioStreamPlayer2D.", Audio);
            }
        }
    }

    public async void StartLoop()
    {
        // increment token to invalidate any previous loop
        int myToken = ++loopToken;

        if (currentState == State.LoopPlaying)
            return;

        currentState = State.LoopPlaying;

        if (CurrentSoundSet == null)
        {
            CurrentSoundSet = DefaultSoundSet;

            if (CurrentSoundSet == null)
            {
                Logger.LogError("No sound set assigned to TimedAudioStreamPlayer2D.", Audio);
                return;
            }
        }

        while (currentState == State.LoopPlaying && myToken == loopToken)
        {
            Play();

            if (TimedAudioStreamPlayerResource.AlwaysRespectBaseDelay)
                await ToSignal(this, AudioStreamPlayer2D.SignalName.Finished);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (currentState != State.LoopPlaying || myToken != loopToken)
                break;

            await ToSignal(GetTree().CreateTimer(GetNewDelay()), Timer.SignalName.Timeout);
        }
    }

    public void StopLoop()
    {
        currentState = State.Idle;
        loopToken++;

        Stop();
    }

    public double GetNewDelay()
    {
        if (MaxDelay < MinDelay)
            MaxDelay = MinDelay;

        double delay = BaseDelay;

        if (RandomDelayAdded > 0.0)
            delay += GD.RandRange(0.0, RandomDelayAdded);

        return Math.Clamp(delay, MinDelay, MaxDelay);
    }
}
