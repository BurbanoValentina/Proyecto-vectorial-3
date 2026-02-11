using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Detecta áreas donde no se deben colocar flechas (islas, terreno sólido)
    /// Usa Raycast para detectar colisiones con objetos marcados con capas específicas
    /// </summary>
    public class IslandDetector
    {
        private LayerMask obstacleLayer;
        private float raycastHeight = 100f;
        private float raycastDistance = 200f;

        public IslandDetector(LayerMask obstacleLayer)
        {
            this.obstacleLayer = obstacleLayer;
        }

        public IslandDetector(string[] layerNames)
        {
            obstacleLayer = LayerMask.GetMask(layerNames);
        }

        /// <summary>
        /// Verifica si en la posición X,Z hay un obstáculo (isla, terreno)
        /// </summary>
        public bool IsObstacleAt(float x, float z, float checkHeight = 0f)
        {
            Vector3 origin = new Vector3(x, raycastHeight, z);
            Vector3 direction = Vector3.down;

            // Raycast hacia abajo desde arriba
            if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastDistance, obstacleLayer))
            {
                // Si hay un hit y está por encima del nivel del agua/suelo base
                if (hit.point.y > checkHeight)
                {
                    return true;
                }
            }

            // También hacer raycast desde abajo por si acaso
            origin = new Vector3(x, checkHeight - 10f, z);
            direction = Vector3.up;
            
            if (Physics.Raycast(origin, direction, out hit, raycastDistance, obstacleLayer))
            {
                if (hit.point.y > checkHeight)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si hay un collider en el punto usando OverlapSphere
        /// </summary>
        public bool IsObstacleAtSphere(Vector3 position, float radius = 0.5f)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius, obstacleLayer);
            return colliders.Length > 0;
        }

        /// <summary>
        /// Obtiene la altura del terreno en la posición dada
        /// </summary>
        public float GetTerrainHeight(float x, float z)
        {
            Vector3 origin = new Vector3(x, raycastHeight, z);
            Vector3 direction = Vector3.down;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastDistance, obstacleLayer))
            {
                return hit.point.y;
            }

            return 0f;
        }

        public void SetRaycastHeight(float height)
        {
            raycastHeight = height;
        }

        public void SetRaycastDistance(float distance)
        {
            raycastDistance = distance;
        }
    }
}
