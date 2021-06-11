using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Hats.Simulation
{
    [Serializable]
    public class BattleGrid
    {
		private const int SanityCheck = 100;

		public enum TileType : byte
		{
			None,
			Start,
			Ground,
			Ice,
			Rock,
			Hole,
			Lava,
		}

		// TODO: Randomize start positions within quadrants
        public readonly Vector3Int[] START_POSITIONS =
        {
            new Vector3Int(-3, 4, 0),
            new Vector3Int(3, 4, 0),
            new Vector3Int(-3, -4, 0),
            new Vector3Int(3, -4, 0),
        };

		public Vector2Int Min, Max;
		public Vector2Int iceQuantityRange;
		public Vector2Int rockQuantityRange;
		public Vector2Int holeQuantityRange;
		public Vector2Int lavaQuantityRange;

		private Dictionary<Vector3Int, TileType> tiles = new Dictionary<Vector3Int, TileType>();

		// Tiles that are about to turn into lava
		private List<Vector3Int> suddenDeathTiles = new List<Vector3Int>();

		[System.Serializable]
		public class TileChangeEvent : UnityEvent<Vector3Int> { }

		// Raised when tiles change
		public TileChangeEvent onTileChange = new TileChangeEvent();

		public IEnumerable<Vector3Int> Tiles => tiles.Keys;


		// public BattleGrid(int xMin, int xMax, int yMin, int yMax)
		// {
		//     Min = new Vector2Int(xMin, yMin);
		//     Max = new Vector2Int(xMax, yMax);
		// }

		// Deltas relative to center in an even (long) row, clockwise from west.
		private Vector3Int[] _evenRow =
        {
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, -1, 0)
        };

        // Deltas relative to center in an odd (short) row, clockwise from west.
        private Vector3Int[] _oddRow =
        {
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(0, -1, 0)
        };

		// Initialize the grid by placing different types of tiles
		public void Initialize(System.Random random)
		{
			// Set the board to all ground tiles
			for (int y = Min.y; y <= Max.y; y++)
			{
				for (int x = Min.x; x <= Max.x; x++)
				{
					tiles.Add(new Vector3Int(x, y, 0), TileType.Ground);
				}
			}

			// Create start tiles
			tiles[new Vector3Int(Min.x, Min.y, 0)] = TileType.Start;
			tiles[new Vector3Int(Max.x, Min.y, 0)] = TileType.Start;
			tiles[new Vector3Int(Min.x, Max.y, 0)] = TileType.Start;
			tiles[new Vector3Int(Max.x, Max.y, 0)] = TileType.Start;

			// Place ice tiles
			var iceCount = random.Next(iceQuantityRange.x, iceQuantityRange.y + 1);
			for (int index = 0; GetTileTypeCount(TileType.Ice) < iceCount && index < SanityCheck; index++)
			{
				// Can spawn anwhere but on the left and right edges of the map
				var tile = new Vector3Int(0, random.Next(Min.y, Max.y + 1), 0);
				tile.x = random.Next(Min.x + 1, Max.x + (tile.y % 2 == 0 ? 1 : 0));
				
				// ... except on tiles of other types
				if (GetTileType(tile) != TileType.Ground)
				{
					continue;
				}

				tiles[tile] = TileType.Ice;
			}

			// Place rock tiles
			var rockCount = random.Next(rockQuantityRange.x, rockQuantityRange.y + 1);
			for (int index = 0; GetTileTypeCount(TileType.Rock) < rockCount && index < SanityCheck; index++)
			{
				// Can spawn anywhere
				var tile = new Vector3Int(random.Next(Min.x, Max.x + 1), random.Next(Min.y, Max.y + 1), 0);

				// ... except on tiles of other types
				if (GetTileType(tile) != TileType.Ground)
				{
					continue;
				}

				// ... except on or next to start positions
				if (IsAdjacentToTileType(tile, TileType.Start))
				{
					continue;
				}

				// ... except on or next to other rocks
				if (IsAdjacentToTileType(tile, TileType.Rock))
				{
					continue;
				}

				tiles[tile] = TileType.Rock;
			}

			// Place hole tiles
			var holeCount = random.Next(holeQuantityRange.x, holeQuantityRange.y + 1);
			for (int index = 0; GetTileTypeCount(TileType.Hole) < holeCount && index < SanityCheck; index++)
			{
				// Can spawn anwhere but on the edges of the map
				var tile = new Vector3Int(0, random.Next(Min.y + 1, Max.y - 1), 0);
				tile.y = random.Next(Min.x + (tile.y % 2 == 0 ? 2 : 1), Max.x - 1);
				
				// ... except on tiles of other types
				if (GetTileType(tile) != TileType.Ground)
				{
					continue;
				}
				
				// ... except on or next to start positions
				if (IsAdjacentToTileType(tile, TileType.Start))
				{
					continue;
				}

				// ... except on or next to other holes
				if (IsAdjacentToTileType(tile, TileType.Hole))
				{
					continue;
				}

				tiles[tile] = TileType.Hole;
			}

			// Place lava tiles
			var lavaCount = random.Next(lavaQuantityRange.x, lavaQuantityRange.y + 1);
			for (int index = 0; GetTileTypeCount(TileType.Lava) < lavaCount && index < SanityCheck; index++)
			{
				// Can spawn anywhere
				var tile = new Vector3Int(random.Next(Min.x, Max.x + 1), random.Next(Min.y, Max.y + 1), 0);
				
				// ... except on tiles of other types
				if (GetTileType(tile) != TileType.Ground)
				{
					continue;
				}

				// ... except on or next to start positions
				if (IsAdjacentToTileType(tile, TileType.Start))
				{
					continue;
				}

				tiles[tile] = TileType.Lava;
			}
		}

		public bool IsCellInBounds(Vector3Int cell)
        {
            if (cell.y % 2 == 0 && cell.x < 0)
            {
                cell.x -= 1;
            }
            return cell.x >= Min.x && cell.x <= Max.x && cell.y >= Min.y && cell.y <= Max.y;
        }

        public IEnumerable<Vector3Int> Neighbors(Vector3Int origin)
        {
            var result = new List<Vector3Int>();
            var table = origin.y % 2 == 0 ? _evenRow : _oddRow;
            foreach (var delta in table)
            {
                var gridPosition = origin + delta;
                 if (IsCellInBounds(gridPosition))
                {
                    result.Add(gridPosition);
                }
            }

            return result;
        }

        // // ReSharper disable once MemberCanBePrivate.Global
        // public bool IsOnBoard(Vector3Int gridPosition)
        // {
        //     return tileMap.GetTile(gridPosition) != null;
        // }

        // // ReSharper disable once MemberCanBePrivate.Global
        // public bool OccupiedByPlayer(Vector3Int gridPosition, List<GamePlayer> players)
        // {
        //     return players.Find(player => player.gridPosition == gridPosition);
        // }

        public bool IsAdjacent(Vector3Int a, Vector3Int b)
        {
			//    A   B
			//  C   D   E
			//    F   G

			// In short rows, x=0 is to the right of the center line.
			// If D = (0, 0) then C = (-1, 0) and E = (1, 0)
			// and A = (-1, -1), B = (0, -1)
			// and F = (-1, 1), G = (0, 1)

			// True case: same value
			if (a.Equals(b)) return true;

			// False cases: too far away
			if (Mathf.Abs(b.x - a.x) > 1) return false;
            if (Mathf.Abs(b.y - a.y) > 1) return false;

            // Easy true case: same row, delta-x of one.
            if (b.y == a.y && Mathf.Abs(b.x - a.x) == 1) return true;

            // Maybe case: adjacent row, delta-x in the range [-1, 1].
            if (a.y % 2 == 0)
            {
                // Even row. In our map these are long rows.
                return b.x - a.x < 1;
            }
            else
            {
                // Odd row. Short row.
                return b.x - a.x >= 0;
            }
        }

		public TileType GetTileType(Vector3Int tile)
		{
			if(tiles.ContainsKey(tile))
			{
				return tiles[tile];
			}
			return TileType.None;
		}

		public bool IsIce(Vector3Int tile)
		{
			return GetTileType(tile) == TileType.Ice;
		}

		public bool IsRock(Vector3Int tile)
		{
			return GetTileType(tile) == TileType.Rock;
		}

		public bool IsHole(Vector3Int tile)
		{
			return GetTileType(tile) == TileType.Hole;
		}

		public bool IsLava(Vector3Int tile)
		{
			return GetTileType(tile) == TileType.Lava;
		}

		// Get the number of tiles of this type
		public int GetTileTypeCount(TileType type)
		{
			return tiles.Values.Count(obj => obj == type);
		}

		public bool IsAdjacentToTileType(Vector3Int tile, TileType type)
		{
			foreach (var neighbor in Neighbors(tile))
			{
				if(GetTileType(neighbor) == type)
				{
					return true;
				}
			}
			return false;
		}
		
		// Returns true if the tile at this position can be walked on
		public bool IsWalkable(Vector3Int tile)
		{
			if (!IsCellInBounds(tile))
			{
				return false;
			}
			switch (GetTileType(tile))
			{
				case TileType.Start:
				case TileType.Ground:
				case TileType.Ice:
				case TileType.Lava:
					return true;
				default:
					return false;
			}
		}

		public bool GetRandomValidSuddenDeathTile(System.Random random, out Vector3Int tile)
		{
			var validTiles = tiles.Where(obj => (obj.Value == TileType.Ground || obj.Value == TileType.Ice || obj.Value == TileType.Start) && !IsInSuddenDeath(obj.Key));
			if(validTiles.Count() > 0)
			{
				var randomTile = validTiles.ElementAt(random.Next(0, validTiles.Count()));
				tile = randomTile.Key;
				return true;
			}
			tile = Vector3Int.zero;
			return false;
		}

		// Returns true if the tile is about to enter sudden death
		public bool IsInSuddenDeath(Vector3Int tile)
		{
			return suddenDeathTiles.Contains(tile);
		}

		// Advance all sudden death tiles to lava
		// Should be called before entering new tiles into sudden death this turn
		public void AdvanceSuddenDeathTiles()
		{
			for (int index = suddenDeathTiles.Count - 1; index >= 0; index--)
			{
				var tile = suddenDeathTiles[index];
				tiles[tile] = TileType.Lava;
				suddenDeathTiles.RemoveAt(index);
				onTileChange.Invoke(tile);
			}
			suddenDeathTiles.Clear();
		}

		public bool EnterSuddenDeath(Vector3Int tile)
		{
			if (!IsInSuddenDeath(tile) && IsWalkable(tile) && GetTileType(tile) != TileType.Lava)
			{
				// Tile is new to sudden death and will change next time sudden death is advanced
				suddenDeathTiles.Add(tile);
				onTileChange.Invoke(tile);
				return true;
			}
			return false;
		}

		public Direction GetDirection(Vector3Int origin, Vector3Int target)
        {
            if (!IsAdjacent(origin, target))
            {
                return Direction.Nowhere;
            }

            var table = origin.y % 2 == 0 ? _evenRow : _oddRow;
            var delta = target - origin;
            for (var i = 0; i < 6; i++)
            {
                var testVector = table[i];
                if (delta == testVector)
                {
                    return (Direction) i;
                }
            }

            return Direction.Nowhere;
        }

        public Vector3Int InDirection(Vector3Int gridPosition, Direction direction)
        {
            if (direction == Direction.Nowhere)
            {
                return gridPosition;
            }

            var table = gridPosition.y % 2 == 0 ? _evenRow : _oddRow;
            return gridPosition + table[(int) direction];
        }
    }
}