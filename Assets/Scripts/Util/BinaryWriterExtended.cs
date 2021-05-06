using System.IO;
using UnityEngine;
using InteractionData;

public class BinaryWriterExtended : BinaryWriter
{
    public BinaryWriterExtended ( MemoryStream stream ) : base ( stream ) { }

    public void Write ( Vector3 value )
    {
        Write ( value.x );
        Write ( value.y );
        Write ( value.z );
    }

    public void Write ( Color value )
    {
        ColorVector colorVector = new ColorVector ( value );
        Write ( colorVector.r );
        Write ( colorVector.g );
        Write ( colorVector.b );
        Write ( colorVector.a );
    }
}
