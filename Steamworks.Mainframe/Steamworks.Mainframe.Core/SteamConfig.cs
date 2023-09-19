﻿// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.12

using System;

namespace Steamworks.Mainframe.Core
{
	//
	// The SteamManager provides a base implementation of Steamworks.NET on which you can build upon.
	// It handles the basics of starting up and shutting down the SteamAPI for use.
	//
	public static class SteamConfig
	{
		public static bool s_EverInitialized;
		public static bool Initialized { get; private set; }
		public static ILogger Logger { get; set; } = new ConsoleLogger();

		public static bool Init()
		{
			if (s_EverInitialized)
			{
				// This is almost always an error.
				// The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
				// and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
				// You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
				throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
			}

			if (!Packsize.Test())
			{
				Logger.LogError(
					"[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}

			if (!DllCheck.Test())
			{
				Logger.LogError(
					"[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}

			try
			{
				// If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
				// Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

				// Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
				// remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
				// See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
				if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
					return false;
			}
			catch (DllNotFoundException e)
			{
				// We catch this exception here, as it will be the first occurrence of it.
				Logger.LogError($"[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. " +
				                $"It's likely not in the correct location. Refer to the README for more details.\n{e}");
				return false;
			}

			// Initializes the Steamworks API.
			// If this returns false then this indicates one of the following conditions:
			// [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
			// [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
			// [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
			// [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
			// [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
			// Valve's documentation for this is located here:
			// https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
			Initialized = SteamAPI.Init();
			if (!Initialized)
			{
				Logger.LogError(
					"[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
				return false;
			}

			s_EverInitialized = true;
			return true;
		}

		// OnApplicationQuit gets called too early to shutdown the SteamAPI.
		// Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
		// Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
		public static void Shutdown()
		{
			if (Initialized)
				SteamAPI.Shutdown();
		}

		public static void Update()
		{
			// Run Steam client callbacks
			if (Initialized)
				SteamAPI.RunCallbacks();
		}
	}
}