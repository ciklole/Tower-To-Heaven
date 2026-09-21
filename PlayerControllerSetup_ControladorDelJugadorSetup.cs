// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
// PLAYER CONTROLLER SETUP — Automatic Axis Setup // CONTROLADOR DEL JUGADOR SETUP — Setup Automático de Ejes
// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
//
// En: This script lives in an "Editor" folder and runs automatically on project load or asset
//     import. It registers the right stick and D-pad axes in Unity's Input Manager so the
//     gamepad works plug and play without manual setup.
// Es: Este script vive en una carpeta de tipo "Editor" y se ejecuta automáticamente al importar el asset
//     o al abrir el proyecto. Registra los ejes del joystick derecho y la cruceta en el Input
//     Manager de Unity para que el mando funcione como un plug and play sin configuración manual.
//
// REGISTERED AXES / EJES QUE REGISTRA:
//   · RSHorizontal   — Joystick Axis 4  (Right Stick H / Joystick Derecho Horizontal)
//   · RSVertical     — Joystick Axis 5  (Right Stick V / Joystick Derecho Vertical)
//   · DpadHorizontal — Joystick Axis 6  (D-Pad H / Cruceta Horizontal)
//   · DpadVertical   — Joystick Axis 7  (D-Pad V / Cruceta Vertical)
//   · LT             — Joystick Axis 9  (Left Trigger / Gatillo izquierdo)
//   · RT             — Joystick Axis 10 (Right Trigger / Gatillo derecho)
//
// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using FPC_CPP.Runtime;


namespace FPC_CPP.Editor
{
    [InitializeOnLoad]
    public static class PlayerControllerSetup_ControladorDelJugadorSetup
    {
        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region INICIALIZACIÓN / INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        static PlayerControllerSetup_ControladorDelJugadorSetup()
        {
            RegisterGamepadAxesIfMissing();
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════


        // ═══════════════════════════════════════════════════════════════════════════════════════════
        #region REGISTRO DE EJES / AXIS REGISTRATION
        // ═══════════════════════════════════════════════════════════════════════════════════════════

        [MenuItem("Tools / 48AssetsAndGames / FPC CPP / Register Gamepad Axes - Registrar Ejes del Mando")]
        public static void RegisterGamepadAxesIfMissing()
        {
            var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
            if (inputManagerAsset.Length == 0)
            {
                Debug.LogWarning("[PlayerController_ControladorDelJugador] InputManager.asset not found. Gamepad axes were not registered.");
                return;
            }

            var inputManager = new SerializedObject(inputManagerAsset[0]);
            var axesProperty = inputManager.FindProperty("m_Axes");

            bool huboCambios = false;
            huboCambios |= RegisterAxis(axesProperty, "RSHorizontal", 4, false);
            huboCambios |= RegisterAxis(axesProperty, "RSVertical", 5, true);
            huboCambios |= RegisterAxis(axesProperty, "DpadHorizontal", 6, false);
            huboCambios |= RegisterAxis(axesProperty, "DpadVertical", 7, true);

            huboCambios |= RegisterAxis(axesProperty, "LT", 9, false);
            huboCambios |= RegisterAxis(axesProperty, "RT", 10, false);

            if (huboCambios)
            {
                inputManager.ApplyModifiedProperties();
                Debug.Log("[PlayerController_ControladorDelJugador] Gamepad axes successfully registered in the Input Manager.");
            }
        }


        private static bool RegisterAxis(SerializedProperty axesProperty, string name, int joyAxisNum, bool inverted)
        {

            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                if (axesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue == name)
                    return false;
            }

            axesProperty.arraySize++;
            var eje = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);

            eje.FindPropertyRelative("m_Name").stringValue = name;
            eje.FindPropertyRelative("descriptiveName").stringValue = "";
            eje.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
            eje.FindPropertyRelative("negativeButton").stringValue = "";
            eje.FindPropertyRelative("positiveButton").stringValue = "";
            eje.FindPropertyRelative("altNegativeButton").stringValue = "";
            eje.FindPropertyRelative("altPositiveButton").stringValue = "";
            eje.FindPropertyRelative("gravity").floatValue = 0f;
            eje.FindPropertyRelative("dead").floatValue = 0.19f;
            eje.FindPropertyRelative("sensitivity").floatValue = 1f;
            eje.FindPropertyRelative("snap").boolValue = false;
            eje.FindPropertyRelative("invert").boolValue = inverted;
            eje.FindPropertyRelative("type").intValue = 2;
            eje.FindPropertyRelative("axis").intValue = joyAxisNum - 1;
            eje.FindPropertyRelative("joyNum").intValue = 0;

            return true;
        }

        #endregion
        // ═══════════════════════════════════════════════════════════════════════════════════════════
    }
}
#endif