#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public class CheckWindow : EditorWindow
{
    private bool ok = false;
    public GUIStyle red, green;
    private String icon;

    [MenuItem("Window/Check")]
    public static void ShowWindow()
    {
        GetWindow<CheckWindow>("Check");
    }

    void createStyles()
    {
        if (red == null)
        {
            red = new GUIStyle(EditorStyles.label);
            red.normal.textColor = Color.red;
        }

        if (green == null)
        {
            green = new GUIStyle(EditorStyles.label);
            green.normal.textColor = Color.green;
        }
    }

    private void OnGUI()
    {
        createStyles();
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("Press here to run tests:");
        if (GUILayout.Button("Check", GUILayout.Width(100)))
        {
            icon = "";
            EditorScript.result = "";
            EditorScript.RunPlayModeTests();
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Stage:");
        int projectPathHash = Application.dataPath.GetHashCode();
        EditorPrefs.SetInt("Current stage" + projectPathHash.ToString(),
            EditorGUILayout.IntSlider(leftValue: 1,
                rightValue: EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()),
                value: EditorPrefs.GetInt("Current stage" + projectPathHash.ToString())));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Label("\nResult:");

        GUILayout.BeginHorizontal();
        if (ok)
        {
            GUILayout.Label(icon, green, GUILayout.Width(15));
        }
        else
        {
            GUILayout.Label(icon, red, GUILayout.Width(15));
        }

        GUILayout.Label(EditorScript.result, EditorStyles.wordWrappedLabel);
        GUILayout.EndHorizontal();

        if (ok)
        {
            GUILayout.Label("Your unique code for this stage:");
            GUILayout.TextArea(EditorScript.code);
            if (GUILayout.Button(text: "Copy"))
            {
                GUIUtility.systemCopyBuffer = EditorScript.code;
            }
        }

        Event e = Event.current;

        if (e.commandName == "FinishedWrong")
        {
            icon = "✖️";
            ok = false;
            Repaint();
        }

        if (e.commandName == "FinishedOk")
        {
            icon = "✔️";
            ok = true;
            Repaint();
        }
    }
}

public static class StagePreferences
{
    static int projectPathHash = Application.dataPath.GetHashCode();

    [PreferenceItem("Stage Preferences")]
    private static void OnPreferencesGUI()
    {
        int amount_stages = EditorPrefs.GetInt("Stages amount" + projectPathHash.ToString());
        int max_stage = EditorPrefs.GetInt("Max stage" + projectPathHash.ToString());
        int current_stage = EditorPrefs.GetInt("Current stage" + projectPathHash.ToString());
        EditorGUI.BeginDisabledGroup(true);
        amount_stages = EditorGUILayout.IntField(label: "Stages amount", value: amount_stages);
        max_stage = EditorGUILayout.IntField(label: "Max stage", value: max_stage);
        current_stage = EditorGUILayout.IntSlider(leftValue: 1, rightValue: max_stage, label: "Current stage",
            value: current_stage);
        EditorGUI.EndDisabledGroup();
        EditorPrefs.SetInt("Stages amount" + projectPathHash.ToString(), amount_stages);
        EditorPrefs.SetInt("Max stage" + projectPathHash.ToString(), max_stage);
        EditorPrefs.SetInt("Current stage" + projectPathHash.ToString(), current_stage);
    }
}
#endif