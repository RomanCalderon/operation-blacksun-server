using System;
using System.Linq;
using UnityEngine;
using InteractionData;

public class Interactable : MonoBehaviour, IInteractable
{
    #region Members

    public string InstanceId { get => m_instanceId; }
    public int InteractionType { get => m_interactionType; }
    public bool IsInteractable { get => m_isInteractable; }
    public string InteractionContext { get => m_interactionContext; }
    public Color InteractionColor { get => m_interactionLabelColor; }
    public bool IsInteracting { get => m_isInteracting; }
    public float InteractTime { get => m_interactTime; }
    public string AccessKey { get => m_accessKey; }
    public int ClientId { get => m_clientId; }

    private string m_instanceId = null;
    private int m_interactionType = 0;
    private bool m_isInteractable = false;
    private string m_interactionContext;
    private Color m_interactionLabelColor;
    private bool m_isInteracting = false;
    private float m_interactTime = 0f;
    private string m_accessKey = null;
    private int m_clientId = 0;
    private bool m_hasInteracted = false;
    private float m_interactTimer = 0f;

    #endregion

    #region Interface

    /// <summary>
    /// Initializes Interactable with an optional <paramref name="isInteractable"/> flag and <paramref name="accessKey"/>.
    /// </summary>
    /// <param name="isInteractable">Is this Interactable interactable? Default is true.</param>
    /// <param name="accessKey">A prerequisite 'passcode' in the form of a string. Omitting a value (null) disables this prerequisite. Default is null.</param>
    public virtual void Initialize ( int interactionType, bool isInteractable, string interactionContext, Color interactionLabelColor, float interactTime = 0f, string accessKey = null )
    {
        m_instanceId = Guid.NewGuid ().ToString ();
        m_interactionType = interactionType;
        m_isInteractable = isInteractable;
        m_interactionContext = interactionContext;
        m_interactionLabelColor = interactionLabelColor;
        m_interactTime = interactTime;
        m_accessKey = accessKey;
    }

    /// <summary>
    /// Called when Interactable becomes accessible.
    /// </summary>
    public virtual void StartHover () { }

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
        m_interactTimer = InteractTime;
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
                    m_interactTimer = InteractTime;
                    return;
                }
            }
            StopInteract ();
        }
    }

    /// <summary>
    /// Called when Interactable interaction timer is complete.
    /// </summary>
    protected virtual void OnInteract () { }

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
    public virtual void StopHover () { }

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

    public byte [] GetData ( string [] accessKeys )
    {
        return new InteractableData (
            m_instanceId,
            m_interactionType,
            m_isInteractable,
            m_interactionContext,
            m_interactionLabelColor,
            m_interactTime,
            HasAccessKey ( accessKeys )
            ).ToArray ();
    }

    #endregion

    #region Util

    private bool HasAccessKey ( string [] accessKeys )
    {
        if ( string.IsNullOrEmpty ( m_accessKey ) )
        {
            return true;
        }
        return accessKeys.Contains ( m_accessKey );
    }

    #endregion
}
