using System;
using System.Collections.Generic;
using UnityEngine;

namespace Steamworks.Mainframe
{
	/// <summary>
	/// Quick property and methods to access some steam API
	/// </summary>
	public static class Steam
	{
		/// <summary>
		/// Returns if SteamManager is initialised
		/// </summary>
		public static bool Valid => SteamManager.Initialized;

		/// <summary>
		/// Id of the owner
		/// </summary>
		public static ulong SteamId => Valid ? SteamUser.GetSteamID().m_SteamID : 0;

		/// <summary>
		/// Username of the owner
		/// </summary>
		public static string Username => Valid ? SteamFriends.GetPersonaName() : string.Empty;

		/// <summary>
		/// The branch that steam was launched on 
		/// </summary>
		public static string Branch => SteamApps.GetCurrentBetaName(out var branchName, 32) ? branchName : string.Empty;
		
		
		public static string GetLaunchCommandLine()
		{
			if (!Valid)
				return string.Empty;

			var ret = SteamApps.GetLaunchCommandLine(out string cmdLine, 512);

			if (ret == -1)
				throw new Exception("GetLaunchCommandLine failed");

			Debug.Log($"GetLaunchCommandLine: '{cmdLine}'");

			return cmdLine?.Trim() ?? string.Empty;
		}
		
		public static IEnumerable<SteamFriend> GetFriends()
		{
			const EFriendFlags FLAGS = EFriendFlags.k_EFriendFlagAll;
			var count = SteamFriends.GetFriendCount(FLAGS);
			
			for (int i = 0; i < count; i++)
			{
				var friendId = SteamFriends.GetFriendByIndex(i, FLAGS);
				var username = SteamFriends.GetFriendPersonaName(friendId);
				var player = new SteamFriend(friendId.m_SteamID, username);
				yield return player;
			}
		}
	}
}