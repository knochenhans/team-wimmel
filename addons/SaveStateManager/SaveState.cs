using Godot;
using Godot.Collections;

public partial class SaveState : Resource
{
    [Export] public Dictionary<string, Variant> EntitiesData = [];
    [Export] public Dictionary<string, Variant> ObjectsData = [];
    [Export] public Dictionary<string, Variant> PlayerData = [];
}