using UnityEngine;
using System;
using System.Collections.Generic;

public class MaskTexture : TextureProvider
{

    private Camera m_MaskCamera;
    [SerializeField]
    private RenderTexture m_RenderTexture = null;

    private bool modified = false;
    [HideInInspector]
    public bool dirty = false;
    [HideInInspector]
    public int mode;

    private Material m_BlurMaterial;
    private int m_HorizontalBlurPass;
    private int m_VerticalBlurPass;

    new void Awake()
    {
        base.Awake();

        m_BlurMaterial = new Material(Shader.Find("Compute/Blur"));
        m_BlurMaterial.SetFloat("_BlurSize", .03f);
        m_BlurMaterial.EnableKeyword("R2G");
        m_HorizontalBlurPass = m_BlurMaterial.FindPass("Horizontal");
        m_VerticalBlurPass   = m_BlurMaterial.FindPass("Vertical");
    }

    new void OnDestroy()
    {
        base.OnDestroy();

        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    void LateUpdate()
    {
        if (InputMode.Instance.isBrush()
            && InputMode.Instance.isMode(mode)
            && InputManager.Instance.withinContainer
            && InputManager.Instance.held)
        {
            modified = true;   
        }

        if (InputManager.Instance.released && modified)
        {
            dirty = true;
            modified = false;
        }
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public Texture2D GetReadableTexture()
    {
        int width  = m_RenderTexture.width;
        int height = m_RenderTexture.height;
        
        RenderTexture temp = RenderTexture.GetTemporary(width, height);
        RenderTexture prev = RenderTexture.active;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        Graphics.Blit(m_RenderTexture, temp);

        RenderTexture.active = temp;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = prev;

        RenderTexture.ReleaseTemporary(temp);

        return tex;
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        // Horizontal Blur from R channel to G channel
        Graphics.Blit(m_RenderTexture, m_RenderTexture, m_BlurMaterial, m_HorizontalBlurPass);

        Texture2D readableTex = GetReadableTexture();
        Color[] colors = readableTex.GetPixels();

        for (int x = 0; x < readableTex.width; x++)
        {
            Stack<Tuple<float, float>> stk = new Stack<Tuple<float, float>>();
            bool state = false;
            float lastBoundary = 1.0f;
            for (int y = readableTex.height - 1; y >= 0; y--)
            {
                float _y = (float)y / (float)readableTex.height;
                bool cur_state = colors[x + y * readableTex.width].r > 0.5;

                if (!state && cur_state) // MASK OFF -> MASK ON
                {
                    stk.Push(new Tuple<float, float>(_y, lastBoundary - _y));
                }

                if (state && !cur_state)
                {
                    lastBoundary = _y;
                }

                if (cur_state) // MASK ON
                {
                    while (stk.Count > 0 && stk.Peek().Item1 - _y > stk.Peek().Item2)
                        stk.Pop();
                    
                    if (stk.Count > 0)
                        colors[x + y * readableTex.width].b = stk.Peek().Item1;
                    else
                        colors[x + y * readableTex.width].b = 1;
                }
                else           // MASK OFF
                    colors[x + y * readableTex.width].b = 0;

                state = cur_state;
            }
        }
        
        readableTex.SetPixels(colors);
        readableTex.Apply();

        m_RenderTexture.DiscardContents();
        Graphics.Blit(readableTex, m_RenderTexture);

        return true;
    }

    public void Setup(int width, int height)
    {
        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.antiAliasing = 4;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;
    }

    public void SetCamera(Camera camera)
    {
        m_MaskCamera = camera;
        m_MaskCamera.targetTexture = m_RenderTexture;
    }

}
