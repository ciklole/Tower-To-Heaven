

// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
// PLAYER CONFIGURATION - ScriptableObject / CONFIGURACIÓN DEL JUGADOR — ScriptableObject
// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
//
// En: Holds all configurable parameters for the PlayerController_ControladorDelJugador.
//     Field tooltips are in the FPC_CPP_Window (custom Inspector Window), not here.
// Es: Contiene todos los parámetros configurables del PlayerController_ControladorDelJugador.
//     Los tooltips de cada campo están en la FPC_CPP_Window no aqui
//
// SECTIONS / SECCIONES:
//   01 · Movement                        / Movimiento
//   02 · Omni-Directional Movement       / Omnidireccionalidad
//   03 · Body States                     / Estados Corporales
//   04 · Jump                            / Salto
//   05 · Stamina                         / Resistencia
//   06 · Head Bob                        / Balanceo de Cabeza
//   07 · Camera                          / Cámara
//   08 · Zoom                            / Zoom
//   09 · Object Interaction              / Interacción de Objetos
//   10 · Advanced Movement               / Movimiento Avanzado 
//   11 · Input — Keyboard                / Input — Teclado
//   12 · Input — Gamepad                 / Input — Mando
//
// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════

using UnityEngine;

namespace FPC_CPP.Runtime
{
    [CreateAssetMenu(fileName = "PlayerConfiguration_ConfiguracionDelJugador", menuName = "PlayerConfiguration_ConfiguracionDelJugador", order = 0)]
    public class PlayerConfiguration_ConfiguracionDelJugador : ScriptableObject
    {

        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 01 · MOVIMIENTO  /  MOVEMENT
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  01 · MOVIMIENTO  /  MOVEMENT ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        // ── Permisos / Permissions ────────────────────────────────────────────────────────────────────────────────
        public bool AllowThePlayerToWalk = true;
        public bool AllowThePlayerToRun = true;
        public bool AllowThePlayerToRunWhileCrouching = false;
        public bool AllowThePlayerToRunWhileProne = false;
        public bool AllowThePlayerToMoveInTheAir = true;

        [Space(10)]

        // ── Velocidad / Speed  ────────────────────────────────────────────────────────────────────────────────────
        public float BaseSpeedOfThePlayer = 5f;
        public float SpeedMultiplierWhileRunning = 1.8f;

        [Space(10)]

        // ── Modo Correr / Running Mode  ───────────────────────────────────────────────────────────────────────────
        public bool HoldToRun = true;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 02 · OMNIDIRECCIONALIDAD  /  OMNI-DIRECTIONAL MOVEMENT
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  02 · OMNIDIRECCIONALIDAD  /  OMNI-DIRECTIONAL MOVEMENT ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]
        public bool EnableOmniDirectionalMovement = true;

        [Space(10)]

        // ── De Pie / Standing ────────────────────────────────────────────────────────────────────────────────────
        [Header("  — De Pie / Standing ")]
        public float ForwardOmnidirectionalMultiplierWhileStanding = 1.0f;
        public float BackwardOmnidirectionalMultiplierWhileStanding = 0.5f;
        public float LateralOmnidirectionalMultiplierWhileStanding = 0.8f;

        [Space(10)]

        // ── Agachado  / Crouching ─────────────────────────────────────────────────────────────────────────────────
        [Header("  — Agachado / Crouching ")]
        public float ForwardOmnidirectionalMultiplierWhileCrouching = 0.6f;
        public float BackwardOmnidirectionalMultiplierWhileCrouching = 0.4f;
        public float LateralOmnidirectionalMultiplierWhileCrouching = 0.5f;

        [Space(10)]

        // ── Acostado / Prone ─────────────────────────────────────────────────────────────────────────────────────
        [Header("  — Acostado / Prone ")]
        public float ForwardOmnidirectionalMultiplierWhileProne = 0.3f;
        public float BackwardOmnidirectionalMultiplierWhileProne = 0.2f;
        public float LateralOmnidirectionalMultiplierWhileProne = 0.25f;

        [Space(10)]

        // ── En el Aire / In the Air ─────────────────────────────────────────────────────────────────────────────
        [Header("  — En el Aire / In the Air ")]
        public float ForwardOmnidirectionalMultiplierWhileInTheAir = 0.9f;
        public float BackwardOmnidirectionalMultiplierWhileInTheAir = 0.7f;
        public float LateralOmnidirectionalMultiplierWhileInTheAir = 0.8f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 03 · ESTADOS CORPORALES  /  BODY STATES
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  03 · ESTADOS CORPORALES  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        // ── Permisos / Permissions ─────────────────────────────────────────────────────────────────────────────────────────────────────
        public bool AllowThePlayerToCrouch = true;
        public bool HoldToCrouch = false;

        public bool AllowThePlayerToGoProne = true;
        public bool HoldToProne = false;

        [Space(10)]

        // ── Cooldown entre transiciones / Cooldown between transitions ─────────────────────────────────────────────────────────────────
        public bool EnableCooldownBetweenBodyStateTransitions = true;
        public float BodyStateTransitionCooldownTime = 0.2f;

        [Space(10)]

        // ── Velocidades de transición / Transition Speeds ───────────────────────────────────────────────────────────────────────────────
        public float SpeedOfTheCapsuleColliderTransition = 8f;

        [Space(10)]

        // ── Alturas del CapsuleCollider segun estado / CapsuleCollider heights by State ──────────────────────────────────────────────────
        [Header("  — Alturas del CapsuleCollider / CapsuleCollider heights by State ")]
        public float CapsuleColliderHeightWhileStanding = 1.8f;
        public float CapsuleColliderHeightWhileCrouching = 1.0f;
        public float CapsuleColliderHeightWhileProne = 0.4f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 04 · SALTO  /  JUMP
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  04 · SALTO  /  JUMP  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        // ── Permisos / Permissions ────────────────────────────────────────────────────────────────────────────────
        public bool AllowThePlayerToJump = true;
        public int HowManyJumps = 1;
        public bool AllowThePlayerToJumpWhileProne = true;

        [Space(10)]

        // ── Fuerza y Gravedad / Force and Gravity ───────────────────────────────────────────────────────────────────────
        public float ForceAppliedWhenJumping = 6f;
        public float GravityMultiplierDuringTheJump = 2f;
        public float AdditionalGravityMultiplierDuringTheFall = 1.5f;

        [Space(10)]

        // ── Salto Variable / Variable Jump ──────────────────────────────────────────────────────────────────────────
        [Header("  — Salto Variable / Variable Jump ")]
        public bool EnableVariableJump = true;
        public float MaximumHeldTimeOfTheJumpInput = 0.3f;
        public float ExtraForcePerSecondOfTheVariableJump = 15f;

        [Space(10)]

        // ── Detección del Suelo / Ground Detection ─────────────────────────────────────────────────────────────────────
        [Header("  — Detección del Suelo / Ground Detection ")]
        public LayerMask LayersThatAreConsideredGround;
        public float RadiusOfTheGroundDetectionOverlapSphere = 0.25f;
        public float DownwardOffsetOfTheGroundDetectionOverlapSphere = 0.05f;

        [Space(10)]

        // ── Coyote Time ─────────────────────────────────────────────────────────────────────────────
        [Header("  — Coyote Time")]
        public bool EnableCoyoteTime = true;
        public float DurationOfTheCoyoteTime = 0.15f;

        [Space(10)]

        // ── Jump Buffering ──────────────────────────────────────────────────────────────────────────
        [Header("  — Jump Buffering")]
        public bool EnableJumpBuffering = true;
        public float DurationOfTheJumpBuffering = 0.15f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 05 · RESISTENCIA  /  STAMINA
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  05 · RESISTENCIA  /  STAMINA  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]
        public bool EnableTheStaminaSystem = true;
        public float MaximumPlayerStamina = 100f;

        [Space(10)]

        // ── Costes por acción / Costs Per Action ───────────────────────────────────────────────────────────────────────
        [Header("  — Costes por Acción / Costs Per Action ")]
        public bool EnableStaminaCostWhenRunning = true;
        public float StaminaCostPerSecondWhenRunning = 15f;
        public bool EnableStaminaCostWhenJumping = true;
        public float StaminaCostWhenJumping = 10f;
        public bool EnableStaminaCostWhenCrouching = false;
        public float StaminaCostWhenCrouching = 5f;
        public bool EnableStaminaCostWhenGoingProne = false;
        public float StaminaCostWhenGoingProne = 5f;
        public bool EnableStaminaCostOnEachPostureTransition = false;
        public float StaminaCostPerPostureTransition = 3f;
        public bool EnableExtraStaminaCostWhenJumpingFromProne = true;
        public float ExtraStaminaCostWhenJumpingFromTheProneState = 8f;

        [Space(10)]

        // ── Regeneración / Regeneration ────────────────────────────────────────────────────────────────────────────
        [Header("  — Regeneración / Regeneration ")]
        public float DelayInSecondsBeforeStaminaStartsRegenerating = 1.5f;
        public float StaminaRegenerationSpeedWhileThePlayerIsIdle = 25f;
        public float StaminaRegenerationSpeedWhileThePlayerWalks = 10f;

        [Space(10)]

        // ── HUD ─────────────────────────────────────────────────────────────────────────────────────
        [Header("  — HUD")]
        public bool ShowTheStaminaBarOnTheHUD = true;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 06 · BALANCEO DE CABEZA  /  HEAD BOB
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  06 · BALANCEO DE CABEZA  /  HEAD BOB  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]
        public bool EnableTheHeadBobbingSystem = true;

        [Space(10)]

        // ── Al Caminar / When Walking ──────────────────────────────────────────────────────────────────────────────
        [Header("  — Al Caminar / When Walking ")]
        public bool EnableHeadBobbingWhileThePlayerWalks = true;
        public float IntensityOfHeadBobbingWhileThePlayerWalks = 0.05f;
        public float FrequencyOfHeadBobbingWhileThePlayerWalks = 12f;

        [Space(10)]

        // ── Al Correr / When Running ───────────────────────────────────────────────────────────────────────────────
        [Header("  — Al Correr / When Running ")]
        public bool EnableHeadBobbingWhileThePlayerRuns = true;
        public float IntensityOfHeadBobbingWhileThePlayerRuns = 0.1f;
        public float FrequencyOfHeadBobbingWhileThePlayerRuns = 16f;

        [Space(10)]

        // ── Reactivo / Reactive ────────────────────────────────────────────────────────────────────────────────
        [Header("  — Reactivo (Salto · Aterrizaje · Posturas) / Reactive (Jump · Landing · Postures) ")]
        public bool EnableReactiveHeadBobbingWhenJumpingAndLanding = true;
        public float IntensityOfReactiveHeadBobbingWhenJumping = 0.08f;
        public float IntensityOfReactiveHeadBobbingWhenLanding = 0.12f;
        public bool EnableReactiveHeadBobbingWhenCrouching = true;
        public float IntensityOfReactiveHeadBobbingWhenCrouching = 0.07f;
        public bool EnableReactiveHeadBobbingWhenGoingProne = true;
        public float IntensityOfReactiveHeadBobbingWhenGoingProne = 0.12f;
        public float ReturnSpeedOfReactiveHeadBobbingToTheNeutralPosition = 10f;

        [Space(10)]

        // ── Al Deslizarse / When Sliding ────────────────────────────────────────────────────────────────────────────
        [Header("  — Al Deslizarse / When Sliding ")]
        public bool EnableReactiveHeadBobbingWhenSliding = true;
        public float IntensityOfReactiveHeadBobbingWhenSliding = 0.15f;

        [Space(10)]

        // ── Al Hacer Dash / When Dashing ────────────────────────────────────────────────────────────────────────────
        [Header("  — Al Hacer Dash / When Dashing ")]
        public bool EnableReactiveHeadBobbingWhenDashing = true;
        public float IntensityOfReactiveHeadBobbingWhenDashing = 0.1f;

        [Space(10)]

        // ── Respiración según Resistencia / Breathing according to Stamina ────────────────────────────────────────────────────────────
        [Header("  — Respiración según Resistencia / Breathing according to Stamina ")]
        public bool EnableBreathingEffect = true;
        [Range(8f, 40f)]
        public float BreathsPerMinuteAtRest = 14f;
        [Range(8f, 40f)]
        public float BreathsPerMinuteWhenExhausted = 30f;
        [Range(0.1f, 0.9f)]
        public float BreathingInhaleFraction = 0.3f;
        public float BreathingBasePitchIntensity = 3f;
        public float BreathingExhaustedPitchIntensity = 5f;
        public float BreathingBaseTranslationIntensity = 0.05f;
        public float BreathingExhaustedTranslationIntensity = 0.05f;
        public float BreathingExhaustionFadeInSpeed = 1.5f;
        public float BreathingExhaustionFadeOutSpeed = 0.8f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 07 · CÁMARA  /  CAMERA
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  07 · CÁMARA  /  CAMERA  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        // ── Campo de Visión / Field of View ─────────────────────────────────────────────────────────────────────────
        public float BaseFieldOfViewOfTheCamera = 75f;

        [Space(10)]

        // ── Speed FOV ────────────────────────────────────────────────────────────────────────────────────
        [Header("  — Velocidad En El FOV / Speed On FOV ")]
        public bool EnableSpeedFOVEffect = true;
        public float SpeedFOVStartPercent = 0.8f;
        public float SpeedFOVMaxPercent = 1.6f;
        public float SpeedFOVMaxMultiplier = 1.15f;
        public float SpeedFOVTransitionSpeed = 6f;

        [Space(10)]

        // ── Sensibilidad / Sensitivity ────────────────────────────────────────────────────────────────────────────
        [Header("  — Sensibilidad / Sensitivity ")]
        public float HorizontalMouseSensitivity = 2f;
        public float VerticalMouseSensitivity = 2f;

        [Space(10)]

        // ── Límites verticales / Vertical Limits ──────────────────────────────────────────────────────────────────────
        [Header("  — Límites Verticales / Vertical Limits ")]
        public float UpperVerticalLimitOfTheCamera = 80f;
        public float LowerVerticalLimitOfTheCamera = 80f;

        [Space(10)]

        // ── Mira / Crosshair ───────────────────────────────────────────────────────────────────────────────
        [Header("  — Mira / Crosshair")]
        public bool ShowTheCrosshairOnTheHUD = true;

        [Space(10)]

        // ── Alturas de la Cámara por Estado / Camera Heights by Body State ────────────────────────────────────────────────────────────
        [Header("  — Alturas de la Cámara por Estado Corporal / Camera Heights by Body State ")]
        public float HeightOfTheCameraSupportWhileStanding = 1.6f;
        public float HeightOfTheCameraSupportWhileCrouching = 0.7f;
        public float HeightOfTheCameraSupportWhileProne = 0.2f;
        public float SpeedOfTheCameraHeightTransition = 8f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 08 · ZOOM  /  ZOOM
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  08 · ZOOM  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]
        public bool EnableTheZoomSystem = true;
        public bool HoldToZoom = true;
        public bool AllowZoomWhileHoldingAnObject = true;

        [Space(10)]

        // ── Valores del FOV al hacer zoom / FOV Values While Zooming ──────────────────────────────────────────────────────────────────────────
        [Header("  — Valores del FOV al hacer zoom / FOV Values While Zooming ")]
        public float CameraFieldOfViewDuringZoom = 40f;
        public float SpeedOfTheFieldOfViewTransitionDuringZoom = 8f;

        [Space(10)]

        // ── Sensibilidad durante el Zoom / Sensitivity During Zoom ────────────────────────────────────────────────────────────
        [Header("  — Sensibilidad durante el Zoom / Sensitivity During Zoom ")]
        public bool ReduceSensitivityDuringZoom = true;
        public float SensitivityMultiplierDuringZoom = 0.5f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 09 · INTERACCIÓN CON OBJETOS  /  OBJECT INTERACTION
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  09 · INTERACCIÓN DE OBJETOS  /  OBJECT INTERACTION  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]
        public bool EnableTheObjectInteractionSystem = true;

        [Space(10)]

        // ── Recoger y Soltar / Pick Up and Drop ────────────────────────────────────────────────────────────────────────
        [Header("  — Recoger y Soltar / Pick Up and Drop ")]
        public string TagOfPickableObjects = "Recogible";
        public float MaximumDistanceToPickUpAnObject = 3f;
        public bool DisableTheObjectColliderWhenPickingItUp = true;

        public float SpeedOfTheObjectMovementTowardsTheAnchorPoint = 15f;

        [Space(10)]

        // ── Rotación del Objeto en Mano / Object Rotation in Hand ─────────────────────────────────────────────────────────────────────
        [Header("  — Rotación del Objeto en Mano / Object Rotation in Hand ")]
        public bool HoldToRotateTheObject = false;
        public float RotationSpeedOfTheObjectInHand = 90f;

        [Space(10)]

        // ── Lanzar Objeto / Throw Object ───────────────────────────────────────────────────────────────────────────
        [Header("  — Lanzar Objeto / Throw Object ")]
        public bool TakeIntoAccountTheMassOfTheObjectWhenThrowingIt = false;
        public float MinimumObjectThrowForce = 5f;
        public float MaximumObjectThrowForce = 20f;
        public float MaximumChargeTimeOfTheObjectThrow = 1.5f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 10 · MOVIMIENTO AVANZADO - Deslizamiento · Dash  /  ADVANCED MOVEMENT  —  Slide · Dash
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  10 · MOVIMIENTO AVANZADO - Deslizamiento · Dash  /  ADVANCED MOVEMENT  —  Slide · Dash  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        // ── Deslizamiento / Sliding ───────────────────────────────────────────────────────────────────────────
        [Header("  — Deslizamiento / Sliding ")]
        public bool EnableTheSlidingSystem = true;
        public bool UseRealPhysicsInSliding = false;
        public bool AllowJumpingDuringSliding = true;
        public bool AllowInterruptingSliding = true;
        public float RecoveryTimeAfterSliding = 0.4f;
        public bool AllowRunningImmediatelyAfterSliding = false;
        public bool EnableStaminaCostWhenSliding = true;
        public float StaminaCostWhenSliding = 15f;
        public bool AllowSlidingOnAnySurface = true;

        [Space(5)]

        // ── Deslizamiento Arcade / Arcade Sliding ────────────────────────────────────────────────────────────────────
        [Header("    · Deslizamiento Arcade / Arcade Sliding ")]
        public float DurationOfSlidingInArcadeMode = 0.8f;
        public float InitialSpeedMultiplierOfArcadeSliding = 2.5f;
        public float MinimumSpeedToKeepSliding = 1f;

        [Space(5)]

        // ── Deslizamiento Físico / Physical Sliding ────────────────────────────────────────────────────────────────────
        [Header("    · Deslizamiento Físico / Physical Sliding ")]
        public float FrictionDuringPhysicalSliding = 0.3f;
        public float SlopeAccelerationMultiplierDuringSliding = 1.5f;

        // ── Sensibilidad de Cámara al Deslizarse / Camera Sensitivity While Sliding ────────────────────────────────────────────────────
        [Header("    · Sensibilidad de Cámara al Deslizarse / Camera Sensitivity While Sliding ")]
        public bool ReduceCameraSensitivityDuringSliding = true;
        [Range(0.1f, 1f)]
        public float CameraSensitivityMultiplierDuringSliding = 0.5f;

        [Space(10)]

        // ── Dash ────────────────────────────────────────────────────────────────────────────────────
        [Header("  — Dash")]
        public bool EnableTheDashSystem = true;
        public bool AllowDashInTheAir = true;
        public float DashForce = 15f;
        public float DurationOfTheDashImpulse = 0.2f;
        public float CooldownBetweenDashUses = 1f;
        public bool RequireDoublePressForDashOnGamepad = false;
        public float MaximumTimeBetweenTheTwoPressesForDashDoublePress = 0.3f;
        public bool AllowDashDuringSliding = true;
        public bool EnableStaminaCostWhenUsingDash = true;
        public float StaminaCostWhenUsingDash = 20f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 11 · INPUT — TECLADO  /  INPUT — KEYBOARD
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  11 · INPUT — TECLADO  /  INPUT — KEYBOARD  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]
        public KeyCode KeyboardKeyToMoveForward = KeyCode.W;
        public KeyCode KeyboardKeyToMoveBackward = KeyCode.S;
        public KeyCode KeyboardKeyToMoveLeft = KeyCode.A;
        public KeyCode KeyboardKeyToMoveRight = KeyCode.D;
        public KeyCode KeyboardKeyToRun = KeyCode.LeftShift;
        public KeyCode KeyboardKeyToJump = KeyCode.Space;
        public KeyCode KeyboardKeyToCrouch = KeyCode.C;
        public KeyCode KeyboardKeyToGoProne = KeyCode.V;
        public KeyCode KeyboardKeyForZoom = KeyCode.Z;
        public KeyCode KeyboardKeyToPickUpOrDropAnObject = KeyCode.E;
        public KeyCode KeyboardKeyToActivateObjectRotationInHand = KeyCode.R;
        public KeyCode KeyboardKeyToRotateObjectUp = KeyCode.UpArrow;
        public KeyCode KeyboardKeyToRotateObjectDown = KeyCode.DownArrow;
        public KeyCode KeyboardKeyToRotateObjectLeft = KeyCode.LeftArrow;
        public KeyCode KeyboardKeyToRotateObjectRight = KeyCode.RightArrow;
        public int MouseButtonToThrowTheObjectInHand = 0;
        public KeyCode KeyboardKeyForDash = KeyCode.Q;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region 12 · INPUT — MANDO  /  INPUT — GAMEPAD
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [Header("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  12 · INPUT — MANDO  /  INPUT — GAMEPAD  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
        [Space(10)]

        [Space(5)]
        public float LeftStickDeadZone = 0.2f;
        public float RightStickDeadZone = 0.2f;

        [Space(10)]
        [Header("  — Botones de Acción / Action Buttons ")]
        public KeyCode GamepadButtonToJump = KeyCode.JoystickButton0;
        public KeyCode GamepadButtonToCrouchAndGoProne = KeyCode.JoystickButton1;
        public KeyCode GamepadButtonToPickUpOrDropAnObject = KeyCode.JoystickButton2;
        public KeyCode GamepadButtonToActivateObjectRotationInHand = KeyCode.JoystickButton5;
        public KeyCode GamepadButtonForDash = KeyCode.JoystickButton4;
        public KeyCode GamepadButtonToRun = KeyCode.JoystickButton8;

        [Space(10)]
        [Header("  — Gatillos / Triggers ")]
        public KeyCode GamepadButtonForZoom = KeyCode.JoystickButton9;
        public KeyCode GamepadButtonToThrowTheObjectInHand = KeyCode.JoystickButton10;

        [Space(10)]
        [Header("  — Tiempo de Pulsación para Acostarse / Button Hold Time to go Prone")]
        public float ButtonHoldTimeToBeConsideredAsProne = 0.4f;

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════

    }

}