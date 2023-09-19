using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Steamworks.Mainframe.Core
{
	/// <summary>
	/// Wrapper for lobby
	/// </summary>
	public static class SteamLobby
	{
		public static event Action<string> OnJoinRequested;
		
		private static TaskCompletionSource<HashSet<ulong>> _lobbyCodesTask;
		[Preserve] private static Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;

		[RuntimeInitializeOnLoadMethod]
		public static void Init()
		{
			_gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested_t);
		}
		
		private static void OnGameLobbyJoinRequested_t(GameLobbyJoinRequested_t param)
		{
			var connect = SteamRichPresence.GetFriendConnect((ulong)param.m_steamIDFriend);
			OnJoinRequested?.Invoke(connect);
		}
		
		/// <summary>
		/// Returns Lobby Code from friends rich presence
		/// </summary>
		/// <param name="friendsOnly"></param>
		/// <param name="maxLobbies"></param>
		/// <returns></returns>
		public static async Task<List<string>> GetLobbyListAsync(bool friendsOnly, int maxLobbies = 60)
		{
			HashSet<ulong> steamPlayerIds;
			
			if (friendsOnly)
			{
				steamPlayerIds = new HashSet<ulong>();
				foreach (var lobbyId in GetLobbyIdsFromFriends())
					steamPlayerIds.Add(lobbyId);
			}
			else
			{
				_lobbyCodesTask = new TaskCompletionSource<HashSet<ulong>>();
				SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxLobbies);
				SteamMatchmaking.RequestLobbyList();
				steamPlayerIds = await _lobbyCodesTask.Task;
			}

			// return lobby codes
			return steamPlayerIds
				.Select(SteamRichPresence.GetFriendConnect)
				// .Where(info => info.IsActive && info.AppVersion == App.VersionString)
				.ToList();
		}
		
		/// <summary>
		/// Docs: https://partner.steamgames.com/doc/features/multiplayer/matchmaking
		/// </summary>
		private static IEnumerable<ulong> GetLobbyIdsFromFriends()
		{
			var cFriends = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			for (int i = 0; i < cFriends; i++)
			{
				var steamIDFriend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
				var isInGame = SteamFriends.GetFriendGamePlayed(steamIDFriend, out var friendGameInfo);
				var isThisGame = friendGameInfo.m_gameID.m_GameID == Steam.AppId;
				if (isInGame && isThisGame && friendGameInfo.m_steamIDLobby.IsValid())
					yield return friendGameInfo.m_steamIDLobby.m_SteamID;
			}
		}
		
	}
}