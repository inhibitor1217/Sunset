﻿using UnityEngine;

public class MaskRendererCamera : MonoBehaviour
{
    
    private Material m_Material;
    private RenderTexture m_LastFrame;

    public SLICLabelTexture labelTexture;

    void Awake()
    {
        m_Material = new Material(Shader.Find("Compute/MaskRenderer"));
    }

    void Update()
    {
        m_Material.SetInt("_UseLabel",
            InputMode.Instance.isSLIC()
            && InputMode.Instance.isBrush()
            && InputManager.Instance.held
            && InputManager.Instance.withinContainer
            ? 1 : 0);

        m_Material.SetInt("_UseEraser",
            InputMode.Instance.isBrush()
            && InputMode.Instance.isErase()
            ? 1 : 0);

        if (InputMode.Instance.isBrush() && InputMode.Instance.isSLIC())
        {
            if (labelTexture)
                m_Material.SetTexture("_LabelTex", labelTexture.GetTexture());

            Vector2 inputTexCoords = EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition);
            m_Material.SetVector("_InputCoords", new Vector4(
                inputTexCoords.x, inputTexCoords.y,
                InputManager.Instance.inputPosition.x, InputManager.Instance.inputPosition.y
            ));
        }
    }

    void OnDestroy()
    {
        if (m_LastFrame)
            RenderTexture.ReleaseTemporary(m_LastFrame);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (m_LastFrame)
            m_Material.SetTexture("_PrevTex", m_LastFrame);
        Graphics.Blit(src, dst, m_Material);
        if (!m_LastFrame)
            m_LastFrame = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        m_LastFrame.DiscardContents();
        Graphics.Blit(dst, m_LastFrame);
    }

}
