using UnityEngine;

public class FractalNoiseTexture: TextureProvider
{
    public FractalNoise.NoiseType noiseType = FractalNoise.NoiseType.Block;

    [Header("Texture")]
    public int resolution = 256;
    
    [Header("Transform")]
    public Vector2 offset = Vector2.zero;
    public Vector2 scale = 8f * Vector2.one;
    [Range(0, 360)]
    public float rotation = 0f;

    [Header("Complexity")]
    [Range(1, 10)]
    public int complexity = 7;
    [Range(0, 1)]
    public float subInfluence = .7f;
    [Range(1, 10)]
    public float subScale = 2f;
    [Range(0, 360)]
    public float subRotation = 0f;
    public Vector2 subOffset = Vector2.zero;

    [Header("Output")]
    public Gradient gradient;
    [Range(-2f, 2f)]
    public float brightness = 0f;
    [Range(0, 10)]
    public float contrast = 1f;

    void Start()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
    }

    public override bool Draw()
    {
        float[,] values = FractalNoise.GetValues(
            resolution, noiseType, 
            offset, scale, rotation,
            complexity,
            subInfluence, subOffset, subScale, subRotation
        );

        Color[] colors = new Color[resolution * resolution];
        
        for (int i = 0; i < resolution; i++)
            for (int j = 0; j < resolution; j++)
                colors[i * resolution + j] = gradient.Evaluate(
                    Mathf.Clamp((.5f + brightness) + contrast * (values[i, j] - .5f), 0f, 1f)
                );

        (texture as Texture2D).SetPixels(0, 0, resolution, resolution, colors);
        (texture as Texture2D).Apply();

        return true;
    }

}
