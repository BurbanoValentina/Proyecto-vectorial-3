using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VectorFieldTools
{
    /// <summary>
    /// Este es el cerebro de la configuración inicial. Cuando inicias la escena,
    /// este script se encarga de preparar TODO automáticamente: el campo vectorial,
    /// la interfaz de usuario, el jugador, y los obstáculos. Es como un asistente
    /// que configura tu escena para que todo funcione sin que tengas que hacerlo manualmente.
    /// </summary>
    public class VectorFieldSceneSetup : MonoBehaviour
    {
        [Header("Configuración Automática")]
        [Tooltip("¿Configurar todo automáticamente cuando empiece el juego?")]
        public bool autoSetup = true;

        [Header("Prefabs")]
        // Modelo 3D de la flecha que se usa para visualizar los vectores
        public GameObject arrowPrefab;
        // Modelo del jugador que explorará el campo vectorial
        public GameObject playerPrefab;

        [Header("Configuración del Campo")]
        // Punto central del campo vectorial en el espacio 3D
        public Vector3 fieldCenter = Vector3.zero;
        // Tamaño del área que cubrirá el campo (ancho y largo)
        public Vector2 fieldSize = new Vector2(100f, 100f);
        // Cantidad mínima de flechas que se generarán para visualizar el campo
        public int minArrows = 2000;
        // Fórmulas que definen el comportamiento matemático del campo
        public string formulaX = "-y";
        public string formulaY = "x";

        [Header("Layers")]
        // Nombre del layer que usaremos para identificar obstáculos
        public string obstacleLayerName = "Obstacle";

        void Start()
        {
            // Si la configuración automática está activada, preparamos toda la escena al iniciar
            if (autoSetup)
            {
                SetupScene();
            }
        }

        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("=== Iniciando configuración automática de la escena ===");

            // Paso 1: Verificamos que exista el layer para detectar obstáculos
            SetupObstacleLayer();

            // Paso 2: Creamos y configuramos el sistema que genera el campo vectorial
            VectorFieldRuntimeController controller = SetupVectorFieldController();

            // Paso 3: Preparamos toda la interfaz de usuario (paneles, botones, tooltips)
            SetupUI(controller);

            // Paso 4: Creamos al jugador y su cámara para que pueda explorar
            SetupPlayer();

            // Paso 5: Configuramos todos los objetos del mapa (islas, muelles, barcos)
            SetupMapObjects();

            Debug.Log("=== ¡Configuración completada exitosamente! ===");
        }

        private void SetupObstacleLayer()
        {
            Debug.Log($"Verificando layer '{obstacleLayerName}'...");
            
            int layer = LayerMask.NameToLayer(obstacleLayerName);
            if (layer == -1)
            {
                Debug.LogWarning($"El layer '{obstacleLayerName}' no existe. Por favor créalo manualmente en:");
                Debug.LogWarning("Edit > Project Settings > Tags and Layers > Layers");
                Debug.LogWarning("Agrega 'Obstacle' en uno de los User Layers disponibles");
            }
            else
            {
                Debug.Log($"Layer '{obstacleLayerName}' encontrado (index: {layer})");
            }
        }

        private VectorFieldRuntimeController SetupVectorFieldController()
        {
            Debug.Log("Configurando VectorFieldRuntimeController...");

            VectorFieldRuntimeController controller = GetComponent<VectorFieldRuntimeController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<VectorFieldRuntimeController>();
            }

            // Configurar controlador
            controller.arrowPrefab = arrowPrefab;
            controller.fieldCenter = fieldCenter;
            controller.fieldSize = fieldSize;
            controller.minArrows = minArrows;
            controller.formulaX = formulaX;
            controller.formulaY = formulaY;
            
            int obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);
            if (obstacleLayer != -1)
            {
                controller.obstacleLayer = 1 << obstacleLayer;
            }

            Debug.Log("VectorFieldRuntimeController configurado");
            return controller;
        }

        private void SetupUI(VectorFieldRuntimeController controller)
        {
            Debug.Log("Configurando UI...");

            // Buscar o crear objeto UI
            GameObject uiObject = GameObject.Find("VectorFieldUI");
            if (uiObject == null)
            {
                uiObject = new GameObject("VectorFieldUI");
            }

            // Control Panel
            VectorFieldControlPanel controlPanel = uiObject.GetComponent<VectorFieldControlPanel>();
            if (controlPanel == null)
            {
                controlPanel = uiObject.AddComponent<VectorFieldControlPanel>();
            }
            controlPanel.fieldController = controller;

            // Tooltip UI
            VectorFieldTooltipUI tooltipUI = uiObject.GetComponent<VectorFieldTooltipUI>();
            if (tooltipUI == null)
            {
                tooltipUI = uiObject.AddComponent<VectorFieldTooltipUI>();
            }

            Debug.Log("UI configurada");
        }

        private void SetupPlayer()
        {
            Debug.Log("Configurando jugador...");

            // Primero verificamos si ya hay un jugador en la escena
            PlayerExplorer player = FindAnyObjectByType<PlayerExplorer>();
            
            if (player == null)
            {
                // Crear jugador
                GameObject playerObj;
                
                if (playerPrefab != null)
                {
                    playerObj = Instantiate(playerPrefab);
                    playerObj.name = "Player";
                }
                else
                {
                    playerObj = new GameObject("Player");
                }

                // Posicionar jugador
                playerObj.transform.position = fieldCenter + new Vector3(0, 10f, 0);

                // Agregamos todos los componentes necesarios para que el jugador funcione
                CharacterController charController = playerObj.GetComponent<CharacterController>();
                if (charController == null)
                {
                    charController = playerObj.AddComponent<CharacterController>();
                    // Configuramos el tamaño del personaje (como una cápsula)
                    charController.radius = 0.5f;  // Radio del personaje
                    charController.height = 1.8f;  // Altura de una persona promedio
                    charController.center = new Vector3(0, 0.9f, 0);  // Centro de la cápsula
                }

                player = playerObj.GetComponent<PlayerExplorer>();
                if (player == null)
                {
                    // Agregamos el script que permite al jugador moverse y explorar
                    player = playerObj.AddComponent<PlayerExplorer>();
                }

                // Configuramos la cámara para que el jugador pueda ver el mundo
                Camera cam = playerObj.GetComponentInChildren<Camera>();
                if (cam == null)
                {
                    GameObject camObj = new GameObject("PlayerCamera");
                    camObj.transform.SetParent(playerObj.transform);
                    // Posicionamos la cámara a la altura de los ojos
                    camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                    cam = camObj.AddComponent<Camera>();
                }

                // Marcamos esta cámara como la principal del juego
                cam.tag = "MainCamera";

                Debug.Log("Jugador creado");
            }
            else
            {
                Debug.Log("Jugador ya existe en la escena");
            }
        }

        private void SetupMapObjects()
        {
            Debug.Log("Configurando objetos del mapa...");

            // Lista de nombres comunes de objetos que deberían comportarse como obstáculos
            string[] objectNames = { "Boat", "Island", "Muelle", "Dock", "Terrain", "Ground" };
            
            int objectsConfigured = 0;

            // Recorremos cada tipo de objeto para configurarlo como obstáculo
            foreach (string objName in objectNames)
            {
                // Intentamos encontrar objetos usando sus tags
                GameObject[] objects = GameObject.FindGameObjectsWithTag(objName);
                
                // Si no encontramos nada con tags, intentamos buscar por nombre del objeto
                if (objects.Length == 0)
                {
                    GameObject obj = GameObject.Find(objName);
                    if (obj != null)
                    {
                        objects = new GameObject[] { obj };
                    }
                }

                // Para cada objeto encontrado, lo configuramos como obstáculo
                foreach (GameObject obj in objects)
                {
                    MapObjectCollider mapCollider = obj.GetComponent<MapObjectCollider>();
                    if (mapCollider == null)
                    {
                        // Agregamos el componente que hace que este objeto bloquee los vectores
                        mapCollider = obj.AddComponent<MapObjectCollider>();
                        mapCollider.obstacleLayer = obstacleLayerName;
                        objectsConfigured++;
                    }
                }
            }

            Debug.Log($"Se configuraron {objectsConfigured} objetos como obstáculos en el mapa");
        }

        [ContextMenu("Find Arrow Prefab")]
        public void FindArrowPrefab()
        {
            // Buscar el prefab de flecha en el proyecto
            #if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("3D arrow");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject arrow = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (arrow != null)
                {
                    arrowPrefab = arrow;
                    Debug.Log($"Arrow prefab encontrado: {path}");
                }
            }
            #endif
        }

        [ContextMenu("Add Boat Controllers")]
        public void AddBoatControllers()
        {
            Debug.Log("Agregando controladores de movimiento a los barcos...");

            VectorFieldRuntimeController controller = GetComponent<VectorFieldRuntimeController>();
            if (controller == null)
            {
                Debug.LogError("No hay VectorFieldRuntimeController en este objeto");
                return;
            }

            // Nombres comunes que podrían tener los barcos en la escena
            string[] boatNames = { "Boat", "Ship", "Barco" };
            int boatsConfigured = 0;

            foreach (string boatName in boatNames)
            {
                // Primero intentamos encontrar barcos por sus tags
                GameObject[] boats = GameObject.FindGameObjectsWithTag(boatName);
                
                if (boats.Length == 0)
                {
                    // Si no hay tags, buscamos por nombre en todos los objetos de la escena
                    GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains(boatName))
                        {
                            ConfigureBoat(obj, controller);
                            boatsConfigured++;
                        }
                    }
                }
                else
                {
                    foreach (GameObject boat in boats)
                    {
                        ConfigureBoat(boat, controller);
                        boatsConfigured++;
                    }
                }
            }

            Debug.Log($"Configurados {boatsConfigured} barcos");
        }

        private void ConfigureBoat(GameObject boat, VectorFieldRuntimeController controller)
        {
            // Agregamos el script que hace que el barco siga las corrientes del campo
            BoatVectorFollower follower = boat.GetComponent<BoatVectorFollower>();
            if (follower == null)
            {
                follower = boat.AddComponent<BoatVectorFollower>();
            }

            // Conectamos el barco con el controlador del campo vectorial
            follower.vectorField = controller;

            // Configuramos la física del barco para que flote y se mueva naturalmente
            Rigidbody rb = boat.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = boat.AddComponent<Rigidbody>();
                rb.useGravity = false;  // Los barcos flotan, no caen
                // Bloqueamos movimientos verticales y rotaciones no deseadas
                rb.constraints = RigidbodyConstraints.FreezePositionY | 
                                 RigidbodyConstraints.FreezeRotationX | 
                                 RigidbodyConstraints.FreezeRotationZ;
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualización en el editor: dibujamos el área del campo vectorial en color cyan
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(fieldCenter, new Vector3(fieldSize.x, 1f, fieldSize.y));

            // Dibujamos una esfera verde donde aparecerá el jugador al inicio
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(fieldCenter + new Vector3(0, 10f, 0), 1f);
        }
    }
}
