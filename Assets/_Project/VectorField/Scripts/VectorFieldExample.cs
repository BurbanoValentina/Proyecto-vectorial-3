using UnityEngine;
using VectorFieldTools;

/// <summary>
/// Ejemplo de uso del Vector Field Manager en runtime
/// </summary>
public class VectorFieldExample : MonoBehaviour
{
    [Header("Referencias")]
    public VectorFieldManager vectorField;

    [Header("Configuración de Animación")]
    public bool animateField = false;
    public float animationSpeed = 1f;
    
    private float time = 0f;

    void Start()
    {
        if (vectorField == null)
        {
            vectorField = GetComponent<VectorFieldManager>();
        }

        if (vectorField != null)
        {
            // Generar el campo inicial
            vectorField.GenerateField();
        }
    }

    void Update()
    {
        if (animateField && vectorField != null)
        {
            time += Time.deltaTime * animationSpeed;
            
            // Ejemplo de campo animado: vórtice que rota
            float rotation = time * 0.5f;
            vectorField.formulaX = $"(-y*cos({rotation}) - x*sin({rotation}))/(x^2+y^2+1)";
            vectorField.formulaY = $"(x*cos({rotation}) - y*sin({rotation}))/(x^2+y^2+1)";
            
            vectorField.RegenerateField();
        }
    }

    // Métodos que puedes llamar desde el Inspector o código
    public void SetCircularField()
    {
        if (vectorField != null)
        {
            vectorField.formulaX = "-y";
            vectorField.formulaY = "x";
            vectorField.RegenerateField();
            Debug.Log("Campo circular aplicado");
        }
    }

    public void SetRadialField()
    {
        if (vectorField != null)
        {
            vectorField.formulaX = "x";
            vectorField.formulaY = "y";
            vectorField.RegenerateField();
            Debug.Log("Campo radial aplicado");
        }
    }

    public void SetWaveField()
    {
        if (vectorField != null)
        {
            vectorField.formulaX = "sin(y)";
            vectorField.formulaY = "cos(x)";
            vectorField.RegenerateField();
            Debug.Log("Campo de onda aplicado");
        }
    }

    public void SetSpiralField()
    {
        if (vectorField != null)
        {
            vectorField.formulaX = "-y + x*0.1";
            vectorField.formulaY = "x + y*0.1";
            vectorField.RegenerateField();
            Debug.Log("Campo espiral aplicado");
        }
    }

    public void SetVortexField()
    {
        if (vectorField != null)
        {
            vectorField.formulaX = "-y/(x^2+y^2+1)";
            vectorField.formulaY = "x/(x^2+y^2+1)";
            vectorField.RegenerateField();
            Debug.Log("Campo vórtice aplicado");
        }
    }

    public void ClearField()
    {
        if (vectorField != null)
        {
            vectorField.ClearField();
            Debug.Log("Campo limpiado");
        }
    }
}
