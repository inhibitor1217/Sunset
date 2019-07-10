using UnityEngine;

public class TextureProvider : MonoBehaviour
{

    protected Texture m_Texture = null;
    private RawImageController m_TargetRawImage = null;

    protected void Awake()
    {
        m_TargetRawImage = GetComponent<RawImageController>();
        Debug.Log(m_TargetRawImage);
    }

    public Texture texture {
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
