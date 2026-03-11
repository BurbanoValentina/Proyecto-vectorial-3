using UnityEngine;
using UnityEditor;

namespace VectorFieldTools
{
    [CustomEditor(typeof(VectorFieldManager))]
    public class VectorFieldManagerEditor : UnityEditor.Editor
    {
        private VectorFieldManager manager;
        private bool showExamples = false;

        private void OnEnable()
        {
            manager = (VectorFieldManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Campo Vectorial Matemático", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Ingresa fórmulas matemáticas para X e Y. Las flechas se generarán según las fórmulas.", MessageType.Info);

            EditorGUILayout.Space();

            // Fórmulas
            EditorGUILayout.LabelField("Fórmulas Matemáticas", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Fórmula X (componente horizontal):", EditorStyles.miniBoldLabel);
            manager.formulaX = EditorGUILayout.TextArea(manager.formulaX, GUILayout.Height(40));
            
            // Validar fórmula X
            ValidateFormula(manager.formulaX, "X");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Fórmula Y (componente vertical):", EditorStyles.miniBoldLabel);
            manager.formulaY = EditorGUILayout.TextArea(manager.formulaY, GUILayout.Height(40));
            
            // Validar fórmula Y
            ValidateFormula(manager.formulaY, "Y");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Botón de ejemplos
            showExamples = EditorGUILayout.Foldout(showExamples, "Ejemplos de Fórmulas", true);
            if (showExamples)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Variables disponibles:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("  • x, y - Coordenadas locales");
                EditorGUILayout.LabelField("  • pi - Número π (3.14159...)");
                EditorGUILayout.LabelField("  • e - Número e (2.71828...)");
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Operadores:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("  • +, -, *, /, ^ (potencia)");
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Funciones:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("  • sin(x), cos(x), tan(x)");
                EditorGUILayout.LabelField("  • sqrt(x), abs(x), exp(x), log(x)");
                EditorGUILayout.LabelField("  • asin(x), acos(x), atan(x)");
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Ejemplos:", EditorStyles.miniBoldLabel);
                
                if (GUILayout.Button("Campo Circular: X=-y, Y=x"))
                {
                    manager.formulaX = "-y";
                    manager.formulaY = "x";
                }
                
                if (GUILayout.Button("Campo Radial: X=x, Y=y"))
                {
                    manager.formulaX = "x";
                    manager.formulaY = "y";
                }
                
                if (GUILayout.Button("Onda Senoidal: X=sin(y), Y=cos(x)"))
                {
                    manager.formulaX = "sin(y)";
                    manager.formulaY = "cos(x)";
                }
                
                if (GUILayout.Button("Espiral: X=-y+x*0.1, Y=x+y*0.1"))
                {
                    manager.formulaX = "-y+x*0.1";
                    manager.formulaY = "x+y*0.1";
                }
                
                if (GUILayout.Button("Vórtice: X=-y/(x^2+y^2+1), Y=x/(x^2+y^2+1)"))
                {
                    manager.formulaX = "-y/(x^2+y^2+1)";
                    manager.formulaY = "x/(x^2+y^2+1)";
                }
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Área del campo
            EditorGUILayout.LabelField("Área del Campo", EditorStyles.boldLabel);
            manager.fieldCenter = EditorGUILayout.Vector3Field("Centro", manager.fieldCenter);
            manager.fieldSize = EditorGUILayout.Vector2Field("Tamaño (X, Z)", manager.fieldSize);
            manager.arrowDensity = EditorGUILayout.Slider("Densidad de Flechas", manager.arrowDensity, 0.1f, 5f);
            manager.arrowHeight = EditorGUILayout.FloatField("Altura", manager.arrowHeight);

            EditorGUILayout.Space();

            // Configuración de flechas
            EditorGUILayout.LabelField("Configuración de Flechas", EditorStyles.boldLabel);
            manager.arrowPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab de Flecha", manager.arrowPrefab, typeof(GameObject), false);
            manager.arrowScale = EditorGUILayout.Slider("Escala Base", manager.arrowScale, 0.1f, 5f);
            manager.scaleByMagnitude = EditorGUILayout.Toggle("Escalar por Magnitud", manager.scaleByMagnitude);
            
            if (manager.scaleByMagnitude)
            {
                EditorGUI.indentLevel++;
                manager.magnitudeScaleMultiplier = EditorGUILayout.Slider("Multiplicador", manager.magnitudeScaleMultiplier, 0.1f, 2f);
                manager.minArrowScale = EditorGUILayout.Slider("Escala Mínima", manager.minArrowScale, 0.1f, 2f);
                manager.maxArrowScale = EditorGUILayout.Slider("Escala Máxima", manager.maxArrowScale, 0.5f, 5f);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Detección de obstáculos
            EditorGUILayout.LabelField("Detección de Obstáculos", EditorStyles.boldLabel);
            manager.avoidObstacles = EditorGUILayout.Toggle("Evitar Obstáculos", manager.avoidObstacles);
            
            if (manager.avoidObstacles)
            {
                EditorGUI.indentLevel++;
                var obstacleLayerProp = serializedObject.FindProperty("obstacleLayer");
                EditorGUILayout.PropertyField(obstacleLayerProp, new GUIContent("Capas de Obstáculos"));
                manager.obstacleCheckRadius = EditorGUILayout.Slider("Radio de Verificación", manager.obstacleCheckRadius, 0.1f, 2f);
                EditorGUI.indentLevel--;
                
                EditorGUILayout.HelpBox("Las flechas NO se generarán en posiciones donde haya colliders de las capas seleccionadas (islas, terreno, etc.)", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Visualización
            EditorGUILayout.LabelField("Visualización", EditorStyles.boldLabel);
            manager.arrowColor = EditorGUILayout.ColorField("Color de Flechas", manager.arrowColor);
            manager.showFieldBounds = EditorGUILayout.Toggle("Mostrar Límites", manager.showFieldBounds);
            manager.liveUpdate = EditorGUILayout.Toggle("Actualización en Vivo", manager.liveUpdate);

            EditorGUILayout.Space(10);

            // Botones de acción
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generar Campo Vectorial", GUILayout.Height(40)))
            {
                manager.GenerateField();
            }
            GUI.backgroundColor = Color.white;
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Regenerar", GUILayout.Height(40)))
            {
                manager.RegenerateField();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Limpiar Campo", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirmar", 
                    "¿Estás seguro de que quieres eliminar todas las flechas?", 
                    "Sí", "Cancelar"))
                {
                    manager.ClearField();
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
            }
        }

        private void ValidateFormula(string formula, string name)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                EditorGUILayout.HelpBox($"Fórmula {name} vacía.", MessageType.Warning);
                return;
            }

            // Usar EvaluateSafe para no mandar Debug.LogError al validar en el Inspector
            MathExpressionParser parser = new MathExpressionParser();
            try
            {
                float result = parser.Evaluate(formula, 1, 1); // x=1, y=1 para probar bien
                EditorGUILayout.LabelField($"  ✓ OK  (valor en 1,1 = {result:F3})", EditorStyles.miniLabel);
            }
            catch (System.Exception e)
            {
                // Mostrar error en el inspector sin spam en consola
                EditorGUILayout.HelpBox($"Error en fórmula {name}: {e.Message}", MessageType.Error);
            }
        }
    }
}
