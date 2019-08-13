using UnityEngine;

public class FlowTexture : TextureProvider
{

    private RenderTexture m_RenderTexture = null;

    [SerializeField]
    private Mesh _flowVectorMesh;
    public Mesh flowVectorMesh {
        get { return _flowVectorMesh; }
        set {
            _flowVectorMesh = value;
            textureShouldUpdate = true;
        }
    }

    new void Awake()
    {
        base.Awake();

        /* SETUP MATERIALS  */


        /* SETUP PROPERTIES */
        AddProperty("Horizon",            "FLOAT");
        // SubscribeProperty("Horizon", <MATERIAL>, "_Horizon");

        AddProperty("Perspective",        "FLOAT");
        // SubscribeProperty("Perspective", MATERIAL>, "_Perspective");
    }

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override string GetProviderName()
    {
        return "FlowTexture";
    }

    new void OnDestroy()
    {
        base.OnDestroy();

        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    public void Setup(int width, int height)
    {
        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RGFloat);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.antiAliasing = 4;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Bilinear;
    }

}