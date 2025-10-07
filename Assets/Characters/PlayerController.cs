using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    // Variables de Movimiento
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    // Detección de Suelo
    [Header("Detección de Suelo")]
    public float groundCheckDistance = 1.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    // Manejo de Pendientes
    [Header("Manejo de Pendientes")]
    public float maxSlopeAngle = 60f;
    private Vector3 slopeNormal;

    // Componentes
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsule;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (capsule.material == null)
        {
            PhysicMaterial mat = new PhysicMaterial();
            mat.dynamicFriction = 0;
            mat.staticFriction = 0;
            mat.frictionCombine = PhysicMaterialCombine.Multiply;
            capsule.material = mat;
        }
    }

    void Update()
    {
        // --- LECTURA DE INPUT ---
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        // --- ROTACIÓN DEL PERSONAJE ---
        if (moveInput.magnitude > 0.1f)
        {
            Quaternion newRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 10f);
        }

        // --- SALTO ---
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");

            if (isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            }
        }

        // --- ATAQUE ---
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("Attack");
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
    }

    private void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            slopeNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            slopeNormal = Vector3.up;
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded)
        {
            Vector3 airMove = moveInput * moveSpeed * 0.8f;
            rb.velocity = new Vector3(airMove.x, rb.velocity.y, airMove.z);
            return;
        }

        Vector3 moveDirection = Vector3.ProjectOnPlane(moveInput, slopeNormal).normalized;
        float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);

        if (slopeAngle > maxSlopeAngle)
        {
            moveDirection = Vector3.zero;
        }

        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        if (moveInput.magnitude > 0.1f)
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }

    // ---------- LA CORRECCIÓN ESTÁ AQUÍ ----------
    private void UpdateAnimator()
    {
        // La animación de caminar/idle ahora solo depende de si el jugador presiona las teclas.
        // Si no se presiona nada (magnitud < 0.1), la velocidad es 0 y se activa el Idle.
        // Si se presiona algo (magnitud > 0.1), se activa la animación de caminar.
        float inputMagnitude = moveInput.magnitude;
        animator.SetFloat("Speed", inputMagnitude);
        
        // Seguimos enviando la información del suelo y la velocidad vertical
        // para las animaciones de salto y caída.
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
    }
    // ---------------------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.down * groundCheckDistance);
    }
}