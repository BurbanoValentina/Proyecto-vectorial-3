using System.Collections.Generic;
using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Gestiona el campo vectorial matemático con fórmulas para X e Y
    /// Genera flechas en una malla 2D evitando obstáculos
    /// </summary>
    [ExecuteInEditMode]
    public class VectorFieldManager : MonoBehaviour
    {
        [Header("Fórmulas Matemáticas")]
        [Tooltip("Fórmula para el componente X del vector (ej: -y, sin(x), cos(y)*x)")]
        [TextArea(2, 4)]
        public string formulaX = "-y";
        
        [Tooltip("Fórmula para el componente Y del vector (ej: x, cos(x), sin(y)*y)")]
        [TextArea(2, 4)]
        public string formulaY = "x";

        [Header("Área del Campo Vectorial")]
        [Tooltip("Centro del campo vectorial")]
        public Vector3 fieldCenter = Vector3.zero;
        
        [Tooltip("Tamaño del área del campo vectorial")]
        public Vector2 fieldSize = new Vector2(20f, 20f);
        
        [Tooltip("Densidad de flechas (cuántas flechas por unidad)")]
        [Range(0.1f, 5f)]
        public float arrowDensity = 1f;
        
        [Tooltip("Altura a la que se colocan las flechas")]
        public float arrowHeight = 0.5f;

        [Header("Configuración de Flechas")]
        [Tooltip("Prefab de la flecha a instanciar")]
        public GameObject arrowPrefab;
        
        [Tooltip("Escala base de las flechas")]
        [Range(0.1f, 5f)]
        public float arrowScale = 1f;
        
        [Tooltip("Escalar flechas según la magnitud del vector")]
        public bool scaleByMagnitude = true;
        
        [Tooltip("Multiplicador de escala por magnitud")]
        [Range(0.1f, 2f)]
        public float magnitudeScaleMultiplier = 0.5f;
        
        [Tooltip("Escala mínima de las flechas")]
        [Range(0.1f, 2f)]
        public float minArrowScale = 0.3f;
        
        [Tooltip("Escala máxima de las flechas")]
        [Range(0.5f, 5f)]
        public float maxArrowScale = 2f;

        [Header("Detección de Obstáculos")]
        [Tooltip("Capas que representan obstáculos donde NO se colocarán flechas")]
        public LayerMask obstacleLayer;
        
        [Tooltip("Activar detección de obstáculos (islas, terreno)")]
        public bool avoidObstacles = true;
        
        [Tooltip("Radio de verificación de obstáculos")]
        [Range(0.1f, 2f)]
        public float obstacleCheckRadius = 0.5f;

        [Header("Opciones de Visualización")]
        [Tooltip("Color de las flechas")]
        public Color arrowColor = Color.cyan;
        
        [Tooltip("Mostrar el área del campo en el editor")]
        public bool showFieldBounds = true;
        
        [Tooltip("Actualizar en tiempo real en el editor")]
        public bool liveUpdate = false;

        [Header("Estado")]
        [SerializeField]
        private bool fieldGenerated = false;

        // Componentes internos
        private MathExpressionParser parser;
        private IslandDetector islandDetector;
        private List<GameObject> generatedArrows = new List<GameObject>();
        private Transform arrowContainer;

        void OnEnable()
        {
            parser = new MathExpressionParser();
            
            if (avoidObstacles)
            {
                islandDetector = new IslandDetector(obstacleLayer);
            }
        }

        void Update()
        {
            if (liveUpdate && Application.isEditor && !Application.isPlaying)
            {
                RegenerateField();
            }
        }

        /// <summary>
        /// Genera el campo vectorial completo
        /// </summary>
        public void GenerateField()
        {
            ClearField();

            if (parser == null)
                parser = new MathExpressionParser();

            if (avoidObstacles && islandDetector == null)
                islandDetector = new IslandDetector(obstacleLayer);

            // Validar fórmulas
            if (!ValidateFormulas())
            {
                Debug.LogError("Las fórmulas contienen errores. Por favor verifica la sintaxis.");
                return;
            }

            if (arrowPrefab == null)
            {
                Debug.LogError("No se ha asignado un prefab de flecha.");
                return;
            }

            // Crear contenedor para las flechas
            if (arrowContainer == null)
            {
                GameObject container = new GameObject("Vector Field Arrows");
                container.transform.parent = transform;
                container.transform.localPosition = Vector3.zero;
                arrowContainer = container.transform;
            }

            // Calcular el número de flechas en cada dirección
            int arrowsX = Mathf.CeilToInt(fieldSize.x * arrowDensity);
            int arrowsY = Mathf.CeilToInt(fieldSize.y * arrowDensity);

            float stepX = fieldSize.x / arrowsX;
            float stepY = fieldSize.y / arrowsY;

            Vector3 startPos = fieldCenter - new Vector3(fieldSize.x / 2f, 0, fieldSize.y / 2f);

            int arrowCount = 0;

            // Generar flechas en una malla
            for (int i = 0; i <= arrowsX; i++)
            {
                for (int j = 0; j <= arrowsY; j++)
                {
                    float worldX = startPos.x + i * stepX;
                    float worldZ = startPos.z + j * stepY;

                    // Coordenadas locales para las fórmulas
                    float localX = worldX - fieldCenter.x;
                    float localY = worldZ - fieldCenter.z;

                    // Verificar si hay un obstáculo en esta posición
                    if (avoidObstacles && islandDetector != null)
                    {
                        Vector3 checkPos = new Vector3(worldX, fieldCenter.y + arrowHeight, worldZ);
                        if (islandDetector.IsObstacleAtSphere(checkPos, obstacleCheckRadius))
                        {
                            continue; // Saltar esta posición
                        }
                    }

                    // Evaluar las fórmulas
                    float vectorX = parser.EvaluateSafe(formulaX, localX, localY);
                    float vectorY = parser.EvaluateSafe(formulaY, localX, localY);

                    // Crear el vector
                    Vector2 vector = new Vector2(vectorX, vectorY);
                    
                    // Saltar vectores muy pequeños
                    if (vector.magnitude < 0.01f)
                        continue;

                    // Crear la flecha
                    Vector3 arrowPos = new Vector3(worldX, fieldCenter.y + arrowHeight, worldZ);
                    GameObject arrow = Instantiate(arrowPrefab, arrowPos, Quaternion.identity, arrowContainer);
                    
                    // Rotar la flecha para que apunte en la dirección del vector
                    float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
                    arrow.transform.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);

                    // Escalar la flecha
                    float scale = arrowScale;
                    if (scaleByMagnitude)
                    {
                        float magnitudeScale = Mathf.Clamp(vector.magnitude * magnitudeScaleMultiplier, 
                                                           minArrowScale, maxArrowScale);
                        scale *= magnitudeScale;
                    }
                    arrow.transform.localScale = Vector3.one * scale;

                    // Aplicar color
                    ApplyColorToArrow(arrow, arrowColor);

                    arrow.name = $"Arrow_{i}_{j}";
                    generatedArrows.Add(arrow);
                    arrowCount++;
                }
            }

            fieldGenerated = true;
            Debug.Log($"Campo vectorial generado: {arrowCount} flechas creadas.");
        }

        /// <summary>
        /// Regenera el campo (limpia y genera nuevamente)
        /// </summary>
        public void RegenerateField()
        {
            GenerateField();
        }

        /// <summary>
        /// Limpia todas las flechas generadas
        /// </summary>
        public void ClearField()
        {
            foreach (GameObject arrow in generatedArrows)
            {
                if (arrow != null)
                {
                    if (Application.isPlaying)
                        Destroy(arrow);
                    else
                        DestroyImmediate(arrow);
                }
            }
            generatedArrows.Clear();

            if (arrowContainer != null)
            {
                if (Application.isPlaying)
                    Destroy(arrowContainer.gameObject);
                else
                    DestroyImmediate(arrowContainer.gameObject);
                arrowContainer = null;
            }

            fieldGenerated = false;
        }

        /// <summary>
        /// Valida que las fórmulas sean correctas
        /// </summary>
        private bool ValidateFormulas()
        {
            try
            {
                parser.Evaluate(formulaX, 0, 0);
                parser.Evaluate(formulaY, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Aplica color a todos los materiales de la flecha
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
        /// Dibuja el área del campo en el editor
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (showFieldBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = fieldCenter;
                center.y += arrowHeight;
                Gizmos.DrawWireCube(center, new Vector3(fieldSize.x, 0.1f, fieldSize.y));
            }

            // Dibujar algunas flechas de ejemplo
            if (!fieldGenerated && parser != null)
            {
                Gizmos.color = Color.cyan;
                int samples = 5;
                float stepX = fieldSize.x / samples;
                float stepY = fieldSize.y / samples;
                Vector3 startPos = fieldCenter - new Vector3(fieldSize.x / 2f, 0, fieldSize.y / 2f);

                for (int i = 0; i <= samples; i++)
                {
                    for (int j = 0; j <= samples; j++)
                    {
                        float worldX = startPos.x + i * stepX;
                        float worldZ = startPos.z + j * stepY;
                        float localX = worldX - fieldCenter.x;
                        float localY = worldZ - fieldCenter.z;

                        try
                        {
                            float vectorX = parser.Evaluate(formulaX, localX, localY);
                            float vectorY = parser.Evaluate(formulaY, localX, localY);
                            
                            Vector3 pos = new Vector3(worldX, fieldCenter.y + arrowHeight, worldZ);
                            Vector3 dir = new Vector3(vectorX, 0, vectorY).normalized * 0.5f;
                            
                            Gizmos.DrawLine(pos, pos + dir);
                            Gizmos.DrawSphere(pos + dir, 0.1f);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
