using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
[InitializeOnLoad]
static class MyEditorScript
{
    static int projectPathHash = Application.dataPath.GetHashCode( );
    static public TestRunnerApi testRunnerApi;
    static TestCallbacks testCallbacks= new TestCallbacks();

    static public String result = String.Empty;

    static public String code = String.Empty;
    
    static MyEditorScript()
    {
        testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        testRunnerApi.RegisterCallbacks(testCallbacks);
    }
    
    public static void RunPlayModeTests()
    {
        List<string> categories = new List<string>();
        for (int i = 1; i <= EditorPrefs.GetInt("Current stage"); i++)
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
        private bool next=true;
        public bool written = false;

        public void RunFinished(ITestResultAdaptor result)
        {
            if (!next && !written)
            {
                MyEditorScript.result = "Failed in test #" + testNum + "\n\n" + results;
                EditorWindow win = EditorWindow.GetWindow<CheckWindow>();
                win.SendEvent(EditorGUIUtility.CommandEvent("FinishedWrong"));
                written = true;
            }

            if (next)
            {
                MyEditorScript.result = "All tests passed!";
                if (EditorPrefs.GetInt("Current stage" + projectPathHash.ToString()) ==
                    EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()) &&
                    EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()) !=
                    EditorPrefs.GetInt("Stages amount" + projectPathHash.ToString()))
                {
                    EditorPrefs.SetInt("Max stage" + projectPathHash.ToString(),
                        EditorPrefs.GetInt("Max stage" + projectPathHash.ToString()) + 1);
                }
                string code_base = result.Children.First().Children.Last().Test.Description+"_"+
                                      result.Children.First().Children.Last().EndTime.ToUniversalTime();
                
                byte[] bytesToEncode = Encoding.UTF8.GetBytes (code_base);
                MyEditorScript.code = Convert.ToBase64String (bytesToEncode);
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
                if(next)
                    results=result.Message;
                next = false;
            }
        }

        public void TestStarted(ITestAdaptor test){ }
    }
}