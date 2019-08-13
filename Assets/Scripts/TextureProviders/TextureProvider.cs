using UnityEngine;
using System.Collections.Generic;

public abstract class TextureProvider : MonoBehaviour
{

#if UNITY_EDITOR
    public RawImageController defaultTarget;
#endif

    [SerializeField]
    private TextureProvider[] m_PipeInputs  = { null, null, null, null };
    [SerializeField]
    private TextureProvider[] m_PipeOutputs = { null, null, null, null };

    [Header("Properties")]
    [SerializeField]
    private Dictionary<string, TextureProviderProperty> _properties;

    protected RawImageController m_Target = null;

    [HideInInspector]
    public bool textureShouldUpdate = false;

    protected void Awake()
    {
        TextureProviderManager.AddTextureProvider(this);

        textureShouldUpdate = true;

        _properties = new Dictionary<string, TextureProviderProperty>();
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
    public abstract string GetProviderName();

    protected void AddProperty(string propertyName, string type)
    {
        switch (type)
        {
        case "INT":
            _properties[propertyName] = new TextureProviderIntProperty(propertyName);
            break;
        case "FLOAT":
            _properties[propertyName] = new TextureProviderFloatProperty(propertyName);
            break;
        case "VECTOR":
            _properties[propertyName] = new TextureProviderVectorProperty(propertyName);
            break;
        case "PROVIDER":
            _properties[propertyName] = new TextureProviderTextureProperty(propertyName);
            break;
        default:
#if UNITY_EDITOR
            Debug.Log("AddProperty: Property type " + type + " is not supported");
#endif
            break;
        }
    }
    protected void SubscribeProperty(string propertyName, Material material, string uniformName, TextureProviderProperty.CustomSetter mapper=null)
    {
        _properties[propertyName].Subscribe(material, uniformName, mapper);
    }

    protected void UnsubscribeProperty(string propertyName, Material material, string uniformName)
    {
        _properties[propertyName].Unsubscribe(material, uniformName);
    }

    public int GetPropertyInt(string propertyName) 
    {
        if (_properties.ContainsKey(propertyName))
        {
            return _properties[propertyName].GetInt();
        }
        else
        {
            return 0;
        }
    }
    public void SetPropertyInt(string propertyName, int value)
    {
        if (_properties.ContainsKey(propertyName))
        {
            _properties[propertyName].SetInt(value);
            textureShouldUpdate = true;
        }
    }

    public float GetPropertyFloat(string propertyName) 
    {
        if (_properties.ContainsKey(propertyName))
        {
            return _properties[propertyName].GetFloat();
        }
        else
        {
            return 0;
        }
    }
    public void SetPropertyFloat(string propertyName, float value)
    {
        if (_properties.ContainsKey(propertyName))
        {
            _properties[propertyName].SetFloat(value);
            textureShouldUpdate = true;
        }
    }

    public Vector4 GetPropertyVector(string propertyName)
    {
        if (_properties.ContainsKey(propertyName))
        {
            return _properties[propertyName].GetVector();
        }
        else
        {
            return Vector4.zero;
        }
    }
    public void SetPropertyVector(string propertyName, Vector4 value)
    {
        if (_properties.ContainsKey(propertyName))
        {
            _properties[propertyName].SetVector(value);
            textureShouldUpdate = true;
        }
    }

    public TextureProvider GetPropertyProvider(string propertyName)
    {
        if (_properties.ContainsKey(propertyName))
        {
            return _properties[propertyName].GetTextureProvider();
        }
        else
        {
            return null;
        }
    }
    public void SetPropertyProvider(string propertyName, TextureProvider value)
    {
        if (_properties.ContainsKey(propertyName))
        {
            updatePipeline(_properties[propertyName].GetTextureProvider(), value);
            _properties[propertyName].SetTextureProvider(value);
            textureShouldUpdate = true;
        }
        else
        {
            Debug.Log(GetProviderName() + " does not contain property " + propertyName);
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

    void updatePipeline(TextureProvider old, TextureProvider value)
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
