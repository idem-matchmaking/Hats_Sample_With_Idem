using Beamable;
using Beamable.Experimental.Api.Matchmaking;
using Idem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.UI.Matchmaking
{
	public class IdemMatchmaking : IMatchmakingService
	{
		public MatchmakingState? State => idemService.IsMatchmaking
			? MatchmakingState.Searching
			: idemService.CurrentMatchInfo?.ready ?? false
				? MatchmakingState.Ready
				: MatchmakingState.Cancelled;
		
		public event Action<string, List<long>> OnMatchReady;
		public event Action OnTimedOut;

		private readonly string[] _servers = {"MainServer"};
		private readonly string gameModeId;
		private readonly IdemService idemService;

		public IdemMatchmaking(string gameModeId)
		{
			// this.gameModeId = gameModeId;
			this.gameModeId = "1v1";
			idemService = BeamContext.Default.IdemService();
			idemService.OnMatchReady += matchInfo =>
				OnMatchReady?.Invoke(matchInfo.matchId, matchInfo.players.Select(p => long.Parse(p.playerId)).ToList());
			idemService.OnMatchmakingStopped += () => OnTimedOut?.Invoke();
		}
		
		public async Task FindGame()
		{
			await idemService.StartMatchmaking(gameModeId, _servers);
		}

		public async Task Cancel()
		{
			await idemService.StopMatchmaking();
			OnTimedOut?.Invoke();
		}
	}
}