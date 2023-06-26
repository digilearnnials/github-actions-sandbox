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

            if (hasRemoteCatalog)
                LoadStoredCatalogSettings(GetBuildTargetName());
            else
                BuildAddressablesAssetBundles();

            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
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

        private static string GetLibraryCatalogFolderPath (string buildTargetName = null)
        {
            string folderPath;

            buildTargetName ??= GetBuildTargetName(); 

            folderPath = Application.dataPath.Replace("/Assets", string.Empty);
            folderPath += $"/Library/com.unity.addressables/aa/{buildTargetName}";

            return folderPath;
        }

        private static void SaveCatalogSettings ()
        {
            try
            {
                string libraryCatalogFolderPath = GetLibraryCatalogFolderPath();
                string libraryLinkFolderPath = $"{libraryCatalogFolderPath}/{LinkFolder}";
                string storedCatalogFolderPath = GetStoredCatalogFolderPath();
                string storedCatalogForBuildTargetPath = $"{storedCatalogFolderPath}/{GetBuildTargetName()}";

                if (!Directory.Exists(storedCatalogForBuildTargetPath))
                    Directory.CreateDirectory(storedCatalogForBuildTargetPath);

                File.Copy($"{libraryCatalogFolderPath}/{SettingsFileName}", $"{storedCatalogForBuildTargetPath}/{GetStoredSettingsFileName()}", overwrite: true);
                File.Copy($"{libraryCatalogFolderPath}/{CatalogFileName}", $"{storedCatalogForBuildTargetPath}/{GetStoredCatalogFileName()}", overwrite: true);
                File.Copy($"{libraryLinkFolderPath}/{LinkFileName}", $"{storedCatalogFolderPath}/{LinkFileName}", overwrite: true);

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Could not copy the Library settings for Addressables: " + e.Message);
            }
        }

        private static void LoadCatalogSettings (string buildTargetName)
        {
            try
            {
                string libraryCatalogFolderPath = GetLibraryCatalogFolderPath(buildTargetName);
                string libraryLinkFolderPath = $"{libraryCatalogFolderPath}/{LinkFolder}";
                string storedCatalogFolderPath = GetStoredCatalogFolderPath();
                string storedCatalogForBuildTargetPath = $"{storedCatalogFolderPath}/{buildTargetName}";

                if (!Directory.Exists(libraryLinkFolderPath))
                    Directory.CreateDirectory(libraryLinkFolderPath);

                string settingsJson = File.ReadAllText($"{storedCatalogForBuildTargetPath}/{GetStoredSettingsFileName()}");
                string catalogJson = File.ReadAllText($"{storedCatalogForBuildTargetPath}/{GetStoredCatalogFileName()}");
                string linkXml = File.ReadAllText($"{storedCatalogFolderPath}/{LinkFileName}");
                
                File.WriteAllText($"{libraryCatalogFolderPath}/{SettingsFileName}", settingsJson);
                File.WriteAllText($"{libraryCatalogFolderPath}/{CatalogFileName}", catalogJson);
                File.WriteAllText($"{libraryLinkFolderPath}/{LinkFileName}", linkXml);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not copy the stored settings for Addressables: " + e.Message);
            }
        }

        public static async Task BuildAndReleaseAddressablesAssetBundles ()
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

        public static void BuildAddressablesAssetBundles ()
        {
            AddressablesPlayerBuildResult result;

            AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent(out result);
        }
        
        public static void LoadStoredCatalogSettings (string buildTargetName)
        {
            LoadCatalogSettings(buildTargetName);
        }

        public static string GetActiveAddressablesProfileName ()
        {
            string activeProfileName;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetProfileSettings profileSettings = settings.profileSettings;
            
            activeProfileName = profileSettings.GetProfileName(settings.activeProfileId);

            return activeProfileName;
        }

        public static void SetActiveProfile (string environmentName, string buildTargetName = null)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetProfileSettings profileSettings = settings.profileSettings;

            buildTargetName ??= GetBuildTargetName();
            
            settings.activeProfileId = profileSettings.GetProfileId($"{environmentName}_{buildTargetName}");
        }
    }
}