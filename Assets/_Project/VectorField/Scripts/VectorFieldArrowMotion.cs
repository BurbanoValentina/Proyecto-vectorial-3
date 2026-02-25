using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Animación ligera para dar vida a las flechas (bobbing y twist).
    /// Funciona en Play y también en Editor cuando el objeto existe en escena.
    /// </summary>
    [ExecuteAlways]
    public class VectorFieldArrowMotion : MonoBehaviour
    {
        [Header("Movimiento")]
        public float bobAmplitude = 0.05f;
        public float bobSpeed = 1.5f;

        [Header("Rotación")]
        public float twistDegrees = 6f;
        public float twistSpeed = 1.2f;

        [Header("Editor")]
        [Tooltip("Si está desactivado, solo anima durante Play (recomendado para no ensuciar la escena)")]
        public bool animateInEditor = false;

        [SerializeField]
        private float seed = 0f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;

        void OnEnable()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;

            if (Mathf.Approximately(seed, 0f))
            {
                // Semilla estable por posición para que en editor no "salte" tanto
                Vector3 p = transform.position;
                seed = Mathf.Abs(p.x * 0.13f + p.y * 0.71f + p.z * 0.37f) + 0.1234f;
            }
        }

        void Update()
        {
            if (!Application.isPlaying && !animateInEditor)
                return;

            float t = GetTime();

            float bob = Mathf.Sin((t + seed) * bobSpeed) * bobAmplitude;
            float twist = Mathf.Sin((t + seed) * twistSpeed) * twistDegrees;

            transform.localPosition = baseLocalPosition + new Vector3(0f, bob, 0f);
            transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, twist, 0f);
        }

        private static float GetTime()
        {
            if (Application.isPlaying)
                return Time.time;

#if UNITY_EDITOR
            return (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
            return Time.realtimeSinceStartup;
#endif
        }
    }
}
