using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Script de configuración automática para el sistema de campo vectorial.
    /// Agrega este componente a un GameObject vacío en tu escena y todo se configurará automáticamente.
    /// </summary>
    public class VectorFieldAutoSetup : MonoBehaviour
    {
        [Header("Configuración Inicial")]
        [Tooltip("Prefab de la flecha 3D para el campo vectorial")]
        public GameObject arrowPrefab;
        
        [Tooltip("Ejecutar configuración automáticamente al iniciar")]
        public bool autoSetupOnStart = true;

        [Header("Configuración del Campo")]
        public Vector3 fieldCenter = Vector3.zero;
        public Vector2 fieldSize = new Vector2(100f, 100f);
        public int minArrows = 2000;
        public string formulaX = "-y";
        public string formulaY = "x";

        private bool setupComplete = false;

        void Start()
        {
            if (autoSetupOnStart && !setupComplete)
            {
                SetupVectorFieldSystem();
            }
        }

        [ContextMenu("Configurar Sistema de Campo Vectorial")]
        public void SetupVectorFieldSystem()
        {
            if (setupComplete)
            {
                Debug.LogWarning("El sistema ya está configurado. Si quieres reconfigurar, elimina los componentes primero.");
                return;
            }

            Debug.Log("=== Configurando Sistema de Campo Vectorial ===");

            // 1. Configurar el controlador del campo
            VectorFieldRuntimeController controller = SetupController();
            
            // 2. Configurar el panel de control UI
            SetupControlPanel(controller);
            
            // 3. Configurar el sistema de tooltips
            SetupTooltips();

            setupComplete = true;
            Debug.Log("=== Sistema configurado exitosamente ===");
            Debug.Log("Presiona Play para ver el botón 'Campo Vectorial ▼' en la esquina superior izquierda");
        }

        private VectorFieldRuntimeController SetupController()
        {
            VectorFieldRuntimeController controller = GetComponent<VectorFieldRuntimeController>();
            
            if (controller == null)
            {
                controller = gameObject.AddComponent<VectorFieldRuntimeController>();
                Debug.Log("✓ VectorFieldRuntimeController agregado");
            }

            // Configurar el controlador
            controller.arrowPrefab = arrowPrefab;
            controller.fieldCenter = fieldCenter;
            controller.fieldSize = fieldSize;
            controller.minArrows = minArrows;
            controller.formulaX = formulaX;
            controller.formulaY = formulaY;
            controller.arrowHeight = 0.5f;
            controller.arrowScale = 0.3f;
            controller.arrowColor = Color.cyan;

            // Configurar detección de obstáculos
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1)
            {
                controller.obstacleLayer = 1 << obstacleLayer;
            }

            Debug.Log("✓ Controlador configurado");
            return controller;
        }

        private void SetupControlPanel(VectorFieldRuntimeController controller)
        {
            // Buscar si ya existe
            VectorFieldControlPanel existingPanel = FindAnyObjectByType<VectorFieldControlPanel>();
            
            if (existingPanel != null)
            {
                existingPanel.fieldController = controller;
                Debug.Log("✓ Panel de control ya existe, reconectado");
                return;
            }

            // Crear nuevo objeto para el panel
            GameObject panelObj = new GameObject("VectorFieldUI");
            VectorFieldControlPanel panel = panelObj.AddComponent<VectorFieldControlPanel>();
            panel.fieldController = controller;

            Debug.Log("✓ Panel de control creado - Aparecerá al presionar Play");
        }

        private void SetupTooltips()
        {
            VectorFieldTooltipUI existingTooltip = FindAnyObjectByType<VectorFieldTooltipUI>();
            
            if (existingTooltip != null)
            {
                Debug.Log("✓ Sistema de tooltips ya existe");
                return;
            }

            GameObject tooltipObj = GameObject.Find("VectorFieldUI");
            if (tooltipObj == null)
            {
                tooltipObj = new GameObject("VectorFieldUI");
            }

            VectorFieldTooltipUI tooltip = tooltipObj.GetComponent<VectorFieldTooltipUI>();
            if (tooltip == null)
            {
                tooltip = tooltipObj.AddComponent<VectorFieldTooltipUI>();
                Debug.Log("✓ Sistema de tooltips creado");
            }
        }

        void OnDrawGizmos()
        {
            // Mostrar el área donde se generará el campo
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireCube(fieldCenter, new Vector3(fieldSize.x, 1f, fieldSize.y));
            
            // Mostrar el centro
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(fieldCenter, 2f);
        }
    }
}
