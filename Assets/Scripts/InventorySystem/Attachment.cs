using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A PlayerItem which defines the properties of an attachment.
/// </summary>
[CreateAssetMenu(fileName = "New Attachment", menuName = "ScriptableObjects/PlayerItems/Attachment", order = 2)]
class Attachment : PlayerItem
{
    public enum AttachmentTypes
    {
        Sight,
        Stock
    }

    /// <summary>
    /// The type of attachment.
    /// </summary>
    public AttachmentTypes AttachmentType;
}
