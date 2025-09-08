using Tomat.Hacksaw.Metadata.Image.Pooling;

namespace Tomat.Hacksaw.Metadata.Image;

#region Type parameters
public readonly record struct ImageTypeFunction(
    TypeHandle[] ArgumentTypes,
    TypeHandle ReturnType
)
{
    /*public bool Equals(ImageTypeFunction? other)
    {
        if (!other.HasValue)
        {
            return false;
        }

        return ReturnType == other.Value.ReturnType && ArgumentTypes.SequenceEqual(other.Value.ArgumentTypes);
    }

    public override int GetHashCode()
    {
        var hash = ReturnType.GetHashCode();
        return ArgumentTypes.Aggregate(hash, HashCode.Combine);
    }*/
}

public readonly record struct ImageTypeObjectField(
    StringHandle Name,
    TypeHandle Type,
    int Index
);

public readonly record struct ImageTypeObjectPrototype(
    StringHandle Name,
    int FunctionIndex,
    int PrototypeIndex
);

public readonly record struct ImageTypeObject(
    StringHandle Name,
    TypeHandle? Super,
    ImageTypeObjectField[] Fields,
    ImageTypeObjectPrototype[] Prototypes,
    ImageTypeObject.BindingData[] Bindings,
    int GlobalValue
)
{
    public readonly record struct BindingData(int FieldIndex, int FunctionIndex);
}

public readonly record struct ImageTypeEnumConstruct(
    StringHandle Name,
    TypeHandle[] Parameters,
    int[] Offsets
);

public readonly record struct ImageTypeEnum(
    StringHandle Name,
    ImageTypeEnumConstruct[] Constructs,
    int GlobalValue
)
{
    /*public bool Equals(ImageTypeEnum? other)
    {
        if (!other.HasValue)
        {
            return false;
        }

        return Name == other.Value.Name && GlobalValue == other.Value.GlobalValue && Constructs.SequenceEqual(other.Value.Constructs);
    }

    public override int GetHashCode()
    {
        var hash = Name.GetHashCode();
        hash = HashCode.Combine(hash, GlobalValue);

        return Constructs.Aggregate(hash, HashCode.Combine);
    }*/
}

public readonly record struct ImageTypeVirtual(
    ImageTypeObjectField[] Fields
)
{
    /*public bool Equals(ImageTypeVirtual? other)
    {
        if (!other.HasValue)
        {
            return false;
        }

        return Fields.SequenceEqual(other.Value.Fields);
    }

    public override int GetHashCode()
    {
        return Fields.Aggregate(0, HashCode.Combine);
    }*/
}
#endregion

public abstract record ImageType(HlTypeKind Kind)
{
    public sealed record Simple(HlTypeKind Kind) : ImageType(Kind);

    public sealed record WithAbstractName(HlTypeKind Kind, StringHandle AbstractName) : ImageType(Kind);

    public sealed record WithFunction(HlTypeKind Kind, ImageTypeFunction Function) : ImageType(Kind);

    public sealed record WithObject(HlTypeKind Kind, ImageTypeObject Object) : ImageType(Kind);

    public sealed record WithEnum(HlTypeKind Kind, ImageTypeEnum Enum) : ImageType(Kind);

    public sealed record WithVirtual(HlTypeKind Kind, ImageTypeVirtual Virtual) : ImageType(Kind);

    public sealed record WithType(HlTypeKind Kind, TypeHandle Type) : ImageType(Kind);
}