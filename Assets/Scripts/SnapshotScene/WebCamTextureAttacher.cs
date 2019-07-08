using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebCamTextureAttacher : MonoBehaviour
{
    private RawImage m_RawImage;
    private WebCamTexture m_WebCamTexture;

    public Image snapshotImage;

    public const int WEBCAM_TEXTURE_WIDTH = 1920;
    public const int WEBCAM_TEXTURE_HEIGHT = 1080;

    void Awake()
    {
        m_RawImage = GetComponent<RawImage>();
        if (m_RawImage)
        {
            m_WebCamTexture = new WebCamTexture(WEBCAM_TEXTURE_WIDTH, WEBCAM_TEXTURE_HEIGHT);
            m_RawImage.texture = m_WebCamTexture;
            m_RawImage.material = new Material(Shader.Find("UI/Default"));
            m_RawImage.material.SetTexture("_MainTexture", m_WebCamTexture);

            m_WebCamTexture.Play();
        }
        if (snapshotImage)
        {
            snapshotImage.material = new Material(Shader.Find("UI/Default"));
            // Sleep unti user takes snapshot
            snapshotImage.gameObject.SetActive(false);
        }
    }

    Texture2D snapshot()
    {
        if (!m_WebCamTexture)
            return null;

        Texture2D snapshotTexture = new Texture2D(m_WebCamTexture.width, m_WebCamTexture.height);
        snapshotTexture.SetPixels(m_WebCamTexture.GetPixels());
        snapshotTexture.Apply();

        return snapshotTexture;
    }

    public void onSnapshotButtonClicked()
    {
        Debug.Log("Snapshot Button Clicked.");

        Texture2D snapshotTexture = snapshot();
        
        // Apply Snapshot Texture to View
        if (snapshotImage)
        {
            snapshotImage.rectTransform.sizeDelta = .25f * new Vector2(snapshotTexture.width, snapshotTexture.height);
            snapshotImage.rectTransform.anchoredPosition = new Vector3(-325f -.125f * snapshotTexture.width, -25f -.125f * snapshotTexture.height, 0);

            snapshotImage.sprite = Sprite.Create(
                snapshotTexture, 
                new Rect(0f, 0f, snapshotTexture.width, snapshotTexture.height), 
                new Vector2(0.5f, 0.5f)
            );

            if (!snapshotImage.gameObject.activeInHierarchy)
                snapshotImage.gameObject.SetActive(true);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidContextManager androidContext = GameObject.FindGameObjectWithTag("AndroidContextManager")
            .GetComponent<AndroidContextManager>();

        // Save Texture as PNG Image to Android Gallery.
        StartCoroutine(saveTextureToGallery(snapshotTexture));

        // Show Toast with Message.
        AndroidToast.Instance.makeText(
            androidContext.CurrentActivity,
            androidContext.ApplicationContext,
            "촬영 완료"
        );
#endif
    }

    IEnumerator saveTextureToGallery(Texture2D texture)
    {
        yield return new WaitForEndOfFrame();

        // Save the screenshot to Gallery/Photos
        Debug.Log( "Permission result: " + NativeGallery.SaveImageToGallery( texture, "Sunset", "img{0}.png" ) );
    }

}
