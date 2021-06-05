using System;
using System.Collections.Generic;
using System.Linq;
using Hats.Simulation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hats.Game
{
	public class BattleGridBehaviour : MonoBehaviour
	{
		public BattleGrid BattleGrid;

		public Tilemap Tilemap;
		public Grid Grid;

		[Header("Tiles")]
		[SerializeField]
		private Tile iceTile;

		[SerializeField]
		private Tile rockTile;

		[SerializeField]
		private Tile holeTile;

		[SerializeField]
		private Tile lavaTile;

		private void Start()
		{
			foreach (var tile in BattleGrid.iceTiles)
			{
				Tilemap.SetTile(tile, iceTile);
			}
			foreach (var tile in BattleGrid.rockTiles)
			{
				Tilemap.SetTile(tile, rockTile);
			}
			foreach (var tile in BattleGrid.holeTiles)
			{
				Tilemap.SetTile(tile, holeTile);
			}
			foreach (var tile in BattleGrid.lavaTiles)
			{
				Tilemap.SetTile(tile, lavaTile);
			}
		}

		public List<Vector3Int> Neighbors(Vector3Int cell)
		{
			return BattleGrid.Neighbors(cell).Where(n => Tilemap.HasTile(n)).ToList();
		}

		public T SpawnObjectAtCell<T>(T prefab, Vector3Int cell) where T : Component
		{
			var instance = Instantiate(prefab, Grid.transform);
			var localPosition = Grid.CellToLocal(cell);
			instance.transform.localPosition = localPosition;
			return instance;
		}
	}
}