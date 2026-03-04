using UnityEditor;
using UnityEngine;
using VectorFieldTools;

namespace VectorFieldTools.Editor
{
    /// <summary>
    /// Inspector personalizado para VectorFieldRuntimeController.
    /// Muestra las coordenadas X, Y, Z de forma clara y botones de acción.
    /// </summary>
    [CustomEditor(typeof(VectorFieldRuntimeController))]
    public class VectorFieldRuntimeControllerEditor : UnityEditor.Editor
    {
        private VectorFieldRuntimeController controller;

        private void OnEnable()
        {
            controller = (VectorFieldRuntimeController)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ═══════════════════════════════════════════════
            // CABECERA
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(4);
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("☽  CAMPO VECTORIAL", headerStyle, GUILayout.Height(22));
            DrawHLine(0.5f);

            // ═══════════════════════════════════════════════
            // PREFAB
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Referencias", EditorStyles.boldLabel);
            controller.arrowPrefab = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Prefab de Flecha", "Modelo 3D que se usará para cada vector"),
                controller.arrowPrefab, typeof(GameObject), false);

            controller.waterSurface = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Superficie del Agua", "Transform del océano/agua"),
                controller.waterSurface, typeof(Transform), true);

            // ═══════════════════════════════════════════════
            // MODO DEL CAMPO
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(6);
            DrawHLine(0.3f);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Modo del Campo Vectorial", EditorStyles.boldLabel);

            controller.fieldMode = (VectorFieldRuntimeController.FieldMode)EditorGUILayout.EnumPopup(
                new GUIContent("Modo", "• Coordenadas: los vectores apuntan hacia un punto objetivo\n• Fórmula: define la dirección con ecuaciones matemáticas"),
                controller.fieldMode);

            EditorGUILayout.Space(4);

            // ═══════════════════════════════════════════════
            // BLOQUE COORDENADAS (modo TargetCoordinate)
            // ═══════════════════════════════════════════════
            if (controller.fieldMode == VectorFieldRuntimeController.FieldMode.TargetCoordinate)
            {
                DrawCoordinateBlock();
            }
            else
            {
                DrawFormulaBlock();
            }

            // ═══════════════════════════════════════════════
            // CONFIGURACIÓN DEL ÁREA
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(4);
            DrawHLine(0.3f);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Área del Campo", EditorStyles.boldLabel);

            controller.fieldCenter = EditorGUILayout.Vector3Field(
                new GUIContent("Centro", "Posición central del campo vectorial en el mundo"),
                controller.fieldCenter);

            controller.fieldSize = EditorGUILayout.Vector2Field(
                new GUIContent("Tamaño (X, Z)", "Ancho y largo del área cubierta por flechas"),
                controller.fieldSize);

            controller.minArrows = EditorGUILayout.IntField(
                new GUIContent("Mín. Flechas", "Número mínimo de flechas a generar (≥1000 recomendado)"),
                controller.minArrows);

            // ═══════════════════════════════════════════════
            // APARIENCIA
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(4);
            DrawHLine(0.3f);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Apariencia de Flechas", EditorStyles.boldLabel);

            controller.arrowHeight  = EditorGUILayout.FloatField(
                new GUIContent("Altura", "Distancia sobre el agua donde se colocan las flechas"),
                controller.arrowHeight);

            string scaleTooltip = controller.autoScaleToGrid
                ? "Fracción del tamaño de celda (1.0 = rellena la celda, 0.8 = sin huecos visibles)"
                : "Escala absoluta en unidades de mundo";
            controller.autoScaleToGrid = EditorGUILayout.Toggle(
                new GUIContent("Auto-Escala por Celda", "Escala las flechas relativo al espaciado de la grilla para eliminar huecos"),
                controller.autoScaleToGrid);
            controller.arrowScale   = EditorGUILayout.Slider(
                new GUIContent("Escala", scaleTooltip),
                controller.arrowScale, 0.05f, 2f);

            controller.arrowColor   = EditorGUILayout.ColorField(
                new GUIContent("Color", "Color de las flechas del campo vectorial"),
                controller.arrowColor);

            controller.alignArrowsToWater = EditorGUILayout.Toggle(
                new GUIContent("Alinear al Agua", "Coloca las flechas a la altura de la superficie del agua"),
                controller.alignArrowsToWater);

            // ═══════════════════════════════════════════════
            // ANIMACIÓN
            // ═══════════════════════════════════════════════
            controller.animateArrows = EditorGUILayout.BeginFoldoutHeaderGroup(
                controller.animateArrows, "Animación de Flechas");
            if (controller.animateArrows)
            {
                EditorGUI.indentLevel++;
                controller.arrowBobAmplitude = EditorGUILayout.Slider("Amplitud", controller.arrowBobAmplitude, 0f, 0.5f);
                controller.arrowBobSpeed     = EditorGUILayout.Slider("Velocidad Bob", controller.arrowBobSpeed, 0f, 5f);
                controller.arrowTwistDegrees = EditorGUILayout.Slider("Giro (°)", controller.arrowTwistDegrees, 0f, 30f);
                controller.arrowTwistSpeed   = EditorGUILayout.Slider("Velocidad Giro", controller.arrowTwistSpeed, 0f, 5f);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // ═══════════════════════════════════════════════
            // OBSTÁCULOS
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(4);
            DrawHLine(0.3f);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Detección de Obstáculos", EditorStyles.boldLabel);
            controller.avoidObstacles = EditorGUILayout.Toggle(
                new GUIContent("Evitar Obstáculos", "No genera flechas sobre islas, terreno, etc."),
                controller.avoidObstacles);

            if (controller.avoidObstacles)
            {
                EditorGUI.indentLevel++;
                var obstProp = serializedObject.FindProperty("obstacleLayer");
                EditorGUILayout.PropertyField(obstProp, new GUIContent("Capas de Obstáculos"));
                controller.obstacleCheckRadius = EditorGUILayout.Slider(
                    "Radio de Chequeo", controller.obstacleCheckRadius, 0.1f, 3f);
                EditorGUI.indentLevel--;
            }

            // ═══════════════════════════════════════════════
            // AUTO-GENERACIÓN
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(4);
            DrawHLine(0.3f);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Auto-Generación", EditorStyles.boldLabel);
            controller.autoGenerateInEditor  = EditorGUILayout.Toggle(
                new GUIContent("Generar en Editor", "Genera flechas al abrir la escena (sin Play)"),
                controller.autoGenerateInEditor);
            controller.liveRegenerateInEditor = EditorGUILayout.Toggle(
                new GUIContent("Live Update Editor", "Regenera cuando cambias valores (puede ser lento)"),
                controller.liveRegenerateInEditor);
            controller.autoGenerateOnPlay    = EditorGUILayout.Toggle(
                new GUIContent("Generar al Iniciar Play", "Genera flechas automáticamente al entrar en Play"),
                controller.autoGenerateOnPlay);

            // ═══════════════════════════════════════════════
            // BOTONES DE ACCIÓN
            // ═══════════════════════════════════════════════
            EditorGUILayout.Space(8);
            DrawHLine(0.5f);
            EditorGUILayout.Space(4);

            var btnStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = 12 };

            GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f);
            if (GUILayout.Button("▶  Generar Campo Vectorial", btnStyle, GUILayout.Height(38)))
            {
                controller.GenerateField();
                EditorUtility.SetDirty(controller);
            }

            GUI.backgroundColor = new Color(1f, 0.55f, 0.2f);
            if (GUILayout.Button("↺  Regenerar", btnStyle, GUILayout.Height(30)))
            {
                controller.RegenerateField();
                EditorUtility.SetDirty(controller);
            }

            GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
            if (GUILayout.Button("✕  Limpiar Campo", btnStyle, GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog("Limpiar Campo",
                    "¿Eliminar todas las flechas generadas?", "Sí", "Cancelar"))
                {
                    controller.ClearField();
                    EditorUtility.SetDirty(controller);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(4);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(controller);

                // Si está en modo coordenadas y live update activo, regenerar sobre la marcha
                if (!Application.isPlaying && controller.liveRegenerateInEditor)
                {
                    controller.RegenerateField();
                }
            }
        }

        // ─────────────────────────────────────────────────
        // Bloque de coordenadas objetivo
        // ─────────────────────────────────────────────────
        private void DrawCoordinateBlock()
        {
            var boxStyle = new GUIStyle(GUI.skin.box);
            EditorGUILayout.BeginVertical(boxStyle);

            // Título llamativo
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal   = { textColor = new Color(0.2f, 0.85f, 1f) }
            };
            EditorGUILayout.LabelField("📍  COORDENADAS OBJETIVO (X, Y, Z)", titleStyle);
            EditorGUILayout.HelpBox(
                "Los vectores apuntarán desde cada posición del campo hacia estas coordenadas.\n" +
                "Modifica X, Y, Z para cambiar el destino del flujo.",
                MessageType.Info);

            EditorGUILayout.Space(4);

            // Campo Vector3 con los tres inputs X, Y, Z
            EditorGUILayout.LabelField("Coordenada Destino:", EditorStyles.miniBoldLabel);
            controller.targetCoordinate = EditorGUILayout.Vector3Field(GUIContent.none, controller.targetCoordinate);

            EditorGUILayout.Space(4);
            controller.targetStrength        = EditorGUILayout.Slider(
                new GUIContent("Intensidad", "Qué tan fuerte fluyen los vectores hacia el objetivo"),
                controller.targetStrength, 0.1f, 10f);

            controller.targetInfluenceRadius = EditorGUILayout.FloatField(
                new GUIContent("Radio de Influencia", "0 = influye en todo el campo; >0 = solo en ese radio"),
                controller.targetInfluenceRadius);

            controller.invertTargetDirection = EditorGUILayout.Toggle(
                new GUIContent("Invertir Dirección", "Los vectores se alejan del objetivo en vez de acercarse"),
                controller.invertTargetDirection);

            EditorGUILayout.EndVertical();
        }

        // ─────────────────────────────────────────────────
        // Bloque de fórmulas matemáticas
        // ─────────────────────────────────────────────────
        private void DrawFormulaBlock()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal   = { textColor = new Color(1f, 0.85f, 0.2f) }
            };
            EditorGUILayout.LabelField("∬  FÓRMULAS MATEMÁTICAS", titleStyle);
            EditorGUILayout.HelpBox(
                "Variables: x, y · Funciones: sin, cos, tan, sqrt, abs, exp, log · Constantes: pi, e",
                MessageType.Info);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Fórmula X (horizontal):", EditorStyles.miniBoldLabel);
            controller.formulaX = EditorGUILayout.TextArea(controller.formulaX, GUILayout.Height(36));

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Fórmula Y (profundidad Z):", EditorStyles.miniBoldLabel);
            controller.formulaY = EditorGUILayout.TextArea(controller.formulaY, GUILayout.Height(36));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Ejemplos rápidos:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Circular")) { controller.formulaX = "-y"; controller.formulaY = "x"; }
            if (GUILayout.Button("Radial"))   { controller.formulaX = "x";  controller.formulaY = "y"; }
            if (GUILayout.Button("Onda"))     { controller.formulaX = "sin(y)"; controller.formulaY = "cos(x)"; }
            if (GUILayout.Button("Espiral"))  { controller.formulaX = "-y+x*0.1"; controller.formulaY = "x+y*0.1"; }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // ─────────────────────────────────────────────────
        // Utilidad: línea horizontal decorativa
        // ─────────────────────────────────────────────────
        private static void DrawHLine(float alpha = 0.5f)
        {
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, alpha));
        }
    }
}
