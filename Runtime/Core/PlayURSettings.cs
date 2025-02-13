using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlayUR
{
    public class PlayURSettings : ScriptableObject
    {
        public const string ResourcePath = "PlayURSettings";
        public const string SettingsPath = "Assets/PlayURPlugin/Resources/"+ResourcePath+".asset";

        [SerializeField]
        private int gameId;
        public int GameID => gameId;

        /// <summary>
        /// this automagically creates a session handler (using <see cref="PlayURSessionTracker"/>), otherwise if false need to manually called StartSession etc
        /// </summary>
        public bool standardSessionTracking = true;

        /// <summary>
        /// Override the PlayUR Platform's automatic choosing of an experiment on mobile builds by forcing the use of <see cref="mobileExperiment"/>
        /// </summary>
        public bool useSpecificExperimentForMobileBuild = false;

        /// <summary>
        /// The experiment to choose if <see cref="useSpecificExperimentForMobileBuild"/> is true.
        /// </summary>
        public Experiment mobileExperiment;

        /// <summary>
        /// Override the PlayUR Platform's automatic choosing of an experiment on desktop builds by forcing the use of <see cref="desktopExperiment"/>
        /// </summary>
        public bool useSpecificExperimentForDesktopBuild = false;

        /// <summary>
        /// The experiment to choose if <see cref="useSpecificExperimentForDesktopBuild"/> is true.
        /// </summary>
        public Experiment desktopExperiment;
        [TextArea(3, 3)]
        /// <summary>
        /// What message should be shown to the user when they complete the game as a Amazon Mechanical Turk user. \
        /// This value can (and should) be set at any time.
        /// </summary>
        public string mTurkCompletionMessage = "HiT Completed\nCode: 6226";

        /// <summary>
        /// For use in-editor only, this allows us to test the game with the Experiment defined in <see cref="experimentToTestInEditor"/>.
        /// </summary>
        public bool forceToUseSpecificExperiment = false;

        /// <summary>
        /// For use in-editor only, this allows us to test the game with a specific <see cref="Experiment"/>.
        /// </summary>
        public Experiment experimentToTestInEditor;

        /// <summary>
        /// For use in-editor only, this allows us to test the game with the ExperimentGroup defined in <see cref="groupToTestInEditor"/>.
        /// </summary>
        public bool forceToUseSpecificGroup = false;

        /// <summary>
        /// For use in-editor only, this allows us to test the game with a specific <see cref="ExperimentGroup"/>.
        /// </summary>
        public ExperimentGroup groupToTestInEditor;

        /// <summary>
        /// For use in-editor only, this allows us to test the game with a given Amazon Mechanical Turk ID.
        /// </summary>
        public string forceMTurkIDInEditor = null;

        /// <summary>
        /// The prefab to use that represents the highscore table. You can link this to the pre-made prefab in the PlayUR/HighScores folder, or create your own.
        /// </summary>
        public GameObject defaultHighScoreTablePrefab;

        /// <summary>
        /// The prefab to use that represents a dialog popup. You can link this to the pre-made prefab in the PlayUR folder, or create your own.
        /// </summary>
        public GameObject defaultPopupPrefab;

        /// <summary>
        /// The prefab to use that represents a survey popup window. You can link this to the pre-made prefab in the PlayUR/Survey folder, or create your own.
        /// </summary>
        public GameObject defaultSurveyPopupPrefab;

        /// <summary>
        /// The prefab to use that represents a survey row item. You can link this to the pre-made prefab in the PlayUR/Survey folder, or create your own.
        /// </summary>
        public GameObject defaultSurveyRowPrefab;

        /// <summary>
        /// The sprite asset representing the Amazon Mechanical Turk Logo. You can link this to the logo in the PlayUR/Murk folder.
        /// </summary>
        public Sprite mTurkLogo;

        /// <summary>
        /// The minimum log level to store in the PlayUR Platform. This is useful if you want to ignore certain log messages.
        /// </summary>
        public PlayURPlugin.LogLevel minimumLogLevelToStore = PlayURPlugin.LogLevel.Log;

        /// <summary>
        /// The minimum level to log to the console.
        /// </summary>
        public PlayURPlugin.LogLevel logLevel = PlayURPlugin.LogLevel.Log;

#if UNITY_EDITOR
        internal static PlayURSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PlayURSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PlayURSettings>();
                settings.gameId = 0;
                settings.minimumLogLevelToStore = PlayURPlugin.LogLevel.Log;
                settings.logLevel = PlayURPlugin.LogLevel.Log;

                var runtimeFolder = Path.Combine("Packages","io.playur.unity", "Runtime");
                settings.defaultHighScoreTablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(runtimeFolder, "HighScores", "HighScoreTable.prefab"));
                settings.defaultPopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(runtimeFolder, "Popups", "PopupCanvas.prefab"));
                settings.defaultSurveyPopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(runtimeFolder, "Survey", "SurveyPopupPrefab.prefab"));
                settings.defaultSurveyRowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(runtimeFolder, "Survey", "SurveyRowPrefab.prefab"));
                settings.mTurkLogo = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(runtimeFolder, "MTurk", "mturk.png"));


                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}