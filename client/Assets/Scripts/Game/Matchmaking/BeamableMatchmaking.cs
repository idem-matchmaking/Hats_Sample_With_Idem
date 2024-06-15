using Beamable;
using Beamable.Experimental.Api.Matchmaking;
using Hats.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.UI.Matchmaking
{
	public class BeamableMatchmaking : IMatchmakingService
	{
		public bool IsSearching => _isSearching;
		public MatchmakingState? State { get; }
		
		private MatchmakingHandle MatchmakingHandle = null;
		public event Action<string, List<long>> OnMatchReady;
		public event Action OnTimedOut;

		private BeamContext _beamContext;
		private bool _isSearching = false;
		private readonly string gameModeId;

		public BeamableMatchmaking(string gameModeId)
		{
			this.gameModeId = gameModeId;
		}

		public async Task FindGame()
		{
			_isSearching = true;
			_beamContext = BeamContext.Default;
			await _beamContext.OnReady;

			Debug.Log($"Starting matchmaking with game_type={gameModeId}...");
			
			MatchmakingHandle = await _beamContext.Api.Experimental.MatchmakingService.StartMatchmaking(
				gameModeId,
				readyHandler: MatchReadyHandler,
				timeoutHandler: MatchTimedOutHandler
			);

			Debug.Log($"Matchmaking started with handle={MatchmakingHandle}");
		}

		public async Task Cancel()
		{
			_isSearching = false;
			await MatchmakingHandle.Cancel();
		}

		private void MatchReadyHandler(MatchmakingHandle handle)
		{
			Debug.Assert(handle.State == MatchmakingState.Ready);

			var dbids = MatchmakingHandle.Status.Players;
			var gameId = MatchmakingHandle.Status.GameId;
			
			OnMatchReady?.Invoke(gameId, dbids.Select(i => long.Parse(i)).ToList());
		}

		private void MatchTimedOutHandler(MatchmakingHandle handle)
		{
			Debug.Log($"Matchmaking timed out! state={handle.State}");
			_isSearching = false;
			OnTimedOut?.Invoke();
		}
	}
}