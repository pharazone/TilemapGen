using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class TilemapGen : MonoBehaviour
{
    public Tile groundTile;
    public Tile activatorTile;
    public Tile crackedTile;

    private int _currentLevel = 1;
    // The number of times we ran PlaceActivator()
    private int _activatorIter = 0;
    // The number of activators placed already;
    private int _activatorCount = 0;
    // The maximum number of activators to place on a level;
    private int _maxActivators = 8;

    private Tilemap _tilemap;

    // Only sets a tile if there isn't one there already.
    private void SetTileHumble(Vector3Int position, TileBase tile)
    {
        if (!_tilemap.HasTile(position))
        {
            _tilemap.SetTile(position, tile);
        }
    }

    // Returns the requested tile type as a Tile
    private Tile GetTile(TileType type)
    {
#if UNITY_EDITOR
        if (groundTile == null || activatorTile == null || crackedTile == null)
        {
            Debug.LogWarning("Your tiles are not set up!");
        }
#endif
        switch (type)
        {
            case TileType.Activator:
                return activatorTile;
            case TileType.Cracked:
                return crackedTile;
            default:
                return groundTile;
        }
    }

    void Awake()
    {
        _tilemap = GetComponent<Tilemap>();

        SetActivatorCount();


    }

    // Returns true if both offset coordinates are 0.
    private bool FirstPlatform(Vector3Int offset)
    {
        if (offset.x == 0 && offset.y == 0)
        {
            return true;
        }

        return false;
    }

    // Returns the requested edge of the platform.
    private Vector3Int Edge(EdgeType edgeType, Vector3Int bounds, Vector3Int offset)
    {
        switch (edgeType)
        {
            case EdgeType.TopLeft:
                return new Vector3Int(offset.x, offset.y + bounds.y - 1, 0);
            case EdgeType.TopRight:
                return new Vector3Int(offset.x + bounds.x - 1, offset.y + bounds.y - 1, 0);
            case EdgeType.BottomRight:
                return new Vector3Int(offset.x + bounds.x - 1, offset.y, 0);
            default: // BottomLeft
                return new Vector3Int(offset.x, offset.y, 0);
        }
    }

    // Returns the requested side of the platform.
    private List<Vector3Int> Side(Sides side, Vector3Int bounds, Vector3Int offset)
    {
        List<Vector3Int> sides = new List<Vector3Int>();

        switch (side) {
            case Sides.Bottom:
                for (int i = 0; i < bounds.x; i++)
                {
                    sides.Add(new Vector3Int(offset.x + i, offset.y, 0));
                }
                return sides;
            case Sides.Top:
                for (int i = 0; i < bounds.x; i++)
                {
                    sides.Add(new Vector3Int(offset.x + i, bounds.y + offset.y - 1, 0));
                }
                return sides;
            case Sides.Left:
                for (int i = 0; i < bounds.y; i++)
                {
                    sides.Add(new Vector3Int(offset.x - 1, offset.y + i, 0));
                }
                return sides;
            case Sides.Right:
                for (int i = 0; i < bounds.y; i++)
                {
                    sides.Add(new Vector3Int(bounds.x + offset.x - 1, offset.y + i, 0));
                }
                return sides;
            default: // Sides.All
                for (int i = 0; i < bounds.x; i++)
                {
                    sides.Add(new Vector3Int(offset.x + i, offset.y, 0));
                    sides.Add(new Vector3Int(offset.x + i, bounds.y + offset.y - 1, 0));
                }
                for (int j = 0; j < bounds.y; j++)
                {
                    sides.Add(new Vector3Int(offset.x - 1, offset.y + j, 0));
                    sides.Add(new Vector3Int(bounds.x + offset.x - 1, offset.y + j, 0));
                }
                return sides;
        }
    }

    private void DrawBorder(Vector3Int bounds, Vector3Int offset, TileType tileType)
    {
        for (int j = -1; j < bounds.y + 1; j++)
        {
            SetTileHumble(new Vector3Int(bounds.x + offset.x, j + offset.y, 0), GetTile(tileType));
            SetTileHumble(new Vector3Int(offset.x - 1, j + offset.y, 0), GetTile(tileType));
        }

        for (int i = 0; i < bounds.x; i++)
        {
            SetTileHumble(new Vector3Int(offset.x + i, offset.y - 1, 0), GetTile(tileType));
            SetTileHumble(new Vector3Int(offset.x + i, offset.y + bounds.y, 0), GetTile(tileType));
        }
    }

    private void SetActivatorCount()
    {
        _currentLevel = GameManager.Instance.CurrentLevel;

        _maxActivators = 8 + (_currentLevel * 2);
    }

    private void PlaceActivator(Vector3Int bounds, Vector3Int offset)
    {
        // If this is the first platform spawned..
        if (FirstPlatform(offset))
        {
            // and it's the first level, we place an activator on the edge always
            // in later levels we skip the first platform.
            if (_currentLevel == 1)
            {
                PlaceActivator(ActivatorType.SingleOnEdge, bounds, offset);
            }
        }
        else
        {
            // TODO: Randomly chose an activator
            PlaceActivator(ActivatorType.SingleOnEdge, bounds, offset);
        }
    }

    private void PlaceActivator(ActivatorType type, Vector3Int bounds, Vector3Int offset)
    {
        if (type == ActivatorType.SingleOnEdge)
        {
            // If this is the first platform, we skip BottomLeft because that's where the player spawns.
            if (FirstPlatform(offset))
            {
                PlaceSingleActivatorOnEdge((EdgeType)Random.Range(1, System.Enum.GetValues(typeof(EdgeType)).Length), bounds, offset);
            }
            else
            {
                PlaceSingleActivatorOnEdge((EdgeType)Random.Range(0, System.Enum.GetValues(typeof(EdgeType)).Length), bounds, offset);
            }
        }

        _activatorIter++;
    }

    private void PlaceSingleActivatorOnEdge(EdgeType edgeType, Vector3Int bounds, Vector3Int offset)
    {
        _tilemap.SetTile(Edge(edgeType, bounds, offset), GetTile(TileType.Activator));
        _activatorCount++;
    }

    private void FillTile(Vector3Int bounds, Vector3Int offset, TileType type = TileType.Ground)
    {
        Tile tileType = GetTile(type);

        for (int j = 0; j < bounds.y; j++)
        {
            for (int i = 0; i < bounds.x; i++)
            {
                _tilemap.SetTile(new Vector3Int(i + offset.x, j + offset.y, 0), tileType);
            }
        }

        // If this is the first platform, we make sure the player spawns on ground
        if (FirstPlatform(offset))
        {
            _tilemap.SetTile(Vector3Int.zero, GetTile(TileType.Ground));
        }
    }
}
