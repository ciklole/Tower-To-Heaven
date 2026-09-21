// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
// PLAYER CONTROLLER  /  CONTROLADOR DEL JUGADOR 
// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
//
// En: Main MonoBehaviour for the FPC CPP system. Handles movement, jump, body states, stamina,
//     camera, head bob, zoom, object interaction, slide, and dash.
//     Configurable parameters live in the PlayerConfiguration_ConfiguracionDelJugador ScriptableObject.
//     Field tooltips are available in the FPC_CPP_Window.
//
// Es: MonoBehaviour principal del sistema FPC CPP. Gestiona el movimiento, saltar, estados corporales,
//     resistencia, cámara, balanceo, zoom, interacción de objetos, deslizamiento y dash.
//     Los parámetros configurables estan en el SO PlayerConfiguration_ConfiguracionDelJugador.
//     Los tooltips de cada campo están en FPC_CPP_Window.
//
// En: Requires on the same GameObject:
// Es: Se requiere en el mismo GameObject:
//   · Rigidbody        
//   · CapsuleCollider  
//
// SECTIONS / SECCIONES:
//   01 · Initialization              / Inicialización      
//   02 · Update                      / Update              
//   03 · FixedUpdate                 / FixedUpdate         
//   04 · Ground Detection            / Detección del Suelo
//   05 · Gravity                     / Gravedad
//   06 · Movement                    / Movimiento
//   07 · Body States                 / Estados Corporales
//   08 · Collider Transitions        / Transiciónes del Collider
//   09 · Jump                        / Salto
//   10 · Stamina                     / Resistencia
//   11 · HUD                         / HUD
//   12 · Camera                      / Cámara
//   13 · Camera Height               / Altura de la Cámara
//   14 · Head Bob                    / Balanceo de Cabeza
//   15 · Zoom                        / Zoom
//   16 · Object Interaction          / Interacción De Objetos
//   17 · Slide                       / Deslizamiento
//   18 · Dash                        / Dash
//   19 · Input Layer                 / Capa del Input
//   20 · Public Utilities            / Utilidades Públicas
//
// ═══════════════════════════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FPC_CPP.Runtime;

namespace FPC_CPP.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController_ControladorDelJugador : MonoBehaviour
    {

        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region REFERENCIAS PÚBLICAS  /  PUBLIC REFERENCES
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━  REFERENCIAS  /  REFERENCES  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia del ScriptableObject 'PlayerConfiguration_ConfiguracionDelJugador'.\n\n" +
        "English:\n\n" +
        "Reference to the 'PlayerConfiguration_ConfiguracionDelJugador' ScriptableObject."
        )]
        public PlayerConfiguration_ConfiguracionDelJugador Configuration_Configuracion;

        [Space(5)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia al Transform donde se ubicará el 'CameraSupport_SoporteDeLaCamara'.\n\n" +
        "English:\n\n" +
        "Reference to the Transform where the 'CameraSupport_SoporteDeLaCamara' will be placed."
        )]
        public Transform CameraSupport_SoporteDeLaCamara;

        [Space(5)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia a la Cámara 'MainCamera_CamaraPrincipal'.\n\n" +
        "English:\n\n" +
        "Reference to the Camera 'MainCamera_CamaraPrincipal'."
        )]
        public Camera MainCamera_CamaraPrincipal;

        [Space(20)]
        [Header("━━━━━━━  HUD  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia a la Imagen de la 'StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia'.\n\n" +
        "English:\n\n" +
        "Reference to the Image of the 'StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia'."
        )]
        public Image StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia;

        [Space(5)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia a la Imagen de la 'CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual'.\n\n" +
        "English:\n\n" +
        "Reference to the Image of the 'CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual'."
        )]
        public Image CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual;

        [Space(5)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia al GameObject de la 'CrosshairOnHUD_CrosshairEnElHUD'.\n\n" +
        "English:\n\n" +
        "Reference to the GameObject of the 'CrosshairOnHUD_CrosshairEnElHUD'."
        )]
        public GameObject CrosshairOnHUD_CrosshairEnElHUD;

        [Space(20)]
        [Header("━━━━━━━  OBJETOS  /  OBJECTS  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        [Tooltip(
        "Español:\n\n" +
        "Referencia al Transform 'AnchorPointOfPickedObject_PuntoDeAnclajeDelObjetoRecogido' donde los Objetos se sostendrán.\n\n" +
        "English:\n\n" +
        "Reference to the Transform 'AnchorPointOfPickedObject_PuntoDeAnclajeDelObjetoRecogido' where the objects will be place."
        )]
        public Transform AnchorPointOfPickedObject_PuntoDeAnclajeDelObjetoRecogido;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region VARIABLES PRIVADAS  /  PRIVATE VARIABLES
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        // En: Full internal controller state: components, body state, ground, jump, sprint,
        //     stamina, camera, head bob, zoom, objects, slide, dash, analog triggers,
        //     HUD, and input device detection.
        // Es: Estado interno del controlador: componentes, estado corporal, suelo, salto,
        //     carrera, resistencia, cámara, balanceo, zoom, objetos, deslizamiento, dash,
        //     gatillos analógicos, HUD y detección del dispositivo del input.

        // ── Componentes / Components ─────────────────────────────────────────────────────────────────────────────


        // En: Player Rigidbody. Fetched in Awake and is the core of the physics system.
        // Es: Rigidbody del jugador. Se obtiene en el Awake y es el eje central del sistema de físicas.
        private Rigidbody _playerRigidbody;


        // En: Player CapsuleCollider. Its height and center are modified at runtime based on the body state.
        // Es: CapsuleCollider del jugador. Su altura y su centro se modifican en runtime según el estado corporal.
        private CapsuleCollider _playerCapsuleCollider;


        // En: Original radius of the CapsuleCollider. Read in Awake and used to restore the radius after prone state.
        // Es: Radio original del CapsuleCollider. Leído en el Awake y usado para restaurar el radio tras el estado acostado.
        private float _originalCapsuleRadius = 0.5f;


        // ── Estado Corporal / Body State ──────────────────────────────────────────────────────────────────────────


        // En: Enum with the four possible player body states.
        // Es: Enum con los cuatro posibles estados corporales del jugador.
        private enum BodyState { Standing, Crouching, Prone, InTheAir }


        // En: Currently active body state.
        // Es: Estado corporal activo en 'este' momento.
        private BodyState _currentBodyState = BodyState.Standing;


        // En: True if the player has explicitly chosen to stay crouched or prone via toggle input.
        //     Prevents the posture system from standing the player up automatically.
        // Es: True si el jugador eligió explícitamente quedarse agachado o acostado via input palanca.
        //     Evita que el sistema de postura levante al jugador automáticamente.
        private bool _playerWantsToStayDown = false;


        // En: Last body state recorded while the player was on the ground.
        //     Used to restore the correct posture upon landing.
        // Es: Último estado corporal que se registrá previo a que el jugador dejara el Suelo.
        //     Se usa para restaurar la postura correcta al aterrizar.
        private BodyState _lastBodyStateWhileOnGround = BodyState.Standing;


        // En: Timestamp of the last body state transition. Used to calculate the cooldown.
        // Es: Marca de tiempo de la última transición del estado corporal. Usada para poder calcular el cooldown.
        private float _timeOfLastBodyStateTransition = -999f;


        // En: Last time the crouch input was pressed. Resolves conflicts with the prone input.
        // Es: Última vez en la que se pulsó el input de agacharse. Resuelve los conflictos con la entrada de acostarse.
        private float _timestampOfLastCrouchInputPulse = -999f;


        // En: Last time the prone input was pressed. Resolves conflicts with the crouch input.
        // Es: Última vez que se pulsó el input de acostarse. Resuelve los conflictos con el input de agacharse.
        private float _timestampOfLastProneInputPulse = -999f;


        // En: True if the crouch toggle mode is active (only relevant when HoldToCrouch = false).
        // Es: True si el modo palanca de agacharse está activo (solo relevante cuando HoldToCrouch = false).
        private bool _crouchToggleModeIsActive = false;


        // En: True if the prone toggle mode is active (only relevant when HoldToProne = false).
        // Es: True si el modo palanca de acostarse está activo (solo relevante cuando HoldToProne = false).
        private bool _proneToggleModeIsActive = false;


        // En: Timer that 'accumulates' how long the crouch/prone gamepad button has been held.
        //     Used to distinguish a short press (crouch) from a long press (prone).
        // Es: Temporizador que 'recopila' el tiempo que lleva pulsado, en mando, el botón de agacharse/acostarseo.
        //     Se usa para distinguir un pulsado corto (agacharse) de un pulsado largo (acostarse).
        private float _buttonHoldTimeForCrouchOrProneOnGamepad = 0f;


        // En: True when the crouch/prone gamepad button has already been interpreted as prone
        //     in the current frame, preventing it from also being processed as crouch.
        // Es: True cuando en mando el botón de agacharse/acostarse en ya fue interpretado como acostarse
        //     en el frame actual, evitando que también se procese como agacharse.
        private bool _gamepadButtonAlreadyInterpretedAsProne = false;


        // ── Suelo / Ground ────────────────────────────────────────────────────────────────────────────────────


        // En: True if the player is currently in contact with the ground according to the OverlapSphere.
        // Es: True si el jugador está en contacto con el suelo según el OverlapSphere.
        private bool _playerIsOnGround = false;


        // En: Ground mask actually used at runtime. Built from LAYER NAMES (not indices) so it works
        //     in any project, and the player's own layer is always excluded to avoid self-detection.
        // Es: Máscara de suelo que se usa realmente en runtime. Se construye a partir de los NOMBRES de
        //     las capas (no de los índices) para que funcione en cualquier proyecto, y siempre excluye
        //     la capa del propio jugador para evitar la auto-detección.
        private int _effectiveGroundMask;


        // En: Ground contact state from the previous frame.
        //     Compared with the current state to detect the exact landing moment.
        // Es: Estado del contacto con el suelo en el frame anterior.
        //     Se compara con el estado actual para detectar el momento exacto del aterrizaje.
        private bool _playerWasOnGroundLastFrame = false;


        // ── Salto / Jump ────────────────────────────────────────────────────────────────────────────────────


        // En: Remaining jumps before the player needs to touch the ground again.
        // Es: Saltos restantes antes de que el jugador necesite tocar el suelo de nuevo.
        private int _remainingJumps;


        // En: Remaining Coyote Time after leaving the ground without intentionally jumping.
        // Es: Tiempo restante del Coyote Time tras abandonar el suelo sin haber saltado.
        private float _remainingCoyoteTime = 0f;


        // En: Remaining jump buffer time. If it reaches zero before touching the ground, the input is discarded.
        // Es: Tiempo restante del buffer de salto. Si llega a cero antes de tocar suelo, el input recibido se descarta.
        private float _remainingJumpBufferTime = 0f;


        // En: True while the player is airborne as a direct result of executing a jump.
        // Es: True mientras el jugador esté en el aire como consecuencia directa de haber ejecutado un salto.
        private bool _playerIsPerformingJump = false;


        // En: Accumulated time the player has been holding the jump input.
        //     Used for the variable jump mechanic.
        // Es: Tiempo acumulado que el jugador lleva manteniendo pulsado el input de saltar.
        //     Se usa para la mecánica del salto variable.
        private float _jumpInputHoldTime = 0f;


        // En: Movement direction at the moment of leaving the ground.
        //     Used when 'AllowThePlayerToMoveInTheAir' is False.
        // Es: Dirección de movimiento cuando se deja el suelo.
        //     Se usa cuando 'AllowThePlayerToMoveInTheAir' está en False.
        private Vector3 _movementDirectionAtTakeoff = Vector3.zero;


        // En: True when the system is waiting for the CapsuleCollider to reach standing height
        //     before executing a jump from the prone state.
        // Es: True cuando el sistema está esperando que el CapsuleCollider alcance la altura de pie
        //     antes de ejecutar un salto ejecutado desde el estado acostado.
        private bool _systemWaitingForTransitionToPerformJump = false;


        // En: Reference to the active stand-up-then-jump coroutine from the prone state.
        //     Stored so it can be cancelled if the jump is aborted before executing.
        // Es: Referencia a la corrutina activa de la transición-salto desde el estado acostado.
        //     Se guarda para poder cancelarla si el salto se cancela antes de ejecutarse.
        private Coroutine _proneJumpCoroutineActive;


        // ── Correr / Run ───────────────────────────────────────────────────────────────────────────────────


        // En: True if the sprint toggle mode is active (only relevant when 'HoldToRun' = false).
        // Es: True si el modo palanca de correr está activo (solo relevante cuando 'HoldToRun' = false).
        private bool _runToggleModeIsActive = false;

        // En: Movement direction cached in Update and consumed in FixedUpdate.
        //     Ensures both rotation and physics use the same direction snapshot per frame.
        // Es: Dirección de movimiento cacheada en Update y consumida en FixedUpdate.
        //     Garantiza que la rotación y la física usen el mismo snapshot de dirección por frame.
        private Vector3 _cachedMovementDirection = Vector3.zero;


        // ── Resistencia / Stamina ──────────────────────────────────────────────────────────────────────────────


        // En: Player's current stamina in real time.
        //     Accessible from other scripts via GetNormalizedStamina() or ModifyStamina().
        // Es: Resistencia actual del jugador, en tiempo real.
        //     Accesible desde otros scripts a través de 'ObtenerLaResistenciaNormalizada()' o 'ModifyStamina()'.
        [HideInInspector] public float PlayerCurrentStamina;


        // En: Time elapsed without spending stamina. Compared against the configured regeneration delay.
        // Es: Tiempo transcurrido sin haber gastado resistencia. Se compara con el delay de la regeneración de resistencia configurada.
        private float _timeElapsedWithoutSpendingStamina = 0f;


        // ── Cámara / Camera ───────────────────────────────────────────────────────────────────────────────────


        // En: Accumulated vertical camera angle in degrees. Positive = looking down.
        // Es: Ángulo vertical acumulado, de la cámara, en grados. Positivo = mirar hacia abajo.
        private float _cameraAccumulatedVerticalRotationAngle = 0f;


        // En: Accumulated horizontal rotation angle in degrees. Used to rotate the player body via MoveRotation in FixedUpdate.
        // Es: Ángulo de rotación horizontal acumulado en grados. Se usa para rotar el cuerpo del jugador via MoveRotation en FixedUpdate.
        //private float _accumulatedYRotation = 0f;


        // En: Original Y position of the CameraSupport_SoporteDeLaCamara read in Awake. Used as the base reference for head bob.
        // Es: Posición Y original del CameraSupport_SoporteDeLaCamara leída en el Awake. Usada como la referencia base para el balanceo.
        private float _originalYPositionOfCameraSupport = 0f;


        // ── Balanceo de Cabeza / Head Bob ───────────────────────────────────────────────────────────────────────


        // En: Internal continuous bob timer. Advances with Time.deltaTime and feeds the sine function.
        // Es: Temporizador interno del balanceo continuo. Avanza con Time.deltaTime y alimenta la función seno.
        private float _continuousBobbingTimer = 0f;


        // En: Continuous bob offset (walk/run) applied to the CameraSupport_SoporteDeLaCamara every frame.
        // Es: Desplazamiento del balanceo continuo (caminar/correr) aplicado al CameraSupport_SoporteDeLaCamara en cada frame.
        private Vector3 _continuousBobbingOffset = Vector3.zero;


        // En: Discrete reactive bob impulse (jump, land, postures) that damps with Lerp toward zero.
        // Es: Impulso puntual del balanceo reactivo (salto, aterrizaje, posturas) que se 'amortigua' con Lerp hasta cero.
        private Vector3 _reactiveBobbingImpulse = Vector3.zero;


        // En: Internal timer that advances the breathing cycle. Resets modulo 1 each full breath.
        // Es: Temporizador interno que avanza el ciclo de respiración. Se reinicia el módulo 1 en cada respiración completa.
        private float _breathingTimer = 0f;


        // En: Smoothed exhaustion value between 0 and 1. Drives breathing intensity and frequency.
        // Es: Valor de agotamiento suavizado entre 0 y 1. Controla la intensidad y frecuencia de la respiración.
        private float _smoothedExhaustionInfluence = 0f;


        // En: Current breathing pitch offset applied to the camera vertical rotation.
        // Es: Desplazamiento del pitch de respiración actual aplicado a la rotación vertical de la cámara.
        private float _breathingPitchOffset = 0f;


        // En: Current breathing translation offset applied to the camera Y position.
        // Es: Desplazamiento de traslación de respiración actual aplicado a la posición Y de la cámara.
        private float _breathingTranslationOffset = 0f;


        // ── Zoom ─────────────────────────────────────────────────────────────────────────────────────


        // En: True if zoom is currently active.
        // Es: True si el zoom está activo en 'este' momento.
        private bool _zoomIsActive = false;


        // En: True if the zoom toggle mode is active (only relevant when 'HoldToZoom' = false).
        // Es: True si el modo palanca del zoom está activo (solo relevante cuando 'HoldToZoom' = false).
        private bool _zoomToggleModeIsActive = false;


        // En: Current interpolated FOV in real time. Applied to MainCamera_CamaraPrincipal every frame.
        // Es: FOV actual, interpolado, en tiempo real. Se aplica a la MainCamera_CamaraPrincipal en cada frame.
        private float _currentCameraFieldOfView = 75f;


        // ── Interacción de Objetos / Object Interaction ───────────────────────────────────────────────────────────────────

        private Vector3 _swayOffset = Vector3.zero;
        private Vector3 _swayVelocity = Vector3.zero;
        private bool _objectReachingAnchor = false;



        // En: Reference to the object the player currently holds. Null if none.
        // Es: Referencia al objeto que el jugador tiene en la mano actualmente. Null si no hay ninguno.
        private GameObject _currentlyHeldObject = null;


        // En: Collider of the held object. Stored so it can be re-enabled on drop.
        // Es: Collider del objeto recogido. Se guarda para poder reactivarlo al soltar el Objeto.
        private Collider _heldObjectCollider = null;


        // En: True if the object rotation mode is active.
        // Es: True si el modo de rotación del objeto está activo.
        private bool _objectRotationModeIsActive = false;


        // En: True if the throw input is currently being held.
        // Es: True si el input de lanzar está siendo presionado actualmente.
        private bool _throwInputIsBeingHeld = false;


        // En: Accumulated time the throw input has been held to charge throw force.
        // Es: Tiempo acumulado en el input de lanzar para cargar la fuerza de lanzamiento.
        private float _throwInputHoldTime = 0f;


        // ── Deslizamiento / Slide ────────────────────────────────────────────────────────────────────────────


        // En: True if the player is currently sliding.
        // Es: True si el jugador se está deslizándose en este momento.
        private bool _playerIsSliding = false;


        // En: Time elapsed since the current slide began. Used in arcade mode.
        // Es: Tiempo transcurrido desde que empezó el deslizamiento actual. Usado en modo arcade.
        private float _timeSinceSlidingStarted = 0f;


        // En: Slide direction at start. Frozen on begin to prevent mid-slide direction changes.
        // Es: Dirección del deslizamiento al inicio. Se congela al comenzar para no permitir cambio de dirección en el caso de que eso se quiera.
        private Vector3 _slideDirectionAtStart = Vector3.zero;


        // En: True while the player is in the post-slide recovery period.
        // Es: True mientras el jugador está en el período de recuperación tras terminar un deslizamiento.
        private bool _playerIsInPostSlideRecovery = false;

        // En: Cached result of InputToInterruptSliding(), read in Update and consumed in FixedUpdate.
        //     GetKeyDown is only reliable in Update, not in FixedUpdate.
        // Es: Resultado cacheado de InputToInterruptSliding(), leído en Update y consumido en FixedUpdate.
        //     GetKeyDown solo es confiable en Update, no en FixedUpdate.
        private bool _interruptSlideInputCached = false;

        // En: Remaining time of the post-slide recovery period.
        // Es: Tiempo restante del período de recuperación post-deslizamiento.
        private float _remainingPostSlideRecoveryTime = 0f;


        // ── Dash ─────────────────────────────────────────────────────────────────────────────────────


        // En: True if a dash is currently being executed.
        // Es: True si el dash está siendo ejecutado en 'este' momento.
        private bool _playerIsPerformingDash = false;


        // En: Time elapsed since the last dash. Compared against the configured cooldown.
        // Es: Tiempo transcurrido desde el último dash ejecutado. Se compara con el tiempo configurado en el cooldown
        private float _timeSinceLastDash = 999f;


        // En: Timestamp of the first dash button press. Used to detect the double press.
        // Es: Marca de tiempo del primer presionado del input del dash. Se usa para detectar el doble presionado.
        private float _timestampOfFirstDashButtonPress = -999f;


        // En: True if the first press of the dash double-press has been registered and the second is awaited.
        // Es: True si ya se registró el primer presionado del doble presionado de dash y se espera el segundo.
        private bool _waitingForSecondDashButtonPress = false;


        // ── Gatillos Analógicos (LT / RT) / Analogic Triggers (LT / RT) ───────────────────────────────────────────────────────────


        // En: Current value of the LT axis (left trigger). Read every frame in DetectGamepadTriggers().
        // Es: Valor actual del eje LT (gatillo izquierdo). Leído en cada frame en DetectGamepadTriggers().
        private float _currentLeftTriggerValue = 0f;


        // En: Current value of the RT axis (right trigger). Read every frame in DetectGamepadTriggers().
        // Es: Valor actual del eje RT (gatillo derecho). Leído en cada frame en DetectGamepadTriggers().
        private float _currentRightTriggerValue = 0f;


        // En: True if LT exceeded the activation threshold in the current frame.
        // Es: True si LT superó el umbral de activación en el frame actual.
        private bool _LTIsActive = false;


        // En: True if RT exceeded the activation threshold in the current frame.
        // Es: True si RT superó el umbral de activación en el frame actual.
        private bool _RTIsActive = false;


        // En: True if LT went from inactive to active 'this' frame (equivalent to GetButtonDown).
        // Es: True si LT para el Zoom pasó de inactivo a activo en 'este' frame (equivalente a GetButtonDown).
        private bool _LTZoomPressedThisFrame = false;


        // En: True if RT went from active to inactive 'this' frame (equivalent to GetButtonUp).
        // Es: True si RT pasó de activo a inactivo en 'este' frame (equivalente a GetButtonUp).
        private bool _RTReleasedThisFrame = false;


        // En: LT state from the previous frame, used for edge detection.
        // Es: Estado de LT en el frame anterior para detectar flancos.
        private bool _LTWasActiveLastFrame = false;


        // En: RT state from the previous frame, used for edge detection.
        // Es: Estado de RT en el frame anterior para detectar flancos.
        private bool _RTWasActiveLastFrame = false;


        // ── HUD ──────────────────────────────────────────────────────────────────────────────────────


        // En: Original pixel width of the stamina bar. Read in Start as the fill reference.
        // Es: Ancho original en píxeles de la barra de resistencia. Leído en el Start como referencia para el fill.
        private float _originalWidthInPixelsOfStaminaBar = -1f;


        // ── Input ────────────────────────────────────────────────────────────────────────────────────


        // En: True if the currently active device is a gamepad.
        //     Detected automatically by comparing joystick input against keyboard/mouse input.
        // Es: True si el dispositivo activo en este momento es un mando.
        //     Se detecta automáticamente comparando si hay input del joystick o de teclado/ratón.
        private bool _isUsingGamepad = false;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 01 · INICIALIZACIÓN  /  INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        // En: Awake — runs before Start, even if the object is disabled.
        //     Initializes references and values that other scripts might need immediately.
        // Es: Awake - se ejecuta antes que el Start, incluso si el objeto donde está el Script está desactivado.
        //     Inicializa referencias y valores que otros scripts podrían necesitar inmediatamente.
        private void Awake()
        {

            _playerRigidbody = GetComponent<Rigidbody>();
            _playerCapsuleCollider = GetComponent<CapsuleCollider>();
            _originalCapsuleRadius = _playerCapsuleCollider.radius;

            _playerRigidbody.freezeRotation = true;

            _playerRigidbody.interpolation = RigidbodyInterpolation.None;

            PlayerCurrentStamina = Configuration_Configuracion.MaximumPlayerStamina;

            if (CameraSupport_SoporteDeLaCamara != null)
                _originalYPositionOfCameraSupport = CameraSupport_SoporteDeLaCamara.localPosition.y;

            if (Configuration_Configuracion != null)
            {
                Configuration_Configuracion.HeightOfTheCameraSupportWhileStanding = _originalYPositionOfCameraSupport;
                Configuration_Configuracion.CapsuleColliderHeightWhileStanding = _playerCapsuleCollider.height;
            }

            if (MainCamera_CamaraPrincipal != null && Configuration_Configuracion != null)
            {
                _currentCameraFieldOfView = Configuration_Configuracion.BaseFieldOfViewOfTheCamera;
                MainCamera_CamaraPrincipal.fieldOfView = _currentCameraFieldOfView;
            }

            _playerWasOnGroundLastFrame = true;
            _playerIsOnGround = true;
            _remainingJumps = Configuration_Configuracion.HowManyJumps;

            // En: Build the ground mask by name + exclude the player's own layer (portable across projects).
            // Es: Construye la máscara de suelo por nombre + excluye la capa del propio jugador (portable entre proyectos).
            BuildEffectiveGroundMask();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            if (Configuration_Configuracion == null)
            {
                Debug.LogError("[PlayerController] No PlayerConfiguration assigned. The controller will not work.");
                enabled = false;
                return;
            }

            UpdateCrosshairVisibility();
            UpdateStaminaHUD();

            if (CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual != null)
                _originalWidthInPixelsOfStaminaBar = CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual.rectTransform.sizeDelta.x;

            _timeSinceLastDash = Configuration_Configuracion.CooldownBetweenDashUses + 1f;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 02 · UPDATE  /  UPDATE
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void Update()
        {
            if (Configuration_Configuracion == null) return;

            DetectActiveInputDevice();
            DetectGamepadTriggers();

            ReadCameraInput();
            ReadBodyStateInput();
            ReadJumpInput();
            ReadRunInput();
            ReadZoomInput();
            ReadObjectInteractionInput();
            ReadDashInput();

            if (_playerIsSliding && Configuration_Configuracion.AllowInterruptingSliding && _timeSinceSlidingStarted > 0f)
                _interruptSlideInputCached = _interruptSlideInputCached || InputToInterruptSliding();

            UpdateCoyoteTimeTimer();
            UpdateJumpBufferTimer();
            UpdateDashCooldownTimer();

            UpdatePostSlideRecovery();
            UpdateCapsuleColliderTransition();
            UpdateCameraHeight();
            UpdateHeadBobbing();
            UpdateZoom();
            UpdateStaminaHUD();

            if (MainCamera_CamaraPrincipal != null)
                MainCamera_CamaraPrincipal.fieldOfView = _currentCameraFieldOfView;

            EnforceCanStandUp();
            _cachedMovementDirection = GetMovementDirection();
        }


        private void LateUpdate()
        {
            UpdateHeldObjectPosition();
        }


        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 03 · FIXED UPDATE  /  FIXED UPDATE
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void FixedUpdate()
        {
            if (Configuration_Configuracion == null) return;

            GroundDetection();
            ApplyGravity();
            ApplyMovement();
            ApplyJump();
            ExecuteSliding();
            UpdateStamina();
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 04 · DETECCIÓN DEL SUELO  /  GROUND DETECTION
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // En: Names of the layers treated as ground. Resolved at runtime so the mask is correct in ANY
        //     project regardless of layer indices. Add your own layer names here if you need more
        //     (e.g. "Terrain", "StaticGeometry"). Do NOT add the player's layer.
        //     Public so the editor window can reuse the SAME list when re-resolving the mask.
        // Es: Nombres de las capas consideradas suelo. Se resuelven en runtime para que la máscara sea
        //     correcta en CUALQUIER proyecto sin importar los índices. Añade aquí tus propios nombres de
        //     capa si necesitas más (p. ej. "Terrain", "StaticGeometry"). NO añadas la capa del jugador.
        //     Es público para que la ventana del editor reutilice la MISMA lista al re-resolver la máscara.
        public static readonly string[] GroundLayerNames = { "Ground", "Suelo" };

        // En: Rebuilds the runtime ground mask. Call again if you change the player's layer at runtime.
        // Es: Reconstruye la máscara de suelo en runtime. Vuelve a llamarla si cambias la capa del jugador en runtime.
        public void BuildEffectiveGroundMask()
        {
            int mask = 0;

            // En: 1) Rebuild from layer NAMES → portable across projects.
            // Es: 1) Reconstruir a partir de los NOMBRES de capa → portable entre proyectos.
            foreach (string layerName in GroundLayerNames)
            {
                int layerIndex = LayerMask.NameToLayer(layerName);
                if (layerIndex != -1) mask |= 1 << layerIndex;
            }

            // En: 2) If no named ground layer exists, fall back to the serialized LayerMask.
            // Es: 2) Si no existe ninguna capa de suelo con esos nombres, usar el LayerMask serializado.
            if (mask == 0 && Configuration_Configuracion != null)
                mask = Configuration_Configuracion.LayersThatAreConsideredGround.value;

            // En: 3) ALWAYS exclude the player's own layer (prevents self-detection that breaks jumping).
            // Es: 3) SIEMPRE excluir la capa del propio jugador (evita la auto-detección que rompe el salto).
            mask &= ~(1 << gameObject.layer);

            _effectiveGroundMask = mask;

            if (_effectiveGroundMask == 0)
                Debug.LogWarning("[PlayerController] No se encontró ninguna capa de suelo válida. " +
                    "Crea una capa llamada \"Ground\" y asigna tu piso/terreno a ella, " +
                    "o añade su nombre al array GroundLayerNames. / No valid ground layer found.", this);
        }


        private void GroundDetection()
        {
            _playerWasOnGroundLastFrame = _playerIsOnGround;

            Vector3 detectionPoint = transform.position + Vector3.down * Configuration_Configuracion.DownwardOffsetOfTheGroundDetectionOverlapSphere;
            _playerIsOnGround = Physics.CheckSphere(detectionPoint, Configuration_Configuracion.RadiusOfTheGroundDetectionOverlapSphere, _effectiveGroundMask, QueryTriggerInteraction.Ignore);

            if (_playerIsOnGround && !_playerWasOnGroundLastFrame)
                OnLanding();

            if (!_playerIsOnGround && _playerWasOnGroundLastFrame && !_playerIsPerformingJump)
                _remainingCoyoteTime = Configuration_Configuracion.EnableCoyoteTime ? Configuration_Configuracion.DurationOfTheCoyoteTime : 0f;
        }

        private void OnLanding()
        {
            _remainingJumps = Configuration_Configuracion.HowManyJumps;
            _playerIsPerformingJump = false;
            _jumpInputHoldTime = 0f;
            _systemWaitingForTransitionToPerformJump = false;

            if (_playerIsSliding) CancelSliding();

            if (Configuration_Configuracion.EnableTheHeadBobbingSystem && Configuration_Configuracion.EnableReactiveHeadBobbingWhenJumpingAndLanding)
                _reactiveBobbingImpulse = Vector3.down * Configuration_Configuracion.IntensityOfReactiveHeadBobbingWhenLanding;

            RestoreBodyStateOnLanding();
        }

        private void RestoreBodyStateOnLanding()
        {
            bool volverAAgachado = Configuration_Configuracion.HoldToCrouch ? InputForCrouch() : (_crouchToggleModeIsActive && _lastBodyStateWhileOnGround == BodyState.Crouching);

            bool volverAAcostado = Configuration_Configuracion.HoldToProne ? InputForProne() : (_proneToggleModeIsActive && _lastBodyStateWhileOnGround == BodyState.Prone);

            if (volverAAcostado && Configuration_Configuracion.AllowThePlayerToGoProne)
                ChangeBodyState(BodyState.Prone, false);
            else if (volverAAgachado && Configuration_Configuracion.AllowThePlayerToCrouch)
                ChangeBodyState(BodyState.Crouching, false);
            else
                ChangeBodyState(BodyState.Standing, false);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 05 · GRAVEDAD  /  GRAVITY
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ApplyGravity()
        {
            if (_playerIsOnGround) return;

            if (_playerRigidbody.linearVelocity.y < 0f)
                _playerRigidbody.AddForce(Physics.gravity * (Configuration_Configuracion.GravityMultiplierDuringTheJump * Configuration_Configuracion.AdditionalGravityMultiplierDuringTheFall - 1f), ForceMode.Acceleration);
            else
                _playerRigidbody.AddForce(Physics.gravity * (Configuration_Configuracion.GravityMultiplierDuringTheJump - 1f), ForceMode.Acceleration);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 06 · MOVIMIENTO  /  MOVEMENT
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ApplyMovement()
        {

            if (!Configuration_Configuracion.AllowThePlayerToWalk) return;
            if (_playerIsSliding) return;
            if (_playerIsPerformingDash) return;

            Vector3 movementDirection = _cachedMovementDirection;
            float finalSpeed = CalculateMovementSpeed(movementDirection);

            float velocityY = (_playerIsOnGround && !_playerIsPerformingJump) ? Mathf.Min(_playerRigidbody.linearVelocity.y, 0f) : _playerRigidbody.linearVelocity.y;

            _playerRigidbody.linearVelocity = new Vector3(movementDirection.x * finalSpeed, velocityY, movementDirection.z * finalSpeed);

            if (_playerIsOnGround)
                _movementDirectionAtTakeoff = movementDirection;

        }

        private Vector3 GetMovementDirection()
        {
            if (!_playerIsOnGround && !Configuration_Configuracion.AllowThePlayerToMoveInTheAir)
                return _movementDirectionAtTakeoff;

            float inputHorizontal = 0f;
            float inputVertical = 0f;

            if (_isUsingGamepad)
            {
                inputHorizontal = Input.GetAxisRaw("Horizontal");
                inputVertical = Input.GetAxisRaw("Vertical");
                if (Mathf.Abs(inputHorizontal) < Configuration_Configuracion.LeftStickDeadZone) inputHorizontal = 0f;
                if (Mathf.Abs(inputVertical) < Configuration_Configuracion.LeftStickDeadZone) inputVertical = 0f;
            }
            else
            {
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveForward)) inputVertical += 1f;
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveBackward)) inputVertical -= 1f;
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveRight)) inputHorizontal += 1f;
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveLeft)) inputHorizontal -= 1f;
            }

            if (inputHorizontal == 0f && inputVertical == 0f) return Vector3.zero;

            Vector3 direction = transform.right * inputHorizontal + transform.forward * inputVertical;
            direction.y = 0f;
            return direction.normalized;
        }

        private float CalculateMovementSpeed(Vector3 movementDirection)
        {
            if (movementDirection == Vector3.zero) return 0f;

            bool playerIsRunning = ThePlayerIsRunningNow();
            float baseSpeed = Configuration_Configuracion.BaseSpeedOfThePlayer * (playerIsRunning ? Configuration_Configuracion.SpeedMultiplierWhileRunning : 1f);

            if (!Configuration_Configuracion.EnableOmniDirectionalMovement) return baseSpeed;

            float dotForward = Vector3.Dot(transform.forward, movementDirection);
            float dotLateral = Mathf.Abs(Vector3.Dot(transform.right, movementDirection));

            float directionalMultiplier;
            float lateralMultiplier;

            switch (_currentBodyState)
            {
                case BodyState.Crouching:
                    directionalMultiplier = dotForward >= 0f ? Configuration_Configuracion.ForwardOmnidirectionalMultiplierWhileCrouching : Configuration_Configuracion.BackwardOmnidirectionalMultiplierWhileCrouching;
                    lateralMultiplier = Configuration_Configuracion.LateralOmnidirectionalMultiplierWhileCrouching;
                    break;
                case BodyState.Prone:
                    directionalMultiplier = dotForward >= 0f ? Configuration_Configuracion.ForwardOmnidirectionalMultiplierWhileProne : Configuration_Configuracion.BackwardOmnidirectionalMultiplierWhileProne;
                    lateralMultiplier = Configuration_Configuracion.LateralOmnidirectionalMultiplierWhileProne;
                    break;
                case BodyState.InTheAir:
                    directionalMultiplier = dotForward >= 0f ? Configuration_Configuracion.ForwardOmnidirectionalMultiplierWhileInTheAir : Configuration_Configuracion.BackwardOmnidirectionalMultiplierWhileInTheAir;
                    lateralMultiplier = Configuration_Configuracion.LateralOmnidirectionalMultiplierWhileInTheAir;
                    break;
                default:
                    directionalMultiplier = dotForward >= 0f ? Configuration_Configuracion.ForwardOmnidirectionalMultiplierWhileStanding : Configuration_Configuracion.BackwardOmnidirectionalMultiplierWhileStanding;
                    lateralMultiplier = Configuration_Configuracion.LateralOmnidirectionalMultiplierWhileStanding;
                    break;
            }

            return baseSpeed * Mathf.Lerp(directionalMultiplier, lateralMultiplier, dotLateral);
        }

        private bool ThePlayerIsRunningNow()
        {
            if (_playerIsSliding) return false;

            if (_playerIsInPostSlideRecovery && !Configuration_Configuracion.AllowRunningImmediatelyAfterSliding)
                return false;

            if (!Configuration_Configuracion.AllowThePlayerToRun) return false;
            if (Configuration_Configuracion.EnableTheStaminaSystem && Configuration_Configuracion.EnableStaminaCostWhenRunning && PlayerCurrentStamina <= 0f) return false;

            if (_currentBodyState == BodyState.Crouching && !Configuration_Configuracion.AllowThePlayerToRunWhileCrouching) return false;
            if (_currentBodyState == BodyState.Prone && !Configuration_Configuracion.AllowThePlayerToRunWhileProne) return false;

            return _runToggleModeIsActive || RunInputHeld();
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 07 · ESTADOS CORPORALES  /  BODY STATES
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ReadBodyStateInput()
        {
            if (_currentBodyState == BodyState.InTheAir) return;

            if (_playerIsSliding || _playerIsInPostSlideRecovery) return;

            bool tryCrouch = false;
            bool tryProne = false;

            if (_isUsingGamepad)
                ProcessGamepadBodyStateInput(ref tryCrouch, ref tryProne);
            else
                ProcessKeyboardBodyStateInput(ref tryCrouch, ref tryProne);


            if (tryCrouch && tryProne)
            {
                if (_timestampOfLastProneInputPulse >= _timestampOfLastCrouchInputPulse)
                    tryCrouch = false;
                else
                    tryProne = false;
            }

            if (tryProne && Configuration_Configuracion.AllowThePlayerToGoProne)
            {
                if (_currentBodyState != BodyState.Prone)
                    ChangeBodyState(BodyState.Prone, true);
            }
            else if (tryCrouch && Configuration_Configuracion.AllowThePlayerToCrouch)
            {
                if (_currentBodyState != BodyState.Crouching)
                    ChangeBodyState(BodyState.Crouching, true);
            }
            else if (!tryCrouch && !tryProne)
            {
                if (_playerIsSliding || _playerIsInPostSlideRecovery) return;
                if (_playerWantsToStayDown) return;

                if (_currentBodyState == BodyState.Prone)
                {
                    ChangeBodyState(BodyState.Crouching, false);
                }
                else if (_currentBodyState == BodyState.Crouching && CanStandUp())
                {
                    _crouchToggleModeIsActive = false;
                    _proneToggleModeIsActive = false;
                    ChangeBodyState(BodyState.Standing, false);
                }
            }
        }

        private void ProcessKeyboardBodyStateInput(ref bool tryCrouch, ref bool tryProne)
        {

            if (!Configuration_Configuracion.HoldToCrouch && Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToCrouch))
            {
                if (ThePlayerIsRunningNow() && Configuration_Configuracion.EnableTheSlidingSystem && _playerIsOnGround)
                {
                    StartSliding();
                    return;
                }

                _timestampOfLastCrouchInputPulse = Time.time;
                if (_currentBodyState == BodyState.Crouching)
                {
                    _crouchToggleModeIsActive = false;
                    _playerWantsToStayDown = false;
                }
                else
                { _crouchToggleModeIsActive = true; tryCrouch = true; _playerWantsToStayDown = true; }
            }
            else if (Configuration_Configuracion.HoldToCrouch)
            {
                if (Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToCrouch) && ThePlayerIsRunningNow() && Configuration_Configuracion.EnableTheSlidingSystem && _playerIsOnGround)
                {
                    StartSliding();
                    return;
                }

                tryCrouch = Input.GetKey(Configuration_Configuracion.KeyboardKeyToCrouch);
                _playerWantsToStayDown = tryCrouch;
            }
            else
                tryCrouch = _crouchToggleModeIsActive && _currentBodyState == BodyState.Crouching;

            if (Configuration_Configuracion.HoldToProne)
            {
                tryProne = Input.GetKey(Configuration_Configuracion.KeyboardKeyToGoProne);
            }
            else if (Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToGoProne))
            {
                _timestampOfLastProneInputPulse = Time.time;
                if (_currentBodyState == BodyState.Prone)
                {
                    if (!CanCrouch()) { }
                    else
                    {
                        _proneToggleModeIsActive = false;
                        _playerWantsToStayDown = false;
                    }
                }
                else
                { _proneToggleModeIsActive = true; tryProne = true; _playerWantsToStayDown = true; }
            }
            else
                tryProne = _proneToggleModeIsActive && _currentBodyState == BodyState.Prone;
        }

        private void ProcessGamepadBodyStateInput(ref bool tryCrouch, ref bool tryProne)
        {

            bool buttonHeld = Input.GetKey(Configuration_Configuracion.GamepadButtonToCrouchAndGoProne);
            bool buttonPressed = Input.GetKeyDown(Configuration_Configuracion.GamepadButtonToCrouchAndGoProne);
            bool buttonReleased = Input.GetKeyUp(Configuration_Configuracion.GamepadButtonToCrouchAndGoProne);

            if (buttonHeld)
                _buttonHoldTimeForCrouchOrProneOnGamepad += Time.deltaTime;

            if (buttonHeld && _buttonHoldTimeForCrouchOrProneOnGamepad >= Configuration_Configuracion.ButtonHoldTimeToBeConsideredAsProne && !_gamepadButtonAlreadyInterpretedAsProne)
            {
                _gamepadButtonAlreadyInterpretedAsProne = true;
                _timestampOfLastProneInputPulse = Time.time;

                if (Configuration_Configuracion.HoldToProne)
                    tryProne = true;
                else
                {
                    if (_currentBodyState == BodyState.Prone)
                    {
                        _proneToggleModeIsActive = false;
                        _playerWantsToStayDown = false;
                    }
                    else { _proneToggleModeIsActive = true; tryProne = true; }
                }
            }

            if (Configuration_Configuracion.HoldToProne && buttonHeld && _gamepadButtonAlreadyInterpretedAsProne)
                tryProne = true;

            if (buttonReleased)
            {
                if (!_gamepadButtonAlreadyInterpretedAsProne)
                {

                    if (ThePlayerIsRunningNow() && Configuration_Configuracion.EnableTheSlidingSystem && _playerIsOnGround)
                    {
                        StartSliding();
                    }
                    else
                    {
                        _timestampOfLastCrouchInputPulse = Time.time;
                        if (Configuration_Configuracion.HoldToCrouch)
                        {
                            // En: Do nothing
                            // Es: No hacer nada
                        }
                        else
                        {
                            if (_currentBodyState == BodyState.Crouching)
                            {
                                _crouchToggleModeIsActive = false;
                                _playerWantsToStayDown = false;
                            }
                            else { _crouchToggleModeIsActive = true; tryCrouch = true; }
                        }
                    }
                }

                _buttonHoldTimeForCrouchOrProneOnGamepad = 0f;
                _gamepadButtonAlreadyInterpretedAsProne = false;
            }

            if (Configuration_Configuracion.HoldToCrouch && buttonHeld && !_gamepadButtonAlreadyInterpretedAsProne)
                tryCrouch = true;

            if (!Configuration_Configuracion.HoldToCrouch)
                tryCrouch = tryCrouch || (_crouchToggleModeIsActive && _currentBodyState == BodyState.Crouching);
            if (!Configuration_Configuracion.HoldToProne)
                tryProne = tryProne || (_proneToggleModeIsActive && _currentBodyState == BodyState.Prone);
        }

        private void ChangeBodyState(BodyState newState, bool voluntaryChange)
        {
            if (newState == BodyState.Standing && !CanStandUp()) return;

            if (Configuration_Configuracion.EnableCooldownBetweenBodyStateTransitions && voluntaryChange)
            {
                if (Time.time - _timeOfLastBodyStateTransition < Configuration_Configuracion.BodyStateTransitionCooldownTime) return;
                if (Configuration_Configuracion.EnableStaminaCostOnEachPostureTransition && Configuration_Configuracion.EnableTheStaminaSystem)
                    SpendStamina(Configuration_Configuracion.StaminaCostPerPostureTransition);
            }

            _currentBodyState = newState;
            if (newState != BodyState.InTheAir) _lastBodyStateWhileOnGround = newState;
            _timeOfLastBodyStateTransition = Time.time;

            if (voluntaryChange && Configuration_Configuracion.EnableTheStaminaSystem)
            {
                if (newState == BodyState.Crouching && Configuration_Configuracion.EnableStaminaCostWhenCrouching)
                    SpendStamina(Configuration_Configuracion.StaminaCostWhenCrouching);
                if (newState == BodyState.Prone && Configuration_Configuracion.EnableStaminaCostWhenGoingProne)
                    SpendStamina(Configuration_Configuracion.StaminaCostWhenGoingProne);
            }

            if (Configuration_Configuracion.EnableTheHeadBobbingSystem)
            {
                if (newState == BodyState.Crouching && Configuration_Configuracion.EnableReactiveHeadBobbingWhenCrouching)
                    _reactiveBobbingImpulse = Vector3.down * Configuration_Configuracion.IntensityOfReactiveHeadBobbingWhenCrouching;
                else if (newState == BodyState.Prone && Configuration_Configuracion.EnableReactiveHeadBobbingWhenGoingProne)
                    _reactiveBobbingImpulse = Vector3.down * Configuration_Configuracion.IntensityOfReactiveHeadBobbingWhenGoingProne;
            }
        }

        private void EnforceCanStandUp()
        {
            if (_currentBodyState == BodyState.Standing && !CanStandUp())
            {
                ChangeBodyState(BodyState.Crouching, false);
            }
        }

        private bool CanStandUp()
        {
            float difference = Configuration_Configuracion.CapsuleColliderHeightWhileStanding - _playerCapsuleCollider.height;
            if (difference <= 0.05f) return true;

            int maskWithoutPlayer = Physics.DefaultRaycastLayers & ~(1 << gameObject.layer);

            bool result = !Physics.SphereCast(transform.position + Vector3.up * (_playerCapsuleCollider.center.y), _playerCapsuleCollider.radius, Vector3.up, out RaycastHit hit, Configuration_Configuracion.CapsuleColliderHeightWhileStanding - _playerCapsuleCollider.radius * 2f, maskWithoutPlayer, QueryTriggerInteraction.Ignore);

            return result;
        }

        private bool CanCrouch()
        {
            float difference = Configuration_Configuracion.CapsuleColliderHeightWhileCrouching - _playerCapsuleCollider.height;
            if (difference <= 0.05f) return true;

            int maskWithoutPlayer = Physics.DefaultRaycastLayers & ~(1 << gameObject.layer);

            return !Physics.SphereCast(transform.position + Vector3.up * (_playerCapsuleCollider.radius), _playerCapsuleCollider.radius, Vector3.up, out _, Configuration_Configuracion.CapsuleColliderHeightWhileCrouching - _playerCapsuleCollider.radius * 2f, maskWithoutPlayer, QueryTriggerInteraction.Ignore);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 08 · TRANSICIÓN DEL CAPSULE COLLIDER  /  CAPSULE COLLIDER TRANSITION
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void UpdateCapsuleColliderTransition()
        {
            float targetHeight = _currentBodyState switch
            {
                BodyState.Crouching => Configuration_Configuracion.CapsuleColliderHeightWhileCrouching,
                BodyState.Prone => Configuration_Configuracion.CapsuleColliderHeightWhileProne,
                _ => Configuration_Configuracion.CapsuleColliderHeightWhileStanding
            };

            float targetRadius = _currentBodyState == BodyState.Prone ? targetHeight * 0.5f : _originalCapsuleRadius;

            if (Mathf.Abs(_playerCapsuleCollider.height - targetHeight) < 0.001f)
            {
                _playerCapsuleCollider.height = targetHeight;
                _playerCapsuleCollider.radius = targetRadius;
                _playerCapsuleCollider.center = new Vector3(0f, targetHeight * 0.5f, 0f);
                return;
            }

            _playerCapsuleCollider.radius = Mathf.Lerp(_playerCapsuleCollider.radius, targetRadius, Time.deltaTime * Configuration_Configuracion.SpeedOfTheCapsuleColliderTransition);
            _playerCapsuleCollider.height = Mathf.Lerp(_playerCapsuleCollider.height, targetHeight, Time.deltaTime * Configuration_Configuracion.SpeedOfTheCapsuleColliderTransition);
            _playerCapsuleCollider.center = new Vector3(0f, _playerCapsuleCollider.height * 0.5f, 0f);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 09 · SALTO  /  JUMP
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ReadJumpInput()
        {
            if (!Configuration_Configuracion.AllowThePlayerToJump) return;

            if (JumpInputPressed())
                _remainingJumpBufferTime = Configuration_Configuracion.EnableJumpBuffering ? Configuration_Configuracion.DurationOfTheJumpBuffering : 0f;

            if (JumpInputReleased() && _playerIsPerformingJump)
                _jumpInputHoldTime = Configuration_Configuracion.MaximumHeldTimeOfTheJumpInput + 1f;
        }

        private void UpdateCoyoteTimeTimer()
        {
            if (_remainingCoyoteTime > 0f) _remainingCoyoteTime -= Time.deltaTime;
        }

        private void UpdateJumpBufferTimer()
        {
            if (_remainingJumpBufferTime > 0f) _remainingJumpBufferTime -= Time.deltaTime;
        }

        private void ApplyJump()
        {
            if (!Configuration_Configuracion.AllowThePlayerToJump) return;

            if (_playerIsPerformingJump && Configuration_Configuracion.EnableVariableJump && JumpInputHeld())
            {
                if (_jumpInputHoldTime < Configuration_Configuracion.MaximumHeldTimeOfTheJumpInput)
                {
                    _playerRigidbody.AddForce(Vector3.up * Configuration_Configuracion.ExtraForcePerSecondOfTheVariableJump, ForceMode.Acceleration);
                    _jumpInputHoldTime += Time.fixedDeltaTime;
                }
            }

            bool fromGround = (_playerIsOnGround || (_remainingCoyoteTime > 0f && Configuration_Configuracion.EnableCoyoteTime)) && !_playerIsPerformingJump;
            bool fromAir = !_playerIsOnGround && _remainingJumps > 0 && Configuration_Configuracion.HowManyJumps > 1;
            bool canExecuteJump = (fromGround || fromAir) && _remainingJumpBufferTime > 0f;

            if (!canExecuteJump) return;

            if (_currentBodyState == BodyState.Prone)
            {
                if (!Configuration_Configuracion.AllowThePlayerToJumpWhileProne) return;
                if (!CanStandUp()) return;
                if (!_systemWaitingForTransitionToPerformJump)
                {
                    _systemWaitingForTransitionToPerformJump = true;
                    if (_proneJumpCoroutineActive != null) StopCoroutine(_proneJumpCoroutineActive);
                    _proneJumpCoroutineActive = StartCoroutine(TransitionToStandingAndJump());
                }
                return;
            }

            if (_playerIsSliding)
                CancelSliding();

            if (_currentBodyState == BodyState.Crouching && !CanStandUp()) return;

            ExecuteJump();
        }

        private IEnumerator TransitionToStandingAndJump()
        {
            ChangeBodyState(BodyState.Standing, false);
            float waitedTime = 0f;

            while (Mathf.Abs(_playerCapsuleCollider.height - Configuration_Configuracion.CapsuleColliderHeightWhileStanding) > 0.15f && waitedTime < 0.5f)
            {
                waitedTime += Time.deltaTime;
                yield return null;
            }

            ExecuteJump(fromProneState: true);
            _systemWaitingForTransitionToPerformJump = false;
        }

        private void ExecuteJump(bool fromProneState = false)
        {
            if (Configuration_Configuracion.EnableTheStaminaSystem && Configuration_Configuracion.EnableStaminaCostWhenJumping)
            {
                float totalCost = Configuration_Configuracion.StaminaCostWhenJumping + (fromProneState && Configuration_Configuracion.EnableExtraStaminaCostWhenJumpingFromProne ? Configuration_Configuracion.ExtraStaminaCostWhenJumpingFromTheProneState : 0f);

                if (PlayerCurrentStamina < totalCost) return;
                SpendStamina(totalCost);
            }

            Vector3 currentVelocity = _playerRigidbody.linearVelocity;
            currentVelocity.y = 0f;
            _playerRigidbody.linearVelocity = currentVelocity;
            _playerRigidbody.AddForce(Vector3.up * Configuration_Configuracion.ForceAppliedWhenJumping, ForceMode.VelocityChange);

            _remainingJumps--;
            _playerIsPerformingJump = true;
            _jumpInputHoldTime = 0f;
            _remainingJumpBufferTime = 0f;
            _remainingCoyoteTime = 0f;

            _lastBodyStateWhileOnGround = _currentBodyState;
            ChangeBodyState(BodyState.InTheAir, false);

            if (Configuration_Configuracion.EnableTheHeadBobbingSystem && Configuration_Configuracion.EnableReactiveHeadBobbingWhenJumpingAndLanding)
                _reactiveBobbingImpulse = Vector3.back * Configuration_Configuracion.IntensityOfReactiveHeadBobbingWhenJumping;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 10 · RESISTENCIA  /  STAMINA
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void UpdateStamina()
        {
            if (!Configuration_Configuracion.EnableTheStaminaSystem) return;

            bool playerIsMoving = (_playerRigidbody.linearVelocity.x * _playerRigidbody.linearVelocity.x + _playerRigidbody.linearVelocity.z * _playerRigidbody.linearVelocity.z) > 0.01f;
            bool playerIsRunning = ThePlayerIsRunningNow() && _playerIsOnGround && playerIsMoving;

            if (playerIsRunning && Configuration_Configuracion.EnableStaminaCostWhenRunning)
            {
                SpendStamina(Configuration_Configuracion.StaminaCostPerSecondWhenRunning * Time.fixedDeltaTime);
                return;
            }

            _timeElapsedWithoutSpendingStamina += Time.fixedDeltaTime;
            if (_timeElapsedWithoutSpendingStamina >= Configuration_Configuracion.DelayInSecondsBeforeStaminaStartsRegenerating)
            {
                float regenerationRate = playerIsMoving ? Configuration_Configuracion.StaminaRegenerationSpeedWhileThePlayerWalks : Configuration_Configuracion.StaminaRegenerationSpeedWhileThePlayerIsIdle;

                PlayerCurrentStamina = Mathf.Min(PlayerCurrentStamina + regenerationRate * Time.fixedDeltaTime, Configuration_Configuracion.MaximumPlayerStamina);
            }
        }

        private void SpendStamina(float cantidad)
        {
            PlayerCurrentStamina = Mathf.Max(PlayerCurrentStamina - cantidad, 0f);
            _timeElapsedWithoutSpendingStamina = 0f;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 11 · HUD  /  HUD
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void UpdateStaminaHUD()
        {
            bool show = Configuration_Configuracion.ShowTheStaminaBarOnTheHUD && Configuration_Configuracion.EnableTheStaminaSystem;

            if (StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia != null)
                StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia.gameObject.SetActive(show);

            if (CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual != null)
            {
                CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual.gameObject.SetActive(show);
                if (show)
                {
                    float proportion = PlayerCurrentStamina / Configuration_Configuracion.MaximumPlayerStamina;
                    CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual.fillAmount = Mathf.Lerp(CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual.fillAmount, proportion, Time.deltaTime * 12f);

                    if (_originalWidthInPixelsOfStaminaBar > 0f)
                    {
                        Vector2 size = CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual.rectTransform.sizeDelta;
                        size.x = Mathf.Lerp(size.x, _originalWidthInPixelsOfStaminaBar * proportion, Time.deltaTime * 12f);
                        CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual.rectTransform.sizeDelta = size;
                    }
                }
            }
        }

        private void UpdateCrosshairVisibility()
        {
            if (CrosshairOnHUD_CrosshairEnElHUD != null)
                CrosshairOnHUD_CrosshairEnElHUD.SetActive(Configuration_Configuracion.ShowTheCrosshairOnTheHUD);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 12 · CÁMARA  /  CAMERA
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ReadCameraInput()
        {
            if (CameraSupport_SoporteDeLaCamara == null || MainCamera_CamaraPrincipal == null) return;

            float sensitivityH = Configuration_Configuracion.HorizontalMouseSensitivity;
            float sensitivityV = Configuration_Configuracion.VerticalMouseSensitivity;

            if (_zoomIsActive && Configuration_Configuracion.ReduceSensitivityDuringZoom)
            {
                sensitivityH *= Configuration_Configuracion.SensitivityMultiplierDuringZoom;
                sensitivityV *= Configuration_Configuracion.SensitivityMultiplierDuringZoom;
            }

            if (_playerIsSliding && Configuration_Configuracion.ReduceCameraSensitivityDuringSliding)
            {
                sensitivityH *= Configuration_Configuracion.CameraSensitivityMultiplierDuringSliding;
                sensitivityV *= Configuration_Configuracion.CameraSensitivityMultiplierDuringSliding;
            }

            float inputX, inputY;

            if (_isUsingGamepad)
            {
                float rsH = 0f, rsV = 0f;

                try { rsH = Input.GetAxisRaw("RSHorizontal"); } catch { }
                try { rsV = Input.GetAxisRaw("RSVertical"); } catch { }

                if (Mathf.Abs(rsH) < Configuration_Configuracion.RightStickDeadZone) rsH = 0f;
                if (Mathf.Abs(rsV) < Configuration_Configuracion.RightStickDeadZone) rsV = 0f;

                inputX = rsH * sensitivityH;
                inputY = rsV * sensitivityV;
            }
            else
            {
                inputX = Input.GetAxis("Mouse X") * sensitivityH;
                inputY = Input.GetAxis("Mouse Y") * sensitivityV;
            }

            transform.Rotate(Vector3.up * inputX);

            _cameraAccumulatedVerticalRotationAngle -= inputY;
            _cameraAccumulatedVerticalRotationAngle = Mathf.Clamp(_cameraAccumulatedVerticalRotationAngle, -Configuration_Configuracion.UpperVerticalLimitOfTheCamera, Configuration_Configuracion.LowerVerticalLimitOfTheCamera);

            CameraSupport_SoporteDeLaCamara.localRotation = Quaternion.Euler(_cameraAccumulatedVerticalRotationAngle + _breathingPitchOffset, 0f, 0f);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 13 · ALTURA DE CÁMARA  /  CAMERA HEIGHT
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void UpdateCameraHeight()
        {
            if (CameraSupport_SoporteDeLaCamara == null) return;

            float targetHeight = _currentBodyState switch
            {
                BodyState.Crouching => Configuration_Configuracion.HeightOfTheCameraSupportWhileCrouching,
                BodyState.Prone => Configuration_Configuracion.HeightOfTheCameraSupportWhileProne,
                _ => Configuration_Configuracion.HeightOfTheCameraSupportWhileStanding
            };

            Vector3 position = CameraSupport_SoporteDeLaCamara.localPosition;

            float yBase = Mathf.Lerp(position.y - _continuousBobbingOffset.y - _reactiveBobbingImpulse.y - _breathingTranslationOffset, targetHeight, Time.deltaTime * Configuration_Configuracion.SpeedOfTheCameraHeightTransition);

            position.x = _continuousBobbingOffset.x + _reactiveBobbingImpulse.x;
            position.y = yBase + _continuousBobbingOffset.y + _reactiveBobbingImpulse.y + _breathingTranslationOffset;
            position.z = _continuousBobbingOffset.z + _reactiveBobbingImpulse.z;

            CameraSupport_SoporteDeLaCamara.localPosition = position;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 14 · BALANCEO DE CABEZA  /  HEAD BOB
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void UpdateHeadBobbing()
        {
            if (!Configuration_Configuracion.EnableTheHeadBobbingSystem || CameraSupport_SoporteDeLaCamara == null) return;

            float horizontalSpeed = Mathf.Sqrt(_playerRigidbody.linearVelocity.x * _playerRigidbody.linearVelocity.x + _playerRigidbody.linearVelocity.z * _playerRigidbody.linearVelocity.z);

            bool playerIsMovingOnGround = horizontalSpeed > 0.1f && _playerIsOnGround;
            bool playerIsRunningNow = playerIsMovingOnGround && ThePlayerIsRunningNow();


            // ── Respiración / Breathing ───────────────────────────────────────────────
            float breathingPitchTarget = 0f;
            float breathingTranslationTarget = 0f;

            if (Configuration_Configuracion.EnableBreathingEffect)
            {
                float rawExhaustion = Configuration_Configuracion.EnableTheStaminaSystem ? 1f - (PlayerCurrentStamina / Configuration_Configuracion.MaximumPlayerStamina) : 0f;

                float fadeSpeed = rawExhaustion > _smoothedExhaustionInfluence ? Configuration_Configuracion.BreathingExhaustionFadeInSpeed : Configuration_Configuracion.BreathingExhaustionFadeOutSpeed;
                _smoothedExhaustionInfluence = Mathf.MoveTowards(_smoothedExhaustionInfluence, rawExhaustion, fadeSpeed * Time.deltaTime);

                float bpm = Mathf.Lerp(Configuration_Configuracion.BreathsPerMinuteAtRest, Configuration_Configuracion.BreathsPerMinuteWhenExhausted, _smoothedExhaustionInfluence);

                _breathingTimer += Time.deltaTime * (bpm / 60f);
                _breathingTimer %= 1f;

                float inhaleFraction = Configuration_Configuracion.BreathingInhaleFraction;
                float sineInput = _breathingTimer < inhaleFraction ? (_breathingTimer / inhaleFraction) * Mathf.PI : Mathf.PI + ((_breathingTimer - inhaleFraction) / (1f - inhaleFraction)) * Mathf.PI;

                float wave = Mathf.Sin(sineInput);

                float pitchIntensity = Configuration_Configuracion.BreathingBasePitchIntensity + Configuration_Configuracion.BreathingExhaustedPitchIntensity * _smoothedExhaustionInfluence;

                float translationIntensity = Configuration_Configuracion.BreathingBaseTranslationIntensity + Configuration_Configuracion.BreathingExhaustedTranslationIntensity * _smoothedExhaustionInfluence;

                breathingPitchTarget = -wave * pitchIntensity;
                breathingTranslationTarget = wave * translationIntensity;
            }

            _breathingPitchOffset = Mathf.Lerp(_breathingPitchOffset, breathingPitchTarget, Time.deltaTime * 3f);

            if (playerIsMovingOnGround)
            {
                float baseIntensity;
                float baseFrequency;

                if (playerIsRunningNow && Configuration_Configuracion.EnableHeadBobbingWhileThePlayerRuns)
                {
                    baseIntensity = Configuration_Configuracion.IntensityOfHeadBobbingWhileThePlayerRuns;
                    baseFrequency = Configuration_Configuracion.FrequencyOfHeadBobbingWhileThePlayerRuns;
                }
                else if (Configuration_Configuracion.EnableHeadBobbingWhileThePlayerWalks)
                {
                    baseIntensity = Configuration_Configuracion.IntensityOfHeadBobbingWhileThePlayerWalks;
                    baseFrequency = Configuration_Configuracion.FrequencyOfHeadBobbingWhileThePlayerWalks;
                }
                else
                {
                    baseIntensity = 0f;
                    baseFrequency = Configuration_Configuracion.FrequencyOfHeadBobbingWhileThePlayerWalks;
                }

                // ── Multiplicador por postura / Posture multiplier ────────────────────
                if (_currentBodyState == BodyState.Crouching)
                {
                    baseIntensity *= 0.75f;
                    baseFrequency *= 0.75f;
                }
                else if (_currentBodyState == BodyState.Prone)
                {
                    baseIntensity *= 0.3f;
                    baseFrequency *= 0.6f;
                }

                _continuousBobbingTimer += Time.deltaTime * baseFrequency;
                _continuousBobbingOffset.y = Mathf.Sin(_continuousBobbingTimer) * baseIntensity;
                _continuousBobbingOffset.x = Mathf.Cos(_continuousBobbingTimer * 0.5f) * baseIntensity * 0.5f;
            }
            else
            {
                _continuousBobbingTimer = 0f;
                _continuousBobbingOffset = Vector3.Lerp(_continuousBobbingOffset, Vector3.zero, Time.deltaTime * Configuration_Configuracion.ReturnSpeedOfReactiveHeadBobbingToTheNeutralPosition);
            }

            _breathingTranslationOffset = Mathf.Lerp(_breathingTranslationOffset, breathingTranslationTarget, Time.deltaTime * 3f);

            _reactiveBobbingImpulse = Vector3.Lerp(_reactiveBobbingImpulse, Vector3.zero, Time.deltaTime * Configuration_Configuracion.ReturnSpeedOfReactiveHeadBobbingToTheNeutralPosition);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 15 · ZOOM  /  ZOOM 
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ReadZoomInput()
        {
            if (!Configuration_Configuracion.EnableTheZoomSystem) return;
            if (_currentlyHeldObject != null && !Configuration_Configuracion.AllowZoomWhileHoldingAnObject) return;

            if (Configuration_Configuracion.HoldToZoom)
            {
                _zoomIsActive = ZoomInputHeld();
            }
            else if (ZoomInputPressed())
            {
                _zoomToggleModeIsActive = !_zoomToggleModeIsActive;
                _zoomIsActive = _zoomToggleModeIsActive;
            }
        }

        private void UpdateZoom()
        {
            float targetFov;
            float transitionSpeed;

            if (_zoomIsActive && Configuration_Configuracion.EnableTheZoomSystem)
            {
                targetFov = Configuration_Configuracion.CameraFieldOfViewDuringZoom;
                transitionSpeed = Configuration_Configuracion.SpeedOfTheFieldOfViewTransitionDuringZoom;
            }
            else
            {
                targetFov = Configuration_Configuracion.BaseFieldOfViewOfTheCamera;
                transitionSpeed = Configuration_Configuracion.SpeedOfTheFieldOfViewTransitionDuringZoom;

                if (Configuration_Configuracion.EnableSpeedFOVEffect)
                {
                    float horizontalSpeed = new UnityEngine.Vector2(_playerRigidbody.linearVelocity.x, _playerRigidbody.linearVelocity.z).magnitude;

                    float speedThreshold = Configuration_Configuracion.BaseSpeedOfThePlayer * Configuration_Configuracion.SpeedFOVStartPercent;
                    float speedMax = Configuration_Configuracion.BaseSpeedOfThePlayer * Configuration_Configuracion.SpeedFOVMaxPercent;

                    float speedRatio = Mathf.InverseLerp(speedThreshold, speedMax, horizontalSpeed);
                    targetFov = Configuration_Configuracion.BaseFieldOfViewOfTheCamera * Mathf.Lerp(1f, Configuration_Configuracion.SpeedFOVMaxMultiplier, speedRatio);

                    transitionSpeed = Configuration_Configuracion.SpeedFOVTransitionSpeed;
                }
            }

            _currentCameraFieldOfView = Mathf.Lerp(_currentCameraFieldOfView, targetFov, Time.deltaTime * transitionSpeed);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 16 · INTERACCIÓN DE OBJETOS  /  OBJECT INTERACTION
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ReadObjectInteractionInput()
        {
            if (!Configuration_Configuracion.EnableTheObjectInteractionSystem) return;

            if (PickUpOrDropInputPressed())
            {
                if (_currentlyHeldObject != null)
                    DropObjectInHand();
                else
                    TryToPickUpTheObject();
            }

            if (_currentlyHeldObject != null)
            {
                if (Configuration_Configuracion.HoldToRotateTheObject)
                {
                    _objectRotationModeIsActive = _isUsingGamepad ? Input.GetKey(Configuration_Configuracion.GamepadButtonToActivateObjectRotationInHand) : Input.GetKey(Configuration_Configuracion.KeyboardKeyToActivateObjectRotationInHand);
                }
                else if (ActivateRotationInputPressed())
                {
                    _objectRotationModeIsActive = !_objectRotationModeIsActive;
                }
            }
            else
            {
                _objectRotationModeIsActive = false;
            }

            if (_currentlyHeldObject != null && _objectRotationModeIsActive)
                RotateObjectInHand();

            if (_currentlyHeldObject != null)
            {
                if (ThrowInputHeld())
                {
                    _throwInputIsBeingHeld = true;
                    _throwInputHoldTime = Mathf.Min(_throwInputHoldTime + Time.deltaTime, Configuration_Configuracion.MaximumChargeTimeOfTheObjectThrow);
                }

                if (ThrowInputReleased() && _throwInputIsBeingHeld)
                {
                    ThrowObjectInHand();
                    _throwInputIsBeingHeld = false;
                    _throwInputHoldTime = 0f;
                }
            }
        }

        private void TryToPickUpTheObject()
        {
            if (MainCamera_CamaraPrincipal == null) return;

            Ray ray = new Ray(MainCamera_CamaraPrincipal.transform.position, MainCamera_CamaraPrincipal.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, Configuration_Configuracion.MaximumDistanceToPickUpAnObject))
            {
                if (hit.collider.CompareTag(Configuration_Configuracion.TagOfPickableObjects))
                    PickUpObject(hit.collider.gameObject);
            }
        }

        private void PickUpObject(GameObject objectToPickUp)
        {
            _currentlyHeldObject = objectToPickUp;

            Rigidbody objectRigidbody = objectToPickUp.GetComponent<Rigidbody>();
            if (objectRigidbody != null)
            {
                objectRigidbody.linearVelocity = Vector3.zero;
                objectRigidbody.isKinematic = true;
            }

            if (Configuration_Configuracion.DisableTheObjectColliderWhenPickingItUp)
            {
                _heldObjectCollider = objectToPickUp.GetComponent<Collider>();
                if (_heldObjectCollider != null) _heldObjectCollider.enabled = false;
            }

            _objectReachingAnchor = true;
        }

        private void DropObjectInHand()
        {
            if (_currentlyHeldObject == null) return;

            Rigidbody objectRigidbody = _currentlyHeldObject.GetComponent<Rigidbody>();
            if (objectRigidbody != null)
            {
                objectRigidbody.isKinematic = false;

                objectRigidbody.linearVelocity = _playerRigidbody.linearVelocity;
            }

            if (_heldObjectCollider != null)
            {
                _heldObjectCollider.enabled = true;
                _heldObjectCollider = null;
            }

            _currentlyHeldObject = null;
            _objectRotationModeIsActive = false;
        }

        private void ThrowObjectInHand()
        {
            if (_currentlyHeldObject == null) return;

            GameObject objectToThrow = _currentlyHeldObject;

            float chargeRatio = _throwInputHoldTime / Configuration_Configuracion.MaximumChargeTimeOfTheObjectThrow;
            float finalForce = Mathf.Lerp(Configuration_Configuracion.MinimumObjectThrowForce, Configuration_Configuracion.MaximumObjectThrowForce, chargeRatio);

            Ray cameraRay = new Ray(MainCamera_CamaraPrincipal.transform.position, MainCamera_CamaraPrincipal.transform.forward);

            int mask = Physics.DefaultRaycastLayers & ~(1 << gameObject.layer) & ~(1 << objectToThrow.layer);

            Vector3 throwDirection;
            if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000f, mask))
                throwDirection = (hit.point - objectToThrow.transform.position).normalized;
            else
                throwDirection = cameraRay.direction;

            DropObjectInHand();

            Rigidbody rb = objectToThrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                if (Configuration_Configuracion.TakeIntoAccountTheMassOfTheObjectWhenThrowingIt)
                    rb.AddForce(throwDirection * finalForce, ForceMode.Impulse);
                else
                    rb.AddForce(throwDirection * finalForce, ForceMode.VelocityChange);
            }
        }

        private void RotateObjectInHand()
        {
            if (_currentlyHeldObject == null) return;

            float verticalRotationInput = 0f;
            float horizontalRotationInput = 0f;

            if (_isUsingGamepad)
            {
                float dpadH = 0f, dpadV = 0f;
                try { dpadH = Input.GetAxisRaw("DpadHorizontal"); } catch { }
                try { dpadV = Input.GetAxisRaw("DpadVertical"); } catch { }
                verticalRotationInput = dpadV;
                horizontalRotationInput = dpadH;
            }
            else
            {
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToRotateObjectUp)) verticalRotationInput = -1f;
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToRotateObjectDown)) verticalRotationInput = 1f;
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToRotateObjectRight)) horizontalRotationInput = -1f;
                if (Input.GetKey(Configuration_Configuracion.KeyboardKeyToRotateObjectLeft)) horizontalRotationInput = 1f;
            }

            if (verticalRotationInput == 0f && horizontalRotationInput == 0f) return;

            float rotationSpeed = Configuration_Configuracion.RotationSpeedOfTheObjectInHand * Time.deltaTime;
            Transform camTransform = MainCamera_CamaraPrincipal.transform;

            _currentlyHeldObject.transform.Rotate(camTransform.right, -verticalRotationInput * rotationSpeed, Space.World);
            _currentlyHeldObject.transform.Rotate(camTransform.up, horizontalRotationInput * rotationSpeed, Space.World);
        }

        private void UpdateHeldObjectPosition()
        {
            if (_currentlyHeldObject == null) return;
            if (AnchorPointOfPickedObject_PuntoDeAnclajeDelObjetoRecogido == null) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            Vector3 swayTarget = new Vector3(-mouseX, -mouseY, 0f) * 0.2f;
            _swayOffset = Vector3.SmoothDamp(_swayOffset, swayTarget, ref _swayVelocity, 0.1f);

            Vector3 anchorWithSway = AnchorPointOfPickedObject_PuntoDeAnclajeDelObjetoRecogido.position + MainCamera_CamaraPrincipal.transform.TransformDirection(_swayOffset);

            if (_objectReachingAnchor)
            {
                _currentlyHeldObject.transform.position = Vector3.Lerp(_currentlyHeldObject.transform.position, anchorWithSway, Time.deltaTime * Configuration_Configuracion.SpeedOfTheObjectMovementTowardsTheAnchorPoint);

                if (Vector3.Distance(_currentlyHeldObject.transform.position, AnchorPointOfPickedObject_PuntoDeAnclajeDelObjetoRecogido.position) < 0.25f)
                {
                    _objectReachingAnchor = false;
                    _swayOffset = Vector3.zero;
                    _swayVelocity = Vector3.zero;
                }

                return;
            }

            _currentlyHeldObject.transform.position = anchorWithSway;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 17 · DESLIZAMIENTO  /  SLIDE
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ExecuteSliding()
        {
            if (!Configuration_Configuracion.EnableTheSlidingSystem) return;

            if (_playerIsSliding)
            {
                if (Configuration_Configuracion.AllowInterruptingSliding && _interruptSlideInputCached)
                {
                    _interruptSlideInputCached = false;
                    CancelSliding();
                    return;
                }
                _interruptSlideInputCached = false;

                if (Configuration_Configuracion.UseRealPhysicsInSliding)
                    ProcessPhysicalSliding();
                else
                    ProcessArcadeSliding();
            }
        }

        private bool InputToInterruptSliding()
        {
            if (!Configuration_Configuracion.AllowJumpingDuringSliding && JumpInputPressed())
                return true;

            if (_isUsingGamepad)
                return Input.GetKeyDown(Configuration_Configuracion.GamepadButtonToCrouchAndGoProne);
            return Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToCrouch);
        }

        private void StartSliding()
        {
            if (!ThePlayerIsRunningNow()) return;
            if (!_playerIsOnGround) return;
            if (_playerIsSliding) return;

            Vector3 slideDir = GetMovementDirection();
            if (slideDir == Vector3.zero) slideDir = transform.forward;

            float dotBackward = Vector3.Dot(transform.forward, slideDir);
            if (dotBackward < 0f) return;

            float dotForward = Mathf.Max(0f, dotBackward);
            float dotLateral = Mathf.Abs(Vector3.Dot(transform.right, slideDir));
            float lateralRatio = dotLateral / (dotForward + dotLateral + 0.001f);


            _playerIsSliding = true;
            _timeSinceSlidingStarted = 0f;
            _slideDirectionAtStart = slideDir * Mathf.Lerp(1f, 0.5f, lateralRatio);
            _interruptSlideInputCached = false;

            _currentBodyState = BodyState.Crouching;
            _timeOfLastBodyStateTransition = Time.time;
            _crouchToggleModeIsActive = false;

            float crouchHeight = Configuration_Configuracion.CapsuleColliderHeightWhileCrouching;
            _playerCapsuleCollider.height = crouchHeight;
            _playerCapsuleCollider.center = new Vector3(0f, crouchHeight * 0.5f, 0f);

            if (Configuration_Configuracion.EnableTheStaminaSystem && Configuration_Configuracion.EnableStaminaCostWhenSliding)
                SpendStamina(Configuration_Configuracion.StaminaCostWhenSliding);

            if (Configuration_Configuracion.EnableTheHeadBobbingSystem && Configuration_Configuracion.EnableReactiveHeadBobbingWhenSliding)
                _reactiveBobbingImpulse = Vector3.down * Configuration_Configuracion.IntensityOfReactiveHeadBobbingWhenSliding;
        }

        private void ProcessArcadeSliding()
        {
            _timeSinceSlidingStarted += Time.fixedDeltaTime;

            float progressRatio = _timeSinceSlidingStarted / Configuration_Configuracion.DurationOfSlidingInArcadeMode;
            float currentSpeed = Mathf.Lerp(Configuration_Configuracion.BaseSpeedOfThePlayer * Configuration_Configuracion.InitialSpeedMultiplierOfArcadeSliding, Configuration_Configuracion.MinimumSpeedToKeepSliding, progressRatio);

            _playerRigidbody.linearVelocity = new Vector3(_slideDirectionAtStart.x * currentSpeed, _playerRigidbody.linearVelocity.y, _slideDirectionAtStart.z * currentSpeed);

            if (_timeSinceSlidingStarted >= Configuration_Configuracion.DurationOfSlidingInArcadeMode || currentSpeed <= Configuration_Configuracion.MinimumSpeedToKeepSliding)
                CancelSliding();
        }

        private void ProcessPhysicalSliding()
        {

            _timeSinceSlidingStarted += Time.fixedDeltaTime;

            Vector3 horizontalVelocity = new Vector3(_playerRigidbody.linearVelocity.x, 0f, _playerRigidbody.linearVelocity.z);
            _playerRigidbody.AddForce(-horizontalVelocity * Configuration_Configuracion.FrictionDuringPhysicalSliding, ForceMode.Acceleration);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, Configuration_Configuracion.CapsuleColliderHeightWhileStanding, _effectiveGroundMask))
            {
                Vector3 surfaceNormal = hit.normal;
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
                _playerRigidbody.AddForce(slopeDirection * Configuration_Configuracion.SlopeAccelerationMultiplierDuringSliding, ForceMode.Acceleration);
            }

            if (horizontalVelocity.magnitude <= Configuration_Configuracion.MinimumSpeedToKeepSliding)
                CancelSliding();
        }

        private void CancelSliding()
        {
            _playerIsSliding = false;
            _timeSinceSlidingStarted = 0f;

            _interruptSlideInputCached = false;

            _playerWantsToStayDown = false;

            if (Configuration_Configuracion.RecoveryTimeAfterSliding > 0f)
            {
                _playerIsInPostSlideRecovery = true;
                _remainingPostSlideRecoveryTime = Configuration_Configuracion.RecoveryTimeAfterSliding;

                _currentBodyState = BodyState.Crouching;
            }
            else
            {
                _playerIsInPostSlideRecovery = false;
            }
        }

        private void UpdatePostSlideRecovery()
        {
            if (!_playerIsInPostSlideRecovery) return;

            _remainingPostSlideRecoveryTime -= Time.deltaTime;

            if (_remainingPostSlideRecoveryTime <= 0f)
            {
                _playerIsInPostSlideRecovery = false;

                if (CanStandUp())
                    ChangeBodyState(BodyState.Standing, false);
            }
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 18 · DASH  /  DASH
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void ReadDashInput()
        {
            if (!Configuration_Configuracion.EnableTheDashSystem) return;

            if (_isUsingGamepad)
            {
                if (Configuration_Configuracion.RequireDoublePressForDashOnGamepad)
                {

                    if (Input.GetKeyDown(Configuration_Configuracion.GamepadButtonForDash))
                    {
                        if (!_waitingForSecondDashButtonPress)
                        {
                            _waitingForSecondDashButtonPress = true;
                            _timestampOfFirstDashButtonPress = Time.time;
                        }
                        else
                        {
                            if (Time.time - _timestampOfFirstDashButtonPress <= Configuration_Configuracion.MaximumTimeBetweenTheTwoPressesForDashDoublePress)
                                TryExecuteDash();
                            _waitingForSecondDashButtonPress = false;
                        }
                    }
                    if (_waitingForSecondDashButtonPress && Time.time - _timestampOfFirstDashButtonPress > Configuration_Configuracion.MaximumTimeBetweenTheTwoPressesForDashDoublePress)
                        _waitingForSecondDashButtonPress = false;
                }
                else
                {

                    if (Input.GetKeyDown(Configuration_Configuracion.GamepadButtonForDash))
                        TryExecuteDash();
                }
            }
            else
            {

                if (Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyForDash))
                    TryExecuteDash();
            }
        }

        private void UpdateDashCooldownTimer()
        {
            if (_timeSinceLastDash < Configuration_Configuracion.CooldownBetweenDashUses + 1f)
                _timeSinceLastDash += Time.deltaTime;
        }

        private void TryExecuteDash()
        {
            if (_timeSinceLastDash < Configuration_Configuracion.CooldownBetweenDashUses) return;
            if (!Configuration_Configuracion.AllowDashInTheAir && !_playerIsOnGround) return;

            if (_playerIsSliding || _playerIsInPostSlideRecovery)
            {
                if (!Configuration_Configuracion.AllowDashDuringSliding) return;
                _playerIsSliding = false;
                _playerIsInPostSlideRecovery = false;
                _remainingPostSlideRecoveryTime = 0f;
            }
            if (Configuration_Configuracion.EnableStaminaCostWhenUsingDash && Configuration_Configuracion.EnableTheStaminaSystem)
            {
                if (PlayerCurrentStamina < Configuration_Configuracion.StaminaCostWhenUsingDash) return;
                SpendStamina(Configuration_Configuracion.StaminaCostWhenUsingDash);
            }

            StartCoroutine(ExecuteDash());
        }

        private IEnumerator ExecuteDash()
        {
            _playerIsPerformingDash = true;
            _timeSinceLastDash = 0f;

            Vector3 dashDirection = GetMovementDirection();
            if (dashDirection == Vector3.zero) dashDirection = transform.forward;

            _playerRigidbody.linearVelocity = new Vector3(dashDirection.x * Configuration_Configuracion.DashForce, _playerRigidbody.linearVelocity.y, dashDirection.z * Configuration_Configuracion.DashForce);

            if (Configuration_Configuracion.EnableTheHeadBobbingSystem && Configuration_Configuracion.EnableReactiveHeadBobbingWhenDashing)
                _reactiveBobbingImpulse = transform.InverseTransformDirection(dashDirection) * Configuration_Configuracion.IntensityOfReactiveHeadBobbingWhenDashing;

            yield return new WaitForSeconds(Configuration_Configuracion.DurationOfTheDashImpulse);

            _playerIsPerformingDash = false;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 19 · CAPA DEL INPUT  /  INPUT LAYER
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        private void DetectGamepadTriggers()
        {
            _LTWasActiveLastFrame = _LTIsActive;
            _RTWasActiveLastFrame = _RTIsActive;

            _currentLeftTriggerValue = 0f;
            _currentRightTriggerValue = 0f;

            try { _currentLeftTriggerValue = Input.GetAxisRaw("LT"); } catch { }
            try { _currentRightTriggerValue = Input.GetAxisRaw("RT"); } catch { }

            const float triggerThreshold = 0.3f;
            _LTIsActive = _currentLeftTriggerValue > triggerThreshold;
            _RTIsActive = _currentRightTriggerValue > triggerThreshold;

            _LTZoomPressedThisFrame = _LTIsActive && !_LTWasActiveLastFrame;
            _RTReleasedThisFrame = !_RTIsActive && _RTWasActiveLastFrame;
        }

        private void DetectActiveInputDevice()
        {

            bool thereIsGamepadInput =
                Input.GetKey(KeyCode.JoystickButton0) || Input.GetKey(KeyCode.JoystickButton1) ||
                Input.GetKey(KeyCode.JoystickButton2) || Input.GetKey(KeyCode.JoystickButton3) ||
                Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.JoystickButton5) ||
                Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.JoystickButton7) ||
                Input.GetKey(KeyCode.JoystickButton8) || Input.GetKey(KeyCode.JoystickButton9) ||
                Input.GetKey(KeyCode.JoystickButton10) || Input.GetKey(KeyCode.JoystickButton11) ||
                Input.GetKey(KeyCode.JoystickButton12) || Input.GetKey(KeyCode.JoystickButton13) ||
                Input.GetKey(KeyCode.JoystickButton14) || Input.GetKey(KeyCode.JoystickButton15) ||
                Input.GetKey(KeyCode.JoystickButton16) || Input.GetKey(KeyCode.JoystickButton17) ||
                Input.GetKey(KeyCode.JoystickButton18) || Input.GetKey(KeyCode.JoystickButton19);

            if (thereIsGamepadInput)
            {
                _isUsingGamepad = true;
                return;
            }

            bool thereIsKeyboardInput =
                Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveForward) ||
                Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveBackward) ||
                Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveRight) ||
                Input.GetKey(Configuration_Configuracion.KeyboardKeyToMoveLeft) ||
                Input.anyKeyDown;

            bool thereIsMouseInput = Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f;

            if (thereIsKeyboardInput || thereIsMouseInput)
                _isUsingGamepad = false;
        }


        private bool RunInputHeld()
        {
            if (_isUsingGamepad)
                return Input.GetKey(Configuration_Configuracion.GamepadButtonToRun);
            return Input.GetKey(Configuration_Configuracion.KeyboardKeyToRun);
        }

        private void ReadRunInput()
        {
            if (!Configuration_Configuracion.AllowThePlayerToRun) return;
            if (Configuration_Configuracion.HoldToRun) return;

            bool runButtonPressed = _isUsingGamepad ? Input.GetKeyDown(Configuration_Configuracion.GamepadButtonToRun) : Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToRun);

            if (runButtonPressed)
                _runToggleModeIsActive = !_runToggleModeIsActive;
        }


        private bool JumpInputPressed()
        {
            if (_isUsingGamepad)
                return Input.GetKeyDown(Configuration_Configuracion.GamepadButtonToJump);
            return Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToJump);
        }

        private bool JumpInputHeld()
        {
            if (_isUsingGamepad)
                return Input.GetKey(Configuration_Configuracion.GamepadButtonToJump);
            return Input.GetKey(Configuration_Configuracion.KeyboardKeyToJump);
        }

        private bool JumpInputReleased()
        {
            if (_isUsingGamepad)
                return Input.GetKeyUp(Configuration_Configuracion.GamepadButtonToJump);
            return Input.GetKeyUp(Configuration_Configuracion.KeyboardKeyToJump);
        }


        private bool InputForCrouch()
        {
            if (_isUsingGamepad)
                return Input.GetKey(Configuration_Configuracion.GamepadButtonToCrouchAndGoProne) && _buttonHoldTimeForCrouchOrProneOnGamepad < Configuration_Configuracion.ButtonHoldTimeToBeConsideredAsProne;
            return Input.GetKey(Configuration_Configuracion.KeyboardKeyToCrouch);
        }

        private bool InputForProne()
        {
            if (_isUsingGamepad)
                return Input.GetKey(Configuration_Configuracion.GamepadButtonToCrouchAndGoProne) && _buttonHoldTimeForCrouchOrProneOnGamepad >= Configuration_Configuracion.ButtonHoldTimeToBeConsideredAsProne;
            return Input.GetKey(Configuration_Configuracion.KeyboardKeyToGoProne);
        }


        private bool ZoomInputPressed()
        {
            if (_isUsingGamepad)
                return Input.GetKeyDown(Configuration_Configuracion.GamepadButtonForZoom) || _LTZoomPressedThisFrame;

            return Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyForZoom) || Input.GetMouseButtonDown(1);
        }

        private bool ZoomInputHeld()
        {
            if (_isUsingGamepad)
                return Input.GetKey(Configuration_Configuracion.GamepadButtonForZoom) || _LTIsActive;
            return Input.GetKey(Configuration_Configuracion.KeyboardKeyForZoom) || Input.GetMouseButton(1);
        }


        private bool PickUpOrDropInputPressed()
        {
            if (_isUsingGamepad)
                return Input.GetKeyDown(Configuration_Configuracion.GamepadButtonToPickUpOrDropAnObject);
            return Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToPickUpOrDropAnObject);
        }

        private bool ActivateRotationInputPressed()
        {
            if (_isUsingGamepad)
                return Input.GetKeyDown(Configuration_Configuracion.GamepadButtonToActivateObjectRotationInHand);
            return Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyToActivateObjectRotationInHand);
        }

        private bool ActivateRotationInputReleased()
        {
            if (_isUsingGamepad)
                return Input.GetKeyUp(Configuration_Configuracion.GamepadButtonToActivateObjectRotationInHand);
            return Input.GetKeyUp(Configuration_Configuracion.KeyboardKeyToActivateObjectRotationInHand);
        }

        private bool ThrowInputHeld()
        {
            if (_isUsingGamepad)
                return Input.GetKey(Configuration_Configuracion.GamepadButtonToThrowTheObjectInHand) || _RTIsActive;
            return Input.GetMouseButton(Configuration_Configuracion.MouseButtonToThrowTheObjectInHand);
        }

        private bool ThrowInputReleased()
        {
            if (_isUsingGamepad)
                return (Input.GetKeyUp(Configuration_Configuracion.GamepadButtonToThrowTheObjectInHand) || _RTReleasedThisFrame) && !_RTIsActive;
            return Input.GetMouseButtonUp(Configuration_Configuracion.MouseButtonToThrowTheObjectInHand);
        }


        private bool DashInputPressed()
        {
            if (_isUsingGamepad)
                return Input.GetKeyDown(Configuration_Configuracion.GamepadButtonForDash);
            return Input.GetKeyDown(Configuration_Configuracion.KeyboardKeyForDash);
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 20 · UTILIDADES PÚBLICAS  /  PUBLIC UTILITIES
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        // En: Public API for other scripts to query or modify player state:
        //     ground, body state, stamina, held object, dash available, input device.
        //     ModifyStamina() and ForceBodyStateChange() allow external modification.
        // Es: API pública para que otros scripts consulten o modifiquen el estado del jugador:
        //     suelo, postura, resistencia, objeto en mano, dash disponible, dispositivo de input.
        //     ModifyStamina() y ForceBodyStateChange() permiten una modificación externa.


        // En: Returns true if the player is in contact with the ground.
        // Es: Devuelve true si el jugador está en contacto con el suelo.
        public bool ThePlayerIsOnTheGround() => _playerIsOnGround;


        // En: Returns the current body state name as a string ("Standing", "Crouching", "Prone", "InTheAir").
        // Es: Devuelve el nombre del estado corporal actual como string ("Standing", "Crouching", "Prone", "InTheAir").
        public string GetNameOfCurrentBodyState() => _currentBodyState.ToString();


        // En: Returns the current stamina normalized between 0 and 1. Useful for external HUD bars.
        // Es: Devuelve la resistencia actual normalizada entre 0 y 1. Útil para barras de HUD externas.
        public float GetNormalizedStamina() => PlayerCurrentStamina / Configuration_Configuracion.MaximumPlayerStamina;


        // En: Returns true if the player currently holds an object in hand.
        // Es: Devuelve true si el jugador tiene un objeto en mano actualmente.
        public bool ThePlayerHasAnObjectInHand() => _currentlyHeldObject != null;


        // En: Returns the object currently held in hand. Returns null if there is none.
        // Es: Devuelve el objeto actualmente en mano. Devuelve null si no hay ninguno.
        public GameObject GetTheObjectCurrentlyInHand() => _currentlyHeldObject;


        // En: Returns true if dash is available (cooldown completed).
        // Es: Devuelve true si el dash está disponible (cooldown completado).
        public bool TheDashIsAvailable() => _timeSinceLastDash >= Configuration_Configuracion.CooldownBetweenDashUses;


        // En: Returns true if the player is currently using a gamepad.
        // Es: Devuelve true si el jugador está usando mando en 'este' momento.
        public bool GamepadIsBeingUsed() => _isUsingGamepad;


        // En: Modifies the current stamina from an external script.
        //     Negative values spend stamina. Positive values restore it without exceeding the maximum.
        // Es: Modifica la resistencia actual desde un script externo.
        //     Valores negativos gastan la resistencia. Valores positivos la recuperan sin superar el máximo claro.
        public void ModifyStamina(float cantidad)
        {
            if (cantidad < 0f) SpendStamina(-cantidad);
            else PlayerCurrentStamina = Mathf.Min(PlayerCurrentStamina + cantidad, Configuration_Configuracion.MaximumPlayerStamina);
        }


        // En: Forces a body state change from an external script, bypassing the cooldown.
        //     targetState: "standing", "crouching" or "prone" (case insensitive).
        // Es: Fuerza un cambio de estado corporal desde un script externo sin pasar por el cooldown.
        //     targetState: "standing", "crouching" o "prone" (no distingue mayúsculas).
        public void ForceBodyStateChange(string targetState)
        {
            switch (targetState.ToLower())
            {
                case "standing": ChangeBodyState(BodyState.Standing, false); break;
                case "crouching": ChangeBodyState(BodyState.Crouching, false); break;
                case "prone": ChangeBodyState(BodyState.Prone, false); break;
            }
        }


        // En: Forces the player to drop the object they are holding.
        // Es: Fuerza al jugador a soltar el objeto que tenga en mano.
        public void ForceDropObjectInHand()
        {
            if (_currentlyHeldObject != null) DropObjectInHand();
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════

    }
}