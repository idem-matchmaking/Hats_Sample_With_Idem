using Beamable.Experimental.Api.Matchmaking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.UI.Matchmaking
{
	public interface IMatchmakingService
	{
		bool IsSearching => State == MatchmakingState.Searching;
		MatchmakingState? State { get; }
		
		event Action<string, List<long>> OnMatchReady;
		event Action OnTimedOut;
		
		Task FindGame();
		Task Cancel();
	}
}