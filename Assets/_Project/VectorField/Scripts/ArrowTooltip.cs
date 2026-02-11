using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Este componente se encarga de mostrar información interactiva cuando pasas el cursor
    /// sobre una flecha del campo vectorial. Te permite ver las coordenadas exactas y la
    /// dirección del vector en ese punto específico del campo.
    /// </summary>
    public class ArrowTooltip : MonoBehaviour
    {
        // Posición tridimensional de esta flecha en el mundo
        public Vector3 position;
        
        // Dirección del vector que representa esta flecha (solo X e Y)
        public Vector2 vectorDirection;

        // Referencia compartida al sistema de interfaz que muestra los tooltips
        private static VectorFieldTooltipUI tooltipUI;

        void Start()
        {
            // Si aún no hemos encontrado el sistema de tooltips, lo buscamos en la escena
            // Esto solo se hace una vez al iniciar para optimizar el rendimiento
            if (tooltipUI == null)
            {
                tooltipUI = FindAnyObjectByType<VectorFieldTooltipUI>();
            }
        }

        void OnMouseEnter()
        {
            // Cuando el cursor entra en la flecha, mostramos el tooltip con toda la información
            if (tooltipUI != null)
            {
                tooltipUI.ShowTooltip(position, vectorDirection);
            }
        }

        void OnMouseExit()
        {
            // Cuando el cursor sale de la flecha, ocultamos el tooltip para mantener la interfaz limpia
            if (tooltipUI != null)
            {
                tooltipUI.HideTooltip();
            }
        }
    }
}
