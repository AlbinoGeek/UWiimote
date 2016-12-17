using UnityEngine;

public class Interact : MonoBehaviour
{
    public bool WantsToGrab = false;
    public GrabState State = GrabState.Ready;

    public Transform Target = null;

    public enum GrabState
    {
        Ready, // -> Grabbing (start)
        Grabbing, // Grabbing
        // -> Ready (end)
    }

    private bool hasValidTarget = false;
    private GameObject currentTarget = null;
    private Collider currentTargetCollider = null;
    private Rigidbody currentTargetRigidbody = null;

    /// <summary>
    /// holds properties such as the offset and direction of rotation
    /// </summary>
    private Interactable currentTargetInteractable = null;

    private bool targetColliderWasEnabled = false;

    private int interactLayer;
    private int noraycastLayer;
    private MeshRenderer meshRenderer;

    #region Unity
    /// <summary>
    /// obtains references to used components and layers
    /// </summary>
    private void Awake()
    {
        this.meshRenderer = this.GetComponent<MeshRenderer>();
        this.interactLayer = LayerMask.NameToLayer("Interactable");
        this.noraycastLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        // Used to find a target under the cursor
        InvokeRepeating("FindTarget", .1f, .1f);
    }

    /// <summary>
    /// case to switch between grab action functions
    /// </summary>
    private void Update()
    {
        this.meshRenderer.enabled = this.hasValidTarget && !this.WantsToGrab;

        if (this.WantsToGrab && this.hasValidTarget && this.State == GrabState.Ready)
        {
            // Pause target finding
            this.CancelInvoke("FindTarget");

            // Start of a grab
            this.StartDrag();
        }
        else if (this.hasValidTarget && this.WantsToGrab && this.State == GrabState.Grabbing)
        {
            // Continue a grab
            this.ContinueDrag();
        }
        else if (!this.WantsToGrab && this.State == GrabState.Grabbing)
        {
            // Ending a grab
            this.StopDrag();

            // Restart target finding
            InvokeRepeating("FindTarget", .1f, .1f);
        }
        else
        {
            // Not Grabbing
            if (this.hasValidTarget)
            {
            //    RaycastHit raycastHit;
            //    Ray ray = new Ray(this.Target.position, this.currentTarget.transform.position - this.Target.position);
            //    if (Physics.Raycast(ray, out raycastHit, 3f, this.interactLayer))
            //    {
            //        this.transform.position = raycastHit.point;
            //    }
            }

            // Don't move our cursor when we're not grabbing
            return;
        }

        // If we have a parent object; hold to forward
        if (this.Target != null)
        {
            // Move ourselves to the camera forward
            this.transform.position = this.Target.position + Camera.main.transform.TransformDirection(Vector3.forward * 1f);
        }
    }
    #endregion

    /// <summary>
    /// starts a new drag, must already have a valid target.
    /// </summary>
    private void StartDrag()
    {
        this.State = GrabState.Grabbing;

        // make sure we don't raycast ourselves
        this.currentTarget.layer = this.noraycastLayer;

        // obtain collider and rigidbody if any from target; while dragging:
        // - disables collider
        // - turns off gravity
        this.currentTargetInteractable = this.currentTarget.GetComponent<Interactable>();

        this.currentTargetCollider = this.currentTarget.GetComponent<Collider>();
        if (this.currentTargetCollider)
        {
            this.targetColliderWasEnabled = this.currentTargetCollider.enabled;
            this.currentTargetCollider.enabled = false;
        }
        this.currentTargetRigidbody = this.currentTarget.GetComponent<Rigidbody>();
        if (this.currentTargetRigidbody)
        {
            this.currentTargetRigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// while dragging, move the \ref this.currentTarget to match our \ref this.transform
    /// </summary>
    private void ContinueDrag()
    {
        Vector3 offset = Vector3.zero;
        if (this.currentTargetInteractable)
        {
            offset = this.currentTargetInteractable.GrabOffset;
        }

        this.currentTarget.transform.position = -offset + this.transform.position;
        this.currentTarget.transform.rotation = this.transform.rotation;
    }

    /// <summary>
    /// stops a drag action, freeing our current target
    /// </summary>
    private void StopDrag()
    {
        this.State = GrabState.Ready;

        // make sure we can raycast the object again
        this.currentTarget.layer = this.interactLayer;

        // if there was a collider, and it was enabled, re-nable it
        // if there was a rigidbody, and it was using gravity, re-enable it
        if (this.currentTargetCollider && this.targetColliderWasEnabled)
        {
            this.currentTargetCollider.enabled = true;
        }

        if (this.currentTargetRigidbody)
        {
            this.currentTargetRigidbody.isKinematic = false;
        }

        // Free our target
        this.currentTarget = null;
        this.currentTargetCollider = null;
        this.currentTargetRigidbody = null;
    }

    private void FindTarget()
    {
        // Look for objects overlapping ourselves
        Collider[] col = Physics.OverlapSphere(this.transform.position, .1f);
        if (col.Length == 0)
        {
            this.hasValidTarget = false;
            return;
        }

        // Loop through each collider found
        for (int i = 0; i < col.Length; i++)
        {
            // If it's in the interact layer
            if (col[i].gameObject.layer == this.interactLayer)
            {
                meshRenderer.material.color = Color.green;
                this.currentTarget = col[i].gameObject;
                this.hasValidTarget = true;
                return;
            }
        }
        this.hasValidTarget = false;
    }
}
