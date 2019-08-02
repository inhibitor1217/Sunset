using UnityEngine;
using System;
using System.Collections.Generic;
public class MaskMirrorTexture : TextureProvider
{

    private const int SOURCE_INDEX = 0;

    private MaskTexture m_SrcTexture = null;
    public MaskTexture sourceTexture {
        get { return m_SrcTexture; }
        set {
            if (m_SrcTexture == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("MaskMirrorTexture: Source Texture Pipeline Output is Full.");
                return;
            }

            if (m_SrcTexture)
                TextureProvider.Unlink(m_SrcTexture, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, SOURCE_INDEX);
            
            m_SrcTexture = value;
        }
    }

    [SerializeField]
    private Texture2D m_MirrorTexture;

    public override Texture GetTexture()
    {
        return m_MirrorTexture;
    }

    public override bool Draw()
    {
        if (!m_MirrorTexture)
            return false;

        Texture2D src = m_SrcTexture.GetReadableTexture();
        Color32[] colors = src.GetPixels32();
        Color[] mirror = new Color[m_MirrorTexture.width * m_MirrorTexture.height];

        for (int x = 0; x < m_MirrorTexture.width; x++)
        {
            Stack<Tuple<float, float>> stk = new Stack<Tuple<float, float>>();
            bool state = false;
            float lastBoundary = 1.0f;
            for (int y = m_MirrorTexture.height - 1; y >= 0; y--)
            {
                float _y = (float)y / (float)m_MirrorTexture.height;
                bool cur_state = colors[(4 * x) + (4 * y) * src.width].r > 0.5;

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
                        mirror[x + y * m_MirrorTexture.width] = new Color(stk.Peek().Item1, 0, 0, 1);
                    else
                        mirror[x + y * m_MirrorTexture.width] = new Color(1, 0, 0, 1);
                }
                else           // MASK OFF
                {
                    mirror[x + y * m_MirrorTexture.width] = new Color(0, 0, 0, 0);
                }

                state = cur_state;
            }
        }
        
        m_MirrorTexture.SetPixels(mirror);
        m_MirrorTexture.Apply();

        return true;
    }

    public void Setup()
    {
        Texture srcTex = m_SrcTexture.GetTexture();
        m_MirrorTexture = new Texture2D(srcTex.width / 4, srcTex.height / 4, TextureFormat.R16, false);
    }

}