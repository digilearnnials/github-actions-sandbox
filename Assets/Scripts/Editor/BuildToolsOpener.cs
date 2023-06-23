using UnityEditor;

namespace Digi.EditorTools
{
    public static class BuildToolsOpener
    {
        private const string MenuName = "Build Tools";


        [MenuItem(MenuName + "/Configure Build...")]
        private static void ConfigureBuild ()
        {
            string windowTitle = "Build Configurator";

            BuildConfiguratorWindow window = EditorWindow.GetWindow(typeof(BuildConfiguratorWindow), false, windowTitle, true) as BuildConfiguratorWindow;
            window.Show();
        }
        
        [MenuItem(MenuName + "/Build and Deploy Bundles")]
        private static void BuildAndDeployBundles ()
        {
            string activeProfileName = AddressablesBuildUtility.GetActiveAddressablesProfileName();
            string promptTitle = "Addressables' Deployment to CCD";
            string promptMessage = $"You are about to update the contents of the {activeProfileName} bucket. Are you absolutely sure?";
            string promptOk = "Yes"; 
            string promptCancel = "No"; 

            if (EditorUtility.DisplayDialog(promptTitle, promptMessage, promptOk, promptCancel))
               _ = AddressablesBuildUtility.BuildAndReleaseAddressablesAssetBundles();
        }
    }
}