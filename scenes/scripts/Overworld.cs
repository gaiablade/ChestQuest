using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class Overworld : Node2D
{
    [Export] public PackedScene DefaultMap;
    public AudioStreamPlayer SfxPlayer => _sfxPlayer;

    private Player _player;
    private Camera2D _camera2D;
    private AudioStreamPlayer _musicPlayer;
    private AudioStreamPlayer _sfxPlayer;

    private Map _currentMap;
    private bool _inventoryOpen;
    private string _currentMusicName;

    private Godot.Collections.Dictionary<string, Map> _cachedMaps = new();

    public override void _Ready()
    {
        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode is Player player)
        {
            _player = player;
            GameManager.Singleton.Player = player;
        }

        var map = DefaultMap.Instantiate<Map>();
        _ = ChangeMapInternal(map, _player.GetPosition());
        _cachedMaps.Clear();
        var entrances = GetTree().GetNodesInGroup("Entrance");
        foreach (var node in entrances)
        {
            if (node is Entrance entrance)
            {
                GD.Print($"Preloading map: {entrance.ToMap}");
                var mapName = entrance.ToMap;
                var adjacentMap = ResourceLoader.Load<PackedScene>(mapName).Instantiate<Map>();
                _cachedMaps[mapName] = adjacentMap;
            }
        }

        _camera2D = GetNode<Camera2D>("Player/Camera2D");
        _musicPlayer = GetNode<AudioStreamPlayer>("Music Player");
        _sfxPlayer = GetNode<AudioStreamPlayer>("Sfx Player");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Pause") && !_inventoryOpen)
        {
            _ = OpenInventory();
        }
    }


    public async Task ChangeMap(Entrance entrance)
    {
        var toMap = _cachedMaps.GetValueOrDefault(entrance.ToMap) ??
                    ResourceLoader.Load<PackedScene>(entrance.ToMap).Instantiate<Map>();
        var toSpawn =
            toMap.SpawnPoints.GetChildren().FirstOrDefault(x => x.GetName() == entrance.SpawnPointName) as Marker2D;

        await ChangeMapWithTransition(toMap, toSpawn?.GetPosition() ?? Vector2.Zero);
    }

    private async Task ChangeMapWithTransition(Map map, Vector2 spawn)
    {
        // var leaveSfx = ResourceLoader.Load<AudioStream>("res://assets/isolated/placeholder/sfx/289.wav");
        
        Pause();
        // SfxPlayer.SetStream(leaveSfx);
        // SfxPlayer.Play();
        await GameManager.Singleton.FadeOut(0.5); 
        await ChangeMapInternal(map, spawn);
        await GameManager.Singleton.FadeIn(0.5);
        Resume();
    }

    private async Task ChangeMapInternal(Map map, Vector2 spawn)
    {
        if (_currentMap != null)
        {
            CallDeferred(Node.MethodName.RemoveChild, _currentMap);
        }
        CallDeferred(Node.MethodName.AddChild, map);
        await ToSignal(map, Node.SignalName.Ready);

        _currentMap = map;
        SetCameraLimitsToMapEdges(_camera2D, map);
        PlayMapMusic(map);
        _player.CallDeferred(Node2D.MethodName.SetPosition, spawn);
    }

    private void PlayMapMusic(Map map)
    {
        if (map.Music == null) return;

        GD.Print($"Playing Music Track [{map.Music}]");

        var isMusicPlaying = !string.IsNullOrEmpty(_currentMusicName);
        if (!isMusicPlaying || _currentMusicName != map.Music)
        {
            var stream = ResourceLoader.Load<AudioStream>(map.Music);
            _musicPlayer.SetStream(stream);
            _musicPlayer.Play();
            _currentMusicName = map.Music;
        }
    }

    private void Pause()
    {
        // In the future, pause other entities such as enemies, etc.
        _player.Pause();
    }

    private void Resume()
    {
        _player.Resume();
    }

    private async Task OpenInventory()
    {
        _inventoryOpen = true;
        GameManager.Singleton.Pause();

        var inventoryMenu = ResourceLoader.Load<PackedScene>("res://scenes/PauseMenu.tscn").Instantiate();
        AddChild(inventoryMenu);

        await ToSignal(inventoryMenu, Node.SignalName.TreeExited);

        GameManager.Singleton.Resume();
        _inventoryOpen = false;
    }

    private static void SetCameraLimitsToMapEdges(Camera2D camera, Map map)
    {
        var baseLayer = map.BaseLayer;
        var mapBounds = baseLayer.GetUsedRect();
        var cameraSize = camera.GetViewportRect().Size / camera.GetZoom();

        if (mapBounds.Size * baseLayer.TileSet.TileSize < cameraSize)
        {
            camera.SetLimit(Side.Left, int.MinValue);
            camera.SetLimit(Side.Top, int.MinValue);
            camera.SetLimit(Side.Right, int.MaxValue);
            camera.SetLimit(Side.Bottom, int.MaxValue);
        }
        else
        {
            var tileSize = baseLayer.TileSet.TileSize;

            camera.SetLimit(Side.Left, mapBounds.Position.X * tileSize.X);
            camera.SetLimit(Side.Top, mapBounds.Position.Y * tileSize.Y);
            camera.SetLimit(Side.Right, mapBounds.End.X * tileSize.X);
            camera.SetLimit(Side.Bottom, mapBounds.End.Y * tileSize.Y);
        }
    }
}