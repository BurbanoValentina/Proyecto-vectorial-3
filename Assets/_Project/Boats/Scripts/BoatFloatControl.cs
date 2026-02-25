using UnityEngine;

namespace Eldvmo.Ripples
{
    /// <summary>
    /// Este script controla la flotación de los barcos en el agua.
    /// Detecta las ondas del agua y hace que el barco suba y baje según la fuerza de las olas.
    /// Es como tener un barco real que responde al movimiento del agua.
    /// </summary>
    public class BoatFloatControl : MonoBehaviour
    {
        // Altura inicial del barco cuando empieza el juego
        private float startY;
        // Máscara de capa para detectar solo el agua
        private int waterLayerMask;
        // Punto donde se genera la onda
        private Vector4 ripplePoint;

        [SerializeField] private MeshRenderer ripplePlane;
        Material ripplePlaneMaterial;
        // Intensidad con la que el barco reacciona a las olas
        [SerializeField] private float moveUpStrength = 0.2f;

        void Start()
        {
            // Guardamos la posición inicial del barco para saber su altura base
            startY = gameObject.transform.position.y;
            // Configuramos qué capa es el agua para detectarla correctamente
            waterLayerMask = LayerMask.GetMask("Water");
            ripplePlaneMaterial = ripplePlane.gameObject.GetComponent<MeshRenderer>().material;
        }


        void FixedUpdate()
        {
            // Lanzamos un rayo desde el barco hacia abajo para detectar el agua
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 direction = Vector3.down;

            Ray ray = new Ray(origin, direction);
            RaycastHit hit;

            // Si el rayo golpea el agua, procesamos el movimiento del barco
            if (Physics.Raycast(ray, out hit, 2f, waterLayerMask))
            {
                // Obtenemos la coordenada UV donde el rayo golpeó el agua
                Vector2 uv = hit.textureCoord;

                // Creamos un punto de onda en esa posición
                ripplePoint = new Vector4(uv.x, uv.y, Time.time, 0);

                // Enviamos el punto de onda al material del agua
                ripplePlane.material.SetVector("_InputCentre", ripplePoint);

                // Calculamos la distancia entre el barco y el centro de la onda
                Vector2 rippleUV = new Vector2(ripplePoint.x, ripplePoint.y);
                Vector2 boatUV = new Vector2(uv.x, uv.y); 

                Vector2 offset = boatUV - rippleUV;
                float distance = offset.magnitude;

                // Obtenemos las propiedades de la onda del material
                float waveFrequency = ripplePlaneMaterial.GetFloat("_WaveFrequency");
                float waveSpeed = ripplePlaneMaterial.GetFloat("_WaveSpeed");
                float waveStrength = ripplePlaneMaterial.GetFloat("_WaveStrength");

                // Calculamos la altura de la ola en esta posición usando matemáticas de onda
                float wave = Mathf.Cos(distance * waveFrequency - Time.time * waveSpeed) * 0.5f + 0.5f;

                // Calculamos cuánto debe subir el barco según la ola
                float verticalOffset = wave * waveStrength * moveUpStrength;

                // Aplicamos la nueva altura al barco
                Vector3 currentPos = transform.position;
                currentPos.y = startY + verticalOffset;

                transform.position = currentPos;
            }
        }
    }
}
