using System.Text;
using Steamworks.Mainframe.Core;
using TMPro;
using UnityEngine;

public class SteamworksDemo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    
    private void Start()
    {
        var str = new StringBuilder();

        str.AppendLine("AppId: {}");
        str.AppendLine($"SteamId: {Steam.SteamId}");
        str.AppendLine($"Username: {Steam.Username}");

        _text.text = str.ToString();
    }
}
