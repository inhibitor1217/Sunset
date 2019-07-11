using System.Collections.Generic;
using UnityEngine;

public class TextureProviderManager : MonoBehaviour
{

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

    void Update()
    {
        foreach (var v in textureProviders)
        {
            if (v.enabled && v.textureShouldUpdate)
            {
                Debug.Log("Update: " + v);
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
                Debug.Log("UpdateEager: " + v);
                if (v.Draw())
                {
                    v.Propagate();
                    v.textureShouldUpdate = false;
                }
            }
        }
    }

}