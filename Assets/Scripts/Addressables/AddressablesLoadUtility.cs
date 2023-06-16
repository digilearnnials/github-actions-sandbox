using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Digi.Addressables
{
    public static class AddressableLoadUtility
    {
        public static async Task<T> Load<T> (AssetReference assetReference) where T : class
        {
            T asset;

            if (!assetReference.IsValid())
                assetReference.LoadAssetAsync<T>();

            await assetReference.OperationHandle.Task;

            asset = assetReference.Asset as T;

            return asset;
        }

        public static void Release (AssetReference assetReference)
        {
            if (assetReference.IsValid() && assetReference.IsDone)
                assetReference.ReleaseAsset();
        }
    }
}