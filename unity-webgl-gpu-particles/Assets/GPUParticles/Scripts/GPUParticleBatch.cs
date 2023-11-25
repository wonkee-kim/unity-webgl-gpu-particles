using UnityEngine;
using SpatialSys.UnitySDK;

public class GPUParticleBatch : MonoBehaviour
{
    private static int PROP_RANDOM_SPEEDS = Shader.PropertyToID("_RandomValues");
    private static int PROP_TARGET_POSITION = Shader.PropertyToID("_TargetPosition");
    private static int PROP_DELTA_TIME = Shader.PropertyToID("_DeltaTime");
    private static int PROP_PARTICLE_SPEED_ARGS = Shader.PropertyToID("_ParticleSpeedArgs");

    private static int PROP_RT_DATA = Shader.PropertyToID("_RTData");
    private static int PROP_PARTICLE_SIZE = Shader.PropertyToID("_ParticleSize");
    private static int PROP_PARTICLE_BRIGHTNESS = Shader.PropertyToID("_ParticleBrightness");

    private const int MAX_INSTANCE_COUNT = 512; // Max count depends on other data (max is 1000 if there is no other data)
    public int instanceCount { get { return _instanceCount; } }
    [SerializeField, Range(1, MAX_INSTANCE_COUNT)] private int _instanceCount = MAX_INSTANCE_COUNT;
    private int _instanceCountCached = MAX_INSTANCE_COUNT;

    [SerializeField] private Material _materialBlitOriginal;
    private Material _materialBlit;
    private RenderTexture _rt1;
    private RenderTexture _rt2;
    private bool _rtSwitcher = false;

    [SerializeField] private Material _materialDrawOriginal;
    private Material _materialDraw;
    [SerializeField] private Mesh _mesh;

    private float[] _randomValue;

    private void Start()
    {
        _materialBlit = new Material(_materialBlitOriginal);
        _materialDraw = new Material(_materialDrawOriginal);
        Setup();
    }

    private void Destroy()
    {
        Destroy(_materialBlit);
        Destroy(_materialDraw);
        Destroy(_rt1);
        Destroy(_rt2);
    }

    private void Setup()
    {
        _instanceCountCached = _instanceCount;
        _rt1 = new RenderTexture(_instanceCount, 2, 0, RenderTextureFormat.ARGBFloat); // y0: position, y1: velocity
        _rt2 = new RenderTexture(_instanceCount, 2, 0, RenderTextureFormat.ARGBFloat);

        // Initial values (Use Color instead of Vector4)
        // Color[] positions = new Color[_instanceCount];
        // Color[] velocities = new Color[_instanceCount];
        Color[] colors = new Color[_instanceCount * 2]; // positions + velocities
        _randomValue = new float[_instanceCount];
        for (int i = 0; i < _instanceCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(7.0f, 10.0f);
            float height = Random.Range(1f, 3f);
            float random1 = Random.Range(0f, 1f);
            float random2 = Random.Range(0f, 1f);
            Vector3 position = new Vector3(Mathf.Cos(angle) * distance, height, Mathf.Sin(angle) * distance);
            // Vector3 velocity = new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), Random.Range(-4f, 4f));
            Vector3 velocity = new Vector3(Mathf.Cos(angle) * 10f, Random.Range(1f, 4f), Mathf.Sin(angle) * 10f);
            colors[i] = new Color(position.x, position.y, position.z, random1);
            colors[i + _instanceCount] = new Color(velocity.x, velocity.y, velocity.z, random2);
            _randomValue[i] = Random.Range(0.5f, 1.5f);
        }
        // Write to rt1
        Texture2D texture = new Texture2D(_instanceCount, 2, TextureFormat.RGBAFloat, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels(colors);
        texture.Apply();
        Graphics.Blit(texture, _rt1);
        // Destroy(texture);
    }

    private void Update()
    {
        if (_instanceCountCached != _instanceCount)
        {
            Setup();
        }

        Vector3 targetPosition;
        if (GPUParticleSettings.useCapsule)
        {
            targetPosition = GPUParticleSettings.capsuleTransform.position;
        }
        else
        {
            // targetPosition = SpatialBridge.GetLocalAvatarPosition();
            targetPosition = ClientBridge.GetLocalAvatarPosition();
        }

        // Calculate positions using Blit instead of ComputeShader (WebGL doesn't support ComputeShader)
        RenderTexture rt1 = _rtSwitcher ? _rt2 : _rt1; // read
        RenderTexture rt2 = _rtSwitcher ? _rt1 : _rt2; // write
        _rtSwitcher = !_rtSwitcher;
        _materialBlit.SetVector(PROP_TARGET_POSITION, targetPosition);
        _materialBlit.SetFloat(PROP_DELTA_TIME, Time.deltaTime);
        _materialBlit.SetFloatArray(PROP_RANDOM_SPEEDS, _randomValue);
        _materialBlit.SetVector(PROP_PARTICLE_SPEED_ARGS, GPUParticleSettings.particleSpeedArgs);
        Graphics.Blit(rt1, rt2, _materialBlit);

        // Draw
        _materialDraw.SetTexture(PROP_RT_DATA, rt2);
        _materialDraw.SetFloatArray(PROP_RANDOM_SPEEDS, _randomValue);
        _materialDraw.SetVector(PROP_PARTICLE_SIZE, GPUParticleSettings.particleSize);
        _materialDraw.SetFloat(PROP_PARTICLE_BRIGHTNESS, GPUParticleSettings.particleBrightness);
        // Graphics.DrawMeshInstancedProcedural(_mesh, 0, _materialDraw, new Bounds(Vector3.zero, Vector3.one * 100), _instanceCount, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null
        Graphics.DrawMeshInstancedProcedural(_mesh, 0, _materialDraw, new Bounds(Vector3.zero, Vector3.one * 100), _instanceCount);
    }
}
