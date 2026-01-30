using Godot;
using System.Collections.Generic;

using static Logger;

public partial class VariableManager : GodotObject
{
    private Dictionary<string, Variant> vars = new();

    public void Set(string key, Variant value) => vars[key] = value;

    public T Get<[MustBeVariant] T>(string key, T defaultValue = default)
    {
        if (vars.TryGetValue(key, out var value))
            return value.As<T>();

        return defaultValue;
    }

    public bool Has(string key) => vars.ContainsKey(key);

    public void Increment(string key, int amount = 1)
    {
        int current = Get(key, 0);
        vars[key] = current + amount;
        Log($"Variable '{key}' incremented by {amount}. New value: {vars[key]}", "VariableManager", LogTypeEnum.Framework);
    }

    public Dictionary<string, Variant> GetAll() => vars;

    public void Load(Dictionary<string, Variant> data) => vars = data ?? new();
}
