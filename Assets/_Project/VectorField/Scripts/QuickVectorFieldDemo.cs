using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Demo rápida que genera un campo vectorial al presionar una tecla
    /// </summary>
    public class QuickVectorFieldDemo : MonoBehaviour
    {
        [Header("Referencias")]
        public GameObject arrowPrefab;

        [Header("Configuración Rápida")]
        public KeyCode generateKey = KeyCode.G;
        public KeyCode clearKey = KeyCode.C;

        private VectorFieldRuntimeController controller;
        private bool initialized = false;

        void Start()
        {
            InitializeController();
        }

        void Update()
        {
            if (!initialized)
                return;

            // Generar campo con tecla G
            if (Input.GetKeyDown(generateKey))
            {
                Debug.Log("Generando campo vectorial...");
                controller.GenerateField();
            }

            // Limpiar campo con tecla C
            if (Input.GetKeyDown(clearKey))
            {
                Debug.Log("Limpiando campo vectorial...");
                controller.ClearField();
            }
        }

        private void InitializeController()
        {
            // Buscar o crear controlador
            controller = GetComponent<VectorFieldRuntimeController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<VectorFieldRuntimeController>();
            }

            // Configurar con valores por defecto
            controller.arrowPrefab = arrowPrefab;
            controller.fieldCenter = Vector3.zero;
            controller.fieldSize = new Vector2(100f, 100f);
            controller.minArrows = 2000;
            controller.formulaX = "-y";
            controller.formulaY = "x";
            controller.arrowHeight = 0.5f;
            controller.arrowScale = 0.8f;
            controller.arrowColor = Color.cyan;

            initialized = true;

            Debug.Log($"Demo iniciada. Presiona '{generateKey}' para generar campo, '{clearKey}' para limpiar");
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            style.fontStyle = FontStyle.Bold;

            string message = $"Presiona [{generateKey}] para Generar Campo\n";
            message += $"Presiona [{clearKey}] para Limpiar Campo";

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height - 100, 400, 100), message, style);
        }
    }
}
