using UnityEngine;
using UnityEngine.UI;

public class SnapshotGalleryButton : MonoBehaviour
{

    private Button m_Button;

    public Image galleryImage;

#if UNITY_ANDROID && !UNITY_EDITOR
    void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(onGalleryButtonClicked);

        if (galleryImage)
        {
            galleryImage.material = new Material(Shader.Find("UI/Default"));
            galleryImage.gameObject.SetActive(false);
        }
    }

    void onGalleryButtonClicked()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
            (path) => {
                
                Debug.Log("Picked image path: " + path + " from Gallery");

                if (path != null)
                {
                    /*
                    Texture2D imageTexture = NativeGallery.LoadImageAtPath(path);

                    // Load to UI Image Panel (for Debugging)
                    if (galleryImage)
                    {
                        galleryImage.rectTransform.sizeDelta = new Vector2(imageTexture.width, imageTexture.height);
                        galleryImage.sprite = Sprite.Create(
                            imageTexture,
                            new Rect(0f, 0f, imageTexture.width, imageTexture.height),
                            new Vector2(.5f, .5f)
                        );

                        if (!galleryImage.gameObject.activeInHierarchy)
                            galleryImage.gameObject.SetActive(true);

                        Debug.Log("Texture dimension: " + imageTexture.width + ", " + imageTexture.height);
                    }
                    */

                    // Set Global Storage
                    PlayerPrefs.SetString("image_path", path);

                    // Load Editting Scene
                    Application.LoadLevel(1);
                }

            }
        );
    }
#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif

}