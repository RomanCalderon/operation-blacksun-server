using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attachment", menuName = "ScriptableObjects/PlayerItems/Attachment", order = 2)]
class Attachment : PlayerItem
{
    public enum AttachmentTypes
    {
        Sight,
        Stock
    }

    public AttachmentTypes AttachmentType;
}
