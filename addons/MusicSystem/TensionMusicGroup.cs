using Godot;
using Godot.Collections;

[GlobalClass]
public partial class TensionMusicGroup : Resource
{
    [Export] public int TensionLevel;
    [Export] public Array<TensionMusicClip> Clips = [];
}
