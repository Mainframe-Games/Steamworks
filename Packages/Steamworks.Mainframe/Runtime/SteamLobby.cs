using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks.Mainframe.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace Steamworks.Mainframe
{
	public static class SteamLobby
	{
		public static event Action<SteamLobbyInfo> OnLobbyUpdated;

		/// <summary>
		/// Filter for searching for lobbies.
		/// </summary>
		public const int MaxLobbies = 60;
		public static SteamLobbyInfo Current { get; private set; }

		private static TaskCompletionSource<ulong> _createLobbyTask;
		private static TaskCompletionSource<ulong> _joinLobbyTask;
		private static TaskCompletionSource<HashSet<ulong>> _lobbyIdsTask;

		[Preserve] private static Callback<LobbyCreated_t> _lobbyCreated;
		[Preserve] private static Callback<GameLobbyJoinRequested_t> _lobbyRequested;
		[Preserve] private static Callback<LobbyEnter_t> _lobbyEntered;
		[Preserve] private static Callback<LobbyMatchList_t> _lobbyListRequest;
		[Preserve] private static Callback<LobbyDataUpdate_t> _lobbyDataUpdated;

		public static void Init()
		{
			// create
			_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreatedCallback);

			// join
			_lobbyRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyRequest);
			_lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

			// get
			_lobbyListRequest = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
			_lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
		}

		#region Create

		public static async Task<SteamLobbyInfo> CreateLobbyAsync(int maxConnections, bool friendsOnly = false)
		{
			if (!Steam.Valid)
				throw new Exception("Steam not initialised");

			var lobbyType = friendsOnly
				? ELobbyType.k_ELobbyTypeFriendsOnly
				: ELobbyType.k_ELobbyTypePublic;

			_createLobbyTask = new TaskCompletionSource<ulong>();
			SteamMatchmaking.CreateLobby(lobbyType, maxConnections);
			var lobbyId = await _createLobbyTask.Task;
			
			Current = new SteamLobbyInfo(lobbyId)
			{
				HostId = Steam.SteamId,
				LobbyName = $"{Steam.Username}'s Game",
				AppVersion = Application.version,
				IsActive = true,
				MaxConnections = maxConnections,
			};

			UpdateLobbyPlayerCount(1);
			SteamRichPresence.SetConnect(Steam.SteamId.ToString());

			return Current;
		}

		private static void OnLobbyCreatedCallback(LobbyCreated_t callback)
		{
			if (callback.m_eResult != EResult.k_EResultOK)
			{
				Debug.LogError("Failed to created lobby");
				return;
			}

			Debug.Log($"Lobby created: {callback.m_ulSteamIDLobby}");
			_createLobbyTask.SetResult(callback.m_ulSteamIDLobby);
		}

		#endregion

		#region Join

		public static async Task<SteamLobbyInfo> JoinLobbyAsync(ulong lobbyId)
		{
			if (!Steam.Valid)
				throw new Exception("Steam not initialised");

			_joinLobbyTask = new TaskCompletionSource<ulong>();
			SteamMatchmaking.JoinLobby((CSteamID)lobbyId);
			Current = new SteamLobbyInfo(lobbyId);
			await _joinLobbyTask.Task;
			return Current;
		}

		private static void OnLobbyRequest(GameLobbyJoinRequested_t callback)
		{
			Debug.Log($"Lobby request from {callback.m_steamIDFriend}");
			SteamMatchmaking.JoinLobby((CSteamID)Current.LobbyId);
		}

		private static void OnLobbyEntered(LobbyEnter_t callback)
		{
			Debug.Log($"Lobby Entered: {callback.m_ulSteamIDLobby}");
			_joinLobbyTask.SetResult(0);
		}

		#endregion

		#region Leave

		public static void LeaveLobby()
		{
			if (Steam.Valid)
				SteamMatchmaking.LeaveLobby((CSteamID)Current.LobbyId);
		}

		#endregion

		#region Lobby List

		public static async Task<List<SteamLobbyInfo>> GetLobbyListAsync(bool friendsOnly)
		{
			HashSet<ulong> lobbyIds;

			if (friendsOnly)
			{
				lobbyIds = new HashSet<ulong>();
				foreach (var lobbyId in GetLobbyIdsFromFriends())
					lobbyIds.Add(lobbyId);
			}
			else
			{
				_lobbyIdsTask = new TaskCompletionSource<HashSet<ulong>>();
				SteamMatchmaking.AddRequestLobbyListResultCountFilter(MaxLobbies);
				SteamMatchmaking.RequestLobbyList();
				lobbyIds = await _lobbyIdsTask.Task;
			}

			return lobbyIds
				.Select(x => new SteamLobbyInfo(x))
				.Where(info => info.IsActive && info.AppVersion == Application.version)
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
				{
					SteamMatchmaking.RequestLobbyData(friendGameInfo.m_steamIDLobby);
					yield return friendGameInfo.m_steamIDLobby.m_SteamID;
				}
			}
		}

		private static void OnLobbyMatchList(LobbyMatchList_t callback)
		{
			if (_lobbyIdsTask == null)
				throw new NullReferenceException($"{nameof(_lobbyIdsTask)} is null");

			var lobbyIds = new HashSet<ulong>();
			for (int i = 0; i < callback.m_nLobbiesMatching; i++)
			{
				var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
				lobbyIds.Add((ulong)lobbyId);
				SteamMatchmaking.RequestLobbyData(lobbyId);
			}

			_lobbyIdsTask.SetResult(lobbyIds);
		}

		#endregion

		#region Updates

		private static void OnLobbyDataUpdated(LobbyDataUpdate_t param)
		{
			var info = new SteamLobbyInfo(param.m_ulSteamIDLobby);
			OnLobbyUpdated?.Invoke(info);
		}

		private static void UpdateLobbyPlayerCount(int playerCount)
		{
			Current.PlayerCount = playerCount;
			SteamRichPresence.SetGroup(playerCount);
		}

		#endregion
	}
}