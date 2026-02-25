using UnityEngine;

namespace Eldvmo.Ripples
{
    /// <summary>
    /// Este script permite crear ondas en el agua haciendo clic con el ratón.
    /// Es como poder tocar el agua con el dedo, generando círculos de ondas
    /// donde hagas clic en la pantalla. Útil para pruebas e interacción del jugador.
    /// </summary>
    public class MouseInputForRipple : MonoBehaviour
    {
        [SerializeField] private MeshRenderer ripplePlane;
        // Array que almacena múltiples puntos de onda
        private Vector4[] ripplePoints = new Vector4[100];
        private int rippleIndex = 0;
        // Último punto clicado para evitar ondas duplicadas
        private Vector2 _oldInputCentre;
        // Máscara para detectar solo el agua
        private int waterLayerMask;

        void Start()
        {
            // Configuramos la máscara para detectar solo el agua
            waterLayerMask = LayerMask.GetMask("Water");
        }

        void FixedUpdate()
        {
            // Detectamos si el jugador está haciendo clic
            if (Input.GetMouseButton(0))
            {
                // Convertimos la posición del ratón en un rayo 3D desde la cámara
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Si el rayo golpea el agua, creamos una onda
                if (Physics.Raycast(ray, out RaycastHit hit, waterLayerMask))
                {
                    // Evitamos crear ondas muy cerca de la anterior para mejor rendimiento
                    if (_oldInputCentre == null || Vector2.Distance(_oldInputCentre, hit.textureCoord) < 0.05f) return;

                    // Guardamos el nuevo punto de onda
                    ripplePoints[rippleIndex] = new Vector4(hit.textureCoord.x, hit.textureCoord.y, Time.time, 0);
                    rippleIndex = (rippleIndex + 1) % ripplePoints.Length;
                    _oldInputCentre = hit.textureCoord;
                }
                // Enviamos todos los puntos de onda al material del agua
                ripplePlane.material.SetVectorArray("_InputCentre", ripplePoints);
            }
        }
    }
}