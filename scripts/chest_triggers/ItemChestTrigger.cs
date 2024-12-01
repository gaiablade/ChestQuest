using ChestQuest.types;
using Godot;
using Godot.Collections;

namespace ChestQuest.scripts.chest_triggers;

[Tool]
public partial class ItemChestTrigger : ChestTrigger
{
    [Export]
    public int ItemId
    {
        get => _itemId;
        set
        {
            _itemId = value;
            if (Engine.IsEditorHint())
            {
                SetItemName();
            }
        }
    }

    [Export]
    public string ItemName
    {
        get => _itemName;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    private int _itemId;
    private string _itemName;

    public override void OnChestOpened()
    {
        GD.Print("Got item!");
    }

    private void SetItemName()
    {
        if (ResourceLoader.Exists("res://items/Items.tres"))
        {
            var json = ResourceLoader.Load<Json>("res://items/Items.tres");
            var data = json.GetData().As<Array<Dictionary>>();

            var name = string.Empty;
            foreach (var item in data)
            {
                var id = item["id"].AsInt32();

                if (id == _itemId)
                {
                    name = item["name"].AsString();
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                _itemName = name;
            }
            else
            {
                _itemName = "";
            }
        }
    }
}