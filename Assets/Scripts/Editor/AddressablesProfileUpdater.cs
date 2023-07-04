using UnityEditor;
using UnityEditor.Build;

namespace Digi.EditorTools
{
    public class AddressablesProfileUpdater : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;
        
        
        [InitializeOnLoadMethod]
        private static void UpdateActiveAddressablesProfile ()
        {
            string environmentName = BuildConfigurator.GetCurrentEnvironmentType().ToString();
            AddressablesBuildUtility.SetActiveProfile(environmentName);
        }
        
        public void OnActiveBuildTargetChanged (BuildTarget previousTarget, BuildTarget newTarget)
        {
            UpdateActiveAddressablesProfile();
        }
    }
}