using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Visualizador mejorado para campos vectoriales con efectos adicionales
    /// </summary>
    public class VectorFieldVisualizer : MonoBehaviour
    {
        [Header("Referencias")]
        public VectorFieldRuntimeController fieldController;

        [Header("Visualización")]
        [Tooltip("Mostrar líneas de flujo")]
        public bool showFlowLines = true;
        
        [Tooltip("Color de las líneas de flujo")]
        public Color flowLineColor = Color.cyan;
        
        [Tooltip("Número de líneas de flujo")]
        [Range(5, 50)]
        public int flowLineCount = 10;
        
        [Tooltip("Longitud de las líneas de flujo")]
        public float flowLineLength = 5f;

        [Header("Info Display")]
        [Tooltip("Mostrar información del campo")]
        public bool showFieldInfo = true;

        void OnDrawGizmos()
        {
            if (fieldController == null || !showFlowLines)
                return;

            // Dibujar líneas de flujo
            Gizmos.color = flowLineColor;

            for (int i = 0; i < flowLineCount; i++)
            {
                // Punto de inicio aleatorio en el campo
                float t = (float)i / flowLineCount;
                Vector3 startPos = fieldController.fieldCenter;
                startPos.x += (Random.value - 0.5f) * fieldController.fieldSize.x;
                startPos.z += (Random.value - 0.5f) * fieldController.fieldSize.y;
                startPos.y = fieldController.arrowHeight;

                DrawFlowLine(startPos, flowLineLength, 20);
            }
        }

        private void DrawFlowLine(Vector3 startPos, float length, int segments)
        {
            Vector3 currentPos = startPos;
            float segmentLength = length / segments;

            for (int i = 0; i < segments; i++)
            {
                Vector2 vector = fieldController.GetVectorAtPosition(currentPos);
                
                if (vector.magnitude < 0.01f)
                    break;

                Vector3 direction = new Vector3(vector.x, 0, vector.y).normalized;
                Vector3 nextPos = currentPos + direction * segmentLength;

                // Gradiente de color
                float t = (float)i / segments;
                Gizmos.color = Color.Lerp(Color.cyan, Color.magenta, t);
                
                Gizmos.DrawLine(currentPos, nextPos);
                Gizmos.DrawSphere(nextPos, 0.1f);

                currentPos = nextPos;
            }
        }

        void OnGUI()
        {
            if (!showFieldInfo || fieldController == null)
                return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;

            string info = "=== CAMPO VECTORIAL ===\n";
            info += $"Centro: {fieldController.fieldCenter}\n";
            info += $"Tamaño: {fieldController.fieldSize}\n";
            info += $"Fórmula X: {fieldController.formulaX}\n";
            info += $"Fórmula Y: {fieldController.formulaY}\n";
            info += $"Mín. Flechas: {fieldController.minArrows}";

            GUI.Label(new Rect(Screen.width - 350, 10, 340, 200), info, style);
        }
    }
}
