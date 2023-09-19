using UnityEditor;

namespace Packages.Steamworks.Mainframe.Editor
{
	public static class SteamEditor
	{
        private const string MenuName = "Tools/Steamworks/Initialise in editor";
        private const string PrefsKey = "Steamworks.InitialiseInEditor";

        private static bool InitialiseSteam
        {
            get => EditorPrefs.GetBool(PrefsKey, false);
            set => EditorPrefs.SetBool(PrefsKey, value);
        }
         
        [MenuItem(MenuName)]
        private static void ToggleAction()
        {
            InitialiseSteam = !InitialiseSteam;
        }
 
        [MenuItem(MenuName, true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, InitialiseSteam);
            return true;
        }
	}
}