using UnityEngine;
using InventorySystem.PlayerItems;

[CreateAssetMenu ( fileName = "New Pickup Instance Config", menuName = "Interactable/Pickup Instance Config" )]
public class PickupInstanceConfig : InteractableConfig
{
    [Header ( "Pickup Instance Config" )]
    public PlayerItem PlayerItem = null;
    public int Quantity = 1;
}