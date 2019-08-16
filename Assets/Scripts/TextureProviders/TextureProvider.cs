using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class TextureProvider : MonoBehaviour
{

    [SerializeField]
    private TextureProvider[] m_PipeInputs  = { null, null, null, null };
    [SerializeField]
    private TextureProvider[] m_PipeOutputs = { null, null, null, null };

    protected RawImageController m_Target = null;
    protected List<int> m_Subscriptions = null;

    [HideInInspector]
    public bool textureShouldUpdate = false;

    protected void Awake()
    {
        TextureProviderManager.AddTextureProvider(this);

        m_Subscriptions = new List<int>();

        textureShouldUpdate = true;
    }

    protected void OnDestroy()
    {
        TextureProviderManager.RemoveTextureProvider(this);
        for (int i = 0; i < 4; i++)
        {
            if (m_PipeInputs[i])
                Unlink(m_PipeInputs[i], this);
            if (m_PipeOutputs[i])
                Unlink(this, m_PipeOutputs[i]);
        }
        foreach (var id in m_Subscriptions)
        {
            Store.instance.Unsubscribe(id);
        }
    }

    public abstract bool Draw();
    public abstract Texture GetTexture();
    public abstract string GetProviderName();

    protected void Subscribe(string[] keys, Store.SubscriptionFunction func)
    {
        int id = Store.instance.Subscribe(keys, func);
        m_Subscriptions.Add(id);
    }

    protected void Subscribe(string key, Store.SubscriptionFunction func)
    {
        Subscribe(new string[] { key }, func);
    }

    protected void Subscribe(string key, Material material, string uniformName, string type)
    {
        switch (type)
        {
        case "Float":
            Subscribe(key, (state) => {
                material.SetFloat(uniformName, (float)state[key]);
            });
            break;
        case "Vector":
            Subscribe(key, (state) => {
                material.SetVector(uniformName, (Vector4)state[key]);
            });
            break;
        }
    }

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
        if (srcIndex < 0 || srcIndex >= 4)
        {
#if UNITY_EDITOR
            Debug.Log("Link: invalid source index " + srcIndex);
#endif
            return;
        }
        if (dstIndex < 0 || dstIndex >= 4)
        {
#if UNITY_EDITOR
            Debug.Log("Link: invalid destination index " + dstIndex);
#endif
            return;
        }
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

    int seekFreeInputIndex()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_PipeInputs[i] == null)
                return i;
        }
        return -1;
    }

    int seekFreeOutputIndex()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_PipeOutputs[i] == null)
                return i;
        }
        return -1;
    }

    protected void UpdatePipeline(ref TextureProvider old, TextureProvider value)
    {
        if (old == value)
            return;

        int inIndex = -1, outIndex = -1;

        if (value && (outIndex = value.seekFreeOutputIndex()) == -1)
        {
#if UNITY_EDITOR
            Debug.Log(value.GetProviderName() + ": Pipeline Output is Full.");
#endif
            return;
        }

        if ((inIndex = seekFreeInputIndex()) == -1)
        {
#if UNITY_EDITOR
            Debug.Log(GetProviderName() + ": Pipeline Input is Full.");
#endif
            return;
        }

        if (old)
            Unlink(old, this);
        if (value)
            Link(value, outIndex, this, inIndex);

        old = value;
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
