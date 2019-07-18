using System.Collections.Generic;
using UnityEngine;

public class TextureProviderManager : MonoBehaviour
{

    private static TextureProviderManager m_Instance;
    public static TextureProviderManager Instance { get { return m_Instance; } }

    [SerializeField]
    private static List<TextureProvider> textureProviders = new List<TextureProvider>();

    public static void AddTextureProvider(TextureProvider v)
    {
        textureProviders.Add(v);
        // Debug.Log("Count: " + textureProviders.Count);
    }

    public static void RemoveTextureProvider(TextureProvider v)
    {
        textureProviders.Remove(v);
        // Debug.Log("Count: " + textureProviders.Count);
    }

    void Awake()
    {
        m_Instance = this;
    }

    void Update()
    {
        foreach (var v in textureProviders)
        {
            if (v.enabled && v.textureShouldUpdate)
            {
                #if UNITY_EDITOR
                Debug.Log("Update: " + v);
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
        foreach (var v in textureProviders)
        {
            if (v.enabled)
            {
                #if UNITY_EDITOR
                Debug.Log("UpdateEager: " + v);
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