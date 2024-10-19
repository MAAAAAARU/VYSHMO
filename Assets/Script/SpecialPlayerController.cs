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
        [SerializeField] private int segmentCount = 10; // 绳子的段数，可根据需要调整

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

        // 存储初始距离的列表
        //private List<float> initialSegmentDistances = new List<float>();

        // 计时器变量
        //private float _overstretchDetectionDelay = 2.0f; // 延迟时间，单位：秒
       // private float _timeSinceRopeCreated = 0f; // 记录绳子生成后的时间

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            enabled = false; // 默认禁用脚本
        }

        public void EnterSpecialMode(GameObject prefabInstance)
        {
            _prefabInstance = prefabInstance;
            enabled = true; // 启用脚本
            CreateRope();
        }

        public void ExitSpecialMode()
        {
            enabled = false;
            _rb.velocity = Vector2.zero;
            _prefabInstance = null;
            DestroyRope();

            // 清空初始距离列表
           // initialSegmentDistances.Clear();
        }

        private void Update()
        {
            GatherInput();
            UpdateRopeLine();

            if (Input.GetKeyDown(KeyCode.F))
            {
                ExitSpecialMode();
                GetComponent<PlayerController>().enabled = true;
            }
        }

        private void FixedUpdate()
        {
            if (_prefabInstance == null)
            {
                ExitSpecialMode();
                GetComponent<PlayerController>().enabled = true;
                return;
            }

            // 更新计时器
           // _timeSinceRopeCreated += Time.fixedDeltaTime;

            _prefabPosition = _prefabInstance.transform.position;

            // 检查绳子是否被过度拉伸
            //if (!IsRopeOverstretched())
           // {
                HandleMovement();
           // }
            // else
            // {
            //     Debug.Log(11);
            //     // 阻止玩家移动
            //     _rb.velocity = Vector2.zero;
            // }
        }

        private void GatherInput()
        {
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
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
            // 重置计时器
    //_timeSinceRopeCreated = 0f;

    Vector2 startPoint = transform.position;
    Vector2 endPoint = _prefabInstance.transform.position;

    float ropeLength = Vector2.Distance(startPoint, endPoint) + extraRopeLength;
    segmentLength = ropeLength / segmentCount;

    Rigidbody2D previousRigidBody = _rb;

    // 清空初始距离列表
   // initialSegmentDistances.Clear();

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

        // 使用 segmentLength 作为初始距离
       // initialSegmentDistances.Add(segmentLength);

        previousRigidBody = segmentRb;
    }

    // 连接最后一个绳子段和 prefab
    DistanceJoint2D prefabJoint = _prefabInstance.AddComponent<DistanceJoint2D>();
    prefabJoint.connectedBody = previousRigidBody;
    prefabJoint.autoConfigureDistance = false;
    prefabJoint.distance = segmentLength;
    prefabJoint.maxDistanceOnly = false;

    // 添加最后一个初始距离
    //initialSegmentDistances.Add(segmentLength);

            // 创建 LineRenderer 绘制绳子
            _lineRenderer = new GameObject("RopeLine").AddComponent<LineRenderer>();
            _lineRenderer.positionCount = segmentCount + 2;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = Color.black;
            _lineRenderer.endColor = Color.black;
        }

        // private bool IsRopeOverstretched()
        // {
        //     // 如果绳子生成后经过的时间小于延迟时间，不进行检测
        //     if (_timeSinceRopeCreated < _overstretchDetectionDelay)
        //     {
        //         return false;
        //     }

        //     Rigidbody2D previousRigidBody = _rb;

        //     for (int i = 0; i < ropeSegments.Count; i++)
        //     {
        //         Rigidbody2D currentRigidBody = ropeSegments[i].GetComponent<Rigidbody2D>();

        //         float currentDistance = Vector2.Distance(previousRigidBody.position, currentRigidBody.position);
        //         float initialDistance = initialSegmentDistances[i];

        //         if (currentDistance > 3f * initialDistance)
        //         {
        //             Debug.Log("1");
        //             return true;
        //         }

        //         previousRigidBody = currentRigidBody;
        //     }

        //     // 检查最后一个绳子段和 prefab 之间的距离
        //     Rigidbody2D prefabRb = _prefabInstance.GetComponent<Rigidbody2D>();
        //     float lastCurrentDistance = Vector2.Distance(previousRigidBody.position, prefabRb.position);
        //     float lastInitialDistance = initialSegmentDistances[initialSegmentDistances.Count - 1];

        //     if (lastCurrentDistance > 3f * lastInitialDistance)
        //     {
        //         Debug.Log("2");
        //         return true;
        //     }

        //     return false;
        // }

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
