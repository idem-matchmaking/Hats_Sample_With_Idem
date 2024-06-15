using Beamable;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using Game.UI.Matchmaking;
using Hats.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Hats.Game
{
	[Serializable]
	public class SimGameTypeRef : ContentRef<SimGameType>
	{
	}

	public class MatchmakingBehaviour : MonoBehaviour
	{
		[Header("Content Refs")]
		public SimGameTypeRef GameTypeRef;

		public IMatchmakingService MatchmakingService { get; private set; }

		[SerializeField]
		private Configuration _configuration = null;
		private BeamContext _beamContext;

		private void OnEnable()
		{
			if (_configuration == null)
			{
				Debug.LogError("MatchmakingBehaviour: Configuration is not set!");
				return;
			}

			MatchmakingService = _configuration.UseIdemMatchmaking
				? new IdemMatchmaking(GameTypeRef.Id)
				: new BeamableMatchmaking(GameTypeRef.Id);
			
			MatchmakingService.OnMatchReady += OnMatchReady;
		}

		public async void FindGame()
		{
			await MatchmakingService.FindGame();
		}

		public async void Cancel()
		{
			await MatchmakingService.Cancel();
		}

		private void OnMatchReady(string gameId, List<long> playerIds)
		{
			Debug.Log($"Match is ready! Found gameId={gameId}");
			Debug.Log($"Starting match with DBIDs={string.Join(",", playerIds.ToArray())}");

			HatsScenes.LoadGameScene(gameId, playerIds);
		}
	}
}