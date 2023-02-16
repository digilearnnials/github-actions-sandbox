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
    }
}