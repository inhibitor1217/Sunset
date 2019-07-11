using System.Collections.Generic;
using UnityEngine;

public class TextureProviderManager : MonoBehaviour
{

    [SerializeField]
    private static List<TextureProvider> textureProviders = new List<TextureProvider>();

    public static void AddTextureProvider(TextureProvider v)
    {
        textureProviders.Add(v);
    }

    public static void RemoveTextureProvider(TextureProvider v)
    {
        textureProviders.Remove(v);
    }

    void Update()
    {
        foreach (var v in textureProviders)
        {
            if (v.textureShouldUpdate)
            {
                if (v.Draw())
                {
                    v.Propagate();
                    v.textureShouldUpdate = false;
                }
            }
        }
    }

}