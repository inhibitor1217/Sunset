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

    private float _horizon;
    public float horizon
    {
        get { return _horizon; }
        set {
            _horizon = value;
            // update material
            textureShouldUpdate = true;
        }
    }

    private float _perspective;
    public float perspective
    {
        get { return _perspective; }
        set {
            _perspective = value;
            // update material
            textureShouldUpdate = true;
        }
    }

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
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