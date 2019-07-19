﻿using UnityEngine;
using UnityEngine.UI;

public class RawImageController : MonoBehaviour
{

    public bool isRoot = false;

    private RawImage m_RawImage;
    private RectTransform m_RectTransform;
    private Vector2 m_ImageBaseScale;

    private Camera[] m_MaskCameras;

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

        m_ImageBaseScale = Vector2.one;

        m_MaskCameras = new Camera[EditorSceneMaster.MAX_EFFECTS];
    }

    void Update()
    {

        m_RawImage.materialForRendering.SetFloat("_Grid_Opacity",
            Mathf.Clamp(.5f + .1f * (InputManager.Instance.MultiplicativeScale - GRID_OPACITY_CURVE_CENTER), 0f, 1f)
        );

        if (isRoot)
        {
            m_RectTransform.anchoredPosition = InputManager.Instance.Position * InputManager.Instance.MultiplicativeScale;
            foreach (var maskCamera in m_MaskCameras)
            {
                if (maskCamera)
                    maskCamera.orthographicSize = .5f * m_ImageBaseScale.y * InputManager.Instance.MultiplicativeScale;
            }
        }
        m_RectTransform.sizeDelta = m_ImageBaseScale * InputManager.Instance.MultiplicativeScale;
    }

    public void SetTexture(Texture texture)
    {
        m_RawImage.texture = texture;

        if (texture == null)
            return;
        
        m_ImageBaseScale = new Vector2(texture.width, texture.height);

        if (isRoot)
            m_RectTransform.anchoredPosition = InputManager.Instance.Position * InputManager.Instance.MultiplicativeScale;
        m_RectTransform.sizeDelta = m_ImageBaseScale * InputManager.Instance.MultiplicativeScale;

        InputManager.Instance.xBound = m_ImageBaseScale.x;
        InputManager.Instance.yBound = m_ImageBaseScale.y;
    }

    public void SetMaskCamera(Camera camera, int maskIndex)
    {
        if (isRoot)
            m_MaskCameras[maskIndex] = camera;
    }

}
