using UnityEngine;
using System.Collections.Generic;

public class FlowTexture : TextureProvider
{
    private Texture2D m_Texture;

    new void Awake()
    {
        base.Awake();
    }

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return m_Texture;
    }

    public override string GetProviderName()
    {
        return "FlowTexture";
    }

    public void GenerateTexture(FlowController controller)
    {
        m_Texture = new Texture2D(64, 64);

        List<List<Vector3>> pivots = controller.pivots;

        // TODO
    }

}