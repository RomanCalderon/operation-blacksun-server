using System.Collections;
using UnityEngine;
using InventorySystem.PlayerItems;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A database that stores all PlayerItems for access.
/// </summary>
public class PlayerItemDatabase : MonoBehaviour
{
    [Header ( "Ammunition" ), SerializeField]
    private List<PlayerItem> m_ammunitionPlayerItems = new List<PlayerItem> ();
    [Header ( "Attachments" ), SerializeField]
    private List<PlayerItem> m_attachmentsPlayerItems = new List<PlayerItem> ();
    [Header ( "Equipment" ), SerializeField]
    private List<PlayerItem> m_equipmentPlayerItems = new List<PlayerItem> ();
    [Header ( "Weapons" ), SerializeField]
    private List<PlayerItem> m_weaponsPlayerItems = new List<PlayerItem> ();


    public PlayerItem GetPlayerItem ( string itemId )
    {
        // Check Ammunition list
        if ( m_ammunitionPlayerItems.Any ( p => p.Id == itemId ) )
        {
            return m_ammunitionPlayerItems.FirstOrDefault ( p => p.Id == itemId );
        }
        // Check Attachments list
        if ( m_attachmentsPlayerItems.Any ( p => p.Id == itemId ) )
        {
            return m_attachmentsPlayerItems.FirstOrDefault ( p => p.Id == itemId );
        }
        // Check Equipment list
        if ( m_equipmentPlayerItems.Any ( p => p.Id == itemId ) )
        {
            return m_equipmentPlayerItems.FirstOrDefault ( p => p.Id == itemId );
        }
        // Check Weapons list
        if ( m_weaponsPlayerItems.Any ( p => p.Id == itemId ) )
        {
            return m_weaponsPlayerItems.FirstOrDefault ( p => p.Id == itemId );
        }
        return null;
    }
}
