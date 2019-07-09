using UnityEngine;

public class TextureProvider : MonoBehaviour
{

    protected Texture2D m_Texture = null;
    private RawImageController m_TargetRawImage = null;

    void Awake()
    {
        m_TargetRawImage = GetComponent<RawImageController>();
    }

    public Texture2D texture {
        get {
            return m_Texture;
        }
        set {
            m_Texture = value;
            
            if (m_TargetRawImage)
                m_TargetRawImage.SetTexture(m_Texture);
        }
    }

}
