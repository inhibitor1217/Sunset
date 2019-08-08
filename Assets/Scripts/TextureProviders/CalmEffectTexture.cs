using UnityEngine;

public class CalmEffectTexture : TextureProvider
{
    private const int PALETTE_SRC_INDEX = 0;
    private const int NOISE_SRC_INDEX = 1;
    private const int ENV_SRC_INDEX = 2;

    private TextureProvider m_PaletteTex = null;
    public TextureProvider paletteTexture {
        get { return m_PaletteTex; }
        set {
            if (m_PaletteTex == value)
                return;

            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EffectTexture: Palette Texture Pipeline Output is Full.");
                return;
            }

            if (m_PaletteTex)
                TextureProvider.Unlink(m_PaletteTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, PALETTE_SRC_INDEX);
            
            m_PaletteTex = value;
        }
    }

    private TextureProvider m_NoiseTex = null;
    public TextureProvider noiseTexture {
        get { return m_NoiseTex; }
        set {
            if (m_NoiseTex == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EffectTexture: Noise Texture Pipeline Output is Full.");
                return;
            }

            if (m_NoiseTex)
                TextureProvider.Unlink(m_NoiseTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, NOISE_SRC_INDEX);

            m_NoiseTex = value;
        }
    }

    private TextureProvider m_EnvTex = null;
    public TextureProvider environmentTexture {
        get { return m_EnvTex; }
        set {
            if (m_EnvTex == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EffectTexture: Environment Texture Pipeline Output is Full.");
                return;
            }

            if (m_EnvTex)
                TextureProvider.Unlink(m_EnvTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, ENV_SRC_INDEX);

            m_EnvTex = value;
        }
    }

    private RenderTexture m_RenderTexture;
    private Material m_WaterMaterial;
    private int m_PerspectivePass;
    private int m_CalmPass;

    private float _horizon;
    public float horizon
    {
        get { return _horizon; }
        set {
            _horizon = value;
            if (m_WaterMaterial)
            {
                m_WaterMaterial.SetFloat("_Horizon", _horizon);
            }
        }
    }

    private float _perspective;
    public float perspective
    {
        get { return _perspective; }
        set {
            _perspective = value;
            if (m_WaterMaterial)
            {
                m_WaterMaterial.SetFloat("_Perspective", _perspective);
            }
        }
    }

    private float _sunAltitude;
    public float sunAltitude
    {
        get { return _sunAltitude; }
        set {
            _sunAltitude = value;
            if (m_WaterMaterial)
            {
                m_WaterMaterial.SetVector("_LightDirection", new Vector4(
                    Mathf.Cos(Mathf.Deg2Rad * _sunAltitude) * Mathf.Sin(Mathf.Deg2Rad * _sunDirection),
                    Mathf.Cos(Mathf.Deg2Rad * _sunAltitude) * Mathf.Cos(Mathf.Deg2Rad * _sunDirection),
                    Mathf.Sin(Mathf.Deg2Rad * _sunAltitude),
                    0
                ));
                textureShouldUpdate = true;
            }
        }
    }

    private float _sunDirection;
    public float sunDirection
    {
        get { return _sunDirection; }
        set {
            _sunDirection = value;
            if (m_WaterMaterial)
            {
                m_WaterMaterial.SetVector("_LightDirection", new Vector4(
                    Mathf.Cos(Mathf.Deg2Rad * _sunAltitude) * Mathf.Sin(Mathf.Deg2Rad * _sunDirection),
                    Mathf.Cos(Mathf.Deg2Rad * _sunAltitude) * Mathf.Cos(Mathf.Deg2Rad * _sunDirection),
                    Mathf.Sin(Mathf.Deg2Rad * _sunAltitude),
                    0
                ));
                textureShouldUpdate = true;
            }
        }
    }

    new void Awake()
    {
        base.Awake();

        m_WaterMaterial = new Material(Shader.Find("Compute/WaterEffect"));
        m_PerspectivePass = m_WaterMaterial.FindPass("Perspective");
        m_CalmPass        = m_WaterMaterial.FindPass("Calm");
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        m_WaterMaterial.SetTexture("_ImgTex", EditorSceneMaster.Instance.GetRootTextureProvider().GetBlurredTexture());
        m_WaterMaterial.SetTexture("_PaletteTex", m_PaletteTex.GetTexture());
        m_WaterMaterial.SetTexture("_EnvTex", m_EnvTex.GetTexture());

        Texture noiseTex = m_NoiseTex.GetTexture();

        RenderTexture noisePerspective = RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, 0, RenderTextureFormat.RFloat);
        noisePerspective.filterMode = FilterMode.Bilinear;
        noisePerspective.wrapMode   = TextureWrapMode.Repeat;
        Graphics.Blit(noiseTex, noisePerspective, m_WaterMaterial, m_PerspectivePass);

        m_RenderTexture.DiscardContents();
        Graphics.Blit(noisePerspective, m_RenderTexture, m_WaterMaterial, m_CalmPass);

        RenderTexture.ReleaseTemporary(noisePerspective);

        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
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