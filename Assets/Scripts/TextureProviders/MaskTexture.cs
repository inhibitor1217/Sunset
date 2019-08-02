using UnityEngine;

public class MaskTexture : TextureProvider
{

    private Camera m_MaskCamera;
    [SerializeField]
    private RenderTexture m_RenderTexture = null;
    [SerializeField]
    private RenderTexture m_BlurredTexture = null;

    private bool modified = false;
    [HideInInspector]
    public bool dirty = false;
    [HideInInspector]
    public int mode;

    private Material m_BlurMaterial;
    private int m_HorizontalBlurPass;
    private int m_VerticalBlurPass;

    new void Awake()
    {
        base.Awake();

        m_BlurMaterial = new Material(Shader.Find("Compute/Blur"));
        m_BlurMaterial.SetFloat("_BlurSize", .03f);
        m_HorizontalBlurPass = m_BlurMaterial.FindPass("Horizontal");
        m_VerticalBlurPass   = m_BlurMaterial.FindPass("Vertical");
    }

    new void OnDestroy()
    {
        base.OnDestroy();

        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    void Update()
    {
        if (InputMode.Instance.isBrush()
            && InputMode.Instance.isMode(mode)
            && InputManager.Instance.withinContainer
            && InputManager.Instance.held)
        {
            modified = true;   
        }

        if (InputManager.Instance.released && modified)
        {
            dirty = true;
            modified = false;
        }
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public Texture2D GetReadableTexture()
    {
        int width  = m_RenderTexture.width;
        int height = m_RenderTexture.height;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = m_RenderTexture;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        RenderTexture.active = prev;

        return tex;
    }

    public Texture GetBlurredTexture()
    {
        return m_BlurredTexture;
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        Graphics.Blit(m_RenderTexture, m_BlurredTexture, m_BlurMaterial, m_HorizontalBlurPass);

        return true;
    }

    public void Setup(int width, int height)
    {
        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.antiAliasing = 4;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;

        m_BlurredTexture = new RenderTexture(width / 4, height / 4, 0, RenderTextureFormat.R8);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Bilinear;
    }

    public void SetCamera(Camera camera)
    {
        m_MaskCamera = camera;
        m_MaskCamera.targetTexture = m_RenderTexture;
    }

}
