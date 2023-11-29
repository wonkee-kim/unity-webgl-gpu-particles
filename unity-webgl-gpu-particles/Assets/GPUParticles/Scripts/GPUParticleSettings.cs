using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GPUParticleSettings : MonoBehaviour
{
    private static GPUParticleSettings instance;
    public static Vector3 targetPosition => instance._targetPosition;
    public static Vector2 particleSize { get { return instance._particleSize * instance._sliderSize.value; } }
    public static Vector4 particleSpeedArgs { get { return Multiply(instance._particleSpeedArgs, new Vector4(instance._sliderSpeed.value, instance._sliderSpeed.value, 1, instance._sliderBounceness.value)); } }
    public static float particleBrightness { get { return instance._particleBrightness * instance._sliderBrightness.value; } }
    public static float particleGravity => instance._sliderGravity.value;
    public static float particleExplosion => instance._particleExplosion;

    [Header("Target")]
    private bool _useCapsule = true;
    private bool _useCapsuleCached = true;
    private Vector3 _targetPosition;
    [SerializeField] private Transform _capsuleTransform;
    [SerializeField] private Vector2 _centerMoveSpeed = new Vector2(2.17f, 1.73f);
    [SerializeField] private Vector2 _centerMoveRadius = new Vector2(1.3f, 2.2f);

    [Header("Particle Settings")]
    [SerializeField] private Vector2 _particleSize = new Vector2(0.01f, 0.02f);
    [SerializeField, Tooltip("xy: speedrange, z: speedlimit, w: bounceness")]
    private Vector4 _particleSpeedArgs = new Vector4(2f, 5f, 30f, 1f);
    [SerializeField] private float _particleBrightness = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textParticleCount;
    [SerializeField] private Button _explodeButton;
    [SerializeField] private Toggle _toggleView;
    [SerializeField] private GameObject _virtualCamera;
    [SerializeField] private Slider _sliderSize;
    [SerializeField] private Slider _sliderSpeed;
    [SerializeField] private Slider _sliderGravity;
    [SerializeField] private Slider _sliderBounceness;
    [SerializeField] private Slider _sliderBrightness;

    [Header("Batches")]
    [SerializeField] private GPUParticleBatch[] _particleBatches;

    private bool _isSpatialInitialized = false;

    private float _particleExplosion = 0f;
    private bool _explodeButtonPressed = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _virtualCamera.SetActive(_toggleView.isOn);
        _toggleView.onValueChanged.AddListener((value) => { _virtualCamera.SetActive(value); });
        _explodeButton.onClick.AddListener(() => { _explodeButtonPressed = true; });

        if (_particleBatches != null && _particleBatches.Length > 0)
        {
            int count = 0;
            foreach (var batch in _particleBatches)
            {
                count += batch.instanceCount;
            }
            _textParticleCount.text = $"Particle Count: {count.ToString("N0")}";
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private static Vector4 Multiply(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
    }

    private void Update()
    {
#if !UNITY_EDITOR
        if (!_isSpatialInitialized && SpatialBridge.GetIsSceneInitialized())
        {
            _isSpatialInitialized = true;
            _useCapsule = false;
            if (_useCapsuleCached != _useCapsule)
            {
                _useCapsuleCached = _useCapsule;
                _capsuleTransform.gameObject.SetActive(_useCapsule);
            }
        }
#endif
        if (Input.GetKeyDown(KeyCode.F))
        {
            _toggleView.isOn = !_toggleView.isOn;
        }

        if (_useCapsule)
        {
            _targetPosition = _capsuleTransform.position;
        }
        else
        {
            _targetPosition = SpatialBridge.GetLocalAvatarPosition();
        }
        _targetPosition += new Vector3(Mathf.Cos(Time.time * _centerMoveSpeed[0]) * _centerMoveRadius[0], 0f, Mathf.Sin(Time.time * _centerMoveSpeed[0]) * _centerMoveRadius[0])
        + new Vector3(Mathf.Cos(Time.time * _centerMoveSpeed[1] * 0.5f) * _centerMoveRadius[1], Mathf.Sin(Time.time * _centerMoveSpeed[1] * 0.5f) * _centerMoveRadius[1], 0f);

        if (Input.GetKeyDown(KeyCode.E) || _explodeButtonPressed)
        {
            _explodeButtonPressed = false;
            _particleExplosion = 1f;
        }
        else
        {
            _particleExplosion = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_targetPosition, 0.5f);
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(GetParticleBatches))]
    public void GetParticleBatches()
    {
        _particleBatches = FindObjectsByType<GPUParticleBatch>(FindObjectsSortMode.None);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GPUParticleSettings))]
public class GPUParticleSettingsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GetParticleBatches"))
        {
            ((GPUParticleSettings)target).GetParticleBatches();
        }
    }
}
#endif
