using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class TextureProviderProperty
{

    public TextureProviderProperty(string name)
    {
        _name = name;
        _subscribptions = new List<(Material, string, CustomSetter)>();
    }

    private string _name;
    private List<ValueTuple<Material, string, CustomSetter>> _subscribptions;

    public delegate void CustomSetter(Material material, string propertyName, object value);

    public virtual int GetInt() 
    {
        return 0;
    }
    public virtual float GetFloat() 
    {
        return 0;
    }
    public virtual Vector4 GetVector() 
    {
        return Vector4.zero;
    }
    public virtual TextureProvider GetTextureProvider() 
    {
        return null;
    }
    
    public virtual void SetInt(int value) { }
    public virtual void SetFloat(float value) { }
    public virtual void SetVector(Vector4 value) { }
    public virtual void SetTextureProvider(TextureProvider value) { }

    public void Subscribe(Material material, string uniformName, CustomSetter mapper=null)
    {
        _subscribptions.Add((material, uniformName, mapper));
    }

    public void Unsubscribe(Material material, string uniformName)
    {
        foreach ((Material _material, string _uniformName, CustomSetter _mapper) in _subscribptions)
        {
            if (material == _material && uniformName == _uniformName)
                _subscribptions.Remove((_material, _uniformName, _mapper));
        }
    }

    protected void UpdateMaterials()
    {
        foreach ((Material _material, string _uniformName, CustomSetter _mapper) in _subscribptions)
        {
            UpdateMaterial(_material, _uniformName, _mapper);
        }
    }

    protected abstract void UpdateMaterial(Material material, string uniformName, CustomSetter mapper);

}

public class TextureProviderIntProperty : TextureProviderProperty
{

    private int _value;

    public TextureProviderIntProperty(string name) : base(name) {}

    public override void SetInt(int value) { _value = value; UpdateMaterials(); }
    public override int GetInt() { return _value; }

    protected override void UpdateMaterial(Material material, string uniformName, CustomSetter mapper)
    {
        if (mapper != null)
            mapper(material, uniformName, _value);
        else
            material.SetFloat(uniformName, _value);
    }

}

public class TextureProviderFloatProperty : TextureProviderProperty
{
    private float _value;

    public TextureProviderFloatProperty(string name) : base(name) {}

    public override void SetFloat(float value) { _value = value; UpdateMaterials(); }
    public override float GetFloat() { return _value; }

    protected override void UpdateMaterial(Material material, string uniformName, CustomSetter mapper)
    {
        if (mapper != null)
            mapper(material, uniformName, _value);
        else
            material.SetFloat(uniformName, _value);
    }

}

public class TextureProviderVectorProperty : TextureProviderProperty
{
    private Vector4 _value;

    public TextureProviderVectorProperty(string name) : base(name) {}

    public override void SetVector(Vector4 value) { _value = value; UpdateMaterials(); }
    public override Vector4 GetVector() { return _value; }

    protected override void UpdateMaterial(Material material, string uniformName, CustomSetter mapper)
    {
        if (mapper != null)
            mapper(material, uniformName, _value);
        else
            material.SetVector(uniformName, _value);
    }
}

public class TextureProviderTextureProperty : TextureProviderProperty
{
    private TextureProvider _value;

    public TextureProviderTextureProperty(string name) : base(name) {}

    public override void SetTextureProvider(TextureProvider value) { _value = value; UpdateMaterials(); }
    public override TextureProvider GetTextureProvider() { return _value; }

    protected override void UpdateMaterial(Material material, string uniformName, CustomSetter mapper)
    {
        if (mapper != null)
            mapper(material, uniformName, _value);
        else
            material.SetTexture(uniformName, _value.GetTexture());
    }
}