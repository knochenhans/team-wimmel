using Godot;
using Godot.Collections;

using static Logger;

using SaveData = Godot.Collections.Dictionary<string, Godot.Variant>;

public class TensionManager(Node owner, MusicSystem musicSystem)
{
    #region [Fields and Properties]
    public Dictionary<int, Array<string>> TensionMusicClips;

    private int currentTensionState = -1;
    public int CurrentTensionState
    {
        get => currentTensionState;
        set
        {
            if (currentTensionState == value)
                return;

            TensionStateChanged(currentTensionState, value);
            currentTensionState = value;
            Log($"Tension state changed to {currentTensionState}.", "TensionManager", LogTypeEnum.World);
        }
    }

    readonly MusicSystem musicSystem = musicSystem;
    #endregion

    #region [General Logic]
    // private void SetupTensionTimer(TensionState newState, float waitTime = 10.0f)
    // {
    //     TensionTimer = new Timer
    //     {
    //         WaitTime = waitTime,
    //         OneShot = true
    //     };
    //     TensionTimer.Timeout += () =>
    //     {
    //         CurrentTensionState = newState;
    //         TensionTimer.QueueFree();
    //     };
    //     owner.AddChild(TensionTimer);
    //     TensionTimer.Start();
    // }

    private void TensionStateChanged(int oldState, int newState)
    {
        musicSystem.SwitchToTensionLevel(newState);
    }

    public void SetTensionState(int newState)
    {
        if (newState == currentTensionState)
            return;

        currentTensionState = newState;
        TensionStateChanged(currentTensionState, newState);
    }

    public void IncreaseTension(int v)
    {
        SetTensionState(currentTensionState + v);
    }

    public void DecreaseTension(int v)
    {
        SetTensionState(currentTensionState - v);
    }
    #endregion

    #region [Saving and Loading]
    public SaveData Save()
    {
        var saveData = new SaveData
        {
            ["CurrentTensionState"] = CurrentTensionState
        };
        return saveData;
    }

    public void Load(SaveData saveData)
    {
        if (saveData.TryGetValue("CurrentTensionState", out Variant value))
            CurrentTensionState = (int)value;
    }
    #endregion
}