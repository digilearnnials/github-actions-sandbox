using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Digi.Addressables
{
    public class AddressableLoadTest : MonoBehaviour
    {
        [SerializeField] private AssetReference cubeReference;


        async void Start ()
        {
            GameObject cubePrefab = await AddressableLoadUtility.Load<GameObject>(cubeReference);
            Instantiate(cubePrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity);
        }
    }
}