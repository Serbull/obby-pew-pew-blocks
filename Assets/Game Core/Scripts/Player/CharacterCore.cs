using System;
using UnityEngine;

public class CharacterCore : MonoBehaviour
{
    [Serializable]
    public sealed class PlayerSettings
    {
        public float baseGravity = -9.81f;
        public float speed = 2.3f;
        public float jumpForce = 7;
    }

    public LayerMask groundLayers;
    public LayerMask frontLayers;

    [Header("Player Settings")]
    public PlayerSettings playerSettings = new();

    [HideInInspector]
    public Rigidbody rb;

    private Collider _collider;
    private Animator _animator;

    [HideInInspector]
    public Vector3 moveAxis;

    private PhysicsMaterial pM;

    [HideInInspector]
    public Quaternion rotationAux;

    [HideInInspector]
    public bool grounded;

    public void Start()
    {
        rotationAux = new Quaternion(0, 0, 0, 1);

        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponentInChildren<Animator>();
        pM = _collider.material;
    }

    private void FixedUpdate()
    {
        GroundCheck();
        GravityPhysics();
        PlayerMovement();
    }

    public bool SomethingInFront()
    {
        Vector3 posToDetect = transform.position + transform.up * .5f;
        return Physics.Raycast(posToDetect, transform.forward, 0.5f, frontLayers);
    }

    public void Jump()
    {
        if (grounded)
        {
            rb.linearVelocity = transform.up * playerSettings.jumpForce / 1.1f;
        }
    }

    private void PlayerMovement()
    {
        if (!SomethingInFront())
        {
            var moveSpeed = moveAxis.normalized * playerSettings.speed;
            rb.linearVelocity = new Vector3(moveSpeed.x, rb.linearVelocity.y, moveSpeed.z);
        }

        _animator.SetFloat("Move", moveAxis == Vector3.zero ? 0 : 1);
    }

    private void GroundCheck()
    {
        if (Physics.SphereCast(transform.position + transform.up * 2, .15f, -transform.up, out _, 2.5f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            grounded = true;
            _animator.SetBool("Grounded", true);
            if (moveAxis == Vector3.zero)
            {
                pM.staticFriction = 3;
                pM.dynamicFriction = 3;
            }
            else
            {
                pM.staticFriction = 0;
                pM.dynamicFriction = 0;
            }
        }
        else
        {
            grounded = false;
            _animator.SetBool("Grounded", false);
            pM.staticFriction = 0;
            pM.dynamicFriction = 0;
        }
    }

    private void GravityPhysics()
    {
        var velocity = rb.linearVelocity;
        velocity.y += playerSettings.baseGravity * Time.fixedDeltaTime;
        rb.linearVelocity = velocity;
    }

    public void ForceRotate(Transform targetTransform)
    {
        Quaternion targetRotation = targetTransform.rotation;

        Quaternion rotationWithOffset = targetRotation * Quaternion.Euler(0, 180, 0);

        rotationAux = rotationWithOffset;
        transform.rotation = rotationAux;
    }

    public void ForceMove(Vector3 position)
    {
        transform.position = position;
    }
}
