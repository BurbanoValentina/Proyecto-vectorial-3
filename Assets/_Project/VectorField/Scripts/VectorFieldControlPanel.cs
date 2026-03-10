using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VectorFieldTools
{
    /// <summary>
    /// Este panel de control te permite manipular el campo vectorial en tiempo de ejecución.
    /// Puedes cambiar el centro, tamaño y las fórmulas matemáticas que definen cómo fluyen
    /// los vectores. Es como tener una calculadora visual para campos vectoriales.
    /// Haz clic en el botón de la esquina superior izquierda para mostrar u ocultar el panel.
    /// </summary>
    public class VectorFieldControlPanel : MonoBehaviour
    {
        [Header("Referencias")]
        public VectorFieldRuntimeController fieldController;
        
        [Header("UI Referencias")]
        public GameObject controlPanel;
        public TMP_InputField centerXInput;
        public TMP_InputField centerYInput;
        public TMP_InputField centerZInput;
        public TMP_InputField sizeXInput;
        public TMP_InputField sizeZInput;
        public TMP_InputField formulaXInput;
        public TMP_InputField formulaYInput;
        public TMP_InputField numVectorsInput;
        public TMP_InputField flowSpeedInput;
        public Button generateButton;
        public Button clearButton;
        public Button flowToggleButton;

        private bool flowModeActive = false;

        private bool panelVisible = false;

        void Start()
        {
            if (controlPanel == null)
            {
                CreateControlPanelUI();
            }

            if (generateButton != null)
            {
                generateButton.onClick.AddListener(OnGenerateButtonClick);
            }

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearButtonClick);
            }

            if (flowToggleButton != null)
            {
                flowToggleButton.onClick.AddListener(OnFlowToggleClick);
            }

            if (controlPanel != null)
            {
                controlPanel.SetActive(false);
            }
        }

        void Update()
        {
            // Presiona la tecla V para mostrar u ocultar el panel de control
            if (Input.GetKeyDown(KeyCode.V))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            panelVisible = !panelVisible;
            if (controlPanel != null)
            {
                controlPanel.SetActive(panelVisible);
            }
        }

        private void OnGenerateButtonClick()
        {
            if (fieldController == null)
            {
                Debug.LogError("No hay VectorFieldRuntimeController asignado!");
                return;
            }

            float centerX = ParseFloat(centerXInput?.text, 0f);
            float centerY = ParseFloat(centerYInput?.text, 0.5f);
            float centerZ = ParseFloat(centerZInput?.text, 0f);
            
            float sizeX = ParseFloat(sizeXInput?.text, 100f);
            float sizeZ = ParseFloat(sizeZInput?.text, 100f);

            string formX = formulaXInput?.text ?? "-y";
            string formY = formulaYInput?.text ?? "x";

            int numVectors = ParseInt(numVectorsInput?.text, 2000);
            fieldController.minArrows = Mathf.Max(1, numVectors);

            Vector3 center = new Vector3(centerX, centerY, centerZ);
            Vector2 size = new Vector2(sizeX, sizeZ);

            fieldController.GenerateFieldFromCoordinates(center, size, formX, formY);
        }

        private void OnClearButtonClick()
        {
            if (fieldController != null)
            {
                // Desactivar flujo antes de limpiar
                if (flowModeActive)
                {
                    flowModeActive = false;
                    UpdateFlowButtonLabel();
                }
                fieldController.ClearField();
            }
        }

        private void OnFlowToggleClick()
        {
            if (fieldController == null)
            {
                Debug.LogError("No hay VectorFieldRuntimeController asignado!");
                return;
            }

            flowModeActive = !flowModeActive;
            float speed = ParseFloat(flowSpeedInput?.text, 3f);
            fieldController.SetFlowMode(flowModeActive, speed);
            UpdateFlowButtonLabel();
        }

        private void UpdateFlowButtonLabel()
        {
            if (flowToggleButton == null) return;
            var label = flowToggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null)
                label.text = flowModeActive ? "\u23f9 Detener Flujo" : "\u25b6 Activar Flujo";
        }

        private float ParseFloat(string text, float defaultValue)
        {
            if (float.TryParse(text, out float result))
            {
                return result;
            }
            return defaultValue;
        }

        private int ParseInt(string text, int defaultValue)
        {
            if (int.TryParse(text, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        private void CreateControlPanelUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Panel principal
            GameObject panel = new GameObject("VectorFieldControlPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(10, -45);
            panelRect.sizeDelta = new Vector2(280, 780);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.98f);

            // Contenido
            float yPos = -15;
            float panelWidth = 260;

            // Centro
            CreateSectionHeader(panel.transform, "▼ Centro del Campo", new Vector2(10, yPos), new Vector2(panelWidth, 25));
            yPos -= 30;

            CreateSmallLabel(panel.transform, "X", new Vector2(15, yPos), new Vector2(20, 25));
            centerXInput = CreateCompactInput(panel.transform, "0", new Vector2(38, yPos), new Vector2(70, 25));
            
            CreateSmallLabel(panel.transform, "Y", new Vector2(115, yPos), new Vector2(20, 25));
            centerYInput = CreateCompactInput(panel.transform, "0.5", new Vector2(138, yPos), new Vector2(70, 25));
            
            CreateSmallLabel(panel.transform, "Z", new Vector2(215, yPos), new Vector2(20, 25));
            centerZInput = CreateCompactInput(panel.transform, "0", new Vector2(238, yPos), new Vector2(32, 25));
            yPos -= 35;

            // Tamaño
            CreateSectionHeader(panel.transform, "▼ Tamaño del Campo", new Vector2(10, yPos), new Vector2(panelWidth, 25));
            yPos -= 30;

            CreateSmallLabel(panel.transform, "Ancho (X)", new Vector2(15, yPos), new Vector2(70, 25));
            sizeXInput = CreateCompactInput(panel.transform, "100", new Vector2(90, yPos), new Vector2(70, 25));
            
            CreateSmallLabel(panel.transform, "Largo (Z)", new Vector2(168, yPos), new Vector2(70, 25));
            sizeZInput = CreateCompactInput(panel.transform, "100", new Vector2(238, yPos), new Vector2(32, 25));
            yPos -= 35;

            // Fórmulas
            CreateSectionHeader(panel.transform, "▼ Fórmulas Matemáticas", new Vector2(10, yPos), new Vector2(panelWidth, 25));
            yPos -= 30;

            CreateSmallLabel(panel.transform, "Fórmula X (ej: -y, sin(x))", new Vector2(15, yPos), new Vector2(panelWidth - 20, 20));
            yPos -= 25;
            formulaXInput = CreateCompactInput(panel.transform, "-y", new Vector2(15, yPos), new Vector2(panelWidth - 20, 30));
            yPos -= 40;

            CreateSmallLabel(panel.transform, "Fórmula Y (ej: x, cos(y))", new Vector2(15, yPos), new Vector2(panelWidth - 20, 20));
            yPos -= 25;
            formulaYInput = CreateCompactInput(panel.transform, "x", new Vector2(15, yPos), new Vector2(panelWidth - 20, 30));
            yPos -= 50;

            // Acciones10
            CreateSectionHeader(panel.transform, "▼ Acciones", new Vector2(10, yPos), new Vector2(panelWidth, 25));
            yPos -= 32;

            CreateSmallLabel(panel.transform, "N\u00ba de Vectores:", new Vector2(15, yPos), new Vector2(115, 25));
            numVectorsInput = CreateCompactInput(panel.transform, "2000", new Vector2(135, yPos), new Vector2(115, 25));
            yPos -= 35;

            generateButton = CreateUnityButton(panel.transform, "Generar Campo Vectorial", new Vector2(15, yPos), new Vector2(panelWidth - 20, 35), new Color(0.25f, 0.55f, 0.25f, 1f));
            yPos -= 45;

            clearButton = CreateUnityButton(panel.transform, "Limpiar Campo", new Vector2(15, yPos), new Vector2(panelWidth - 20, 30), new Color(0.5f, 0.25f, 0.25f, 1f));
            yPos -= 42;

            // ─── Modo Flujo ─────────────────────────────────────────────
            CreateSectionHeader(panel.transform, "▼ Modo Flujo (vectores en movimiento)", new Vector2(10, yPos), new Vector2(panelWidth, 25));
            yPos -= 32;

            CreateSmallLabel(panel.transform, "Velocidad:", new Vector2(15, yPos), new Vector2(80, 25));
            flowSpeedInput = CreateCompactInput(panel.transform, "3", new Vector2(100, yPos), new Vector2(150, 25));
            yPos -= 35;

            flowToggleButton = CreateUnityButton(panel.transform, "▶ Activar Flujo", new Vector2(15, yPos), new Vector2(panelWidth - 20, 32), new Color(0.25f, 0.45f, 0.65f, 1f));
            yPos -= 42;

            // ─── Presets ───────────────────────────────────────────────
            CreateSectionHeader(panel.transform, "▼ Presets de Fórmulas (clic para cargar)", new Vector2(10, yPos), new Vector2(panelWidth, 25));
            yPos -= 32;

            // Fila 1
            Button btnRotacion   = CreateUnityButton(panel.transform, "🔄 Rotación",     new Vector2(15,          yPos), new Vector2(115, 30), new Color(0.18f, 0.38f, 0.60f, 1f));
            Button btnConverge  = CreateUnityButton(panel.transform, "▼ Convergente",  new Vector2(140,         yPos), new Vector2(125, 30), new Color(0.18f, 0.50f, 0.40f, 1f));
            yPos -= 36;

            // Fila 2
            Button btnDiverge   = CreateUnityButton(panel.transform, "▲ Divergente",    new Vector2(15,          yPos), new Vector2(115, 30), new Color(0.50f, 0.35f, 0.10f, 1f));
            Button btnEspiral   = CreateUnityButton(panel.transform, "🛆 Espiral",       new Vector2(140,         yPos), new Vector2(125, 30), new Color(0.40f, 0.18f, 0.55f, 1f));
            yPos -= 36;

            // Fila 3
            Button btnSeno      = CreateUnityButton(panel.transform, "~ Onda Seno",     new Vector2(15,          yPos), new Vector2(115, 30), new Color(0.55f, 0.25f, 0.18f, 1f));
            Button btnVortice   = CreateUnityButton(panel.transform, "♻ Vórtice",        new Vector2(140,         yPos), new Vector2(125, 30), new Color(0.20f, 0.40f, 0.55f, 1f));
            yPos -= 42;

            // Nota de ayuda
            CreateSmallLabel(panel.transform,
                "Variables: x, y  |  Ops: + - * / ^ ( )",
                new Vector2(15, yPos), new Vector2(panelWidth - 20, 18));
            yPos -= 18;
            CreateSmallLabel(panel.transform,
                "Funciones: sin cos tan sqrt abs exp log",
                new Vector2(15, yPos), new Vector2(panelWidth - 20, 18));

            // Listeners de presets
            btnRotacion.onClick.AddListener(() => SetPreset("-y",           "x"));
            btnConverge.onClick.AddListener(() => SetPreset("-x",           "-y"));
            btnDiverge.onClick.AddListener(() => SetPreset("x",             "y"));
            btnEspiral.onClick.AddListener(() => SetPreset("-y+0.1*x",     "x+0.1*y"));
            btnSeno.onClick.AddListener(() => SetPreset("sin(y)",          "cos(x)"));
            btnVortice.onClick.AddListener(() => SetPreset("-y^3",          "x^3"));

            controlPanel = panel;
            controlPanel.SetActive(false);
        }

        private TextMeshProUGUI CreateSectionHeader(Transform parent, string text, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject("SectionHeader");
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);

            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 12;
            label.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontStyle = FontStyles.Bold;
            label.margin = new Vector4(8, 0, 0, 0);

            return label;
        }

        private TextMeshProUGUI CreateSmallLabel(Transform parent, string text, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject("Label");
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 11;
            label.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            label.alignment = TextAlignmentOptions.MidlineLeft;

            return label;
        }

        private TMP_InputField CreateCompactInput(Transform parent, string placeholder, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject("InputField");
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.22f, 0.22f, 0.22f, 1f);

            TMP_InputField inputField = obj.AddComponent<TMP_InputField>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 2);
            textRect.offsetMax = new Vector2(-5, -2);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 11;
            text.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            text.alignment = TextAlignmentOptions.MidlineLeft;

            inputField.textComponent = text;
            inputField.text = placeholder;

            return inputField;
        }

        private void SetPreset(string formX, string formY)
        {
            if (formulaXInput != null) formulaXInput.text = formX;
            if (formulaYInput != null) formulaYInput.text = formY;
        }

        private Button CreateUnityButton(Transform parent, string text, Vector2 position, Vector2 size, Color color)
        {
            GameObject obj = new GameObject("Button");
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = obj.AddComponent<Image>();
            image.color = color;

            Button button = obj.AddComponent<Button>();
            
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, 1f);
            colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 1f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 12;
            buttonText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;

            return button;
        }
    }
}
