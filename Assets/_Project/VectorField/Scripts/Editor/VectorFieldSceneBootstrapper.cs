using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VectorFieldTools;

namespace VectorFieldTools.Editor
{
    [InitializeOnLoad]
    public static class VectorFieldSceneBootstrapper
    {
        private const string SceneName = "BigIsland";
        private const string SystemObjectName = "VectorFieldSystem";
        private const string ArrowPrefabPath = "Assets/_Project/VectorField/Prefabs/visual arrow.prefab";

        static VectorFieldSceneBootstrapper()
        {
            EditorApplication.delayCall += EnsureVectorFieldInActiveScene;
            EditorSceneManager.sceneOpened += (_, __) => EditorApplication.delayCall += EnsureVectorFieldInActiveScene;
        }

        private static void EnsureVectorFieldInActiveScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.isLoaded)
                return;

            // Solo para la escena principal del proyecto (evita tocar escenas de pruebas)
            if (!string.Equals(scene.name, SceneName, System.StringComparison.OrdinalIgnoreCase))
                return;

            // Si ya hay un controlador, no recrear
            if (Object.FindAnyObjectByType<VectorFieldRuntimeController>() != null)
                return;

            GameObject systemGo = GameObject.Find(SystemObjectName);
            if (systemGo == null)
            {
                systemGo = new GameObject(SystemObjectName);
            }

            var controller = systemGo.GetComponent<VectorFieldRuntimeController>();
            if (controller == null)
            {
                controller = systemGo.AddComponent<VectorFieldRuntimeController>();
            }

            // Prefab de flecha
            if (controller.arrowPrefab == null)
            {
                controller.arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
            }

            // Agua (por nombre más común en la escena)
            if (controller.waterSurface == null)
            {
                var ocean = GameObject.Find("Ocean");
                if (ocean != null)
                {
                    controller.waterSurface = ocean.transform;
                }
            }

            if (controller.waterLayer.value == 0)
            {
                controller.waterLayer = LayerMask.GetMask("Water");
            }

            controller.alignArrowsToWater = true;
            controller.autoGenerateInEditor = true;
            controller.liveRegenerateInEditor = false;
            controller.autoGenerateOnPlay = true;

            // Campo base: suficiente para ver flechas al abrir
            controller.fieldCenter = Vector3.zero;
            controller.fieldSize = new Vector2(80f, 80f);
            controller.minArrows = Mathf.Max(controller.minArrows, 1500);
            controller.arrowHeight = 0.05f;
            controller.arrowScale = 0.35f;
            controller.arrowColor = Color.cyan;

            // Por defecto: modo coordenadas (más simple que fórmulas)
            controller.fieldMode = VectorFieldRuntimeController.FieldMode.TargetCoordinate;
            if (controller.targetCoordinate == Vector3.zero)
            {
                // Objetivo por defecto en el centro (ajústalo en el Inspector)
                controller.targetCoordinate = new Vector3(0f, 0f, 0f);
            }
            controller.targetStrength = 2f;
            controller.targetInfluenceRadius = 0f;

            // Obstáculos (si el layer existe)
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1)
            {
                controller.obstacleLayer = 1 << obstacleLayer;
            }

            EditorUtility.SetDirty(systemGo);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);

            // Generar ahora para que aparezca inmediatamente al abrir
            controller.GenerateField();
        }
    }
}
