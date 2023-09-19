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
	}
}