using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Mueve una flecha a través del campo vectorial siguiendo la dirección
    /// del vector en cada posición. Cuando la flecha sale de los límites del
    /// campo, reaparece en una posición aleatoria dentro de él.
    ///
    /// Uso: asignado automáticamente desde VectorFieldRuntimeController.SetFlowMode().
    /// </summary>
    public class VectorFieldFlowParticle : MonoBehaviour
    {
        // Configurados por VectorFieldRuntimeController
        [HideInInspector] public VectorFieldRuntimeController fieldController;
        [HideInInspector] public float flowSpeed = 3f;
        [HideInInspector] public Vector3 spawnCenter;
        [HideInInspector] public Vector2 spawnSize;
        [HideInInspector] public float spawnY;

        private Vector3 _currentPos;

        void OnEnable()
        {
            _currentPos = transform.position;
            _currentPos.y = spawnY;
        }

        void Update()
        {
            if (fieldController == null || !Application.isPlaying)
                return;

            // Samplear el campo vectorial en la posición actual
            Vector2 vec = fieldController.GetVectorAtPosition(_currentPos);

            if (vec.sqrMagnitude > 0.0001f)
            {
                // Mover en la dirección del vector (X → mundo X, Y → mundo Z)
                Vector3 dir = new Vector3(vec.x, 0f, vec.y).normalized;
                _currentPos += dir * flowSpeed * Time.deltaTime;
                _currentPos.y = spawnY;
                transform.position = _currentPos;

                // Rotar la flecha para que apunte en la dirección del vector
                float angle = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);
            }

            // Si la flecha sale del campo, reaparecer en posición aleatoria
            if (!WithinBounds(_currentPos))
                Respawn();
        }

        /// <summary>
        /// Coloca la flecha en una posición aleatoria dentro del campo.
        /// Llamado desde SetFlowMode para distribuir visualmente las partículas.
        /// </summary>
        public void RandomizePosition()
        {
            Respawn();
        }

        private void Respawn()
        {
            float halfX = spawnSize.x * 0.5f;
            float halfZ = spawnSize.y * 0.5f;

            _currentPos = new Vector3(
                spawnCenter.x + Random.Range(-halfX, halfX),
                spawnY,
                spawnCenter.z + Random.Range(-halfZ, halfZ)
            );
            transform.position = _currentPos;
        }

        private bool WithinBounds(Vector3 pos)
        {
            float halfX = spawnSize.x * 0.5f;
            float halfZ = spawnSize.y * 0.5f;
            return Mathf.Abs(pos.x - spawnCenter.x) <= halfX
                && Mathf.Abs(pos.z - spawnCenter.z) <= halfZ;
        }
    }
}
