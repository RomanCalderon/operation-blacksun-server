using InventorySystem.Presets;
using InventorySystem.Slots;
using InventorySystem.PlayerItems;

namespace InventorySystem
{
    public class Inventory
    {
        // Rig
        private Slot [] m_rigSlots;

        // Backpack
        private Slot [] m_backpackSlots;

        // Primary weapon and attachment slots
        private WeaponSlot m_primaryWeaponSlot;
        private BarrelSlot m_primaryBarrelSlot;
        private SightSlot m_primarySightSlot;
        private MagazineSlot m_primaryMagazineSlot;
        private StockSlot m_primaryStockSlot;

        // Secondary weapon and attachment slots
        private WeaponSlot m_secondaryWeaponSlot;
        private BarrelSlot m_secondaryBarrelSlot;
        private SightSlot m_secondarySightSlot;
        private MagazineSlot m_secondaryMagazineSlot;
        private StockSlot m_secondaryStockSlot;

        #region Constructors

        public Inventory ()
        {
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];

            m_primaryWeaponSlot = null;
            m_primaryBarrelSlot = null;
            m_primarySightSlot = null;
            m_primaryMagazineSlot = null;
            m_primaryStockSlot = null;

            m_secondaryWeaponSlot = null;
            m_secondaryBarrelSlot = null;
            m_secondarySightSlot = null;
            m_secondaryMagazineSlot = null;
            m_secondaryStockSlot = null;
        }

        public Inventory ( Preset preset )
        {
            // Rig
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                m_rigSlots [ i ].Insert ( preset.RigItems [ i ] );
            }

            // Backpack
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];
            foreach ( PlayerItemPreset playerItemPreset in preset.BackpackItems )
            {
                AddItemToBackpack ( playerItemPreset.PlayerItem, playerItemPreset.Quantity );
            }

            // Primary weapon
            m_primaryWeaponSlot = new WeaponSlot ( preset.PrimaryWeapon.Weapon );
            m_primaryBarrelSlot = new BarrelSlot ( preset.PrimaryWeapon.Barrel );
            m_primarySightSlot = new SightSlot ( preset.PrimaryWeapon.Sight );
            m_primaryMagazineSlot = new MagazineSlot ( preset.PrimaryWeapon.Magazine );
            m_primaryStockSlot = new StockSlot ( preset.PrimaryWeapon.Stock );

            // Secondary weapon
            m_secondaryWeaponSlot = new WeaponSlot ( preset.SecondaryWeapon.Weapon );
            m_secondaryBarrelSlot = new BarrelSlot ( preset.SecondaryWeapon.Barrel );
            m_secondarySightSlot = new SightSlot ( preset.SecondaryWeapon.Sight );
            m_secondaryMagazineSlot = new MagazineSlot ( preset.SecondaryWeapon.Magazine );
            m_secondaryStockSlot = new StockSlot ( preset.SecondaryWeapon.Stock );
        }

        #endregion

        #region Backpack

        public Slot.InsertionResult AddItemToBackpack ( PlayerItem playerItem, int quantity )
        {
            Slot.InsertionResult insertionResult;

            do
            {
                insertionResult = AddToBackpack ( playerItem, quantity );
            } while ( insertionResult.Result == Slot.InsertionResult.Results.OVERFLOW );

            return insertionResult;
        }

        private Slot.InsertionResult AddToBackpack ( PlayerItem playerItem, int quantity )
        {
            // Check each slot
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                // Add to an available or empty slot
                if ( m_backpackSlots [ i ].IsSlotAvailable ( playerItem ) || m_backpackSlots [ i ].IsEmpty () )
                {
                    return m_backpackSlots [ i ].Insert ( playerItem, quantity );
                }
            }

            // No slots available
            return new Slot.InsertionResult ( Slot.InsertionResult.Results.INSERTION_FAILED );
        }

        #endregion
    }
}
