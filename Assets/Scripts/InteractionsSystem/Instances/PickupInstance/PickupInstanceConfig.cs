using UnityEngine;
using InventorySystem.PlayerItems;

namespace InteractableConfiguration
{
    [CreateAssetMenu ( fileName = "New Pickup Instance Config", menuName = "Interactable/Pickup Instance Config" )]
    public class PickupInstanceConfig : InteractableConfig
    {
        public override int InteractionType { get => ( int ) InteractionTypes.STANDARD; }
        public override PlayerItem PlayerItem { get => m_playerItem; protected set => m_playerItem = value; }
        public override int Quantity { get => m_quantity; protected set => m_quantity = value; }

        [SerializeField]
        protected PlayerItem m_playerItem = null;

        [Header ( "Pickup Instance Config" )]
        protected int m_quantity = 1;

        public void Init ( PlayerItem playerItem, int quantity = 1 )
        {
            m_playerItem = playerItem;
            m_quantity = quantity;
        }
    }
}