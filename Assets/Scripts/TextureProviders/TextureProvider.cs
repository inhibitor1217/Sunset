using UnityEngine;

public abstract class TextureProvider : MonoBehaviour
{

    protected Texture m_Texture = null;

    [Header("Texture Provider")]
    public RawImageController target = null;
    [SerializeField]
    private TextureProvider[] m_PipeInputs  = { null, null, null, null };
    [SerializeField]
    private TextureProvider[] m_PipeOutputs = { null, null, null, null };

    [HideInInspector]
    public bool textureShouldUpdate = false;

    protected void Awake()
    {
        TextureProviderManager.AddTextureProvider(this);

        textureShouldUpdate = true;
    }

    void onDestroy()
    {
        TextureProviderManager.RemoveTextureProvider(this);
    }

    public Texture texture {
        get {
            return m_Texture;
        }
        set {
            m_Texture = value;
            
            if (target)
                target.SetTexture(m_Texture);
        }
    }

    public static void Link(TextureProvider src, int srcIndex, TextureProvider dst, int dstIndex)
    {
        src.m_PipeOutputs[srcIndex] = dst;
        dst.m_PipeInputs [dstIndex] = src;
    }

    public static void Unlink(TextureProvider src, TextureProvider dst)
    {
        for (int i = 0; i < 4; i++)
        {
            if (src.m_PipeOutputs[i] == dst)
                src.m_PipeOutputs[i] = null;
            if (dst.m_PipeInputs[i] == src)
                dst.m_PipeInputs[i] = null;
        }
    }

    public abstract bool Draw();

    public int SeekFreeIndex()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_PipeOutputs[i] == null)
                return i;
        }
        return -1;
    }

    public void Propagate()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_PipeOutputs[i])
                m_PipeOutputs[i].textureShouldUpdate = true;
        }
    }

}
