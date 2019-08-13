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
        foreach (var v in textureProviders)
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

    public static void SetPropertyInt(string propertyName, int value)
    {
        foreach (var v in textureProviders)
            v.SetPropertyFloat(propertyName, value);
    }

    public static void SetPropertyFloat(string propertyName, float value)
    {
        foreach (var v in textureProviders)
            v.SetPropertyFloat(propertyName, value);
    }

    public static void SetPropertyVector(string propertyName, Vector4 value)
    {
        foreach (var v in textureProviders)
            v.SetPropertyVector(propertyName, value);
    }

}