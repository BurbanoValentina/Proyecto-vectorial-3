using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Script helper para agregar colliders a objetos del mapa
    /// Adjuntar a objetos como barcos, muelles, islas para evitar generación de flechas
    /// </summary>
    public class MapObjectCollider : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Tipo de collider a agregar")]
        public ColliderType colliderType = ColliderType.Auto;

        [Tooltip("Usar collider como trigger (no físico)")]
        public bool isTrigger = false;

        [Tooltip("Layer del objeto (para evitar flechas)")]
        public string obstacleLayer = "Obstacle";

        public enum ColliderType
        {
            Auto,           // Detecta automáticamente
            Box,            // Box Collider
            Sphere,         // Sphere Collider
            Mesh,           // Mesh Collider
            Capsule         // Capsule Collider
        }

        void Start()
        {
            SetupCollider();
            SetupLayer();
        }

        private void SetupCollider()
        {
            // Verificar si ya tiene collider
            Collider existingCollider = GetComponent<Collider>();
            if (existingCollider != null)
            {
                existingCollider.isTrigger = isTrigger;
                return;
            }

            // Agregar collider según el tipo
            switch (colliderType)
            {
                case ColliderType.Auto:
                    AddAutoCollider();
                    break;
                case ColliderType.Box:
                    AddBoxCollider();
                    break;
                case ColliderType.Sphere:
                    AddSphereCollider();
                    break;
                case ColliderType.Mesh:
                    AddMeshCollider();
                    break;
                case ColliderType.Capsule:
                    AddCapsuleCollider();
                    break;
            }
        }

        private void AddAutoCollider()
        {
            // Detectar si tiene mesh
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // Para objetos simples, usar box collider
                // Para objetos complejos, usar mesh collider
                if (meshFilter.sharedMesh.vertexCount < 100)
                {
                    AddBoxCollider();
                }
                else
                {
                    AddMeshCollider();
                }
            }
            else
            {
                // Si no tiene mesh, usar box collider por defecto
                AddBoxCollider();
            }
        }

        private void AddBoxCollider()
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = isTrigger;

            // Calcular bounds automáticamente
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                col.center = renderer.bounds.center - transform.position;
                col.size = renderer.bounds.size;
            }
        }

        private void AddSphereCollider()
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = isTrigger;

            // Calcular radio automáticamente
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                float maxSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);
                col.radius = maxSize / 2f;
            }
        }

        private void AddMeshCollider()
        {
            MeshCollider col = gameObject.AddComponent<MeshCollider>();
            col.isTrigger = isTrigger;
            col.convex = true; // Necesario para triggers y física

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                col.sharedMesh = meshFilter.sharedMesh;
            }
        }

        private void AddCapsuleCollider()
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.isTrigger = isTrigger;

            // Calcular altura automáticamente
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                col.height = renderer.bounds.size.y;
                col.radius = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.z) / 2f;
            }
        }

        private void SetupLayer()
        {
            // Intentar asignar el layer especificado
            int layer = LayerMask.NameToLayer(obstacleLayer);
            
            if (layer == -1)
            {
                Debug.LogWarning($"Layer '{obstacleLayer}' no existe. Por favor créalo en Project Settings > Tags and Layers");
                // Usar Default layer como fallback
                gameObject.layer = 0;
            }
            else
            {
                gameObject.layer = layer;
            }
        }

        void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                
                if (col is BoxCollider)
                {
                    BoxCollider box = col as BoxCollider;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (col is SphereCollider)
                {
                    SphereCollider sphere = col as SphereCollider;
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                }
            }
        }

        [ContextMenu("Setup All Children")]
        public void SetupAllChildren()
        {
            // Agregar este componente a todos los hijos sin collider
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child == transform) continue;

                if (child.GetComponent<Collider>() == null && child.GetComponent<MapObjectCollider>() == null)
                {
                    MapObjectCollider childCollider = child.gameObject.AddComponent<MapObjectCollider>();
                    childCollider.colliderType = this.colliderType;
                    childCollider.isTrigger = this.isTrigger;
                    childCollider.obstacleLayer = this.obstacleLayer;
                }
            }
        }
    }
}
