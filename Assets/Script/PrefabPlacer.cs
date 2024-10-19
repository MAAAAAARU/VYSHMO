using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [SerializeField] private GameObject prefabPrefab;

    public delegate void PrefabPlacedHandler(GameObject prefabInstance);
    public event PrefabPlacedHandler OnPrefabPlaced;

    private SpriteRenderer _spriteRenderer;
    private GameObject _instantiatedPrefab; // 存储已实例化的预制件

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlacePrefab();
        }
    }

    private void PlacePrefab()
    {
        // 检查是否已经存在一个预制件
        if (_instantiatedPrefab != null)
        {
            // 如果已经存在，可以选择不做任何操作，或者销毁旧的预制件
            // 这里选择不做任何操作
            Debug.Log("已经存在一个预制件，无法放置新的预制件。");
            return;

            // 如果想替换旧的预制件，可以取消注释以下代码
            // Destroy(_instantiatedPrefab);
        }

        // 确定玩家的朝向
        Vector3 facingDirection = _spriteRenderer.flipX ? Vector3.left : Vector3.right;

        // 计算预制件的位置，在玩家前方 1 米处
        Vector3 prefabPosition = transform.position + facingDirection * 1.0f;

        // 实例化预制件，并存储引用
        _instantiatedPrefab = Instantiate(prefabPrefab, prefabPosition, Quaternion.identity);

        // 触发事件，通知其他脚本预制件已被放置
        OnPrefabPlaced?.Invoke(_instantiatedPrefab);
    }

    // 当预制件被销毁时，清空引用
    public void OnPrefabDestroyed()
    {
        _instantiatedPrefab = null;
    }
}
