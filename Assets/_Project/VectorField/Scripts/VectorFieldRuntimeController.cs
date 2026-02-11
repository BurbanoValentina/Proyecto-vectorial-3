using System.Collections.Generic;
using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Controlador runtime para generar campos vectoriales en el juego
    /// Permite crear campos dinámicamente con mínimo 2000 flechas
    /// </summary>
    public class VectorFieldRuntimeController : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Prefab de la flecha 3D")]
        public GameObject arrowPrefab;

        [Header("Configuración de Campo")]
        [Tooltip("Fórmula matemática para X")]
        public string formulaX = "-y";
        
        [Tooltip("Fórmula matemática para Y")]
        public string formulaY = "x";
        
        [Tooltip("Centro del campo vectorial")]
        public Vector3 fieldCenter = Vector3.zero;
        
        [Tooltip("Tamaño del campo (X, Z)")]
        public Vector2 fieldSize = new Vector2(100f, 100f);
        
        [Tooltip("Número mínimo de flechas a generar")]
        public int minArrows = 2000;
        
        [Tooltip("Altura de las flechas")]
        public float arrowHeight = 0.5f;
        
        [Tooltip("Escala de las flechas")]
        public float arrowScale = 0.3f;
        
        [Tooltip("Color de las flechas")]
        public Color arrowColor = Color.cyan;

        [Header("Detección de Obstáculos")]
        [Tooltip("Capas de obstáculos (islas, barcos, muelles)")]
        public LayerMask obstacleLayer;
        
        [Tooltip("Radio de verificación de obstáculos")]
        public float obstacleCheckRadius = 0.5f;

        // Componentes internos
        private MathExpressionParser parser;
        private List<GameObject> generatedArrows = new List<GameObject>();
        private List<ArrowData> arrowDataList = new List<ArrowData>();
        private Transform arrowContainer;

        // Estructura para almacenar datos de cada flecha
        public struct ArrowData
        {
            public Vector3 position;
            public Vector2 vector;
            public GameObject arrowObject;
        }

        void Awake()
        {
            parser = new MathExpressionParser();
        }

        /// <summary>
        /// Genera el campo vectorial con coordenadas personalizadas
        /// </summary>
        public void GenerateFieldFromCoordinates(Vector3 center, Vector2 size, string formX, string formY)
        {
            fieldCenter = center;
            fieldSize = size;
            formulaX = formX;
            formulaY = formY;
            
            GenerateField();
        }

        /// <summary>
        /// Genera el campo vectorial completo con mínimo 2000 flechas
        /// </summary>
        public void GenerateField()
        {
            ClearField();

            if (arrowPrefab == null)
            {
                Debug.LogError("No hay prefab de flecha asignado!");
                return;
            }

            // Validar fórmulas
            try
            {
                parser.Evaluate(formulaX, 0, 0);
                parser.Evaluate(formulaY, 0, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error en fórmulas: {e.Message}");
                return;
            }

            // Crear contenedor
            GameObject container = new GameObject("Vector Field Runtime");
            container.transform.parent = transform;
            container.transform.localPosition = Vector3.zero;
            arrowContainer = container.transform;

            // Calcular densidad para obtener al menos minArrows flechas
            float area = fieldSize.x * fieldSize.y;
            float density = Mathf.Sqrt(minArrows / area);
            
            int arrowsX = Mathf.CeilToInt(fieldSize.x * density);
            int arrowsY = Mathf.CeilToInt(fieldSize.y * density);

            float stepX = fieldSize.x / arrowsX;
            float stepY = fieldSize.y / arrowsY;

            Vector3 startPos = fieldCenter - new Vector3(fieldSize.x / 2f, 0, fieldSize.y / 2f);

            int arrowCount = 0;

            // Generar flechas
            for (int i = 0; i <= arrowsX; i++)
            {
                for (int j = 0; j <= arrowsY; j++)
                {
                    float worldX = startPos.x + i * stepX;
                    float worldZ = startPos.z + j * stepY;

                    // Coordenadas locales
                    float localX = worldX - fieldCenter.x;
                    float localY = worldZ - fieldCenter.z;

                    // Verificar obstáculos
                    Vector3 checkPos = new Vector3(worldX, arrowHeight, worldZ);
                    if (Physics.CheckSphere(checkPos, obstacleCheckRadius, obstacleLayer))
                    {
                        continue; // Saltar si hay obstáculo
                    }

                    // Evaluar fórmulas
                    float vectorX = parser.Evaluate(formulaX, localX, localY);
                    float vectorY = parser.Evaluate(formulaY, localX, localY);

                    Vector2 vector = new Vector2(vectorX, vectorY);
                    
                    // Saltar vectores muy pequeños
                    if (vector.magnitude < 0.01f)
                        continue;

                    // Crear flecha
                    Vector3 arrowPos = new Vector3(worldX, arrowHeight, worldZ);
                    GameObject arrow = Instantiate(arrowPrefab, arrowPos, Quaternion.identity, arrowContainer);
                    
                    // Rotar flecha
                    float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
                    arrow.transform.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);
                    
                    // Escalar
                    arrow.transform.localScale = Vector3.one * arrowScale;

                    // Aplicar color
                    ApplyColorToArrow(arrow, arrowColor);

                    // Agregar collider para detección de mouse
                    if (arrow.GetComponent<Collider>() == null)
                    {
                        BoxCollider col = arrow.AddComponent<BoxCollider>();
                        col.size = new Vector3(0.5f, 0.5f, 1f);
                    }

                    // Agregar componente para mostrar coordenadas
                    ArrowTooltip tooltip = arrow.AddComponent<ArrowTooltip>();
                    tooltip.position = arrowPos;
                    tooltip.vectorDirection = vector;

                    arrow.name = $"Arrow_{arrowCount}";
                    
                    // Guardar datos
                    ArrowData data = new ArrowData
                    {
                        position = arrowPos,
                        vector = vector,
                        arrowObject = arrow
                    };
                    arrowDataList.Add(data);
                    generatedArrows.Add(arrow);
                    arrowCount++;
                }
            }

            Debug.Log($"Campo vectorial generado: {arrowCount} flechas creadas (mínimo requerido: {minArrows})");
        }

        /// <summary>
        /// Limpia todas las flechas
        /// </summary>
        public void ClearField()
        {
            foreach (GameObject arrow in generatedArrows)
            {
                if (arrow != null)
                    Destroy(arrow);
            }
            
            generatedArrows.Clear();
            arrowDataList.Clear();

            if (arrowContainer != null)
            {
                Destroy(arrowContainer.gameObject);
                arrowContainer = null;
            }
        }

        /// <summary>
        /// Obtiene el vector en una posición específica del campo
        /// </summary>
        public Vector2 GetVectorAtPosition(Vector3 worldPosition)
        {
            float localX = worldPosition.x - fieldCenter.x;
            float localZ = worldPosition.z - fieldCenter.z;

            try
            {
                float vectorX = parser.Evaluate(formulaX, localX, localZ);
                float vectorY = parser.Evaluate(formulaY, localX, localZ);
                return new Vector2(vectorX, vectorY);
            }
            catch
            {
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Aplica color a la flecha
        /// </summary>
        private void ApplyColorToArrow(GameObject arrow, Color color)
        {
            Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterial != null)
                {
                    Material mat = new Material(renderer.sharedMaterial);
                    if (mat.HasProperty("_Color"))
                        mat.color = color;
                    else if (mat.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", color);
                    renderer.material = mat;
                }
            }
        }

        /// <summary>
        /// Obtiene la lista de todas las flechas generadas
        /// </summary>
        public List<ArrowData> GetArrowData()
        {
            return arrowDataList;
        }
    }
}
