﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEditor.Build.Reporting;
using Ionic.Zip; // this uses the Unity port of DotNetZip https://github.com/r2d2rigo/dotnetzip-for-unity
using Unity.EditorCoroutines.Editor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Text;
using PlayUR.Core;
using PlayUR.Exceptions;
using System.Diagnostics;

namespace PlayUR.Editor
{
    public class PlayURPluginEditor : MonoBehaviour
    {
        #region Initial Set Up
        public static string PluginLocation = "Packages/io.playur.unity/";
        static string generatedFilesPath = Path.Combine("Assets", "PlayURPlugin");
        static string GeneratedFilesPath(string subPath)
        {
            return Path.Combine(generatedFilesPath, subPath);
        }

        [MenuItem("PlayUR/Plugin Configuration...",priority=1)]
        public static void ReSetUpPlugin()
        {
            SettingsService.OpenProjectSettings("Project/PlayUR");
        }

        //Set PlayURPlugin to run first
        static void SetExecutionOrder()
        {
            //TODO:set order
            /*
            MonoScript playURScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            MonoImporter.SetExecutionOrder(playURScript, -10000);

            playURScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path.Replace("PlayURPlugin.cs", "Core/PlayURPluginHelper.cs"));
            MonoImporter.SetExecutionOrder(playURScript, -9999);
            */
            PlayURPlugin.Log("Set execution order of PlayUR Plugin to -10000");
        }


        //add PlayURLogin to Build Settings
        static string LoginScenePath => Path.Combine("Assets", "PlayURLogin.unity");
        public static void SetSceneBuildSettings()
        {
            var scenePath = PluginLocation + "Runtime/" + LoginScenePath;
            var scenes = new List<EditorBuildSettingsScene>();
            scenes.AddRange(EditorBuildSettings.scenes);

            //check it doesn't already exist
            for (var i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path.Contains("PlayURLogin"))
                {
                    return;
                }
            }
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            PlayURPlugin.Log("Added 'PlayURLogin' to Build Settings");

        }

        [MenuItem("PlayUR/Re-generate Enums", secondaryPriority = 0)]
        public static void GenerateEnum()
        {
            var GET = "?gameID=" + PlayURPlugin.GameID + "&clientSecret=" + PlayURPlugin.ClientSecret;
            //get actions from the server and populate an enum
            EditorCoroutineUtility.StartCoroutine(Rest.Get("Action/listForGame.php" + GET, null, (succ, json) =>
            {
                if (succ)
                {
                    var actions = json["records"].AsArray;
                    string text = "namespace PlayUR\n{\n\t///<summary>Enum generated from server representing possible user actions. To update use PlayUR\\Re-generate Enums.</summary>\n\tpublic enum Action\n\t{\n";
                    foreach (var action in actions.Values)
                    {
                        text += "\t\t" + PlatformNameToValidEnumValue(action["name"].Value) + " = " + action["id"] + ",\n";
                    }
                    text += "\t}\n}\n";

                    //write it out!
                    File.WriteAllBytes(GeneratedFilesPath("Action.cs"), Encoding.UTF8.GetBytes(text));
                    AssetDatabase.Refresh();

                    PlayURPlugin.Log("Generated Actions Enum (" + actions.Count + " actions)");
                }
            }), new CoroutineRunner());

            //get elements from the server and populate an enum
            EditorCoroutineUtility.StartCoroutine(Rest.Get("Element/listForGame.php" + GET, null, (succ, json) =>
            {
                if (succ)
                {
                    var elements = json["records"].AsArray;
                    Debug.Log(elements);
                    string text = "namespace PlayUR\n{\n\t///<summary>Enum generated from server representing top-level game elements. To update use PlayUR\\Re-generate Enums.</summary>\n\tpublic enum Element\n\t{\n";
                    foreach (var element in elements.Values)
                    {
                        text += "\t\t" + PlatformNameToValidEnumValue(element["name"].Value) + " = " + element["id"] + ",\n";
                    }
                    text += "\t}\n}\n";

                    //write it out!
                    File.WriteAllBytes(GeneratedFilesPath("Element.cs"), Encoding.UTF8.GetBytes(text));
                    AssetDatabase.Refresh();

                    PlayURPlugin.Log("Generated Elements Enum (" + elements.Count + " actions)");
                }
            }), new CoroutineRunner());

            //get experiments from the server and populate an enum
            EditorCoroutineUtility.StartCoroutine(Rest.Get("Experiment/listForGame.php" + GET, null, (succ, json) =>
            {
                if (succ)
                {
                    var experiments = json["records"].AsArray;
                    Debug.Log(experiments);
                    string text = "namespace PlayUR\n{\n\t///<summary>Enum generated from server representing experiments for this game. To update use PlayUR\\Re-generate Enums.</summary>\n\tpublic enum Experiment\n\t{\n";
                    foreach (var experiment in experiments.Values)
                    {
                        text += "\t\t" + PlatformNameToValidEnumValue(experiment["name"].Value) + " = " + experiment["id"] + ",\n";
                    }
                    text += "\t}\n}\n";

                    //write it out!
                    File.WriteAllBytes(GeneratedFilesPath("Experiment.cs"), Encoding.UTF8.GetBytes(text));
                    AssetDatabase.Refresh();

                    PlayURPlugin.Log("Generated Experiments Enum (" + experiments.Count + " experiments)");
                }
            }), new CoroutineRunner());

            //get experiment groups from the server and populate an enum
            EditorCoroutineUtility.StartCoroutine(Rest.Get("ExperimentGroup/listForGame.php" + GET, null, (succ, json) =>
            {
                if (succ)
                {
                    var experiments = json["records"].AsArray;
                    Debug.Log(experiments);
                    string text = "namespace PlayUR\n{\n\t///<summary>Enum generated from server representing experiment groups for this game. To update use PlayUR\\Re-generate Enums.</summary>\n\tpublic enum ExperimentGroup\n\t{\n";
                    foreach (var experiment in experiments.Values)
                    {
                        text += "\t\t" + PlatformNameToValidEnumValue(experiment["experiment"].Value) + "_" + experiment["name"].Value.Replace(" ", "") + " = " + experiment["id"] + ",\n";
                    }
                    text += "\t}\n}\n";

                    //write it out!
                    File.WriteAllBytes(GeneratedFilesPath("ExperimentGroup.cs"), Encoding.UTF8.GetBytes(text));
                    AssetDatabase.Refresh();

                    PlayURPlugin.Log("Generated Experiment Groups Enum (" + experiments.Count + " groups)");
                }
            }), new CoroutineRunner());

            //get analytics columns from the server and populate an enum
            EditorCoroutineUtility.StartCoroutine(Rest.Get("AnalyticsColumn/listForGame.php" + GET, null, (succ, json) =>
            {
                if (succ)
                {
                    var columns = json["records"].AsArray;
                    Debug.Log(columns);
                    string text = "namespace PlayUR\n{\n\t///<summary>Enum generated from server representing the extra analytics columns used for this game. To update use PlayUR\\Re-generate Enums.</summary>\n\tpublic enum AnalyticsColumn\n\t{\n";
                    foreach (var column in columns.Values)
                    {
                        text += "\t\t" + PlatformNameToValidEnumValue(column["name"].Value) + " = " + column["id"] + ",\n";
                    }
                    text += "\t}\n}\n";

                    //write it out!
                    File.WriteAllBytes(GeneratedFilesPath("AnalyticsColumns.cs"), Encoding.UTF8.GetBytes(text));
                    AssetDatabase.Refresh();

                    PlayURPlugin.Log("Generated Analytics Columns Enum (" + columns.Count + " columns)");
                }
            }), new CoroutineRunner());

            //get all parameter keys from the server and populate an enum
            EditorCoroutineUtility.StartCoroutine(Rest.Get("GameParameter/listParameterKeys.php" + GET, null, (succ, json) =>
            {
                if (succ)
                {
                    var parameters = json["records"].AsArray;
                    Debug.Log(parameters);
                    string text = "namespace PlayUR\n{\n\t///<summary>Constant Strings generated from server representing the parameter keys for this game. To update use PlayUR\\Re-generate Enums.</summary>\n\tpublic static class Parameter\n\t{\n";
                    foreach (var parameter in parameters.Values)
                    {
                        text += "\t\tpublic static string " + PlatformNameToValidEnumValue(parameter.ToString().Replace("[]", "")) + " = \"" + PlatformNameToValidEnumValue(parameter.ToString().Replace("[]", "")) + "\";\n";
                    }
                    text += "\t}\n}\n";

                    //write it out!
                    File.WriteAllBytes(GeneratedFilesPath("Parameter.cs"), Encoding.UTF8.GetBytes(text));
                    AssetDatabase.Refresh();

                    PlayURPlugin.Log("Generated Parameters Constants (" + parameters.Count + " parameters)");
                }
            }), new CoroutineRunner());
        }
        static string PlatformNameToValidEnumValue(string input)
        {
            var rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9_]");
            input = rgx.Replace(input, "");
            input = input.Replace(" ", "");
            if (!char.IsLetter(input[0])) input = "_" + input;
            return input;
        }
        #endregion

        #region BuildTools
        class CoroutineRunner { }
        static string GetBuildPath()
        {
            return EditorUtility.SaveFolderPanel("Build out WebGL to...",
                                                        Application.dataPath + "/build/",
                                                        "");
        }
        [MenuItem("PlayUR/Build Web Player", secondaryPriority = 2)]
        public static void BuildWebPlayer()
        {
            string path = GetBuildPath(); if (string.IsNullOrEmpty(path)) return;
            BuildPlayer(BuildTargetGroup.WebGL, BuildTarget.WebGL, path, onlyUpload: false, upload: false);
        }
        [MenuItem("PlayUR/Build and Upload Web Player", secondaryPriority = 1)]
        public static void BuildAndUploadWebPlayer()
        {
            string path = GetBuildPath(); if (string.IsNullOrEmpty(path)) return;
            BuildPlayer(BuildTargetGroup.WebGL, BuildTarget.WebGL, path, onlyUpload: false, upload: true);
        }
        [MenuItem("PlayUR/Upload Web Player", secondaryPriority = 3)]
        public static void UploadWebPlayer()
        {
            string path = GetBuildPath(); if (string.IsNullOrEmpty(path)) return;
            BuildPlayer(BuildTargetGroup.WebGL, BuildTarget.WebGL, path, onlyUpload: true, upload: true);
        }


        // this is the main player builder function
        static void BuildPlayer(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string buildPath, bool onlyUpload = false, bool upload = true)
        {
            Debug.ClearDeveloperConsole();
            if (onlyUpload == false)
            {
                PlayURPlugin.Log("====== BuildPlayer: " + buildTarget.ToString() + " at " + buildPath);

                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

                //turning off build just this second
                BuildReport report = BuildPipeline.BuildPlayer(GetScenePaths(), buildPath, buildTarget, BuildOptions.None);
                BuildResult result = report.summary.result;
                if (result != BuildResult.Succeeded)
                {
                    EditorUtility.DisplayDialog("PlayUR Plugin", "Build Failed! Check the log.", "OK");
                    return;
                }
            }
            // ZIP everything
            CompressDirectory(buildPath + "/", buildPath + "/index.zip");

            if (upload || onlyUpload)
            {
                //ask the user for the branch name
                PopupInput.Open("Build Branch Name", "Enter the branch to upload this build to.", (branch, cancelled) =>
                {
                    if (cancelled == false)
                    {
                        branch = string.IsNullOrEmpty(branch) ? "main" : branch;
                        PlayURPlugin.Log("Selected branch: " + branch);

                        //upload to server
                        EditorCoroutineUtility.StartCoroutine(UploadBuild(buildPath + "/index.zip", branch, (succ2, json) =>
                        {
                            if (succ2)
                            {
                                PlayURPlugin.Log("Build Uploaded!");

                                if (EditorUtility.DisplayDialog("PlayUR Plugin", $"Build Uploaded to Branch `{branch}`. Do you want to open it in your browser?", "Yes", "No"))
                                {
                                    OpenGameInBrowser();
                                }
                            }
                            else
                            {
                                PlayURPlugin.LogError("Build Failed...");
                                PlayURPlugin.LogError(json.ToString());
                                EditorUtility.DisplayDialog("PlayUR Plugin", "Build Failed. " + json.ToString(), "OK");
                            }
                        }), new CoroutineRunner());
                    }

                }, defaultText: "main");//TODO: remember last time a branch was used?

            }
        }
        [MenuItem("PlayUR/Run Game In Broswer")]
        public static void OpenGameInBrowser()
        {
            //get the latest build id, so that we can open it up in unity
            var form = GetGameIDForm();
            EditorCoroutineUtility.StartCoroutine(Rest.Get("Build/latestBuildID.php", form, (succ, result) =>
            {
                int buildID = result["latestBuildID"];
                Application.OpenURL(PlayURPlugin.SERVER_URL.Replace("/api/", "/games.php?/game/" + form["clientSecret"] + "/buildID/" + buildID));
            }, debugOutput: true), new CoroutineRunner());
        }

        static string[] GetScenePaths()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }
        // compress the folder into a ZIP file, uses https://github.com/r2d2rigo/dotnetzip-for-unity
        static void CompressDirectory(string directory, string zipFileOutputPath)
        {
            PlayURPlugin.Log("attempting to compress " + directory + " into " + zipFileOutputPath);
            if (File.Exists(zipFileOutputPath))
                File.Delete(zipFileOutputPath);

            // display fake percentage, I can't get zip.SaveProgress event handler to work for some reason, whatever
            EditorUtility.DisplayProgressBar("COMPRESSING... please wait", zipFileOutputPath, 0.38f);
            using (ZipFile zip = new ZipFile())
            {
                zip.ParallelDeflateThreshold = -1; // DotNetZip bugfix that corrupts DLLs / binaries http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                zip.AddDirectory(directory);
                zip.Save(zipFileOutputPath);
            }
            EditorUtility.ClearProgressBar();
        }
        static IEnumerator UploadBuild(string zipPath, string branch, Rest.ServerCallback callback)
        {
            var form = GetGameIDForm();
            form.Add("branch", branch);

            yield return EditorCoroutineUtility.StartCoroutine(Rest.Get("Build/latestBuildID.php", form, (succ, result) =>
            {
                var newBuildID = int.Parse(result["latestBuildID"]) + 1;
                EditorCoroutineUtility.StartCoroutine(UploadBuildPart2(zipPath, branch, callback, form["gameID"], form["clientSecret"], newBuildID), new CoroutineRunner());
            }), new CoroutineRunner());
        }
        static IEnumerator UploadBuildPart2(string zipPath, string branch, Rest.ServerCallback callback, string gameID, string clientSecret, int newBuildID)
        {
            PlayURPlugin.Log("New Build ID: " + newBuildID);
            PlayURPlugin.Log("Branch: " + branch);
            PlayURPlugin.Log(PlayURPlugin.SERVER_URL + "Build/");

            JSONObject jsonSend = new JSONObject();
            jsonSend["gameID"] = gameID;
            jsonSend["clientSecret"] = clientSecret;
            jsonSend["buildID"] = newBuildID;
            jsonSend["branch"] = branch;
            //PlayURPlugin.Log("JSON: " + jsonSend.ToString());

            yield return EditorCoroutineUtility.StartCoroutine(UploadFile("Build/", zipPath, "index.zip", "application/zip", jsonSend, callback), new CoroutineRunner());
        }
        #endregion

        #region utils
        static IEnumerator UploadFile(string endPoint, string filePath, string fileName, string mimeType, JSONObject additionalRequest = null, Rest.ServerCallback callback = null)
        {
            WWWForm form = new WWWForm();
            form.AddField("gameID", PlayURPlugin.GameID);
            form.AddField("clientSecret", PlayURPlugin.ClientSecret);
            form.AddBinaryData("file", File.ReadAllBytes(filePath), fileName, mimeType);

            if (additionalRequest != null) form.AddField("request", additionalRequest.ToString());

            using (var www = UnityWebRequest.Post(PlayURPlugin.SERVER_URL + endPoint, form))
            {
                yield return www.SendWebRequest();

                JSONNode json;
                if (www.isNetworkError)
                {
                    throw new ServerCommunicationException(www.error);
                }
                else if (www.isHttpError)
                {
                    json = JSON.Parse(www.downloadHandler.text);
                    PlayURPlugin.LogError("Response Code: " + www.responseCode);
                    PlayURPlugin.LogError(json);
                    //if (callback != null) callback(false, json["message"]);
                    //yield break;
                }
                PlayURPlugin.Log(www.downloadHandler.text);

                try
                {
                    json = JSON.Parse(www.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    throw new ServerCommunicationException("JSON Parser Error: " + e.Message);
                }


                if (json == null)
                {
                    PlayURPlugin.Log("json == null, Response Code: " + www.responseCode);
                    if (callback != null) callback(false, "Unknown error: " + www.downloadHandler.text);
                    yield break;
                }
                if (json["success"] != null)
                {
                    if (json["success"].AsBool != true)
                    {
                        if (callback != null) callback(false, json["message"]);
                        yield break;
                    }
                }

                if (callback != null) callback(true, json);

            }
        }

        static Dictionary<string, string> GetGameIDForm()
        {

            //first, grab the latest build number for this game
            int gameID = PlayURPlugin.GameID;
            if (gameID == -1)
                return null;

            string clientSecret = PlayURPlugin.ClientSecret;
            if (string.IsNullOrEmpty(clientSecret))
                return null;

            PlayURPlugin.Log("Game ID: " + gameID + ", Client Secret: " + clientSecret);

            //then get the latest build id, so we can upload the next one in sequence
            Dictionary<string, string> gameIDForm = new Dictionary<string, string>();
            gameIDForm.Add("gameID", gameID.ToString());
            gameIDForm.Add("clientSecret", clientSecret);
            return gameIDForm;
        }
        #endregion

        [MenuItem("PlayUR/Clear PlayerPrefs (Local Only)", secondaryPriority = 100)] 
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.Clear();
        }
    }

}
