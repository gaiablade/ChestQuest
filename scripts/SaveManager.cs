using Godot;
using Godot.Collections;

namespace ChestQuest.scripts;

public partial class SaveManager : Node
{
    private Dictionary<string, Variant> _data = new();

    public static Variant? Load(string key)
    {
        var found = GameManager.Singleton.SaveManager._data.TryGetValue(key, out var value);
        return found ? (Variant?)value : null;
    }

    public static void Save(string key, Variant value)
    {
        GameManager.Singleton.SaveManager._data[key] = value;
    }
}