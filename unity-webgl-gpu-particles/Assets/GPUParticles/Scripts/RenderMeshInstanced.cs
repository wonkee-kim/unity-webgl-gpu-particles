using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderMeshInstanced : MonoBehaviour
{
    public Material material;
    public Mesh mesh;
    const int numInstances = 1000;

    private MaterialPropertyBlock _propertyBlock;

    // Start is called before the first frame update
    void Start()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        // _propertyBlock.Clear();
        RenderParams renderParams = new RenderParams(material);
        Matrix4x4[] instData = new Matrix4x4[numInstances];
        Vector4[] colors = new Vector4[numInstances];
        Vector4[] values = new Vector4[numInstances];
        for (int i = 0; i < numInstances; ++i)
        {
            instData[i] = Matrix4x4.Translate(new Vector3(-7.5f + i * 1.5f, 1.5f, 0.0f));
            // _propertyBlock.SetColor("_BaseColor", new Color(0.0f + i * 0.05f, 0.0f, 0.0f, 1.0f));
            colors[i] = new Vector4(0.0f + i * 0.2f, 0.0f, 0.0f, 1.0f);
            values[i] = new Vector4(0.0f, i * 0.2f, 0f, 0f);
        }
        _propertyBlock.SetVectorArray("_CustomColors", colors);
        _propertyBlock.SetVectorArray("_CustomValues", values);

        // Graphics.RenderMeshInstanced(renderParams, mesh, 0, instData);
        // Graphics.RenderMeshInstanced(renderParams, mesh, 0, null, instanceCount: 256, startInstance: 0);
        // Graphics.DrawMeshInstanced(mesh, 0, material, instData);
        Graphics.DrawMeshInstanced(mesh, 0, material, instData, instData.Length, _propertyBlock, UnityEngine.Rendering.ShadowCastingMode.On, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);

    }
}
