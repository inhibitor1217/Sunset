using UnityEngine;

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
            InputMode.instance.isSLIC()
            && InputMode.instance.isBrush()
            && InputManager.instance.held
            && InputManager.instance.withinContainer
            ? 1 : 0);

        m_Material.SetInt("_UseEraser",
            InputMode.instance.isBrush()
            && InputMode.instance.isErase()
            ? 1 : 0);

        if (InputMode.instance.isBrush() && InputMode.instance.isSLIC())
        {
            if (labelTexture)
                m_Material.SetTexture("_LabelTex", labelTexture.GetTexture());

            Vector2 inputTexCoords = EditorSceneMaster.instance.rootLayer.RelativeCoords(InputManager.instance.inputPosition);
            m_Material.SetVector("_InputCoords", new Vector4(
                inputTexCoords.x, inputTexCoords.y,
                InputManager.instance.inputPosition.x, InputManager.instance.inputPosition.y
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
        {
            m_LastFrame = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
            m_LastFrame.filterMode = FilterMode.Point;
            m_LastFrame.antiAliasing = 1;
        }
        m_LastFrame.DiscardContents();
        Graphics.Blit(dst, m_LastFrame);
    }

}
