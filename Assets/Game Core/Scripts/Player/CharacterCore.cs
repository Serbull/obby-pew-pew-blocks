using System;
using UnityEngine;

public class CharacterCore : MonoBehaviour
{
    [Serializable]
    public sealed class PlayerSettings
    {
        public float baseGravity = -9.81f;
        public float groundSnapForce = 20f;
        public float maxGroundedUpVelocity = 0.5f;
        public float speed = 2.3f;
        public float jumpForce = 7;

        [Header("Step Up")]
        public float stepHeight = 0.4f;
        public float stepCheckDistance = 0.4f;
        public float stepSmooth = 0.08f;
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
    private bool _isGrounded;
    private bool _isJumped;

    [HideInInspector]
    public Quaternion rotationAux;

    public bool IsPlayer;

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
        if (_isGrounded)
        {
            if (!_isJumped)
            {
                _isJumped = true;
                _animator.SetTrigger("Jump");
            }

            rb.linearVelocity = transform.up * playerSettings.jumpForce / 1.1f;
        }
    }

    private void PlayerMovement()
    {
        bool steppingUp = _isGrounded && TryStepUp();

        if (!SomethingInFront() || steppingUp)
        {
            var moveSpeed = moveAxis.normalized * playerSettings.speed;
            rb.linearVelocity = new Vector3(moveSpeed.x, rb.linearVelocity.y, moveSpeed.z);
        }

        if (moveAxis == Vector3.zero)
        {
            _animator.SetFloat("Move", 0);
            _animator.SetFloat("RunSpeed", 1);
        }
        else
        {
            _animator.SetFloat("Move", 1);
            _animator.SetFloat("RunSpeed", playerSettings.speed / 4f);
        }
    }

    private bool TryStepUp()
    {
        if (moveAxis == Vector3.zero)
            return false;

        Vector3 moveDir = moveAxis.normalized;

        // Нижний луч на уровне ног: есть ли впереди невысокое препятствие (ступенька)
        Vector3 lowerOrigin = transform.position + transform.up * 0.05f;
        if (Physics.Raycast(lowerOrigin, moveDir, playerSettings.stepCheckDistance, frontLayers))
        {
            // Верхний луч на высоте ступеньки: путь свободен -> препятствие можно перешагнуть
            Vector3 upperOrigin = transform.position + transform.up * playerSettings.stepHeight;
            if (!Physics.Raycast(upperOrigin, moveDir, playerSettings.stepCheckDistance + 0.1f, frontLayers))
            {
                // Плавно поднимаем персонажа вверх
                rb.position += transform.up * playerSettings.stepSmooth;
                return true;
            }
        }

        return false;
    }

    private void GroundCheck()
    {
        if (Physics.SphereCast(transform.position + transform.up * 2, .15f, -transform.up, out _, 2.5f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            _isGrounded = true;
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
            _isGrounded = false;
            _animator.SetBool("Grounded", false);
            pM.staticFriction = 0;
            pM.dynamicFriction = 0;
        }
    }

    private void GravityPhysics()
    {
        var velocity = rb.linearVelocity;
        velocity.y += playerSettings.baseGravity * Time.fixedDeltaTime;

        if (_isGrounded)
        {
            if (_isJumped)
            {
                if (velocity.y <= 0)
                    _isJumped = false;
            }
            else
            {
                if (velocity.y > playerSettings.maxGroundedUpVelocity)
                    velocity.y = playerSettings.maxGroundedUpVelocity;

                // --- ФИНАЛЬНЫЙ ФИКС ДЛЯ ВЕРХНЕЙ ПЛАТФОРМЫ ---
                bool standingOnPhysicsBlock = false;
                if (Physics.Raycast(transform.position + transform.up * 0.1f, -transform.up, out RaycastHit hit, 0.5f, groundLayers))
                {
                    // Ищем Rigidbody на самом объекте ИЛИ на его родителях (это спасет от FinalPlatform)
                    if (hit.collider.GetComponentInParent<Rigidbody>() != null || hit.collider.name.Contains("Platform"))
                    {
                        standingOnPhysicsBlock = true;
                    }
                }

                // Прижимаем игрока только к стабильной статичной земле
                if (!standingOnPhysicsBlock)
                {
                    velocity.y -= playerSettings.groundSnapForce * Time.fixedDeltaTime;
                }
                else
                {
                    // Мягкое удержание на движущихся объектах
                    if (velocity.y < 0) velocity.y = Mathf.Max(velocity.y, -2f);
                }
                // ----------------------------------
            }
        }

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
