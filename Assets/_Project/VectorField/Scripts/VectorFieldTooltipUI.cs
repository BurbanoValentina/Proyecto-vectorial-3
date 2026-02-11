using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VectorFieldTools
{
    /// <summary>
    /// Esta clase maneja el cuadrito de información que aparece cuando pasas el mouse
    /// sobre una flecha. Muestra datos útiles como posición, dirección y magnitud del vector.
    /// El tooltip sigue al cursor para facilitar la lectura.
    /// </summary>
    public class VectorFieldTooltipUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        // El panel visual que contiene toda la información del tooltip
        public GameObject tooltipPanel;
        // El texto donde mostramos los datos numéricos
        public TextMeshProUGUI tooltipText;

        void Start()
        {
            // Al iniciar, nos aseguramos de que el tooltip esté oculto
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }

            // Si no hay un panel asignado, creamos uno automáticamente
            if (tooltipPanel == null)
            {
                CreateTooltipUI();
            }
        }

        void Update()
        {
            // Cada frame, si el tooltip está visible, lo movemos para que siga al cursor del mouse
            if (tooltipPanel != null && tooltipPanel.activeSelf)
            {
                Vector2 mousePosition = Input.mousePosition;
                // Lo desplazamos un poco para que no tape exactamente lo que estás mirando
                tooltipPanel.transform.position = mousePosition + new Vector2(20, -20);
            }
        }

        public void ShowTooltip(Vector3 position, Vector2 vector)
        {
            if (tooltipPanel != null && tooltipText != null)
            {
                // Construimos el texto con toda la información del vector
                string text = $"Posición: ({position.x:F2}, {position.y:F2}, {position.z:F2})\n";
                text += $"Vector: ({vector.x:F2}, {vector.y:F2})\n";
                text += $"Magnitud: {vector.magnitude:F2}"; // La fuerza o intensidad del vector
                
                tooltipText.text = text;
                tooltipPanel.SetActive(true);
            }
        }

        public void HideTooltip()
        {
            // Simplemente ocultamos el panel cuando ya no lo necesitamos
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void CreateTooltipUI()
        {
            // Intentamos encontrar un Canvas existente en la escena
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Crear panel de tooltip estilo Unity
            GameObject panel = new GameObject("TooltipPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280, 90);
            
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.98f); // Mismo color que el panel principal

            // Agregar borde
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            outline.effectDistance = new Vector2(1, -1);

            // Crear texto
            GameObject textObj = new GameObject("TooltipText");
            textObj.transform.SetParent(panel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            // Usar TextMeshPro
            tooltipText = textObj.AddComponent<TextMeshProUGUI>();
            if (tooltipText != null)
            {
                tooltipText.fontSize = 11;
                tooltipText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                tooltipText.alignment = TextAlignmentOptions.TopLeft;
            }
            else
            {
                // Fallback a Text normal si TMPro no está disponible
                Text normalText = textObj.AddComponent<Text>();
                normalText.fontSize = 11;
                normalText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                normalText.alignment = TextAnchor.UpperLeft;
            }

            tooltipPanel = panel;
            tooltipPanel.SetActive(false);
        }
    }
}
