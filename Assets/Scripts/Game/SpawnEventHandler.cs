using System;
using System.Collections;
using Hats.Simulation;
using UnityEngine;

namespace Hats.Game
{
	public class SpawnEventHandler : GameEventHandler
	{
		public PlayerController playerPrefab;
		public PowerupController powerupPrefab;

		public override IEnumerator HandleSpawnEvent(PlayerSpawnEvent evt, Action callback)
		{
			Debug.Log("Spawning player " + evt.Player.dbid);
			yield return null;
			var playerGO = Instantiate(playerPrefab, GameProcessor.BattleGridBehaviour.Grid.transform);
			playerGO.Setup(evt.Player);
			var localPosition = GameProcessor.BattleGridBehaviour.Grid.CellToLocal(evt.Position);
			playerGO.transform.localPosition = localPosition;
			callback();
		}

		public override IEnumerator HandleCollectablePowerupSpawnEvent(CollectablePowerupSpawnEvent evt, Action callback)
		{
			Debug.Log($"Spawning powerup type={evt.PowerupType}");
			yield return null;
			var powerupGO = Instantiate(powerupPrefab, GameProcessor.BattleGridBehaviour.Grid.transform);
			powerupGO.Setup(evt.Position);
			powerupGO.transform.localPosition = GameProcessor.BattleGridBehaviour.Grid.CellToLocal(evt.Position);
			callback();
		}
	}
}