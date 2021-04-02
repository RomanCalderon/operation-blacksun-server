using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

public class PlayerItemInstance : MonoBehaviour
{
    public virtual PlayerItem PlayerItem
    {
        get
        {
            return m_playerItem;
        }
    }
    [SerializeField]
    protected PlayerItem m_playerItem = null;

    public void SetActive ( bool value )
    {
        gameObject.SetActive ( value );
    }

    public override bool Equals ( object obj )
    {
        return obj is PlayerItemInstance instance && m_playerItem == instance.m_playerItem;
    }
}
