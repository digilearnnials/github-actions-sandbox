using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Digi.EditorTools
{
    [CreateAssetMenu(fileName = "Build Configuration Settings", menuName = "Digi Editor Tools/Build Configuration Settings")]
    public class BuildConfigurationSettings : ScriptableObject
    {
        [SerializeField] private string baseProductName; 
        [SerializeField] private List<SceneAsset> clientScenes; 
        [SerializeField] private List<SceneAsset> serverScenes;


        public string BaseProductName => baseProductName; 
        public List<SceneAsset> ClientScenes => clientScenes; 
        public List<SceneAsset> ServerScenes => serverScenes;
    }
}