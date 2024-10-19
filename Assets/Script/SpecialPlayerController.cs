using UnityEngine;
using System.Collections.Generic;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpecialPlayerController : MonoBehaviour
    {
        // 公共变量
        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private GameObject ropeSegmentPrefab; // 在 Inspector 中指定 RopeSegment 预制件
        [SerializeField] private float extraRopeLength = 8.0f; // 额外的绳子长度
        [SerializeField] private int segmentCount = 40; // 绳子的段数，可根据需要调整

        // 私有变量
        private Rigidbody2D _rb;
        private GameObject _prefabInstance;
        private Vector2 _prefabPosition;
        private List<GameObject> ropeSegments = new List<GameObject>();
        private LineRenderer _lineRenderer;
        private Vector2 _input;
        private Vector2 _frameVelocity;
        private float _allowedRadius = 5f; // 玩家可移动的最大半径
        private float segmentLength;

        private SpriteRenderer _spriteRenderer; // 添加对 SpriteRenderer 的引用

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // 获取 SpriteRenderer

            enabled = false; // 默认禁用脚本
        }

        public void EnterSpecialMode(GameObject prefabInstance)
{
    _prefabInstance = prefabInstance;
    enabled = true; // 启用特殊模式脚本

    // 禁用普通模式的控制器
    var playerController = GetComponent<PlayerController>();
    if (playerController != null)
    {
        playerController.enabled = false;
    }

    // 禁用 PlayerAnimator 脚本
    var animator = GetComponent<PlayerAnimator>();
    if (animator != null)
    {
        animator.enabled = false;
    }

    CreateRope();
}


        public void ExitSpecialMode()
{
    enabled = false;
    _rb.velocity = Vector2.zero;
    DestroyRope();

    // 销毁 prefab 实例
    if (_prefabInstance != null)
    {
        Destroy(_prefabInstance);
        _prefabInstance = null;
    }

    // 启用 PlayerAnimator 脚本
    var animator = GetComponent<PlayerAnimator>();
    if (animator != null)
    {
        animator.enabled = true;
    }

    //DestroyRope();

    // 启用普通模式的控制器
    GetComponent<PlayerController>().enabled = true;
}


        private void Update()
        {
            GatherInput();
            HandleSpriteFlip(); // 调用精灵翻转逻辑
            UpdateRopeLine();

            // if (Input.GetKeyDown(KeyCode.F))
            // {
            //     ExitSpecialMode();
            //     GetComponent<PlayerController>().enabled = true;
            // }
        }

        private void FixedUpdate()
        {
            if (_prefabInstance == null)
            {
                ExitSpecialMode();
                GetComponent<PlayerController>().enabled = true;
                return;
            }
            _prefabPosition = _prefabInstance.transform.position;
            HandleMovement();
        }

        private void GatherInput()
        {
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private void HandleSpriteFlip()
        {
            if (_input.x != 0)
            {
                _spriteRenderer.flipX = _input.x < 0;
            }
        }

        private void HandleMovement()
        {
            Vector2 desiredVelocity = _input * _stats.MaxSpeed;
            Vector2 predictedPosition = _rb.position + desiredVelocity * Time.fixedDeltaTime;
            Vector2 offset = predictedPosition - _prefabPosition;

            if (offset.magnitude > _allowedRadius)
            {
                offset = offset.normalized * _allowedRadius;
                predictedPosition = _prefabPosition + offset;
                desiredVelocity = (predictedPosition - _rb.position) / Time.fixedDeltaTime;
            }

            _frameVelocity = desiredVelocity;
            _rb.velocity = _frameVelocity;
        }

        private void CreateRope()
        {
            Vector2 startPoint = transform.position;
            Vector2 endPoint = _prefabInstance.transform.position;

            float ropeLength = Vector2.Distance(startPoint, endPoint) + extraRopeLength;
            segmentLength = ropeLength / segmentCount;

            Rigidbody2D previousRigidBody = _rb;

            for (int i = 0; i < segmentCount; i++)
            {
                float t = (i + 1) / (float)(segmentCount + 1);

                Vector2 segmentPosition = Vector2.Lerp(startPoint, endPoint, t);

                GameObject segment = Instantiate(ropeSegmentPrefab, segmentPosition, Quaternion.identity);
                Rigidbody2D segmentRb = segment.GetComponent<Rigidbody2D>();

                // 设置绳子段的物理属性
                segmentRb.mass = 0.1f;
                segmentRb.drag = 5f;
                segmentRb.angularDrag = 5f;

                // 添加 DistanceJoint2D
                DistanceJoint2D segmentJoint = segment.AddComponent<DistanceJoint2D>();
                segmentJoint.connectedBody = previousRigidBody;
                segmentJoint.autoConfigureDistance = false;
                segmentJoint.distance = segmentLength;
                segmentJoint.maxDistanceOnly = false;

                ropeSegments.Add(segment);

                previousRigidBody = segmentRb;
            }

            // 连接最后一个绳子段和 prefab
            DistanceJoint2D prefabJoint = _prefabInstance.AddComponent<DistanceJoint2D>();
            prefabJoint.connectedBody = previousRigidBody;
            prefabJoint.autoConfigureDistance = false;
            prefabJoint.distance = segmentLength;
            prefabJoint.maxDistanceOnly = false;

            // 创建 LineRenderer 绘制绳子
            _lineRenderer = new GameObject("RopeLine").AddComponent<LineRenderer>();
            _lineRenderer.positionCount = segmentCount + 2;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = Color.black;
            _lineRenderer.endColor = Color.black;
        }

        private void UpdateRopeLine()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, transform.position);

                for (int i = 0; i < ropeSegments.Count; i++)
                {
                    _lineRenderer.SetPosition(i + 1, ropeSegments[i].transform.position);
                }

                _lineRenderer.SetPosition(ropeSegments.Count + 1, _prefabInstance.transform.position);
            }
        }

        private void DestroyRope()
        {
            foreach (GameObject segment in ropeSegments)
            {
                Destroy(segment);
            }
            ropeSegments.Clear();

            // 移除 prefab 上的 DistanceJoint2D
            DistanceJoint2D prefabJoint = _prefabInstance.GetComponent<DistanceJoint2D>();
            if (prefabJoint != null)
            {
                Destroy(prefabJoint);
            }

            if (_lineRenderer != null)
            {
                Destroy(_lineRenderer.gameObject);
            }
        }
    }
}
