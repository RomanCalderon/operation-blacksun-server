using InventorySystem.PlayerItems;

public class WeaponPickupInstance : PickupInstance
{
    protected override void OnInteract ()
    {
        if ( !( m_playerItem is Weapon weapon ) )
        {
            return;
        }

        // Invoke pickup callback
        m_pickupCallback?.Invoke ();

        // Add PlayerItem to Inventory
        Server.clients [ ClientId ].player.InventoryManager.EquipWeapon ( weapon );

        // Destroy this instance
        Destroy ( gameObject );
    }
}
