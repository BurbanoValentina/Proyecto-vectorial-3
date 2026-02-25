using System.Collections;
using UnityEngine;

namespace Eldvmo.Ripples
{
    /// <summary>
    /// Este script hace que los objetos creen ondas al tocar el agua.
    /// Detecta cuando un objeto entra en contacto con el agua y genera ondas realistas.
    /// También puede hacer que el objeto flote sobre las olas del agua.
    /// </summary>
    public class ObjectCollisionRipple : MonoBehaviour
    {
        // Indica si el objeto está actualmente dentro del agua
        private bool isInWater = false;
        [SerializeField] private MeshRenderer ripplePlane;
        private Collider ripplePlaneCollider;
        // Array de puntos donde se generan las ondas
        private Vector4[] ripplePoints = new Vector4[10];
        private int rippleIndex = 0;
        // Último punto de colisión para evitar ondas duplicadas
        private Vector2 _oldInputCentre;
        // Máscara para detectar solo el agua
        private int waterLayerMask;
        // Trigger que define el área del agua
        [SerializeField] private Collider waterTrigger;
        // Si está activado, el objeto flotará con el agua
        [SerializeField] bool isFloatingWithWater = true;
        // Altura a la que el objeto sube cuando está en el agua
        [SerializeField] float moveUpHeight = 2f;
        private Rigidbody rb;

        void Start()
        {
            // Obtenemos todos los componentes necesarios al iniciar
            ripplePlaneCollider = ripplePlane.GetComponent<Collider>();
            waterLayerMask = LayerMask.GetMask("Water");
            rb = GetComponent<Rigidbody>();
        }

        void OnTriggerEnter(Collider other)
        {
            // Detectamos cuando el objeto entra al agua
            if (ripplePlaneCollider != null && other == waterTrigger)
            {
                isInWater = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Detectamos cuando el objeto sale del agua
            if (ripplePlaneCollider != null && other == waterTrigger)
            {
                isInWater = false;
            }
        }

        void FixedUpdate()
        {
            // Solo procesamos si el objeto está en el agua
            if (!isInWater) return;
            
            // Lanzamos un rayo desde el objeto hacia el plano de agua
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 direction = Vector3.down;

            Ray ray = new Ray(origin, direction);
            RaycastHit hit;

            // Si golpeamos el agua, creamos una onda
            if (Physics.Raycast(ray, out hit, 2f, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;

                // Evitamos duplicar ondas muy cercanas para mejor rendimiento
                if (_oldInputCentre != null && Vector2.Distance(_oldInputCentre, uv) < 0.05f) return;

                // Agregamos el nuevo punto de onda
                ripplePoints[rippleIndex] = new Vector4(uv.x, uv.y, Time.time, 0);
                rippleIndex = (rippleIndex + 1) % ripplePoints.Length;
                _oldInputCentre = uv;

                // Enviamos el punto de la onda al material del agua
                ripplePlane.material.SetVectorArray("_InputCentre", ripplePoints);

                // Si está configurado para flotar, movemos el objeto con las olas
                if (!isFloatingWithWater) return;

                SetObjectHeight(hit.point.y + moveUpHeight);
                rb.useGravity = false;
                StartCoroutine(EnableGravity());
            }
        }

        private void SetObjectHeight(float targetHeight)
        {
            // Movemos suavemente el objeto a la altura objetivo
            Vector3 currentPos = transform.position;
            currentPos.y = Mathf.Lerp(currentPos.y, targetHeight, Time.fixedDeltaTime * 0.5f);
            transform.position = currentPos;
        }

        private IEnumerator EnableGravity()
        {
            // Simulamos que el objeto rebota sobre las olas deshabilitando temporalmente la gravedad
            yield return new WaitForSeconds(0.5f);
            rb.useGravity = true;
        }
    }
}