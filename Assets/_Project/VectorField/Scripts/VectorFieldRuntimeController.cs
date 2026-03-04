using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Controlador para generar campos vectoriales.
    /// - En Play: los barcos pueden consultar GetVectorAtPosition
    /// - En Editor: puede auto-generar flechas (para que se vean al abrir Unity)
    /// </summary>
    [ExecuteAlways]
    public class VectorFieldRuntimeController : MonoBehaviour
    {
        public enum FieldMode
        {
            Formula = 0,
            TargetCoordinate = 1,
        }

        [Header("Referencias")]
        [Tooltip("Prefab de la flecha 3D")]
        public GameObject arrowPrefab;

        [Header("Modo de Campo")]
        public FieldMode fieldMode = FieldMode.Formula;

        [Header("Modo Coordenadas (Target)")]
        [Tooltip("Coordenada objetivo (X,Y,Z) que modifica la dirección del flujo")]
        public Vector3 targetCoordinate = Vector3.zero;

        [Tooltip("Intensidad del flujo hacia el objetivo")]
        public float targetStrength = 1f;

        [Tooltip("Radio de influencia del objetivo (0 = infinito)")]
        public float targetInfluenceRadius = 0f;

        [Tooltip("Invertir dirección (en vez de ir hacia el objetivo, se aleja)")]
        public bool invertTargetDirection = false;

        [Header("Agua")]
        [Tooltip("Alinear flechas a la altura del agua")]
        public bool alignArrowsToWater = true;

        [Tooltip("Transform del agua (por ejemplo el objeto 'Ocean')")]
        public Transform waterSurface;

        [Tooltip("Capa del agua (opcional). Si está seteada, intentará hacer Raycast para ubicar el nivel del agua")]
        public LayerMask waterLayer;

        [Tooltip("Altura extra sobre el agua (se suma a arrowHeight cuando alignArrowsToWater=true)")]
        public float waterYOffset = 0.0f;

        [Header("Animación de Flechas")]
        [Tooltip("Dar una animación ligera a las flechas para visualizar el flujo")]
        public bool animateArrows = true;

        [Tooltip("Amplitud del movimiento vertical")]
        public float arrowBobAmplitude = 0.05f;

        [Tooltip("Velocidad del movimiento vertical")]
        public float arrowBobSpeed = 1.5f;

        [Tooltip("Grados de giro (twist) alrededor de Y")]
        public float arrowTwistDegrees = 6f;

        [Tooltip("Velocidad del giro")]
        public float arrowTwistSpeed = 1.2f;

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
        
        [Tooltip("Escala de las flechas (con autoScaleToGrid activo: fracción del tamaño de celda. 0.8 = sin huecos)")]
        public float arrowScale = 0.8f;
        
        [Tooltip("Color de las flechas")]
        public Color arrowColor = Color.cyan;

        [Header("Detección de Obstáculos")]
        [Tooltip("Capas de obstáculos (islas, barcos, muelles)")]
        public LayerMask obstacleLayer;

        [Tooltip("Si está activo, evita crear flechas donde haya colliders")]
        public bool avoidObstacles = true;
        
        [Tooltip("Radio de verificación de obstáculos")]
        public float obstacleCheckRadius = 0.5f;

        [Header("Auto-Generación")]
        [Tooltip("Genera flechas automáticamente al abrir la escena en el Editor")]
        public bool autoGenerateInEditor = false;

        [Tooltip("Regenera flechas en el Editor cuando cambias parámetros (puede ser lento)")]
        public bool liveRegenerateInEditor = false;

        [Tooltip("Genera flechas automáticamente al entrar en Play")]
        public bool autoGenerateOnPlay = true;

        [Header("Escala de Flechas")]
        [Tooltip("Cuando activo, arrowScale se interpreta como fracción del tamaño de celda (1 = rellena la celda completa). Elimina los huecos.")]
        public bool autoScaleToGrid = true;

        private static readonly Collider[] obstacleHits = new Collider[16];

        // Componentes internos
        private MathExpressionParser parser;
        private List<GameObject> generatedArrows = new List<GameObject>();
        private List<ArrowData> arrowDataList = new List<ArrowData>();
        private Transform arrowContainer;

        [SerializeField]
        private bool fieldGenerated = false;

        /// <summary>Espaciado calculado entre flechas según densidad actual.</summary>
        public float arrowSpacing
        {
            get
            {
                float area = fieldSize.x * fieldSize.y;
                if (area <= 0f || minArrows <= 0) return 1f;
                float density = Mathf.Sqrt(minArrows / area);
                return 1f / density;
            }
        }

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

        void OnEnable()
        {
            if (parser == null)
                parser = new MathExpressionParser();

#if UNITY_EDITOR
            if (!Application.isPlaying && autoGenerateInEditor && !fieldGenerated)
                GenerateField();
#endif
        }

        void Start()
        {
            if (Application.isPlaying && autoGenerateOnPlay && !fieldGenerated)
                GenerateField();
        }

        void OnValidate()
        {
            if (parser == null)
            {
                parser = new MathExpressionParser();
            }
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

        public void RegenerateField()
        {
            GenerateField();
        }

        [ContextMenu("Vector Field/Regenerar")]
        private void ContextRegenerate()
        {
            RegenerateField();
        }

        /// <summary>
        /// Genera el campo vectorial completo con mínimo 2000 flechas
        /// </summary>
        [ContextMenu("Vector Field/Generar Ahora")]
        public void GenerateField()
        {
            ClearField();

            if (arrowPrefab == null)
            {
                Debug.LogError("No hay prefab de flecha asignado!");
                return;
            }

            // Validar fórmulas solo si aplica
            if (fieldMode == FieldMode.Formula)
            {
                try
                {
                    parser.Evaluate(formulaX, 0, 0);
                    parser.Evaluate(formulaY, 0, 0);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error en fórmulas: {e.Message}");
                    return;
                }
            }

            // Crear o reusar contenedor
            if (arrowContainer == null)
            {
                GameObject container = new GameObject("Vector Field Runtime");
                container.transform.parent = transform;
                container.transform.localPosition = Vector3.zero;
                arrowContainer = container.transform;
            }

            // Calcular densidad para obtener al menos minArrows flechas
            float area = fieldSize.x * fieldSize.y;
            float density = Mathf.Sqrt(minArrows / area);
            
            int arrowsX = Mathf.CeilToInt(fieldSize.x * density);
            int arrowsY = Mathf.CeilToInt(fieldSize.y * density);

            float stepX = fieldSize.x / arrowsX;
            float stepY = fieldSize.y / arrowsY;
            float cellSize = Mathf.Min(stepX, stepY);

            Vector3 startPos = fieldCenter - new Vector3(fieldSize.x / 2f, 0, fieldSize.y / 2f);

            float arrowWorldY = GetArrowWorldY();

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
                    Vector3 checkPos = new Vector3(worldX, arrowWorldY, worldZ);
                    if (avoidObstacles && IsObstacleAt(checkPos))
                    {
                        continue; // Saltar si hay obstáculo
                    }

                    Vector3 worldPos = new Vector3(worldX, arrowWorldY, worldZ);
                    Vector2 vector = EvaluateVector(localX, localY, worldPos);
                    
                    // Saltar vectores muy pequeños
                    if (vector.magnitude < 0.01f)
                        continue;

                    // Crear flecha
                    Vector3 arrowPos = worldPos;
                    GameObject arrow = Instantiate(arrowPrefab, arrowPos, Quaternion.identity, arrowContainer);
                    
                    // Rotar flecha
                    float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
                    arrow.transform.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);
                    
                    // Escalar: si autoScaleToGrid, arrowScale es fracción del tamaño de celda
                    float finalScale = autoScaleToGrid ? (arrowScale * cellSize) : arrowScale;
                    arrow.transform.localScale = Vector3.one * finalScale;

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

                    if (animateArrows)
                    {
                        VectorFieldArrowMotion motion = arrow.GetComponent<VectorFieldArrowMotion>();
                        if (motion == null)
                        {
                            motion = arrow.AddComponent<VectorFieldArrowMotion>();
                        }
                        motion.bobAmplitude = arrowBobAmplitude;
                        motion.bobSpeed = arrowBobSpeed;
                        motion.twistDegrees = arrowTwistDegrees;
                        motion.twistSpeed = arrowTwistSpeed;
                        motion.animateInEditor = false;
                    }

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

            fieldGenerated = true;
        }

        /// <summary>
        /// Limpia todas las flechas
        /// </summary>
        [ContextMenu("Vector Field/Limpiar")]
        public void ClearField()
        {
            foreach (GameObject arrow in generatedArrows)
            {
                SafeDestroy(arrow);
            }

            generatedArrows.Clear();
            arrowDataList.Clear();

            // No destruimos el contenedor para reutilizarlo en la próxima generación
            fieldGenerated = false;
        }

        /// <summary>
        /// Obtiene el vector en una posición específica del campo
        /// </summary>
        public Vector2 GetVectorAtPosition(Vector3 worldPosition)
        {
            float localX = worldPosition.x - fieldCenter.x;
            float localZ = worldPosition.z - fieldCenter.z;

            return EvaluateVector(localX, localZ, worldPosition);
        }

        private Vector2 EvaluateVector(float localX, float localY, Vector3 worldPosition)
        {
            switch (fieldMode)
            {
                case FieldMode.TargetCoordinate:
                {
                    Vector3 toTarget3D = targetCoordinate - worldPosition;
                    Vector2 toTarget = new Vector2(toTarget3D.x, toTarget3D.z);
                    float dist = toTarget.magnitude;
                    if (dist < 0.0001f)
                        return Vector2.zero;

                    float falloff = 1f;
                    if (targetInfluenceRadius > 0.0001f)
                    {
                        falloff = Mathf.Clamp01(1f - (dist / targetInfluenceRadius));
                    }

                    Vector2 dir = toTarget.normalized * (targetStrength * falloff);
                    if (invertTargetDirection)
                        dir = -dir;

                    return dir;
                }

                case FieldMode.Formula:
                default:
                {
                    try
                    {
                        float vectorX = parser.Evaluate(formulaX, localX, localY);
                        float vectorY = parser.Evaluate(formulaY, localX, localY);
                        return new Vector2(vectorX, vectorY);
                    }
                    catch
                    {
                        return Vector2.zero;
                    }
                }
            }
        }

        private float GetArrowWorldY()
        {
            if (!alignArrowsToWater)
                return arrowHeight;

            float waterY = fieldCenter.y;

            if (waterSurface != null)
            {
                waterY = waterSurface.position.y;
            }
            else if (waterLayer.value != 0)
            {
                // Raycast simple hacia abajo para encontrar agua, si existe
                Vector3 rayOrigin = fieldCenter + Vector3.up * 500f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2000f, waterLayer))
                {
                    waterY = hit.point.y;
                }
            }

            return waterY + arrowHeight + waterYOffset;
        }

        private bool IsObstacleAt(Vector3 position)
        {
            // Si el usuario configuró capas explícitas, usar el camino rápido.
            if (obstacleLayer.value != 0)
            {
                return Physics.CheckSphere(position, obstacleCheckRadius, obstacleLayer, QueryTriggerInteraction.Ignore);
            }

            // Fallback: detectar cualquier collider excepto agua y las propias flechas.
            int hitCount = Physics.OverlapSphereNonAlloc(position, obstacleCheckRadius, obstacleHits, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = obstacleHits[i];
                if (col == null)
                    continue;

                // Ignorar colliders que pertenezcan al sistema de flechas
                if (col.transform.IsChildOf(transform))
                    continue;

                // Ignorar el agua si tenemos referencia
                if (waterSurface != null && col.transform.IsChildOf(waterSurface))
                    continue;

                // Ignorar por layer de agua si existe
                if (waterLayer.value != 0)
                {
                    int mask = 1 << col.gameObject.layer;
                    if ((waterLayer.value & mask) != 0)
                        continue;
                }

                return true;
            }

            return false;
        }

        private void SafeDestroy(GameObject obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(obj);
                return;
            }

#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
        }

        // Reutilizar un solo MaterialPropertyBlock para todas las flechas (sin duplicar materiales)
        private static readonly MaterialPropertyBlock _mpb = new MaterialPropertyBlock();

        /// <summary>
        /// Aplica color usando MaterialPropertyBlock (cero instancias de material extra).
        /// </summary>
        private void ApplyColorToArrow(GameObject arrow, Color color)
        {
            Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", color);
                _mpb.SetColor("_Color",     color);
                renderer.SetPropertyBlock(_mpb);
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
