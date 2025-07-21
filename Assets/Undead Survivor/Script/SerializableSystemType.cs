using System;
using UnityEngine;

/// <summary>
/// System.Type을 Unity 인스펙터에서 직렬화하고 편집할 수 있도록 래핑하는 클래스입니다.
/// </summary>
[Serializable]
public class SerializableSystemType
{
    [SerializeField]
    private string assemblyQualifiedName;

    private Type _type;
    public Type Type
    {
        get
        {
            if (_type == null && !string.IsNullOrEmpty(assemblyQualifiedName))
            {
                _type = Type.GetType(assemblyQualifiedName);
            }
            return _type;
        }
        set
        {
            _type = value;
            assemblyQualifiedName = value != null ? value.AssemblyQualifiedName : null;
        }
    }

    // System.Type으로 암시적 변환을 지원하여 사용 편의성을 높입니다.
    public static implicit operator Type(SerializableSystemType sst) => sst.Type;
    // Type에서 SerializableSystemType으로 암시적 변환을 지원합니다.
    public static implicit operator SerializableSystemType(Type type) => new SerializableSystemType { Type = type };
}
