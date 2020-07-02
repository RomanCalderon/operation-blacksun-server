using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using InventorySystem.Slots;
using InventorySystem.PlayerItems;

public class SlotsTest : MonoBehaviour
{
    private WeaponSlot m_weaponSlot = null;
    private AttachmentSlot m_attachmentSlot = null;
    private Slot m_slot = null;

    [SerializeField]
    private Weapon m_weapon = null;
    [SerializeField]
    private Attachment m_attachment = null;
    [SerializeField]
    private PlayerItem m_playerItem = null;

    private void Start ()
    {
        m_attachmentSlot = new AttachmentSlot ();
        Debug.Log ( m_attachmentSlot );
        Debug.Log ( $"Attempting insert Attachment into AttachmentSlot: {m_attachmentSlot.Insert ( m_attachment )} | {m_attachmentSlot}" );
        Debug.Log ( $"Attempting remove Attachment from AttachmentSlot: {m_attachmentSlot.Remove ()} | {m_attachmentSlot}" );


        /*
        m_weaponSlot = new WeaponSlot ();
        Debug.Log ( m_weaponSlot );
        Debug.Log ( $"Attempting insert PlayerItem into weapon slot: {m_weaponSlot.Insert ( m_playerItem )} | {m_weaponSlot}" );

        m_slot = new Slot ();
        Debug.Log ( m_slot );
        Debug.Log ( $"Attempting insert Weapon into slot: {m_slot.Insert ( m_weapon )} | {m_slot}" );
        */
    }
}
