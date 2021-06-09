using InventorySystem.PlayerItems;

public class AttachmentPickupInstance : PickupInstance
{
    protected override void OnInteract ()
    {
        if ( !( m_playerItem is Attachment attachment ) )
        {
            return;
        }

        // Invoke pickup callback
        m_pickupCallback?.Invoke ();

        // Add PlayerItem to Inventory
        Server.clients [ ClientId ].player.InventoryManager.EquipAttachment ( attachment );

        // Destroy this instance
        Destroy ( gameObject );
    }
}
