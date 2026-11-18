using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //variabel
    public InputActionAsset InputActions;
    public CharacterController playerCharacterController;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private float waterSurfaceY = 0f; // tinggi permukaan air

    private InputAction playerMoveAction;
    private InputAction playerJumpAction;
    private Vector2 playerMoveAmount;
    private float playerWalkSpeed = 5.0f;
    private float playerSwimSpeed = 3.0f;
    private float playerRotateDampening = 0.1f;
    private float turnSmoothingVelocity;
    private float verticalVelocity = 0f;
    private float gravity = -9.81f;
    private float jumpHeight = 5.0f;

    private bool isInWater; //kl player dlm air
    private float swimVerticalSpeed = 2.5f;

    //siklus hidup
    private void OnEnable() => InputActions.FindActionMap("Player").Enable();
    private void OnDisable() => InputActions.FindActionMap("Player").Disable();

    private void Awake()
    {
        playerMoveAction = InputSystem.actions.FindAction("Move");
        playerJumpAction = InputSystem.actions.FindAction("Jump");
    }

    //deteksi Water
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water")) isInWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water")) isInWater = false;
    }

    //update (tiap frame)
    private void Update()
    {
        playerMoveAmount = playerMoveAction.ReadValue<Vector2>();

        if (isInWater) Swim();
        else Jump();

        MoveAndRotatePlayer();

        if (animator != null)
            animator.SetBool("IsSwimming", isInWater);
    }

    //renang
    private void Swim()
    {
        // Tombol Jump = naik, tanpa input = tenggelam pelan
        if (playerJumpAction.IsPressed())
            verticalVelocity = swimVerticalSpeed;
        else
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -1.5f, 5f * Time.deltaTime);

        // Jangan naik melewati permukaan air
        if (transform.position.y >= waterSurfaceY && verticalVelocity > 0f)
            verticalVelocity = 0f;
    }

    //lompat dan gravitasi(darat)
    private void Jump()
    {
        if (playerCharacterController.isGrounded)
        {
            verticalVelocity = -1f;
            if (playerJumpAction.WasPressedThisFrame())
                verticalVelocity = jumpHeight;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    //gerak dan putar
    private void MoveAndRotatePlayer()
    {
        float horizontalInput = playerMoveAmount.x;
        float verticalInput = playerMoveAmount.y;

        Quaternion cameraYRotation = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
        Vector3 forwardDir = cameraYRotation * Vector3.forward;
        Vector3 rightDir = cameraYRotation * Vector3.right;

        Vector3 targetMoveDirection =
            (forwardDir * verticalInput + rightDir * horizontalInput).normalized;

        Vector3 verticalMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;
        float speed = isInWater ? playerSwimSpeed : playerWalkSpeed;

        if (targetMoveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                new Vector3(targetMoveDirection.x, 0f, targetMoveDirection.z));

            float smoothTargetAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetRotation.eulerAngles.y,
                ref turnSmoothingVelocity,
                playerRotateDampening);

            transform.rotation = Quaternion.Euler(0f, smoothTargetAngle, 0f);

            playerCharacterController.Move(
                targetMoveDirection * speed * Time.deltaTime + verticalMove);
        }
        else
        {
            playerCharacterController.Move(verticalMove);
        }
    }
}