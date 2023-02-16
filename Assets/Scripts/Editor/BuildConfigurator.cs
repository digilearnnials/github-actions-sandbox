using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace Digi.EditorTools
{
    public static class BuildConfigurator
    {
        private const string DevelopmentDefine = "DEVELOPMENT";
        private const string StagingDefine = "STAGING";
        private const string ProductionDefine = "PRODUCTION";
        private const string NonHostedServerTestDefine = "NON_HOSTED_SERVER_TEST";


        private static List<string> GetCurrentDefines ()
        {
            List<string> currentDefines;

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            StandaloneBuildSubtarget standaloneBuildSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget;
            bool isServer = (buildTargetGroup == BuildTargetGroup.Standalone && standaloneBuildSubtarget == StandaloneBuildSubtarget.Server);
                                           
            string defines = (isServer) ? PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Server) :
                                          PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup); 

            currentDefines = defines.Split(';').ToList();

            return currentDefines;
        }

        private static void SetCurrentDefines (List<string> currentDefines)
        {
            string defines = string.Join(";", currentDefines);

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            StandaloneBuildSubtarget standaloneBuildSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget;
            bool isServer = (buildTargetGroup == BuildTargetGroup.Standalone && standaloneBuildSubtarget == StandaloneBuildSubtarget.Server);

            if (isServer)
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, defines);
            else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }

        public static ProductType GetCurrentProductType ()
        {
            ProductType productType;

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            StandaloneBuildSubtarget standaloneBuildSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget;
            bool isServer = (buildTargetGroup == BuildTargetGroup.Standalone && standaloneBuildSubtarget == StandaloneBuildSubtarget.Server);

            productType = (isServer) ? ProductType.Server : ProductType.Client;

            return productType;
        }
        
        public static EnvironmentType GetCurrentEnvironmentType ()
        {
            EnvironmentType environmentType = EnvironmentType.Development;

            List<string> currentDefines = GetCurrentDefines();

            if (currentDefines.Contains(DevelopmentDefine))
                environmentType = EnvironmentType.Development;
            else if (currentDefines.Contains(StagingDefine))
                environmentType = EnvironmentType.Staging;
            else if (currentDefines.Contains(ProductionDefine))
                environmentType = EnvironmentType.Production;

            return environmentType;
        }
        
        public static ServerType GetCurrentServerType ()
        {
            ServerType serverType;

            List<string> currentDefines = GetCurrentDefines();

            serverType = (currentDefines.Contains(NonHostedServerTestDefine)) ? ServerType.Local : ServerType.Hosted;

            return serverType;
        }

        public static void SetProductType (ProductType productType)
        {
            const string buildConfigSettingsPath = "Assets/Data/Build Configuration/Build Configuration Settings.asset";
            BuildConfigurationSettings settings = AssetDatabase.LoadAssetAtPath<BuildConfigurationSettings>(buildConfigSettingsPath);

            if (settings)
            {
                PlayerSettings.productName = $"{settings.BaseProductName}_{productType.ToString()}";
                
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                List<SceneAsset> targetScenes = null;

                switch (productType)
                {
                    case ProductType.Client:
                        
                        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
                        targetScenes = settings.ClientScenes;                   
                        break;

                    case ProductType.Server:
                        
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
                        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
                        targetScenes = settings.ServerScenes;
                        break;
                }

                for (int i = 0; i < scenes.Length; i++)
                    scenes[i].enabled = (targetScenes.Exists(sceneAsset => scenes[i].path.Contains(sceneAsset.name)));

                EditorBuildSettings.scenes = scenes;
            }
            else
            {
                Debug.LogError("No Build Configuration Settings file created!");
            }
        }


        public static void SetEnvironmentType (EnvironmentType environmentType)
        {            
            List<string> currentDefines = GetCurrentDefines();

            switch (environmentType)
            {
                case EnvironmentType.Development:

                    if (!currentDefines.Contains(DevelopmentDefine))
                        currentDefines.Add(DevelopmentDefine);
                    
                    if (currentDefines.Contains(StagingDefine))
                        currentDefines.RemoveAll(define => define == StagingDefine);
                    
                    if (currentDefines.Contains(ProductionDefine))
                        currentDefines.RemoveAll(define => define == ProductionDefine);
                    
                    break;
                
                case EnvironmentType.Staging:

                    if (!currentDefines.Contains(StagingDefine))
                        currentDefines.Add(StagingDefine);
                    
                    if (currentDefines.Contains(DevelopmentDefine))
                        currentDefines.RemoveAll(define => define == DevelopmentDefine);
                    
                    if (currentDefines.Contains(ProductionDefine))
                        currentDefines.RemoveAll(define => define == ProductionDefine);
                    
                    break;
                
                case EnvironmentType.Production:
                    
                    if (!currentDefines.Contains(ProductionDefine))
                        currentDefines.Add(ProductionDefine);
                    
                    if (currentDefines.Contains(DevelopmentDefine))
                        currentDefines.RemoveAll(define => define == DevelopmentDefine);
                    
                    if (currentDefines.Contains(StagingDefine))
                        currentDefines.RemoveAll(define => define == StagingDefine);
                    
                    break;
            }

            SetCurrentDefines(currentDefines);
        }
        
        public static void SetServerType (ServerType serverType)
        {            
            List<string> currentDefines = GetCurrentDefines();

            switch (serverType)
            {
                case ServerType.Local:
                    
                    if (!currentDefines.Contains(NonHostedServerTestDefine))
                        currentDefines.Add(NonHostedServerTestDefine);
                    
                    break;
                
                case ServerType.Hosted:
                    
                    if (currentDefines.Contains(NonHostedServerTestDefine))
                        currentDefines.RemoveAll(define => define == NonHostedServerTestDefine);
                    
                    break;
            }

            SetCurrentDefines(currentDefines);
        }
    }
}