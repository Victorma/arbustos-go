using UnityEngine;
using UnityEditor;

using uAdventure.Core;
using uAdventure.Editor;
using UnityEditor.SceneManagement;

namespace uAdventure.Analytics
{

    public class SurveyManagerWindow : LayoutWindow
    {
        private SceneAsset previousScene;

        public SurveyManagerWindow(Rect aStartPos, GUIContent aContent, GUIStyle aStyle, params GUILayoutOption[] aOptions)
            : base(aStartPos, aContent, aStyle, aOptions)
        {
            var _ = AnalyticsController.Instance;
            var surveyManagerConfig = AnalyticsController.Instance.SurveyManagerConfig;
            SetScene(surveyManagerConfig.ShowSurveyOnStart);
        }

        public override void Draw(int aID)
        {
            var surveyManagerConfig = AnalyticsController.Instance.SurveyManagerConfig;

            // Show Survey On Start
            EditorGUI.BeginChangeCheck();
            var showSurveyOnStart = EditorGUILayout.Toggle(new GUIContent("Show Survey On Start"), surveyManagerConfig.ShowSurveyOnStart);
            if (EditorGUI.EndChangeCheck())
            {
                surveyManagerConfig.ShowSurveyOnStart = showSurveyOnStart;
                SetScene(surveyManagerConfig.ShowSurveyOnStart);
            }

            // Start Survey
            EditorGUI.BeginChangeCheck();
            var startSurvey = EditorGUILayout.IntField("Start Survey", surveyManagerConfig.StartSurvey);
            if (EditorGUI.EndChangeCheck())
            {
                surveyManagerConfig.StartSurvey = startSurvey;
            }

            // End Survey
            EditorGUI.BeginChangeCheck();
            var endSurvey = EditorGUILayout.IntField("End Survey", surveyManagerConfig.EndSurvey);
            if (EditorGUI.EndChangeCheck())
            {
                surveyManagerConfig.EndSurvey =  endSurvey;
            }
        }

        private void SetScene(bool active)
        {
            if (!previousScene)
            {
                previousScene = EditorSceneManager.playModeStartScene;
            }

            if (active)
            {
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/uAdventureAnalytics/Plugins/SurveyManager/_Login.unity");
            }
            else
            {
                EditorSceneManager.playModeStartScene = previousScene;
            }
        }
    }
}