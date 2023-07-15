using Scripts.BaseGameScripts;
using Scripts.BaseGameScripts.Helper;
using UnityEditor;
using UnityEngine;

namespace Scripts.GameScripts.DevHelperTools.SoCreator
{
    public class SoCreatorEditorWindow : EditorWindow
    {
        private string scriptableObjectName = "NewScriptableObject";
        private string savePath = "Assets/";
        private System.Type[] scriptableObjectTypes;
        private int selectedTypeIndex = 0;

        [MenuItem("Tools/Sefir-i Mevt Games/Dev Tools/So Creator")]
        public static void ShowWindow()
        {
            GetWindow<SoCreatorEditorWindow>("So Creator");
        }

        private void OnEnable()
        {
            // Get the available Scriptable Object types
            scriptableObjectTypes = GetScriptableObjectTypes();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scriptable Object Creator", EditorStyles.boldLabel);

            // Display the dropdown menu for selecting the Scriptable Object type
            selectedTypeIndex = EditorGUILayout.Popup("Object Type", selectedTypeIndex, GetScriptableObjectNames());

            scriptableObjectName = EditorGUILayout.TextField("Object Name", scriptableObjectName);

            EditorGUI.BeginChangeCheck();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (EditorGUI.EndChangeCheck())
            {
                if (!savePath.EndsWith("/"))
                    savePath += "/";
            }

            if (GUILayout.Button("Create"))
            {
                CreateScriptableObject();
            }
        }

        private System.Type[] GetScriptableObjectTypes()
        {
            var subClasses = AssemblyManager.GetSubClassesOfType(typeof(BaseScriptableObject));
            // Customize this method to retrieve the available Scriptable Object types dynamically
            var types = new System.Type[subClasses.Count];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = subClasses[i];
            }

            return types;
        }
        private string[] GetScriptableObjectNames()
        {
            string[] typeNames = new string[scriptableObjectTypes.Length];
            for (int i = 0; i < scriptableObjectTypes.Length; i++)
            {
                typeNames[i] = scriptableObjectTypes[i].Name;
            }

            return typeNames;
        }
        private void CreateScriptableObject()
        {
            System.Type selectedType = scriptableObjectTypes[selectedTypeIndex];

            string defaultName = scriptableObjectName;
            string path = EditorUtility.SaveFilePanel(
                "Save Scriptable Object",
                savePath,
                defaultName + ".asset",
                "asset"
            );

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Scriptable Object creation canceled.");
                return;
            }

            ScriptableObject newInstance = CreateInstance(selectedType);
            if (newInstance == null)
            {
                Debug.LogError("Failed to create Scriptable Object. Type does not inherit from BaseScriptableObject: " + selectedType.FullName);
                return;
            }

            // Save the new instance as an asset at the selected path
            string assetPath = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.CreateAsset(newInstance, assetPath);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log("Scriptable Object created at: " + assetPath);
        }
    }
}