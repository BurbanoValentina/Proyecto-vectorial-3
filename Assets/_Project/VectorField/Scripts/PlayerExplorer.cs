using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Controlador de cámara/jugador para explorar el mapa
    /// W: Adelante, S: Atrás, A: Izquierda, D: Derecha, Espacio: Flotar/Volar
    /// Mouse: Rotar cámara
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerExplorer : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Velocidad de movimiento horizontal")]
        public float moveSpeed = 10f;
        
        [Tooltip("Velocidad de movimiento vertical (flotar)")]
        public float flySpeed = 8f;
        
        [Tooltip("Aceleración del movimiento")]
        public float acceleration = 10f;

        [Header("Cámara")]
        [Tooltip("Sensibilidad del mouse")]
        public float mouseSensitivity = 2f;
        
        [Tooltip("Límite vertical de rotación")]
        public float verticalLookLimit = 80f;

        [Header("Vuelo")]
        [Tooltip("Activar modo vuelo desde el inicio")]
        public bool flyMode = true;
        
        [Tooltip("Velocidad de ascenso/descenso")]
        public float verticalSpeed = 5f;

        private CharacterController controller;
        private Camera playerCamera;
        private Vector3 currentVelocity;
        private float verticalVelocity;
        private float cameraPitch = 0f;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            
            // Buscar o crear cámara
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                GameObject camObj = new GameObject("PlayerCamera");
                camObj.transform.parent = transform;
                camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                playerCamera = camObj.AddComponent<Camera>();
            }

            // Ocultar y bloquear el cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleInput();
            HandleMovement();
            HandleCamera();

            // Toggle cursor con ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursor();
            }

            // Toggle fly mode con F
            if (Input.GetKeyDown(KeyCode.F))
            {
                flyMode = !flyMode;
                Debug.Log($"Modo vuelo: {(flyMode ? "Activado" : "Desactivado")}");
            }
        }

        private void HandleInput()
        {
            // No hacer nada si el cursor está visible
            if (Cursor.visible)
                return;

            // Movimiento horizontal (WASD)
            float horizontal = 0f;
            float forward = 0f;

            if (Input.GetKey(KeyCode.W))
                forward += 1f;
            if (Input.GetKey(KeyCode.S))
                forward -= 1f;
            if (Input.GetKey(KeyCode.A))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.D))
                horizontal += 1f;

            // Calcular dirección de movimiento
            Vector3 moveDirection = transform.right * horizontal + transform.forward * forward;
            moveDirection.Normalize();

            // Aplicar velocidad
            Vector3 targetVelocity = moveDirection * moveSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * acceleration);

            // Movimiento vertical (Espacio para subir, Shift para bajar)
            if (flyMode)
            {
                if (Input.GetKey(KeyCode.Space))
                    verticalVelocity = verticalSpeed;
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    verticalVelocity = -verticalSpeed;
                else
                    verticalVelocity = 0f;
            }
            else
            {
                // Gravedad simple
                verticalVelocity -= 9.81f * Time.deltaTime;
                
                if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
                {
                    verticalVelocity = 5f; // Salto
                }
            }
        }

        private void HandleMovement()
        {
            // Combinar velocidad horizontal y vertical
            Vector3 movement = currentVelocity;
            movement.y = verticalVelocity;

            // Mover el controlador
            controller.Move(movement * Time.deltaTime);

            // Resetear velocidad vertical si está en el suelo
            if (controller.isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }
        }

        private void HandleCamera()
        {
            // No rotar si el cursor está visible
            if (Cursor.visible || playerCamera == null)
                return;

            // Rotación horizontal (Y)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            // Rotación vertical (X)
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }

        private void ToggleCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void OnGUI()
        {
            // Mostrar controles
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            string controls = "CONTROLES:\n";
            controls += "W/A/S/D - Movimiento\n";
            controls += "Espacio - Flotar hacia arriba\n";
            controls += "Shift - Flotar hacia abajo\n";
            controls += "F - Toggle modo vuelo\n";
            controls += "Mouse - Rotar cámara\n";
            controls += "V - Panel de campo vectorial\n";
            controls += "ESC - Toggle cursor";

            GUI.Label(new Rect(10, 10, 300, 200), controls, style);
        }
    }
}
