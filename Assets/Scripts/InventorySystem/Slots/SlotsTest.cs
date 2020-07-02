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
    [SerializeField]
    private Barrel m_barrel = null;

    private void Start ()
    {

        m_attachmentSlot = new StockSlot ();
        Debug.Log ( m_attachmentSlot );
        Debug.Log ( $"Attempting insert {m_attachment} into StockSlot: {m_attachmentSlot.Insert ( m_attachment )} | {m_attachmentSlot}" );
        Debug.Log ( $"Attempting remove {m_attachment} from StockSlot: {m_attachmentSlot.Remove ()} | {m_attachmentSlot}" );
        Debug.Log ( $"Attempting insert {m_playerItem} into StockSlot: {m_attachmentSlot.Insert ( m_playerItem )} | {m_attachmentSlot}" );
        Debug.Log ( $"Attempting insert {m_barrel} into StockSlot: {m_attachmentSlot.Insert ( m_barrel )} | {m_attachmentSlot}" );


        m_weaponSlot = new WeaponSlot ();
        Debug.Log ( m_weaponSlot );
        Debug.Log ( $"Attempting insert {m_playerItem} into WeaponSlot: {m_weaponSlot.Insert ( m_playerItem )} | {m_weaponSlot}" );
        Debug.Log ( $"Attempting insert {m_weapon} into WeaponSlot: {m_weaponSlot.Insert ( m_weapon )} | {m_weaponSlot}" );


        m_slot = new Slot ();
        Debug.Log ( m_slot );
        Debug.Log ( $"Attempting insert {m_attachment} into Slot: {m_slot.Insert ( m_attachment )} | {m_slot}" );
        Debug.Log ( $"Attempting remove {m_attachment} from Slot: {m_slot.Remove ()} | {m_slot}" );
        Debug.Log ( $"Attempting insert {m_weapon} into Slot: {m_slot.Insert ( m_weapon )} | {m_slot}" );
        Debug.Log ( $"Attempting insert {m_playerItem} into Slot: {m_slot.Insert ( m_playerItem )} | {m_slot}" );

    }
}
