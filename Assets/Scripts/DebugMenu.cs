using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public enum DebugSection
    {
        JoinCode,
        GameState,
        Other1,
        Other2
    }

    public static DebugMenu Instance { get; private set; }

    [SerializeField] private TMPro.TextMeshProUGUI debugMenuText;
    private static List<string> debugMenuTextSections = new List<string> { "Join Code: ABCDEF", "", "", "" };

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        RelayManager relayManager = FindObjectOfType<RelayManager>();
        if (relayManager != null) WriteToDebugMenu(DebugSection.JoinCode, relayManager.JoinCode);
    }

    public void WriteToDebugMenu(DebugSection section, string text)
    {
        string outputText = "";

        for (int i = 0; i < debugMenuTextSections.Count; i++)
        {
            if ((int)section == i)
            {
                switch (i)
                {
                    case (int)DebugSection.JoinCode: debugMenuTextSections[i] = "Join Code: " + text; break;
                    default: debugMenuTextSections[i] = text; break;
                }
            }
            
            outputText += debugMenuTextSections[i] + "\n";
        }

        debugMenuText.text = outputText;
    }
}
