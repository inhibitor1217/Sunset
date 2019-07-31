﻿using UnityEngine;

public abstract class TextureProvider : MonoBehaviour
{

#if UNITY_EDITOR
    public RawImageController defaultTarget;
#endif

    [SerializeField]
    private TextureProvider[] m_PipeInputs  = { null, null, null, null };
    [SerializeField]
    private TextureProvider[] m_PipeOutputs = { null, null, null, null };

    protected RawImageController m_Target = null;

    [HideInInspector]
    public bool textureShouldUpdate = false;

    protected void Awake()
    {
        TextureProviderManager.AddTextureProvider(this);

        textureShouldUpdate = true;
    }

    protected void Start()
    {
#if UNITY_EDITOR
        if (defaultTarget)
            SetTarget(defaultTarget);
#endif
    }

    protected void OnDestroy()
    {
        // Debug.Log("Destroy: " + this);
        TextureProviderManager.RemoveTextureProvider(this);
        for (int i = 0; i < 4; i++)
        {
            if (m_PipeInputs[i])
                Unlink(m_PipeInputs[i], this);
            if (m_PipeOutputs[i])
                Unlink(this, m_PipeOutputs[i]);
        }
    }

    public abstract bool Draw();
    public abstract Texture GetTexture();

    public void SetTarget()
    {
        if (m_Target)
            m_Target.SetTexture(GetTexture());
    }

    public void SetTarget(RawImageController target)
    {
        m_Target = target;
        SetTarget();
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

    public bool HasOutput()
    {
        bool ret = false;
        for (int i = 0; i < 4; i++)
            ret |= (m_PipeOutputs[i] != null);
        return ret;
    }

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
            {
                m_PipeOutputs[i].textureShouldUpdate = true;
            }
        }
    }

}
