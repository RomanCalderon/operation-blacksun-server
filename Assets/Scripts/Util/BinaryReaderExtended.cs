using System.IO;
using UnityEngine;

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
}
