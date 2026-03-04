using UnityEngine;
using VectorFieldTools;

namespace VectorFieldTools
{
    /// <summary>
    /// Ejemplo de uso del Vector Field Manager en runtime.
    /// Nota: La animación del campo está limitada a 2 regeneraciones por segundo
    /// para evitar destruir/crear miles de GameObjects cada frame.
    /// </summary>
    public class VectorFieldExample : MonoBehaviour
    {
        [Header("Referencias")]
        public VectorFieldManager vectorField;

        [Header("Animación")]
        public bool animateField = false;
        [Tooltip("Velocidad de animación del vórtice")]
        public float animationSpeed = 1f;
        [Tooltip("Máximo de regeneraciones por segundo (muy costoso instanciar flechas cada frame)")]
        [Range(0.1f, 5f)]
        public float maxRegenerationsPerSecond = 1f;

        private float time = 0f;
        private float lastRegenTime = -1f;

        void Start()
        {
            if (vectorField == null)
                vectorField = GetComponent<VectorFieldManager>();

            if (vectorField != null)
                vectorField.GenerateField();
        }

        void Update()
        {
            if (!animateField || vectorField == null)
                return;

            time += Time.deltaTime * animationSpeed;

            // Throttle: no regenerar más rápido de lo especificado
            float interval = 1f / Mathf.Max(0.01f, maxRegenerationsPerSecond);
            if (Time.time - lastRegenTime < interval)
                return;

            lastRegenTime = Time.time;

            float rotation = time * 0.5f;
            vectorField.formulaX = $"(-y*cos({rotation:F4})-x*sin({rotation:F4}))/(x^2+y^2+1)";
            vectorField.formulaY = $"(x*cos({rotation:F4})-y*sin({rotation:F4}))/(x^2+y^2+1)";
            vectorField.RegenerateField();
        }

        public void SetCircularField()
        {
            if (vectorField == null) return;
            vectorField.formulaX = "-y";
            vectorField.formulaY = "x";
            vectorField.RegenerateField();
        }

        public void SetRadialField()
        {
            if (vectorField == null) return;
            vectorField.formulaX = "x";
            vectorField.formulaY = "y";
            vectorField.RegenerateField();
        }

        public void SetWaveField()
        {
            if (vectorField == null) return;
            vectorField.formulaX = "sin(y)";
            vectorField.formulaY = "cos(x)";
            vectorField.RegenerateField();
        }

        public void SetSpiralField()
        {
            if (vectorField == null) return;
            vectorField.formulaX = "-y+x*0.1";
            vectorField.formulaY = "x+y*0.1";
            vectorField.RegenerateField();
        }

        public void SetVortexField()
        {
            if (vectorField == null) return;
            vectorField.formulaX = "-y/(x^2+y^2+1)";
            vectorField.formulaY = "x/(x^2+y^2+1)";
            vectorField.RegenerateField();
        }

        public void ClearField()
        {
            if (vectorField == null) return;
            vectorField.ClearField();
        }
    }
}

