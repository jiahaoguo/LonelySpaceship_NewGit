using UnityEngine;

public class SkyboxBlender : MonoBehaviour
{
    public Material blendSkybox; // A custom shader with 2 cubemap slots + _Blend
    public Cubemap skyA;
    public Cubemap skyB;
    public float blendDuration = 5f;

    private float t = 0f;
    private bool blending = false;

    void Start()
    {
        blendSkybox.SetTexture("_Tex1", skyA);
        blendSkybox.SetTexture("_Tex2", skyB);
        RenderSettings.skybox = blendSkybox;
        StartBlend();
    }

    public void StartBlend()
    {
        t = 0f;
        blending = true;
    }

    void Update()
    {
        if (blending)
        {
            t += Time.deltaTime / blendDuration;
            blendSkybox.SetFloat("_Blend", Mathf.Clamp01(t));
            if (t >= 1f) blending = false;
        }
    }
}
