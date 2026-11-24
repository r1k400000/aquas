using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Variabel
    public InputActionAsset InputActions;
    public CharacterController playerCharacterController;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private float waterSurfaceY = 0f; // tinggi permukaan air
    [SerializeField] private float minDistanceFromFloor = 1.0f; // jarak aman dari dasar laut

    private InputAction playerMoveAction;
    private InputAction playerJumpAction;
    private Vector2 playerMoveAmount;
    private float playerWalkSpeed = 5.0f;
    private float playerSwimSpeed = 8.0f;
    private float playerRotateDampening = 0.1f;
    private float turnSmoothingVelocity;
    private float verticalVelocity = 0f;
    private float gravity = -9.81f;
    private float jumpHeight = 5.0f;

    private bool isInWater; // kalau player dalam air
    [SerializeField] private float swimVerticalSpeed = 7.0f; // makin besar angkanya, makin cepat naiknya

    // Siklus hidup
    private void OnEnable() => InputActions.FindActionMap("Player").Enable();
    private void OnDisable() => InputActions.FindActionMap("Player").Disable();

    private void Awake()
    {
        playerMoveAction = InputSystem.actions.FindAction("Move");
        playerJumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Start()
    {
        UpdateColliderSize(isInWater);
    }

    // Atur ukuran CharacterController otomatis
    private void UpdateColliderSize(bool swimming)
    {
        if (swimming)
        {
            // Mode berenang (lebar agar badan & sirip tidak tembus pasir)
            playerCharacterController.radius = 1.1f;
            playerCharacterController.height = 2.2f;
            playerCharacterController.center = new Vector3(0f, 0.5f, -0.15f);
        }
        else
        {
            // Mode darat (berdiri normal agar kaki menapak pasir)
            playerCharacterController.radius = 0.4f;
            playerCharacterController.height = 2.0f;
            playerCharacterController.center = new Vector3(0f, 1.0f, 0f);
        }
    }

    // Deteksi Water
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            UpdateColliderSize(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
            UpdateColliderSize(false);
        }
    }

    // Update (tiap frame)
    private void Update()
    {
        playerMoveAmount = playerMoveAction.ReadValue<Vector2>();

        if (isInWater) Swim();
        else Jump();

        MoveAndRotatePlayer();

        // Update parameter Animator
        if (animator != null)
        {
            bool isMoving = playerMoveAmount.magnitude >= 0.1f;
            animator.SetBool("IsInWater", isInWater);
            animator.SetBool("IsMoving", isMoving);
        }
    }

    // Pengunci ketinggian agar tidak pernah tembus Terrain
    private void LateUpdate()
    {
        if (Terrain.activeTerrain != null)
        {
            float tinggiPasir = Terrain.activeTerrain.SampleHeight(transform.position)
                                + Terrain.activeTerrain.transform.position.y;

            // Saat berenang beri jarak aman sesuai minDistanceFromFloor, saat di darat pas di 0f
            float batasBawah = isInWater ? tinggiPasir + minDistanceFromFloor : tinggiPasir;

            if (transform.position.y < batasBawah)
            {
                playerCharacterController.enabled = false;
                Vector3 pos = transform.position;
                pos.y = batasBawah;
                transform.position = pos;
                playerCharacterController.enabled = true;
            }
        }
    }

    // Renang
    private void Swim()
    {
        // Cek apakah dasar laut terlalu dekat di bawah player (abaikan Trigger seperti WaterVolume)
        bool tooCloseToFloor = Physics.Raycast(
            transform.position + Vector3.up * 0.5f,
            Vector3.down,
            minDistanceFromFloor + 0.5f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        // Tombol Jump = naik, tanpa input = tenggelam pelan
        if (playerJumpAction.IsPressed())
        {
            verticalVelocity = swimVerticalSpeed;
        }
        else if (playerCharacterController.isGrounded || tooCloseToFloor)
        {
            // Berhenti turun begitu sudah dekat dengan dasar laut
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -1.5f, 5f * Time.deltaTime);
        }

        // Jangan naik melewati permukaan air
        if (transform.position.y >= waterSurfaceY && verticalVelocity > 0f)
            verticalVelocity = 0f;
    }

    // Lompat dan gravitasi (darat)
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

    // Gerak dan putar
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