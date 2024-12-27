using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugMenu : MonoBehaviour
{
    public enum DebugSection
    {
        JoinCode,
        GameState,
        Other1,
        Other2
    }

    [SerializeField] private TextMeshProUGUI _debugMenuText;

    private static List<string> s_debugMenuTextSections = new List<string> { "Join Code: ABCDEF", "", "", "" };

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        WriteToDebugMenu(DebugSection.JoinCode, Locator.Instance.RelayManager.JoinCode);
    }

    /// <summary>
    /// Writes the given string to the given section of the debug menu.
    /// </summary>
    /// <param name="section">The section to write to.</param>
    /// <param name="text">The text to write.</param>
    public void WriteToDebugMenu(DebugSection section, string text)
    {
        var outputText = "";

        for (var i = 0; i < s_debugMenuTextSections.Count; i++)
        {
            if ((int)section == i)
            {
                switch (i)
                {
                    case (int)DebugSection.JoinCode: s_debugMenuTextSections[i] = $"Join Code: {text}"; break;
                    default: s_debugMenuTextSections[i] = text; break;
                }
            }
            
            outputText += $"{s_debugMenuTextSections[i]}\n";
        }

        _debugMenuText.text = outputText;
    }
}
