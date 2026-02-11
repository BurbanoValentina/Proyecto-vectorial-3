using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Este script hace que los barcos se muevan automáticamente siguiendo las corrientes
    /// del campo vectorial. Es como si el agua llevara al barco en la dirección que indica
    /// cada vector. Perfecto para simular corrientes marinas o flujos de agua.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BoatVectorFollower : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("El controlador principal del campo vectorial que genera las corrientes")]
        public VectorFieldRuntimeController vectorField;

        [Header("Configuración de Movimiento")]
        [Tooltip("Qué tan rápido se mueve el barco (en unidades por segundo)")]
        public float moveSpeed = 5f;
        
        [Tooltip("Qué tan rápido gira el barco para alinearse con la corriente")]
        public float rotationSpeed = 2f;
        
        [Tooltip("Cuánto afectan las corrientes al barco (1 = afecta completamente, 0 = no afecta)")]
        public float vectorInfluence = 1f;

        [Tooltip("A qué altura del barco medimos la dirección de la corriente")]
        public float checkHeight = 0.5f;

        [Tooltip("¿El barco se mueve solo automáticamente?")]
        public bool autoMove = true;

        [Header("Física")]
        [Tooltip("¿Usar el motor de física de Unity para movimientos más realistas?")]
        public bool usePhysics = true;

        // Componente de física del barco
        private Rigidbody rb;
        // Velocidad actual del barco en el espacio 3D
        private Vector3 currentVelocity;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            
            // Configuramos la física del barco para que no caiga y solo rote en Y
            if (rb != null && usePhysics)
            {
                rb.useGravity = false; // Los barcos flotan, no caen
                // Bloqueamos movimientos verticales y rotaciones laterales para mantener el barco horizontal
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            // Si no asignamos el campo vectorial manualmente, lo buscamos en la escena
            if (vectorField == null)
            {
                vectorField = FindAnyObjectByType<VectorFieldRuntimeController>();
            }
        }

        void FixedUpdate()
        {
            if (!autoMove || vectorField == null)
                return;

            // Obtener la posición donde verificar el vector
            Vector3 checkPosition = transform.position;
            checkPosition.y = checkHeight;

            // Obtener el vector en esta posición
            Vector2 fieldVector = vectorField.GetVectorAtPosition(checkPosition);

            if (fieldVector.magnitude > 0.01f)
            {
                // Convertir el vector 2D a dirección 3D
                Vector3 targetDirection = new Vector3(fieldVector.x, 0, fieldVector.y).normalized;
                
                // Aplicar influencia
                targetDirection *= vectorInfluence;

                if (usePhysics && rb != null)
                {
                    // Mover con física
                    Vector3 targetVelocity = targetDirection * moveSpeed;
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 2f);

                    // Rotar hacia la dirección de movimiento
                    if (rb.linearVelocity.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity);
                        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
                    }
                }
                else
                {
                    // Mover sin física
                    transform.position += targetDirection * moveSpeed * Time.fixedDeltaTime;

                    // Rotar hacia la dirección del vector
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
                }
            }
        }

        void OnDrawGizmos()
        {
            if (vectorField != null && Application.isPlaying)
            {
                Vector3 checkPosition = transform.position;
                checkPosition.y = checkHeight;

                Vector2 fieldVector = vectorField.GetVectorAtPosition(checkPosition);
                
                if (fieldVector.magnitude > 0.01f)
                {
                    Vector3 direction = new Vector3(fieldVector.x, 0, fieldVector.y).normalized;
                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, transform.position + direction * 2f);
                    Gizmos.DrawSphere(transform.position + direction * 2f, 0.2f);
                }
            }
        }

        /// <summary>
        /// Activa o desactiva el movimiento automático
        /// </summary>
        public void SetAutoMove(bool enable)
        {
            autoMove = enable;
            if (rb != null && !enable)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
