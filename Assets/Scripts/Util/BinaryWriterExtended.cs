using System.IO;
using UnityEngine;

public class BinaryWriterExtended : BinaryWriter
{
    public BinaryWriterExtended ( MemoryStream stream ) : base ( stream ) { }

    public void Write ( Vector3 value )
    {
        Write ( value.x );
        Write ( value.y );
        Write ( value.z );
    }
}
