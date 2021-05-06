using System.IO;
using UnityEngine;
using InteractionData;

public class BinaryReaderExtended : BinaryReader
{
    public BinaryReaderExtended ( MemoryStream stream ) : base ( stream ) { }

    public Vector3 ReadVector3 ()
    {
        float x = ReadSingle ();
        float y = ReadSingle ();
        float z = ReadSingle ();
        return new Vector3 ( x, y, z );
    }

    public Color ReadColor ()
    {
        byte r = ReadByte ();
        byte g = ReadByte ();
        byte b = ReadByte ();
        byte a = ReadByte ();
        return ColorVector.ToColor ( r, g, b, a );
    }
}
