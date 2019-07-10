﻿using UnityEngine;

public class FractalNoiseRuntimeTexture : TextureProvider
{

    enum NoiseType {
        Block = 0, Linear = 1, Spline = 2
    };
    enum FractalType {
        Basic = 0, Turbulent = 1, Rocky = 2
    };

    [Header("Texture")]
    public int resolution = 256;

    [Header("General")]
    [SerializeField]
    private int _seed = 0;
    public int seed {
        get { return _seed; }
        set {
            _seed = value;
            loadGradients(_seed);
        }
    }
    [SerializeField]
    private NoiseType _noiseType = NoiseType.Spline;
    public int noiseType {
        get { return (int)_noiseType; }
        set { updateNoiseType(value); }
    }
    [SerializeField]
    private FractalType _fractalType = FractalType.Basic;
    public int fractalType {
        get { return (int)_fractalType; }
        set { updateFractalType(value); }
    }

    [Header("Transform")]
    [SerializeField]
    private Vector2 _offset = Vector2.zero;
    public Vector2 offset {
        get { return _offset; }
        set { updateGlobalOffset(value); }
    }
    [SerializeField]
    private Vector2 _scale = 8f * Vector2.one;
    public Vector2 scale {
        get { return _scale; }
        set { updateGlobalScale(value); }
    }
    [SerializeField, Range(-360, 360)]
    private float _rotation = 0f;
    public float rotation {
        get { return _rotation; }
        set { updateGlobalRotation(value); }
    }

    [Header("Complexity")]
    [SerializeField, Range(1, 10)]
    private int _complexity = 6;
    public int complexity {
        get { return _complexity; }
        set { updateComplexity(value); }
    }
    [SerializeField, Range(0, 1)]
    private float _subInfluence = .5f;
    public float subInfluence {
        get { return _subInfluence; }
        set { updateSubInfluence(value); }
    }
    [SerializeField]
    private Vector2 _subScale = 2f * Vector2.one;
    public Vector2 subScale {
        get { return _subScale; }
        set { updateSubScale(value); }
    }
    [SerializeField, Range(-360, 360)]
    private float _subRotation = 0f;
    public float subRotation {
        get { return _subRotation; }
        set { updateSubRotation(value); }
    }
    [SerializeField]
    private Vector2 _subOffset = Vector2.zero;
    public Vector2 subOffset {
        get { return _subOffset; }
        set { updateSubOffset(value); }
    }

    [Header("Output")]
    [SerializeField, Range(-2f, 2f)]
    private float _brightness = 0f;
    public float brightness {
        get { return _brightness; }
        set { updateBrightness(value); }
    }
    [SerializeField, Range(0, 10)]
    private float _contrast = 1f;
    public float contrast {
        get { return _contrast; }
        set { updateContrast(value); }
    }

    private Material m_FractalNoiseMaterial;
    private RenderTexture m_RenderTexture;

    void Awake()
    {
        base.Awake();
        m_FractalNoiseMaterial = new Material(Shader.Find("Compute/FractalNoise"));
        m_RenderTexture = new RenderTexture(resolution, resolution, 0);
    }

    void Start()
    {
        texture = m_RenderTexture;
        loadGradients(seed);
    }

    void Update()
    {
        m_RenderTexture.DiscardContents();
        Graphics.Blit(null, m_RenderTexture, m_FractalNoiseMaterial);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        noiseType = (int)_noiseType;
        fractalType = (int)_fractalType;
        seed = _seed;
        offset = _offset;
        scale = _scale;
        rotation = _rotation;
        complexity = _complexity;
        subInfluence = _subInfluence;
        subScale = _subScale;
        subOffset = _subOffset;
        brightness = _brightness;
        contrast = _contrast;
    }
#endif

    void updateNoiseType(int value)
    {
        _noiseType = (NoiseType)value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetInt("_NoiseType", value);
        }
    }

    void updateFractalType(int value)
    {
        _fractalType = (FractalType)value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetInt("_FractalType", value);
        }
    }

    void loadGradients(int seed)
    {
        Random.InitState(_seed);
        Vector4[] gradients = new Vector4[256];
        for (int i = 0; i < 256; i++)
            gradients[i] = new Vector4(Random.value, Random.value, Random.value, 1);
            // gradients[i] = 2f * new Vector4(Random.value, Random.value, Random.value, 1f) - Vector4.one;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetVectorArray("_Gradients", gradients);
        }
    }

    void updateGlobalOffset(Vector2 value)
    {
        _offset = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_GlobalOffsetScale");
            m_FractalNoiseMaterial.SetVector("_GlobalOffsetScale", new Vector4(value.x, value.y, oldValue.z, oldValue.w));
        }
    }

    void updateGlobalScale(Vector2 value)
    {
        _scale = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_GlobalOffsetScale");
            m_FractalNoiseMaterial.SetVector("_GlobalOffsetScale", new Vector4(oldValue.x, oldValue.y, value.x, value.y));
        }
    }

    void updateGlobalRotation(float value)
    {
        _rotation = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_GlobalRotation", _rotation);
        }
    }

    void updateComplexity(int value)
    {
        _complexity = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetInt("_Complexity", _complexity);
        }
    }

    void updateSubInfluence(float value)
    {
        _subInfluence = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_SubInfluence", _subInfluence);
        }
    }

    void updateSubScale(Vector2 value)
    {
        _subScale = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_SubOffsetScale");
            m_FractalNoiseMaterial.SetVector("_SubOffsetScale", new Vector4(oldValue.x, oldValue.y, _subScale.x, _subScale.y));
        }
    }

    void updateSubRotation(float value)
    {
        _subRotation = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_SubRotation", _subRotation);
        }
    }

    void updateSubOffset(Vector2 value)
    {
        _subOffset = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_SubOffsetScale");
            m_FractalNoiseMaterial.SetVector("_SubOffsetScale", new Vector4(_subOffset.x, _subOffset.y, oldValue.z, oldValue.w));
        }
    }

    void updateBrightness(float value)
    {
        _brightness = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_Brightness", _brightness);
        }
    }

    void updateContrast(float value)
    {
        _contrast = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_Contrast", _contrast);
        }
    }

}
