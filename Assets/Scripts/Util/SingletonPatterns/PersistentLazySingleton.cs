using UnityEngine;

/// <summary>
/// This is a lazy implementation of the Singleton Design Pattern.
/// An instance is created when someone calls the Instance property.
/// Additionally, with this implementation you have the same instance when moving from scene to scene.
/// </summary>
public class PersistentLazySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Flag used to mark singleton destruction
    private static bool singletonDestroyed = false;

    // Reference to our singular instance
    private static T instance;
    public static T Instance
    {
        get
        {
            if ( singletonDestroyed ) // If the app is closing and we already destroyed the instance, don't create a new one
            {
                Debug.LogWarningFormat ( "[Singleton] Singleton was already destroyed by quiting game. Returning null" );
                return null;
            }

            if ( !instance ) // If there is no object already, we should create new one
            {
                // Creating a new game object with singleton component
                // We don't need to assign reference here as Awake() will be called immediately after component is added
                new GameObject ( typeof ( T ).ToString () ).AddComponent<T> ();

                // And now we are making sure that object won't be destroy when we move to another scene
                DontDestroyOnLoad ( instance );
            }

            return instance;
        }
    }

    /// <summary>
    /// Unity method called just after object creation - like constructor.
    /// </summary>
    protected virtual void Awake ()
    {
        // If we don't have a reference to instance and we didn't destroy instance yet then this object will take control
        if ( instance == null && !singletonDestroyed )
        {
            instance = this as T;
        }
        else if ( instance != this ) // This is the other instance and we should destroy it
        {
            Destroy ( this );
        }
    }

    /// <summary>
    /// Unity method called before object destruction.
    /// </summary>
    protected virtual void OnDestroy ()
    {
        if ( instance != this ) // Skip if instance is other than this object
        {
            return;
        }

        singletonDestroyed = true;
        instance = null;
    }
}