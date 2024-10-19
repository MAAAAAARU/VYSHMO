using UnityEngine;
namespace TarodevController
{
public class PrefabInteraction : MonoBehaviour
{
    private SpecialPlayerController _playerController;
     private PrefabPlacer _prefabPlacer;

     private void Start()
    {
        // 获取 PrefabPlacer 的引用
        _prefabPlacer = FindObjectOfType<PrefabPlacer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("11");
            _playerController = collision.GetComponent<SpecialPlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("22");
            _playerController = null;
        }
    }

    private void Update()
    {
        if (_playerController != null && Input.GetKeyDown(KeyCode.F))
        {
            _playerController.ExitSpecialMode();

            // 销毁 prefab（自身）
           // Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_prefabPlacer != null)
        {
            _prefabPlacer.OnPrefabDestroyed();
        }
    }
}
}