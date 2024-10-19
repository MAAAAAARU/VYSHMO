using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [SerializeField] private GameObject prefabPrefab; // 在Inspector中指定您的prefab

    public delegate void PrefabPlacedHandler(GameObject prefabInstance);
    public event PrefabPlacedHandler OnPrefabPlaced;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlacePrefab();
        }
    }

    private void PlacePrefab()
    {
        Vector3 prefabPosition = transform.position + Vector3.right * transform.localScale.x * 1.0f; // 前方1米处
        GameObject instantiatedPrefab = Instantiate(prefabPrefab, prefabPosition, Quaternion.identity);

        // 触发事件，通知其他脚本prefab已被放置
        OnPrefabPlaced?.Invoke(instantiatedPrefab);
    }
}
