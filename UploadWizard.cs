using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class UploadWizard : ScriptableWizard
    {    
        public DropDownItems GroupId;
        public string Description = "";
        public Object[] BuildScenes = new Object[1];
        public Texture2D Image;

        private const string BuildRoot = "C:\\Build";
        private const string SuperSecretPassphrase = "WombleBomble";

        private LinkedList<string> _scenePaths;
    
        [MenuItem("Assets/Build and Upload")]
        [UsedImplicitly]
        private static void Display()
        {
            DisplayWizard("Build and Upload", typeof(UploadWizard), "Upload");
        }
    
        public void OnEnable()
        {
            if (!EditorPrefs.HasKey("UploadWizard_Description"))
                EditorPrefs.SetString("UploadWizard_Description", "");
            else
                Description = EditorPrefs.GetString("UploadWizard_Description");

            if (!EditorPrefs.HasKey("UploadWizard_GroupID"))
                EditorPrefs.SetInt("UploadWizard_GroupID", (int)DropDownItems.Gruppe1);
            else
                GroupId = (DropDownItems)EditorPrefs.GetInt("UploadWizard_GroupID");

            if (!EditorPrefs.HasKey("UploadWizard_ScenePaths"))
                EditorPrefs.SetString("UploadWizard_ScenePaths", "");
            else
            {
                char[] splitChars = {':'};
                string[] tempScenePaths = EditorPrefs.GetString("UploadWizard_ScenePaths").Split(splitChars);
    
                BuildScenes = new Object[tempScenePaths.Length];
    
                for (int i = 0; i<tempScenePaths.Length; i++)
                    BuildScenes[i] = AssetDatabase.LoadAssetAtPath(tempScenePaths[i], typeof(Object));

            }
        }
    
        public void OnWizardUpdate()
        {
            helpString = "";

            if (BuildScenes.Length == 0)
                BuildScenes = new Object[1];
        
            for(int i = 0 ; i < BuildScenes.Length ; i++)
            {
                string assetPath = AssetDatabase.GetAssetPath(BuildScenes[i]);

                if (assetPath != "" && !assetPath.EndsWith(".unity"))
                {
                    BuildScenes[i] = null;
                    helpString = "The asset you tried to add was not a scene!!!";
                }
            }
        
            _scenePaths = new LinkedList<string>();

            foreach(Object obj in BuildScenes)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);

                if (assetPath.EndsWith(".unity"))
                    _scenePaths.AddLast(assetPath);
            }

            isValid = _scenePaths.Count != 0;
        
            if (Description.Length < 10)
                helpString = "Description must be at least 10 characters long.";
        }

        public void OnWizardCreate()
        {
            if(_scenePaths.Count != 0)
                BuildAndUpload();
        }

        private void BuildAndUpload()
        {
            string[] sceneNames = new string[_scenePaths.Count];
            _scenePaths.CopyTo(sceneNames, 0);

            EditorPrefs.SetString("UploadWizard_Description", Description);
            EditorPrefs.SetInt("UploadWizard_GroupID", (int)GroupId);
            EditorPrefs.SetString("UploadWizard_ScenePaths", string.Join(":", sceneNames));

            BuildPipeline.BuildPlayer(sceneNames, BuildRoot, BuildTarget.WebGL, BuildOptions.None);

            byte[] dataFile = null, jsFile = null, memFile = null, loaderFile = null;

            try {
                dataFile = File.ReadAllBytes(BuildRoot + "\\Release\\Build.datagz");
                jsFile = File.ReadAllBytes(BuildRoot + "\\Release\\Build.jsgz");
                memFile = File.ReadAllBytes(BuildRoot + "\\Release\\Build.memgz");
                loaderFile = File.ReadAllBytes(BuildRoot + "\\Release\\UnityLoader.js");
            } catch (IOException e) {
                Debug.Log("Could not open build files.");
                Debug.Log(e.Message);
            }
        
        
            WWWForm form = new WWWForm();
            form.AddField("passphrase", SuperSecretPassphrase);
            form.AddField("brugernavn", GroupIdName(GroupId)); 
            form.AddField("desc", Description);
            form.AddBinaryData("dataFile", dataFile);
            form.AddBinaryData("jsFile", jsFile);
            form.AddBinaryData("memFile", memFile);
            form.AddBinaryData("loaderFile", loaderFile);

            if (Image != null)
            {
                string texturePath = AssetDatabase.GetAssetPath(Image);
                TextureImporter textureImporter = (TextureImporter) AssetImporter.GetAtPath(texturePath);
            
                textureImporter.isReadable = true;
                textureImporter.textureFormat = TextureImporterFormat.ARGB32;
            
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            
                byte[] bytes = Image.EncodeToPNG();
                form.AddBinaryData("image", bytes);
            }

            WWW webRequest = new WWW("https://game.unf.dk/games/uploadGame.php", form);

            while (!webRequest.isDone)
            {

            }

            Debug.Log("Data uploaded.");

            Debug.Log(webRequest.error ?? webRequest.text);
        
            Debug.Log("Done.");
        }
    
        private static string GroupIdName(DropDownItems id)
        {
            return id.ToString().ToLower();
        }
    }

    public enum DropDownItems
    {
        Gruppe1,
        Gruppe2,
        Gruppe3,
        Gruppe4,
        Gruppe5,
        Gruppe6,
        Gruppe7,
        Gruppe8,
        Gruppe9,
        Gruppe10,
        Gruppe11,
        Gruppe12
    }
}