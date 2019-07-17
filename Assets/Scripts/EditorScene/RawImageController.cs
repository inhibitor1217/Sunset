using UnityEngine;
using UnityEngine.UI;

public class RawImageController : MonoBehaviour
{

    public RectTransform container;
    public TextureProvider provider;

    public bool isRoot = false;
    public Camera maskCamera = null;

    private RawImage m_RawImage;
    private RectTransform m_RectTransform;
    private Vector2 m_ImageBaseScale;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const float GRID_OPACITY_CURVE_CENTER = 24f;
#else
    private const float GRID_OPACITY_CURVE_CENTER = 15f;
#endif

    void Awake()
    {
        m_RawImage = GetComponent<RawImage>();
        m_RectTransform = GetComponent<RectTransform>();

        if (m_RawImage)
        {
            m_RawImage.texture = Texture2D.whiteTexture;
        }

        if (provider)
            provider.SetTarget(this);

        TextureProviderManager.UpdateEager();

        m_ImageBaseScale = Vector2.one;
    }

    void Update()
    {

        m_RawImage.materialForRendering.SetFloat("_Grid_Opacity",
            Mathf.Clamp(.5f + .1f * (InputManager.Instance.MultiplicativeScale - GRID_OPACITY_CURVE_CENTER), 0f, 1f)
        );

        if (isRoot)
        {
            m_RectTransform.anchoredPosition = InputManager.Instance.Position * InputManager.Instance.MultiplicativeScale;
            if (maskCamera)
            {
                maskCamera.orthographicSize = .5f * m_ImageBaseScale.y * InputManager.Instance.MultiplicativeScale;
            }
        }
        m_RectTransform.sizeDelta = m_ImageBaseScale * InputManager.Instance.MultiplicativeScale;
    }

    public void SetTexture(Texture texture)
    {
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        m_RawImage.texture = texture;
        
        m_ImageBaseScale = new Vector2(texture.width, texture.height);

        m_RectTransform.anchoredPosition = InputManager.Instance.Position * InputManager.Instance.MultiplicativeScale;
        m_RectTransform.sizeDelta = m_ImageBaseScale * InputManager.Instance.MultiplicativeScale;

        InputManager.Instance.xBound = m_ImageBaseScale.x;
        InputManager.Instance.yBound = m_ImageBaseScale.y;
    }

}
