using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;

namespace Digi.EditorTools
{
    public static class AddressablesBuildUtility
    {
        [InitializeOnLoadMethod]
        private static void RegisterBuildPlayerHandler ()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuild);
        }

        private static void OnBuild (BuildPlayerOptions buildPlayerOptions)
        {
            bool hasRemoteCatalog = (BuildConfigurator.GetCurrentProductType() == ProductType.Client);

            if (!hasRemoteCatalog)
                LoadCatalogSettings();
            else
                BuildAdressablesAssetBundles();

            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
        }

        private static string GetActiveAddressablesProfileName ()
        {
            string activeProfileName;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetProfileSettings profileSettings = settings.profileSettings;
            
            activeProfileName = profileSettings.GetProfileName(settings.activeProfileId);

            return activeProfileName;
        }

        private static string GetStoredCatalogFolderPath ()
        {
            return $"{Application.dataPath}/Data/CCD";
        }

        private static string GetStoredCatalogSettingsFileName ()
        {
            string activeProfileName = GetActiveAddressablesProfileName();
            return $"{activeProfileName}_settings.json";
        }

        private static string GetLibraryCatalogSettingsFolderPath ()
        {
            string folderPath;

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            string buildTargetName;

            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    buildTargetName = "Windows";
                    break;

                case BuildTarget.StandaloneLinux64:
                    buildTargetName = "Linux";
                    break;

                default:
                    buildTargetName = buildTarget.ToString();
                    break;
            }

            folderPath = Application.dataPath.Replace("/Assets", string.Empty);
            folderPath += $"/Library/com.unity.addressables/aa/{buildTargetName}";

            return folderPath;
        }

        private static void SaveCatalogSettings ()
        {
            try
            {
                string libraryCatalogSettingsFolderPath = GetLibraryCatalogSettingsFolderPath();
                string libraryAddressablesLinkFolderPath = $"{libraryCatalogSettingsFolderPath}/AddressablesLink";
                string storedCatalogSettingsFolderPath = GetStoredCatalogFolderPath();

                if (!Directory.Exists(storedCatalogSettingsFolderPath))
                    Directory.CreateDirectory(storedCatalogSettingsFolderPath);

                File.Copy($"{libraryCatalogSettingsFolderPath}/settings.json", $"{storedCatalogSettingsFolderPath}/{GetStoredCatalogSettingsFileName()}", overwrite: true);
                File.Copy($"{libraryAddressablesLinkFolderPath}/link.xml", $"{storedCatalogSettingsFolderPath}/link.xml", overwrite: true);

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Could not copy the Library settings for Addressables: " + e.Message);
            }
        }

        private static void LoadCatalogSettings ()
        {
            try
            {
                string libraryCatalogSettingsFolderPath = GetLibraryCatalogSettingsFolderPath();
                string libraryAddressablesLinkFolderPath = $"{libraryCatalogSettingsFolderPath}/AddressablesLink";
                string storedCatalogSettingsFolderPath = GetStoredCatalogFolderPath();

                if (!Directory.Exists(libraryAddressablesLinkFolderPath))
                    Directory.CreateDirectory(libraryAddressablesLinkFolderPath);

                string settingsJson = File.ReadAllText($"{storedCatalogSettingsFolderPath}/{GetStoredCatalogSettingsFileName()}");
                string linkXml = File.ReadAllText($"{storedCatalogSettingsFolderPath}/link.xml");
                
                File.WriteAllText($"{libraryCatalogSettingsFolderPath}/settings.json", settingsJson);
                File.WriteAllText($"{libraryAddressablesLinkFolderPath}/link.xml", linkXml);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not copy the stored settings for Addressables: " + e.Message);
            }
        }

        private static async Task BuildAndReleaseAdressablesAssetBundles ()
        {
            Debug.LogWarning("UPLOADING BUNDLES TO CCD. DO NOT CLOSE THE EDITOR!");

            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);

            AddressableAssetBuildResult result = await AddressableAssetSettings.BuildAndReleasePlayerContent();

            SaveCatalogSettings();
            
            Debug.Log("BUNDLES FINISHED UPLOADING TO CCD!");
        }
        
        private static void BuildAdressablesAssetBundles ()
        {
            AddressablesPlayerBuildResult result;

            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent(out result);
        }

        [MenuItem("Addressables' Utilities/Build and deploy bundles for current environment")]
        private static void BuildAndDeployBundlesForCurrentEnvironment ()
        {
            string activeProfileName = GetActiveAddressablesProfileName();
            string promptTitle = "Addressables' Deployment to CCD";
            string promptMessage = $"You are about to update the contents of the {activeProfileName} bucket. Are you absolutely sure?";
            string promptOk = "Yes"; 
            string promptCancel = "No"; 

            if (EditorUtility.DisplayDialog(promptTitle, promptMessage, promptOk, promptCancel))
                _ = BuildAndReleaseAdressablesAssetBundles();
        }

        [MenuItem("Addressables' Utilities/Load stored catalog settings")]
        public static void LoadStoredCatalogSettings ()
        {
            string libraryCatalogSettingsFolderPath = GetLibraryCatalogSettingsFolderPath();

            if (Directory.Exists(libraryCatalogSettingsFolderPath))
                Directory.CreateDirectory(libraryCatalogSettingsFolderPath);

            LoadCatalogSettings();
        }
    }
}