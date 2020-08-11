using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableObjectsManager : MonoBehaviour
{
    public static ShootableObjectsManager Instance;

    #region Models

    public enum HitObjects
    {
        Asphalt,
        Bricks,
        Concrete,
        Glass,
        Ground,
        Metal1,
        Metal2,
        Mud,
        Plywood,
        Rock,
        Sand,
        Skin,
        Tiles,
        Water,
        Wood,
        Debug
    }

    #endregion


    private void Awake ()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning ( "There is more than one ShootableObjectsManager. There can only be one." );
        }
    }

    /// <summary>
    /// Spawns a projectile hit/impact object based on the RaycastHit parameters.
    /// </summary>
    /// <param name="hit">The RaycastHit containing the hit information.</param>
    /// <param name="projectileDamage">The amount of damage received from the projectile.</param>
    public void ProjectileHit ( RaycastHit hit, float projectileDamage )
    {
        switch ( hit.collider.tag )
        {
            case "Asphalt":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Asphalt, hit.point, hit.normal );
                break;

            case "Bricks":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Bricks, hit.point, hit.normal );
                break;

            case "Concrete":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Concrete, hit.point, hit.normal );
                break;

            case "Glass":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Glass, hit.point, hit.normal );
                break;

            case "Ground":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Ground, hit.point, hit.normal );
                break;

            case "Metal":
                if ( projectileDamage < 60 )
                {
                    ServerSend.SpawnHitObject ( ( int ) HitObjects.Metal1, hit.point, hit.normal );
                }
                else
                {
                    ServerSend.SpawnHitObject ( ( int ) HitObjects.Metal2, hit.point, hit.normal );
                }
                break;

            case "Mud":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Mud, hit.point, hit.normal );
                break;

            case "Plywood":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Plywood, hit.point, hit.normal );
                break;

            case "Rock":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Rock, hit.point, hit.normal );
                break;

            case "Sand":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Sand, hit.point, hit.normal );
                break;

            case "Tiles":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Tiles, hit.point, hit.normal );
                break;

            case "Water":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Water, hit.point, hit.normal );
                break;

            case "Wood":
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Wood, hit.point, hit.normal );
                break;

            default: // Debug hit object (pink sphere)
                ServerSend.SpawnHitObject ( ( int ) HitObjects.Debug, hit.point, hit.normal );
                break;
        }
    }
}
