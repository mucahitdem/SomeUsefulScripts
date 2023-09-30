using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Scripts.BaseGameScripts.DevHelperTools.Editor
{
    [Overlay(typeof(SceneView), IdSceneViewerOverlay, "Scene Loader")]
    [Icon("Assets/Sprites/Icons/unity_scene.png")]
    internal class SceneLoaderTool : Overlay
    {
        private const string IdSceneViewerOverlay = "sceneViewerOverlay";
        private static bool s_additive;
        private static readonly Button s_isAdditive = new Button(LoadTypeButton);
        
        // private static readonly Button s_mainLevelButton = new Button(LoadMainLevel);
        // private static readonly Button s_level1Button = new Button(LoadLevel1);
        // private static readonly List<int> s_indicesMainLevel = new List<int>();
        // private static readonly List<int> s_indicesLevel1 = new List<int>();
        private VisualElement _root;

        public override VisualElement CreatePanelContent()
        {
            _root = new VisualElement
            {
                style =
                {
                    width = new StyleLength(new Length(120, LengthUnit.Pixel)),
                    backgroundColor = new StyleColor(Color.black),
                    opacity = new StyleFloat(0.85f),
                    fontSize = 14
                }
            };

            CreateSceneButtons();

            return _root;
        }

        public override void OnCreated()
        {
            EditorBuildSettings.sceneListChanged += CreateSceneButtons;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            EditorBuildSettings.sceneListChanged -= CreateSceneButtons;
        }


        private void CreateSceneButtons()
        {
            _root.Clear();

            AddLoadTypeButton();

            if (CheckIfBuildSettingsIfWeHaveAnyScenes())
                return;

            TryGetScenes();
        }
        private void AddLoadTypeButton()
        {
            _root.Add(s_isAdditive);
            s_additive = PlayerPrefs.GetInt(Defs.SAVE_KEY_SCENE_LOADER_TOOL, 0) == 1;
            UpdateLoadTypeButtonText();
        }
        private void TryGetScenes()
        {
            var sceneCount = SceneManager.sceneCountInBuildSettings;

            for (var i = 0; i < sceneCount; i++)
            {
                var fileName = Path.GetFileName(SceneUtility.GetScenePathByBuildIndex(i));
                var sceneIndex = i;
                var sceneButton = new Button(() => LoadSceneWithIndex(sceneIndex));
                var buttonText = fileName.Substring(0, fileName.Length - 6); //Removes the extension part of the file name (e.g: "MainScene.unity" -> "MainScene")
                sceneButton.text = buttonText;
                
                _root.Add(sceneButton);
            }
        }
        private bool CheckIfBuildSettingsIfWeHaveAnyScenes()
        {
            if (SceneManager.sceneCountInBuildSettings == 0)
            {
                var warningText = new TextElement();
                warningText.text = "No Scenes in Build Settings";
                warningText.style.fontSize = 12;
                warningText.style.color = new StyleColor(Color.red);

                _root.Add(warningText);
                return true;
            }

            return false;
        }
        private void LoadSceneWithIndex(int index)
        {
            if (SceneManager.GetActiveScene().isDirty)
            {
                var dialogResult = EditorUtility.DisplayDialogComplex(
                    "Scene has been modified",
                    "Do you want to save the changes you made in the current scene?",
                    "Save", "Don't Save", "Cancel");

                switch (dialogResult)
                {
                    case 0: //Save and open the new scene
                        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(index));
                        break;
                    case 1: //Open the new scene without saving current.
                        EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(index));
                        break;
                    case 2: //Cancel process (Basically do nothing for now.)
                        break;
                    default:
                        Debug.LogWarning("Something went wrong when switching scenes.");
                        break;
                }
            }
            else
            {
                EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(index), s_additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
            }
        }
     
        private static void LoadTypeButton()
        {
            s_additive = !s_additive;
            PlayerPrefs.SetInt(Defs.SAVE_KEY_SCENE_LOADER_TOOL, s_additive ? 1 : 0);
            UpdateLoadTypeButtonText();
        }
        private static void UpdateLoadTypeButtonText()
        {
            if (s_additive)
                s_isAdditive.text = "Additive";
            else
                s_isAdditive.text = "Single";
        }
    }
}