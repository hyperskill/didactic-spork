#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

[InitializeOnLoad]
static class EditorScript
{
    static int projectPathHash = Application.dataPath.GetHashCode();
    static public TestRunnerApi testRunnerApi;
    static TestCallbacks testCallbacks = new TestCallbacks();

    static public String result = String.Empty;

    static public String code = String.Empty;

    static EditorScript()
    {
        testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        testRunnerApi.RegisterCallbacks(testCallbacks);
    }

    [InitializeOnLoadMethod]
    static void SetDefault()
    {
        if (!EditorPrefs.HasKey("Stages amount" + projectPathHash.ToString()) ||
            EditorPrefs.GetInt("Max stage") > EditorPrefs.GetInt("Stages amount"))
        {
            EditorPrefs.SetInt("Max stage" + projectPathHash.ToString(), 1);
            EditorPrefs.SetInt("Current stage" + projectPathHash.ToString(), 1);
        }

        List<String> categories = new List<String>();
        testRunnerApi.RetrieveTestList(TestMode.PlayMode, (testRoot) =>
        {
            if (testRoot.HasChildren && testRoot.Children.First().HasChildren)
                foreach (var ch in testRoot.Children.First().Children)
                {
                    if (ch.Categories.Length != 0 && !categories.Contains(ch.Categories[0]))
                        categories.Add(ch.Categories[0]);
                }

            //Debug.Log(categories.Count);
            EditorPrefs.SetInt("Stages amount" + projectPathHash.ToString(), categories.Count);
        });
    }

    public static void RunPlayModeTests()
    {
        List<string> categories = new List<string>();
        for (int i = 1; i <= EditorPrefs.GetInt("Current stage" + projectPathHash.ToString()); i++)
        {
            categories.Add(i.ToString());
        }

        RunTests(TestMode.PlayMode, categories.ToArray());
    }

    private static void RunTests(TestMode testModeToRun, string[] categories)
    {
        testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        testRunnerApi.RegisterCallbacks(testCallbacks);

        var filter = new Filter()
        {
            testMode = testModeToRun,
            categoryNames = categories
        };

        testRunnerApi.Execute(new ExecutionSettings(filter));
    }

    private class TestCallbacks : ICallbacks
    {
        public String results = String.Empty;
        private int testNum = 0;
        private bool next = true;
        public bool written = false;

        public void RunFinished(ITestResultAdaptor result)
        {
            if (!next && !written)
            {
                EditorScript.result = "Failed in test #" + testNum + "\n\n" + results;
                EditorWindow win = EditorWindow.GetWindow<CheckWindow>();
                win.SendEvent(EditorGUIUtility.CommandEvent("FinishedWrong"));
                written = true;
            }

            if (next)
            {
                EditorScript.result = "All tests passed!";
                if (EditorPrefs.GetInt("Current stage" + projectPathHash.ToString()) ==
                    EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()) &&
                    EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()) !=
                    EditorPrefs.GetInt("Stages amount" + projectPathHash.ToString()))
                {
                    EditorPrefs.SetInt("Max stage" + projectPathHash.ToString(),
                        EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()) + 1);
                }

                DateTime dt = result.Children.First().Children.Last().EndTime.ToUniversalTime();
                string code_base = result.Children.First().Children.Last().Test.Description + "_" +
                                   dt.ToString("dd.MM.yyyy HH:mm:ss");

                byte[] bytesToEncode = Encoding.UTF8.GetBytes(code_base);
                EditorScript.code = Convert.ToBase64String(bytesToEncode);
                EditorWindow win = EditorWindow.GetWindow<CheckWindow>();
                win.SendEvent(EditorGUIUtility.CommandEvent("FinishedOk"));
            }
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            testNum = 0;
            results = String.Empty;
            next = true;
            written = false;
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (next)
            {
                testNum++;
            }

            if (result.TestStatus == TestStatus.Failed)
            {
                VInput.ReleaseAll();
                if (next)
                    results = result.Message;
                next = false;
            }
        }

        public void TestStarted(ITestAdaptor test)
        {
        }
    }
}
#endif