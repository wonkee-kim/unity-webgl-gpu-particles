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
    public static bool useCapsule { get { return instance._useCapsule; } }
    public static Transform capsuleTransform { get { return instance._capsuleTransform; } }
    public static Vector2 particleSize { get { return instance._particleSize * instance._sliderSize.value; } }
    public static Vector4 particleSpeedArgs { get { return Multiply(instance._particleSpeedArgs, new Vector4(instance._sliderSpeed.value, instance._sliderSpeed.value, 1, instance._sliderBounceness.value)); } }
    public static float particleBrightness { get { return instance._particleBrightness * instance._sliderBrightness.value; } }

    private bool _useCapsule = true;
    private bool _useCapsuleCached = true;
    [SerializeField] private Transform _capsuleTransform;
    [SerializeField] private Vector2 _particleSize = new Vector2(0.01f, 0.02f);
    [SerializeField] private Vector4 _particleSpeedArgs = new Vector4(2f, 5f, 30f, 2f); // (xy: speedrange, z: limit, w: bounceness)
    [SerializeField] private float _particleBrightness = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textParticleCount;
    [SerializeField] private Toggle _toggleView;
    [SerializeField] private GameObject _virtualCamera;
    [SerializeField] private Slider _sliderSize;
    [SerializeField] private Slider _sliderSpeed;
    [SerializeField] private Slider _sliderBounceness;
    [SerializeField] private Slider _sliderBrightness;

    [Header("Batches")]
    [SerializeField] private GPUParticleBatch[] _particleBatches;

    private bool _isSpatialInitialized = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _toggleView.onValueChanged.AddListener((value) => { _virtualCamera.SetActive(value); });

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
        if (!_isSpatialInitialized && ClientBridge.GetIsSceneInitialized())
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
