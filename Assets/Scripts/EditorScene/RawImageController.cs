﻿using UnityEngine;
using UnityEngine.UI;

public class RawImageController : MonoBehaviour
{

    public bool movePosition = false;
    public bool moveScale = true;
    public bool useGrid = true;
    public float globalScale = 1f;

    public float xBound { get; private set; }
    public float yBound { get; private set; }

    private RawImage m_RawImage;
    private RectTransform m_RectTransform;
    private Vector2 m_ImageBaseScale;

    private Camera m_MaskCamera;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const float GRID_OPACITY_CURVE_CENTER = 24f;
#else
    private const float GRID_OPACITY_CURVE_CENTER = 15f;
#endif

    public Material material {
        get { return m_RawImage.materialForRendering; }
        set { m_RawImage.material = value; }
    }

    void Awake()
    {
        m_RawImage = GetComponent<RawImage>();
        m_RectTransform = GetComponent<RectTransform>();

        if (m_RawImage)
        {
            m_RawImage.texture = Texture2D.whiteTexture;
        }

        m_ImageBaseScale = Vector2.one;
    }

    void Update()
    {
        if (useGrid)
        {
            m_RawImage.materialForRendering.SetFloat("_Grid_Opacity",
                Mathf.Clamp(.5f + .1f * (InputManager.instance.multiplicativeScale - GRID_OPACITY_CURVE_CENTER), 0f, 1f)
            );
        }
        else
        {
            m_RawImage.materialForRendering.SetFloat("_Grid_Opacity", 0);
        }

        if (movePosition)
        {
            m_RectTransform.anchoredPosition = InputManager.instance.position * InputManager.instance.multiplicativeScale;
            if (m_MaskCamera)
                m_MaskCamera.orthographicSize = .5f * globalScale * m_ImageBaseScale.y * InputManager.instance.multiplicativeScale;
        }
        if (moveScale)
            m_RectTransform.sizeDelta = globalScale * m_ImageBaseScale * InputManager.instance.multiplicativeScale;
    }

    public Rect GetRect()
    {
        Vector3[] corners = new Vector3[4];
        m_RectTransform.GetWorldCorners(corners);
        return new Rect(corners[0], m_RectTransform.rect.size);
    }

    public Vector2 RelativeCoords(Vector2 pos)
    {
        Rect rect = GetRect();
        return new Vector2((pos.x - rect.xMin) / rect.width, (pos.y - rect.yMin) / rect.height);
    }

    public void SetTexture(Texture texture)
    {
        m_RawImage.texture = texture;

        if (texture == null)
            return;
        
        m_ImageBaseScale = new Vector2(texture.width, texture.height);

        if (movePosition)
            m_RectTransform.anchoredPosition = InputManager.instance.position * InputManager.instance.multiplicativeScale;
        
        if (moveScale)
            m_RectTransform.sizeDelta = globalScale * m_ImageBaseScale * InputManager.instance.multiplicativeScale;
        else
            m_RectTransform.sizeDelta = globalScale * m_ImageBaseScale;

        if (movePosition)
        {
            xBound = globalScale * m_ImageBaseScale.x;
            yBound = globalScale * m_ImageBaseScale.y;
        }
    }

    public void SetMaskCamera(Camera camera)
    {
        if (movePosition)
            m_MaskCamera = camera;
    }

}
