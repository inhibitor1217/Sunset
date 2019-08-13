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

    public float estimatedHorizon { get; private set; } = .5f;

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

    public override string GetProviderName()
    {
        return "MaskTexture";
    }

    public Texture2D GetReadableTexture()
    {
        int width  = m_RenderTexture.width  * 2;
        int height = m_RenderTexture.height * 2;
        
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

        Texture2D readableTex = GetReadableTexture();
        Color32[] colors = readableTex.GetPixels32();

        List<float> horizons = new List<float>();

        for (int x = 0; x < readableTex.width; x++)
        {
            /* Mirror Y Coordinate */
            Stack<Tuple<float, float>> stk = new Stack<Tuple<float, float>>();
            bool state = false;
            float lastBoundary = 1.0f;

            /* Horizon Estimation */
            bool found = false;

            for (int y = readableTex.height - 1; y >= 0; y--)
            {
                float _y = (float)y / (float)readableTex.height;
                bool cur_state = colors[x + y * readableTex.width].r >= 1;

                /* Mirror Y Coordinate  */
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
                    {
                        colors[x + y * readableTex.width].g = (byte)(Mathf.FloorToInt(stk.Peek().Item1 * 65535f) / 256);
                        colors[x + y * readableTex.width].b = (byte)(Mathf.FloorToInt(stk.Peek().Item1 * 65535f) % 256);
                    }
                    else
                    {
                        colors[x + y * readableTex.width].g = 255;
                        colors[x + y * readableTex.width].b = 255;
                    }
                }
                else // MASK OFF
                {
                    colors[x + y * readableTex.width].g = 0;
                    colors[x + y * readableTex.width].b = 0;
                }

                state = cur_state;

                /* Horizon Estimation */
                if (!found && cur_state)
                {
                    found = true;
                    horizons.Add(_y);
                }
            }
        }
        
        /* Update Mirror Texture */
        readableTex.SetPixels32(colors);
        readableTex.Apply();

        m_RenderTexture.DiscardContents();
        Graphics.Blit(readableTex, m_RenderTexture);

        /* Update Estimated Horizon */
        if (horizons.Count > 0)
        {
            horizons.Sort();
            estimatedHorizon = horizons[Mathf.Clamp(Mathf.RoundToInt(.97f * horizons.Count), 0, horizons.Count - 1)];
        }

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
