using System;
using System.IO;
using UnityEngine;

namespace InteractionData
{
    #region Models

    [Serializable]
    public struct InteractableData
    {
        public string InstanceId;
        public int InteractionType;
        public bool IsInteractable;
        public string InteractionLabel;
        public Color InteractionLabelColor;
        public float InteractTime;
        public bool HasAccessKey;

        public InteractableData ( string instanceId, int interactionType, bool isInteractable, string interactionLabel, Color interactionLabelColor, float interactTime, bool hasAccessKey )
        {
            InstanceId = instanceId;
            InteractionType = interactionType;
            IsInteractable = isInteractable;
            InteractionLabel = interactionLabel;
            InteractionLabelColor = interactionLabelColor;
            InteractTime = interactTime;
            HasAccessKey = hasAccessKey;
        }

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriterExtended writer = new BinaryWriterExtended ( stream );

            writer.Write ( InstanceId );
            writer.Write ( InteractionType );
            writer.Write ( IsInteractable );
            writer.Write ( InteractionLabel );
            writer.Write ( InteractionLabelColor );
            writer.Write ( InteractTime );
            writer.Write ( HasAccessKey );

            return stream.ToArray ();
        }

        public static InteractableData FromArray ( byte [] bytes )
        {
            BinaryReaderExtended reader = new BinaryReaderExtended ( new MemoryStream ( bytes ) );
            InteractableData s = default;

            s.InstanceId = reader.ReadString ();
            s.InteractionType = reader.ReadInt32 ();
            s.IsInteractable = reader.ReadBoolean ();
            s.InteractionLabel = reader.ReadString ();
            s.InteractionLabelColor = reader.ReadColor ();
            s.InteractTime = reader.ReadSingle ();
            s.HasAccessKey = reader.ReadBoolean ();

            return s;
        }
    }

    #endregion

    #region User-defined Types

    /// <summary>
    /// A more compact storage of Color data using unsigned bytes to represent each component (r, g, b, a).
    /// </summary>
    public struct ColorVector
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public ColorVector ( byte r, byte g, byte b, byte a )
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public ColorVector ( Color color )
        {
            r = ( byte ) ( color.r * byte.MaxValue );
            g = ( byte ) ( color.g * byte.MaxValue );
            b = ( byte ) ( color.b * byte.MaxValue );
            a = ( byte ) ( color.a * byte.MaxValue );
        }

        /// <summary>
        /// Converts a ColorVector to a Color struct.
        /// </summary>
        /// <param name="colorVector"></param>
        public static Color ToColor ( ColorVector colorVector )
        {
            float r = colorVector.r / byte.MaxValue;
            float g = colorVector.g / byte.MaxValue;
            float b = colorVector.b / byte.MaxValue;
            float a = colorVector.a / byte.MaxValue;
            return new Color ( r, g, b, a );
        }

        /// <summary>
        /// Converts a ColorVector to a Color struct.
        /// </summary>
        /// <param name="colorVector"></param>
        public static Color ToColor ( byte r, byte g, byte b, byte a )
        {
            return new Color ( r / byte.MaxValue, g / byte.MaxValue, b / byte.MaxValue, a / byte.MaxValue );
        }

        public override string ToString ()
        {
            return $"ColorVector ( {r}, {g}, {b}, {a} )";
        }
    }

    #endregion
}
