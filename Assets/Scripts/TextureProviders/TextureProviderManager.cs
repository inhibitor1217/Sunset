using System.Collections.Generic;
using UnityEngine;

public class TextureProviderManager : MonoBehaviour
{

    public TextureProviderManager instance { get; private set; } 

    private bool _initialized = false;

    private static List<TextureProvider> _textureProviders;

    public static void AddTextureProvider(TextureProvider v)
    {
        _textureProviders.Add(v);
    }

    public static void RemoveTextureProvider(TextureProvider v)
    {
        _textureProviders.Remove(v);
    }

    void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        _initialized = true;

        _textureProviders = new List<TextureProvider>();
    }

    void Update()
    {
        if (!_initialized)
            return;

        foreach (var v in _textureProviders)
        {
            if (v.enabled && v.textureShouldUpdate)
            {
                #if UNITY_EDITOR
                Debug.Log("Update: " + v.GetProviderName());
                #endif
                if (v.Draw())
                {
                    v.Propagate();
                    v.textureShouldUpdate = false;
                }
            }
        }
    }

    public static void UpdateEager()
    {
        foreach (var v in _textureProviders)
        {
            if (v.enabled)
            {
                #if UNITY_EDITOR
                Debug.Log("UpdateEager: " + v.GetProviderName());
                #endif
                if (v.Draw())
                {
                    v.Propagate();
                    v.textureShouldUpdate = false;
                }
            }
        }
    }

}