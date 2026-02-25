using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VectorFieldTools
{
    /// <summary>
    /// Herramientas de debug y testing para el campo vectorial
    /// </summary>
    public class VectorFieldDebug : MonoBehaviour
    {
        [Header("Referencias")]
        public VectorFieldManager vectorField;

        [Header("Debug Visualization")]
        public bool showDebugRays = true;
        public bool showObstacleChecks = true;
        public Color rayHitColor = Color.red;
        public Color rayMissColor = Color.green;
        public float rayLength = 5f;

        [Header("Test Points")]
        public bool testSpecificPoints = false;
        public Vector2[] testPoints = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(5, 5),
            new Vector2(-5, 5),
            new Vector2(5, -5),
            new Vector2(-5, -5)
        };

        private MathExpressionParser parser;
        private IslandDetector detector;

        void OnEnable()
        {
            parser = new MathExpressionParser();
            
            if (vectorField != null && vectorField.avoidObstacles)
            {
                detector = new IslandDetector(vectorField.obstacleLayer);
            }
        }

        void OnDrawGizmos()
        {
            if (!showDebugRays || vectorField == null)
                return;

            if (parser == null)
                parser = new MathExpressionParser();

            // Mostrar vectores en puntos de test
            if (testSpecificPoints && testPoints != null)
            {
                foreach (Vector2 testPoint in testPoints)
                {
                    DrawVectorAtPoint(testPoint);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!showObstacleChecks || vectorField == null || !vectorField.avoidObstacles)
                return;

            if (detector == null)
                detector = new IslandDetector(vectorField.obstacleLayer);

            // Dibujar grid de verificación de obstáculos
            int checkPoints = 10;
            float stepX = vectorField.fieldSize.x / checkPoints;
            float stepY = vectorField.fieldSize.y / checkPoints;
            Vector3 startPos = vectorField.fieldCenter - new Vector3(vectorField.fieldSize.x / 2f, 0, vectorField.fieldSize.y / 2f);

            for (int i = 0; i <= checkPoints; i++)
            {
                for (int j = 0; j <= checkPoints; j++)
                {
                    float worldX = startPos.x + i * stepX;
                    float worldZ = startPos.z + j * stepY;
                    Vector3 checkPos = new Vector3(worldX, vectorField.fieldCenter.y + vectorField.arrowHeight, worldZ);

                    bool hasObstacle = detector.IsObstacleAtSphere(checkPos, vectorField.obstacleCheckRadius);
                    
                    Gizmos.color = hasObstacle ? Color.red : Color.green;
                    Gizmos.DrawWireSphere(checkPos, vectorField.obstacleCheckRadius * 0.5f);
                }
            }
        }

        private void DrawVectorAtPoint(Vector2 localPoint)
        {
            try
            {
                float vectorX = parser.Evaluate(vectorField.formulaX, localPoint.x, localPoint.y);
                float vectorY = parser.Evaluate(vectorField.formulaY, localPoint.x, localPoint.y);

                Vector3 worldPos = vectorField.fieldCenter + new Vector3(localPoint.x, vectorField.arrowHeight, localPoint.y);
                Vector3 direction = new Vector3(vectorX, 0, vectorY).normalized;

                // Verificar obstáculo
                bool hasObstacle = false;
                if (detector != null && vectorField.avoidObstacles)
                {
                    hasObstacle = detector.IsObstacleAtSphere(worldPos, vectorField.obstacleCheckRadius);
                }

                Gizmos.color = hasObstacle ? rayHitColor : rayMissColor;
                Gizmos.DrawSphere(worldPos, 0.2f);
                Gizmos.DrawLine(worldPos, worldPos + direction * rayLength);
                
                // Dibujar punta de flecha
                Vector3 arrowTip = worldPos + direction * rayLength;
                Vector3 right = Vector3.Cross(direction, Vector3.up) * 0.3f;
                Gizmos.DrawLine(arrowTip, arrowTip - direction * 0.5f + right);
                Gizmos.DrawLine(arrowTip, arrowTip - direction * 0.5f - right);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error evaluating at point ({localPoint.x}, {localPoint.y}): {e.Message}");
            }
        }

        // Métodos de utilidad para testing
        public void TestFormulaAtPoint(float x, float y)
        {
            if (parser == null)
                parser = new MathExpressionParser();

            try
            {
                float resultX = parser.Evaluate(vectorField.formulaX, x, y);
                float resultY = parser.Evaluate(vectorField.formulaY, x, y);
                
                Debug.Log($"Vector en ({x}, {y}): ({resultX:F3}, {resultY:F3}), Magnitud: {Mathf.Sqrt(resultX*resultX + resultY*resultY):F3}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error: {e.Message}");
            }
        }

        public void TestObstacleDetection(float x, float z)
        {
            if (detector == null)
                detector = new IslandDetector(vectorField.obstacleLayer);

            Vector3 worldPos = new Vector3(x, vectorField.fieldCenter.y + vectorField.arrowHeight, z);
            bool hasObstacle = detector.IsObstacleAtSphere(worldPos, vectorField.obstacleCheckRadius);
            
            Debug.Log($"Obstáculo en ({x}, {z}): {(hasObstacle ? "SÍ" : "NO")}");
        }

        public void PrintFieldInfo()
        {
            if (vectorField == null)
            {
                Debug.LogError("No Vector Field Manager assigned!");
                return;
            }

            Debug.Log($"=== VECTOR FIELD INFO ===\n" +
                     $"Fórmula X: {vectorField.formulaX}\n" +
                     $"Fórmula Y: {vectorField.formulaY}\n" +
                     $"Centro: {vectorField.fieldCenter}\n" +
                     $"Tamaño: {vectorField.fieldSize}\n" +
                     $"Densidad: {vectorField.arrowDensity}\n" +
                     $"Evitar obstáculos: {vectorField.avoidObstacles}\n" +
                     $"Capa de obstáculos: {LayerMask.LayerToName(vectorField.obstacleLayer)}");
        }

        [ContextMenu("Test Formula at Origin")]
        public void TestAtOrigin()
        {
            TestFormulaAtPoint(0, 0);
        }

        [ContextMenu("Test Formula at (5,5)")]
        public void TestAt5_5()
        {
            TestFormulaAtPoint(5, 5);
        }

        [ContextMenu("Print Field Information")]
        public void PrintInfo()
        {
            PrintFieldInfo();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VectorFieldDebug))]
    public class VectorFieldDebugEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            VectorFieldDebug debug = (VectorFieldDebug)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Print Field Information"))
            {
                debug.PrintFieldInfo();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Tests:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test at (0,0)"))
            {
                debug.TestFormulaAtPoint(0, 0);
            }
            if (GUILayout.Button("Test at (5,5)"))
            {
                debug.TestFormulaAtPoint(5, 5);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test at (-5,5)"))
            {
                debug.TestFormulaAtPoint(-5, 5);
            }
            if (GUILayout.Button("Test at (5,-5)"))
            {
                debug.TestFormulaAtPoint(5, -5);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
