using UnityEngine;
using Cinemachine;

public class CameraRoomSwitcher : MonoBehaviour
{
    public Transform roomAnchor;  // 当前房间的锚点
    private CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        // 获取场景中的 Cinemachine 虚拟摄像机
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 将摄像机的 Follow 对象设置为房间的锚点
            virtualCamera.Follow = roomAnchor;
            virtualCamera.LookAt = roomAnchor;
        }
    }
}
