using System.IO;
using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    #region Models

    [System.Serializable]
    public struct InteractableData
    {
        public bool IsInteractable;
        public string AccessKey;

        public InteractableData ( bool isInteractable, string accessKey )
        {
            IsInteractable = isInteractable;
            AccessKey = accessKey;
        }

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriter writer = new BinaryWriter ( stream );

            writer.Write ( IsInteractable );
            writer.Write ( AccessKey );

            return stream.ToArray ();
        }

        public static InteractableData FromArray ( byte [] bytes )
        {
            BinaryReader reader = new BinaryReader ( new MemoryStream ( bytes ) );
            InteractableData s = default;

            s.IsInteractable = reader.ReadBoolean ();
            s.AccessKey = reader.ReadString ();

            return s;
        }
    }

    #endregion

    public bool IsInteractable { get; set; } = false;
    public bool IsInteracting { get => m_isInteracting; }
    public int ClientId { get => m_clientId; }
    public string AccessKey { get; set; }
    public float InteractTimer { get => m_interactTimer; }

    private const float m_interactTimeThreshold = 0.5f;
    private int m_clientId = 0;
    private bool m_isInteracting = false;
    private bool m_hasInteracted = false;
    private float m_interactTimer = 0f;

    #region Interface

    /// <summary>
    /// Initializes Interactable with an optional <paramref name="isInteractable"/> flag and <paramref name="accessKey"/>.
    /// </summary>
    /// <param name="isInteractable">Is this Interactable interactable? Default is true.</param>
    /// <param name="accessKey">A prerequisite 'passcode' in the form of a string. Omitting a value (null) disables this prerequisite. Default is null.</param>
    public virtual void Initialize ( bool isInteractable = true, string accessKey = null )
    {
        IsInteractable = isInteractable;
        AccessKey = accessKey;
    }

    /// <summary>
    /// Called when Interactable becomes accessible.
    /// </summary>
    public abstract void StartHover ();

    /// <summary>
    /// Called when Interactable begins interaction.
    /// </summary>
    /// <param name="clientId">The ID of the client interacting with Interactable.</param>
    /// <param name="accessKey">An access key used to compare against Interactable's AccessKey.</param>
    public virtual void StartInteract ( int clientId, string accessKey = null )
    {
        if ( !IsInteractable )
        {
            return;
        }
        if ( m_isInteracting )
        {
            StopInteract ();
            return;
        }
        if ( !string.IsNullOrEmpty ( AccessKey ) && accessKey != AccessKey )
        {
            StopInteract ();
            return;
        }
        m_clientId = clientId;
        m_isInteracting = true;
        m_interactTimer = m_interactTimeThreshold;
    }

    /// <summary>
    /// Called when Interactable begins interaction.
    /// </summary>
    /// <param name="clientId">The ID of the client interacting with Interactable.</param>
    /// <param name="accessKeys">An array of access keys used to compare against Interactable's AccessKey.</param>
    public virtual void StartInteract ( int clientId, string [] accessKeys )
    {
        if ( m_isInteracting || accessKeys == null )
        {
            StopInteract ();
            return;
        }
        if ( !string.IsNullOrEmpty ( AccessKey ) )
        {
            foreach ( string accessKey in accessKeys )
            {
                if ( accessKey.Equals ( AccessKey ) )
                {
                    m_clientId = clientId;
                    m_isInteracting = true;
                    m_interactTimer = m_interactTimeThreshold;
                    return;
                }
            }
            StopInteract ();
        }
    }

    /// <summary>
    /// Called when Interactable interaction timer is complete.
    /// </summary>
    protected abstract void OnInteract ();

    /// <summary>
    /// Called when Interactable interaction has ended or got interrupted.
    /// </summary>
    public virtual void StopInteract ()
    {
        m_isInteracting = m_hasInteracted = false;
        m_interactTimer = 0f;
    }

    /// <summary>
    /// Called when Interactable becomes inaccessible.
    /// </summary>
    public abstract void StopHover ();

    #endregion

    #region Runtime

    private void Update ()
    {
        CheckInteractTime ();
    }

    private void CheckInteractTime ()
    {
        if ( m_isInteracting )
        {
            if ( m_hasInteracted )
            {
                return;
            }
            // Increment interact timer
            if ( m_interactTimer > 0f )
            {
                m_interactTimer -= Time.deltaTime;
                return;
            }
            // Interaction timer is complete
            OnInteract ();
            m_hasInteracted = true;
        }
    }

    #endregion

    #region Accessors

    public byte [] GetData ()
    {
        return new InteractableData ( IsInteractable, AccessKey ).ToArray ();
    }

    #endregion
}
