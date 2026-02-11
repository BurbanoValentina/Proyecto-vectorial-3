using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eldvmo.Ripples
{
    /// <summary>
    /// Este script simula gotas de lluvia cayendo sobre el agua.
    /// Genera ondas aleatorias en diferentes puntos del plano de agua,
    /// creando un efecto visual de lluvia cayendo sobre la superficie.
    /// </summary>
    public class RaindropRipple : MonoBehaviour
    {
        [SerializeField] private MeshRenderer ripplePlane;
        // Array que almacena hasta 100 puntos donde caen gotas
        private Vector4[] ripplePoints = new Vector4[100];
        // Índice para rotar entre los puntos de onda
        private int rippleIndex = 0;
        // Último punto donde cayó una gota para evitar duplicados muy cercanos
        private Vector2 _oldInputCentre;
        // Máscara de capa para detectar solo el agua
        private int waterLayerMask;

        // Frecuencia de caída de gotas por segundo
        [SerializeField] private float raindropFrequency = 10f;
        private float rainDropTimer = 0f;
        // Límites del plano de agua donde pueden caer las gotas
        Bounds bounds;



        void Start()
        {
            // Configuramos la máscara de capa para detectar solo el agua
            waterLayerMask = LayerMask.GetMask("Water");
            // Obtenemos los límites del plano de agua
            bounds = ripplePlane.bounds;
        }
        void Update()
        {
            // Actualizamos el temporizador de las gotas
            rainDropTimer += Time.deltaTime;
            // Calculamos el intervalo entre gotas según la frecuencia
            float interval = 1f / raindropFrequency;

            // Generamos gotas mientras haya tiempo acumulado
            while (rainDropTimer >= interval)
            {
                rainDropTimer -= interval;
                // Elegimos una posición aleatoria dentro del área del agua
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomZ = Random.Range(bounds.min.z, bounds.max.z);
                // Lanzamos un rayo desde arriba hacia el agua
                Vector3 randomPos = new Vector3(randomX, bounds.max.y + 0.5f, randomZ);

                Ray ray = new Ray(randomPos, Vector3.down);

                // Si el rayo golpea el agua, creamos una onda
                if (Physics.Raycast(ray, out RaycastHit hit, 10f, waterLayerMask))
                {
                    // Evitamos crear ondas muy cerca de la anterior para mejor rendimiento
                    if (_oldInputCentre == null || Vector2.Distance(_oldInputCentre, hit.textureCoord) < 0.05f) return;

                    // Guardamos el punto de la nueva onda
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