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
        private const string SettingsFileName = "settings.json";
        private const string CatalogFileName = "catalog.json";
        private const string LinkFileName = "link.xml";
        private const string LinkFolder = "AddressablesLink";


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
                BuildAddressablesAssetBundles();

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

        private static string GetBuildTargetName ()
        {
            string buildTargetName;

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

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

            return buildTargetName;
        }

        private static string GetStoredCatalogFolderPath ()
        {
            return $"{Application.dataPath}/Data/CCD";
        }

        private static string GetStoredSettingsFileName ()
        {
            string activeProfileName = GetActiveAddressablesProfileName();
            return $"{activeProfileName}_{SettingsFileName}";
        }
        
        private static string GetStoredCatalogFileName ()
        {
            string activeProfileName = GetActiveAddressablesProfileName();
            return $"{activeProfileName}_{CatalogFileName}";
        }

        private static string GetLibraryCatalogSettingsFolderPath ()
        {
            string folderPath;

            folderPath = Application.dataPath.Replace("/Assets", string.Empty);
            folderPath += $"/Library/com.unity.addressables/aa/{GetBuildTargetName()}";

            return folderPath;
        }

        private static void SaveCatalogSettings ()
        {
            try
            {
                string libraryCatalogSettingsFolderPath = GetLibraryCatalogSettingsFolderPath();
                string libraryAddressablesLinkFolderPath = $"{libraryCatalogSettingsFolderPath}/{LinkFolder}";
                string storedCatalogSettingsFolderPath = GetStoredCatalogFolderPath();

                if (!Directory.Exists(storedCatalogSettingsFolderPath))
                    Directory.CreateDirectory(storedCatalogSettingsFolderPath);

                File.Copy($"{libraryCatalogSettingsFolderPath}/{SettingsFileName}", $"{storedCatalogSettingsFolderPath}/{GetStoredSettingsFileName()}", overwrite: true);
                File.Copy($"{libraryCatalogSettingsFolderPath}/{CatalogFileName}", $"{storedCatalogSettingsFolderPath}/{GetStoredCatalogFileName()}", overwrite: true);
                File.Copy($"{libraryAddressablesLinkFolderPath}/{LinkFileName}", $"{storedCatalogSettingsFolderPath}/{LinkFileName}", overwrite: true);

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
                string libraryAddressablesLinkFolderPath = $"{libraryCatalogSettingsFolderPath}/{LinkFolder}";
                string storedCatalogSettingsFolderPath = GetStoredCatalogFolderPath();

                if (!Directory.Exists(libraryAddressablesLinkFolderPath))
                    Directory.CreateDirectory(libraryAddressablesLinkFolderPath);

                string settingsJson = File.ReadAllText($"{storedCatalogSettingsFolderPath}/{GetStoredSettingsFileName()}");
                string catalogJson = File.ReadAllText($"{storedCatalogSettingsFolderPath}/{GetStoredCatalogFileName()}");
                string linkXml = File.ReadAllText($"{storedCatalogSettingsFolderPath}/{LinkFileName}");
                
                File.WriteAllText($"{libraryCatalogSettingsFolderPath}/{SettingsFileName}", settingsJson);
                File.WriteAllText($"{libraryCatalogSettingsFolderPath}/{CatalogFileName}", catalogJson);
                File.WriteAllText($"{libraryAddressablesLinkFolderPath}/{LinkFileName}", linkXml);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not copy the stored settings for Addressables: " + e.Message);
            }
        }

        private static async Task BuildAndReleaseAddressablesAssetBundles ()
        {
            Debug.LogWarning("UPLOADING BUNDLES TO CCD. DO NOT CLOSE THE EDITOR!");

            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);

            AddressableAssetBuildResult result = await AddressableAssetSettings.BuildAndReleasePlayerContent();

            string contentStateFolderPath = $"{Application.dataPath}/AddressableAssetsData/{GetBuildTargetName()}";

            if (Directory.Exists(contentStateFolderPath))
            {
                Directory.Delete(contentStateFolderPath, recursive: true);
                
                if (File.Exists($"{contentStateFolderPath}.meta"))
                    File.Delete($"{contentStateFolderPath}.meta");
            }

            if (string.IsNullOrEmpty(result.Error))
            {
                SaveCatalogSettings();
                Debug.Log("BUNDLES FINISHED UPLOADING TO CCD!");
            }
            else
            {
                Debug.LogWarning("BUNDLES WERE NOT UPLOADED CORRECTLY: " + result.Error);
            }
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
                _ = BuildAndReleaseAddressablesAssetBundles();
        }

        public static void BuildAddressablesAssetBundles ()
        {
            AddressablesPlayerBuildResult result;

            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent(out result);
        }
        
        public static void LoadStoredCatalogSettings ()
        {
            string libraryCatalogSettingsFolderPath = GetLibraryCatalogSettingsFolderPath();

            if (Directory.Exists(libraryCatalogSettingsFolderPath))
                Directory.CreateDirectory(libraryCatalogSettingsFolderPath);

            LoadCatalogSettings();
        }
    }
}