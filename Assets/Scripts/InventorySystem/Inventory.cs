using System;
using UnityEngine;
using InventorySystem.Presets;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;
using InventorySystem.PlayerItems;
using System.Linq;

namespace InventorySystem
{
    [Serializable]
    public class Inventory
    {
        // Rig
        [SerializeField]
        private Slot [] m_rigSlots;

        // Backpack
        [SerializeField]
        private Slot [] m_backpackSlots;

        // Primary weapon and attachment slots
        [SerializeField]
        private WeaponSlot m_primaryWeaponSlot;
        [SerializeField]
        private BarrelSlot m_primaryBarrelSlot;
        [SerializeField]
        private SightSlot m_primarySightSlot;
        [SerializeField]
        private MagazineSlot m_primaryMagazineSlot;
        [SerializeField]
        private StockSlot m_primaryStockSlot;

        // Secondary weapon and attachment slots
        [SerializeField]
        private WeaponSlot m_secondaryWeaponSlot;
        [SerializeField]
        private BarrelSlot m_secondaryBarrelSlot;
        [SerializeField]
        private SightSlot m_secondarySightSlot;
        [SerializeField]
        private MagazineSlot m_secondaryMagazineSlot;
        [SerializeField]
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

            OnValidate ();
        }

        public Inventory ( Preset preset )
        {
            if ( preset == null )
            {
                return;
            }

            // Initialize Rig slots
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                m_rigSlots [ i ] = new Slot ( preset.RigItems [ i ] );
            }

            // Initialize Backpack slots
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                m_backpackSlots [ i ] = new Slot ();
            }
            // Add PlayerItems to the backpack
            foreach ( PlayerItemPreset playerItemPreset in preset.BackpackItems )
            {
                AddToBackpack ( playerItemPreset.PlayerItem, playerItemPreset.Quantity );
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

            OnValidate ();
        }

        #endregion

        #region Slot access

        #region Rig

        /// <summary>
        /// Add one instance of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a rig slot.</param>
        /// <param name="slotIndex">The rig slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToRig ( PlayerItem playerItem, int slotIndex )
        {
            return AddToRig ( playerItem, 1, slotIndex );
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a rig slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The rig slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToRig ( PlayerItem playerItem, int quantity, int slotIndex )
        {
            if ( slotIndex < 0 || slotIndex >= m_rigSlots.Length || playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = m_rigSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

            return slot.Insert ( playerItem, quantity );
        }
        
        /// <summary>
        /// Get the slot at <paramref name="index"/> in the rig.
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>The Slot at <paramref name="index"/> in the rig.</returns>
        public Slot GetFromRig ( int index )
        {
            index = Mathf.Clamp ( index, 0, m_rigSlots.Length - 1 );
            return m_rigSlots [ index ];
        }

        #endregion

        #region Backpack

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into any available backpack slot or slots.
        /// </summary>
        /// <param name="playerItem">The PlayerItem being added to the backpack.</param>
        /// <param name="quantity">The amount of items being added.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpackAny ( PlayerItem playerItem, int quantity )
        {
            if ( playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( null, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = null;
            InsertionResult insertionResult = null;
            do
            {
                slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

                if ( slot != null )
                {
                    // Insert
                    insertionResult = slot.Insert ( playerItem, quantity );

                    // Update quantity
                    quantity = insertionResult.OverflowAmount;

                }
            } while ( slot != null && insertionResult.Result == InsertionResult.Results.OVERFLOW );

            return insertionResult;
        }

        /// <summary>
        /// Add one instance of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a backpack slot.</param>
        /// <param name="slotIndex">The backpack slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int slotIndex )
        {
            return AddToBackpack ( playerItem, 1, slotIndex );
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a backpack slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The backpack slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int quantity, int slotIndex )
        {
            if ( slotIndex < 0 || slotIndex >= m_rigSlots.Length || playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

            return slot.Insert ( playerItem, quantity );
        }

        /// <summary>
        /// Get the slot at <paramref name="index"/> in the backpack.
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>The Slot at <paramref name="index"/> in the backpack.</returns>
        public Slot GetFromBackpack ( int index )
        {
            index = Mathf.Clamp ( index, 0, m_rigSlots.Length - 1 );
            return m_backpackSlots [ index ];
        }

        #endregion

        #endregion

        /// <summary>
        /// Updates the inspector when a change to the inventory is made.
        /// </summary>
        public void OnValidate ()
        {
            // Rig slots
            foreach ( Slot slot in m_rigSlots )
            {
                if ( slot != null )
                {
                    slot.OnValidate ();
                }
            }

            // Backpack slots
            foreach ( Slot slot in m_backpackSlots )
            {
                if ( slot != null )
                {
                    slot.OnValidate ();
                }
            }

            // Primary weapon and attachment slots
            if ( m_primaryWeaponSlot != null )
            {
                m_primaryWeaponSlot.OnValidate ();
            }
            if ( m_primaryBarrelSlot != null )
            {
                m_primaryBarrelSlot.OnValidate ();
            }
            if ( m_primarySightSlot != null )
            {
                m_primarySightSlot.OnValidate ();
            }
            if ( m_primaryMagazineSlot != null )
            {
                m_primaryMagazineSlot.OnValidate ();
            }
            if ( m_primaryStockSlot != null )
            {
                m_primaryStockSlot.OnValidate ();
            }

            // Secondary weapon and attachment slots
            if ( m_secondaryWeaponSlot != null )
            {
                m_secondaryWeaponSlot.OnValidate ();
            }
            if ( m_secondaryBarrelSlot != null )
            {
                m_secondaryBarrelSlot.OnValidate ();
            }
            if ( m_secondarySightSlot != null )
            {
                m_secondarySightSlot.OnValidate ();
            }
            if ( m_secondaryMagazineSlot != null )
            {
                m_secondaryMagazineSlot.OnValidate ();
            }
            if ( m_secondaryStockSlot != null )
            {
                m_secondaryStockSlot.OnValidate ();
            }
        }
    }
}
