// <copyright file="AnimatedPhysics.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class AnimatedPhysics : MonoBehaviour
{
    public bool GoForward = false;
    public bool GoLeft = false;
    public bool GoRight = false;
    public bool DoJump = false;

    [Range(0, 2)]
    public float Acceleration = 1f;

    [Range(0, 2)]
    public float Decceleration = 1.5f;

    [Range(0, 2)]
    public float MaxForwardSpeed = .75f;

    [Range(0, 1)]
    public float RotationSpeed = .50f;

    private QueryMechanimController controller;

    private new Rigidbody rigidbody;

    private bool jumping = false;
    private bool moving = false;
    private bool wasMoving = false;

    private float speed = 0f;

    private int idleTimer = 160;

    #region Unity
    private void Awake()
    {
        this.controller = this.GetComponent<QueryMechanimController>();
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        this.ChangeAnimationState();
    }

    private void Update()
    {
        // Jumping
        if ((Input.GetButtonDown("Jump") || this.DoJump) && !this.jumping)
        {
            StartCoroutine("Jump");
        }

        // Don't process movement while jumping
        if (this.jumping)
        {
            return;
        }

        // Movement / Speed
        if (Input.GetAxisRaw("Vertical") == 1 || this.GoForward)
        {
            this.speed = Mathf.MoveTowards(this.speed, this.MaxForwardSpeed, Time.deltaTime * this.Acceleration);
        }
        else
        {
            this.speed = Mathf.MoveTowards(this.speed, 0f, Time.deltaTime * this.Decceleration);
        }

        this.moving = this.speed >= 0.1f;
        
        // Rotation
        if (Input.GetAxisRaw("Horizontal") != 0f || this.GoLeft || this.GoRight)
        {
            float rot = Input.GetAxisRaw("Horizontal");
            if (rot == 0)
            {
                if (this.GoLeft)
                {
                    rot = -1;
                }
                if (this.GoRight)
                {
                    rot = 1;
                }
            }

            Vector3 translate = this.transform.TransformDirection(new Vector3(rot, 0, 0));
            Quaternion look = Quaternion.LookRotation(this.transform.position + translate - this.transform.position, Vector3.up);
            this.rigidbody.MoveRotation(Quaternion.Lerp(this.rigidbody.rotation, look, Time.deltaTime * this.RotationSpeed));
        }
        
        // If moving, reset idle.
        if (this.moving)
        {
            if (!this.wasMoving)
            {
                this.ChangeAnimationState();
                this.wasMoving = true;
            }
            this.idleTimer = 190;
            return;
        }

        // If we just stopped moving, reset animation
        if (this.wasMoving)
        {
            this.ChangeAnimationState();
            this.wasMoving = false;
        }

        // Increae iddle timer while not moving or jumping
        this.idleTimer--;
        if (this.idleTimer <= 0)
        {
            StartCoroutine("IdleStand");
            this.idleTimer = 260 + Random.Range(100, 450);
        }
    }

    private void FixedUpdate()
    {
        // You can only move forwards
        if (this.speed <= 0)
        {
            return;
        }

        Vector3 translate = this.transform.TransformDirection(new Vector3(0, 0, this.speed * Time.fixedDeltaTime));
        this.rigidbody.MovePosition(this.rigidbody.position + translate);
    }
    #endregion

    private void ChangeAnimationState()
    {
        if (this.jumping)
        {
            this.controller.ChangeAnimation(QueryMechanimController.QueryChanAnimationType.JUMP);
        }
        else if (this.moving)
        {
            this.controller.ChangeAnimation(QueryMechanimController.QueryChanAnimationType.WALK);
        }
        else
        {
            this.controller.ChangeAnimation(QueryMechanimController.QueryChanAnimationType.STAND);
        }
    }

    private IEnumerator IdleStand()
    {
        this.controller.ChangeAnimation(QueryMechanimController.QueryChanAnimationType.CH_Stand);
        yield return new WaitForSeconds(5f);
        this.ChangeAnimationState();
        yield return null;
    }

    private IEnumerator Jump()
    {
        this.speed = 0f;
        this.moving = false;
        this.jumping = true;
        this.ChangeAnimationState();

        yield return new WaitForSeconds(1f);
        this.rigidbody.velocity += new Vector3(0, 5f, 0);

        yield return new WaitForSeconds(1f);
        this.jumping = false;
        yield return null;
    }
}
