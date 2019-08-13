using UnityEngine;

public class CalmEffectTexture : TextureProvider
{

    private RenderTexture m_RenderTexture;
    private Material m_WaterMaterial;
    private int m_CalmPass;

    new void Awake()
    {
        base.Awake();

        /* SETUP MATERIALS */
        m_WaterMaterial = new Material(Shader.Find("Compute/WaterEffect"));
        m_WaterMaterial.EnableKeyword("USE_MIPMAP");
        m_CalmPass        = m_WaterMaterial.FindPass("Calm");

        /* SETUP PROPERTIES */
        m_WaterMaterial.SetTexture("_ImgTex", EditorSceneMaster.Instance.GetRootTextureProvider().GetBlurredTexture());

        AddProperty("PaletteTexture",     "PROVIDER");
        SubscribeProperty("PaletteTexture", m_WaterMaterial, "_PaletteTex");
        
        AddProperty("NoiseTexture",       "PROVIDER");
        SubscribeProperty("NoiseTexture", m_WaterMaterial, "_NoiseTex");

        AddProperty("EnvironmentTexture", "PROVIDER");
        SubscribeProperty("EnvironmentTexture", m_WaterMaterial, "_EnvTex");

        AddProperty("Horizon",            "FLOAT");
        SubscribeProperty("Horizon", m_WaterMaterial, "_Horizon");

        AddProperty("Perspective",        "FLOAT");
        SubscribeProperty("Perspective", m_WaterMaterial, "_Perspective");

        AddProperty("SunAltitude",        "FLOAT");
        SubscribeProperty("SunAltitude", m_WaterMaterial, "_LightDirection", 
            (Material material, string uniformName, object value) => {
                float valueFloat   = (float)value;
                float sunDirection = GetPropertyFloat("SunDirection");
                material.SetVector(uniformName, new Vector4(
                    Mathf.Cos(Mathf.Deg2Rad * valueFloat) * Mathf.Sin(Mathf.Deg2Rad * sunDirection),
                    Mathf.Cos(Mathf.Deg2Rad * valueFloat) * Mathf.Cos(Mathf.Deg2Rad * sunDirection),
                    Mathf.Sin(Mathf.Deg2Rad * valueFloat),
                    0
                ));
            });

        AddProperty("SunDirection",       "FLOAT");
        SubscribeProperty("SunDirection", m_WaterMaterial, "_LightDirection",
            (Material material, string uniformName, object value) => {
                float valueFloat   = (float)value;
                float sunAltitude = GetPropertyFloat("SunAltitude");
                material.SetVector(uniformName, new Vector4(
                    Mathf.Cos(Mathf.Deg2Rad * sunAltitude) * Mathf.Sin(Mathf.Deg2Rad * valueFloat),
                    Mathf.Cos(Mathf.Deg2Rad * sunAltitude) * Mathf.Cos(Mathf.Deg2Rad * valueFloat),
                    Mathf.Sin(Mathf.Deg2Rad * sunAltitude),
                    0
                ));
            });

        AddProperty("Rotation",           "FLOAT");
        SubscribeProperty("Rotation", m_WaterMaterial, "_Rotation",
            (Material material, string uniformName, object value) => {
                float valueFloat   = (float)value;
                material.SetVector(uniformName, new Vector4(
                     Mathf.Cos(valueFloat), 
                    -Mathf.Sin(valueFloat), 
                     Mathf.Sin(valueFloat), 
                     Mathf.Cos(valueFloat))
                );
            });
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;
        
        m_RenderTexture.DiscardContents();
        Graphics.Blit(GetPropertyProvider("NoiseTexture").GetTexture(), m_RenderTexture, m_WaterMaterial, m_CalmPass);

        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override string GetProviderName()
    {
        return "CalmEffectTexture";
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        
        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    public void Setup(int width, int height)
    {
        if (m_RenderTexture)
            m_RenderTexture.Release();

        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;
    }
}