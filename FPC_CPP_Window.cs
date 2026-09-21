using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using FPC_CPP.Runtime;

namespace FPC_CPP.Editor
{
    public class FPC_CPP_Window : EditorWindow
    {
        private static FPC_CPP_Window _instancia;

        internal PlayerConfiguration_ConfiguracionDelJugador Cfg;
        internal SerializedObject SO;

        internal const string PREF_IDIOMA = "CPPFPC_Idioma";
        internal bool ES = true;

        internal static readonly Color C_FONDO = new Color(0.08f, 0.08f, 0.09f, 1f);
        internal static readonly Color C_BLOQUE = new Color(0.05f, 0.05f, 0.06f, 1f);
        internal static readonly Color C_BORDE = new Color(0.95f, 0.95f, 1.00f, 1f);
        internal static readonly Color C_BORDE_SUB = new Color(0.65f, 0.65f, 0.72f, 1f);
        internal static readonly Color C_ACENTO = new Color(0.35f, 0.75f, 1.00f, 1f);
        internal static readonly Color C_TEXTO = new Color(1.00f, 1.00f, 1.00f, 1f);
        internal static readonly Color C_TEXTO_SUB = new Color(0.80f, 0.82f, 0.88f, 1f);
        internal static readonly Color C_VERDE_LED = new Color(0.10f, 0.95f, 0.40f, 1f);
        internal static readonly Color C_VERDE = new Color(0.20f, 0.80f, 0.35f, 1f);
        internal static readonly Color C_ROJO = new Color(0.90f, 0.20f, 0.20f, 1f);
        internal static readonly Color C_AZUL = new Color(0.15f, 0.50f, 0.90f, 1f);
        internal static readonly Color C_GHOST = new Color(0.35f, 0.75f, 1.00f, 0.22f);
        internal static readonly Color C_GHOST_BORDE = new Color(0.35f, 0.75f, 1.00f, 0.85f);
        internal static readonly Color C_RESIZE = new Color(0.35f, 0.75f, 1.00f, 0.70f);

        private static Font _fuentePersonalizada;
        private static bool _fuenteBuscada = false;

        private static Font ObtenerFuente()
        {
            if (_fuenteBuscada) return _fuentePersonalizada;
            _fuenteBuscada = true;
            Font[] fuentes = Resources.LoadAll<Font>("Dico");
            if (fuentes != null && fuentes.Length > 0)
            {
                _fuentePersonalizada = fuentes[0];
                return _fuentePersonalizada;
            }
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Font", new[] { "Assets/48A-01FPC_CPP" });
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                _fuentePersonalizada = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(path);
            }
#endif
            return _fuentePersonalizada;
        }

        internal GUIStyle StTitulo;
        internal GUIStyle StSeccion;
        internal GUIStyle StLabel;
        internal GUIStyle StLabelSub;
        internal GUIStyle StLabelInfo;
        internal GUIStyle StBoton;
        internal GUIStyle StBotonPeligro;
        internal GUIStyle StFoldout;
        internal GUIStyle StTooltipBox;
        internal bool _estilosListos;

        internal class BloqueCanvas
        {
            public int ID;
            public string TituloES;
            public string TituloEN;
            public string Icono;
            public Rect Rect;
            public bool Visible;
            public bool Plegado;
            public Vector2 Scroll;
            public float AlturaContenido;

            public bool Escalando;
            public Vector2 EscaladoOrigen;
            public Rect RectAlEscalar;

            public BloqueCanvas(int id, string es, string en, string icono, Rect rect)
            {
                ID = id; TituloES = es; TituloEN = en; Icono = icono;
                Rect = rect; Visible = false; Plegado = false;
            }
        }

        internal const int B_MECBASE = 0;
        internal const int B_MECAVZ = 1;
        internal const int B_CAMARA = 2;
        internal const int B_INPUT = 3;
        internal const int B_SAVE = 4;
        internal const int B_DOCS = 5;
        internal const int B_CREDITS = 6;
        internal const int B_CONFIG = B_MECBASE;

        internal List<BloqueCanvas> _bloques = new List<BloqueCanvas>();

        private int _bloqueArrastrando = -1;
        private Vector2 _offsetArrastre;
        private bool _arrastreDesdeCanvas;
        private Vector2 _posicionGhost;
        private bool _hayGhost;
        private int _bloqueGhost = -1;

        private int _bloqueEscalando = -1;
        private Vector2 _escalaOrigen;
        private Rect _rectAlIniciarEscalado;
        private int _escalaBorde = 0;
        private const float ZONA_RESIZE = 14f;
        private const float ANCHO_MIN = 260f;
        private const float ALTO_MIN = 80f;

        private const float ANCHO_DOCK = 200f;
        private const float ALTO_CABECERA = 70f;
        private const float ALTO_BARRA = 28f;

        private Vector2 _scrollCanvas;

        private string _tooltipActivo = "";
        private Vector2 _tooltipMousePos;

        internal string NotaDock = "48 :)";

        internal string NombreNuevoJSON = "";
        internal List<string> ArchivosJSON = new List<string>();
        private const string CARPETA_JSON_FALLBACK = "Assets/48A-01-FPC_CPP/Configs";
        private const string NOMBRE_CARPETA_CONFIGS = "Configs";
        private const string NOMBRE_CARPETA_PADRE = "48A-01-FPC_CPP";

        internal static string ObtenerCarpetaJSON()
        {

            if (Directory.Exists(CARPETA_JSON_FALLBACK)) return CARPETA_JSON_FALLBACK;

            string[] guids = AssetDatabase.FindAssets("t:Folder");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("/" + NOMBRE_CARPETA_CONFIGS) && path.Contains(NOMBRE_CARPETA_PADRE))
                    return path;
            }

            return CARPETA_JSON_FALLBACK;
        }

        internal bool[] SeccionAbierta = new bool[12];

        internal bool IndTeclado = true;
        internal bool IndGamepad = false;

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region APERTURA  /  WINDOW OPEN
        // ════════════════════════════════════════════════════════════════════════════════════════════

        [MenuItem("Tools / 48AssetsAndGames / FPC CPP / Window Inspector")]
        public static void AbrirVentana()
        {
            _instancia = GetWindow<FPC_CPP_Window>(false, "FPC - CPP Window Inspector", true);
            _instancia.minSize = new Vector2(640, 480);
            _instancia.Show();
        }

        public static void AbrirConAsset(PlayerConfiguration_ConfiguracionDelJugador cfg)
        {
            AbrirVentana();
            _instancia.AsignarAsset(cfg);
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region INICIALIZACIÓN  /  INITIALIZATION
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void OnEnable()
        {
            ES = EditorPrefs.GetBool(PREF_IDIOMA, true);

            for (int i = 0; i < SeccionAbierta.Length; i++)
                SeccionAbierta[i] = EditorPrefs.GetBool($"FPC_Sec_{i}", true);

            InicializarBloques();
            CargarEstadoBloques();
            ActualizarListaJSON();

            wantsMouseMove = true;
            titleContent = new GUIContent(ES ? "FPC CPP — Window Inspector" : "FPC CPP — Window Inspector");

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (Cfg != null)
                SO = new SerializedObject(Cfg);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(PREF_IDIOMA, ES);
            for (int i = 0; i < SeccionAbierta.Length; i++)
                EditorPrefs.SetBool($"FPC_Sec_{i}", SeccionAbierta[i]);
            GuardarEstadoBloques();

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                _estilosListos = false;

                if (Cfg != null)
                    SO = new SerializedObject(Cfg);
                else
                    SO = null;

                Repaint();
            }
        }

        private void InicializarBloques()
        {
            _bloques.Clear();

            float cx = 10f, cy = 10f;
            _bloques.Add(new BloqueCanvas(B_MECBASE, "Mecánicas Base", "Base Mechanics", "48",
                new Rect(cx, cy, 400f, 560f)));
            _bloques.Add(new BloqueCanvas(B_MECAVZ, "Mecánicas Avanzadas", "Advanced Mechanics", "48",
                new Rect(cx + 50f, cy + 30f, 400f, 520f)));
            _bloques.Add(new BloqueCanvas(B_CAMARA, "Cámara", "Camera", "48",
                new Rect(cx + 100f, cy + 60f, 380f, 400f)));
            _bloques.Add(new BloqueCanvas(B_INPUT, "Input", "Input", "48",
                new Rect(cx + 150f, cy + 90f, 420f, 460f)));
            _bloques.Add(new BloqueCanvas(B_SAVE, "Guardado", "Save", "48",
                new Rect(cx + 460f, cy, 320f, 280f)));
            _bloques.Add(new BloqueCanvas(B_DOCS, "Documentación", "Documentation", "48",
                new Rect(cx + 460f, cy + 300f, 320f, 220f)));
            _bloques.Add(new BloqueCanvas(B_CREDITS, "Créditos", "Credits", "48",
                new Rect(cx + 460f, cy + 540f, 320f, 200f)));
        }

        private void GuardarEstadoBloques()
        {
            foreach (var b in _bloques)
            {
                EditorPrefs.SetBool($"CPPFPC_BV_{b.ID}", b.Visible);
                EditorPrefs.SetBool($"CPPFPC_BP_{b.ID}", b.Plegado);
                EditorPrefs.SetFloat($"CPPFPC_BX_{b.ID}", b.Rect.x);
                EditorPrefs.SetFloat($"CPPFPC_BY_{b.ID}", b.Rect.y);
                EditorPrefs.SetFloat($"CPPFPC_BW_{b.ID}", b.Rect.width);
                EditorPrefs.SetFloat($"CPPFPC_BH_{b.ID}", b.Rect.height);
            }
        }

        private void CargarEstadoBloques()
        {
            foreach (var b in _bloques)
            {
                if (!EditorPrefs.HasKey($"CPPFPC_BV_{b.ID}")) continue;
                b.Visible = EditorPrefs.GetBool($"CPPFPC_BV_{b.ID}");
                b.Plegado = EditorPrefs.GetBool($"CPPFPC_BP_{b.ID}");
                b.Rect.x = EditorPrefs.GetFloat($"CPPFPC_BX_{b.ID}", b.Rect.x);
                b.Rect.y = EditorPrefs.GetFloat($"CPPFPC_BY_{b.ID}", b.Rect.y);
                b.Rect.width = EditorPrefs.GetFloat($"CPPFPC_BW_{b.ID}", b.Rect.width);
                b.Rect.height = EditorPrefs.GetFloat($"CPPFPC_BH_{b.ID}", b.Rect.height);
            }
        }

        internal void AsignarAsset(PlayerConfiguration_ConfiguracionDelJugador cfg)
        {
            Cfg = cfg;
            SO = cfg != null ? new SerializedObject(cfg) : null;
            ActualizarListaJSON();
            Repaint();
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region INICIALIZAR ESTILOS  /  INITIALIZE STYLES
        // ════════════════════════════════════════════════════════════════════════════════════════════


        internal void InicializarEstilos()
        {
            if (_estilosListos) return;

            Font fuentePX = ObtenerFuente();

            StTitulo = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                normal = { textColor = C_TEXTO },
                font = fuentePX
            };

            StSeccion = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                normal = { textColor = C_TEXTO },
                hover = { textColor = C_TEXTO },
                font = fuentePX
            };
            if (fuentePX != null)
            {
                StSeccion.normal.background = StSeccion.normal.background;
                StSeccion.hover.background = StSeccion.hover.background;
            }

            StLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = C_TEXTO },
                wordWrap = false,
                fontStyle = FontStyle.Normal,
                font = fuentePX
            };
            StLabelSub = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Normal,
                normal = { textColor = C_TEXTO_SUB },
                wordWrap = false,
                font = fuentePX
            };
            StLabelInfo = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                normal = { textColor = C_TEXTO },
                wordWrap = false,
                padding = new RectOffset(6, 6, 4, 4)
            };
            StBoton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                normal = { textColor = Color.white }
            };
            StBotonPeligro = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                normal = { textColor = Color.white }
            };

            StFoldout = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                font = fuentePX
            };
            StFoldout.normal.textColor = C_TEXTO;
            StFoldout.onNormal.textColor = C_TEXTO;
            StFoldout.hover.textColor = C_TEXTO;
            StFoldout.onHover.textColor = C_TEXTO;
            StFoldout.focused.textColor = C_ACENTO;
            StFoldout.onFocused.textColor = C_ACENTO;
            StFoldout.active.textColor = C_ACENTO;
            StFoldout.onActive.textColor = C_ACENTO;

            StTooltipBox = new GUIStyle(GUI.skin.box)
            {
                fontSize = 10,
                normal = { textColor = C_TEXTO_SUB },
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(6, 6, 4, 4)
            };

            _estilosListos = true;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region LOOP PRINCIPAL  /  MAIN LOOP
        // ════════════════════════════════════════════════════════════════════════════════════════════


        private void OnGUI()
        {

            if (position.width < 10f || position.height < 10f)
                return;

            InicializarEstilos();

            _tooltipActivo = "";

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), C_FONDO);

            if (Cfg == null)
            {
                DibujarSelectorDeAsset();
                return;
            }

            if (SO == null || !SO.targetObject)
                SO = new SerializedObject(Cfg);

            SO.Update();

            DibujarCabecera();

            float yInicio = ALTO_CABECERA + 4f;
            Rect areaTotal = new Rect(0, yInicio, position.width, position.height - yInicio);

            Rect rectDock = new Rect(areaTotal.xMax - ANCHO_DOCK, areaTotal.y, ANCHO_DOCK, areaTotal.height);
            Rect rectCanvas = new Rect(areaTotal.x, areaTotal.y, areaTotal.width - ANCHO_DOCK - 1f, areaTotal.height);

            EditorGUI.DrawRect(new Rect(rectCanvas.xMax, areaTotal.y, 2f, areaTotal.height), C_BORDE);

            DibujarCanvas(rectCanvas);
            DibujarDock(rectDock);
            DibujarGhost();

            DibujarTooltipFlotante();

            if (SO != null)
                SO.ApplyModifiedProperties();

            if (_bloqueArrastrando >= 0 || _bloqueEscalando >= 0)
                Repaint();
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region SELECTOR DE ASSET  /  ASSET SELECTOR
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void DibujarSelectorDeAsset()
        {
            float cx = position.width * 0.5f;
            float cy = position.height * 0.5f;

            string titulo = "                           48A-01-FPC_CPP\n\n";
            string sub = ES ? "Assign a PlayerConfiguration_ConfiguracionDelJugador asset to begin\n\n" +
                              "Asigna un asset PlayerConfiguration_ConfiguracionDelJugador para comenzar"
                            : "Asigna un asset PlayerConfiguration_ConfiguracionDelJugador para comenzar\n\n" +
                              "Assign a PlayerConfiguration_ConfiguracionDelJugador asset to begin";

            GUI.Label(new Rect(cx - 160f, cy - 60f, 620f, 60f), titulo, StTitulo);
            GUI.Label(new Rect(cx - 160f, cy - 24f, 620f, 80f), sub, StLabelSub);

            Cfg = (PlayerConfiguration_ConfiguracionDelJugador)EditorGUI.ObjectField(
                new Rect(cx - 162f, cy + 44f, 320f, 22f),
                ES ? "Asset_SO:" : "Asset_SO:", Cfg,
                typeof(PlayerConfiguration_ConfiguracionDelJugador), false);

            if (Cfg != null)
                AsignarAsset(Cfg);
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region CABECERA  /  HEADER
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void DibujarCabecera()
        {
            Rect cabRect = new Rect(0, 0, position.width, ALTO_CABECERA);
            EditorGUI.DrawRect(cabRect, C_BLOQUE);
            DibujarBordeGrosor(new Rect(0, ALTO_CABECERA - 2f, position.width, 2f), C_BORDE, 2f);

            string titulo = ES ? "CPP  -  Controlador de Primer Persona" : "FPC  -  First Person Controller";

            GUI.Label(new Rect(12f, 6f, 460f, 22f), titulo, StTitulo);
            GUI.Label(new Rect(14f, 26f, 200f, 13f), "v1.0", new GUIStyle(StLabelSub) { fontSize = 12, normal = { textColor = C_ACENTO } });

            float xAsset = position.width * 0.38f;

            GUI.Label(new Rect(xAsset, 14f, 50f, 20f), ES ? "Asset:" : "Asset:", StLabelSub);
            PlayerConfiguration_ConfiguracionDelJugador nuevo = (PlayerConfiguration_ConfiguracionDelJugador)EditorGUI.ObjectField(new Rect(xAsset + 46f, 12f, 180f, 20f), Cfg, typeof(PlayerConfiguration_ConfiguracionDelJugador), false);

            if (nuevo != Cfg)
                AsignarAsset(nuevo);

            ActualizarIndicadoresInput();

            float xInd = xAsset + 240f;

            DibujarIndicador(new Rect(xInd, 10f, 80f, 26f), ES ? "Teclado" : "Keyboard", IndTeclado);
            DibujarIndicador(new Rect(xInd + 86f, 10f, 76f, 26f), ES ? "Mando" : "Gamepad", IndGamepad);

            float xLang = position.width - 280f;

            DibujarSwitchIdioma(new Rect(xLang, 38f, 268f, 28f));
        }

        private void ActualizarIndicadoresInput()
        {
            IndTeclado = true;
            string[] joysticks = Input.GetJoystickNames();
            IndGamepad = false;

            if (joysticks != null)
                foreach (var j in joysticks)
                    if (!string.IsNullOrEmpty(j)) { IndGamepad = true; break; }
        }

        private void DibujarIndicador(Rect rect, string label, bool activo)
        {
            Rect circulo = new Rect(rect.x, rect.y + 7f, 13f, 13f);
            DibujarCirculo(circulo, activo ? C_VERDE_LED : new Color(0.18f, 0.18f, 0.20f, 1f));
            GUI.Label(new Rect(rect.x + 17f, rect.y + 5f, rect.width - 17f, 20f), label, StLabelSub);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (label == "Teclado" || label == "Keyboard")
                    IndTeclado = !IndTeclado;
                else
                    IndGamepad = !IndGamepad;
                Event.current.Use();
                Repaint();
            }
        }

        private void DibujarSwitchIdioma(Rect rect)
        {
            DibujarPXFondo(rect, new Color(0.04f, 0.04f, 0.05f, 1f), C_BORDE_SUB, 1);

            float mid = rect.x + rect.width * 0.5f;
            float cy = rect.y + rect.height * 0.5f;
            float ledS = 12f;

            Rect rLED1 = new Rect(rect.x + 6f, cy - ledS * 0.5f, ledS, ledS);
            DibujarCirculo(rLED1, C_VERDE_LED);
            GUI.Label(new Rect(rect.x + 22f, cy - 8f, mid - rect.x - 26f, 16f), ES ? "Español" : "English", new GUIStyle(StLabel) { fontSize = 10, fontStyle = FontStyle.Normal });

            EditorGUI.DrawRect(new Rect(mid - 0.5f, rect.y + 4f, 1f, rect.height - 8f), C_BORDE_SUB);

            Rect rLED2 = new Rect(mid + 6f, cy - ledS * 0.5f, ledS, ledS);
            DibujarCirculo(rLED2, new Color(0.22f, 0.22f, 0.24f, 1f));
            Rect rBtn = new Rect(mid + 22f, rect.y + 3f, rect.xMax - mid - 26f, rect.height - 6f);
            Color cA = GUI.backgroundColor;
            GUI.backgroundColor = Color.clear;
            if (GUI.Button(rBtn, ES ? "English" : "Español", new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = C_TEXTO_SUB }, font = ObtenerFuente() }))
            {
                ES = !ES;
                EditorPrefs.SetBool(PREF_IDIOMA, ES);
                _estilosListos = false;
                Repaint();
            }
            GUI.backgroundColor = cA;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region CANVAS LIBRE  /  FREE CANVAS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void DibujarCanvas(Rect rectCanvas)
        {
            GUI.BeginClip(rectCanvas);
            Rect local = new Rect(0, 0, rectCanvas.width, rectCanvas.height);
            EditorGUI.DrawRect(local, C_FONDO);

            DibujarCuadricula(local);

            Event e = Event.current;
            Vector2 mouseLocal = e.mousePosition;

            ProcesarEscalado(mouseLocal, e);
            ProcesarDropEnCanvas(mouseLocal, e, rectCanvas);

            for (int i = 0; i < _bloques.Count; i++)
            {
                if (!_bloques[i].Visible) continue;
                if (i == _bloqueArrastrando) continue;
                DibujarBloqueEnCanvas(_bloques[i], mouseLocal, e);
            }

            if (_bloqueArrastrando >= 0 && _bloques[_bloqueArrastrando].Visible)
                DibujarBloqueEnCanvas(_bloques[_bloqueArrastrando], mouseLocal, e);

            GUI.EndClip();
        }

        private void DibujarCuadricula(Rect area)
        {
            Color cSmall = new Color(0.14f, 0.14f, 0.16f, 1f);
            Color cBig = new Color(0.22f, 0.22f, 0.26f, 1f);
            for (float x = 0; x < area.width; x += 8f)
            {
                bool mayor = ((int)(x) % 32 == 0);
                EditorGUI.DrawRect(new Rect(x, 0, 1f, area.height), mayor ? cBig : cSmall);
            }
            for (float y = 0; y < area.height; y += 8f)
            {
                bool mayor = ((int)(y) % 32 == 0);
                EditorGUI.DrawRect(new Rect(0, y, area.width, 1f), mayor ? cBig : cSmall);
            }
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region BLOQUE EN CANVAS  /  CANVAS BLOCK
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void DibujarBloqueEnCanvas(BloqueCanvas b, Vector2 mouseLocal, Event e)
        {
            bool estaArrastrando = (_bloqueArrastrando == b.ID);
            bool estaEscalando = (_bloqueEscalando == b.ID);

            EditorGUI.DrawRect(new Rect(b.Rect.x + 4f, b.Rect.y + 4f, b.Rect.width, b.Rect.height), new Color(0, 0, 0, 0.45f));

            Color colorBorde = (estaArrastrando || estaEscalando) ? C_ACENTO : C_BORDE;
            DibujarPXFondo(b.Rect, C_BLOQUE, colorBorde, (estaArrastrando || estaEscalando) ? 2 : 1);

            Rect barraRect = new Rect(b.Rect.x, b.Rect.y, b.Rect.width, ALTO_BARRA);
            DibujarPXFondo(barraRect, new Color(0.06f, 0.06f, 0.08f, 1f), colorBorde, 1);
            EditorGUI.DrawRect(new Rect(b.Rect.x + 2f, b.Rect.y + ALTO_BARRA - 1f, b.Rect.width - 4f, 1f), colorBorde);

            string titulo = ES ? b.TituloES : b.TituloEN;
            GUI.Label(new Rect(barraRect.x + 8f, barraRect.y + 5f, barraRect.width - 60f, 20f), titulo, StSeccion);

            Rect btnPlegar = new Rect(barraRect.xMax - 52f, barraRect.y + 4f, 22f, 20f);
            if (GUI.Button(btnPlegar, b.Plegado ? "▶" : "▼", StLabelSub))
                b.Plegado = !b.Plegado;

            Rect btnCerrar = new Rect(barraRect.xMax - 26f, barraRect.y + 4f, 20f, 20f);
            Color cAnterior = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.5f, 0.1f, 0.1f, 1f);
            if (GUI.Button(btnCerrar, "✕", StLabelSub))
            {
                b.Visible = false;
                if (_bloqueArrastrando == b.ID)
                    _bloqueArrastrando = -1;
            }
            GUI.backgroundColor = cAnterior;

            if (!b.Plegado)
            {
                float cw = b.Rect.width - 2f;
                float ch = b.Rect.height - ALTO_BARRA - 1f;

                if (cw < 2f || ch < 2f) return;

                Rect contenidoRect = new Rect(b.Rect.x + 1f, b.Rect.y + ALTO_BARRA, cw, ch);

                GUI.BeginClip(contenidoRect);
                Rect scrollView = new Rect(0, 0, contenidoRect.width, contenidoRect.height);
                Rect scrollContent = new Rect(0, 0, contenidoRect.width - 14f, Mathf.Max(b.AlturaContenido, contenidoRect.height));

                b.Scroll = GUI.BeginScrollView(scrollView, b.Scroll, scrollContent);

                float alturaUsada = DibujarContenidoBloque(b, scrollContent.width);
                b.AlturaContenido = alturaUsada;

                GUI.EndScrollView();
                GUI.EndClip();
            }

            {
                float z = ZONA_RESIZE;
                float bx = b.Rect.x, by = b.Rect.y, bw = b.Rect.width, bh = b.Rect.height;

                Rect zR = new Rect(bx + bw - z, by + ALTO_BARRA, z, bh - ALTO_BARRA);
                Rect zL = new Rect(bx, by + ALTO_BARRA, z, bh - ALTO_BARRA);
                Rect zB = new Rect(bx + z, by + bh - z, bw - z * 2f, z);
                Rect zSE = new Rect(bx + bw - z, by + bh - z, z, z);
                Rect zSW = new Rect(bx, by + bh - z, z, z);

                EditorGUIUtility.AddCursorRect(zR, MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(zL, MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(zB, MouseCursor.ResizeVertical);
                EditorGUIUtility.AddCursorRect(zSE, MouseCursor.ResizeUpLeft);
                EditorGUIUtility.AddCursorRect(zSW, MouseCursor.ResizeUpRight);

                Color cMarca = estaEscalando ? C_ACENTO : C_BORDE_SUB;
                for (int px = 0; px < 3; px++)
                    for (int py = 0; py < 3; py++)
                        if (px + py < 4)
                            EditorGUI.DrawRect(new Rect(bx + bw - 8f + px * 3f, by + bh - 8f + py * 3f, 2f, 2f), cMarca);

                bool enZona = zR.Contains(mouseLocal) || zL.Contains(mouseLocal) || zB.Contains(mouseLocal) || zSE.Contains(mouseLocal) || zSW.Contains(mouseLocal);
                bool sobreBotones = btnCerrar.Contains(mouseLocal) || btnPlegar.Contains(mouseLocal);

                if (e.type == EventType.MouseDown && enZona && !sobreBotones && _bloqueEscalando < 0 && _bloqueArrastrando < 0)
                {
                    _bloqueEscalando = b.ID;
                    _escalaOrigen = mouseLocal;
                    _rectAlIniciarEscalado = b.Rect;
                    _escalaBorde = zL.Contains(mouseLocal) ? -1 : zR.Contains(mouseLocal) ? 1 : zSW.Contains(mouseLocal) ? -2 : zSE.Contains(mouseLocal) ? 2 : 0;
                    e.Use();
                }
            }

            if (e.type == EventType.MouseDown && barraRect.Contains(mouseLocal) && !btnPlegar.Contains(mouseLocal) && !btnCerrar.Contains(mouseLocal) && _bloqueArrastrando < 0 && _bloqueEscalando < 0)
            {
                _bloqueArrastrando = b.ID;
                _arrastreDesdeCanvas = true;
                _offsetArrastre = mouseLocal - new Vector2(b.Rect.x, b.Rect.y);
                _hayGhost = false;
                e.Use();
            }

            if (_bloqueArrastrando == b.ID && _arrastreDesdeCanvas && e.type == EventType.MouseDrag)
            {
                b.Rect.x = mouseLocal.x - _offsetArrastre.x;
                b.Rect.y = mouseLocal.y - _offsetArrastre.y;
                e.Use();
                Repaint();
            }

            if (_bloqueArrastrando == b.ID && _arrastreDesdeCanvas && e.type == EventType.MouseUp)
            {
                _bloqueArrastrando = -1;
                e.Use();
            }
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region ESCALADO  /  RESIZE
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void ProcesarEscalado(Vector2 mouseLocal, Event e)
        {
            if (_bloqueEscalando < 0) return;

            BloqueCanvas b = _bloques[_bloqueEscalando];

            if (e.type == EventType.MouseDrag)
            {
                Vector2 delta = mouseLocal - _escalaOrigen;
                float x0 = _rectAlIniciarEscalado.x;
                float y0 = _rectAlIniciarEscalado.y;
                float w0 = _rectAlIniciarEscalado.width;
                float h0 = _rectAlIniciarEscalado.height;

                if (_escalaBorde == 1 || _escalaBorde == 2)
                    b.Rect.width = Mathf.Max(ANCHO_MIN, w0 + delta.x);
                if (_escalaBorde == -1 || _escalaBorde == -2)
                {
                    float nw = Mathf.Max(ANCHO_MIN, w0 - delta.x);
                    b.Rect.x = x0 + w0 - nw;
                    b.Rect.width = nw;
                }
                if (_escalaBorde == 0 || _escalaBorde == 2 || _escalaBorde == -2)
                    b.Rect.height = Mathf.Max(ALTO_MIN, h0 + delta.y);
                e.Use();
                EditorApplication.delayCall += Repaint;
            }

            if (e.type == EventType.MouseUp)
            {
                _bloqueEscalando = -1;
                e.Use();
                Repaint();
            }
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region DROP EN CANVAS  /  DROP ON CANVAS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void ProcesarDropEnCanvas(Vector2 mouseLocal, Event e, Rect rectCanvas)
        {
            if (_bloqueArrastrando < 0 || _arrastreDesdeCanvas) return;

            BloqueCanvas b = _bloques[_bloqueArrastrando];

            if (e.type == EventType.MouseDrag)
            {
                _posicionGhost = e.mousePosition;
                _hayGhost = true;
                Repaint();
            }

            if (e.type == EventType.MouseUp)
            {
                float yCanvas = ALTO_CABECERA + 4f;
                Vector2 posEnCanvas = new Vector2(e.mousePosition.x, e.mousePosition.y - yCanvas);

                if (posEnCanvas.x >= 0 && posEnCanvas.x <= rectCanvas.width && posEnCanvas.y >= 0 && posEnCanvas.y <= rectCanvas.height)
                {
                    b.Visible = true;
                    b.Rect = new Rect(posEnCanvas.x - b.Rect.width * 0.5f, posEnCanvas.y - ALTO_BARRA * 0.5f, b.Rect.width, b.Rect.height);
                    b.Rect.x = Mathf.Max(0, b.Rect.x);
                    b.Rect.y = Mathf.Max(0, b.Rect.y);
                }

                _bloqueArrastrando = -1;
                _hayGhost = false;
                e.Use();
                Repaint();
            }
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region DOCK  /  DOCK
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private static readonly string[] _dockGrupoES = { "CPP — Configuration_Configuracion", "CPP — Guardar Configuration_Configuracion", "Documentación", "Créditos" };
        private static readonly string[] _dockGrupoEN = { "FPC — Configuration_Configuracion", "FPC — Save Configuration_Configuracion", "Documentation", "Credits" };
        private static readonly int[][] _dockGrupoIDs = { new[] { B_MECBASE, B_MECAVZ, B_CAMARA, B_INPUT }, new[] { B_SAVE }, new[] { B_DOCS }, new[] { B_CREDITS }, };

        private void DibujarDock(Rect rectDock)
        {
            EditorGUI.DrawRect(rectDock, new Color(0.04f, 0.04f, 0.05f, 1f));
            DibujarBordeGrosor(new Rect(rectDock.x, rectDock.y, 1.5f, rectDock.height), C_BORDE, 1.5f);

            GUI.BeginClip(rectDock);

            float y = 8f;
            float ancho = rectDock.width - 16f;

            GUI.Label(new Rect(8f, y, ancho, 18f), ES ? "Bloques disponibles" : "Available blocks", StLabelSub);
            y += 22f;
            EditorGUI.DrawRect(new Rect(4f, y, rectDock.width - 8f, 1f), C_BORDE);
            y += 8f;

            for (int g = 0; g < _dockGrupoIDs.Length; g++)
            {
                string labelGrupo = ES ? _dockGrupoES[g] : _dockGrupoEN[g];

                float txtW = Mathf.Min(EditorStyles.miniLabel.CalcSize(new GUIContent(labelGrupo)).x + 12f, ancho - 8f);
                float lineW = (ancho - txtW) * 0.5f - 2f;
                float lineY = y + 7f;
                Color cLinea = new Color(C_ACENTO.r, C_ACENTO.g, C_ACENTO.b, 0.30f);
                EditorGUI.DrawRect(new Rect(8f, lineY, lineW, 1f), cLinea);
                EditorGUI.DrawRect(new Rect(8f + lineW + txtW + 4f, lineY, lineW, 1f), cLinea);
                GUI.Label(new Rect(8f + lineW + 2f, y, txtW, 14f), labelGrupo, new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(C_ACENTO.r, C_ACENTO.g, C_ACENTO.b, 0.75f) }, font = ObtenerFuente(), fontSize = 8 });
                y += 18f;

                foreach (int id in _dockGrupoIDs[g])
                {
                    BloqueCanvas b = null;
                    for (int i = 0; i < _bloques.Count; i++)
                        if (_bloques[i].ID == id) { b = _bloques[i]; break; }
                    if (b == null) continue;

                    Rect tarjeta = new Rect(8f, y, ancho, 62f);
                    DibujarTarjetaDock(b, tarjeta);
                    y += 68f;
                }

                y += 2f;
            }

            if (_bloqueArrastrando >= 0 && _arrastreDesdeCanvas)
            {
                Rect zonaDrop = new Rect(4f, y, rectDock.width - 8f, 40f);
                EditorGUI.DrawRect(zonaDrop, C_GHOST);
                DibujarBorde(zonaDrop, C_GHOST_BORDE);
                GUI.Label(new Rect(zonaDrop.x + 6f, zonaDrop.y + 10f, zonaDrop.width - 12f, 20f), ES ? "Arrastra a aquí para minimizar" : "Drop here to minimize", StLabelSub);

                Event e = Event.current;
                if (e.type == EventType.MouseUp)
                {
                    _bloques[_bloqueArrastrando].Visible = false;
                    _bloqueArrastrando = -1;
                    _hayGhost = false;
                    e.Use();
                    Repaint();
                }
            }

            float yNota = rectDock.height - 36f;
            EditorGUI.DrawRect(new Rect(4f, yNota - 1f, rectDock.width - 8f, 1f), C_BORDE_SUB);
            GUI.Label(new Rect(8f, yNota + 2f, 40f, 14f), ES ? "Nota:" : "Note:", new GUIStyle(StLabelSub) { fontSize = 8 });
            NotaDock = EditorGUI.TextField(new Rect(48f, yNota + 1f, rectDock.width - 54f, 20f), NotaDock, new GUIStyle(EditorStyles.textField) { font = ObtenerFuente(), fontSize = 9, normal = { textColor = C_TEXTO } });
            GUI.EndClip();

            ProcesarDropEnDock(rectDock);
        }

        private void DibujarTarjetaDock(BloqueCanvas b, Rect tarjeta)
        {
            bool visible = b.Visible;
            Color fondoTarjeta = visible ? new Color(0.12f, 0.22f, 0.35f, 1f) : new Color(0.09f, 0.09f, 0.10f, 1f);

            DibujarPXFondo(tarjeta, fondoTarjeta, visible ? C_ACENTO : C_BORDE, 1);

            GUI.Label(new Rect(tarjeta.x + 6f, tarjeta.y + 4f, 22f, 22f), b.Icono, StSeccion);

            string titulo = ES ? b.TituloES : b.TituloEN;
            string tituloCorto = titulo.Contains("  ") ? titulo.Substring(titulo.IndexOf("  ") + 2) : titulo;
            GUI.Label(new Rect(tarjeta.x + 30f, tarjeta.y + 4f, tarjeta.width - 34f, 20f), tituloCorto, StLabelSub);

            string sub = b.ID switch
            {
                B_MECBASE => ES ? "OmniDireccionalidad · Salto · Resistencia" : "OmniDirectionality · Jump · Stamina",
                B_MECAVZ => ES ? "Interac con Objs · Mov Avanzado · Zoom" : "Obj Interaction · Advanced Movmt · Zoom",
                B_CAMARA => ES ? "Cámara · Balanceo de Cabeza" : "Camera · Head Bob",
                B_INPUT => ES ? "Teclado · Mando" : "Keyboard · Gamepad",
                B_SAVE => ES ? "Guardar Config · Cargar Config" : "Save Config · Load Config",
                B_DOCS => ES ? "Links a la Doc · Guía Básica del Asset" : "Links to the Doc · Basic Guide of the Asset",
                B_CREDITS => ES ? "Versión · Autor" : "Version · Author",
                _ => ""
            };
            GUI.Label(new Rect(tarjeta.x + 6f, tarjeta.y + 22f, tarjeta.width - 12f, 14f), sub, new GUIStyle(StLabelSub) { fontSize = 8, normal = { textColor = C_BORDE_SUB } });

            Rect btnToggle = new Rect(tarjeta.x + 6f, tarjeta.y + tarjeta.height - 22f, tarjeta.width - 12f, 18f);
            Color cAnterior = GUI.backgroundColor;
            GUI.backgroundColor = visible ? new Color(0.3f, 0.1f, 0.1f, 1f) : new Color(0.1f, 0.3f, 0.1f, 1f);
            if (GUI.Button(btnToggle, visible ? (ES ? "Remover" : "Remove") : (ES ? "Agregar" : "Show"), StLabelSub))
            {
                b.Visible = !b.Visible;
                Repaint();
            }
            GUI.backgroundColor = cAnterior;

            Event e = Event.current;
            if (e.type == EventType.MouseDown && tarjeta.Contains(e.mousePosition) && !visible && _bloqueArrastrando < 0)
            {
                _bloqueArrastrando = b.ID;
                _arrastreDesdeCanvas = false;
                _posicionGhost = e.mousePosition + new Vector2(position.x, position.y);
                _hayGhost = true;
                _bloqueGhost = b.ID;
                e.Use();
            }
        }

        private void ProcesarDropEnDock(Rect rectDock)
        {
            if (_bloqueArrastrando < 0 || !_arrastreDesdeCanvas) return;

            Event e = Event.current;

            if (e.type == EventType.MouseUp && rectDock.Contains(e.mousePosition))
            {
                _bloques[_bloqueArrastrando].Visible = false;
                _bloqueArrastrando = -1;
                _hayGhost = false;
                e.Use();
                Repaint();
            }
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region GHOST  /  GHOST PREVIEW
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void DibujarGhost()
        {
            if (!_hayGhost || _bloqueArrastrando < 0) return;

            BloqueCanvas b = _bloques[_bloqueArrastrando];

            float ghostAncho = _arrastreDesdeCanvas ? b.Rect.width : Mathf.Min(b.Rect.width, 200f);
            float ghostAlto = _arrastreDesdeCanvas ? ALTO_BARRA + 20f : 56f;

            Event e = Event.current;
            if (e.type == EventType.MouseDrag)
                _posicionGhost = e.mousePosition;

            Rect ghostRect = new Rect(_posicionGhost.x - ghostAncho * 0.5f, _posicionGhost.y - ghostAlto * 0.5f, ghostAncho, ghostAlto);

            EditorGUI.DrawRect(new Rect(ghostRect.x + 3f, ghostRect.y + 3f, ghostRect.width, ghostRect.height), new Color(0, 0, 0, 0.4f));

            EditorGUI.DrawRect(ghostRect, C_GHOST);
            DibujarPX(ghostRect, C_GHOST_BORDE, 2);

            string titulo = ES ? b.TituloES : b.TituloEN;
            GUI.Label(new Rect(ghostRect.x + 8f, ghostRect.y + 6f, ghostRect.width - 16f, 22f), titulo, StSeccion);

            if (e.type == EventType.MouseDrag || e.type == EventType.MouseMove)
                Repaint();
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region TOOLTIP FLOTANTE  /  FLOATING TOOLTIP
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void DibujarTooltipFlotante()
        {
            if (string.IsNullOrEmpty(_tooltipActivo)) return;

            GUIContent content = new GUIContent(_tooltipActivo);
            float maxW = 260f;
            Vector2 tam;
            tam.x = Mathf.Min(StTooltipBox.CalcSize(content).x + 16f, maxW);
            tam.y = StTooltipBox.CalcHeight(new GUIContent(_tooltipActivo), tam.x) + 8f;

            float offsetX = 18f;
            float offsetY = 16f;
            float tx = Mathf.Clamp(_tooltipMousePos.x + offsetX, 0f, position.width - tam.x - 2f);
            float ty = Mathf.Clamp(_tooltipMousePos.y + offsetY, 0f, position.height - tam.y - 2f);

            Rect tipRect = new Rect(tx, ty, tam.x, tam.y);

            EditorGUI.DrawRect(tipRect, new Color(0.06f, 0.06f, 0.07f, 0.97f));
            DibujarBorde(tipRect, C_BORDE_SUB);
            GUI.Label(tipRect, _tooltipActivo, StTooltipBox);
        }

        internal void RegistrarTooltip(Rect rectFila, string texto, Vector2 mousePosEnVentana)
        {
            if (!string.IsNullOrEmpty(texto) && rectFila.Contains(Event.current.mousePosition))
            {
                _tooltipActivo = texto;
                _tooltipMousePos = mousePosEnVentana;
            }
        }

        internal void LimpiarTooltip()
        {
            _tooltipActivo = "";
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region CONTENIDO DE BLOQUES  /  BLOCK CONTENT
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private float DibujarContenidoBloque(BloqueCanvas b, float ancho)
        {
            switch (b.ID)
            {
                case B_MECBASE: return DibujarContenidoMecBase(ancho);
                case B_MECAVZ: return DibujarContenidoMecAvz(ancho);
                case B_CAMARA: return DibujarContenidoCamara(ancho);
                case B_INPUT: return DibujarContenidoInput(ancho);
                case B_SAVE: return DibujarContenidoGuardado(ancho);
                case B_DOCS: return DibujarContenidoDocumentacion(ancho);
                case B_CREDITS: return DibujarContenidoCreditos(ancho);
            }
            return 100f;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region HELPERS DE DIBUJO  /  DRAWING HELPERS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        internal void DibujarBorde(Rect r, Color c)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1f), c);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1f, r.width, 1f), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y, 1f, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - 1f, r.y, 1f, r.height), c);
        }

        internal void DibujarBordeGrosor(Rect r, Color c, float g)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, g), c);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - g, r.width, g), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y, g, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - g, r.y, g, r.height), c);
        }

        internal bool BotonColoreado(Rect r, string texto, Color color)
        {
            Color ca = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool result = GUI.Button(r, texto, StBoton);
            GUI.backgroundColor = ca;
            return result;
        }

        internal void DibujarPX(Rect r, Color c, int grosor)
        {
            float g = grosor;
            float e = g + 1f;
            EditorGUI.DrawRect(new Rect(r.x + e, r.y, r.width - e * 2f, g), c);
            EditorGUI.DrawRect(new Rect(r.x + e, r.yMax - g, r.width - e * 2f, g), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y + e, g, r.height - e * 2f), c);
            EditorGUI.DrawRect(new Rect(r.xMax - g, r.y + e, g, r.height - e * 2f), c);
            float d = g;
            EditorGUI.DrawRect(new Rect(r.x + d, r.y + d - 1f, 1f, 1f), c);
            EditorGUI.DrawRect(new Rect(r.xMax - d - 1f, r.y + d - 1f, 1f, 1f), c);
            EditorGUI.DrawRect(new Rect(r.x + d, r.yMax - d, 1f, 1f), c);
            EditorGUI.DrawRect(new Rect(r.xMax - d - 1f, r.yMax - d, 1f, 1f), c);
        }

        internal void DibujarPXFondo(Rect r, Color fondo, Color borde, int grosor)
        {
            float e = grosor + 1f;
            EditorGUI.DrawRect(new Rect(r.x + e, r.y + e, r.width - e * 2f, r.height - e * 2f), fondo);
            EditorGUI.DrawRect(new Rect(r.x + grosor, r.y + e, e - grosor, r.height - e * 2f), fondo);
            EditorGUI.DrawRect(new Rect(r.xMax - e, r.y + e, e - grosor, r.height - e * 2f), fondo);
            EditorGUI.DrawRect(new Rect(r.x + e, r.y + grosor, r.width - e * 2f, e - grosor), fondo);
            EditorGUI.DrawRect(new Rect(r.x + e, r.yMax - e, r.width - e * 2f, e - grosor), fondo);
            DibujarPX(r, borde, grosor);
        }

        internal void DibujarCirculo(Rect r, Color color)
        {
            bool activo = (color == C_VERDE || color == C_VERDE_LED || color.g > 0.5f);
            Color bordeColor = activo ? new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f) : new Color(0.18f, 0.18f, 0.20f, 1f);
            EditorGUI.DrawRect(r, bordeColor);
            float inset = Mathf.Max(1.5f, r.width * 0.2f);
            Rect rInner = new Rect(r.x + inset, r.y + inset, r.width - inset * 2f, r.height - inset * 2f);
            Color centroColor = activo ? color : new Color(0.25f, 0.25f, 0.28f, 1f);
            EditorGUI.DrawRect(rInner, centroColor);
            float c = r.width * 0.22f;
            EditorGUI.DrawRect(new Rect(r.x, r.y, c, c), C_BLOQUE);
            EditorGUI.DrawRect(new Rect(r.xMax - c, r.y, c, c), C_BLOQUE);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - c, c, c), C_BLOQUE);
            EditorGUI.DrawRect(new Rect(r.xMax - c, r.yMax - c, c, c), C_BLOQUE);
            if (activo)
            {
                Rect rGlow = new Rect(rInner.x + 1f, rInner.y + 1f, rInner.width * 0.4f, rInner.height * 0.4f);
                EditorGUI.DrawRect(rGlow, new Color(1f, 1f, 1f, 0.35f));
            }
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region CONSTANTES DE LAYOUT INTERNO  /  INTERNAL LAYOUT CONSTANTS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private const float PAD = 10f;
        private const float FILA_H = 20f;
        private const float SEP_H = 6f;
        private const float LABEL_W = 550f;
        private const float HEADER_H = 22f;

        private const float KC_LABEL_W = 180f;

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region GRUPOS DE CONTENIDO  /  CONTENT GROUPS
        // ════════════════════════════════════════════════════════════════════════════════════════════


        private (string es, string en, int idx, System.Func<float, float, float> fn)[] ObtenerDefinicionSecciones()
        {
            return new (string es, string en, int idx, System.Func<float, float, float> fn)[] {
            ("01 · Movimiento",                           "01 · Movement",               0,  DibujarS01),
            ("02 · Omnidireccionalidad",                  "02 · Omnidirectionality",     1,  DibujarS02),
            ("03 · Estados Corporales",                   "03 · Body States",            2,  DibujarS03),
            ("04 · Salto",                                "04 · Jump",                   3,  DibujarS04),
            ("05 · Resistencia",                          "05 · Stamina",                4,  DibujarS05),
            ("06 · Movimiento Avanzado",                  "06 · Advanced Movement",      5,  DibujarS06),
            ("07 · Interacción de Objetos",               "07 · Object Interaction",     6,  DibujarS07),
            ("08 · Zoom",                                 "08 · Zoom",                   7,  DibujarS08),
            ("09 · Cámara",                               "09 · Camera",                 9,  DibujarS09),
            ("10 · Balanceo de Cabeza",                   "10 · Head Bob",               8,  DibujarS10),
            ("11 · Teclado",                              "11 · Keyboard",               10, DibujarS11),
            ("12 · Mando",                                "12 · Gamepad",                11, DibujarS12),
        };
        }

        private float DibujarContenidoConfigGrupo(float ancho, int[] indices)
        {
            if (Cfg == null) return 0f;
            float y = PAD;
            var secs = ObtenerDefinicionSecciones();
            foreach (int i in indices)
            {
                if (i < 0 || i >= secs.Length) continue;
                var sec = secs[i];
                y = SecBase(y, ancho, sec.idx, ES ? sec.es : sec.en, sec.fn);
            }
            return y + PAD;
        }

        private float DibujarContenidoMecBase(float ancho) => DibujarContenidoConfigGrupo(ancho, new int[] { 0, 1, 2, 3, 4 });
        private float DibujarContenidoMecAvz(float ancho) => DibujarContenidoConfigGrupo(ancho, new int[] { 5, 6, 7 });
        private float DibujarContenidoCamara(float ancho) => DibujarContenidoConfigGrupo(ancho, new int[] { 8, 9 });
        private float DibujarContenidoInput(float ancho) => DibujarContenidoConfigGrupo(ancho, new int[] { 10, 11 });

        private bool[] _seccionesActivas = new bool[12];

        private float DibujarContenidoConfig(float ancho)
        {
            if (Cfg == null) return 0f;

            float y = PAD;

            EditorGUI.DrawRect(new Rect(PAD, y, ancho - PAD * 2f, 1f), C_BORDE_SUB);
            y += 6f;

            var secciones = new (string es, string en, int idx, System.Func<float, float, float> fn)[] {
            ("Movimiento",    "Movement",      0,  DibujarS01),
            ("Omnidirec.",    "Omnidirec.",    1,  DibujarS02),
            ("Estados",       "Body States",   2,  DibujarS03),
            ("Salto",         "Jump",          3,  DibujarS04),
            ("Resistencia",   "Stamina",       4,  DibujarS05),
            ("Mov. Avanzado", "Adv. Movement", 9,  DibujarS06),
            ("Zoom",          "Zoom",          7,  DibujarS07),
            ("Interacción",   "Interaction",   8,  DibujarS08),
            ("Cámara",        "Camera",        6,  DibujarS09),
            ("Balanceo",      "Head Bob",      5,  DibujarS10),
            ("Teclado",       "Keyboard",      10, DibujarS11),
            ("Mando",         "Gamepad",       11, DibujarS12),
        };

            int cols = 3;
            float margen = PAD;
            float espacio = 4f;
            float btnAlto = 28f;

            int[][] grupos = new int[][] {
            new int[]{0,1,2},
            new int[]{3,4,5},
            new int[]{6,7,8,9},
            new int[]{10,11}
        };
            string[] grupoNombresES = { "Movimiento", "Salto · Resistencia", "Cámara · Avanzado", "Input" };
            string[] grupoNombresEN = { "Movement", "Jump · Stamina", "Camera · Advanced", "Input" };

            for (int g = 0; g < grupos.Length; g++)
            {
                GUI.Label(new Rect(margen, y, ancho - margen * 2f, 13f), ES ? grupoNombresES[g] : grupoNombresEN[g], new GUIStyle(StLabelSub) { fontSize = 8, normal = { textColor = C_ACENTO } });
                y += 14f;

                int[] grpIdx = grupos[g];
                int gCols = Mathf.Min(cols, grpIdx.Length);
                float gBtnW = (ancho - margen * 2f - espacio * (gCols - 1)) / gCols;

                for (int gi = 0; gi < grpIdx.Length; gi++)
                {
                    int i = grpIdx[gi];
                    int col = gi % gCols;
                    int fila = gi / gCols;
                    float bx = margen + col * (gBtnW + espacio);
                    float by2 = y + fila * (btnAlto + espacio);
                    Rect rBtn = new Rect(bx, by2, gBtnW, btnAlto);

                    bool activo = _seccionesActivas[i];
                    Color cFondo = activo ? new Color(0.10f, 0.20f, 0.36f, 1f) : new Color(0.06f, 0.06f, 0.09f, 1f);
                    Color cBrd = activo ? C_ACENTO : C_BORDE_SUB;
                    DibujarPXFondo(rBtn, cFondo, cBrd, 1);

                    Rect rLed = new Rect(rBtn.x + 5f, rBtn.y + rBtn.height * 0.5f - 5f, 10f, 10f);
                    DibujarCirculo(rLed, activo ? C_VERDE_LED : new Color(0.22f, 0.22f, 0.25f, 1f));

                    string btnLabel = ES ? secciones[i].es : secciones[i].en;
                    if (GUI.Button(new Rect(rBtn.x + 18f, rBtn.y + 2f, rBtn.width - 20f, rBtn.height - 4f),
                        btnLabel, new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 10,
                            normal = { textColor = activo ? C_TEXTO : C_TEXTO_SUB },
                            font = ObtenerFuente(),
                            alignment = TextAnchor.MiddleLeft
                        }))
                    {
                        _seccionesActivas[i] = !activo;
                    }
                }
                int filasCnt = Mathf.CeilToInt(grpIdx.Length / (float)gCols);
                y += filasCnt * (btnAlto + espacio) + 6f;
            }

            y += 4f;

            for (int i = 0; i < secciones.Length; i++)
            {
                if (!_seccionesActivas[i]) continue;
                EditorGUI.DrawRect(new Rect(PAD, y, ancho - PAD * 2f, 1f), C_BORDE_SUB);
                y += 6f;
                GUI.Label(new Rect(PAD + 4f, y, ancho, 18f), ES ? secciones[i].es : secciones[i].en, new GUIStyle(StSeccion) { normal = { textColor = C_ACENTO } });
                y += 22f;
                y = secciones[i].fn(y, ancho);
                y += 4f;
                EditorGUI.DrawRect(new Rect(PAD, y, ancho - PAD * 2f, 1f), C_ACENTO);
                y += 8f;
            }

            return y + PAD;
        }

        private float SecBase(float y, float ancho, int idx, string titulo, System.Func<float, float, float> dibujador)
        {
            Rect hRect = new Rect(PAD, y, ancho - PAD * 2f, HEADER_H);
            DibujarPXFondo(hRect, new Color(0.04f, 0.04f, 0.05f, 1f), C_BORDE, 1);

            Rect fRect = new Rect(hRect.x + 6f, hRect.y + 3f, hRect.width - 12f, 16f);
            SeccionAbierta[idx] = EditorGUI.Foldout(fRect, SeccionAbierta[idx], titulo, true, StFoldout);
            y += HEADER_H + 2f;

            if (!SeccionAbierta[idx]) return y + 2f;

            float yFin = dibujador(y, ancho);
            y = yFin;

            EditorGUI.DrawRect(new Rect(PAD * 2f, y, ancho - PAD * 4f, 1f), C_BORDE_SUB);
            return y + 6f;
        }

        private float GrupoAvanzado(float y, float ancho)
        {
            Rect gRect = new Rect(PAD, y, ancho - PAD * 2f, HEADER_H + 2f);
            EditorGUI.DrawRect(gRect, new Color(0.08f, 0.12f, 0.18f, 1f));
            DibujarBordeGrosor(gRect, C_ACENTO, 1.5f);
            GUI.Label(new Rect(gRect.x + 8f, gRect.y + 4f, gRect.width, 18f), ES ? "Mecánicas Avanzadas" : "Advanced Mechanics", StSeccion);
            y += HEADER_H + 6f;

            float xInd = PAD + 8f;
            float anchoInd = ancho - PAD * 2f - 8f;

            y = SecBaseIndent(y, xInd, anchoInd, 7, ES ? "08 · Zoom" : "08 · Zoom", DibujarS08);
            y = SecBaseIndent(y, xInd, anchoInd, 8, ES ? "07 · Interacción de Objetos" : "07 · Object Interaction", DibujarS07);
            y = SecBaseIndent(y, xInd, anchoInd, 9, ES ? "06 · Movimiento Avanzado" : "06 · Advanced Movement", DibujarS06);

            EditorGUI.DrawRect(new Rect(PAD, y, ancho - PAD * 2f, 1f), C_ACENTO);
            return y + 8f;
        }

        private float SecBaseIndent(float y, float x, float ancho, int idx, string titulo, System.Func<float, float, float> dibujador)
        {
            Rect hRect = new Rect(x, y, ancho, HEADER_H);
            DibujarPXFondo(hRect, new Color(0.06f, 0.08f, 0.12f, 1f), C_BORDE_SUB, 1);

            Rect fRect = new Rect(hRect.x + 6f, hRect.y + 3f, hRect.width - 12f, 16f);
            SeccionAbierta[idx] = EditorGUI.Foldout(fRect, SeccionAbierta[idx], titulo, true, StFoldout);
            y += HEADER_H + 2f;

            if (!SeccionAbierta[idx]) return y + 2f;

            float yFin = dibujador(y, x + ancho + PAD);
            y = yFin;
            EditorGUI.DrawRect(new Rect(x, y, ancho, 0.5f), C_BORDE_SUB);
            return y + 6f;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region PRIMITIVAS DE CAMPO  /  FIELD PRIMITIVES
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private float SubHeader(float y, float ancho, string texto)
        {
            y += SEP_H;
            Rect r = new Rect(PAD * 2f, y, ancho - PAD * 4f, 16f);
            EditorGUI.DrawRect(new Rect(r.x, r.y + 7f, r.width * 0.25f, 0.5f), C_ACENTO);
            GUI.Label(new Rect(r.x + r.width * 0.25f + 4f, r.y, r.width * 0.74f, 16f), texto, new GUIStyle(StLabelSub) { normal = { textColor = C_ACENTO } });
            return y + 18f;
        }

        private float SubHeaderN2(float y, float ancho, string texto)
        {
            y += 3f;
            GUI.Label(new Rect(PAD * 3f, y, ancho, 14f), texto, StLabelSub);
            return y + 16f;
        }

        private float InfoBox(float y, float ancho, string texto)
        {
            float maxAncho = ancho - PAD * 4f;
            float h = StLabelInfo.CalcHeight(new GUIContent(texto), maxAncho) + 8f;
            Rect r = new Rect(PAD * 2f, y, maxAncho, h);
            EditorGUI.DrawRect(r, new Color(0.08f, 0.10f, 0.14f, 1f));
            DibujarBorde(r, C_BORDE_SUB);
            GUI.Label(new Rect(r.x + 4f, r.y + 4f, r.width - 8f, h - 8f), texto, StLabelInfo);
            return y + h + 4f;
        }

        private Vector2 MouseEnVentana()
        {
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            return new Vector2(screenPos.x - position.x, screenPos.y - position.y);
        }

        private float CampoBool(float y, float ancho, string prop, string labelES, string labelEN, string tipES, string tipEN, float labelAncho = -1f)
        {
            SerializedProperty p = SO.FindProperty(prop);
            if (p == null) return y;
            string label = ES ? labelES : labelEN;
            string tip = ES ? tipES : tipEN;
            float lw = labelAncho < 0f ? LABEL_W - 16f : labelAncho;
            float filaH = Mathf.Max(FILA_H, StLabel.CalcHeight(new GUIContent(label), lw));
            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, filaH);
            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                _tooltipActivo = tip;
                _tooltipMousePos = MouseEnVentana();
            }
            Rect circulo = new Rect(rFila.x + 2f, rFila.y + filaH * 0.5f - 5f, 10f, 10f);
            Rect rLabel = new Rect(rFila.x + 16f, rFila.y, lw, filaH);
            Rect rToggle = new Rect(rFila.xMax - 36f, rFila.y + filaH * 0.5f - 9f, 32f, 18f);
            DibujarCirculo(circulo, p.boolValue ? C_VERDE_LED : new Color(0.18f, 0.18f, 0.20f, 1f));
            GUI.Label(rLabel, label, StLabel);
            p.boolValue = EditorGUI.Toggle(rToggle, p.boolValue);
            return y + filaH + 2f;
        }


        private float CampoFloat(float y, float ancho, string prop, string labelES, string labelEN, string tipES, string tipEN, float min = 0f, float max = 0f, bool slider = false, float labelAncho = -1f)
        {
            SerializedProperty p = SO.FindProperty(prop);
            if (p == null) return y;
            string label = ES ? labelES : labelEN;
            string tip = ES ? tipES : tipEN;

            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, FILA_H);
            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                _tooltipActivo = tip;
                _tooltipMousePos = MouseEnVentana();
            }

            GUIStyle stCampo = new GUIStyle(EditorStyles.numberField)
            { font = ObtenerFuente(), normal = { textColor = C_TEXTO }, fontSize = 10 };

            if (slider && max > min)
            {
                float numW = 48f;
                float sliderW = 100f;
                float sliderX = rFila.xMax - sliderW - 4f - numW;
                Rect rLabel = new Rect(rFila.x + 4f, rFila.y, sliderX - rFila.x - 8f, FILA_H);
                Rect rSlider = new Rect(sliderX, rFila.y + 3f, sliderW, FILA_H - 6f);
                Rect rNum = new Rect(rFila.xMax - numW, rFila.y + 1f, numW, FILA_H - 2f);
                GUI.Label(rLabel, label, StLabel);
                p.floatValue = GUI.HorizontalSlider(rSlider, p.floatValue, min, max);
                p.floatValue = EditorGUI.FloatField(rNum, p.floatValue, stCampo);
            }
            else
            {
                float fieldW = 68f;
                float fieldX = rFila.xMax - fieldW;
                Rect rLabel = new Rect(rFila.x + 4f, rFila.y, fieldX - rFila.x - 8f, FILA_H);
                Rect rField = new Rect(fieldX, rFila.y + 1f, fieldW, FILA_H - 2f);
                GUI.Label(rLabel, label, StLabel);
                p.floatValue = EditorGUI.FloatField(rField, p.floatValue, stCampo);
                if (max > min) p.floatValue = Mathf.Clamp(p.floatValue, min, max);
            }

            return y + FILA_H + 2f;
        }


        private float CampoInt(float y, float ancho, string prop, string labelES, string labelEN, string tipES, string tipEN, int min, int max, float labelAncho = -1f)
        {
            SerializedProperty p = SO.FindProperty(prop);
            if (p == null) return y;
            string label = ES ? labelES : labelEN;
            string tip = ES ? tipES : tipEN;
            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, FILA_H);
            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                _tooltipActivo = tip;
                _tooltipMousePos = MouseEnVentana();
            }
            GUIStyle stCampo = new GUIStyle(EditorStyles.numberField) { font = ObtenerFuente(), normal = { textColor = C_TEXTO }, fontSize = 10 };

            float numW = 48f;
            float sliderW = 100f;
            float sliderX = rFila.xMax - sliderW - 4f - numW;
            Rect rLabel = new Rect(rFila.x + 4f, rFila.y, sliderX - rFila.x - 8f, FILA_H);
            Rect rSlider = new Rect(sliderX, rFila.y + 3f, sliderW, FILA_H - 6f);
            Rect rNum = new Rect(rFila.xMax - numW, rFila.y + 1f, numW, FILA_H - 2f);
            GUI.Label(rLabel, label, StLabel);
            p.intValue = Mathf.RoundToInt(GUI.HorizontalSlider(rSlider, p.intValue, min, max));
            p.intValue = EditorGUI.IntField(rNum, p.intValue, stCampo);
            p.intValue = Mathf.Clamp(p.intValue, min, max);

            return y + FILA_H + 2f;
        }


        private float CampoString(float y, float ancho, string prop, string labelES, string labelEN, string tipES, string tipEN, float labelAncho = -1f)
        {
            SerializedProperty p = SO.FindProperty(prop);
            if (p == null) return y;
            string label = ES ? labelES : labelEN;
            string tip = ES ? tipES : tipEN;

            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, FILA_H);

            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                _tooltipActivo = tip;
                _tooltipMousePos = MouseEnVentana();
            }

            float fieldW = 120f;
            float fieldX = rFila.xMax - fieldW;
            Rect rLabel = new Rect(rFila.x + 4f, rFila.y, fieldX - rFila.x - 8f, FILA_H);

            GUI.Label(rLabel, label, StLabel);
            p.stringValue = EditorGUI.TextField(new Rect(fieldX, rFila.y + 1f, fieldW, FILA_H - 2f), p.stringValue, new GUIStyle(EditorStyles.textField) { font = ObtenerFuente(), fontSize = 10, normal = { textColor = C_TEXTO } });

            return y + FILA_H + 2f;
        }

        private const float KC_H = 24f;
        private const float KC_POPUP_W = 160f;

        private float CampoKeyCode(float y, float ancho, string prop, string labelES, string labelEN, string tipES = "", string tipEN = "")
        {
            SerializedProperty p = SO.FindProperty(prop);
            if (p == null) return y;

            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, KC_H);

            if (!string.IsNullOrEmpty(tipES) && Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.04f));
                _tooltipActivo = ES ? tipES : tipEN;
                _tooltipMousePos = MouseEnVentana();
            }

            float popupX = rFila.xMax - KC_POPUP_W;
            float labelW = popupX - rFila.x - 8f;

            Rect rLabel = new Rect(rFila.x + 4f, rFila.y + 2f, labelW, KC_H);
            Rect rProp = new Rect(popupX, rFila.y + 1f, KC_POPUP_W, KC_H - 2f);

            GUI.Label(rLabel, ES ? labelES : labelEN, StLabel);

            DibujarPXFondo(rProp, new Color(0.10f, 0.18f, 0.30f, 1f), C_ACENTO, 1);

            GUIStyle stPopup = new GUIStyle(EditorStyles.popup)
            {
                font = ObtenerFuente(),
                fontSize = 10,
                normal = { textColor = C_TEXTO },
                hover = { textColor = C_TEXTO },
                focused = { textColor = C_TEXTO },
                active = { textColor = C_TEXTO },
            };
            EditorGUI.BeginChangeCheck();
            KeyCode nuevoValor = (KeyCode)EditorGUI.EnumPopup(rProp, (KeyCode)p.intValue, stPopup);
            if (EditorGUI.EndChangeCheck())
                p.intValue = (int)nuevoValor;

            return y + KC_H + 3f;
        }

        private float CampoLayerMask(float y, float ancho, string prop, string labelES, string labelEN, string tipES, string tipEN)
        {
            SerializedProperty p = SO.FindProperty(prop);
            if (p == null) return y;
            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, KC_H);
            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                if (!string.IsNullOrEmpty(tipES)) { _tooltipActivo = ES ? tipES : tipEN; _tooltipMousePos = MouseEnVentana(); }
            }
            float popupX = rFila.xMax - KC_POPUP_W;
            Rect rLabel = new Rect(rFila.x + 4f, rFila.y + 2f, popupX - rFila.x - 8f, KC_H);
            Rect rProp = new Rect(popupX, rFila.y + 1f, KC_POPUP_W, KC_H - 2f);
            GUI.Label(rLabel, ES ? labelES : labelEN, StLabel);
            DibujarPXFondo(rProp, new Color(0.10f, 0.18f, 0.30f, 1f), C_ACENTO, 1);
            GUIStyle stMask = new GUIStyle(EditorStyles.popup)
            {
                font = ObtenerFuente(),
                fontSize = 10,
                normal = { textColor = C_TEXTO },
                hover = { textColor = C_TEXTO },
                focused = { textColor = C_TEXTO },
                active = { textColor = C_TEXTO },
            };
            EditorGUI.BeginChangeCheck();
            // En: EditorGUI.MaskField works with a CONCATENATED (positional) mask over the compact
            //     InternalEditorUtility.layers array, NOT with the real LayerMask bits. With non-contiguous
            //     layers (e.g. "Ground" at index 6) passing/reading raw bits corrupts the value. We convert
            //     both ways so what you tick is exactly what gets stored.
            // Es: EditorGUI.MaskField trabaja con una máscara CONCATENADA (posicional) sobre el array compacto
            //     InternalEditorUtility.layers, NO con los bits reales del LayerMask. Con capas no contiguas
            //     (p. ej. "Ground" en el índice 6) pasar/leer bits crudos corrompe el valor. Convertimos en
            //     ambos sentidos para que lo que marcas sea exactamente lo que se guarda.
            int mascaraConcatenada = UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(p.intValue);
            int nuevaConcatenada = EditorGUI.MaskField(rProp, GUIContent.none, mascaraConcatenada, UnityEditorInternal.InternalEditorUtility.layers, stMask);
            if (EditorGUI.EndChangeCheck())
            {
                LayerMask mascaraReal = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(nuevaConcatenada);
                p.intValue = mascaraReal.value;
            }
            return y + KC_H + 3f;
        }

        private float CampoBoolFloat(float y, float ancho, string propBool, string propFloat, string labelES, string labelEN, string tipES, string tipEN, float min = 0f, float max = 100f)
        {
            SerializedProperty pb = SO.FindProperty(propBool);
            SerializedProperty pf = SO.FindProperty(propFloat);
            if (pb == null || pf == null) return y;

            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, FILA_H);
            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                _tooltipActivo = ES ? tipES : tipEN;
                _tooltipMousePos = MouseEnVentana();
            }

            DibujarCirculo(new Rect(rFila.x + 2f, rFila.y + 5f, 10f, 10f), pb.boolValue ? C_VERDE_LED : new Color(0.18f, 0.18f, 0.20f, 1f));

            pb.boolValue = EditorGUI.Toggle(new Rect(rFila.x + 14f, rFila.y + 1f, 18f, FILA_H - 2f), pb.boolValue);
            GUI.Label(new Rect(rFila.x + 34f, rFila.y, LABEL_W - 34f, FILA_H), ES ? labelES : labelEN, StLabel);

            GUI.enabled = pb.boolValue;
            pf.floatValue = EditorGUI.FloatField(new Rect(rFila.xMax - 70f, rFila.y + 1f, 68f, FILA_H - 2f), pf.floatValue, new GUIStyle(EditorStyles.numberField) { font = ObtenerFuente(), fontSize = 10, normal = { textColor = C_TEXTO } });
            if (max > min) pf.floatValue = Mathf.Clamp(pf.floatValue, min, max);
            GUI.enabled = true;

            return y + FILA_H + 2f;
        }

        private float CampoBoolFF(float y, float ancho, string pb_, string pf1_, string pf2_, string labelES, string labelEN, string tipES, string tipEN, string lf1ES, string lf1EN, string lf2ES, string lf2EN, float min1, float max1, float min2, float max2)
        {
            SerializedProperty pb = SO.FindProperty(pb_);
            SerializedProperty pf1 = SO.FindProperty(pf1_);
            SerializedProperty pf2 = SO.FindProperty(pf2_);
            if (pb == null || pf1 == null || pf2 == null) return y;
            Rect rFila = new Rect(PAD * 2f, y, ancho - PAD * 4f, FILA_H);
            if (Event.current.type == EventType.Repaint && rFila.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rFila, new Color(1f, 1f, 1f, 0.03f));
                _tooltipActivo = ES ? tipES : tipEN;
                _tooltipMousePos = MouseEnVentana();
            }
            DibujarCirculo(new Rect(rFila.x + 2f, rFila.y + 5f, 10f, 10f), pb.boolValue ? C_VERDE_LED : new Color(0.18f, 0.18f, 0.20f, 1f));
            pb.boolValue = EditorGUI.Toggle(new Rect(rFila.x + 14f, rFila.y + 1f, 18f, FILA_H - 2f), pb.boolValue);

            float fw = 60f;
            float xCursor = rFila.xMax - (fw * 2f) - (36f * 2f) - 4f;
            float labelW = xCursor - (rFila.x + 34f) - 8f;
            GUI.Label(new Rect(rFila.x + 34f, rFila.y - 3f, labelW, FILA_H), ES ? labelES : labelEN, StLabel);

            GUI.enabled = pb.boolValue;
            GUIStyle stFF = new GUIStyle(EditorStyles.numberField) { font = ObtenerFuente(), fontSize = 10, normal = { textColor = C_TEXTO } };
            GUI.Label(new Rect(xCursor, rFila.y, 34f, FILA_H), ES ? lf1ES : lf1EN, StLabelSub);
            xCursor += 36f;
            pf1.floatValue = Mathf.Clamp(EditorGUI.FloatField(new Rect(xCursor, rFila.y + 1f, fw, FILA_H - 2f), pf1.floatValue, stFF), min1, max1);
            xCursor += fw + 4f;
            GUI.Label(new Rect(xCursor, rFila.y, 34f, FILA_H), ES ? lf2ES : lf2EN, StLabelSub);
            xCursor += 36f;
            pf2.floatValue = Mathf.Clamp(EditorGUI.FloatField(new Rect(xCursor, rFila.y + 1f, fw, FILA_H - 2f), pf2.floatValue, stFF), min2, max2);
            GUI.enabled = true;
            return y + FILA_H + 2f;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region SECCIONES S01–S12  /  SECTIONS S01–S12
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private float DibujarS01(float y, float ancho)
        {
            y = SubHeader(y, ancho, ES ? "— Permisos" : "— Permissions");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToWalk",
                "Permitir Que El Jugador Camine", "Allow The Player To Walk",
                "Explicación Base:\nDefine si el jugador puede caminar.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, se ignora completamente el input de movimiento horizontal, sin importar qué input se presione.\n\n" +
                "Ejemplo de Uso:\nPuedes desactivarla durante cinemáticas, secuencias de muerte, menús de pausa o cualquier momento en que el jugador no deba moverse.\n\n" +
                "Ejemplo General:\nEn prácticamente todos los juegos de acción en primera persona como Half-Life 2 o DOOM Eternal, esta variable siempre está activa durante el gameplay.",
                "Base Explanation:\nDefines whether the player can walk.\n\n" +
                "Technical Explanation:\nBool variable. If False, it completely ignores horizontal movement input, regardless of which input is pressed.\n\n" +
                "Usage Example:\nYou can disable it during cutscenes, death sequences, pause menus, or any moment the player should not move.\n\n" +
                "General Example:\nIn virtually every first-person action game like Half-Life 2 or DOOM Eternal, this variable is always active during gameplay.");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToRun",
                "Permitir Que El Jugador Corra", "Allow The Player To Run",
                "Explicación Base:\nDefine si el jugador puede correr.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el input de correr no tiene ningún efecto aunque el input esté bien asignado y pulsado. La velocidad máxima del jugador queda limitada a la velocidad base.\n\n" +
                "Ejemplo de Uso:\nPuedes desactivarla en zonas donde el diseño narrativo pide calma, como en una habitación de hospital en un juego de terror, o cuando el jugador está herido y su movilidad se ve reducida.\n\n" +
                "Ejemplo General:\nEn Call of Duty siempre se puede correr.",
                "Base Explanation:\nDefines whether the player can run.\n\n" +
                "Technical Explanation:\nBool variable. If False, the sprint input has no effect even if the input is assigned and held. Maximum player speed is capped at base speed.\n\n" +
                "Usage Example:\nDisable it in areas where narrative design requires calm, like a hospital room in a horror game, or when the player is injured and mobility is reduced.\n\n" +
                "General Example:\nIn Call of Duty, sprinting is always available.");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToRunWhileCrouching",
                "Permitir Que El Jugador Corra Agachado", "Allow The Player To Run While Crouching",
                "Explicación Base:\nDefine si el jugador puede activar el multiplicador de correr mientras está en estado agachado.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el sistema bloquea la entrada de correr cuando el estado corporal activo es agachado.\n\n" +
                "Ejemplo de Uso:\nDesactivar esto obliga al jugador a salir del agachado para correr a mayor velocidad.\n\n" +
                "Ejemplo General:\nEn la mayoría de shooters militares como Arma III el sprint en cuclillas no existe o está muy limitado para reforzar el peso postural, en shooters más tipo arcade el movimiento agachado suele ser más permisivo.",
                "Base Explanation:\nDefines whether the player can activate the sprint multiplier while been in the crouching state.\n\n" +
                "Technical Explanation:\nBool variable. If False, the system blocks the sprint input when the active body state is Crouching.\n\n" +
                "Usage Example:\nDisabling it forces the player to exit the crouch before running at major speed.\n\n" +
                "General Example:\nIn most military shooters like Arma III, sprinting while crouched does not exist or is heavily restricted to reinforce postural realism, in more arcade shooters, crouched movement tends to be more permissive.");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToRunWhileProne",
                "Permitir Que El Jugador Corra Acostado", "Allow The Player To Run While Prone",
                "Explicación Base:\nDefine si el jugador puede activar el multiplicador de correr mientras está en estado de acostado.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el sistema bloquea la entrada de correr cuando el estado corporal activo es acostado. Generalmente estar acostado implica tener la velocidad más baja del sistema, por lo que permitir correr en ese estado rompería la jerarquía de velocidades.\n\n" +
                "Ejemplo de Uso:\nEn casi todos los contextos debería estar en False. Activarla solo tendría sentido en juegos donde el acostado es una postura dinámica de combate y no una postura de sigilo o cobertura estática.\n\n" +
                "Ejemplo General:\nEn Call of Duty, acostarse reduce drásticamente la velocidad de movimiento y no permite correr estando acostado.",
                "Base Explanation:\nDefines whether the player can activate the sprint multiplier while in the prone state.\n\n" +
                "Technical Explanation:\nBool variable. If False, the system blocks the sprint input when the active body state is Prone. Prone generally implies the lowest speed in the system, so allowing sprinting in that state would break the speed hierarchy.\n\n" +
                "Usage Example:\nIn almost all contexts this should be False. Enabling it would only make sense in games where prone is a dynamic combat posture rather than a stealth or static cover position.\n\n" +
                "General Example:\nIn Call of Duty, the prone position drastically reduces movement speed and does not allow sprinting.");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToMoveInTheAir",
                "Permitir Que El Jugador Se Mueva En El Aire", "Allow The Player To Move In The Air",
                "Explicación Base:\nDefine si el jugador puede cambiar de dirección mientras está en el aire.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, la dirección horizontal se congela en el momento del salto. El jugador sigue con la trayectoria inicial sin poder corregirla hasta volver al suelo.\n\n" +
                "Ejemplo de Uso:\nDesactivarla da saltos más pesados y comprometedores, donde cada salto requiere planificación previa. Activarla da la sensación de control total en el aire.\n\n" +
                "Ejemplo General:\nEn DOOM Eternal el control aéreo está muy muy activo. En algunos juegos de plataformas clásicos el control aéreo es limitado o nulo para añadir dificultad.",
                "Base Explanation:\nDefines whether the player can change direction while airborne.\n\n" +
                "Technical Explanation:\nBool variable. If False, horizontal direction is frozen at the moment of the jump. The player continues with the initial trajectory without being able to correct it until back on the ground.\n\n" +
                "Usage Example:\nDisabling it gives heavier, more committed jumps where each jump requires prior planning. Enabling it gives the feeling of full control in the air.\n\n" +
                "General Example:\nIn DOOM Eternal, air control is very active. In some classic platformers, air control is limited or absent to add difficulty.");

            y = SubHeader(y, ancho, ES ? "— Velocidad" : "— Speed");
            y = CampoFloat(y, ancho, "BaseSpeedOfThePlayer",
                "Velocidad Base Del Jugador u/s", "Player Base Speed u/s",
                "Explicación Base:\nVelocidad de movimiento base del jugador expresada en unidades Unity por segundo.\n\n" +
                "Explicación Técnica:\nVariable de tipo Float. Es el valor raíz sobre el que operan todos los multiplicadores del sistema, incluyendo los de omnidireccionalidad, los de estados corporales y el de correr. Cambiar este valor afecta indirectamente a toda la cadena de velocidades.\n\n" +
                "Ejemplo de Uso:\nUn valor de 5 u/s es un punto de partida común para un FPS estándar. Valores entre 3 y 6 generan sensación de peso y realismo. Valores superiores a 8 empiezan a sentirse arcade o fantásticos.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 el jugador camina a aproximadamente 5 u/s. En Quake III Arena los valores de movimiento son extremadamente altos para favorecer el juego rápido.",
                "Base Explanation:\nPlayer's base movement speed expressed in Unity units per second.\n\n" +
                "Technical Explanation:\nFloat variable. It is the root value on which all system multipliers operate, including omnidirectionality, body state multipliers and the sprint multiplier. Changing this value indirectly affects the entire speed chain.\n\n" +
                "Usage Example:\nA value of 5 u/s is a common starting point for a standard FPS. Values between 3 and 6 generate a sense of weight and realism. Values above 8 start to feel arcade-like or fantastical.\n\n" +
                "General Example:\nIn Half-Life 2 the player walks at approximately 5 u/s. In Quake III Arena movement values are extremely high to favor fast-paced gameplay.", 0.1f, 48f);
            y = CampoFloat(y, ancho, "SpeedMultiplierWhileRunning",
                "Multiplicador De la Velocidad Base Al Correr", "Base Speed Multiplier When Running",
                "Explicación Base:\nMultiplica la velocidad base cuando el jugador corre.\n\n" +
                "Explicación Técnica:\nVariable de tipo Float. La velocidad del sprint se calcula como 'Velocidad Base Del Jugador u/s' × 'Multiplicador De Velocidad Al Correr'. Un valor de 1.0 significa que correr y caminar tienen la misma velocidad. Un valor de 2.0 significa que el jugador va el doble de rápido al correr que al caminar.\n\n" +
                "Ejemplo de Uso:\nCon Velocidad Base Del Jugador = 5 y Multiplicador De Velocidad Al Correr = 1.8, el jugador corre a 9 u/s. Este ratio 1:1.8 es habitual en shooters donde el sprint existe pero no es dramáticamente más rápido.\n\n" +
                "Ejemplo General:\nEn Call of Duty: Modern Warfare el ratio caminar/sprint es aproximadamente 1:1.6. En DOOM Eternal el movimiento es tan rápido que la diferencia entre caminar y correr es menos relevante que en otros juegos y por ende no hay un 'correr'.",
                "Base Explanation:\nMultiplies the base speed when the player runs.\n\n" +
                "Technical Explanation:\nFloat variable. Sprint speed is calculated as 'Player Base Speed u/s' × 'Base Speed Multiplier When Running'. A value of 1.0 means running and walking are the same speed. A value of 2.0 means the player runs twice as fast as they walk.\n\n" +
                "Usage Example:\nWith Player Base Speed u/s = 5 and Multiplier = 1.8, the player runs at 9 u/s. This 1:1.8 ratio is common in shooters where sprinting exists but is not dramatically faster.\n\n" +
                "General Example:\nIn Call of Duty: Modern Warfare the walk/sprint ratio is approximately 1:1.6. In DOOM Eternal movement is so fast that the difference between walking and running is non existent.", 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Modo de Correr" : "— Run Mode");
            y = CampoBool(y, ancho + 20, "HoldToRun",
                "Mantener El Input Presionado Para Correr", "Hold Input To Run",
                "Explicación Base:\nDefine si el jugador debe mantener pulsada el input de correr o si funciona como una palanca.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. En modo True, el jugador corre solo mientras se mantenga pulsado el input. En modo False, pulsar el input activa el sprint y volver a pulsarlo lo desactiva, sin necesidad de mantenerlo.\n\n" +
                "Ejemplo de Uso:\nEl modo palanca (False) es más accesible y reduce la fatiga en partidas largas, especialmente útil en juegos de exploración. El modo mantener (True) da más control táctico sobre cuándo exactamente se corre.\n\n" +
                "Ejemplo General:\nLa mayoría de shooters como Battlefield o Call of Duty usan el modo mantener (True) porque el sprint es una decisión táctica activa. Juegos como Minecraft ofrecen ambas opciones en su configuración de accesibilidad.",
                "Base Explanation:\nDefines whether the player must hold the run input or if it works as a toggle.\n\n" +
                "Technical Explanation:\nBool variable. In True mode, the player only runs while holding the input. In False mode, a first press activates sprint and a second press deactivates it, without needing to hold.\n\n" +
                "Usage Example:\nToggle mode (False) is more accessible and reduces fatigue in long sessions, especially useful in exploration games. Hold mode (True) gives more tactical control over exactly when to sprint.\n\n" +
                "General Example:\nMost shooters like Battlefield or Call of Duty use hold mode (True) because sprinting is an active tactical decision. Games like Minecraft offer both options in their accessibility settings.");
            return y;
        }

        private float DibujarS02(float y, float ancho)
        {
            y = CampoBool(y, ancho + 20, "EnableOmniDirectionalMovement",
                "Activar La Omnidireccionalidad En El Movimiento", "Enable Omnidirectionality In Movement",
                "Explicación Base:\nPermite que el jugador tenga velocidades diferentes según la dirección en la que se mueve y el estado corporal en el que se encuentra.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, todos los multiplicadores omnidireccionales son ignorados y la velocidad es uniforme en todas las direcciones y posturas. Si está en True, cada combinación de dirección y estado tiene su propio multiplicador.\n\n" +
                "Ejemplo de Uso:\nActívala si quieres que moverse hacia atrás sea más lento que hacia adelante, o que estando agachado te muevas más despacio que estando de pie.\n\n" +
                "Ejemplo General:\nEn Counter-Strike, moverse hacia atrás es notablemente más lento que hacia adelante, lo que penaliza el uso excesivo de caminar de espaldas. En Titanfall 2 todas las direcciones tienen velocidades similares para favorecer el movimiento fluido.",
                "Base Explanation:\nAllows the player to have different speeds depending on the direction they move and the body state they are in.\n\n" +
                "Technical Explanation:\nBool variable. If False, all omnidirectional multipliers are ignored and speed is uniform in all directions and stances. If True, each direction and state combination has its own multiplier.\n\n" +
                "Usage Example:\nEnable it if you want moving backward to be slower than forward, or crouching to be slower than standing.\n\n" +
                "General Example:\nIn Counter-Strike, moving backward is noticeably slower than forward, which penalizes excessive backpedaling. In Titanfall 2 all directions have similar speeds to favor fluid movement.");

            y = SubHeader(y, ancho, ES ? "— De Pie" : "— Standing");
            y = CampoFloat(y, ancho, "ForwardOmnidirectionalMultiplierWhileStanding",
                "Multiplicador Omnidireccional Hacia Adelante - Estando De Pie", "Omnidirectional Multiplier Forward While Standing",
                "Explicación Base:\nMultiplicador de velocidad al moverse hacia adelante estando de pie.\n\n" +
                "Explicación Técnica:\nFloat que se multiplica por 'Velocidad Base Del Jugador u/s' cuando el jugador está de pie y se mueve hacia adelante. Un valor de 1.0 es neutro. Valores mayores aceleran, menores ralentizan.\n\n" +
                "Ejemplo de Uso:\nDejar este valor en 1.0 es lo más común. Es la dirección principal de movimiento y suele ser la velocidad de referencia del juego.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS como Halo o Titanfall 2, avanzar de pie es la velocidad base de referencia (multiplicador 1.0).",
                "Base Explanation:\nSpeed multiplier when moving forward while standing.\n\n" +
                "Technical Explanation:\nFloat multiplied by 'Player Base Speed u/s' when the player is standing and moving forward. A value of 1.0 is neutral. Higher values accelerate, lower values slow down.\n\n" +
                "Usage Example:\nLeaving this at 1.0 is most common. It is the primary movement direction and typically serves as the game's reference speed.\n\n" +
                "General Example:\nIn most FPS games like Halo or Titanfall 2, moving forward while standing is the base reference speed (multiplier 1.0).", 0f, 48f);
            y = CampoFloat(y, ancho, "BackwardOmnidirectionalMultiplierWhileStanding",
                "Multiplicador Omnidireccional Hacia Atras - Estando De Pie", "Omnidirectional Multiplier Backward While Standing",
                "Explicación Base:\nMultiplicador de velocidad al moverse hacia atrás estando de pie.\n\n" +
                "Explicación Técnica:\nFloat que se multiplica por 'Velocidad Base Del Jugador u/s' cuando el jugador está de pie y retrocede. Reducirlo simula la dificultad de moverse hacia atrás sin ver hacia donde vas.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.7 es muy común para dar sensación de peso sin hacer el retroceso frustrante.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2, retroceder es considerablemente más lento que avanzar. Este valor ronda en la mitad del total de velocidad hacia adelante, lo que penaliza activamente el backpedaling en combate.",
                "Base Explanation:\nSpeed multiplier when moving backward while standing.\n\n" +
                "Technical Explanation:\nFloat multiplied by 'Player Base Speed u/s' when the player is standing and moving backward. Reducing it simulates the difficulty of moving without seeing where you are going.\n\n" +
                "Usage Example:\nA value of 0.7 is very common to give a sense of weight without making backpedaling frustrating.\n\n" +
                "General Example:\nIn Counter-Strike 2, moving backward is considerably slower than forward. This value is around half the forward speed forward speed, actively penalizing backpedaling in combat.", 0f, 48f);
            y = CampoFloat(y, ancho, "LateralOmnidirectionalMultiplierWhileStanding",
                "Multiplicador Omnidireccional Lateralmente - Estando De Pie", "Omnidirectional Multiplier Laterally While Standing",
                "Explicación Base:\nMultiplicador de velocidad al moverse lateralmente estando de pie.\n\n" +
                "Explicación Técnica:\nFloat que se multiplica por 'Velocidad Base Del Jugador u/s' al moverse hacia la izquierda o la derecha de pie. El movimiento lateral es clave en combate para esquivar proyectiles.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.85 es equilibrado: el movimiento lateral es ligeramente más lento que avanzar pero lo suficientemente rápido para ser efectivo en combate.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2 el movimiento lateral equivale aproximadamente a 0.9 de la velocidad frontal. En Quake el salto lateral permite incluso superar la velocidad frontal.",
                "Base Explanation:\nSpeed multiplier when moving laterally (strafing) while standing.\n\n" +
                "Technical Explanation:\nFloat multiplied by 'Player Base Speed u/s' when moving left or right while standing. Lateral strafing is key in combat for dodging projectiles.\n\n" +
                "Usage Example:\nA value of 0.85 is well-balanced: strafing is slightly slower than moving forward but fast enough to be effective in combat.\n\n" +
                "General Example:\nIn Counter-Strike 2 lateral strafing is approximately 0.9 of the forward speed. In Quake, strafe-jumping can even exceed forward speed through advanced techniques.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Agachado" : "— Crouching");
            y = CampoFloat(y, ancho, "ForwardOmnidirectionalMultiplierWhileCrouching",
                "Multiplicador Omnidireccional Hacia Adelante - Estando Agachado", "Omnidirectional Multiplier Forward While Crouching",
                "Explicación Base:\nMultiplicador de velocidad al avanzar en estado agachado.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' cuando el jugador está en la postura de agachado y se mueve hacia adelante. Debe ser menor que el multiplicador de pie para que agacharse tenga un coste de movilidad.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.5 hace que moverse agachado sea la mitad de rápido que estando de pie. Esto refuerza que estar agachado es una postura de sigilo y no una forma de moverse a la misma velocidad pero con menor hitbox.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2 agacharse reduce la velocidad a aproximadamente 0.34 de la velocidad base que se usa de pie, haciendo el caminar agachado muy lento pero preciso para el control del retroceso.",
                "Base Explanation:\nSpeed multiplier when moving forward while crouching.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when the player is crouching and moving forward. It should be lower than the standing multiplier so that crouching has a mobility cost.\n\n" +
                "Usage Example:\nA value of 0.5 makes crouched movement half as fast as standing. This reinforces that crouching is a stealth stance and not a way to move at the same speed with a smaller hitbox.\n\n" +
                "General Example:\nIn Counter-Strike 2, crouching reduces speed to approximately 0.34 of the standing base speed, making crouch-walking very slow but precise for recoil control.", 0f, 48f);
            y = CampoFloat(y, ancho, "BackwardOmnidirectionalMultiplierWhileCrouching",
                "Multiplicador Omnidireccional Hacia Atras - Estando Agachado", "Omnidirectional Multiplier Backward While Crouching",
                "Explicación Base:\nMultiplicador de velocidad al retroceder en estado agachado.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al retroceder estando agachado. Generalmente será igual o menor que el multiplicador frontal estando agachado.\n\n" +
                "Ejemplo de Uso:\nSi el movimiento frontal agachado es 0.5, poner este en 0.4 hace que el retroceder estando agachado sea ligeramente más penalizado, empujando al jugador a reposicionarse girando en lugar de simplemente caminar de espaldas.\n\n" +
                "Ejemplo General:\nEn la mayoría de shooters tácticos, el retroceder estando agachado no tiene una penalización adicional respecto al avance estando agachado, ambos son igual de lentos.",
                "Base Explanation:\nSpeed multiplier when moving backward while crouching.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when moving backward while crouched. It will generally be equal to or lower than the forward crouching multiplier.\n\n" +
                "Usage Example:\nIf the forward crouching multiplier is 0.5, setting this to 0.4 makes crouched backpedaling slightly more penalized, pushing the player to reposition by turning rather than backpedaling.\n\n" +
                "General Example:\nIn most tactical shooters, crouched backward movement has no additional penalty compared to crouched forward movement — both are equally slow.", 0f, 48f);
            y = CampoFloat(y, ancho, "LateralOmnidirectionalMultiplierWhileCrouching",
                "Multiplicador Omnidireccional Lateralmente - Estando Agachado", "Omnidirectional Multiplier Laterally While Crouching",
                "Explicación Base:\nMultiplicador de velocidad al moverse lateralmente en estado agachado.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al moverse lateralmente estando agachado. Combinado con los otros multiplicadores del estado agachado, define completamente la velocidad de la postura.\n\n" +
                "Ejemplo de Uso:\nMantenerlo igual al movimiento frontal agachado da una uniformidad de movimiento. Reducirlo incentiva aún más a que el jugador salga de estar agachado para maniobrar rápidamente.\n\n" +
                "Ejemplo General:\nEn Valorant el movimiento lateral estando agachado es igual de lento que el movimiento frontal agachado, reforzando el que estar agachado es una herramienta táctica puntual y no de movilidad.",
                "Base Explanation:\nSpeed multiplier when moving laterally while crouching.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when lateral strafing in crouch. Combined with the other crouching multipliers, it fully defines the speed of the crouch stance.\n\n" +
                "Usage Example:\nKeeping it equal to the forward crouching multiplier gives uniform movement. Reducing it further incentivizes the player to exit crouch in order to maneuver quickly.\n\n" +
                "General Example:\nIn Valorant, lateral crouched movement is just as slow as forward crouched movement, reinforcing crouch as a precise tactical tool rather than a mobility option.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Acostado" : "— Prone");
            y = CampoFloat(y, ancho, "ForwardOmnidirectionalMultiplierWhileProne",
                "Multiplicador Omnidireccional Hacia Adelante - Estando Acostado", "Omnidirectional Multiplier Forward While Prone",
                "Explicación Base:\nMultiplicador de velocidad al arrastrarse hacia adelante en el estado acostado.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al moverse hacia adelante estando acostado. Debe ser significativamente menor que el de agachado para que el estado acostado sea principalmente defensivo.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.3 o menor hace que acostarse sea una postura prácticamente estática. Valores entre 0.3 y 0.5 permiten un arrastre lento pero funcional para reposicionarse.\n\n" +
                "Ejemplo General:\nEn Arma 3 el movimiento estando acostado es muy lento, reservado principalmente para disparar con precisión desde el suelo.",
                "Base Explanation:\nSpeed multiplier when crawling forward while prone.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when moving forward while prone. It should be significantly lower than the crouching multiplier so that the prone state is primarily defensive.\n\n" +
                "Usage Example:\nA value of 0.3 or lower makes going prone a nearly static stance. Values between 0.3 and 0.5 allow a slow but functional crawl for repositioning.\n\n" +
                "General Example:\nIn Arma 3, prone movement is very slow, reserved mainly for shooting with precision from the ground.", 0f, 48f);
            y = CampoFloat(y, ancho, "BackwardOmnidirectionalMultiplierWhileProne",
                "Multiplicador Omnidireccional Hacia Atras - Estando Acostado", "Omnidirectional Multiplier Backward While Prone",
                "Explicación Base:\nMultiplicador de velocidad al arrastrarse hacia atrás estando acostado.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al retroceder estando acostado. Arrastrarse hacia atrás es biomecánicamente más difícil, por lo que este valor suele ser el más bajo de todos los multiplicadores.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.2 o incluso 0.1 hace que retroceder estando acostado sea casi imposible, forzando al jugador a incorporarse si necesita retirarse con rapidez.\n\n" +
                "Ejemplo General:\nEn casi todos los COD, el movimiento estando acostado hacia atrás es extremadamente lento.",
                "Base Explanation:\nSpeed multiplier when crawling backward while prone.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when moving backward while prone. Crawling backward is biomechanically harder, so this value tends to be the lowest of all multipliers.\n\n" +
                "Usage Example:\nA value of 0.2 or even 0.1 makes backward prone movement nearly impossible, forcing the player to stand up if they need to retreat quickly.\n\n" +
                "General Example:\nIn almost every COD, backward prone movement is extremely slow, making the stance a committed position once taken under fire.", 0f, 48f);
            y = CampoFloat(y, ancho, "LateralOmnidirectionalMultiplierWhileProne",
                "Multiplicador Omnidireccional Lateralmente - Estando Acostado", "Omnidirectional Multiplier Laterally While Prone",
                "Explicación Base:\nMultiplicador de velocidad al arrastrarse lateralmente en estando acostado.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al moverse lateralmente estando acostado. Moverse lateralmente acostado es anatómicamente poco natural, por lo que suele tener el valor más restrictivo junto a el retroceder estando acostado.\n\n" +
                "Ejemplo de Uso:\nValores muy bajos (0.1–0.2) convierten el moverse lateralmente estando acostado en algo casi estática. Valores moderados (0.3–0.4) permiten microajustes de posición sin abandonar la postura.\n\n" +
                "Ejemplo General:\nEn Arma 3 el movimiento lateral estando acostado es sumamente lento.",
                "Base Explanation:\nSpeed multiplier when crawling laterally while prone.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when strafing while prone. Moving sideways while prone is anatomically unnatural, so it typically has the most restrictive value alongside backward movement.\n\n" +
                "Usage Example:\nVery low values (0.1–0.2) turn prone into a nearly static stance. Moderate values (0.3–0.4) allow micro-adjustments without leaving the stance.\n\n" +
                "General Example:\nIn Arma 3, lateral prone movement is so f slow.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— En el Aire" : "— In the Air");
            y = CampoFloat(y, ancho, "ForwardOmnidirectionalMultiplierWhileInTheAir",
                "Multiplicador Omnidireccional Hacia Adelante - Estando En El Aire", "Omnidirectional Multiplier Forward While Airborne",
                "Explicación Base:\nMultiplicador de velocidad al moverse hacia adelante mientras el jugador está en el aire.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al avanzar mientras el jugador está en el aire. Solo tiene efecto si 'Permitir Que El Jugador Se Mueva En El Aire' está en True.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.8 da buen control aéreo frontal sin que los saltos hacia adelante sean más rápidos que correr por el suelo.\n\n" +
                "Ejemplo General:\nEn DOOM Eternal el control aéreo frontal es muy alto, permitiendo mantener casi toda la velocidad de sprint en el aire, lo que es fundamental para el estilo de combate dinámico del juego.",
                "Base Explanation:\nSpeed multiplier when moving forward while the player is airborne.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when moving forward while airborne. Only takes effect if 'Allow The Player To Move In The Air' is set to True.\n\n" +
                "Usage Example:\nA value of 0.8 gives good forward air control without making forward jumps faster than running on the ground.\n\n" +
                "General Example:\nIn DOOM Eternal, forward air control is very high, allowing the player to maintain nearly full sprint speed in the air, which is fundamental to the game's dynamic combat style.", 0f, 48f);
            y = CampoFloat(y, ancho, "BackwardOmnidirectionalMultiplierWhileInTheAir",
                "Multiplicador Omnidireccional Hacia Atras - Estando En El Aire", "Omnidirectional Multiplier Backward While Airborne",
                "Explicación Base:\nMultiplicador de velocidad al retroceder mientras el jugador está en el aire.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al retroceder mientras el jugador está en el aire. Controlar la dirección hacia atrás en el aire suele tener menos respuesta por diseño.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.6 permite correcciones hacia atrás en el aire sin que sean tan ágiles como avanzar, dando sensación de inercia natural.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS el control aéreo hacia atrás es más limitado que hacia adelante, reforzando que los saltos deben planificarse en la dirección correcta.",
                "Base Explanation:\nSpeed multiplier when moving backward while the player is airborne.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when moving backward while airborne. Backward air control is intentionally less responsive by design.\n\n" +
                "Usage Example:\nA value of 0.6 allows backward corrections in the air without being as agile as moving forward, giving a natural sense of inertia.\n\n" +
                "General Example:\nIn most FPS games, backward air control is more limited than forward, reinforcing that jumps should be planned in the correct direction.", 0f, 48f);
            y = CampoFloat(y, ancho, "LateralOmnidirectionalMultiplierWhileInTheAir",
                "Multiplicador Omnidireccional Lateralmente - Estando En El Aire", "Omnidirectional Multiplier Laterally While Airborne",
                "Explicación Base:\nMultiplicador de velocidad al moverse lateralmente mientras el jugador está en el aire.\n\n" +
                "Explicación Técnica:\nFloat aplicado sobre 'Velocidad Base Del Jugador u/s' al moverse lateralmente mientras el jugador está en el aire. El movimiento lateral aéreo es clave en juegos competitivos para esquivar y en plataformers para ajustar aterrizajes.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.75 da buen control lateral en el aire manteniendo cierta inercia de salto. Combinado con el movimiento frontal aéreo define completamente la maniobrabilidad mientras el jugador está en el aire.\n\n" +
                "Ejemplo General:\nEn Apex Legends el movimiento lateral aéreo es una habilidad avanzada muy usada para esquivar disparos durante saltos. En juegos más realistas como Escape from Tarkov el control aéreo lateral es mínimo.",
                "Base Explanation:\nSpeed multiplier when moving laterally while the player is airborne.\n\n" +
                "Technical Explanation:\nFloat applied to 'Player Base Speed u/s' when lateral strafing while airborne. Aerial strafing is key in competitive games for dodging and in platformers for adjusting landings.\n\n" +
                "Usage Example:\nA value of 0.75 gives good lateral air control while maintaining some jump inertia. Combined with the forward air multiplier, it fully defines maneuverability while the player is in the air.\n\n" +
                "General Example:\nIn Apex Legends, aerial strafing is an advanced skill widely used to dodge shots during jumps. In more realistic games like Escape from Tarkov, lateral air control is minimal.", 0f, 48f);
            return y;
        }

        private float DibujarS03(float y, float ancho)
        {
            y = SubHeader(y, ancho, ES ? "— Permisos" : "— Permissions");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToCrouch",
                "Permitir Que El Jugador Se Agache", "Allow The Player To Crouch",
                "Explicación Base:\nDefine si el jugador puede agacharse.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el input asignado a agacharse no tiene ningún efecto. El CapsuleCollider mantiene siempre la altura de pie y el sistema de transición de posturas no se activa.\n\n" +
                "Ejemplo de Uso:\nPuedes desactivarla durante secciones de juego donde el diseño no permite sigilo o cobertura baja, o en cinemáticas donde el jugador no debe cambiar de postura.\n\n" +
                "Ejemplo General:\nEn casi todos los FPS agacharse está disponible siempre. En juegos como Superhot VR agacharse es casi siempre vital.",
                "Base Explanation:\nDefines whether the player can crouch.\n\n" +
                "Technical Explanation:\nBool variable. If False, the crouch key has no effect. The CapsuleCollider always maintains standing height and the stance transition system is not activated.\n\n" +
                "Usage Example:\nYou can disable it during game sections where the design does not allow stealth or low cover, or in cutscenes where the player should not change stance.\n\n" +
                "General Example:\nIn almost all FPS games, crouching is always available. In games like Superhot VR been able to crouch is vital.");
            y = CampoBool(y, ancho + 20, "HoldToCrouch",
                "Mantener El Input Presionado Para Agacharse", "Hold Input To Crouch",
                "Explicación Base:\nDefine si el jugador debe mantener pulsada el input de agacharse o si funciona como una palanca.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. En modo True, el jugador está agachado siempre y cuando mantenga pulsado el input asignado. En modo False, presionar el input activa el agacharse y volver a presionar el input lo desactiva.\n\n" +
                "Ejemplo de Uso:\nEl modo palanca (False) es ideal para la exploración o el sigilo prolongado donde mantener la input pulsado durante minutos sería incómodo. El modo mantener (True) es más intuitivo para su uso puntual en combate.\n\n" +
                "Ejemplo General:\nCounter-Strike 2 usa mantener (True) para el agachado porque su uso es táctico y breve. Muchos juegos de sigilo como Splinter Cell ofrecen ambas opciones en su configuración.",
                "Base Explanation:\nDefines whether the player must hold the crouch input or if it works as a toggle.\n\n" +
                "Technical Explanation:\nBool variable. In True mode, the player is crouched only while holding the input. In False mode, a first press activates crouch and a second press deactivates it.\n\n" +
                "Usage Example:\nToggle mode (False) is ideal for prolonged exploration or stealth where holding the input for minutes would be uncomfortable. Hold mode (True) is more intuitive for tactical use in combat.\n\n" +
                "General Example:\nCounter-Strike 2 uses hold (True) for crouch because its use is tactical and brief. Many stealth games like Splinter Cell offer both options in their settings.");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToGoProne",
                "Permitir Que El Jugador Se Acueste", "Allow The Player To Go Prone",
                "Explicación Base:\nDefine si el jugador puede acostarse.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el estado acostado está completamente bloqueado. El CapsuleCollider nunca alcanza la altura mínima del acostado y el sistema de transición ignora esa postura.\n\n" +
                "Ejemplo de Uso:\nDesactivar el acostarse simplifica el sistema de posturas a dos estados (pie/agachado). Es una buena opción para juegos arcade o de ritmo rápido donde acostarse rompería el ritmo o simplemente no tendría utilidad.\n\n" +
                "Ejemplo General:\nJuegos tácticos como Arma 3 o Squad tienen el acostarse como una mecánica fundamental. Juegos de acción rápida como Titanfall 2 no tienen el estado acostado para mantener el ritmo frenético.",
                "Base Explanation:\nDefines whether the player can go prone.\n\n" +
                "Technical Explanation:\nBool variable. If False, the prone state is completely blocked. The CapsuleCollider never reaches the minimum prone height and the transition system ignores that stance.\n\n" +
                "Usage Example:\nDisabling prone simplifies the stance system to two states (standing/crouching). It is a good option for arcade or fast-paced games where lying down would break the rhythm.\n\n" +
                "General Example:\nTactical games like Arma 3 or Squad have prone as a fundamental mechanic. Fast-action games like Titanfall 2 have no prone to maintain agile combat pace.");
            y = CampoBool(y, ancho + 20, "HoldToProne",
                "Mantener El Input Presionado Para Acostarse", "Hold Input To Go Prone",
                "Explicación Base:\nDefine si el jugador debe mantener pulsada el input de acostarse o si funciona como una palanca.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Funciona igual que el equivalente para agacharse pero aplicado al estado acostado. En modo mantener, soltar la tecla inicia la transición de vuelta al estado anterior.\n\n" +
                "Ejemplo de Uso:\nEl modo palanca (False) es muy útil si se planea estar mucho tiempo Prone.\n\n" +
                "Ejemplo General:\nEn COD, al entrar al modo acostado ya no es necesario seguir pulsando el input para poder relajar la mano.",
                "Base Explanation:\nDefines whether the player must hold the prone input or if it works as a toggle.\n\n" +
                "Technical Explanation:\nBool variable. Works the same as the crouch equivalent but applied to the prone state. In hold mode, releasing the input starts the transition back to the previous state.\n\n" +
                "Usage Example:\nToggle mode (False) is very useful if you'll go on prone mode for a long time.\n\n" +
                "General Example:\nCOD uses the toggle mode to let the player rest they're hand.");

            y = SubHeader(y, ancho, ES ? "— Cooldown entre Transiciones" : "— Transition Cooldown");
            y = CampoBool(y, ancho + 20, "EnableCooldownBetweenBodyStateTransitions",
                "Activar El Cooldown Entre Las Transiciones Del Estado Corporal", "Enable Cooldown Between Body State Transitions",
                "Explicación Base:\nEvita que el jugador pueda cambiar de postura repetidamente de forma instantánea.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, después de cada transición de postura hay un tiempo mínimo de espera antes de poder realizar otra. Esto previene el spam de posturas y comportamientos explotables.\n\n" +
                "Ejemplo de Uso:\nActivar esta opción es fundamental en juegos competitivos. El crouch-spam (agacharse y levantarse muy rápido repetidamente para hacer el hitbox impredecible) es una técnica muy usada y considerada exploit en muchos juegos.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2 existe un cooldown implícito en la animación de agacharse para limitar el crouch-spam, aunque la comunidad ha criticado históricamente su implementación por ser inconsistente.",
                "Base Explanation:\nPrevents the player from changing stances repeatedly in an instant manner.\n\n" +
                "Technical Explanation:\nBool variable. If True, after each stance transition there is a minimum wait time before another can be performed. This prevents stance spam and exploitable behaviors like crouch-spam in combat.\n\n" +
                "Usage Example:\nEnabling this option is essential in competitive games. Crouch-spam (rapidly crouching and standing to make the hitbox unpredictable) is a widely used technique considered an exploit in many games.\n\n" +
                "General Example:\nIn Counter-Strike 2 there is an implicit cooldown in the crouch animation to limit crouch-spam, although the community has historically criticized its implementation for being inconsistent.");
            y = CampoFloat(y, ancho, "BodyStateTransitionCooldownTime",
                "Tiempo Del Cooldown Entre Las Transiciones Del Estado Corporal", "Cooldown Time Between Body State Transitions",
                "Explicación Base:\nDuración en segundos del cooldown entre cambios de postura.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Solo tiene efecto si 'Activar El Cooldown Entre Las Transiciones Del Estado Corporal' está en True. Este tiempo se cuenta desde que termina la transición anterior, no desde que empieza.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.4 segundos es suficiente para eliminar el crouch-spam sin que el juego se sienta torpe. Valores por encima de 0.8 empiezan a sentirse lentos y pueden frustrar al jugador en situaciones de urgencia.\n\n" +
                "Ejemplo General:\nEn COD se estima que el cooldown implícito de agacharse, ronda los 0.35 segundos, suficiente para limitar el spam pero sin penalizar el uso táctico normal.",
                "Base Explanation:\nDuration in seconds of the cooldown between stance changes.\n\n" +
                "Technical Explanation:\nFloat in seconds. Only takes effect if 'Enable Cooldown Between Body State Transitions' is True. This time is counted from when the previous transition ends, not from when it starts.\n\n" +
                "Usage Example:\nA value of 0.4 seconds is enough to eliminate crouch-spam without the game feeling clunky. Values above 0.8 start to feel slow and can frustrate the player in urgent situations.\n\n" +
                "General Example:\nIn Call of Duty the implicit crouch cooldown is estimated around 0.35 seconds, enough to limit spam but without penalizing normal tactical use.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Velocidad de la Transición" : "— Transition Speed");
            y = CampoFloat(y, ancho, "SpeedOfTheCapsuleColliderTransition",
                "Velocidad De La Transicion Del Capsule Collider", "Capsule Collider Transition Speed",
                "Explicación Base:\nVelocidad a la que el CapsuleCollider cambia de altura entre posturas.\n\n" +
                "Explicación Técnica:\nFloat que controla el Lerp de la altura del CapsuleCollider entre su valor actual y el objetivo. Valores altos hacen la transición casi instantánea. Valores bajos la hacen lenta y gradual. El valor se multiplica por Time.deltaTime en el Lerp.\n\n" +
                "Ejemplo de Uso:\nUn valor de 8 da una transición snappy que se siente responsiva. Un valor de 4 da una transición más suave que ayuda a la lectura visual del cambio de postura. Por debajo de 3 empieza a sentirse como que el jugador 'se derrite' al agacharse.\n\n" +
                "Ejemplo General:\nEn la mayoría de juegos FPS modernos la transición del collider es prácticamente instantánea para no penalizar mecánicamente al jugador que se agacha bajo una barrera.",
                "Base Explanation:\nSpeed at which the CapsuleCollider changes height between stances.\n\n" +
                "Technical Explanation:\nFloat that controls the Lerp of the CapsuleCollider height between its current value and the target. High values make the transition nearly instantaneous. Low values make it slow and gradual. The value is multiplied by Time.deltaTime in the Lerp.\n\n" +
                "Usage Example:\nA value of 8 gives a snappy transition that feels responsive. A value of 4 gives a smoother transition that helps the visual readability of the stance change. Below 3 it starts to feel like the player is 'melting' when crouching.\n\n" +
                "General Example:\nIn most modern FPS games the collider transition is practically instantaneous to avoid mechanically penalizing the player who crouches under a barrier.", 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Alturas del CapsuleCollider" : "— CapsuleCollider Heights");
            y = CampoFloat(y, ancho, "CapsuleColliderHeightWhileStanding",
                "Altura Del Capsule Collider - Estando De Pie", "Capsule Collider Height While Standing",
                "Explicación Base:\nAltura del CapsuleCollider cuando el jugador está de pie.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Este valor debe coincidir exactamente con la altura configurada en el componente CapsuleCollider en el Inspector de Unity para el estado de pie. Si hay discrepancia habrá huecos o solapamientos de colisión.\n\n" +
                "Ejemplo de Uso:\nUn valor de 1.8 representa una altura de jugador realista en escala 1:1 (1 unidad Unity = 1 metro). Para un FPS con una escala más arcade puede usarse 2.0 o 2.2.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS se trabaja con un CapsuleCollider de entre 1.8 y 2.0 unidades de altura de pie, representando aproximadamente la altura de un adulto promedio en escala 1:1.",
                "Base Explanation:\nHeight of the CapsuleCollider when the player is standing.\n\n" +
                "Technical Explanation:\nFloat in Unity units. This value must match exactly with the height configured in the CapsuleCollider component in the Unity Inspector for the standing state. If there is a discrepancy there will be collision gaps or overlaps.\n\n" +
                "Usage Example:\nA value of 1.8 represents a realistic player height at 1:1 scale (1 Unity unit = 1 meter). For an FPS with a more arcade scale, 2.0 or 2.2 can be used.\n\n" +
                "General Example:\nIn most FPS games a CapsuleCollider of between 1.8 and 2.0 units of standing height is used, representing approximately the height of an adult human at 1:1 scale.", 0.5f, 48f);
            y = CampoFloat(y, ancho, "CapsuleColliderHeightWhileCrouching",
                "Altura Del Capsule Collider - Estando Agachado", "Capsule Collider Height While Crouching",
                "Explicación Base:\nAltura del CapsuleCollider cuando el jugador está agachado.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Es el valor objetivo del Lerp de altura al entrar en el estado agachado. Define cuánto espacio físico ocupa el jugador agachado y por tanto qué espacios puede atravesar o en qué puede cubrirse.\n\n" +
                "Ejemplo de Uso:\nSi la altura de pie es 1.8, una altura agachada de 1.0 representa aproximadamente el 55%, que es una altura razonable para representar el agachado. Esto permite al jugador pasar por espacios y cubrirse detrás de objetos medianos.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2, la altura del jugador agachado es aproximadamente el 62% de la altura de pie, reduciendo el perfil pero manteniendo una silueta reconocible.",
                "Base Explanation:\nHeight of the CapsuleCollider when the player is crouching.\n\n" +
                "Technical Explanation:\nFloat in Unity units. It is the Lerp height target when entering the crouch state. Defines how much physical space the player occupies while crouching and therefore what spaces they can traverse or take cover behind.\n\n" +
                "Usage Example:\nIf standing height is 1.8, a crouching height of 1.0 represents approximately 55%, which is a reasonable crouch. This allows the player to pass through spaces and take cover behind medium-sized objects.\n\n" +
                "General Example:\nIn Counter-Strike 2, the crouching player height is approximately 62% of standing height, reducing the profile while maintaining a recognizable combat silhouette.", 0.3f, 48f);
            y = CampoFloat(y, ancho, "CapsuleColliderHeightWhileProne",
                "Altura Del Capsule Collider - Estando Acostado", "Capsule Collider Height While Prone",
                "Explicación Base:\nAltura del CapsuleCollider cuando el jugador está acostado.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Es el valor mínimo de altura del CapsuleCollider, correspondiente al estado acostado. Valores muy bajos permiten pasar por huecos muy estrechos como debajo de vehículos o en conductos de ventilación.\n\n" +
                "Ejemplo de Uso:\nCon una altura de pie de 1.8, un valor de 0.5 para acostado permite arrastrarse por espacios de apenas medio metro de altura, creando posibilidades de diseño de niveles muy interesantes.\n\n" +
                "Ejemplo General:\nEn Arma 3 el agachado reduce el perfil a su mínima expresión, siendo fundamental para tenderse en campo abierto y minimizar la silueta visible al enemigo en combate de largo alcance.",
                "Base Explanation:\nHeight of the CapsuleCollider when the player is prone.\n\n" +
                "Technical Explanation:\nFloat in Unity units. It is the minimum height value of the CapsuleCollider, corresponding to the prone state. Very low values allow passing through very tight gaps like under vehicles or in ventilation ducts.\n\n" +
                "Usage Example:\nWith a standing height of 1.8, a value of 0.5 for prone allows crawling through spaces barely half a meter high, creating very interesting level design possibilities.\n\n" +
                "General Example:\nIn Arma 3, prone reduces the soldier's profile to its minimum expression, being essential for lying in open terrain and minimizing the visible silhouette to enemies in long-range combat.", 0.1f, 48f);
            return y;
        }

        private float DibujarS04(float y, float ancho)
        {
            y = SubHeader(y, ancho, ES ? "— Permisos y Cantidad" : "— Permissions and Quantity");
            y = CampoBool(y, ancho + 20, "AllowThePlayerToJump",
                "Permitir Que El Jugador Salte", "Allow The Player To Jump",
                "Explicación Base:\nDefine si el jugador puede saltar.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el input de saltar no genera ninguna fuerza vertical. El jugador permanece en el suelo independientemente de si pulsa el input de salto.\n\n" +
                "Ejemplo de Uso:\nDesactivar el salto es útil en secciones de juego donde el diseño de niveles no admite exploración vertical, o durante secuencias narrativas donde el jugador no debe interrumpir la escena saltando.\n\n" +
                "Ejemplo General:\nEn juegos como DOOM o Quake, el salto es fundamental para el combate y siempre está activo.",
                "Base Explanation:\nDefines whether the player can jump.\n\n" +
                "Technical Explanation:\nBool variable. If False, the jump input generates no vertical force. The player stays on the ground regardless of whether they press the jump input.\n\n" +
                "Usage Example:\nDisabling jumping is useful in game sections where the level design does not support vertical exploration, or during narrative sequences where the player should not interrupt the scene by jumping.\n\n" +
                "General Example:\nIn games like DOOM or Quake, jumping is fundamental to combat and is always active.");
            y = CampoInt(y, ancho + 20, "HowManyJumps",
                "Cantidad De Saltos Disponibles", "Available Jump Count",
                "Explicación Base:\nDefine la cantidad de veces que el jugador puede saltar antes de necesitar tocar el suelo.\n\n" +
                "Explicación Técnica:\nVariable de tipo int. Si el valor asignado es 2, el jugador puede hacer un doble salto antes de necesitar tocar el suelo nuevamente.\n\n" +
                "Ejemplo de Uso:\nAsigna 1 a este valor si quieres que el jugador solo sea capaz de saltar 1 vez de forma consecutiva, asigna 4 si tu Jugador por ejemplo, es un Conejo.\n\n" +
                "Ejemplo General:\nEn la mayoria de juegos,el Doble o Triple Salto son la cantidad máxima de saltos, mayormente como una habilidad desbloqueable, pero puedes poner la cantidad que desees, desde 10, hasta 100 o más.",
                "Base Explanation:\nDefines how many times the player can jump before needing to touch the ground.\n\n" +
                "Technical Explanation:\nInt variable. If the assigned value is 2, the player can perform a double jump before needing to touch the ground again.\n\n" +
                "Usage Example:\nAssign 1 to this value if you want the player to only be able to jump once consecutively, assign 4 if your Player is, for example, a Bunny.\n\n" +
                "General Example:\nIn most games, Double or Triple Jumping are the maximum jump count, mostly as an unlockable ability, but you can set whatever amount you want, from 10 to 100 or more.", 0, 1048);
            y = CampoBool(y, ancho + 20, "AllowThePlayerToJumpWhileProne",
                "Permitir Que El Jugador Salte Desde Acostado", "Allow The Player To Jump While Prone",
                "Explicación Base:\nDefine si el jugador puede saltar directamente desde el estado acostado.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el jugador debe pasar primero agachado o de pie antes de poder saltar. Si está en True, el salto desde acostado inicia una transición forzada a de pie y aplica la fuerza de salto simultáneamente.\n\n" +
                "Ejemplo de Uso:\nActivar esto añade una maniobra de emergencia: saltar desde el suelo para levantarse rápidamente cuando estás siendo flanqueado. Desactivarlo obliga a una recuperación más lenta y penaliza más la postura acostado.\n\n" +
                "Ejemplo General:\nPocos FPS permiten saltar directamente desde acostado. La mayoría requieren pasar por la animación de levantarse antes de poder saltar, haciendo a la postura acostado una opción mas crítica a la hora de ser elegida.",
                "Base Explanation:\nDefines whether the player can jump directly from the prone state.\n\n" +
                "Technical Explanation:\nBool variable. If False, the player must first go through crouch or standing before being able to jump. If True, jumping from prone starts a forced transition to standing and applies the jump force simultaneously.\n\n" +
                "Usage Example:\nEnabling this adds an emergency maneuver: jumping from the ground to quickly get up when being flanked. Disabling it forces a slower recovery and further penalizes the prone stance.\n\n" +
                "General Example:\nFew FPS games allow jumping directly from prone. Most like Call of Duty require going through the get-up animation before being able to jump, making the prone stance a real commitment.");

            y = SubHeader(y, ancho, ES ? "— Fuerza y Gravedad" : "— Force & Gravity");
            y = CampoFloat(y, ancho, "ForceAppliedWhenJumping",
                "Fuerza Aplicada Al Saltar", "Force Applied When Jumping",
                "Explicación Base:\nFuerza vertical aplicada instantáneamente al jugador en el momento del salto.\n\n" +
                "Explicación Técnica:\nFloat que se aplica como velocidad inicial en el eje Y usando el Rigidbody. La altura máxima alcanzada depende de esta fuerza, la gravedad de Unity y los multiplicadores de gravedad configurados en este mismo bloque.\n\n" +
                "Ejemplo de Uso:\nCon la gravedad estándar de Unity (-9.81) un valor de 6 genera un salto de aproximadamente 1.8 unidades de altura. Para saltos más espectaculares al estilo Halo, valores entre 8 y 12 dan resultados satisfactorios.\n\n" +
                "Ejemplo General:\nEn Halo Infinite los Spartans saltan aproximadamente 1.5 veces su propia altura, lo que en términos de fuerza equivale a valores considerablemente altos, pero esto se usa para reflejar la potencia de los spartan.",
                "Base Explanation:\nVertical force applied instantly to the player at the moment of jumping.\n\n" +
                "Technical Explanation:\nFloat applied as initial velocity on the Y axis using the Rigidbody. Maximum jump height depends on this force, Unity gravity and the gravity multipliers configured in this same block.\n\n" +
                "Usage Example:\nWith Unity's standard gravity (-9.81) a value of 6 generates a jump of approximately 1.8 units of height, reasonable for a realistic FPS. For more spectacular jumps in the Halo style, values between 8 and 12 give satisfying results.\n\n" +
                "General Example:\nIn Halo Infinite, Spartans jump approximately 1.5 times their own height, which in terms of force equates to considerably high values reflecting the enhanced power of the characters.", 1f, 48f);
            y = CampoFloat(y, ancho, "GravityMultiplierDuringTheJump",
                "Multiplicador De Gravedad Durante El Salto", "Gravity Multiplier During The Jump",
                "Explicación Base:\nMultiplica la gravedad de Unity durante la fase de ascenso del salto.\n\n" +
                "Explicación Técnica:\nFloat. Durante el ascenso (velocidad Y positiva), la gravedad aplicada es Physics.gravity.y × este multiplicador. Valores mayores de 1 hacen el ascenso más corto y rápido. Valores menores de 1 lo hacen más largo y flotante.\n\n" +
                "Ejemplo de Uso:\nUn valor de 1.5 durante el ascenso combinado con 2.5 durante la caída crea el clásico arco de salto asimétrico que se siente muy satisfactorio en FPS: subida rápida y caída más rápida aún.\n\n" +
                "Ejemplo General:\nEsta técnica de gravedad asimétrica es la base del diseño de saltos en prácticamente todos los plataformas y FPS modernos, popularizada por videos como el de +.",
                "Base Explanation:\nMultiplies Unity gravity during the ascending phase of the jump.\n\n" +
                "Technical Explanation:\nFloat. During ascent (positive Y velocity), the applied gravity is Physics.gravity.y × this multiplier. Values greater than 1 make the ascent shorter and faster. Values less than 1 make it longer and floatier.\n\n" +
                "Usage Example:\nA value of 1.5 during ascent combined with 2.5 during fall creates the classic asymmetric jump arc that feels very satisfying in FPS: fast rise and even faster fall.\n\n" +
                "General Example:\nThis asymmetric gravity technique is the foundation of jump design in practically all modern platformers and FPS, popularized by game design analyses like the famous GDC video by Kyle Pittman.", 0.5f, 48f);
            y = CampoFloat(y, ancho, "AdditionalGravityMultiplierDuringTheFall",
                "Multiplicador De Gravedad Adicional Durante La Caida", "Additional Gravity Multiplier During The Fall",
                "Explicación Base:\nMultiplica adicionalmente la gravedad de Unity durante la fase de caída.\n\n" +
                "Explicación Técnica:\nFloat. Durante la caída (velocidad Y negativa), la gravedad aplicada es Physics.gravity.y × MultiplicadorDuranteSalto × este multiplicador adicional. La caída resulta ser significativamente más rápida que la subida.\n\n" +
                "Ejemplo de Uso:\nUn valor de 1.8 combinado con el multiplicador de ascenso en 1.0 da caídas notablemente más rápidas que la subida. Esto elimina la sensación de 'flotar' en el punto más alto del salto y da peso al personaje.\n\n" +
                "Ejemplo General:\nEn DOOM Eternal la caída es visiblemente más rápida que la subida, dando al Slayer una sensación de peso a pesar de su movilidad extrema. Este es uno de los elementos que hace sus saltos sentirse satisfactorios y predecibles.",
                "Base Explanation:\nAdditionally multiplies Unity gravity during the falling phase.\n\n" +
                "Technical Explanation:\nFloat. During falling (negative Y velocity), the applied gravity is Physics.gravity.y × JumpMultiplier × this additional multiplier. The fall ends up being significantly faster than the rise.\n\n" +
                "Usage Example:\nA value of 1.8 combined with an ascent multiplier of 1.0 gives noticeably faster falls than the rise. This eliminates the 'floating' feeling at the apex of the jump and gives weight to the character.\n\n" +
                "General Example:\nIn DOOM Eternal the fall is visibly faster than the rise, giving the Slayer a sense of weight despite his extreme mobility. This is one of the elements that makes his jumps feel satisfying and predictable.", 0.5f, 48f);

            y = SubHeader(y, ancho, ES ? "— Salto Variable" : "— Variable Jump");
            y = CampoBool(y, ancho + 20, "EnableVariableJump",
                "Activar El Salto Variable", "Enable The Variable Jump",
                "Explicación Base:\nPermite que mantener pulsado el input de saltar aumente la altura del salto.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, mientras el jugador mantiene pulsado el input de salto durante el ascenso, se aplica una fuerza adicional por segundo. La duración máxima de este boost está limitada por 'Tiempo Maximo Manteniendo Del Input De Salto'.\n\n" +
                "Ejemplo de Uso:\nEl salto variable añade una dimensión de habilidad al salto: pulsar brevemente da un salto bajo, mantener pulsado da el salto máximo. Esto permite esquivar obstáculos bajos sin gastar toda la altura disponible.\n\n" +
                "Ejemplo General:\nEsta mecánica viene del diseño de plataformas 2D (Super Mario Bros. la usa desde 1985) y ha sido adaptada a juegos 3D. En juegos FPS como Halo el salto es fijo, pero en plataformas 3D como A Hat in Time el salto variable es fundamental.",
                "Base Explanation:\nAllows holding the jump input to increase jump height.\n\n" +
                "Technical Explanation:\nBool variable. If True, while the player holds the jump input during ascent, additional force is applied per second. The maximum duration of this boost is limited by 'Maximum Time The Jump Input Is Held'.\n\n" +
                "Usage Example:\nVariable jump adds a skill dimension to jumping: a brief press gives a low jump, holding gives the maximum jump. This allows dodging low obstacles without using all available height.\n\n" +
                "General Example:\nThis mechanic comes from 2D platform design (Super Mario Bros. has used it since 1985) and has been adapted to 3D games. In FPS games like Halo the jump is fixed, but in 3D platformers like A Hat in Time variable jump is fundamental.");
            y = CampoFloat(y, ancho, "MaximumHeldTimeOfTheJumpInput",
                "Tiempo Maximo Manteniendo El Input De Salto", "Maximum Time The Jump Input Is Held",
                "Explicación Base:\nTiempo máximo en segundos durante el que mantener el input de salto añade altura extra.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Una vez transcurrido este tiempo desde el inicio del salto, la fuerza adicional del salto variable deja de aplicarse aunque el jugador siga pulsando el input. Esto limita la altura máxima alcanzable.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.2 segundos da una ventana de boost muy breve, resultando en una diferencia moderada entre salto corto y largo. Un valor de 0.4 segundos amplía bastante la diferencia.\n\n" +
                "Ejemplo General:\nEn Super Mario 64, la ventana de salto variable es de aproximadamente 0.25 segundos. En juegos FPS donde se implementa esta mecánica, suele ser similar para no dar saltos excesivamente altos.",
                "Base Explanation:\nMaximum time in seconds during which holding the jump input adds extra height.\n\n" +
                "Technical Explanation:\nFloat in seconds. Once this time has elapsed since the start of the jump, the variable jump additional force stops being applied even if the player keeps pressing the input. This limits the maximum reachable height.\n\n" +
                "Usage Example:\nA value of 0.2 seconds gives a very brief boost window, resulting in a moderate difference between short and long jump. A value of 0.4 seconds considerably widens the difference.\n\n" +
                "General Example:\nIn Super Mario 64, the variable jump window is approximately 0.25 seconds. In FPS games where this mechanic is implemented, it is usually similar to avoid excessively high jumps.", 0.05f, 48f);
            y = CampoFloat(y, ancho, "ExtraForcePerSecondOfTheVariableJump",
                "Fuerza Extra Por Segundo Del Salto Variable", "Extra Force Per Second Of The Variable Jump",
                "Explicación Base:\nFuerza adicional por segundo que se aplica mientras el jugador mantiene pulsado el input de salto.\n\n" +
                "Explicación Técnica:\nFloat. Durante el tiempo de boost del salto variable, este valor se suma a la velocidad Y del jugador cada segundo (multiplicado por Time.deltaTime). Trabaja en conjunción con 'Tiempo Maximo Manteniendo Del Input De Salto' para definir la altura máxima adicional.\n\n" +
                "Ejemplo de Uso:\nCon una fuerza inicial de salto de 6 y este valor en 10, durante 0.2 segundos de boost el jugador gana 2 unidades extra de velocidad Y, resultando en un salto notablemente más alto que el mínimo.\n\n" +
                "Ejemplo General:\nLa calibración de este valor requiere prueba y error ya que interactúa con la fuerza base de salto y los multiplicadores de gravedad. Empezar con un valor igual a la fuerza base de salto y ajustar desde ahí suele dar buenos resultados.",
                "Base Explanation:\nAdditional force per second applied while the player holds the jump input.\n\n" +
                "Technical Explanation:\nFloat. During the variable jump boost time, this value is added to the player's Y velocity each second (multiplied by Time.deltaTime). Works in conjunction with 'Maximum Time The Jump Input Is Held' to define the maximum additional height.\n\n" +
                "Usage Example:\nWith a base jump force of 6 and this value at 10, during 0.2 seconds of boost the player gains 2 extra units of Y velocity, resulting in a noticeably higher jump than the minimum.\n\n" +
                "General Example:\nCalibrating this value requires trial and error as it interacts with the base jump force and gravity multipliers. Starting with a value equal to the base jump force and adjusting from there usually gives good results.", 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Detección del Suelo" : "— Ground Detection");
            y = CampoLayerMask(y, ancho, "LayersThatAreConsideredGround",
                "Capas Que Se Consideran Suelo", "Layers That Are Considered Ground",
                "Explicación Base:\nDefine qué capas de las físicas de Unity cuentan como suelo para el sistema de detección.\n\n" +
                "Explicación Técnica:\nLayerMask usada en el OverlapSphere de la detección del suelo. Solo los objetos en estas capas se consideran suelo. Es fundamental excluir la capa del propio jugador para evitar que se detecte a sí mismo como suelo.\n\n" +
                "Ejemplo de Uso:\nIncluir capas como 'Ground', 'Terrain', 'StaticGeometry' y excluir 'Player', 'Trigger', 'Water'. Si también quieres poder caminar sobre enemigos o vehículos, añade esas capas.\n\n" +
                "Ejemplo General:\nEn Source Engine (Half-Life, Counter-Strike) el sistema de detección de suelo distingue entre geometría de mundo, modelos de prop y entidades dinámicas, permitiendo caminar sobre objetos físicos en movimiento.",
                "Base Explanation:\nDefines which Unity physics layers count as ground for the detection system.\n\n" +
                "Technical Explanation:\nLayerMask used in the ground detection OverlapSphere. Only objects in these layers will trigger IsGrounded. It is essential to exclude the player's own layer to prevent it from detecting itself as ground.\n\n" +
                "Usage Example:\nInclude layers like 'Ground', 'Terrain', 'StaticGeometry' and exclude 'Player', 'Trigger', 'Water'. If you also want to walk on enemies or vehicles, add those layers.\n\n" +
                "General Example:\nIn Source Engine (Half-Life, Counter-Strike) the ground detection system distinguishes between world geometry, prop models and dynamic entities, allowing walking on moving physical objects.");
            y = CampoFloat(y, ancho, "RadiusOfTheGroundDetectionOverlapSphere",
                "Radio Del Overlap Sphere De Deteccion Del Suelo", "Ground Detection Overlap Sphere Radius",
                "Explicación Base:\nRadio de la esfera usada para detectar si el jugador está en contacto con el suelo.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Una Physics.OverlapSphere de este radio se ejecuta cada frame en la posición de detección. Si detecta algún collider en las capas configuradas, se considera que el jugador se encuentra en el suelo. Radios muy pequeños pueden causar falsos negativos en geometría irregular. Radios muy grandes pueden detectar suelo a través de paredes o cuando el jugador está junto a una pared.\n\n" +
                "Ejemplo de Uso:\nUn radio de 0.3 funciona bien para un jugador con CapsuleCollider de radio 0.4. Siempre debe ser ligeramente menor que el radio del CapsuleCollider para evitar detecciones falsas con paredes verticales.\n\n" +
                "Ejemplo General:\nLa detección de suelo mediante OverlapSphere es el método más común en FPS de Unity. La alternativa es usar raycast hacia abajo, pero la esfera da más tolerancia en geometría irregular como escalones o rampas.",
                "Base Explanation:\nRadius of the sphere used to detect if the player is in contact with the ground.\n\n" +
                "Technical Explanation:\nFloat in Unity units. A Physics.OverlapSphere of this radius runs every frame at the detection position. If it detects any collider in the configured layers, the player is considered grounded. Very small radii can cause false negatives on irregular geometry. Very large radii can detect ground through walls or when the player is next to a wall.\n\n" +
                "Usage Example:\nA radius of 0.3 works well for a player with a CapsuleCollider radius of 0.4. It should always be slightly smaller than the CapsuleCollider radius to avoid false detections with vertical walls.\n\n" +
                "General Example:\nGround detection via OverlapSphere is the most common method in Unity FPS. The alternative is using a downward raycast, but the sphere gives more tolerance on irregular geometry like steps or ramps.", 0.05f, 48f);
            y = CampoFloat(y, ancho, "DownwardOffsetOfTheGroundDetectionOverlapSphere",
                "Desplazamiento Hacia Abajo Del Overlap Sphere De Deteccion Del Suelo", "Downward Offset Of The Ground Detection Overlap Sphere",
                "Explicación Base:\nDistancia hacia abajo desde el pivote del jugador donde se centra la esfera de detección de suelo.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. El centro de la OverlapSphere se posiciona en transform.position + Vector3.down × este valor. Debe colocarse cerca de los pies del jugador pero sin exceder el suelo en condiciones normales.\n\n" +
                "Ejemplo de Uso:\nSi el pivote del jugador está en su centro (altura 1.8, pivote a 0.9 del suelo), un desplazamiento de 0.85 coloca la esfera justo encima del suelo. Ajusta este valor si el jugador 'flota' o si se detecta el suelo antes de si quiera tocarlo.\n\n" +
                "Ejemplo General:\nEn la mayoría de implementaciones de CharacterControllers en Unity este offset está entre 0.8 y 1.0 veces la mitad de la altura del collider, dependiendo de dónde esté el pivote del modelo.",
                "Base Explanation:\nDownward distance from the player's pivot where the ground detection sphere is centered.\n\n" +
                "Technical Explanation:\nFloat in Unity units. The center of the OverlapSphere is positioned at transform.position + Vector3.down × this value. Should be placed near the player's feet but without going through the ground under normal conditions.\n\n" +
                "Usage Example:\nIf the player's pivot is at their center (height 1.8, pivot at 0.9 from ground), an offset of 0.85 places the sphere just above the ground. Adjust this value if the player 'floats' or if the ground is detected before even touching it.\n\n" +
                "General Example:\nIn most Unity CharacterControllers implementations this offset is between 0.8 and 1.0 times half the collider height, depending on where the model's pivot is located.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Coyote Time" : "— Coyote Time");
            y = CampoBool(y, ancho + 20, "EnableCoyoteTime",
                "Activar El Coyote Time", "Enable The Coyote Time",
                "Explicación Base:\nPermite al jugador saltar durante un breve periodo de tiempo después de haber caminado fuera del borde de una plataforma.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, cuando el jugador deja de estar en el suelo sin haber saltado, se inicia un contador. Durante ese tiempo el jugador aún puede saltar aunque técnicamente esté en el aire. El nombre viene del personaje Wile E. Coyote, que sigue corriendo un momento antes de caer.\n\n" +
                "Ejemplo de Uso:\nSin coyote time, los saltos desde bordes se sienten injustos porque el jugador pulsa el salto justo cuando está al borde pero el sistema ya lo considera en el aire. Con un coyote time de 0.15s, esos saltos funcionan correctamente.\n\n" +
                "Ejemplo General:\nEl coyote time es considerado una buena práctica de diseño en casi todos los juegos de plataformas modernos. Juegos como Hollow Knight, Celeste y prácticamente todos los platformers indie modernos lo implementan de forma estándar.",
                "Base Explanation:\nAllows the player to jump for a brief time after having walked off the edge of a platform.\n\n" +
                "Technical Explanation:\nBool variable. If True, when the player stops being grounded without having jumped, a timer starts. During that time the player can still jump even though they are technically in the air. The name comes from Wile E. Coyote, who keeps running for a moment before falling.\n\n" +
                "Usage Example:\nWithout coyote time, jumps from edges feel unfair because the player presses jump right at the edge but the system already considers them airborne. With a coyote time of 0.15s, those jumps work correctly.\n\n" +
                "General Example:\nCoyote time is considered a design best practice in almost all modern platforming games. Games like Hollow Knight, Celeste and virtually all modern indie platformers implement it as standard.");
            y = CampoFloat(y, ancho, "DurationOfTheCoyoteTime",
                "Duracion Del Coyote Time", "Coyote Time Duration",
                "Explicación Base:\nVentana de tiempo en segundos durante la que el coyote time está activo tras salir del borde.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Este es el tiempo desde que el jugador deja de estar en el suelo hasta que el coyote time expira. Durante este tiempo la detección del suelo retorna True aunque el jugador ya esté técnicamente cayendo.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.1 a 0.15 segundos es el estándar de la industria. Por encima de 0.2 empieza a sentirse como que el jugador 'flota' en los bordes. Por debajo de 0.08 es demasiado breve para ser perceptible.\n\n" +
                "Ejemplo General:\nCeleste usa un coyote time de aproximadamente 0.1 segundos, que es el valor de referencia del género. Este juego es frecuentemente citado en análisis de diseño de juego como ejemplo perfecto de implementación del coyote time.",
                "Base Explanation:\nTime window in seconds during which coyote time is active after leaving the edge.\n\n" +
                "Technical Explanation:\nFloat in seconds. This is the time from when the player stops being grounded until coyote time expires. During this time ground detection returns true even though the player is technically already falling.\n\n" +
                "Usage Example:\nA value of 0.1 to 0.15 seconds is the industry standard. Above 0.2 it starts to feel like the player 'floats' at edges. Below 0.08 it is too brief to be perceptible.\n\n" +
                "General Example:\nCeleste uses a coyote time of approximately 0.1 seconds, which is the genre reference value. This game is frequently cited in game design analyses as a perfect example of coyote time implementation.", 0.05f, 48f);

            y = SubHeader(y, ancho, ES ? "— Jump Buffering" : "— Jump Buffering");
            y = CampoBool(y, ancho + 20, "EnableJumpBuffering",
                "Activar El Jump Buffering", "Enable The Jump Buffering",
                "Explicación Base:\nRecuerda que el jugador pulsó el input de saltar mientras estaba en el aire y ejecuta el salto automáticamente en cuanto aterriza.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, cuando el jugador pulsa el input de salto mientras está en el aire, se guarda una 'marca' temporal. Si el jugador aterriza antes de que expire esa 'marca', el salto se ejecuta automáticamente. Esto compensa el desajuste entre el timing del jugador y la detección de suelo.\n\n" +
                "Ejemplo de Uso:\nSin jump buffering, el jugador que pulsa el salto justo antes de aterrizar no salta porque el sistema no estaba en el suelo en ese momento. Con una 'marca' de 0.15s, ese salto anticipado funciona.\n\n" +
                "Ejemplo General:\nJunto con el coyote time, el jump buffering es una de las dos técnicas de calidad de vida más implementadas en plataformas modernos. Celeste combina ambas técnicas y es el ejemplo de referencia más citado para jump feel en el diseño de juegos.",
                "Base Explanation:\nRemembers that the player pressed the jump button while airborne and automatically executes the jump as soon as they land.\n\n" +
                "Technical Explanation:\nBool variable. If True, when the player presses the jump input while airborne, a timed mark is saved. If the player lands before that mark expires, the jump executes automatically. This compensates for the mismatch between the player's timing and ground detection.\n\n" +
                "Usage Example:\nWithout jump buffering, a player who presses jump just before landing does not jump because the system was not grounded at that moment. With buffering of 0.15s, that anticipatory jump works.\n\n" +
                "General Example:\nAlong with coyote time, jump buffering is one of the two most widely implemented quality-of-life techniques in modern platformers. Celeste combines both techniques and is the most cited reference example for jump feel in game design.");
            y = CampoFloat(y, ancho, "DurationOfTheJumpBuffering",
                "Duracion Del Jump Buffering", "Jump Buffering Duration",
                "Explicación Base:\nTiempo en segundos durante el que el sistema recuerda la pulsación del salto antes de aterrizar.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Si el jugador pulsa el salto y aterriza dentro de este tiempo, el salto se ejecuta. Si pasa más tiempo del indicado sin aterrizar, la 'marca' expira y la pulsación se descarta.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.1 a 0.15 segundos es el rango estándar. Valores más altos hacen el sistema más permisivo pero pueden sentirse como que el juego 'anticipa demasiado' las acciones del jugador.\n\n" +
                "Ejemplo General:\nCeleste usa jump buffering de 0.1 segundos emparejado con coyote time del mismo valor. Esta combinación es tan efectiva que muchos desarrolladores la adoptan sin modificar estos valores exactos.",
                "Base Explanation:\nTime in seconds during which the system remembers the jump press before landing.\n\n" +
                "Technical Explanation:\nFloat in seconds. If the player presses jump and lands within this time, the jump executes. If more time passes without landing, the buffer expires and the press is discarded.\n\n" +
                "Usage Example:\nA value of 0.1 to 0.15 seconds is the standard range. Higher values make the system more forgiving but can feel like the game 'anticipates too much' the player's actions.\n\n" +
                "General Example:\nCeleste uses jump buffering of 0.1 seconds paired with coyote time of the same value. This combination is so effective that many developers adopt it without modifying these exact values.", 0.05f, 48f);
            return y;
        }

        private float DibujarS05(float y, float ancho)
        {
            y = CampoBool(y, ancho + 20, "EnableTheStaminaSystem",
                "Activar El Sistema De Resistencia", "Enable The Stamina System",
                "Explicación Base:\nInterruptor maestro del sistema de resistencia.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, ningún coste de resistencia se aplica bajo ninguna circunstancia. El jugador puede correr, saltar, deslizarse y hacer dash indefinidamente. Las barras y displays de la resistencia en el HUD también deben ocultarse cuando esto está en False para no ser un desproposito.\n\n" +
                "Ejemplo de Uso:\nDesactivar la resistencia simplifica radicalmente el gameplay. Es ideal para juegos de acción arcade donde la gestión de recursos no es parte del diseño, o para un modo 'sin penalizaciones' de accesibilidad.\n\n" +
                "Ejemplo General:\nJuegos como DOOM Eternal no tienen resistencia para el movimiento básico, lo que refuerza su filosofía de combate sin restricciones. En cambio, Escape from Tarkov tiene un sistema de resistencia muy detallado que es central en su diseño de supervivencia.",
                "Base Explanation:\nMaster switch for the stamina system.\n\n" +
                "Technical Explanation:\nBool variable. If False, no stamina cost is applied under any circumstance. The player can sprint, jump, slide and dash indefinitely. Stamina HUD bars and displays should also be hidden when this is False.\n\n" +
                "Usage Example:\nDisabling stamina radically simplifies gameplay. It is ideal for arcade action games where resource management is not part of the design, or for an accessibility 'no penalties' mode.\n\n" +
                "General Example:\nGames like DOOM Eternal have no stamina for basic movement, reinforcing their philosophy of unrestricted combat. In contrast, Escape from Tarkov has a very detailed stamina system that is central to its survival design.");
            y = CampoFloat(y, ancho, "MaximumPlayerStamina",
                "Resistencia Maxima Del Jugador", "Player Maximum Stamina",
                "Explicación Base:\nValor máximo de resistencia del jugador.\n\n" +
                "Explicación Técnica:\nFloat que define el tope del valor de la resistencia. Todos los costes y tasas de regeneración operan sobre este valor como referencia. Usar un valor de 100 facilita pensar los costes como porcentajes directos.\n\n" +
                "Ejemplo de Uso:\nCon un valor de 100 y un coste al correr de 15 por segundo, el jugador puede correr aproximadamente 6.6 segundos continuos si inició con la resistencia llena. Con un valor de 200 y el mismo coste, puede correr 13.3 segundos.\n\n" +
                "Ejemplo General:\nMuchos juegos usan 100 como el valor máximo de de la resistencia por conveniencia matemática. Halo Infinite no tiene barra de resistencia visible para el movimiento, pero internamente gestiona un cooldown al correr que funciona de manera similar.",
                "Base Explanation:\nPlayer's maximum stamina value.\n\n" +
                "Technical Explanation:\nFloat that defines the ceiling of the stamina bar. All costs and regeneration rates operate on this value as a reference. Using a value of 100 makes it easy to think of costs as direct percentages.\n\n" +
                "Usage Example:\nWith a value of 100 and a sprint cost of 15 per second, the player can sprint approximately 6.6 seconds continuously from full stamina. With a value of 200 and the same cost, they can sprint for 13.3 seconds.\n\n" +
                "General Example:\nMany games use 100 as the maximum stamina value for mathematical convenience. Halo Infinite has no visible stamina bar for movement, but internally manages a sprint cooldown that works similarly.", 1f, 480f);

            y = SubHeader(y, ancho, ES ? "— Costes por Acción" : "— Action Costs");
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostWhenRunning", "StaminaCostPerSecondWhenRunning",
                "Coste De Resistencia Por Segundo Al Correr", "Stamina Cost Per Second While Running",
                "Explicación Base:\nCoste de resistencia por segundo mientras el jugador está corriendo.\n\n" +
                "Explicación Técnica:\nFloat que se descuenta de la resistencia actual cada segundo (multiplicado por Time.deltaTime) mientras el jugador está corriendo. Si la resistencia llega a 0 el correr debería cancelarse automáticamente o reducir su velocidad.\n\n" +
                "Ejemplo de Uso:\nCon la resistencia máxima de 100 y un coste de 20 por segundo al correr, se puede correr exactamente 5 segundos. Para Escape from Tarkov el límite corriendo ronda los 5-8 segundos con equipo ligero, lo que da una referencia realista.\n\n" +
                "Ejemplo General:\nEn juegos arcade este valor suele ser 0 o el sistema está directamente desactivado/ no existe.",
                "Base Explanation:\nStamina cost per second while the player is sprinting.\n\n" +
                "Technical Explanation:\nFloat deducted from current stamina each second (multiplied by Time.deltaTime) while the player is in sprint state. If stamina reaches 0, sprint should automatically cancel or reduce speed.\n\n" +
                "Usage Example:\nWith maximum stamina of 100 and a cost of 20 per second, sprint lasts exactly 5 seconds. For Escape from Tarkov the sprint limit is around 5-8 seconds with light gear, which provides a realistic reference.\n\n" +
                "General Example:\nIn arcade games this value is usually 0 or the system is disabled.", 0f, 148f);
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostWhenJumping", "StaminaCostWhenJumping",
                "Coste De Resistencia Al Saltar", "Stamina Cost When Jumping",
                "Explicación Base:\nCoste instantáneo de resistencia al ejecutar un salto.\n\n" +
                "Explicación Técnica:\nFloat que se descuenta de la resistencia actual en el frame exacto en el que se ejecuta el salto. A diferencia del coste al correr que es por segundo, este es un coste único por evento de salto.\n\n" +
                "Ejemplo de Uso:\nUn coste de 10 por salto con una resistencia de 100, permite exactamente 10 saltos consecutivos con la resistencia llena. Combinado con el costeo al correr, un jugador que salta mientras corre como en Minecarft, gasta resistencia tanto al correr como con cada salto.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov saltar tiene un coste considerable de resistencia. Esto penaliza el salto repetitivo (bunny hopping) que sería un exploit en un juego realista.",
                "Base Explanation:\nInstant stamina cost when executing a jump.\n\n" +
                "Technical Explanation:\nFloat deducted from current stamina in the exact frame the jump is executed. Unlike the sprint cost which is per second, this is a unique cost per jump event.\n\n" +
                "Usage Example:\nA cost of 10 per jump with stamina of 100 allows exactly 10 consecutive jumps from full stamina. Combined with the sprint cost, a player who sprint-jumps spends stamina from both sprinting and each jump.\n\n" +
                "General Example:\nIn Escape from Tarkov jumping has a considerable stamina cost. This penalizes repetitive jumping (bunny hopping) which would be an exploit in a realistic game.", 0f, 148f);
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostWhenCrouching", "StaminaCostWhenCrouching",
                "Coste De Resistencia Al Agacharse", "Stamina Cost When Crouching",
                "Explicación Base:\nCoste instantáneo de resistencia al entrar en el estado agachado.\n\n" +
                "Explicación Técnica:\nFloat que se descuenta al inicio de la transición a agacharse. No es un coste continuo mientras se está agachado, sino el costo único de la acción agacharse.\n\n" +
                "Ejemplo de Uso:\nUn valor bajo de 3-5 penaliza el spam de estarse agachando sin hacer el agacharse algo normalmente costoso. Combinado con el cooldown de la transición, el spammear el agachado se vuelve poco rentable tanto por el cooldown como por la pérdida de resistencia.\n\n" +
                "Ejemplo General:\nPocos juegos implementan un coste de resistencia al agacharse. Escape from Tarkov es uno de los ejemplos más completos donde prácticamente toda acción física tiene un impacto en la resistencia del personaje, por eso le menciono tanto.",
                "Base Explanation:\nInstant stamina cost when entering the crouching state.\n\n" +
                "Technical Explanation:\nFloat deducted at the start of the crouch transition. It is not a continuous cost while crouching, but the one-time cost of the crouching action.\n\n" +
                "Usage Example:\nA low value of 3-5 penalizes crouch-spam without making normal crouch use costly. Combined with the transition cooldown, crouch-spam becomes unprofitable both due to the cooldown and stamina loss.\n\n" +
                "General Example:\nFew games implement a stamina cost for crouching. Escape from Tarkov is one of the most complete examples where virtually every physical action has an impact on the character's stamina, that's why i mention it so much.", 0f, 48f);
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostWhenGoingProne", "StaminaCostWhenGoingProne",
                "Coste De Resistencia Al Acostarse", "Stamina Cost When Going Prone",
                "Explicación Base:\nCoste instantáneo de resistencia al entrar en el estado acostado.\n\n" +
                "Explicación Técnica:\nFloat que se descuenta al inicio de la transición a acostado. Acostarse es una acción más drástica que agacharse, por lo que su coste puede ser mayor para reflejar el esfuerzo físico de tirarse al suelo.\n\n" +
                "Ejemplo de Uso:\nUn coste de 8-12 al acostarse vs 3-5 al agacharse refleja que acostarse es un movimiento más comprometedor y físicamente demandante. Esto da coherencia al sistema de posturas.\n\n" +
                "Ejemplo General:\nEn juegos militares realistas como Arma 3, acostarse tiene un coste de resistencia implícito en el tiempo de la animación y la recuperación posterior, aunque no siempre es un sistema de resistencia explícito.",
                "Base Explanation:\nInstant stamina cost when entering the prone state.\n\n" +
                "Technical Explanation:\nFloat deducted at the start of the prone transition. Going prone is a more drastic action than crouching, so its cost can be higher to reflect the physical effort of throwing yourself to the ground.\n\n" +
                "Usage Example:\nA cost of 8-12 for prone vs 3-5 for crouch reflects that going prone is a more committed and physically demanding movement. This gives coherence to the stance system.\n\n" +
                "General Example:\nIn realistic military games like Arma 3, going prone has an implicit stamina cost in animation time and subsequent recovery, although it is not always an explicit stamina system.", 0f, 48f);
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostOnEachPostureTransition", "StaminaCostPerPostureTransition",
                "Coste De Resistencia Por Cada Transicion De Postura", "Stamina Cost Per Each Stance Transition",
                "Explicación Base:\nCoste adicional de resistencia que se aplica en cada cambio de postura, independientemente de cuál sea la transición.\n\n" +
                "Explicación Técnica:\nFloat que se suma a los costes específicos de cada postura en cada transición. Este coste se aplica en todas las transiciones: de pie a agachado, agachado a acostado, acostado a agachado, agachado a de pie, etc.\n\n" +
                "Ejemplo de Uso:\nUsarlo junto a los costes específicos de postura amplifica el sistema: cambiar de postura continuamente en combate es costoso. Un valor de 2-5 añade penalizaciónes sin ser excesivo.\n\n" +
                "Ejemplo General:\nEste tipo de coste transaccional por cambio de postura no es habitual en juegos mainstream por lo estresante que puede resultar, pero refuerza el realismo en juegos de simulación táctica donde la gestión de postura es una habilidad en sí misma.",
                "Base Explanation:\nAdditional stamina cost applied on each stance change, regardless of which transition it is.\n\n" +
                "Technical Explanation:\nFloat added to the specific costs of each stance on each transition. This cost applies to all transitions: standing to crouch, crouch to prone, prone to crouch, crouch to standing, etc.\n\n" +
                "Usage Example:\nUsing it alongside specific stance costs amplifies the system: continuously changing stance in combat is costly. A value of 2-5 adds penalization without being excessive.\n\n" +
                "General Example:\nThis type of transactional cost per stance change is not common in mainstream games but reinforces realism in tactical simulation games where stance management is a skill in itself.", 0f, 48f);
            y = CampoBoolFloat(y, ancho, "EnableExtraStaminaCostWhenJumpingFromProne", "ExtraStaminaCostWhenJumpingFromTheProneState",
                "Coste Extra De Resistencia Al Saltar Desde El Estado Acostado", "Extra Stamina Cost When Jumping From Prone",
                "Explicación Base:\nCoste adicional de resistencia al saltar desde el estado acostado, que se suma al coste base de saltar.\n\n" +
                "Explicación Técnica:\nFloat que se añade al 'Coste De Resistencia Al Saltar' cuando el jugador ejecuta el salto desde el estado acostado. Refleja el esfuerzo físico que se hace al incorporarse desde el suelo con suficiente impulso como para igual saltar.\n\n" +
                "Ejemplo de Uso:\nSi el coste del salto normal es 10 y este coste extra es 15, saltar desde el estado acostado cuesta 25 en total. Esto disuade usar el acostarse como una táctica de snipear y saltar inmediatamente si alguien se acerca.\n\n" +
                "Ejemplo General:\nLa penalización extra al saltar desde posiciones comprometedoras es un mecanismo de diseño que recompensa al jugador que planifica su postura y penaliza los cambios de postura demasiado abusivos.",
                "Base Explanation:\nAdditional stamina cost when jumping from the prone state, added to the base jump cost.\n\n" +
                "Technical Explanation:\nFloat added to 'Stamina Cost When Jumping' when the player executes a jump from prone. Reflects the greater physical effort of getting up from the ground with enough momentum to jump.\n\n" +
                "Usage Example:\nIf the normal jump cost is 10 and this extra cost is 15, jumping from prone costs 25 stamina total. This discourages using prone as a sniper tactic and immediately jumping if someone approaches.\n\n" +
                "General Example:\nThe extra penalty for jumping from compromised positions is a design mechanism that rewards players who plan their stance and penalizes overly abusive stance changes.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Regeneración" : "— Regeneration");
            y = CampoFloat(y, ancho, "DelayInSecondsBeforeStaminaStartsRegenerating",
                "Delay En Segundos Antes De Que Empiece A Regenerarse La Resistencia", "Delay In Seconds Before Stamina Starts Regenerating",
                "Explicación Base:\nTiempo de espera en segundos después del último gasto de resistencia antes de que empiece su regeneración.\n\n" +
                "Explicación Técnica:\nFloat en segundos. El contador se reinicia cada vez que se descuenta resistencia. La regeneración solo comienza cuando este tiempo transcurre sin ningún gasto. Esto previene que la resistencia se regenere al instante entre costes pequeños y frecuentes.\n\n" +
                "Ejemplo de Uso:\nUn delay de 1.5 segundos obliga al jugador a descansar un momento antes de recuperar resistencia. Un delay de 3 segundos es más severo, típico de juegos de supervivencia donde la recuperación requiere parar completamente.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov el delay de regeneración de resistencia es significativo, obligando al jugador a gestionar activamente cuándo correr y cuándo caminar.",
                "Base Explanation:\nWait time in seconds after the last stamina expenditure before regeneration begins.\n\n" +
                "Technical Explanation:\nFloat in seconds. The counter resets every time stamina is deducted. Regeneration only begins when this time elapses without any expenditure. This prevents stamina from regenerating instantly between small, frequent costs.\n\n" +
                "Usage Example:\nA delay of 1.5 seconds forces the player to rest a moment before recovering stamina. A delay of 3 seconds is more severe, typical of survival games where recovery requires stopping completely.\n\n" +
                "General Example:\nIn Escape from Tarkov the stamina regeneration delay is significant, forcing the player to actively manage when to run and when to walk.", 0f, 48f);
            y = CampoFloat(y, ancho, "StaminaRegenerationSpeedWhileThePlayerIsIdle",
                "Velocidad De Regeneracion De Resistencia Mientras El Jugador Esta Quieto", "Stamina Regeneration Speed While The Player Is Idle",
                "Explicación Base:\nCantidad de resistencia recuperada por segundo cuando el jugador está quieto.\n\n" +
                "Explicación Técnica:\nFloat en unidades de resistencia por segundo. Se aplica cuando el jugador está quieto, sin ningún input de movimiento activo. Es la tasa de regeneración más rápida del sistema.\n\n" +
                "Ejemplo de Uso:\nSi la resistencia máxima es 100 y este valor es 30, el jugador tarda aproximadamente 3.3 segundos en recuperarse completamente desde 0 estando quieto. Para una experiencia más 'dura', valores de 10-15 obligan a tener que recurrir a pausas largas.\n\n" +
                "Ejemplo General:\nEn la mayoría de juegos de acción con resistencia, recuperarse de pie y o estando quieto es la opción más rápida. En Dark Souls la resistencia se recupera completamente en aproximadamente 4.3 segundos al quedarse de pie o al solo caminar.",
                "Base Explanation:\nAmount of stamina recovered per second when the player is idle.\n\n" +
                "Technical Explanation:\nFloat in stamina units per second. Applied when the player is standing still with no active movement input. It is the fastest regeneration rate in the system.\n\n" +
                "Usage Example:\nIf maximum stamina is 100 and this value is 30, the player takes approximately 3.3 seconds to fully recover from 0 while idle. For a more punishing experience, values of 10-15 force long pauses.\n\n" +
                "General Example:\nIn most action games with stamina, recovering while standing still is the fastest option. In Dark Souls stamina fully recovers in approximately 4.3 seconds while idle/walking.", 0f, 248f);
            y = CampoFloat(y, ancho, "StaminaRegenerationSpeedWhileThePlayerWalks",
                "Velocidad De Regeneracion De Resistencia Mientras El Jugador Camina", "Stamina Regeneration Speed While The Player Walks",
                "Explicación Base:\nCantidad de resistencia recuperada por segundo mientras el jugador camina.\n\n" +
                "Explicación Técnica:\nFloat en unidades de resistencia por segundo. Se aplica cuando el jugador está caminando (no corriendo ni quieto). Debe ser menor que la regeneración parado para recompensar hacer pausas.\n\n" +
                "Ejemplo de Uso:\nSi la regeneración estando quieto es de 30 y caminando es de 15, caminar recupera resistencia pero a la mitad de velocidad. Esto da al jugador una opción intermedia: no tiene que parar completamente para recuperarse.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov caminar lento permite recuperar algo de stamina, creando un ciclo de gestión muy orgánico: correr, camina para recuperar, correr de nuevo cuando sea necesario. Esto da mucho ritmo.",
                "Base Explanation:\nAmount of stamina recovered per second while the player is walking.\n\n" +
                "Technical Explanation:\nFloat in stamina units per second. Applied when the player is walking (not sprinting, not idle). Should be less than the idle regeneration to reward taking breaks.\n\n" +
                "Usage Example:\nIf idle regeneration is 30 and walking is 15, walking recovers stamina but at half speed. This gives the player an intermediate option: they don't have to stop completely to recover.\n\n" +
                "General Example:\nIn Escape from Tarkov walking slowly allows recovering some stamina, creating a very organic management cycle: sprint, walk to recover, sprint again when necessary. This gives a lot of rhythm.", 0f, 248f);

            y = SubHeader(y, ancho, ES ? "— HUD" : "— HUD");
            y = CampoBool(y, ancho + 20, "ShowTheStaminaBarOnTheHUD",
                "Mostrar La Barra De Resistencia En El HUD", "Show The Stamina Bar On The HUD",
                "Explicación Base:\nDefine si la barra de resistencia se muestra en el HUD durante el juego.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. El GameObject que contiene la barra de resistencia se activa o desactiva según este valor. El propio GameObject debe estar asignado en el inspector del PlayerController_ControladorDelJugador, no en esta Window Inspector.\n\n" +
                "Ejemplo de Uso:\nOcultar la barra puede ser parte de un diseño de HUD minimalista donde el jugador aprende a gestionar la resistencia por feedback visual del personaje (animaciones, efectos de respiración) en lugar de una barra explícita.\n\n" +
                "Ejemplo General:\nHalo Infinite no muestra una barra de resistencia, en cambio el jugador aprende intuitivamente la duración por puro uso repetido.",
                "Base Explanation:\nDefines whether the stamina bar is shown on the HUD during gameplay.\n\n" +
                "Technical Explanation:\nBool variable. The GameObject containing the stamina bar is activated or deactivated based on this value. The GameObject itself must be assigned in the PlayerController_ControladorDelJugador inspector, not in this Window.\n\n" +
                "Usage Example:\nHiding the bar can be part of a minimalist HUD design where the player learns to manage stamina through visual character feedback (animations, breathing effects) rather than an explicit bar.\n\n" +
                "General Example:\nHalo Infinite does not show a sprint stamina bar; players learn the duration intuitively through repeated use.");
            return y;
        }

        private float DibujarS06(float y, float ancho)
        {
            y = SubHeader(y, ancho, ES ? "— Deslizamiento" : "— Sliding");
            y = CampoBool(y, ancho + 20, "EnableTheSlidingSystem",
                "Activar El Sistema De Deslizamiento", "Enable The Sliding System",
                "Explicación Base:\nInterruptor maestro del sistema de deslizamiento.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el input de deslizamiento no tiene ningún efecto y el sistema está completamente desactivado, independientemente de la configuración del resto de parámetros de esta sección.\n\n" +
                "Ejemplo de Uso:\nDesactivar el deslizamiento simplifica el sistema de movimiento. Es una buena opción para juegos de terror o aventura donde deslizarse rompería la inmersión o el ritmo narrativo.\n\n" +
                "Ejemplo General:\nDeslizarse es una mecánica que se popularizó mucho mucho con Titanfall 2, donde al combinarlo con el bunny hopping y el wall-running se creó un sistema de movimiento extremadamente fluido. Posteriormente se adoptó en Apex Legends, Warzone y muchos otros FPS.",
                "Base Explanation:\nMaster switch for the sliding system.\n\n" +
                "Technical Explanation:\nBool variable. If False, the slide input has no effect and the slide system is completely disabled, regardless of the configuration of the rest of the parameters in this section.\n\n" +
                "Usage Example:\nDisabling slide simplifies the movement system. It is a good option for horror or adventure games where slide would break immersion or narrative pacing.\n\n" +
                "General Example:\nSlide is a mechanic that became very popular with Titanfall 2, where combining it with bunny hopping and wall-running creates an extremely fluid movement system. It was later adopted in Apex Legends, Warzone and many other FPS games.");
            y = CampoBool(y, ancho + 20, "UseRealPhysicsInSliding",
                "Usar Fisica Real En El Deslizamiento", "Use Real Physics In The Sliding",
                "Explicación Base:\nDefine si el deslizamiento usa una simulación física real o un modo arcade con una distancia fija.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. En modo False (arcade), el slide tiene una duración y distancia definidas por los parámetros del modo arcade. En modo True (físico), la velocidad del deslizamiento decae por fricción y puede acelerarse en pendientes, creando comportamientos emergentes.\n\n" +
                "Ejemplo de Uso:\nEl modo arcade (False) es más fácil de diseñar y más predecible para el jugador: siempre sabe cuánto recorre. El modo físico (True) es más inmersivo y variado pero requiere más calibración.\n\n" +
                "Ejemplo General:\nTitanfall 2 y Apex Legends usan un sistema de deslizamiento que se siente más físico que arcade, acelerando en rampas y desacelerando en superficies planas. COD usa un sistema más arcade con una duración fija para mayor consistencia competitiva.",
                "Base Explanation:\nDefines whether sliding uses real physics simulation or a fixed-distance arcade mode.\n\n" +
                "Technical Explanation:\nBool variable. In False mode (arcade), slide has fixed duration and distance defined by arcade mode parameters. In True mode (physical), slide speed decays through friction and can accelerate on slopes, creating emergent behavior.\n\n" +
                "Usage Example:\nArcade mode (False) is easier to design and more predictable for the player: they always know how far they slide. Physical mode (True) is more immersive and varied but requires more calibration.\n\n" +
                "General Example:\nTitanfall 2 and Apex Legends use a slide system that feels more physics-based than arcade, accelerating on ramps and decelerating on flat surfaces. Call of Duty uses a more arcade system with fixed duration for competitive consistency.");
            y = CampoBool(y, ancho + 20, "AllowJumpingDuringSliding",
                "Permitir Saltar Durante El Deslizamiento", "Allow Jumping During The Slide",
                "Explicación Base:\nPermite que el jugador salte mientras está deslizándose, cancelando el deslizamiento por el impulso del salto.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True y el jugador pulsa el salto mientras se desliza, se cancela y el jugador salta con la velocidad actual del deslizamiento añadida. Esto permite el 'slide jump', una de las maniobras de movimiento avanzado más satisfactorias.\n\n" +
                "Ejemplo de Uso:\nEl 'slide jump' es fundamental en juegos de movimiento avanzado. Permite mantener la inercia del deslizamiento y convertirla en un salto de largo alcance. En juegos competitivos puede ser una herramienta táctica importante para cruzar espacios rápidamente.\n\n" +
                "Ejemplo General:\nEn Titanfall 2 el 'slide jump' es parte central de la filosofía de movimiento del juego, permitiendo encadenar 'deslizarse-saltar-correr en pared' para mantener velocidad indefinidamente. En Apex Legends también es una técnica muy usada.",
                "Base Explanation:\nAllows the player to jump while sliding, canceling the slide with momentum.\n\n" +
                "Technical Explanation:\nBool variable. If True and the player presses jump during an active slide, the slide is canceled and the player jumps with the current slide speed added. This enables the 'slide jump', one of the most satisfying advanced movement maneuvers.\n\n" +
                "Usage Example:\nSlide jump is fundamental in advanced movement games. It allows maintaining the slide's momentum and converting it into a long-range jump. In competitive games it can be an important tactical tool for crossing spaces quickly.\n\n" +
                "General Example:\nIn Titanfall 2 the slide jump is central to the game's movement philosophy, allowing chaining slide-jump-wallrun to maintain speed indefinitely. In Apex Legends it is also a widely used technique.");
            y = CampoBool(y, ancho + 20, "AllowInterruptingSliding",
                "Permitir Interrumpir El Deslizamiento", "Allow Interrupting The Slide",
                "Explicación Base:\nPermite cancelar el deslizamiento pulsando de nuevo el input de agacharse.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, durante el deslizamiento, el jugador puede pulsar el input para agachado para cancelar el deslizamiento y pasar al estado agachado o de pie. Si está en False, el deslizamiento debe completarse o interrumpirse con un salto.\n\n" +
                "Ejemplo de Uso:\nPermitir la interrupción general(True) da más control al jugador pero puede ser explotada para hacer 'cancel-slide' evitando el tiempo de recuperación. Prohibirla (False) hace el deslizarse algo mas comprometedor.\n\n" +
                "Ejemplo General:\nLa mayoría de FPS modernos permiten cancelarlo, pero algunos juegos tácticos lo hacen irrevocable para añadir consecuencias a la decisión de deslizarse en mal momento.",
                "Base Explanation:\nAllows canceling the slide by pressing the crouch input again.\n\n" +
                "Technical Explanation:\nBool variable. If True, during an active slide the player can press the crouch input to cancel the slide and transition to crouch or standing state. If False, the slide must complete or end with a jump.\n\n" +
                "Usage Example:\nAllowing interruption (True) gives the player more control but can be exploited to cancel the slide just before it ends to avoid recovery time. Prohibiting it (False) makes the slide more committed.\n\n" +
                "General Example:\nMost modern FPS games with slide allow canceling it, but some tactical games make it irrevocable to add consequences to the decision of sliding at the wrong moment.");
            y = CampoFloat(y, ancho, "RecoveryTimeAfterSliding",
                "Tiempo De Recuperacion Tras El Deslizamiento", "Recovery Time After The Slide",
                "Explicación Base:\nTiempo en segundos que el jugador permanece forzosamente agachado después de terminar el deslizamiento.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Una vez se termina el deslizamiento (por tiempo, velocidad mínima o colisión), el jugador queda 'bloqueado' en el estado agachado durante este tiempo antes de poder incorporarse. Esto es la 'penalización del deslizamiento'.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.3-0.5 es un equilibrio entre consecuencia y jugabilidad. Sin tiempo de recuperación (0), deslizarse se convierte en una herramienta de evasión perfecta sin ningún coste. Con 0.8 segundos, el jugador queda vulnerable brevemente al terminar.\n\n" +
                "Ejemplo General:\nEn Apex Legends deslizarse no tiene un tiempo de recuperación explícito porque el sistema de movimiento está diseñado para encadenar maniobras. En Warzone hay una breve animación de recuperación que limita las acciones inmediatas.",
                "Base Explanation:\nTime in seconds the player is forced to remain in crouch after the slide ends.\n\n" +
                "Technical Explanation:\nFloat in seconds. Once the slide ends (by time, minimum speed or collision), the player is locked in crouch state for this time before being able to stand up. This is the 'slide penalty'.\n\n" +
                "Usage Example:\nA value of 0.3-0.5 is a balance between consequence and playability. Without recovery time (0), slide becomes a perfect evasion tool with no cost. With 0.8 seconds, the player is briefly vulnerable after ending.\n\n" +
                "General Example:\nIn Apex Legends slide has no explicit recovery time because the movement system is designed to chain maneuvers. In Warzone there is a brief recovery animation that limits immediate post-slide actions.", 0f, 48f);
            y = CampoBool(y, ancho + 20, "AllowRunningImmediatelyAfterSliding",
                "Permitir Correr Inmediatamente Tras El Deslizamiento", "Allow Running Immediately After The Slide",
                "Explicación Base:\nDefine si el jugador puede directamente correr al terminar un deslizamiento.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, al terminar de deslizarse el jugador puede mantener pulsado el input de correr para comenzar a correr inmediatamente, saltándose el estado de pie 'estático'. Si está en False, debe pasar por el estado de pie antes de poder correr.\n\n" +
                "Ejemplo de Uso:\nActivar esto (True) permite un flujo de movimiento más dinámico: 'deslizarse-correr-deslizarse' encadenado. Desactivarlo (False) añade una pequeña pausa obligatoria entre acciones, haciendo el movimiento menos fluido pero más legible.\n\n" +
                "Ejemplo General:\nTitanfall 2 permite esta cadena de movimientos sin interrupciones, siendo uno de los juegos más fluidos del género. En juegos más tácticos como Rainbow Six Siege, la transición entre acciones de movimiento tiene pausas implícitas.",
                "Base Explanation:\nDefines whether the player can immediately transition to sprint at the end of the slide.\n\n" +
                "Technical Explanation:\nBool variable. If True, when the slide ends the player can hold the sprint input to start running immediately, bypassing the 'static' standing state. If False, they must go through the standing state before being able to run.\n\n" +
                "Usage Example:\nEnabling this (True) allows more dynamic movement flow: chained slide-sprint-slide. Disabling it (False) adds a mandatory small pause between actions, making movement less fluid but more readable.\n\n" +
                "General Example:\nTitanfall 2 allows this chain of movements without interruptions, being one of the most fluid games in the genre. In more tactical games like Rainbow Six Siege, transitions between movement actions have implicit pauses.");
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostWhenSliding", "StaminaCostWhenSliding",
                "Coste De Resistencia Al Deslizarse", "Stamina Cost When Sliding",
                "Explicación Base:\nCoste de resistencia al deslizarse.\n\n" +
                "Explicación Técnica:\nFloat que se descuenta en el frame de inicio del deslizamiento. Como deslizarse es una maniobra que explota la inercia generada al correr, su coste puede ser mayor que el del salto para limitar el abuso continuo como método de desplazamiento.\n\n" +
                "Ejemplo de Uso:\nUn coste de 20-25 con una resistencia de 100 permite 4-5 deslizamientos consecutivos teniendo la resistencia llena. Combinado con el costeo por correr, gestionar la resistencia se vuelve algo muy orgánico.\n\n" +
                "Ejemplo General:\nPocos FPS mainstream implementan un coste de resistencia explícito a la hora de deslizarse, pero en juegos con sistemas de resistencia detallados como Escape from Tarkov, el deslizamiento consumiría una cantidad significativa de resistencia.",
                "Base Explanation:\nInstant stamina cost when initiating a slide.\n\n" +
                "Technical Explanation:\nFloat deducted in the slide start frame. As slide is a maneuver that exploits sprint inertia, its cost can be higher than jumping to limit the abuse of continuous sliding as a movement method.\n\n" +
                "Usage Example:\nA cost of 20-25 with stamina of 100 allows 4-5 consecutive slides from full stamina. Combined with the sprint cost, the stamina management gameplay becomes very organic.\n\n" +
                "General Example:\nFew mainstream FPS implement an explicit stamina cost for slide, but in games with detailed stamina systems like Escape from Tarkov, sliding would consume a significant amount of physical stamina.", 0f, 148f);
            y = CampoBool(y, ancho + 20, "AllowSlidingOnAnySurface",
                "Permitir El Deslizamiento En Cualquier Superficie", "Allow Sliding On Any Surface",
                "Explicación Base:\nDefine si deslizarse puede iniciarse en cualquier superficie o solo en pendientes.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, deslizarse puede iniciarse en suelo plano, rampas y cualquier superficie. Si está en False, el slide solo puede iniciarse cuando el jugador está en una superficie con inclinación suficiente, añadiendo una condición física al movimiento.\n\n" +
                "Ejemplo de Uso:\nRestringuir el deslizamiento a pendientes (False) hace que sea exclusivamente una mecánica de descenso rápido y aprovechamiento de la gravedad, mucho más simulacionista. Permitirlo en cualquier superficie (True) lo hace una herramienta de movilidad general.\n\n" +
                "Ejemplo General:\nEn Apex Legends y Titanfall 2 puedes deslizarte en cualquier superficie, siendo una herramienta de movilidad general. En juegos más realistas solo tendría sentido en superficies inclinadas.",
                "Base Explanation:\nDefines whether slide can be initiated on any surface or only on slopes.\n\n" +
                "Technical Explanation:\nBool variable. If True, slide can be initiated on flat ground, ramps and any surface. If False, slide can only be initiated when the player is on a surface with sufficient inclination, adding a physical condition to the movement.\n\n" +
                "Usage Example:\nRestricting slide to slopes (False) makes it exclusively a fast descent and gravity-exploitation mechanic, much more simulationist. Allowing it on any surface (True) makes it a general mobility tool.\n\n" +
                "General Example:\nIn Apex Legends and Titanfall 2 slide can be initiated on any surface, making it a general mobility tool. In more realistic games it would only make sense on inclined surfaces.");

            y = SubHeaderN2(y, ancho, ES ? "· Modo Arcade" : "· Arcade Mode");
            y = CampoFloat(y, ancho, "DurationOfSlidingInArcadeMode",
                "Duracion Del Deslizamiento En Modo Arcade", "Slide Duration In Arcade Mode",
                "Explicación Base:\nDuración fija en segundos del deslizamiento en modo arcade.\n\n" +
                "Explicación Técnica:\nFloat en segundos. En modo arcade ('Usar Fisica Real En El Deslizamiento' = False), el deslizamiento dura exactamente este tiempo independientemente de la velocidad, superficie o pendiente. Es la duración máxima; si la velocidad cae al mínimo antes, el deslizamiento también termina.\n\n" +
                "Ejemplo de Uso:\nUna duración de 0.6-0.8 segundos es el rango más común en FPS. Por debajo de 0.4 se vuelve muy breve y puntual. Por encima de 1.0 puede sentirse que el jugador 'se aleja' demasiado con cada deslizada.\n\n" +
                "Ejemplo General:\nEn COD MW 2, el deslizamiento dura aproximadamente 0.6 segundos en terreno plano, suficiente para cruzar un pasillo o esquivar bajo una barrera, pero no tan largo como para que se sienta absurdo.",
                "Base Explanation:\nFixed duration in seconds of the slide in arcade mode.\n\n" +
                "Technical Explanation:\nFloat in seconds. In arcade mode ('Use Real Physics In The Sliding' = False), the slide lasts exactly this time regardless of speed, surface or slope. It is the maximum duration; if speed drops to minimum before this, the slide also ends.\n\n" +
                "Usage Example:\nA duration of 0.6-0.8 seconds is the most common range in FPS. Below 0.4 the slide is very brief and punctual. Above 1.0 it can feel like the player 'escapes' too far with each slide.\n\n" +
                "General Example:\nIn Call of Duty Modern Warfare 2 (2022), slide lasts approximately 0.6 seconds on flat terrain, enough to cross a corridor or dodge under a barrier, but not so long that it feels exploitable.", 0.1f, 48f);
            y = CampoFloat(y, ancho, "InitialSpeedMultiplierOfArcadeSliding",
                "Multiplicador De Velocidad Inicial Del Deslizamiento Arcade", "Initial Speed Multiplier Of The Arcade Slide",
                "Explicación Base:\nMultiplicador sobre la velocidad base que define la velocidad inicial del deslizamiento en modo arcade.\n\n" +
                "Explicación Técnica:\nFloat. La velocidad inicial del deslizamiento es 'Velocidad Base Del Jugador u/s' × 'Multiplicador De la Velocidad Base Al Correr' × este multiplicador. La velocidad inicial ya incluye el multiplicador de correr.\n\n" +
                "Ejemplo de Uso:\nUn valor de 1.5 significa que el deslizamiento comienza al 150% de la velocidad al correr, dando ese impulso inicial que hace el deslizamiento se sienta satisfactorio. Un valor de 1.0 hace que se inicie el deslizamiento sin impulso adicional.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS el deslizamiento tiene un 'boost' de velocidad inicial que justifica usarlo en lugar de simplemente agacharse. Este boost es parte de lo que hace el deslizarse una mecánica de movimiento ofensivo y no solo defensivo.",
                "Base Explanation:\nMultiplier over base speed that defines the initial speed of slide in arcade mode.\n\n" +
                "Technical Explanation:\nFloat. The initial slide speed is 'Player Base Speed u/s' × 'Base Speed Multiplier When Running' × this multiplier. The initial speed already includes the sprint multiplier.\n\n" +
                "Usage Example:\nA value of 1.5 means the slide starts at 150% of sprint speed, giving that initial boost that makes the slide satisfying. A value of 1.0 starts the slide without additional boost.\n\n" +
                "General Example:\nIn most FPS games slide has an initial speed 'boost' that justifies using it instead of simply crouching. This boost is part of what makes slide an offensive movement mechanic and not just defensive.", 1f, 10f);
            y = CampoFloat(y, ancho, "MinimumSpeedToKeepSliding",
                "Velocidad Minima Para Mantenerse Deslizando", "Minimum Speed To Keep Sliding",
                "Explicación Base:\nVelocidad mínima en unidades Unity por segundo por debajo de la cual el deslizamiento termina automáticamente.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity por segundo. En modo arcade, la velocidad al deslizarse decrece durante la duración. Si en algún momento la velocidad cae por debajo de este umbral, el deslizamiento termina aunque no haya expirado el tiempo. Previene deslizamientos a velocidad ultra-lenta que se verían absurdos.\n\n" +
                "Ejemplo de Uso:\nUn valor de 2.0 significa que si durante el deslizamiento el jugador desacelera hasta 2 u/s (por ejemplo en una subida pronunciada), se cancela automáticamente. Debe ser menor que la velocidad base que se le da a caminar para no cancelarse al instante.\n\n" +
                "Ejemplo General:\nEste umbral de velocidad mínima es especialmente relevante cuando el deslizamiento físico está activo, ya que en pendientes contrarias, la desaceleración puede ser muy rápida.",
                "Base Explanation:\nMinimum speed in Unity units per second below which the slide ends automatically.\n\n" +
                "Technical Explanation:\nFloat in Unity units per second. In arcade mode, slide speed decreases during the duration. If at any point speed falls below this threshold, the slide ends even if the time has not expired. Prevents ultra-slow slides that would look absurd.\n\n" +
                "Usage Example:\nA value of 2.0 means if the slide decelerates to 2 u/s (for example on a steep uphill), it is automatically canceled. It must be less than the base walking speed to avoid instant cancellation.\n\n" +
                "General Example:\nThis minimum speed threshold is especially relevant when physical slide is active, since on opposing slopes deceleration can be very rapid.", 0.1f, 48f);

            y = SubHeaderN2(y, ancho, ES ? "· Modo Físico" : "· Physics Mode");
            y = CampoFloat(y, ancho, "FrictionDuringPhysicalSliding",
                "Friccion Durante El Deslizamiento Fisico", "Friction During The Physics Slide",
                "Explicación Base:\nCoeficiente de fricción aplicado al deslizamiento en el modo físico.\n\n" +
                "Explicación Técnica:\nFloat. En el modo físico ('Usar Fisica Real En El Deslizamiento' = True), la velocidad al deslizarse decrece a esta tasa por segundo. Un valor de 2.0 significa que la velocidad pierde 2 unidades por segundo por la fricción. El deslizamiento también interactúa con la pendiente del terreno.\n\n" +
                "Ejemplo de Uso:\nUna fricción de 1.5 en terreno plano da un deslizamiento moderado que termina en unos 2-3 segundos desde que inicia. En una pendiente positiva la fricción efectiva aumenta, en una pendiente negativa disminuye.\n\n" +
                "Ejemplo General:\nLos motores de juego como Source (Valve) modelan la fricción al deslizarse de forma física para crear comportamientos emergentes como el 'bhop' ('bunny hopping') donde reducir la fricción al saltar mantiene la velocidad acumulada.",
                "Base Explanation:\nFriction coefficient applied during slide in physical mode.\n\n" +
                "Technical Explanation:\nFloat. In physical mode ('Use Real Physics In The Sliding' = True), slide speed decreases at this rate per second. A value of 2.0 means speed loses 2 units per second due to friction. The slide also interacts with terrain slope.\n\n" +
                "Usage Example:\nA friction of 1.5 on flat terrain gives a moderate slide that ends in about 2-3 seconds from sprint speed. On positive slope effective friction increases, on negative slope it decreases.\n\n" +
                "General Example:\nGame engines like Source (Valve) model slide friction physically to create emergent behaviors like bhop (bunny hopping) where reducing friction when jumping maintains accumulated speed.", 0f, 48f);
            y = CampoFloat(y, ancho, "SlopeAccelerationMultiplierDuringSliding",
                "Multiplicador De Aceleracion En Pendiente Durante El Deslizamiento", "Slope Acceleration Multiplier During The Slide",
                "Explicación Base:\nMultiplicador de la aceleración extra que gana el deslizamiento en cuesta abajo.\n\n" +
                "Explicación Técnica:\nFloat. En modo físico, cuando el jugador se desliza en una pendiente hacia abajo, se añade una aceleración adicional proporcional al ángulo de pendiente multiplicada por este valor. Permite que al deslizarse se gane velocidad en descensos.\n\n" +
                "Ejemplo de Uso:\nUn valor de 2.0 hace que deslizarse en una rampa de 30 grados haga que el jugador se acelere notablemente, recompensandolo por usar el terreno a su favor. Un valor de 0 elimina completamente la aceleración por pendiente.\n\n" +
                "Ejemplo General:\nEn Apex Legends y Titanfall 2 deslizarse cuesta abajo te hace gana velocidad notablemente, lo que incentiva a usar el terreno verticalmente y hace los mapas con diferencias de altura mucho más dinámicos.",
                "Base Explanation:\nMultiplier for the extra acceleration that slide gains when sliding downhill.\n\n" +
                "Technical Explanation:\nFloat. In physical mode, when the player slides on a downward slope, additional acceleration proportional to the slope angle multiplied by this value is added. Allows slide to gain speed on descents.\n\n" +
                "Usage Example:\nA value of 2.0 makes a slide on a 30-degree ramp accelerate noticeably, rewarding the player who uses the terrain to their advantage. A value of 0 completely eliminates slope acceleration.\n\n" +
                "General Example:\nIn Apex Legends and Titanfall 2 sliding downhill gains noticeable speed, which incentivizes using the terrain vertically and makes maps with height differences much more dynamic.", 0f, 48f);

            y = SubHeaderN2(y, ancho, ES ? "· Sensibilidad de Cámara" : "· Camera Sensitivity");
            y = CampoBool(y, ancho + 20, "ReduceCameraSensitivityDuringSliding",
                "Reducir La Sensibilidad De La Camara Durante El Deslizamiento", "Reduce Camera Sensitivity During The Slide",
                "Explicación Base:\nReduce la sensibilidad de la cámara mientras el jugador está deslizándose.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, cuando te deslizas la sensibilidad de la cámara se multiplica por 'Multiplicador De Sensibilidad De La Camara Durante El Deslizamiento'. Simula la pérdida de control al querer 'apuntar' a alta velocidad.\n\n" +
                "Ejemplo de Uso:\nActivar esto añade una penalización de apuntado al deslizamiento: si bien te mueves rápido, no puedes apuntar con la misma precisión. Esto equilibra la mecánica al añadir un intercambio claro.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS la sensibilidad no cambia durante el deslizamiento, pero algunos juegos tácticos como Rainbow Six Siege reducen implícitamente la precisión al moverse a alta velocidad a través del sistema de dispersión de las armas.",
                "Base Explanation:\nReduces camera sensitivity while the player is sliding.\n\n" +
                "Technical Explanation:\nBool variable. If True, during an active slide the camera sensitivity is multiplied by 'Camera Sensitivity Multiplier During The Slide'. Simulates the loss of fine aiming control at high speed.\n\n" +
                "Usage Example:\nEnabling this adds an aiming penalty to slide: while slide moves you fast, you cannot aim with the same precision. This balances the mechanic by adding a clear trade-off.\n\n" +
                "General Example:\nIn most FPS games sensitivity does not change during slide, but some tactical games like Rainbow Six Siege implicitly reduce precision when moving at high speed through the weapon dispersion system.");
            y = CampoFloat(y, ancho, "CameraSensitivityMultiplierDuringSliding",
                "Multiplicador De Sensibilidad De La Camara Durante El Deslizamiento", "Camera Sensitivity Multiplier During The Slide",
                "Explicación Base:\nFactor por el que se multiplica la sensibilidad de la cámara durante el deslizamiento.\n\n" +
                "Explicación Técnica:\nFloat típicamente entre 0 y 1 (aunque técnicamente puede ser mayor de 1 para aumentar la sensibilidad en lugar de reducirla). Solo tiene efecto si 'Reducir La Sensibilidad De La Camara Durante El Deslizamiento' está en True.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.5 reduce la sensibilidad a la mitad durante el slide. Un valor de 0.7 es una reducción más sutil que apenas se nota pero suaviza los movimientos de la cámara a alta velocidad.\n\n" +
                "Ejemplo General:\nLa reducción de sensibilidad durante movimientos rápidos es también una práctica común en juegos de conducción de vehículos en FPS.",
                "Base Explanation:\nFactor by which camera sensitivity is multiplied during sliding.\n\n" +
                "Technical Explanation:\nFloat between 0 and 1 typically (though technically it can be greater than 1 to increase rather than reduce sensitivity). Only takes effect if 'Reduce Camera Sensitivity During The Slide' is True.\n\n" +
                "Usage Example:\nA value of 0.5 reduces sensitivity to half during slide. A value of 0.7 is a subtler reduction that is barely noticeable but smooths camera movements at high speed.\n\n" +
                "General Example:\nSensitivity reduction during fast movement is also a common practice in FPS vehicle driving games.", 0.1f, 48f, true);

            y = SubHeader(y, ancho, ES ? "— Dash" : "— Dash");
            y = CampoBool(y, ancho + 20, "EnableTheDashSystem",
                "Activar El Sistema De Dash", "Enable The Dash System",
                "Explicación Base:\nInterruptor maestro del sistema de dash.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el input de dash no tiene ningún efecto y el sistema de dash está completamente desactivado.\n\n" +
                "Ejemplo de Uso:\nEl dash es una mecánica de evasión instantánea que añade profundidad táctica. Desactivarlo simplifica el sistema de movimiento para juegos donde el movimiento pausado es parte del diseño.\n\n" +
                "Ejemplo General:\nEl dash en FPS está popularizado por juegos como Dishonored (Guiño), Titanfall 2(Titanes) y Destiny 2 (El del Cazador). En juegos tácticos como Rainbow Six Siege no hay dash para mantener el ritmo pausado del combate.",
                "Base Explanation:\nMaster switch for the dash system.\n\n" +
                "Technical Explanation:\nBool variable. If False, the dash input has no effect and the dash system is completely disabled.\n\n" +
                "Usage Example:\nDash is an instant evasion mechanic that adds tactical depth. Disabling it simplifies the movement system for games where deliberate movement is part of the design.\n\n" +
                "General Example:\nDash in FPS is popularized by games like Dishonored (Blink), Titanfall 2 and Destiny 2 (Hunter Dodge). In tactical games like Rainbow Six Siege there is no dash to maintain the deliberate pace of combat.");
            y = CampoBool(y, ancho + 20, "AllowDashInTheAir",
                "Permitir El Dash En El Aire", "Allow The Dash In The Air",
                "Explicación Base:\nPermite ejecutar el dash mientras el jugador está en el aire.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, el dash puede activarse independientemente de si el jugador está en suelo o en el aire. El cooldown aplica igual.\n\n" +
                "Ejemplo de Uso:\nEl dash aéreo es una herramienta de maniobra avanzada que permite correcciones de trayectoria en el aire, esquivar proyectiles durante saltos y crear combos de movimiento (salto + dash + salto).\n\n" +
                "Ejemplo General:\nEn Titanfall 2 el dash aéreo para los titanes crea posibilidades de movimiento extraordinarias para el combate.",
                "Base Explanation:\nAllows executing the dash while the player is airborne.\n\n" +
                "Technical Explanation:\nBool variable. If True, dash can be activated regardless of whether the player is grounded or airborne. Cooldown applies equally.\n\n" +
                "Usage Example:\nAir dash is an advanced movement tool that allows trajectory corrections in the air, dodging projectiles during jumps and creating movement combos (jump + dash + jump).\n\n" +
                "General Example:\nIn Titanfall 2 the air dash for the Titan creates extraordinary movement possibilities for the combat.");
            y = CampoFloat(y, ancho, "DashForce",
                "Fuerza Del Dash", "Dash Force",
                "Explicación Base:\nFuerza aplicada instantáneamente en la dirección del dash.\n\n" +
                "Explicación Técnica:\nFloat aplicado como velocidad instantánea en la dirección del input o de la cámara en el momento del dash.\n\n" +
                "Ejemplo de Uso:\nUn valor de 15-20 da un dash que cubre aproximadamente 3-4 unidades en la duración del impulso, suficiente para evadir ataques o cruzar espacios pequeños. Valores de 30+ crean teletransportes virtuales.\n\n" +
                "Ejemplo General:\nEn Dishonored el Guiño equivaldría a una fuerza de dash extremadamente alta con duración mínima. En Destiny 2 el Esquive del Cazador es más moderado, moviéndose unos metros en un instante.",
                "Base Explanation:\nForce applied instantly in the direction of the dash.\n\n" +
                "Technical Explanation:\nFloat applied as instant velocity in the direction of the input or camera at the moment of the dash.\n\n" +
                "Usage Example:\nA value of 15-20 gives a dash that covers approximately 3-4 units during the impulse duration, enough to evade attacks or cross small spaces. Values of 30+ create virtual teleportations.\n\n" +
                "General Example:\nIn Dishonored, Blink (teleport) would equate to an extremely high dash force with minimum duration. In Destiny 2, the Hunter's Dodge is more moderate, moving a few meters in an instant.", 1f, 148f);
            y = CampoFloat(y, ancho, "DurationOfTheDashImpulse",
                "Duracion Del Impulso Del Dash", "Dash Impulse Duration",
                "Explicación Base:\nDuración en segundos del impulso activo del dash.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Durante este tiempo el dash aplica su fuerza. Después, el jugador vuelve al movimiento normal. Valores muy bajos crean un dash instantáneo (Parecido a un teletransporte). Valores más altos crean un dash con una trayectoria visible.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.15-0.2 segundos combinado con una fuerza alta crea el dash clásico de los FPS: muy rápido pero con una trayectoria visible. Un valor de 0.05 con una fuerza alta crea algo más parecido a un 'parpadeo' de movimiento.\n\n" +
                "Ejemplo General:\nEn Overwatch el dash de Tracer dura aproximadamente 0.1 segundos, siendo prácticamente instantáneo pero con una pequeña estela visual que lo distingue de una teletransportación.",
                "Base Explanation:\nDuration in seconds of the active dash impulse.\n\n" +
                "Technical Explanation:\nFloat in seconds. During this time the dash applies its force. After, the player returns to normal movement. Very low values create an instantaneous dash (teleport-like). Higher values create a dash with visible trajectory.\n\n" +
                "Usage Example:\nA value of 0.15-0.2 seconds combined with high force creates the classic FPS dash: very fast but with visible trajectory. A value of 0.05 with high force creates something closer to a blink.\n\n" +
                "General Example:\nIn Overwatch, Tracer's dash lasts approximately 0.1 seconds, being practically instantaneous but with a small visual trail that distinguishes it from teleportation.", 0.05f, 48f);
            y = CampoFloat(y, ancho, "CooldownBetweenDashUses",
                "Cooldown Entre Uso Del Dash", "Cooldown Between Dash Uses",
                "Explicación Base:\nTiempo de espera en segundos entre un dash y el siguiente.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Después de ejecutar un dash, el sistema bloquea el input del dash durante este tiempo. Previene el dash continuo como método principal de desplazamiento.\n\n" +
                "Ejemplo de Uso:\nUn cooldown de 1.5-2 segundos hace que el dash sea una herramienta táctica puntual y no de uso continuo. Un cooldown de 0.5 permite dashes muy frecuentes que definen completamente el estilo del movimiento.\n\n" +
                "Ejemplo General:\nEn Destiny 2 el cooldown del Esquive del Cazador es de varios segundos, siendo una habilidad de uso estratégico.",
                "Base Explanation:\nWait time in seconds between one dash and the next.\n\n" +
                "Technical Explanation:\nFloat in seconds. After executing a dash, the system blocks the dash input for this time. Prevents continuous dashing as the primary means of movement.\n\n" +
                "Usage Example:\nA cooldown of 1.5-2 seconds makes dash a punctual tactical tool and not for continuous use. A cooldown of 0.5 allows very frequent dashes that completely define the movement style.\n\n" +
                "General Example:\nIn Destiny 2 the Hunter's Dodge cooldown is several seconds, making it a strategic use ability.", 0f, 48f);
            y = CampoBool(y, ancho + 20, "RequireDoublePressForDashOnGamepad",
                "Requerir Presionar Dos Veces El Input Del Dash En Mando", "Require Double Press For Dash On Gamepad",
                "Explicación Base:\nEn mando, requiere pulsar el input del dash dos veces rápidamente para ejecutar el dash.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, presionar una vez el input del dash no ejecuta el dash. Solo se activa presionandolo dos veces dentro del tiempo definido en 'Tiempo Maximo En El Primer Y Segundo Presionado Para Validar El Dash'. Previene activaciones accidentales del dash en mando.\n\n" +
                "Ejemplo de Uso:\nEn el mando el LB que es el input que puse por default para el dash, se usa frecuentemente para otras acciones y puede activarse accidentalmente. El requisito de presionar 2 veces reduce los falsos positivos a costa de hacer el dash ligeramente más difícil de ejecutar en situaciones de urgencia.\n\n" +
                "Ejemplo General:\nEn muchos juegos que el dash necesite que su input asignado sea presionado dos veces en mando es debido a la falta de opciones de input en el mismo.",
                "Base Explanation:\nOn gamepad, requires pressing the dash input twice quickly to execute the dash.\n\n" +
                "Technical Explanation:\nBool variable. If True, a single press of the dash input does not execute the dash. It only activates with two presses within the time defined in 'Maximum Time Between The Two Presses During The Double Press Dash'. Prevents accidental dash activations on gamepad.\n\n" +
                "Usage Example:\nOn gamepad LB(the input i assigned to dash) is frequently used for other actions and can be accidentally activated. Double press reduces false positives at the cost of making dash slightly harder to execute in urgent situations.\n\n" +
                "General Example:\nDouble tap for dash is a common convention in action games on gamepad.");
            y = CampoFloat(y, ancho, "MaximumTimeBetweenTheTwoPressesForDashDoublePress",
                "Tiempo Maximo En El Primer Y Segundo Presionado Para Validar El Dash", "Maximum Time Between The Two Presses During The Double Press Dash",
                "Explicación Base:\nVentana de tiempo máximo en segundos entre el primer y segundo presionado del input de dash para que se considere válido.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Si la segunda vez que se presiona ocurre dentro de este tiempo desde el primero, se ejecuta el dash. Si tarda más, el primer presionado se descarta y el jugador debe volver a 'empezar'. Solo tiene efecto cuando 'Requerir Presionar Dos Veces El Input Del Dash En Mando' está en True.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.3 segundos es el estándar para un doble tap en videojuegos. Por debajo de 0.2 es demasiado preciso y frustrante. Por encima de 0.5 es tan permisivo que prácticamente cualquier doble presionado accidental lo activa.\n\n" +
                "Ejemplo General:\nLa mayoría de mecánicas de doble tap en juegos usan ventanas de entre 0.2 y 0.4 segundos. Este rango es bien conocido en el diseño porque equilibra accesibilidad con prevención de falsos positivos.",
                "Base Explanation:\nMaximum time window in seconds between the first and second press of the dash button for it to be considered a valid double press.\n\n" +
                "Technical Explanation:\nFloat in seconds. If the second press occurs within this time from the first, the dash executes. If it takes longer, the first press is discarded and the player must start over. Only takes effect when 'Require Double Press For Dash On Gamepad' is True.\n\n" +
                "Usage Example:\nA value of 0.3 seconds is the standard for double tap in video games. Below 0.2 is too precise and frustrating. Above 0.5 is so permissive that virtually any accidental double press activates it.\n\n" +
                "General Example:\nMost double tap mechanics in games use windows of between 0.2 and 0.4 seconds. This range is well known in control design because it balances accessibility with prevention of false positives.", 0.1f, 48f);
            y = CampoBool(y, ancho + 20, "AllowDashDuringSliding",
                "Permitir El Dash Durante El Deslizamiento", "Allow Dash During The Slide",
                "Explicación Base:\nPermite usar el dash mientras el jugador está deslizándose, cancelando obviamente el deslizamiento.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True y el jugador activa el dash durante el deslizamiento, este se cancela y el dash se ejecuta inmediatamente.\n\n" +
                "Ejemplo de Uso:\nActivar esto crea combos de movimiento avanzados: deslizamiento para ganar velocidad y luego dash para conservar esa velocidad en una dirección diferente.\n\n" +
                "Ejemplo General:\nHalo 5: God Guardians, es suficiente explicación con esas bellas palabras.",
                "Base Explanation:\nAllows using the dash while the player is sliding, canceling the slide.\n\n" +
                "Technical Explanation:\nBool variable. If True and the player activates dash during an active slide, the slide is canceled and the dash executes immediately. This enables 'slide-to-dash', an advanced movement maneuver.\n\n" +
                "Usage Example:\nEnabling this creates advanced movement combos: slide to gain speed then dash to carry that speed in a different direction. It is a high skill ceiling mechanic.\n\n" +
                "General Example:\nHalo 5: God Guardians, those beauiful words alone explains it.");
            y = CampoBoolFloat(y, ancho, "EnableStaminaCostWhenUsingDash", "StaminaCostWhenUsingDash",
                "Coste De Resistencia Al Usar El Dash", "Stamina Cost When Using The Dash",
                "Explicación Base:\nCoste de Resistencia al ejecutar un dash.\n\n" +
                "Explicación Técnica:\nFloat que se descuenta a la resistencia en el frame de inicio del dash. Con el cooldown de dash ya existe una limitación natural de uso, pero añadir coste de resistencia crea una segunda restricción que enriquece la gestión de recursos.\n\n" +
                "Ejemplo de Uso:\nUn coste de 25 con una resistencia max de 100, permite 4 dashes consecutivos teniendo la resistencia llena. Combinado con el cooldown, el jugador gestiona dos recursos al mismo tiempo.\n\n" +
                "Ejemplo General:\nEn Destiny 2 el Esquive del Cazador no tiene un coste de resistencia explícito sino un cooldown de recarga progresiva similar a el thruster de Halo 5. Combinar cooldown Y el coste de resistencia es una opción de diseño más compleja que enriquece la toma de decisiones.",
                "Base Explanation:\nInstant stamina cost when executing a dash.\n\n" +
                "Technical Explanation:\nFloat deducted in the dash start frame. With the dash cooldown there is already a natural usage limitation, but adding a stamina cost creates a second restriction that enriches resource management.\n\n" +
                "Usage Example:\nA cost of 25 with stamina of 100 allows 4 consecutive dashes from full stamina. Combined with the cooldown, the player manages two resources simultaneously.\n\n" +
                "General Example:\nIn Destiny 2 the Hunter's Dodge has no explicit stamina cost but a progressive recharge cooldown similar to the thruster of Halo 5. Combining cooldown AND stamina cost is a more complex design option that enriches decision-making.", 0f, 148f);
            return y;
        }

        private float DibujarS07(float y, float ancho)
        {
            y = CampoBool(y, ancho + 20, "EnableTheObjectInteractionSystem",
                "Activar El Sistema De Interaccion De Objetos", "Enable The Object Interaction System",
                "Explicación Base:\nInterruptor maestro del sistema de recoger, rotar y lanzar objetos.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, ningún input de interacción con objetos tiene efecto. El raycast de detección de objetos recogibles tampoco se ejecuta.\n\n" +
                "Ejemplo de Uso:\nDesactivar esto elimina completamente la interacción con los objetos interactuables del mundo. Útil si tu juego no tiene mecánicas de recogida o si quieres desactivarla en ciertas secciones.\n\n" +
                "Ejemplo General:\nJuegos como Half-Life 2 tienen un sistema de interacción de objetos (Gravity Gun) como mecánica central.",
                "Base Explanation:\nMaster switch for the pick up, rotate and throw object system.\n\n" +
                "Technical Explanation:\nBool variable. If False, no object interaction input has any effect. The raycast for detecting pickable objects does not execute.\n\n" +
                "Usage Example:\nDisabling this completely removes world object interaction. Useful if your game has no pickup mechanics or if you want to disable it in certain sections.\n\n" +
                "General Example:\nGames like Half-Life 2 have an object interaction system (Gravity Gun) as a central mechanic.");

            y = CampoBool(y, ancho + 20, "TakeIntoAccountTheMassOfTheObjectWhenThrowingIt",
                "Tener En Cuenta La Masa Del Objeto", "Consider the Object’s Mass",
                "Explicación Base:\nLa masa influye o no a la hora de lanzar el objeto.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, no importa si el objeto en el universo del juego deberia pesar, eso se ignora.\n\n" +
                "Ejemplo de Uso:\nTener este valor en true, significa poder simular con mayor realismo el peso de los objetos en el mundo, en cuanto a la interacción con ellos.\n\n" +
                "Ejemplo General:\nJuegos como Half-Life 2 tienen un sistema de interacción de objetos que hace que el peso importe.",
                "Base Explanation:\nThe object's mass may or may not influence how it is thrown.\n\n" +
                "Technical Explanation:\nBool variable. If set to False, it does not matter whether the object in the game world should have weight; it will be ignored.\n\n" +
                "Usage Example:\nSetting this value to true allows for a more realistic simulation of object weight in the world when interacting with them.\n\n" +
                "General Example:\nGames like Half-Life 2 have an object interaction system where weight matters.");

            y = SubHeader(y, ancho, ES ? "— Recoger y Soltar" : "— Pick Up & Drop");
            y = CampoString(y, ancho, "TagOfPickableObjects",
                "Tag De Los Objetos Recogibles", "Tag Of The Pickable Objects",
                "Explicación Base:\nTag de Unity que deben tener los GameObjects para poder ser recogidos por el jugador.\n\n" +
                "Explicación Técnica:\nString comparado con el tag del objeto detectado por el raycast. Solo los objetos con este tag exacto serán reconocidos como recogibles. El tag obviamente debe existir en el proyecto de Unity (editables en Edit - Project Settings - Tags and Layers).\n\n" +
                "Ejemplo de Uso:\nUsar un tag específico como 'Recogible' o 'Interactuable' es mejor práctica que usar 'Untagged' o tags genéricos. Permite marcar exactamente qué objetos del escenario son recogibles.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 los objetos físicos interactuables con la Gravity Gun están marcados con propiedades específicas que los identifican como manipulables. El sistema de tags de Unity es el equivalente de este sistema de marcado.",
                "Base Explanation:\nUnity tag that GameObjects must have to be pickable by the player.\n\n" +
                "Technical Explanation:\nString compared with the tag of the object detected by the raycast. Only objects with this exact tag will be recognized as pickable. The tag must exist in the Unity project (editable in Edit - Project Settings - Tags and Layers).\n\n" +
                "Usage Example:\nUsing a specific tag like 'Pickable' or 'Interactable' is better practice than using 'Untagged' or generic tags. Allows precisely marking which scene objects are pickable.\n\n" +
                "General Example:\nIn Half-Life 2, physical objects interactable with the Gravity Gun are marked with specific properties that identify them as manipulable. Unity's tag system is the equivalent of this marking system.");
            y = CampoFloat(y, ancho, "MaximumDistanceToPickUpAnObject",
                "Distancia Maxima Para Recoger Un Objeto", "Maximum Distance To Pick Up An Object",
                "Explicación Base:\nDistancia máxima en unidades Unity desde la cámara hasta el objeto para poder recogerlo.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Es el MaxDistance del raycast de interacción. El raycast se lanza desde la cámara en la dirección de visión. Si el primer objeto con tag recogible está a mayor distancia que este valor, no puede recogerse.\n\n" +
                "Ejemplo de Uso:\nUn valor de 2.5-3 unidades es natural para recoger objetos cercanos en primera persona. Valores de 5+ crean una sensación de estar usando 'magia' donde el jugador recoge objetos a distancia sin necesidad de acercarse.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 la distancia de recogida manual sin Gravity Gun es de aproximadamente 1.5 metros (unidades). Con la Gravity Gun básica, puede atraer objetos desde varios metros de distancia.",
                "Base Explanation:\nMaximum distance in Unity units from the camera to the object to be able to pick it up.\n\n" +
                "Technical Explanation:\nFloat in Unity units. It is the MaxDistance of the interaction raycast. The raycast is cast from the camera in the view direction. If the first pickable tagged object is farther than this value, it cannot be picked up.\n\n" +
                "Usage Example:\nA value of 2.5-3 units is natural for picking up nearby objects in first person. Values of 5+ create a 'magic' feeling where the player picks up objects from a distance without needing to approach.\n\n" +
                "General Example:\nIn Half-Life 2, manual pickup distance without Gravity Gun is approximately 1.5 meters (units). With the basic Gravity Gun, it can attract objects from several meters away.", 0.5f, 48f);
            y = CampoBool(y, ancho + 20, "DisableTheObjectColliderWhenPickingItUp",
                "Desactivar El Collider Del Objeto Al Recogerlo", "Disable The Object Collider When Picking It Up",
                "Explicación Base:\nDesactiva el Collider del objeto en el momento de ser recogido.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, al recoger el objeto se llama a Collider.enabled = false. Esto evita que el objeto colisione con el entorno mientras es transportado, previniendo situaciones donde el objeto 'empuja' paredes o al propio jugador.\n\n" +
                "Ejemplo de Uso:\nActivar esto (True) es generalmente más estable y menos propenso a bugs físicos. Si necesitas que el objeto siga colisionando con el entorno mientras se transporta (por ejemplo para usarlo como escudo), desactívalo (False).\n\n" +
                "Ejemplo General:\nEn Half-Life 2, los objetos recogidos con la Gravity Gun mantienen su colisión activa porque son proyectiles potenciales. En juegos con mecánicas de puzzle, desactivar la colisión previene que los objetos traspasen las paredes durante el transporte.",
                "Base Explanation:\nDisables the object's Collider at the moment of being picked up.\n\n" +
                "Technical Explanation:\nBool variable. If True, when picking up the object, Collider.enabled = false is called. This prevents the object from colliding with the environment while being carried, preventing situations where the object 'pushes' walls or the player themselves.\n\n" +
                "Usage Example:\nEnabling this (True) is generally more stable and less prone to physics bugs. If you need the object to continue colliding with the environment while transported (for example to use as a shield), disable it (False).\n\n" +
                "General Example:\nIn Half-Life 2, objects picked up with the Gravity Gun maintain their collision active because they are potential projectiles. In puzzle games, disabling collision prevents objects from passing through walls during transport.");
            y = CampoFloat(y, ancho, "SpeedOfTheObjectMovementTowardsTheAnchorPoint",
                "Velocidad De Movimiento Del Objeto Hacia El Punto De Anclaje", "Object Movement Speed Towards The Anchor Point",
                "Explicación Base:\nVelocidad del Lerp que mueve el objeto desde su posición actual hasta el punto de anclaje frente al jugador.\n\n" +
                "Explicación Técnica:\nFloat que controla el Vector3.Lerp de posición del objeto hacia el punto de anclaje. Valores altos hacen que el objeto 'vuele' instantáneamente a la posición de carga. Valores bajos crean una atracción más gradual y física.\n\n" +
                "Ejemplo de Uso:\nUn valor de 10 da una atracción rápida pero con una pequeña inercia visible. Un valor de 20 la hace casi instantánea. Valores de 3-5 crean una atracción lenta y deliberada, ideal para puzzles donde ver el objeto moverse tiene valor narrativo.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 con la Gravity Gun, los objetos son atraídos hacia el jugador con una velocidad que varía según el tipo de objeto y la distancia, creando una sensación de peso diferente según lo que se recoge.",
                "Base Explanation:\nSpeed of the Lerp that moves the object from its current position to the anchor point in front of the player.\n\n" +
                "Technical Explanation:\nFloat that controls the Vector3.Lerp of the object's position towards the anchor point. High values make the object 'fly' instantaneously to the carry position. Low values create a more gradual and physical attraction.\n\n" +
                "Usage Example:\nA value of 10 gives fast attraction but with a small visible inertia. A value of 20 makes it nearly instantaneous. Values of 3-5 create a slow and deliberate attraction, ideal for puzzles where seeing the object move has narrative value.\n\n" +
                "General Example:\nIn Half-Life 2 with the Gravity Gun, objects are attracted toward the player at a speed that varies depending on the type of object and distance, creating a different sense of weight depending on what is picked up.", 10f, 48f);

            y = SubHeader(y, ancho, ES ? "— Rotación del Objeto" : "— Object Rotation");
            y = CampoBool(y, ancho + 20, "HoldToRotateTheObject",
                "Mantener El Input Presionado Para Rotar El Objeto", "Hold Input To Rotate The Object",
                "Explicación Base:\nDefine si el modo de rotación de objeto se activa manteniendo pulsado el input o si es como una palanca.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. En modo True, el jugador solo puede rotar el objeto mientras mantenga presionado el input de rotación. En modo False, presionar una vez el input activa el modo de rotación y presionarlo otra vez lo desactiva.\n\n" +
                "Ejemplo de Uso:\nEl modo mantener (True) es más intuitivo: el jugador presiona el input, rota, lo suelta y vuelve al control normal. El modo palanca (False) libera la mano del jugador para rotar con mayor comodidad durante ajustes largos.\n\n" +
                "Ejemplo General:\nEn juegos de puzzles como The Witness, donde rotar objetos es una mecánica central, el modo palanca puede ser más cómodo para inspecciones prolongadas. En FPS de 'acción/horror', el modo mantener es más natural.",
                "Base Explanation:\nDefines whether object rotation mode activates by holding the key or as a toggle.\n\n" +
                "Technical Explanation:\nBool variable. In True mode, the player can only rotate the object while holding the rotation key. In False mode, one press activates rotation mode and another press deactivates it.\n\n" +
                "Usage Example:\nHold mode (True) is more intuitive: the player presses the key, rotates, releases it and returns to normal control. Toggle (False) frees the player's hand for more comfortable rotation during long adjustments.\n\n" +
                "General Example:\nIn puzzle games like The Witness, where rotating objects is a central mechanic, toggle can be more comfortable for prolonged inspection. In 'action/horror' FPS, hold is more natural.");
            y = CampoFloat(y, ancho, "RotationSpeedOfTheObjectInHand",
                "Velocidad De Rotacion Del Objeto En Mano", "Rotation Speed Of The Object In Hand",
                "Explicación Base:\nVelocidad de rotación en grados por segundo al rotar un objeto sostenido.\n\n" +
                "Explicación Técnica:\nFloat en grados por segundo. Se multiplica por Time.deltaTime y el input de rotación para calcular la rotación aplicada al objeto en cada frame.\n\n" +
                "Ejemplo de Uso:\nUn valor de 90 grados por segundo da una vuelta completa en 4 segundos con un input continuo. Para inspecciones detalladas de objetos, valores bajos (30-60) dan más control. Para ajustes rápidos, valores altos (180-360) son más eficientes.\n\n" +
                "Ejemplo General:\nEn juegos de puzzles o aventuras donde inspeccionar objetos es parte del gameplay, como Resident Evil 7 donde se rotan objetos para encontrar puzzles ocultos, la velocidad de rotación está cuidadosamente calibrada para ser lenta y controlada.",
                "Base Explanation:\nRotation speed in degrees per second when rotating a held object.\n\n" +
                "Technical Explanation:\nFloat in degrees per second. Multiplied by Time.deltaTime and the rotation input to calculate the rotation applied to the object each frame.\n\n" +
                "Usage Example:\nA value of 90 degrees per second gives a full rotation in 4 seconds with continuous input. For detailed object inspection, low values (30-60) give more control. For quick adjustments, high values (180-360) are more efficient.\n\n" +
                "General Example:\nIn puzzle or adventure games where inspecting objects is part of gameplay, like Resident Evil 7 where objects are rotated to find hidden puzzles, the rotation speed is carefully calibrated to be slow and controlled.", 10f, 480f);

            y = SubHeader(y, ancho, ES ? "— Lanzar Objeto" : "— Throw Object");
            y = CampoFloat(y, ancho, "MinimumObjectThrowForce",
                "Fuerza Minima Del Lanzamiento Del Objeto", "Minimum Force Of The Object Throw",
                "Explicación Base:\nFuerza de lanzamiento aplicada cuando el jugador lanza un objeto sin cargarlo\n\n" +
                "Explicación Técnica:\nFloat. Es la fuerza de lanzamiento cuando el input de lanzamiento se pulsa brevemente sin mantenerlo. Se aplica como un AddForce en el Rigidbody del objeto y en la dirección de la cámara.\n\n" +
                "Ejemplo de Uso:\nUn valor de 5 lanza el objeto suavemente hacia adelante. Valores de 2-3 crean más un 'soltar hacia adelante' que un lanzamiento. Valores de 8+ crean un lanzamiento perceptiblemente poderoso incluso sin cargarlo.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 lanzar objetos sin cargar con la Gravity Gun los expulsa con suficiente fuerza para derribar enemigos. El sistema de carga-lanzamiento de este juego es probablemente el más conocido del género.",
                "Base Explanation:\nLaunch force applied when the player throws the object without charging.\n\n" +
                "Technical Explanation:\nFloat. It is the throw force when the throw input is pressed briefly without holding. Applied as AddForce on the object's Rigidbody in the camera direction.\n\n" +
                "Usage Example:\nA value of 5 launches the object gently forward. Values of 2-3 create more of a 'forward drop' than a throw. Values of 8+ create a noticeably powerful throw even without charging.\n\n" +
                "General Example:\nIn Half-Life 2, throwing objects without charging with the Gravity Gun expels them with enough force to knock down enemies. This game's charge-throw system is probably the most well known in the genre.", 0.5f, 48f);
            y = CampoFloat(y, ancho, "MaximumObjectThrowForce",
                "Fuerza Maxima Del Lanzamiento Del Objeto", "Maximum Force Of The Object Throw",
                "Explicación Base:\nFuerza máxima de lanzamiento alcanzada al cargar completamente el tiempo de carga.\n\n" +
                "Explicación Técnica:\nFloat. Es la fuerza aplicada cuando el jugador mantiene el input el tiempo máximo de carga definido por 'Tiempo Maximo De Carga Del Lanzamiento Del Objeto'. La fuerza real es un Lerp entre la mínima y la máxima según el porcentaje de carga.\n\n" +
                "Ejemplo de Uso:\nUna relación de 5:20 (mínima:máxima) da un rango de lanzamiento que va desde suave hasta potente. Para un juego de puzzle donde los objetos deben alcanzar puntos exactos, este rango permite la precisión necesaria.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 con la Gravity Gun cargada al máximo, los objetos pueden matar enemigos directamente. Este dualismo de fuerza variable hace la mecánica de lanzamiento versátil tanto para puzzles como para combate.",
                "Base Explanation:\nMaximum throw force reached when fully charging the charge time.\n\n" +
                "Technical Explanation:\nFloat. It is the force applied when the player holds the input for the maximum charge time defined by 'Maximum Charge Time Of The Object Throw'. The actual force is a Lerp between minimum and maximum based on charge percentage.\n\n" +
                "Usage Example:\nA ratio of 5:20 (min:max) gives a throw range that goes from gentle to powerful. For a puzzle game where objects must reach exact points, this range allows the necessary precision.\n\n" +
                "General Example:\nIn Half-Life 2 with the Gravity Gun fully charged, objects can kill enemies directly. This variable force dualism makes the throw mechanic versatile for both puzzles and combat.", 1f, 148f);
            y = CampoFloat(y, ancho, "MaximumChargeTimeOfTheObjectThrow",
                "Tiempo Maximo De Carga Del Lanzamiento Del Objeto", "Maximum Charge Time Of The Object Throw",
                "Explicación Base:\nTiempo en segundos que se tarda en alcanzar la fuerza máxima de lanzamiento.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Mientras el jugador mantiene pulsado el input de lanzamiento, el porcentaje de carga aumenta de 0 a 1 a lo largo de este tiempo. Al soltar el input, el objeto se lanza con la fuerza correspondiente al porcentaje alcanzado.\n\n" +
                "Ejemplo de Uso:\nUn tiempo de carga de 0.8 segundos crea un sistema de lanzamiento reactivo: el jugador puede calcular cuánto carga mentalmente. Un tiempo de 2 segundos hace el lanzamiento máximo un recurso más deliberado que requiere planificación.\n\n" +
                "Ejemplo General:\nEn Portal 2, aunque el sistema de portales no usa carga, la mecánica de 'agarrar y lanzar cubos' tiene un timing muy específico que los jugadores aprenden a dominar. Un buen sistema de lanzamiento tiene que ser suficientemente intuitivo para no interrumpir el flujo del juego.",
                "Base Explanation:\nTime in seconds to reach maximum throw force.\n\n" +
                "Technical Explanation:\nFloat in seconds. While the player holds the throw input, the charge percentage increases from 0 to 1 over this time. Releasing the input throws the object with the force corresponding to the percentage reached.\n\n" +
                "Usage Example:\nA charge time of 0.8 seconds creates a reactive throw system: the player can mentally calculate how much to charge. A time of 2 seconds makes maximum throw a more deliberate resource requiring planning.\n\n" +
                "General Example:\nIn Portal 2, although the portal system doesn't use charging, the mechanic of 'grabbing and throwing cubes' has a very specific timing that players learn to master. A good throw system must be intuitive enough not to interrupt game flow.", 0.1f, 48f);
            return y;
        }

        private float DibujarS08(float y, float ancho)
        {
            y = CampoBool(y, ancho + 20, "EnableTheZoomSystem",
                "Activar El Sistema De Zoom", "Enable The Zoom System",
                "Explicación Base:\nInterruptor maestro del sistema de zoom.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el input de zoom no tiene ningún efecto. El FOV se mantiene siempre en el valor base y no se aplica ningún multiplicador de sensibilidad por zoom.\n\n" +
                "Ejemplo de Uso:\nDesactivar el zoom simplifica el sistema de apuntado. Es una buena opción para juegos donde no hay sistema de miras o donde el zoom es innecesario por el diseño de los niveles.\n\n" +
                "Ejemplo General:\nEl Zoom es una mecánica estándar en prácticamente todos los FPS modernos. Antes  por ejemplo, juegos como Quake usaban exclusivamente el disparo de cadera.",
                "Base Explanation:\nMaster switch for the zoom system.\n\n" +
                "Technical Explanation:\nBool variable. If False, the zoom input has no effect. FOV always stays at the base value and no zoom sensitivity multiplier is applied.\n\n" +
                "Usage Example:\nDisabling zoom simplifies the aiming system. It is a good option for games without a sights system or where zoom is unnecessary due to level design.\n\n" +
                "General Example:\nThe Zoom is a standard mechanic in virtually all modern FPS. Before that, games like Quake used exclusively hipfire.");
            y = CampoBool(y, ancho + 20, "HoldToZoom",
                "Mantener El Input Presionado Para El Zoom", "Hold Input For The Zoom",
                "Explicación Base:\nDefine si el zoom se activa manteniendo pulsado el input o como una palanca.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. En modo True, el jugador está en zoom siempre y cuando mantenga pulsado el input. En modo False, presionar una vez activa el zoom y presionar otra vez lo desactiva.\n\n" +
                "Ejemplo de Uso:\nEl modo mantener (True) es el estándar en FPS: el jugador entra y sale del Zoom manteniendo o soltando el input. El modo palanca (False) es más cómodo para francotiradores donde se permanece en zoom durante períodos largos.\n\n" +
                "Ejemplo General:\nSniper Elite y Battlefield usan el modo mantener por defecto pero igual ofrecen el modo palanca como una opción de accesibilidad..",
                "Base Explanation:\nDefines whether zoom activates by holding the key or as a toggle.\n\n" +
                "Technical Explanation:\nBool variable. In True mode, the player is in zoom only while holding the input. In False mode, one press activates zoom and another press deactivates it.\n\n" +
                "Usage Example:\nHold mode (True) is the standard in action FPS: the player enters and exits ADS by holding or releasing the button. Toggle mode (False) is more comfortable for snipers where zoom is maintained for long periods.\n\n" +
                "General Example:\nSniper Elite and Battlefield use hold mode by default but offer toggle as an accessibility option.");
            y = CampoBool(y, ancho + 20, "AllowZoomWhileHoldingAnObject",
                "Permitir El Zoom Mientras Se Tiene Un Objeto Recogido", "Allow Zoom While Holding A Picked Up Object",
                "Explicación Base:\nPermite usar el zoom mientras el jugador está sosteniendo un objeto.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, el sistema de zoom bloquea su activación mientras el jugador lleve un objeto. Si está en True, ambos sistemas pueden estar activos simultáneamente.\n\n" +
                "Ejemplo de Uso:\nDesactivar esto inhabilita el poder inspeccionar algo de cerca mientras lo mueves. Activarlo (True) da más flexibilidad al jugador pero puede ser visualmente inconsistente.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 como ejemplo un poco meh, lo siento, no puedes usar la Gravity Gun como arma y disparar otra simultáneamente.",
                "Base Explanation:\nAllows using zoom while the player is holding a picked up object.\n\n" +
                "Technical Explanation:\nBool variable. If False, the zoom system blocks its activation while the player is carrying an object. If True, both systems can be active simultaneously.\n\n" +
                "Usage Example:\nEnabling this (True) gives the player more flexibility but can be visually inconsistent.\n\n" +
                "General Example:\nIn Half-Life 2 as a mid example for this, sorry, you cannot use the Gravity Gun and fire another weapon simultaneously.");

            y = SubHeader(y, ancho, ES ? "— Valores del FOV" : "— FOV Values");
            y = CampoFloat(y, ancho, "CameraFieldOfViewDuringZoom",
                "Campo De Vision De La Camara Durante El Zoom", "Camera Field Of View During The Zoom",
                "Explicación Base:\nFOV (Field of View - Campo de Visión) de la cámara cuando el zoom está activo.\n\n" +
                "Explicación Técnica:\nFloat en grados. Debe ser menor que 'Campo De Vision Base De La Camara' para que el zoom reduzca el campo visual (efecto de acercamiento). Un FOV más bajo crea más 'zoom' pero también más distorsión de perspectiva en los bordes.\n\n" +
                "Ejemplo de Uso:\nSi el FOV base es 75 y el FOV de zoom es 45, el zoom equivale aproximadamente a 1.67x de magnificación. Para simular miras de un rifle militar o el zoom de una buena cam (2x-4x), valores de 35-50 grados son apropiados.\n\n" +
                "Ejemplo General:\nEn COD, las miras de hierro bajan el FOV de aproximadamente 75 a 55-60. Con miras de punto rojo son valores similares. Con miras de alto alcance como 4x pueden bajar hasta 25-30 FOV.",
                "Base Explanation:\nCamera FOV (Field of View) when zoom is active.\n\n" +
                "Technical Explanation:\nFloat in degrees. Must be less than 'Camera Base Field Of Vision' for the zoom to reduce the visual field (close-up effect). A lower FOV creates more 'zoom' but also more perspective distortion at the edges.\n\n" +
                "Usage Example:\nIf base FOV is 75 and zoom FOV is 45, the zoom equates to approximately 1.67x magnification. To simulate military rifle scopes (2x-4x), values of 35-50 degrees are appropriate.\n\n" +
                "General Example:\nIn COD, ADS with iron sights lowers FOV from approximately 75 to 55-60. With red dot sights values are similar. With high-range scopes like 4x, they can drop to 25-30 FOV.", 10f, 148f);
            y = CampoFloat(y, ancho, "SpeedOfTheFieldOfViewTransitionDuringZoom",
                "Velocidad De La Transicion Del Campo De Vision Durante El Zoom", "Field Of View Transition Speed During The Zoom",
                "Explicación Base:\nVelocidad del Lerp que transiciona el FOV entre el valor base y el valor del zoom.\n\n" +
                "Explicación Técnica:\nFloat que controla el Mathf.Lerp del FOV de la cámara. Valores altos hacen la transición casi instantánea. Valores bajos crean un zoom gradual y suave.\n\n" +
                "Ejemplo de Uso:\nUn valor de 10-12 da una transición de zoom 'snappy' típica de FPS. Un valor de 4-6 crea un zoom más cinematográfico y deliberado, adecuado para juegos por ejemplo de terror donde el tiempo de hacer zoom tiene peso propio.\n\n" +
                "Ejemplo General:\nEn Battlefield, la transición tiene distintas velocidades dependiendo del arma: las pistolas apuntan más rápido que los rifles de precisión, creando diferencias de manejo entre armas. Una velocidad de transición única puede simplificar esta distinción.",
                "Base Explanation:\nSpeed of the Lerp that transitions the FOV between the base value and the zoom value.\n\n" +
                "Technical Explanation:\nFloat that controls the Mathf.Lerp of the camera's FOV. High values make the transition nearly instantaneous. Low values create a gradual and smooth zoom.\n\n" +
                "Usage Example:\nA value of 10-12 gives a snappy zoom transition typical of action shooters. A value of 4-6 creates a more cinematic and deliberate zoom, suitable for sniper or horror games where ADS time has weight.\n\n" +
                "General Example:\nIn Battlefield, the transition to ADS has different speeds depending on the weapon: pistols do ADS faster than precision rifles, creating handling differences between weapons. A single transition speed can simplify this distinction.", 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Sensibilidad durante el Zoom" : "— Sensitivity During Zoom");
            y = CampoBool(y, ancho + 20, "ReduceSensitivityDuringZoom",
                "Reducir La Sensibilidad De La Camara Durante El Zoom", "Reduce Sensitivity While Zooming",
                "Explicación Base:\nReduce la sensibilidad de la cámara mientras el zoom está activo.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True, durante el zoom la sensibilidad horizontal y vertical de la cámara se multiplican por 'Multiplicador De La Sensibilidad De la Cámara Durante El Zoom'. Simula la dificultad de apuntar con precisión a mayor magnificación.\n\n" +
                "Ejemplo de Uso:\nEn juegos competitivos, reducir la sensibilidad durante el apuntado es fundamental. La mayoría de jugadores ya calibran su sensibilidad de apuntado independientemente de su sensibilidad normal para mantener consistencia muscular.\n\n" +
                "Ejemplo General:\nTodos los FPS competitivos modernos como Call of Duty, Valorant y Apex Legends tienen esta opción. Es una de las configuraciones más importantes para jugadores de nivel medio-alto.",
                "Base Explanation:\nReduces mouse sensitivity while zoom is active.\n\n" +
                "Technical Explanation:\nBool variable. If True, during zoom the horizontal and vertical mouse sensitivity are multiplied by 'Camera Sensitivity Multiplier During The Zoom'. Simulates the greater difficulty of aiming precisely at higher magnification.\n\n" +
                "Usage Example:\nIn competitive games, reducing sensitivity during ADS is essential. Most players already calibrate their ADS sensitivity independently of their hipfire sensitivity to maintain muscle consistency.\n\n" +
                "General Example:\nAll modern competitive FPS like COD, Valorant and Apex Legends have this option. It is one of the most important settings for mid-to-high level players.");
            y = CampoFloat(y, ancho, "SensitivityMultiplierDuringZoom",
                "Multiplicador De La Sensibilidad De la Cámara Durante El Zoom", "Camera Sensitivity Multiplier During The Zoom",
                "Explicación Base:\nFactor por el que se multiplica la sensibilidad de la cámara durante el zoom.\n\n" +
                "Explicación Técnica:\nFloat típicamente entre 0.05 y 1. Solo tiene efecto si 'Reducir La Sensibilidad De La Camara Durante El Zoom' está en True. Un valor de 1.0 no cambia nada. Un valor de 0.5 da la mitad de sensibilidad durante el zoom.\n\n" +
                "Ejemplo de Uso:\nEl ratio más estudiado en FPS competitivo es el que mantiene el mismo 'cm/360°' tanto en cadera como en apuntado. Esto se calcula como: sensibilidad en Zoom = sensibilidad Base × (FOV zoom / FOV base). Para un FOV base de 75 y un FOV zoom de 45, el multiplicador sería 0.6.\n\n" +
                "Ejemplo General:\nEn Valorant, la sensibilidad de apuntado puede configurarse independientemente.",
                "Base Explanation:\nFactor by which mouse sensitivity is multiplied during zoom.\n\n" +
                "Technical Explanation:\nFloat typically between 0.05 and 1. Only takes effect if 'Reduce Sensitivity While Zooming' is True. A value of 1.0 changes nothing. A value of 0.5 gives half sensitivity during ADS.\n\n" +
                "Usage Example:\nThe most studied ratio in competitive FPS is the one that maintains the same 'cm/360°' both in hipfire and ADS. This is calculated as: ADSsensitivity = baseSensitivity × (zoomFOV / baseFOV). For base FOV 75 and zoom FOV 45, the multiplier would be 0.6.\n\n" +
                "General Example:\nIn Valorant, ADS sensitivity can be configured independently.", 0.05f, 48f, true);
            return y;
        }

        private float DibujarS09(float y, float ancho)
        {
            y = CampoFloat(y, ancho, "BaseFieldOfViewOfTheCamera",
                "Campo De Vision Base De La Camara", "Camera Base Field Of Vision",
                "Explicación Base:\nFOV base de la cámara en grados durante el gameplay normal.\n\n" +
                "Explicación Técnica:\nFloat en grados. Es el valor de Camera.fieldOfView en condiciones normales (sin zoom, sin efectos de velocidad). El FOV del zoom y todos los efectos de FOV dinámicos parten de este valor como referencia principal.\n\n" +
                "Ejemplo de Uso:\nEntre 60 y 90 grados es el rango estándar para FPS en un monitor a 16:9. Un FOV bajo (60) es más cinemático pero puede generar mareos en algunos jugadores. Un FOV alto (90+) da más conciencia situacional pero puede distorsionar la perspectiva.\n\n" +
                "Ejemplo General:\nHalo Infinite usa 78 FOV por defecto. En consola el estándar suele ser 75-80.",
                "Base Explanation:\nBase camera FOV in degrees during normal gameplay.\n\n" +
                "Technical Explanation:\nFloat in degrees. It is the Camera.fieldOfView value under normal conditions (no zoom, no speed effects). The zoom FOV and all dynamic FOV effects use this value as a reference.\n\n" +
                "Usage Example:\nBetween 60 and 90 degrees is the standard range for FPS on a 16:9 monitor. A low FOV (60) is more cinematic but can cause motion sickness in some players. A high FOV (90+) gives more situational awareness but can distort perspective.\n\n" +
                "General Example:\nHalo Infinite uses 78 FOV by default. On console the standard is usually 75-80.", 10f, 148f);

            y = SubHeader(y, ancho, ES ? "— Velocidad En El FOV" : "— Speed On FOV");
            y = CampoBool(y, ancho + 20, "EnableSpeedFOVEffect",
                "Activar El Aumento De FOV Por Velocidad", "Enable The Speed FOV Effect",
                "Explicación Base:\nActiva un efecto que aumenta el campo de visión (FOV) de la cámara conforme el jugador se mueva más rápido, dando una sensación de velocidad.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en True y el sistema de zoom no está activo, se calcula la velocidad horizontal del Rigidbody del jugador y se aplica un InverseLerp entre el umbral de inicio y el máximo de velocidad para obtener un ratio que escala el FOV base mediante un Lerp hacia el multiplicador máximo configurado.\n\n" +
                "Ejemplo de Uso:\nIdeal para juegos donde el sprint o la velocidad alta deben sentirse impactantes visualmente. Desactivarlo es útil si se busca una cámara más sobria o si el juego no tiene variación significativa de velocidad.\n\n" +
                "Ejemplo General:\nEn Minecraft (Java Edition), al activar el sprint el FOV aumenta automáticamente en 20 unidades de forma nativa, sin mods, como mecánica de base del juego.",
                "Base Explanation:\nActivates an effect that increases the camera field of view (FOV) as the player moves faster, giving a sense of speed.\n\n" +
                "Technical Explanation:\nBool variable. If True and the zoom system is not active, the player Rigidbody's horizontal speed is calculated and an InverseLerp is applied between the start threshold and the speed maximum to obtain a ratio that scales the base FOV via a Lerp toward the configured maximum multiplier.\n\n" +
                "Usage Example:\nIdeal for games where sprinting or high speed should feel visually impactful. Disabling it is useful if a more sober camera is desired or if the game has no significant speed variation.\n\n" +
                "General Example:\nIn Minecraft (Java Edition), activating sprint automatically increases the FOV by 20 units natively, without mods, as a base game mechanic.");
            y = CampoFloat(y, ancho, "SpeedFOVStartPercent",
                "Porcentaje De Velocidad Para Iniciar El FOV", "Speed Percent To Start The FOV",
                "Explicación Base:\nPorcentaje de la velocidad base del jugador a partir del cual comienza a aumentar el FOV por velocidad.\n\n" +
                "Explicación Técnica:\nFloat multiplicador sobre BaseSpeedOfThePlayer. Se usa como límite inferior en un InverseLerp: cuando la velocidad horizontal del Rigidbody supera (BaseSpeed * este valor), el efecto de FOV empieza a interpolarse. Un valor de 0.8 significa que el efecto inicia al 80% de la velocidad base.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.8 hace que el efecto arranque justo antes de llegar a la velocidad normal de caminar, por lo que casi siempre estará activo levemente. Un valor de 1.0 o superior lo reserva exclusivamente para el sprint, manteniéndolo más discreto.\n\n" +
                "Ejemplo General:\nEn Minecraft el efecto de FOV se activa en el momento exacto en que el jugador entra en estado de sprint, no hay transición gradual desde velocidad baja, sino un umbral binario (caminar/sprint). Esto es equivalente a un StartPercent de 1.0, reservando el efecto exclusivamente para la velocidad máxima de movimiento.",
                "Base Explanation:\nPercentage of the player's base speed from which the speed FOV effect starts increasing.\n\n" +
                "Technical Explanation:\nFloat multiplier over BaseSpeedOfThePlayer. Used as the lower bound in an InverseLerp: when the Rigidbody's horizontal speed exceeds (BaseSpeed * this value), the FOV effect starts interpolating. A value of 0.8 means the effect begins at 80% of base speed.\n\n" +
                "Usage Example:\nA value of 0.8 makes the effect start just before reaching normal walking speed, so it will almost always be slightly active. A value of 1.0 or higher reserves it exclusively for sprinting, keeping it more subtle.\n\n" +
                "General Example:\nIn Minecraft the FOV effect activates at the exact moment the player enters the sprint state, there is no gradual transition from low speed, but a binary threshold (walking/sprinting). This is equivalent to a StartPercent of 1.0, reserving the effect exclusively for maximum movement speed.", 0f, 48f, true);
            y = CampoFloat(y, ancho, "SpeedFOVMaxPercent",
                "Porcentaje De Velocidad Para El FOV Maximo", "Speed Percent For Maximum FOV",
                "Explicación Base:\nPorcentaje de la velocidad base del jugador en el que el efecto de FOV alcanza su valor máximo.\n\n" +
                "Explicación Técnica:\nFloat multiplicador sobre BaseSpeedOfThePlayer. Se usa como límite superior en el InverseLerp: cuando la velocidad horizontal alcanza (BaseSpeed * este valor), el ratio es 1 y el FOV llega a su máximo (BaseFieldOfView * SpeedFOVMaxMultiplier). Debe ser mayor que SpeedFOVStartPercent.\n\n" +
                "Ejemplo de Uso:\nCon StartPercent en 0.8 y MaxPercent en 1.6, el efecto se despliega progresivamente a lo largo de toda la gama de velocidades, desde caminar hasta el sprint máximo. Un rango estrecho entre ambos valores crea una transición más brusca.\n\n" +
                "Ejemplo General:\nWitchfire (The Astronauts), aumenta el FOV al correr y al hacer dash. El efecto alcanza su máximo al llegar a la velocidad punta de movimiento, siendo perceptible pero no distorsionador, lo que corresponde a un rango de StartPercent a MaxPercent relativamente estrecho y bien controlado.",
                "Base Explanation:\nPercentage of the player's base speed at which the FOV effect reaches its maximum value.\n\n" +
                "Technical Explanation:\nFloat multiplier over BaseSpeedOfThePlayer. Used as the upper bound in the InverseLerp: when horizontal speed reaches (BaseSpeed * this value), the ratio is 1 and FOV reaches its maximum (BaseFieldOfView * SpeedFOVMaxMultiplier). Must be greater than SpeedFOVStartPercent.\n\n" +
                "Usage Example:\nWith StartPercent at 0.8 and MaxPercent at 1.6, the effect unfolds progressively across the full speed range from walking to maximum sprint. A narrow range between both values creates a more abrupt transition.\n\n" +
                "General Example:\nWitchfire (The Astronauts), increases the FOV when running and dashing. The effect reaches its maximum upon reaching peak movement speed, being noticeable but not distorting, which corresponds to a relatively narrow and well controlled StartPercent to MaxPercent range.", 0f, 48f, true);
            y = CampoFloat(y, ancho, "SpeedFOVMaxMultiplier",
                "Multiplicador Maximo Del FOV Por Velocidad", "Maximum FOV Multiplier By Speed",
                "Explicación Base:\nMultiplicador que se aplica sobre el FOV base cuando el jugador alcanza la velocidad máxima configurada.\n\n" +
                "Explicación Técnica:\nFloat multiplicador. El FOV objetivo en velocidad máxima es BaseFieldOfView * este valor. Se interpola mediante Lerp(1f, SpeedFOVMaxMultiplier, speedRatio), por lo que en velocidad mínima del umbral el multiplicador efectivo es 1 (sin cambio) y en velocidad máxima es este valor.\n\n" +
                "Ejemplo de Uso:\nUn valor de 1.10-1.15 produce un ensanchamiento sutil pero perceptible. Un valor de 1.25 o superior genera un efecto dramático que puede resultar incómodo en sesiones largas. Se recomienda mantenerse en el rango 1.05-1.20 para la mayoría de juegos.\n\n" +
                "Ejemplo General:\nEn Minecraft el multiplicador efectivo al hacer sprint es de aproximadamente 1.22 sobre el FOV base (el FOV sube 20 unidades sobre el valor configurado por el jugador). Esto encaja en el rango recomendado de 1.10–1.20 y demuestra que incluso en un juego de ritmo moderado, un multiplicador de esa magnitud es perceptible y satisfactorio sin resultar incómodo.",
                "Base Explanation:\nMultiplier applied to the base FOV when the player reaches the configured maximum speed.\n\n" +
                "Technical Explanation:\nFloat multiplier. The target FOV at maximum speed is BaseFieldOfView * this value. It is interpolated via Lerp(1f, SpeedFOVMaxMultiplier, speedRatio), so at the threshold minimum speed the effective multiplier is 1 (no change) and at maximum speed it is this value.\n\n" +
                "Usage Example:\nA value of 1.10-1.15 produces subtle but perceptible widening. A value of 1.25 or higher generates a dramatic effect that can be uncomfortable over long sessions. It is recommended to stay in the 1.05-1.20 range for most games.\n\n" +
                "General Example:\nIn Minecraft the effective multiplier when sprinting is approximately 1.22 over the base FOV (the FOV increases by 20 units over the value configured by the player). This fits within the recommended range of 1.10–1.20 and demonstrates that even in a moderate-paced game, a multiplier of that magnitude is noticeable and satisfying without being uncomfortable.", 1f, 48f);
            y = CampoFloat(y, ancho, "SpeedFOVTransitionSpeed",
                "Velocidad De Transicion Del FOV Por Velocidad", "Speed FOV Transition Speed",
                "Explicación Base:\nVelocidad con la que el FOV se adapta a los cambios de velocidad del jugador.\n\n" +
                "Explicación Técnica:\nFloat usado como factor en un Mathf.Lerp por Time.deltaTime: _currentFOV = Lerp(_currentFOV, targetFov, Time.deltaTime * transitionSpeed). Valores altos hacen la transición casi instantánea; valores bajos crean un retardo suave. Este valor reemplaza al SpeedOfTheFieldOfViewTransitionDuringZoom cuando el efecto de velocidad está activo.\n\n" +
                "Ejemplo de Uso:\nUn valor de 4-6 crea una respuesta ágil que sigue bien los cambios de velocidad sin ser brusca. Un valor de 2-3 añade inercia visual agradable. Valores por encima de 10 hacen la transición prácticamente inmediata, perdiendo la sensación de aceleración gradual.\n\n" +
                "Ejemplo General:\nWitchfire aplica una transición suave al cambiar el FOV durante el sprint y el dash, según sus propios desarrolladores. La transición es lo suficientemente rápida para responder al input sin ser instantánea, evitando el salto brusco de FOV que resultaría de un valor de transitionSpeed muy alto.",
                "Base Explanation:\nSpeed at which the FOV adapts to changes in the player's movement speed.\n\n" +
                "Technical Explanation:\nFloat used as a factor in a Mathf.Lerp per Time.deltaTime: _currentFOV = Lerp(_currentFOV, targetFov, Time.deltaTime * transitionSpeed). High values make the transition nearly instant; low values create a smooth delay. This value replaces SpeedOfTheFieldOfViewTransitionDuringZoom when the speed effect is active.\n\n" +
                "Usage Example:\nA value of 4-6 creates an agile response that follows speed changes well without being abrupt. A value of 2-3 adds pleasant visual inertia. Values above 10 make the transition practically immediate, losing the gradual acceleration feel.\n\n" +
                "General Example:\nWitchfire applies a smooth transition when changing the FOV during sprint and dash, according to its own developers. The transition is fast enough to respond to input without being instantaneous, avoiding the abrupt FOV jump that would result from a very high transitionSpeed value.", 0.1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Sensibilidad" : "— Sensitivity");
            y = CampoFloat(y, ancho, "HorizontalMouseSensitivity",
                "Sensibilidad Horizontal De La Camara", "Horizontal Sensitivity",
                "Explicación Base:\nMultiplicador de la velocidad de rotación horizontal de la cámara.\n\n" +
                "Explicación Técnica:\nFloat que se multiplica por Input.GetAxis('X') para calcular la rotación horizontal del jugador en cada frame. Un valor mayor resulta en movimientos de cámara más rápidos con el mismo movimiento físico del ratón/joystick.\n\n" +
                "Ejemplo de Uso:\nHablando de mouse, la sensibilidad 'correcta' depende del DPI del ratón del jugador. Con un ratón a 800 DPI, una sensibilidad de 2.0 puede sentirse muy lenta. Con 3200 DPI la misma sensibilidad puede sentirse muy rápida. Siempre debe ser configurable por el jugador.\n\n" +
                "Ejemplo General:\nEn juegos competitivos como Counter-Strike o Valorant, los jugadores suelen usar sensibilidades equivalentes a 400-800 eDPI (DPI × sensibilidad del juego), que resulta en aproximadamente 30-50 cm para un giro de 360 grados en el ratón.",
                "Base Explanation:\nMultiplier for camera horizontal rotation speed.\n\n" +
                "Technical Explanation:\nFloat multiplied by Input.GetAxis('X') to calculate the player's horizontal rotation each frame. A higher value results in faster camera movements with the same physical mouse/joystick movement.\n\n" +
                "Usage Example:\nTalking about mouse, the 'correct' sensitivity depends on the player's mouse DPI. With an 800 DPI mouse, a sensitivity of 2.0 can feel very slow. With 3200 DPI the same sensitivity can feel fast. It should always be player-configurable.\n\n" +
                "General Example:\nIn competitive games like Counter-Strike or Valorant, players typically use sensitivities equivalent to 400-800 eDPI (DPI × ingame sensitivity), resulting in approximately 30-50 cm for a 360-degree turn with the mouse.", 0.1f, 48f);
            y = CampoFloat(y, ancho, "VerticalMouseSensitivity",
                "Sensibilidad Vertical De La Camara", "Vertical Sensitivity",
                "Explicación Base:\nMultiplicador de la velocidad de rotación vertical de la cámara.\n\n" +
                "Explicación Técnica:\nFloat que se multiplica por Input.GetAxis('Y') para calcular la rotación vertical de la cámara en cada frame. Normalmente se invierte el eje Y (para que mirar hacia arriba requiera mover el ratón/joystick hacia arriba) pero esto igual depende de la configuración del Input Manager de Unity.\n\n" +
                "Ejemplo de Uso:\nMuchos juegos permiten configurar sensibilidad H y V independientemente. Tener la misma sensibilidad en ambos ejes es lo más natural para la mayoría de jugadores. Algunos prefieren la vertical ligeramente más baja para mayor precisión vertical en combate.\n\n" +
                "Ejemplo General:\nEn Counter-Strike existe una única sensibilidad que aplica igual a horizontal y vertical, lo que obliga a un ratio 1:1. En juegos como Apex Legends se pueden configurar independientemente, aunque la recomendación general es mantenerlas iguales.",
                "Base Explanation:\nMultiplier for camera vertical rotation speed.\n\n" +
                "Technical Explanation:\nFloat multiplied by Input.GetAxis('Y') to calculate the camera's vertical rotation each frame. Normally the Y axis is inverted (looking up requires moving the mouse up) but this also depends on Unity's Input Manager configuration.\n\n" +
                "Usage Example:\nMany games allow configuring H and V sensitivity independently. Having the same sensitivity on both axes is most natural for most players. Some prefer vertical slightly lower for greater vertical precision in combat.\n\n" +
                "General Example:\nIn Counter-Strike there is a single sensitivity that applies equally to horizontal and vertical, forcing a 1:1 ratio. In games like Apex Legends they can be configured independently, although the general recommendation is to keep them equal.", 0.1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Límites Verticales" : "— Vertical Clamp");
            y = CampoFloat(y, ancho, "UpperVerticalLimitOfTheCamera",
                "Limite Vertical Superior De La Camara", "Camera Upper Vertical Limit",
                "Explicación Base:\nÁngulo máximo en grados al que la cámara puede girar hacia arriba.\n\n" +
                "Explicación Técnica:\nFloat en grados. La rotación vertical de la cámara se limita entre -'Limite Vertical Inferior De La Camara' y +'Limite Vertical Superior De La Camara'. Un valor de 90 permite mirar completamente hacia el cielo (90 grados arriba del horizonte). Este clamp previene que la cámara 'voltee' al superar los 90 grados.\n\n" +
                "Ejemplo de Uso:\nUn límite de 85-88 grados es más natural que 90 exactos porque previene la sensación de 'bloqueo' al llegar al límite. Valores de 70-75 crean un juego con menos conciencia vertical, útil en juegos donde los enemigos no están en el cielo.\n\n" +
                "Ejemplo General:\nLa mayoría de FPS usan un límite de 89-90 grados hacia arriba. En juegos con mecánicas verticales como Titanfall 2 o Apex Legends donde los enemigos pueden estar sobre ti, el límite máximo de 90 es fundamental.",
                "Base Explanation:\nMaximum angle in degrees the camera can rotate upward.\n\n" +
                "Technical Explanation:\nFloat in degrees. Camera vertical rotation is clamped between -'Camera Lower Vertical Limit' and +'Camera Upper Vertical Limit'. A value of 90 allows looking completely skyward (90 degrees above horizon). This clamp prevents the camera from 'flipping' when exceeding 90 degrees.\n\n" +
                "Usage Example:\nA limit of 85-88 degrees is more natural than exactly 90 because it prevents the 'snapping' feeling when reaching the limit. Values of 70-75 create a game with less vertical awareness, useful in games where enemies are not in the sky.\n\n" +
                "General Example:\nMost FPS games use an upper limit of 89-90 degrees. In games with vertical mechanics like Titanfall 2 or Apex Legends where enemies can be above you, the maximum limit of 90 is essential.", 5f, 90f);
            y = CampoFloat(y, ancho, "LowerVerticalLimitOfTheCamera",
                "Limite Vertical Inferior De La Camara", "Camera Lower Vertical Limit",
                "Explicación Base:\nÁngulo máximo en grados al que la cámara puede girar hacia abajo.\n\n" +
                "Explicación Técnica:\nFloat en grados. El valor se aplica como límite negativo en la rotación vertical. Un valor de 90 permite mirar completamente hacia el suelo. Al igual que el límite superior, se recomienda no usar exactamente 90 para evitar sensación de bloqueo.\n\n" +
                "Ejemplo de Uso:\nUn límite inferior de 80-85 grados permite al jugador mirar casi hacia sus pies, útil para ver objetos en el suelo, enemigos caídos o resolver puzzles que están a los pies del jugador.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2, los jugadores pueden mirar completamente hacia abajo para ver la posición de sus pies al hacer saltos de precisión, aunque en la práctica el límite máximo inferior apenas se usa en combate.",
                "Base Explanation:\nMaximum angle in degrees the camera can rotate downward.\n\n" +
                "Technical Explanation:\nFloat in degrees. The value is applied as a negative limit on vertical rotation. A value of 90 allows looking completely at the ground. As with the upper limit, it is recommended not to use exactly 90 to avoid a snapping feeling.\n\n" +
                "Usage Example:\nA lower limit of 80-85 degrees allows the player to look almost at their feet, useful for seeing objects on the ground, fallen enemies or solving puzzles at the player's feet.\n\n" +
                "General Example:\nIn Counter-Strike 2, players can look completely downward to see their foot position when making precision jumps, although in practice the maximum lower limit is barely used in combat.", 5f, 90f);

            y = SubHeader(y, ancho, ES ? "— Crosshair" : "— Crosshair");
            y = CampoBool(y, ancho + 20, "ShowTheCrosshairOnTheHUD",
                "Mostrar El Crosshair En El HUD", "Show The Crosshair On The HUD",
                "Explicación Base:\nDefine si se muestra el crosshair (el puntito en el centro) en el HUD.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. El GameObject del crosshair se activa o desactiva según este valor. El propio GameObject debe estar asignado en el inspector del PlayerController_ControladorDelJugador. Esta variable solo controla la visibilidad; la posición y el diseño del crosshair se configuran en el UI - Canvas.\n\n" +
                "Ejemplo de Uso:\nOcultar el crosshair aumenta la dificultad de apuntado y la inmersión. Puede usarse para diferenciar dificultades: sin crosshair en nivel difícil, con crosshair en fácil. También es común ocultarlo cuando se apunta porque la mira del arma lo reemplaza.\n\n" +
                "Ejemplo General:\nEn DOOM Eternal el crosshair es parte esencial del gameplay.",
                "Base Explanation:\nDefines whether the crosshair (center sight) is shown on the HUD.\n\n" +
                "Technical Explanation:\nBool variable. The crosshair GameObject is activated or deactivated based on this value. The GameObject itself must be assigned in PlayerController_ControladorDelJugador inspector. This variable only controls visibility; crosshair position and design are configured in the UI component.\n\n" +
                "Usage Example:\nHiding the crosshair increases aiming difficulty and immersion. It can be used to differentiate difficulties: no crosshair on hard difficulty, with crosshair on easy. It is also common to hide it during ADS when the weapon sight replaces it.\n\n" +
                "General Example:\nIn DOOM Eternal the crosshair is an essential part of gameplay.");

            y = SubHeader(y, ancho, ES ? "— Alturas de Cámara por Estado" : "— Camera Heights by State");
            y = CampoFloat(y, ancho, "HeightOfTheCameraSupportWhileStanding",
                "Altura Del Soporte De La Camara Estando De Pie", "Camera Support Height While Standing",
                "Explicación Base:\nPosición Y del soporte de la cámara cuando el jugador está de pie.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Es el valor objetivo del Lerp de posición Y del soporte de la cámara cuando el jugador está en estado de pie. Representa la altura a la que están 'los ojos' del jugador estando de pie.\n\n" +
                "Ejemplo de Uso:\nSi el CapsuleCollider de de pie tiene 1.8 unidades y el pivote está en el suelo, una altura de cámara de 1.65 representa los ojos a 165cm, que es natural para un personaje de 180cm. La diferencia entre altura del collider y la cámara, da espacio para el cuello.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS la cámara en primera persona está a aproximadamente en un 90-92% de la altura total del personaje, representando la posición natural de los ojos en la cabeza.",
                "Base Explanation:\nY position of the camera support when the player is standing.\n\n" +
                "Technical Explanation:\nFloat in Unity units. It is the Y position Lerp target of the camera support when the player is in standing state. Represents the height at which the player's 'eyes' are while standing.\n\n" +
                "Usage Example:\nIf the standing CapsuleCollider is 1.8 units and the pivot is at the ground, a camera height of 1.65 represents eyes at 165cm, which is natural for a 180cm character. The difference between collider height and camera gives space for the neck.\n\n" +
                "General Example:\nIn most FPS games the first-person camera is at approximately 90-92% of the character's total height, representing the natural eye position in the head.", 0.5f, 48f);
            y = CampoFloat(y, ancho, "HeightOfTheCameraSupportWhileCrouching",
                "Altura Del Soporte De Camara Estando Agachado", "Camera Support Height While Crouching",
                "Explicación Base:\nPosición Y del soporte de la cámara cuando el jugador está agachado.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Es el valor objetivo del Lerp de posición Y del soporte de cámara al entrar al estado agachado. Debe ser coherente con la altura del CapsuleCollider en estado agachado.\n\n" +
                "Ejemplo de Uso:\nSi la altura del collider agachado es de 1.0 y la de pie es de 1.8 (osea una reducción del 44%), la cámara estando agachado debería estar en un porcentaje similar de su posición original. Si la cámara de pie está a 1.65, en agachado podría estar a 0.85-0.90.\n\n" +
                "Ejemplo General:\nLa reducción de altura de la cámara al agacharse también reduce la visibilidad del jugador en combate real, siendo parte del valor táctico más allá de sólo reducir el hitbox.",
                "Base Explanation:\nY position of the camera support when the player is crouching.\n\n" +
                "Technical Explanation:\nFloat in Unity units. It is the Y position Lerp target of the camera support when entering the crouch state. Must be consistent with the crouching CapsuleCollider height.\n\n" +
                "Usage Example:\nIf the crouching collider height is 1.0 and standing is 1.8 (44% reduction), the crouching camera should be at a similar percentage of its original position. If the standing camera is at 1.65, crouching could be at 0.85-0.90.\n\n" +
                "General Example:\nThe camera height reduction when crouching also reduces the player's actual combat visibility, being part of the tactical value of crouching beyond just reducing the hitbox.", 0.1f, 48f);
            y = CampoFloat(y, ancho, "HeightOfTheCameraSupportWhileProne",
                "Altura Del Soporte De Camara Estando Acostado", "Camera Support Height While Prone",
                "Explicación Base:\nPosición Y del soporte de la cámara cuando el jugador está acostado.\n\n" +
                "Explicación Técnica:\nFloat en unidades Unity. Es el valor mínimo de altura de la cámara, correspondiente al estado acostado. Valores muy bajos colocan la cámara casi a ras del suelo, creando la perspectiva visual correcta al estar acostado.\n\n" +
                "Ejemplo de Uso:\nCon un collider acostado de 0.4 unidades, una altura de la cámara del 0.25-0.30 coloca la vista muy cerca del suelo, creando una perspectiva dramáticamente diferente que refuerza el sentido de estar acostado en el suelo.\n\n" +
                "Ejemplo General:\nEn Arma 3, la vista al acostarse está muy próxima al suelo, dando una perspectiva que hace el juego de una cobertura extremadamente realista e inmersiva. Esta perspectiva baja es parte de lo que hace el acostarse tan efectivo.",
                "Base Explanation:\nY position of the camera support when the player is prone.\n\n" +
                "Technical Explanation:\nFloat in Unity units. It is the minimum height value of the camera, corresponding to the prone state. Very low values place the camera almost at ground level, creating the correct visual perspective for being prone.\n\n" +
                "Usage Example:\nWith a prone collider of 0.4 units, a camera height of 0.25-0.30 places the view very close to the ground, creating a dramatically different perspective that reinforces the sense of being lying on the ground.\n\n" +
                "General Example:\nIn Arma 3, the prone view is very close to the ground, giving a perspective that makes cover gameplay extremely realistic and immersive. This low perspective is part of what makes prone so effective.", 0.0f, 48f);
            y = CampoFloat(y, ancho, "SpeedOfTheCameraHeightTransition",
                "Velocidad De La Transicion De Altura De La Camara", "Camera Height Transition Speed",
                "Explicación Base:\nVelocidad de interpolación de la altura de la cámara entre estados corporales.\n\n" +
                "Explicación Técnica:\nFloat que controla el Lerp de posición Y del soporte de la cámara entre sus valores objetivo según el estado actual. Valores altos hacen la transición casi instantánea. Valores bajos crean una transición gradual y suave.\n\n" +
                "Ejemplo de Uso:\nUn valor de 10-12 da una transición responsiva que sigue la lógica del collider. Un valor de 5-7 crea una bajada más lenta y cinematográfica. Demasiado lento (valores por debajo de 3) puede desorientar al jugador al haber desincronización entre la colisión y lo que ve.\n\n" +
                "Ejemplo General:\nEn COD MW, la transición de cámara al agacharse es muy rápida y sigue casi instantáneamente al collider, priorizando la responsividad sobre el realismo visual del movimiento.",
                "Base Explanation:\nInterpolation speed of camera height between body states.\n\n" +
                "Technical Explanation:\nFloat that controls the Y position Lerp of the camera support between its target values based on current state. High values make the transition nearly instantaneous. Low values create a gradual and smooth transition.\n\n" +
                "Usage Example:\nA value of 10-12 gives a responsive transition that follows the collider logic. A value of 5-7 creates a slower, more cinematic descent. Too slow (values below 3) can disorient the player due to desynchronization between the collision and what they see.\n\n" +
                "General Example:\nIn COD MW, the camera transition when crouching is very fast and follows the collider almost instantly, prioritizing responsiveness over visual realism of the movement.", 1f, 48f);
            return y;
        }

        private float DibujarS10(float y, float ancho)
        {
            y = CampoBool(y, ancho + 20, "EnableTheHeadBobbingSystem",
                "Activar El Sistema De Balanceo De Cabeza", "Enable The Head Bob System",
                "Explicación Base:\nInterruptor maestro de todo el sistema de balanceo de cabeza.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool. Si está en False, ningún subsistema del balanceo tiene efecto: ni el balanceo al caminar, ni al correr, ni los balanceos reactivos del salto/aterrizaje/agacharse/deslizarse/dash, ni el efecto de respiración. La cámara permanece completamente estática respecto al soporte.\n\n" +
                "Ejemplo de Uso:\nDesactivar el balanceo de cabeza es una opción de accesibilidad importante. El balanceo puede causar mareos por movimiento (motion sickness) en algunos jugadores. Muchos juegos modernos ofrecen esta opción en su menú de accesibilidad(el desactivarlo).\n\n" +
                "Ejemplo General:\nHigh on Life tiene un head bob notable y característico que muchos jugadores recuerdan como parte de su identidad visual.",
                "Base Explanation:\nMaster switch for the entire head bob system.\n\n" +
                "Technical Explanation:\nBool variable. If False, no bob subsystem has any effect: not walking bob, not running bob, not reactive bobs from jump/landing/crouch/slide/dash, not the breathing effect. The camera remains completely static relative to the support.\n\n" +
                "Usage Example:\nDisabling head bob is an important accessibility option. Head bob can cause motion sickness in some players. Many modern games offer this option in their accessibility menu.\n\n" +
                "General Example:\nGames like Cyberpunk 2077 allow completely disabling head bob in accessibility options. High on Life has a notable and characteristic head bob that many players remember as part of its visual identity.");

            y = SubHeader(y, ancho, ES ? "— Al Caminar" : "— While Walking");
            y = CampoBoolFF(y, ancho,
                "EnableHeadBobbingWhileThePlayerWalks",
                "IntensityOfHeadBobbingWhileThePlayerWalks",
                "FrequencyOfHeadBobbingWhileThePlayerWalks",
                "Activar El Balanceo De Cabeza Mientras El Jugador Camina",
                "Enable Head Bob While The Player Walks",
                "Explicación Base:\nActiva el balanceo rítmico de la cámara mientras el jugador camine, con sus parámetros de intensidad y frecuencia.\n\n" +
                "Explicación Técnica:\nVariable de tipo Bool más dos floats. Si está en True, se aplica una oscilación senoidal a la posición de la cámara mientras el jugador camina. La intensidad controla la amplitud de la oscilación (en unidades Unity) y la frecuencia controla qué tan rápido oscila (ciclos por segundo, aproximadamente en Hz).\n\n" +
                "Ejemplo de Uso:\nUn balanceo sutil (intensidad 0.03, frecuencia 1.5) añade 'mucho' pero sin marear. Uno muy pronunciado (intensidad 0.1, frecuencia 2.5) puede evocar el estilo de juegos como Mirror's Edge donde el movimiento corporal es parte de la experiencia.\n\n" +
                "Ejemplo General:\nEl Balanceo y la sincronización entre el audio de los pasos, es lo que hace al movimiento sentirse natural.",
                "Base Explanation:\nActivates the rhythmic camera bob while the player walks, with its intensity and frequency parameters.\n\n" +
                "Technical Explanation:\nBool variable plus two floats. If True, a sinusoidal oscillation is applied to the camera position while the player walks. Intensity controls the oscillation amplitude (in Unity units) and frequency controls how fast it oscillates (cycles per second, approximately in Hz).\n\n" +
                "Usage Example:\nSubtle bob (intensity 0.03, frequency 1.5) adds life without causing nausea. A very pronounced one (intensity 0.1, frequency 2.5) can evoke the style of games like Mirror's Edge where body movement is part of the experience.\n\n" +
                "General Example:\nThe Head bobbing and the synchronization between the footstep audio is what makes the movement feel natural.", "Int.", "Int.", "Frec.", "Freq.", 0f, 48f, 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Al Correr" : "— While Running");
            y = CampoBoolFF(y, ancho,
                "EnableHeadBobbingWhileThePlayerRuns",
                "IntensityOfHeadBobbingWhileThePlayerRuns",
                "FrequencyOfHeadBobbingWhileThePlayerRuns",
                "Activar El Balanceo De Cabeza Mientras El Jugador Corre",
                "Enable Head Bob While The Player Runs",
                "Explicación Base:\nActiva el balanceo rítmico de la cámara mientras el jugador corra, con parámetros independientes del balanceo al caminar.\n\n" +
                "Explicación Técnica:\nIgual al el balanceo de cabeza al caminar pero con valores propios. Generalmente el balanceo de cabeza al correr tiene mayor intensidad y frecuencia que el de caminando, reflejando el mayor esfuerzo físico al correr.\n\n" +
                "Ejemplo de Uso:\nUn balanceo de cabeza al correr debe ser más pronunciado que al caminar (ej: intensidad 0.07 vs 0.03) refuerza visualmente la diferencia entre caminar y correr. Combinado con cambios de FOV por velocidad, crea una sensación de velocidad muy convincente.\n\n" +
                "Ejemplo General:\nEn juegos de immersión como Far Cry, el balanceo al correr es más pronunciado que el de caminar y ayuda a comunicar el estado físico del personaje sin necesidad de usar barras de resistencia visibles.",
                "Base Explanation:\nActivates the rhythmic camera bob while the player runs, with parameters independent of the walking bob.\n\n" +
                "Technical Explanation:\nSame as walking bob but with its own values. Generally sprint bob has greater intensity and frequency than walking bob, reflecting the greater physical effort of sprinting.\n\n" +
                "Usage Example:\nA more pronounced sprint bob than walking bob (e.g., intensity 0.07 vs 0.03) visually reinforces the difference between walking and running. Combined with FOV changes by speed, it creates a very convincing sense of speed.\n\n" +
                "General Example:\nIn immersion games like Far Cry, sprint bob is more pronounced than walking bob and helps communicate the character's physical state without needing visible stamina bars.", "Int.", "Int.", "Frec.", "Freq.", 0f, 48f, 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Reactivo Al Saltar/Aterrizar/Agacharse/Acostarse" : "— Reactive To Jumping/Landing/Crouching/Prone");
            y = CampoBoolFloat(y, ancho,
                "EnableReactiveHeadBobbingWhenJumpingAndLanding",
                "IntensityOfReactiveHeadBobbingWhenJumping",
                "Activar El Balanceo De Cabeza Reactivo Al Saltar y Aterrizar - Intensidad Al Saltar",
                "Enable Reactive Head Bob When Jumping And Landing - Jump Intensity",
                "Explicación Base:\nActiva el sistema de impulso de balanceo en la cámara en el momento de saltar y aterrizar y controla la intensidad de dicho impulso al saltar.\n\n" +
                "Explicación Técnica:\nBool más float. Si está en True, en el frame exacto del salto se aplica un impulso vertical a la cámara de la magnitud indicada. El impulso se suaviza de vuelta a 0 según 'Velocidad De Retorno De Los Balanceos De Cabeza Reactivos A La Posicion Neutral'.\n\n" +
                "Ejemplo de Uso:\nUn impulso de 0.05-0.08 al saltar crea un 'sacudimiento' visible que refuerza el momento del salto. Combinado con el aterrizaje (que tiene su propio parámetro), crea una sensación de peso y física convincente.\n\n" +
                "Ejemplo General:\nEn Titanfall 2, los saltos tienen un feedback visual de cámara muy pronunciado que es parte de lo que hace el movimiento sentirse tan satisfactorio.",
                "Base Explanation:\nEnables the system of an camera bob impulse at the moment of jumping and landing, and controls the intensity of said impulse at jumping.\n\n" +
                "Technical Explanation:\nBool plus float. If True, in the exact frame of the jump a vertical impulse is applied to the camera of the indicated magnitude. The impulse smooths back to 0 according to 'Return Speed Of Reactive Head Bobs To Neutral Position'.\n\n" +
                "Usage Example:\nAn impulse of 0.05-0.08 when jumping creates a visible 'jolt' that reinforces the jump moment. Combined with landing (which has its own parameter), it creates a convincing sense of weight and physics.\n\n" +
                "General Example:\nIn Titanfall 2, jumps have a very pronounced camera visual feedback that is part of what makes movement feel so satisfying.", 0f, 48f);
            y = CampoFloat(y, ancho, "IntensityOfReactiveHeadBobbingWhenLanding",
                "Intensidad Al Aterrizar",
                "Intensity When Landing",
                "Explicación Base:\nIntensidad del impulso de balanceo de la cámara al aterrizar.\n\n" +
                "Explicación Técnica:\nFloat. En el frame del aterrizaje (transición de en al aire a el suelo), se aplica un impulso descendente a la cámara de esta magnitud. El aterrizaje suele tener un impulso mayor que el salto para simular el impacto.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.08-0.12 al aterrizar crea una sensación de impacto convincente. Esto puede complementarse con un audio de aterrizaje y con efectos de partículas para crear un feedback completo al tocar suelo.\n\n" +
                "Ejemplo General:\nEn DOOM Eternal, aterrizar desde grandes alturas produce un impacto visual de cámara muy pronunciado que comunica el peso del Slayer a pesar de su extrema movilidad. Este contraste entre agilidad aérea e impacto en tierra es un diseño intencionado hermoso.",
                "Base Explanation:\nIntensity of the camera bob impulse when landing.\n\n" +
                "Technical Explanation:\nFloat. In the landing frame (transition from airborne to grounded), a downward impulse of this magnitude is applied to the camera. Landing usually has a greater impulse than the jump to simulate the impact.\n\n" +
                "Usage Example:\nA value of 0.08-0.12 when landing creates a convincing impact sensation. This can be complemented with landing audio and particle effects to create complete feedback when touching the ground.\n\n" +
                "General Example:\nIn DOOM Eternal, landing from great heights produces a very pronounced camera impact that communicates the Slayer's weight despite his extreme mobility. This contrast between aerial agility and ground impact is a beautiful intentional design.", 0f, 48f);
            y = CampoBoolFloat(y, ancho,
                "EnableReactiveHeadBobbingWhenCrouching",
                "IntensityOfReactiveHeadBobbingWhenCrouching",
                "Activar El Balanceo De Cabeza Reactivo Al Agacharse - Intensidad",
                "Enable Reactive Head Bob When Crouching - Intensity",
                "Explicación Base:\nActiva y Aplica un impulso de balanceo de la cámara al entrar en el estado agachado.\n\n" +
                "Explicación Técnica:\nBool más float. Si está en True, en el frame de inicio de la transición a agachado se aplica un pequeño impulso descendente a la cámara adicional a la transición de la altura normal.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.03-0.05 añade un micro-impacto al agacharse que refuerza la acción sin ser exagerado. Contribuye a que agacharse se sienta como una acción física real y no solo una reducción de la hitbox.\n\n" +
                "Ejemplo General:\nEn juegos de inmersión de primera persona como Arma 3 o DayZ, el feedback de cámara al cambiar de postura es parte de la experiencia realista.",
                "Base Explanation:\nEnables and Applies a camera bob impulse when entering the crouched state.\n\n" +
                "Technical Explanation:\nBool plus float. If True, in the frame of the crouch transition start, a small downward impulse is applied to the camera in addition to the normal height transition.\n\n" +
                "Usage Example:\nA value of 0.03-0.05 adds a micro-impact when crouching that reinforces the action without being exaggerated. Contributes to crouching feeling like a real physical action and not just a hitbox reduction.\n\n" +
                "General Example:\nIn first-person immersion games like Arma 3 or DayZ, camera feedback when changing stance is part of the realism and weight experience that defines the gameplay loop.", 0f, 48f);
            y = CampoBoolFloat(y, ancho,
                "EnableReactiveHeadBobbingWhenGoingProne",
                "IntensityOfReactiveHeadBobbingWhenGoingProne",
                "Activar El Balanceo De Cabeza Reactivo Al Acostarse - Intensidad",
                "Enable Reactive Head Bob When Going Prone - Intensity",
                "Explicación Base:\nActiva y Aplica un impulso de balanceo de la cámara al entrar en el estado acostado.\n\n" +
                "Explicación Técnica:\nBool más float. Igual que el reactivo del agachado pero para la transición a acostado. El impulso suele ser mayor que el de agacharse al ser una acción más drástica.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.06-0.10 para acostarse (mayor que el de agachado) refuerza visualmente que acostarse es una acción más intensa. El impulso crea la sensación de tirarse al suelo.\n\n" +
                "Ejemplo General:\nEn Arma 3, tirarse al suelo tiene un feedback visual y de sonido muy pronunciado que comunica el peso del equipo del soldado.",
                "Base Explanation:\nEnables and Applies a camera bob impulse when entering the prone state.\n\n" +
                "Technical Explanation:\nBool plus float. Same as the crouch reactive but for the prone transition. The impulse is usually greater than the crouch one as it is a more drastic action.\n\n" +
                "Usage Example:\nA value of 0.06-0.10 for prone (greater than crouch) visually reinforces that going prone is a more intense action. The impulse creates the feeling of throwing yourself to the ground.\n\n" +
                "General Example:\nIn Arma 3, going prone has very pronounced visual and sound feedback that communicates the weight of the soldier's gear.", 0f, 48f);
            y = CampoFloat(y, ancho, "ReturnSpeedOfReactiveHeadBobbingToTheNeutralPosition",
                "Velocidad De Retorno De Los Balanceos De Cabeza Reactivos A La Posicion Neutral",
                "Return Speed Of Reactive Head Bobs To Neutral Position",
                "Explicación Base:\nVelocidad a la que los impulsos reactivos de la cámara regresan a la posición neutral.\n\n" +
                "Explicación Técnica:\nFloat que controla el Lerp de amortiguación de todos los impulsos reactivos (salto, aterrizaje, agachado, acostado, deslizamiento, dash) de vuelta a cero. Valores altos crean impulsos breves y una clase de snap. Valores bajos crean oscilaciones más largas.\n\n" +
                "Ejemplo de Uso:\nUn valor de 8-12 amortigua el impulso en 0.1-0.15 segundos, creando un micro-sacudón perceptible pero breve. Un valor de 3-4 crea una oscilación más larga y cinematográfica que puede usarse para saltos de gran impacto.\n\n" +
                "Ejemplo General:\nEn DOOM Eternal, la velocidad de retorno del cabeceo reactivo es muy alta (oscilaciones muy breves) para no interferir con el combate frenético.",
                "Base Explanation:\nSpeed at which reactive camera impulses return to the neutral position.\n\n" +
                "Technical Explanation:\nFloat that controls the Lerp damping of all reactive impulses (jump, landing, crouch, prone, slide, dash) back to zero. High values create brief, snappy impulses. Low values create longer oscillations.\n\n" +
                "Usage Example:\nA value of 8-12 dampens the impulse in 0.1-0.15 seconds, creating a perceptible but brief micro-jolt. A value of 3-4 creates a longer, more cinematic oscillation that can be used for high-impact jumps.\n\n" +
                "General Example:\nIn DOOM Eternal, the reactive head bob return speed is high (very brief oscillations) to not interfere with frantic combat .", 1f, 48f);

            y = SubHeader(y, ancho, ES ? "— Al Deslizarse / Dash" : "— Slide / Dash");
            y = CampoBoolFloat(y, ancho,
                "EnableReactiveHeadBobbingWhenSliding",
                "IntensityOfReactiveHeadBobbingWhenSliding",
                "Activar El Balanceo De Cabeza Reactivo Al Deslizarse - Intensidad",
                "Enable Reactive Head Bob When Sliding - Intensity",
                "Explicación Base:\nActiva y Aplica un impulso de balanceo de la cámara al iniciar un deslizamiento.\n\n" +
                "Explicación Técnica:\nBool más float. En el frame de inicio del deslizamiento se aplica un impulso a la cámara en la dirección del movimiento. Simula el arranque físico del cuerpo al iniciar el deslizamiento.\n\n" +
                "Ejemplo de Uso:\nUn impulso de 0.05-0.08 al inicio del deslizamiento añade un pequeño feedback que hace que el deslizamiento se sienta muy físico.\n\n" +
                "Ejemplo General:\nEn Apex Legends el deslizamiento tiene un feedback visual de cámara muy satisfactorio que inclina ligeramente la vista en la dirección de movimiento, contribuyendo a la sensación de velocidad y momentum.",
                "Base Explanation:\nEnables and Applies a camera bob impulse at the start of a slide.\n\n" +
                "Technical Explanation:\nBool plus float. In the slide start frame, an impulse is applied to the camera in the movement direction. Simulates the physical start of the body when initiating the slide.\n\n" +
                "Usage Example:\nAn impulse of 0.05-0.08 at slide start adds a feedback 'bite' that makes the slide feel physical.\n\n" +
                "General Example:\nIn Apex Legends the slide has very satisfying camera visual feedback that slightly tilts the view in the direction of movement, contributing to the sense of speed and momentum.", 0f, 48f);
            y = CampoBoolFloat(y, ancho,
                "EnableReactiveHeadBobbingWhenDashing",
                "IntensityOfReactiveHeadBobbingWhenDashing",
                "Activar El Balanceo De Cabeza Reactivo Al Hacer Dash - Intensidad",
                "Enable Reactive Head Bob When Dashing - Intensity",
                "Explicación Base:\nActiva y Aplica un impulso de balanceo de la cámara en la dirección del dash al ejecutarlo.\n\n" +
                "Explicación Técnica:\nBool más float. En el frame de inicio del dash se aplica un impulso a la cámara en la dirección del dash. El impulso refuerza visualmente la dirección y velocidad del movimiento instantáneo.\n\n" +
                "Ejemplo de Uso:\nUn impulso de 0.06-0.10 crea un 'wuush' visual que hace el dash sentirse impactante.\n\n" +
                "Ejemplo General:\nEn Dishonored, el Guiño tiene un efecto visual de cámara muy pronunciado al activarse.",
                "Base Explanation:\nEnables and Applies a camera bob impulse in the dash direction when executing it.\n\n" +
                "Technical Explanation:\nBool plus float. In the dash start frame, an impulse is applied to the camera in the dash direction. The impulse visually reinforces the direction and speed of the instantaneous movement.\n\n" +
                "Usage Example:\nAn impulse of 0.06-0.10 creates a visual 'whoosh' that makes the dash feel impactful.\n\n" +
                "General Example:\nIn Dishonored, Blink has a very pronounced camera visual effect when activated.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Respiración General Y Respiración Agitada" : "— Resting And Exhausted Breathing");
            y = CampoBool(y, ancho + 20, "EnableBreathingEffect",
                "Activar Efecto De Respiración", "Enable Breathing Effect",
                "Explicación Base:\nActiva o desactiva el sistema completo de animación de respiración de la cámara.\n\n" +
                "Explicación Técnica:\nCuando está en True, el sistema calcula cada frame un ciclo de respiración basado en BPM y en el nivel de agotamiento del jugador, generando un desplazamiento vertical y una rotación en pitch sobre la cámara. Si está en False, ambos efectos se mantienen en cero y el sistema no realiza ningún cálculo.\n\n" +
                "Ejemplo de Uso:\nActivarlo añade vida orgánica a la cámara en reposo y refuerza la sensación de esfuerzo físico al correr. Desactivarlo es útil en prototipos, en géneros donde la cámara debe estar completamente estable, o cuando el efecto se gestiona desde otro sistema.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov la respiración de la cámara es uno de los efectos más notorios del juego, afectando directamente a la puntería y variando visiblemente con el nivel de agotamiento del personaje.",
                "Base Explanation:\nEnables or disables the entire camera breathing animation system.\n\n" +
                "Technical Explanation:\nWhen True, the system calculates each frame a breathing cycle based on BPM and the player's exhaustion level, generating a vertical displacement and a pitch rotation on the camera. When False, both effects stay at zero and the system performs no calculations.\n\n" +
                "Usage Example:\nEnabling it adds organic life to the camera at rest and reinforces the feeling of physical effort while running. Disabling it is useful in prototypes, in genres where the camera must be completely stable, or when the effect is managed by another system.\n\n" +
                "General Example:\nIn Escape from Tarkov, camera breathing is one of the most noticeable effects in the game, directly affecting aim and visibly varying with the character's exhaustion level.");
            y = CampoFloat(y, ancho, "BreathsPerMinuteAtRest",
                "Respiraciones Por Minuto Estando En Reposo", "Breaths Per Minute At Rest",
                "Explicación Base:\nFrecuencia del ciclo de respiración cuando el jugador está con la resistencia al máximo, expresada en respiraciones por minuto.\n\n" +
                "Explicación Técnica:\nDefine el extremo inferior del rango de BPM. Cuando el jugador no está agotado, el ciclo de respiración va a esta velocidad, resultando en un movimiento de cámara lento y apenas perceptible.\n\n" +
                "Ejemplo de Uso:\nEl rango fisiológico real de reposo está entre 12 y 20 respiraciones por minuto. Valores bajos como 10-14 generan una respiración casi imperceptible que añade vida sin distraer. Valores más altos como 20-25 hacen la respiración notoria incluso sin agotamiento.\n\n" +
                "Ejemplo General:\nEn juegos de simulación táctica como Arma III la respiración en reposo es suave y su efecto principal es visible al apuntar, donde el movimiento de la mira refleja el ciclo respiratorio.",
                "Base Explanation:\nFrequency of the breathing cycle when the player has full stamina, expressed in breaths per minute.\n\n" +
                "Technical Explanation:\nDefines the lower end of the BPM range. When the player is not exhausted, the breathing cycle runs at this speed, resulting in a slow, barely perceptible camera movement.\n\n" +
                "Usage Example:\nThe real physiological resting range is between 12 and 20 breaths per minute. Low values like 10-14 generate an almost imperceptible breathing that adds life without distracting. Higher values like 20-25 make breathing noticeable even without exhaustion.\n\n" +
                "General Example:\nIn tactical simulation games like Arma III, resting breathing is subtle and its main effect is visible when aiming, where the crosshair movement reflects the breathing cycle.", 1f, 48f);
            y = CampoFloat(y, ancho, "BreathsPerMinuteWhenExhausted",
                "Respiraciones Por Minuto Al Estar Agotado", "Breaths Per Minute When Exhausted",
                "Explicación Base:\nFrecuencia del ciclo de respiración cuando el jugador está agotado, expresada en respiraciones por minuto.\n\n" +
                "Explicación Técnica:\nDefine el extremo superior del rango de BPM. Cuando el jugador está completamente agotado, el ciclo va a esta velocidad, produciendo un movimiento de cámara más frecuente e intenso en combinación con los parámetros de intensidad de agotamiento.\n\n" +
                "Ejemplo de Uso:\nValores entre 25 y 35 BPM transmiten agitación sin resultar mareantes. Valores por encima de 35 pueden generar una oscilación tan rápida que resulte molesta.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov tras correr un tramo largo, la frecuencia y amplitud de la respiración aumentan de forma notable, dificultando la puntería hasta que el jugador se detiene y recupera.",
                "Base Explanation:\nFrequency of the breathing cycle when the player is exhausted, expressed in breaths per minute.\n\n" +
                "Technical Explanation:\nDefines the upper end of the BPM range. When the player is completely exhausted, the cycle runs at this speed, producing more frequent and intense camera movement in combination with the exhaustion intensity parameters.\n\n" +
                "Usage Example:\nValues between 25 and 35 BPM convey agitation without being nauseating. Values above 35 can generate oscillation so fast it becomes uncomfortable.\n\n" +
                "General Example:\nIn Escape from Tarkov, after running a long stretch, the frequency and amplitude of breathing increase noticeably, making aiming difficult until the player stops and recovers.", 1f, 48f);
            y = CampoFloat(y, ancho, "BreathingInhaleFraction",
                "Fracción Perteneciente A La Inhalación", "Breathing Inhale Fraction",
                "Explicación Base:\nProporción del ciclo de respiración dedicada a la inhalación, expresada como fracción del ciclo total entre 0 y 1.\n\n" +
                "Explicación Técnica:\nDivide el ciclo de respiración en dos tramos: la inhalación ocupa la fracción definida aquí y la exhalación ocupa el resto. Un valor de 0.3 genera una subida rápida y una bajada lenta, que es el patrón fisiológico natural.\n\n" +
                "Ejemplo de Uso:\nValores cercanos a 0.3-0.4 reproducen el ritmo respiratorio real. Un valor de 0.5 produce una respiración perfectamente simétrica que puede sentirse mecánica. Valores por debajo de 0.2 generan una inhalación muy brusca seguida de una exhalación muy prolongada.\n\n" +
                "Ejemplo General:\nLa asimetría inhalación-exhalación es un recurso habitual en sistemas de respiración de juegos de simulación para evitar que el efecto se sienta como una onda perfectamente regular y resulte artificial.",
                "Base Explanation:\nProportion of the breathing cycle dedicated to inhalation, expressed as a fraction of the total cycle between 0 and 1.\n\n" +
                "Technical Explanation:\nSplits the breathing cycle into two segments: inhalation occupies the fraction defined here and exhalation occupies the rest. A value of 0.3 generates a fast rise and slow fall, which is the natural physiological pattern.\n\n" +
                "Usage Example:\nValues around 0.3-0.4 reproduce the real breathing rhythm. A value of 0.5 produces a perfectly symmetrical breath that can feel mechanical. Values below 0.2 generate a very abrupt inhalation followed by a very prolonged exhalation.\n\n" +
                "General Example:\nThe inhalation-exhalation asymmetry is a common resource in simulation game breathing systems to prevent the effect from feeling like a perfectly regular wave and coming across as artificial.", 0.1f, 48f);
            y = CampoFloat(y, ancho, "BreathingBasePitchIntensity",
                "Intensidad Base Del Pitch De Respiración", "Breathing Base Pitch Intensity",
                "Explicación Base:\nÁngulo de rotación en pitch(hacia arriba y hacia abajo) que aplica la respiración sobre la cámara cuando el jugador está con la resistencia al máximo, expresado en grados.\n\n" +
                "Explicación Técnica:\nEs el valor mínimo de pitch del sistema. Al estar el jugador 'descansado', la cámara rota en pitch hasta este ángulo en cada ciclo de respiración. A medida que el jugador se agota, 'Intensidad Del Pitch De Respiración Al Estar Agotado' se suma a este valor escalado por el nivel de agotamiento.\n\n" +
                "Ejemplo de Uso:\nValores entre 1 y 3 grados generan un cabeceo apenas perceptible en reposo. Valores superiores a 5 hacen el efecto notorio incluso sin agotamiento y pueden resultar incómodos en sesiones largas.\n\nEjemplo General:\nEn shooters de simulación el pitch de respiración en reposo suele ser muy sutil, reservando la intensidad alta para los estados de agotamiento donde se comunica esfuerzo físico de forma clara.",
                "Base Explanation:\nPitch rotation angle applied by breathing to the camera when the player has full stamina, expressed in degrees.\n\n" +
                "Technical Explanation:\nThis is the minimum pitch value of the system. When the player is 'rested', the camera rotates in pitch up to this angle each breathing cycle. As the player becomes exhausted, 'Breathing Exhausted Pitch Intensity' is added to this value scaled by the exhaustion level.\n\n" +
                "Usage Example:\nValues between 1 and 3 degrees generate a barely perceptible nod at rest. Values above 5 make the effect noticeable even without exhaustion and can be uncomfortable in long sessions.\n\n" +
                "General Example:\nIn simulation shooters, resting breathing pitch tends to be very subtle, reserving high intensity for exhausted states where it clearly communicates physical effort.", 0f, 48f);
            y = CampoFloat(y, ancho, "BreathingExhaustedPitchIntensity",
                "Intensidad Del Pitch De Respiración Al Estar Agotado", "Breathing Exhausted Pitch Intensity",
                "Explicación Base:\nÁngulo de rotación en pitch(hacia arriba y hacia abajo) adicional que se suma a la base cuando el jugador está completamente agotado, expresado en grados.\n\n" +
                "Explicación Técnica:\nCuando el agotamiento es total, la intensidad del pitch final es la suma de 'Intensidad Base Del Pitch De Respiración' más este valor. Esto permite controlar el comportamiento en 'reposo' y el pico máximo de agotamiento de forma independiente.\n\n" +
                "Ejemplo de Uso:\nUn valor de 5 sobre una base de 3 produce un pitch máximo de 8 grados en agotamiento total. Conviene que sea significativamente mayor que la base para que la transición entre 'reposo' y agotamiento sea visualmente legible.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov el agotamiento de la stamina produce un aumento visible del movimiento y temblor del arma, siendo uno de los indicadores más directos del estado físico del personaje durante el combate.",
                "Base Explanation:\nAdditional pitch rotation angle added on top of the base when the player is completely exhausted, expressed in degrees.\n\n" +
                "Technical Explanation:\nWhen exhaustion is total, the final pitch intensity is the sum of 'Breathing Base Pitch Intensity' plus this value. This allows controlling 'resting' behaviour and the maximum exhaustion peak independently.\n\n" +
                "Usage Example:\nA value of 5 on top of a base of 3 produces a maximum pitch of 8 degrees at full exhaustion. It should be significantly larger than the base so the transition between 'rest' and exhaustion is visually readable.\n\n" +
                "General Example:\nIn Escape from Tarkov, stamina exhaustion produces a visible increase in weapon sway and tremor, making it one of the most direct indicators of the character's physical state during combat.", 0f, 48f);
            y = CampoFloat(y, ancho, "BreathingBaseTranslationIntensity",
                "Intensidad Base De La Traslación En La Respiración", "Breathing Base Translation Intensity",
                "Explicación Base:\nDesplazamiento vertical de la cámara causado por la respiración cuando el jugador tiene la resistencia al máximo, expresado en unidades Unity.\n\n" +
                "Explicación Técnica:\nEs el valor mínimo de traslación vertical del sistema. Al estar el jugador descansado, la cámara sube y baja hasta esta distancia en cada ciclo. A medida que el jugador se agota, 'Intensidad De La Traslación En La Respiración Al Estar Agotado' se suma a este valor escalado por el nivel de agotamiento.\n\n" +
                "Ejemplo de Uso:\nValores entre 0.02 y 0.06 generan un movimiento vertical sutil que refuerza la sensación de estar vivo sin resultar mareante. Valores superiores a 0.1 hacen el desplazamiento notorio en reposo. Es importante ajustar siempre junto a 'Intensidad Base Del Pitch De Respiración' para que rotación y traslación sean coherentes.\n\n" +
                "Ejemplo General:\nLa combinación de traslación vertical y rotación en pitch es el estándar en la mayoría de sistemas de respiración de cámara en primera persona, ya que replica el movimiento natural de la cabeza al respirar.",
                "Base Explanation:\nVertical camera displacement caused by breathing when the player has full stamina, expressed in Unity units.\n\n" +
                "Technical Explanation:\nThis is the minimum vertical translation value of the system. When the player is rested, the camera moves up and down by this distance each cycle. As the player becomes exhausted, 'Breathing Exhausted Translation Intensity' is added to this value scaled by the exhaustion level.\n\n" +
                "Usage Example:\nValues between 0.02 and 0.06 generate a subtle vertical movement that reinforces the feeling of being alive without being nauseating. Values above 0.1 make the displacement noticeable at rest. Always adjust together with 'Breathing Base Pitch Intensity' so rotation and translation remain coherent.\n\n" +
                "General Example:\nThe combination of vertical translation and pitch rotation is the standard in most first-person camera breathing systems, as it replicates the natural movement of the head while breathing.", 0f, 48f);
            y = CampoFloat(y, ancho, "BreathingExhaustedTranslationIntensity",
                "Intensidad De La Traslación En La Respiración Al Estar Agotado", "Breathing Exhausted Translation Intensity",
                "Explicación Base:\nDesplazamiento vertical adicional de la cámara causado por la respiración cuando el jugador está completamente agotado, expresado en unidades Unity.\n\n" +
                "Explicación Técnica:\nCuando el agotamiento es total, la traslación vertical final es la suma de 'Intensidad Base De La Traslación En La Respiración' más este valor. Opera en conjunto con 'Intensidad Del Pitch De Respiración Al Estar Agotado' Al Agotarse para construir el estado visual de máximo esfuerzo.\n\n" +
                "Ejemplo de Uso:\nUn valor igual al base duplica el desplazamiento en agotamiento total. Para una diferencia más dramática, usar un valor dos o tres veces mayor que el base. Conviene que el incremento sea perceptible pero no tan grande que resulte incómodo en sesiones largas.\n\n" +
                "Ejemplo General:\nEn shooters de simulación táctica el aumento de la agitación visual al agotarse se usa como retroalimentación del estado físico del personaje, complementando otros indicadores como el sonido de la respiración o la reducción de velocidad.",
                "Base Explanation:\nAdditional vertical camera displacement caused by breathing when the player is completely exhausted, expressed in Unity units.\n\n" +
                "Technical Explanation:\nWhen exhaustion is total, the final vertical translation is the sum of 'Breathing Base Translation Intensity' plus this value. Works together with 'Breathing Exhausted Pitch Intensity' to build the maximum effort visual state.\n\n" +
                "Usage Example:\nA value equal to the base doubles the displacement at full exhaustion. For a more dramatic difference, use a value two or three times larger than the base. The increment should be perceptible but not so large that it becomes uncomfortable in long play sessions.\n\n" +
                "General Example:\nIn tactical simulation shooters, the increase in visual agitation when exhausted is used as feedback of the character's physical state, complementing other indicators such as breathing sounds or speed reduction.", 0f, 48f);
            y = CampoFloat(y, ancho, "BreathingExhaustionFadeInSpeed",
                "Velocidad De Entrada Del Agotamiento En La Respiración", "Breathing Exhaustion Fade In Speed",
                "Explicación Base:\nVelocidad a la que el efecto de agotamiento sobre la respiración aumenta cuando la resistencia baja, expresada en unidades por segundo.\n\n" +
                "Explicación Técnica:\nCuando el nivel de agotamiento real supera al valor suavizado actual, este parámetro controla qué tan rápido el sistema alcanza ese nivel. Un valor alto hace que la respiración agitada aparezca casi de inmediato al bajar la resistencia. Un valor bajo introduce una transición gradual con latencia perceptible.\n\n" +
                "Ejemplo de Uso:\nValores entre 1 y 2 producen una transición rápida pero no instantánea, comunicando el esfuerzo con una ligera latencia que se siente natural. Valores por debajo de 0.5 hacen la respuesta tan lenta que el feedback visual llega tarde respecto a la acción del jugador.\n\n" +
                "Ejemplo General:\nEn juegos de simulación donde el agotamiento tiene consecuencias tácticas directas, una respuesta visual rápida de la respiración es importante para que el jugador pueda anticipar su estado y tomar decisiones antes de quedarse sin resistencia.",
                "Base Explanation:\nSpeed at which the exhaustion effect on breathing increases when stamina drops, expressed in units per second.\n\n" +
                "Technical Explanation:\nWhen the actual exhaustion level exceeds the current smoothed value, this parameter controls how quickly the system reaches that level. A high value makes agitated breathing appear almost immediately when stamina drops. A low value introduces a gradual transition with perceptible latency.\n\n" +
                "Usage Example:\nValues between 1 and 2 produce a fast but not instantaneous transition, communicating effort with a slight latency that feels natural. Values below 0.5 make the response so slow that visual feedback arrives late relative to the player's action.\n\n" +
                "General Example:\nIn simulation games where exhaustion has direct tactical consequences, a fast visual breathing response is important so the player can anticipate their state and make decisions before running out of stamina.", 0.1f, 48f);
            y = CampoFloat(y, ancho, "BreathingExhaustionFadeOutSpeed",
                "Velocidad De Salida Del Agotamiento En La Respiración", "Breathing Exhaustion Fade Out Speed",
                "Explicación Base:\nVelocidad a la que el efecto de agotamiento sobre la respiración disminuye cuando la resistencia se recupera, expresada en unidades por segundo.\n\n" +
                "Explicación Técnica:\nCuando el nivel de agotamiento real cae por debajo del valor suavizado actual, este parámetro controla qué tan rápido el sistema baja a ese nivel. Separarlo de 'Velocidad De Entrada Del Agotamiento En La Respiración' permite que la recuperación visual sea más lenta que la activación, replicando el comportamiento fisiológico real.\n\n" +
                "Ejemplo de Uso:\nUsar un valor menor que 'Velocidad De Entrada Del Agotamiento En La Respiración' es lo más natural. Por ejemplo, entrada de 1.5 y salida de 0.8 hace que el agotamiento aparezca rápido pero se disipe lentamente.\n\n" +
                "Ejemplo General:\nEn Escape from Tarkov la recuperación de la resistencia tras el esfuerzo tiene una velocidad de regeneración separada de su consumo, lo que convierte el manejo de la resistencia en un elemento táctico relevante durante los combates.",
                "Base Explanation:\nSpeed at which the exhaustion effect on breathing decreases when stamina recovers, expressed in units per second.\n\n" +
                "Technical Explanation:\nWhen the actual exhaustion level drops below the current smoothed value, this parameter controls how quickly the system reaches that level. Separating it from 'Breathing Exhaustion Fade In Speed' allows visual recovery to be slower than activation, replicating real physiological behaviour.\n\n" +
                "Usage Example:\nUsing a value lower than 'Breathing Exhaustion Fade In Speed' is the most natural approach. For example, fade in of 1.5 and fade out of 0.8 makes exhaustion appear quickly but dissipate slowly.\n\n" +
                "General Example:\nIn Escape from Tarkov, stamina recovery after effort has a separate regeneration rate from its consumption, making stamina management a relevant tactical element during combat.", 0.1f, 48f);
            return y;
        }

        private float DibujarS11(float y, float ancho)
        {
            y = SubHeader(y, ancho, ES ? "— Movimiento" : "— Movement");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToMoveForward",
                "Tecla Para Moverse Hacia Adelante", "Key To Move Forward",
                "Explicación Base:\nTecla asignada para mover al jugador hacia adelante.\n\n" +
                "Explicación Técnica:\nEnvía un input positivo en el eje vertical del sistema de movimiento. Es la tecla de mayor uso durante el juego y su respuesta debe ser inmediata.\n\n" +
                "Ejemplo de Uso:\nAsignar W en teclado QWERTY es el estándar en PC. En distribuciones alternativas como AZERTY se suele reasignar a la Z.\n\n" +
                "Ejemplo General:\nEn prácticamente todos los FPS modernos como Valorant, CS2 o DOOM Eternal, W es la tecla de movimiento hacia adelante por defecto.",
                "Base Explanation:\nKey assigned to move the player forward.\n\n" +
                "Technical Explanation:\nSends positive input on the vertical axis of the movement system. It is the most used key during gameplay and its response must be immediate.\n\n" +
                "Usage Example:\nAssigning W on a QWERTY keyboard is the de facto standard on PC. On alternative layouts like AZERTY it is usually remapped to Z.\n\n" +
                "General Example:\nIn virtually all modern FPS games like Valorant, CS2, or DOOM Eternal, W is the default forward movement key.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToMoveBackward",
                "Tecla Para Moverse Hacia Atrás", "Key To Move Backward",
                "Explicación Base:\nTecla asignada para mover al jugador hacia atrás.\n\n" +
                "Explicación Técnica:\nEnvía input negativo en el eje vertical del sistema de movimiento. Según la configuración omnidireccional, este movimiento puede ser más lento que el de avanzar.\n\n" +
                "Ejemplo de Uso:\nS en QWERTY es el estándar. Combinada con el multiplicador de retroceso define cuán penalizado es el moverse de espaldas en el juego.\n\n" +
                "Ejemplo General:\nEn Counter-Strike 2, retroceder con S es notablemente más lento que avanzar, penalizando activamente el movimiento de espaldas en duelos.",
                "Base Explanation:\nKey assigned to move the player backward.\n\n" +
                "Technical Explanation:\nSends negative input on the vertical axis of the movement system. Depending on the omnidirectional configuration, this movement may be slower than moving forward.\n\n" +
                "Usage Example:\nS on QWERTY is the standard. Combined with the backward multiplier, it defines how penalized backpedaling is in the game.\n\n" +
                "General Example:\nIn Counter-Strike 2, moving backward with S is noticeably slower than moving forward, actively penalizing backpedaling in duels.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToMoveLeft",
                "Tecla Para Moverse Hacia La Izquierda", "Key To Move Left",
                "Explicación Base:\nTecla asignada para mover al jugador lateralmente hacia la izquierda.\n\n" +
                "Explicación Técnica:\nEnvía input negativo en el eje horizontal del sistema de movimiento. El movimiento lateral izquierdo es fundamental para esquivar cosas en general en combate.\n\n" +
                "Ejemplo de Uso:\nA en QWERTY es el estándar. Junto con la tecla de la derecha forma el par de teclas para moverse lateralmente que define la maniobrabilidad lateral del jugador.\n\n" +
                "Ejemplo General:\nEn Quake y sus derivados, moverse hacia la izquierda combinadolo con el salto es la base del 'strafe-jumping'.",
                "Base Explanation:\nKey assigned to move the player laterally to the left.\n\n" +
                "Technical Explanation:\nSends negative input on the horizontal axis of the movement system. Left lateral strafing is essential for dodging in combat.\n\n" +
                "Usage Example:\nA on QWERTY is the standard. Together with the right key it forms the strafe pair that defines the player's lateral maneuverability.\n\n" +
                "General Example:\nIn Quake and its derivatives, left strafe combined with jumping is the foundation of 'strafe-jumping'.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToMoveRight",
                "Tecla Para Moverse Hacia La Derecha", "Key To Move Right",
                "Explicación Base:\nTecla asignada para mover al jugador lateralmente hacia la derecha.\n\n" +
                "Explicación Técnica:\nEnvía input positivo en el eje horizontal del sistema de movimiento. Simétrica a la tecla de izquierda, ambas juntas definen la amplitud del movimiento lateral disponible.\n\n" +
                "Ejemplo de Uso:\nD en QWERTY es el estándar. Junto a WAS forman el núcleo del esquema WASD, la convención de movimiento más extendida en PC y que seguramente conoces.\n\n" +
                "Ejemplo General:\nEn Apex Legends el movimiento lateral derecho e izquierdo son fundamentales para el movimiento.",
                "Base Explanation:\nKey assigned to move the player laterally to the right.\n\n" +
                "Technical Explanation:\nSends positive input on the horizontal axis of the movement system. Symmetrical to the left key, both together define the available strafe range.\n\n" +
                "Usage Example:\nD on QWERTY is the standard. Together with A they form the core of the WASD scheme, the most widespread movement convention on PC, you surely know about it.\n\n" +
                "General Example:\nIn Apex Legends, right and left strafe are fundamental for the movement.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToRun",
                "Tecla Para Correr", "Key To Run",
                "Explicación Base:\nTecla asignada para la mec de correr.\n\n" +
                "Explicación Técnica:\nMientras se mantiene pulsada, el sistema aplica el multiplicador de velocidad al correr sobre 'Velocidad Base Del Jugador u/s'. Según la configuración, puede ser en modo palanca o en modo mantener.\n\n" +
                "Ejemplo de Uso:\nShift Izq es el estándar más conocido en PC. Algunos juegos lo implementan con el modo Palanca para reducir la fatiga en la mano.\n\n" +
                "Ejemplo General:\nEn COD para correr se usa Shift por defecto.",
                "Base Explanation:\nKey assigned to activate the player's running state.\n\n" +
                "Technical Explanation:\nWhile held, the system applies the sprint speed multiplier on top of 'Player Base Speed u/s'. Depending on the configuration, it can work as toggle or hold.\n\n" +
                "Usage Example:\nLeft Shift is the most widespread standard on PC. Some games implement it as a toggle to reduce hand fatigue.\n\n" +
                "General Example:\nIn COD, sprinting is activated with Shift by default.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToJump",
                "Tecla Para Saltar", "Key To Jump",
                "Explicación Base:\nTecla asignada para ejecutar el salto del jugador.\n\n" +
                "Explicación Técnica:\nAl pulsarla, aplica la fuerza de salto configurada sobre el rigidbody del jugador, siempre que las condiciones de salto se cumplan (en suelo, resistencia suficiente si el sistema está prendido, etc.).\n\n" +
                "Ejemplo de Uso:\nLa Barra espaciadora es el estándar en PC. Su posición ergonómica permite pulsarla con el pulgar sin desplazar los dedos del esquema WASD.\n\n" +
                "Ejemplo General:\nEn la práctica, la totalidad de los FPS y plataformers en PC, la barra espaciadora es la tecla de salto por defecto, desde Halo hasta Minecraft.",
                "Base Explanation:\nKey assigned to execute the player's jump.\n\n" +
                "Technical Explanation:\nWhen pressed, it applies the configured jump force on the player's rigidbody, as long as jump conditions are met (grounded, sufficient stamina, etc.).\n\n" +
                "Usage Example:\nSpacebar is the universal standard on PC. Its ergonomic position allows pressing it with the thumb without moving fingers away from the WASD layout.\n\n" +
                "General Example:\nIn virtually all FPS and platformers on PC, spacebar is the default jump key, from Halo to Minecraft.");

            y = SubHeader(y, ancho, ES ? "— Posturas/Estados Corporales" : "— Stances/Body States");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToCrouch",
                "Tecla Para Agacharse", "Key To Crouch",
                "Explicación Base:\nTecla asignada para que el jugador adopte la postura agachado.\n\n" +
                "Explicación Técnica:\nActiva la transición hacia el estado agachado, reduciendo la hitbox vertical y aplicando los multiplicadores de velocidad agachado si la omni está activa. Puede funcionar en modo mantener o modo palanca según la configuración.\n\n" +
                "Ejemplo de Uso:\nControl izquierdo es el estándar.\n\n" +
                "Ejemplo General:\nEn Counter-Strike el agachado se activa con Ctrl y su modo es el de mantener, por defecto.",
                "Base Explanation:\nKey assigned for the player to adopt the crouching stance.\n\n" +
                "Technical Explanation:\nTriggers the transition to the crouch state, reducing the vertical hitbox and applying the crouching speed multipliers if omni is active. Can work as hold or toggle depending on configuration.\n\n" +
                "Usage Example:\nLeft Control is the most widespread standard.\n\n" +
                "General Example:\nIn Counter-Strike 2, crouch is activated with Ctrl and is hold by default.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToGoProne",
                "Tecla Para Acostarse", "Key To Go Prone",
                "Explicación Base:\nTecla asignada para que el jugador adopte la postura acostado.\n\n" +
                "Explicación Técnica:\nActiva la transición hacia el estado acostado, la postura más baja disponible. Minimiza la hitbox vertical al máximo y aplica los multiplicadores de velocidad acostado si la omni está activa.\n\n" +
                "Ejemplo de Uso:\nZ es la tecla más común para acostarse, en PC.\n\n" +
                "Ejemplo General:\nEn DayZ te acuestas con Z por default.",
                "Base Explanation:\nKey assigned for the player to adopt the prone stance.\n\n" +
                "Technical Explanation:\nTriggers the transition to the prone state, the lowest available stance. It minimizes the vertical hitbox to the maximum and applies the prone speed multipliers if omni is active.\n\n" +
                "Usage Example:\nZ is the most common prone key on PC.\n\n" +
                "General Example:\nIn DayZ, you prone with Z by default.");

            y = SubHeader(y, ancho, ES ? "— Acciones" : "— Actions");
            y = CampoKeyCode(y, ancho, "KeyboardKeyForZoom",
                "Tecla Para Activar O Desactivar El Zoom", "Key To Toggle Zoom",
                "Explicación Base:\nTecla del teclado para activar o desactivar el zoom. El click derecho del ratón también realiza esta misma acción.\n\n" +
                "Explicación Técnica:\nAlterna el estado de zoom del sistema de cámara. Puede ser complementaria al botón del ratón asignado o actuar como una tecla de accesibilidad para jugadores que prefieran no usar el click derecho.\n\n" +
                "Ejemplo de Uso:\nTener una tecla de teclado como fallback del zoom es útil para jugadores con ratones de un solo botón o que juegan con una mano.\n\n" +
                "Ejemplo General:\nEn la mayoría de FPS el zoom se activa con click derecho, pero juegos como Battlefield permiten configurar una tecla de teclado alternativa para el mismo efecto.",
                "Base Explanation:\nKeyboard key to toggle zoom. Right mouse click performs the same action.\n\n" +
                "Technical Explanation:\nToggles the zoom state of the camera system. It can complement the assigned mouse button or act as an accessibility key for players who prefer not to use the right mouse button.\n\n" +
                "Usage Example:\nHaving a keyboard fallback for zoom is useful for players with single-button mice or one-handed setups.\n\n" +
                "General Example:\nIn most FPS games, ADS zoom is activated with right click, but games like Battlefield allow configuring an alternative keyboard key for the same effect.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToPickUpOrDropAnObject",
                "Tecla Para Recoger O Soltar Un Objeto", "Key To Pick Up Or Drop An Object",
                "Explicación Base:\nTecla asignada para recoger un objeto del entorno o soltar el que el jugador lleva en la mano.\n\n" +
                "Explicación Técnica:\nEjecuta la lógica de interacción con objetos del entorno. Si el jugador no lleva nada, intenta recoger el objeto más cercano dentro del rango de interacción. Si ya lleva un objeto, lo suelta.\n\n" +
                "Ejemplo de Uso:\nE o F son las teclas más comunes para interacción en FPS. Asignarla a una tecla de fácil acceso sin abandonar WASD es clave para que la interacción no interrumpa el flujo de movimiento.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 la tecla E cumple esta función.",
                "Base Explanation:\nKey assigned to pick up an object from the environment or drop the one the player is currently holding.\n\n" +
                "Technical Explanation:\nExecutes the object interaction logic. If the player is not holding anything, it attempts to pick up the nearest object within interaction range. If the player is already holding an object, it drops it.\n\n" +
                "Usage Example:\nE or F are the most common interaction keys in FPS games. Assigning it to an easily reachable key without leaving WASD is key to keeping interaction from interrupting movement flow.\n\n" +
                "General Example:\nIn Half-Life 2, E picks up things.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToActivateObjectRotationInHand",
                "Tecla Para Activar El Modo De Rotación Del Objeto En Mano", "Key To Activate Object Rotation Mode",
                "Explicación Base:\nTecla que, mientras se mantiene pulsada, activa el modo de rotación del objeto que el jugador lleva en la mano.\n\n" +
                "Explicación Técnica:\nEsta tecla hace que las teclas de rotación asignadas controlen la orientación del objeto. Al soltar, el objeto queda fijado en la rotación alcanzada.\n\n" +
                "Ejemplo de Uso:\nAsignarla a R o a Alt es cómodo ya que son teclas accesibles sin abandonar el WASD.n\n" +
                "Ejemplo General:\nEn Garry's Mod el modo de rotación de props con la Physics Gun funciona de forma similar, permitiendo orientar objetos con precisión antes de colocarlos en el entorno.",
                "Base Explanation:\nKey that, while held, activates the rotation mode for the object the player is carrying.\n\n" +
                "Technical Explanation:\nWhile this key is held, the assigned rotation keys control the object's orientation. Upon release, the object is locked at the achieved rotation.\n\n" +
                "Usage Example:\nAssigning it to R or Alt is comfortable since they are accessible without leaving WASD.\n\n" +
                "General Example:\nIn Garry's Mod, the Physics Gun's prop rotation mode works similarly, allowing precise object orientation before placing it in the environment.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyForDash",
                "Tecla Para Ejecutar El Dash", "Key To Execute Dash",
                "Explicación Base:\nTecla asignada para ejecutar la acción de dash del jugador.\n\n" +
                "Explicación Técnica:\nAl pulsarla, activa el dash en la dirección de movimiento actual siempre que las condiciones se cumplan (cooldown completado, etc.).\n\n" +
                "Ejemplo de Uso:\nQ o una tecla lateral del ratón son opciones cómodas.\n\n" +
                "Ejemplo General:\nEn Titanfall 2 el dash en Titanes se activa con una tecla dedicada y es fundamental para el combate.",
                "Base Explanation:\nKey assigned to execute the player's dash action.\n\n" +
                "Technical Explanation:\nWhen pressed, activates the dash in the current movement direction as long as conditions are met (cooldown complete, etc.).\n\n" +
                "Usage Example:\nQ or a mouse side button are comfortable options.\n\n" +
                "General Example:\nIn Titanfall 2 the dash on Titans is activated with a dedicated key and is fundamental to combat.");

            y = SubHeader(y, ancho, ES ? "— Rotar El Objeto En Mano" : "— Rotate Object");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToRotateObjectUp",
                "Tecla Para Rotar El Objeto Hacia Arriba", "Key To Rotate Object Up",
                "Explicación Base:\nTecla para rotar el objeto en mano hacia arriba mientras el Modo Rotación está activo.\n\n" +
                "Explicación Técnica:\nAplica una rotación positiva sobre el eje horizontal del objeto. La velocidad de rotación depende de la configuración del sistema de objetos.\n\n" +
                "Ejemplo de Uso:\nAsignarla a la teclas de flecha hacia arriba o a I en un esquema IJKL da un control de rotación intuitivo y separado del movimiento WASD.\n\n" +
                "Ejemplo General:\nEn juegos con construcción como Valheim o The Forest, rotar objetos antes de colocarlos es una mecánica esencial para que todo se mantenga a gusto de uno mismo.",
                "Base Explanation:\nKey to rotate the held object upward while Rotation Mode is active.\n\n" +
                "Technical Explanation:\nApplies a positive rotation on the object's horizontal axis. Rotation speed depends on the object system configuration.\n\n" +
                "Usage Example:\nAssigning it to arrow keys or I in an IJKL layout gives intuitive rotation control separated from WASD movement.\n\n" +
                "General Example:\nIn building games like Valheim or The Forest, rotating objects before placing them is an essential mechanic for fitting pieces into you home.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToRotateObjectDown",
                "Tecla Para Rotar El Objeto Hacia Abajo", "Key To Rotate Object Down",
                "Explicación Base:\nTecla para rotar el objeto en mano hacia abajo mientras el Modo Rotación está activo.\n\n" +
                "Explicación Técnica:\nAplica una rotación negativa sobre el eje horizontal del objeto. Es la contraparte directa de la tecla de rotar hacia arriba.\n\n" +
                "Ejemplo de Uso:\nDebe ser la tecla opuesta a la de rotar hacia arriba para que el control sea simétrico e intuitivo. En un esquema IJKL sería K.\n\n" +
                "Ejemplo General:\nEn No Man's Sky el sistema de construcción permite rotar piezas en múltiples ejes.",
                "Base Explanation:\nKey to rotate the held object downward while Rotation Mode is active.\n\n" +
                "Technical Explanation:\nApplies a negative rotation on the object's horizontal axis. It is the direct counterpart of the rotate-up key.\n\n" +
                "Usage Example:\nIt should be the opposite key to rotate-up so that control is symmetrical and intuitive. In an IJKL layout this would be K.\n\n" +
                "General Example:\nIn No Man's Sky the building system allows rotating pieces on multiple axes.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToRotateObjectLeft",
                "Tecla Para Rotar El Objeto Hacia La Izquierda", "Key To Rotate Object Left",
                "Explicación Base:\nTecla para rotar el objeto en mano hacia la izquierda mientras el Modo Rotación está activo.\n\n" +
                "Explicación Técnica:\nAplica una rotación negativa sobre el eje vertical del objeto. Permite girar el objeto en el plano horizontal sin mover al jugador.\n\n" +
                "Ejemplo de Uso:\nEn un esquema de flechas sería la flecha izquierda. En IJKL sería J. Lo importante es que sea intuitivamente opuesta a la tecla de rotar a la derecha.\n\n" +
                "Ejemplo General:\nEn Ark el sistema para rotar abarca la dirección hacia la izquierda y la derecha.",
                "Base Explanation:\nKey to rotate the held object to the left while Rotation Mode is active.\n\n" +
                "Technical Explanation:\nApplies a negative rotation on the object's vertical axis. Allows spinning the object in the horizontal plane without moving the player.\n\n" +
                "Usage Example:\nIn an arrow key layout this would be the left arrow. In IJKL it would be J. The important thing is that it is intuitively opposite to the rotate-right key.\n\n" +
                "General Example:\nIn Ark the rotation system has the posibilitie to rotate everything from left to right.");
            y = CampoKeyCode(y, ancho, "KeyboardKeyToRotateObjectRight",
                "Tecla Para Rotar El Objeto Hacia La Derecha", "Key To Rotate Object Right",
                "Explicación Base:\nTecla para rotar el objeto en mano hacia la derecha mientras el Modo Rotación está activo.\n\n" +
                "Explicación Técnica:\nAplica una rotación positiva sobre el eje vertical del objeto. Es la contraparte simétrica de la tecla de rotar a la izquierda.\n\n" +
                "Ejemplo de Uso:\nEn un esquema de flechas sería la flecha derecha. En IJKL sería L. Junto a las otras tres teclas de rotación se forma un sistema completo de orientación de objetos en dos ejes.\n\n" +
                "Ejemplo General:\nEn Subnautica el sistema de construcción de bases permite rotar módulos en el plano horizontal para adaptarlos antes de confirmar su colocación.",
                "Base Explanation:\nKey to rotate the held object to the right while Rotation Mode is active.\n\n" +
                "Technical Explanation:\nApplies a positive rotation on the object's vertical axis. It is the symmetrical counterpart of the rotate-left key.\n\n" +
                "Usage Example:\nIn an arrow key layout this would be the right arrow. In IJKL it would be L. Together with the other three rotation keys it forms a complete two-axis object orientation system.\n\n" +
                "General Example:\nIn Subnautica the base building system allows rotating modules in the horizontal plane to adapt them before confirming their placement.");

            y = SubHeader(y, ancho, ES ? "— Ratón" : "— Mouse");
            y = CampoInt(y, ancho, "MouseButtonToThrowTheObjectInHand",
                "Botón Del Ratón Para Lanzar El Objeto En Mano", "Mouse Button To Throw Held Object",
                "Explicación Base:\nBotón del ratón asignado para lanzar el objeto que el jugador lleva en la mano.\n\n" +
                "Explicación Técnica:\nInt donde 0 es click izquierdo, 1 es click derecho y 2 es click central. Al pulsarlo, aplica una fuerza de lanzamiento al objeto en la dirección de la mira del jugador con la intensidad configurada en el sistema de objetos.\n\n" +
                "Ejemplo de Uso:\nEl click izquierdo (0) es lo más intuitivo ya que es el botón de acción primaria. Si el zoom está en click derecho, usar el izquierdo para lanzar evita conflictos de input.\n\n" +
                "Ejemplo General:\nEn Half-Life 2 el click izquierdo con la Gravity Gun lanza los objetos.",
                "Base Explanation:\nMouse button assigned to throw the object the player is holding.\n\n" +
                "Technical Explanation:\nTakes an integer where 0 is left click, 1 is right click, and 2 is middle click. When pressed, it applies a throw force to the object in the player's look direction with the intensity configured in the object system.\n\n" +
                "Usage Example:\nLeft click (0) is the most intuitive as it is the primary action button. If zoom is on right click, using left click to throw avoids input conflicts.\n\n" +
                "General Example:\nIn Half-Life 2, left clicking with the Gravity Gun throws objects.", 0, 2);
            return y;
        }

        private float DibujarS12(float y, float ancho)
        {
            y = CampoFloat(y, ancho, "LeftStickDeadZone",
                "Zona Muerta Del Joystick Izquierdo", "Left Joystick Dead Zone",
                "Explicación Base:\nUmbral mínimo de input del joystick izquierdo por debajo del cual el movimiento es ignorado, evitando el drift del stick.\n\n" +
                "Explicación Técnica:\nFloat entre 0 y 0.5 en este apartado que filtra los valores de entrada del joystick analógico. Cualquier input cuya magnitud sea inferior a este valor se trata como cero. Necesario porque los sticks analógicos físicos nunca reposan exactamente en el centro.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.1 a 0.15 es el rango estándar. Valores demasiado bajos comunmente 'permiten' drift en sticks desgastados; valores demasiado altos hacen el stick menos responsivo en movimientos sutiles.\n\n" +
                "Ejemplo General:\nTodos los motores de juego y SDKs de mando implementan zonas muertas. Unity recomienda aplicarlas manualmente para tener control total sobre el comportamiento del input analógico.",
                "Base Explanation:\nMinimum input threshold of the left joystick below which movement is ignored, preventing stick drift.\n\n" +
                "Technical Explanation:\nFloat between 0 and 0.5 that filters analog joystick input values. Any input whose magnitude is below this value is treated as zero. Necessary because physical analog sticks never rest exactly at center.\n\n" +
                "Usage Example:\nA value of 0.1 to 0.15 is the standard range. Values too low 'allow' drift on worn sticks; values too high make the stick less responsive for subtle movements.\n\n" +
                "General Example:\nAll game engines and gamepad SDKs implement dead zones. Unity recommends applying them manually to have full control over analog input behavior.", 0f, 48f);
            y = CampoFloat(y, ancho, "RightStickDeadZone",
                "Zona Muerta Del Joystick Derecho", "Right Joystick Dead Zone",
                "Explicación Base:\nUmbral mínimo de input del joystick derecho por debajo del cual el movimiento es ignorado, evitando el drift del stick.\n\n" +
                "Explicación Técnica:\nFloat entre 0 y 0.5 en este apartado que filtra los valores de entrada del joystick analógico. Cualquier input cuya magnitud sea inferior a este valor se trata como cero. Necesario porque los sticks analógicos físicos nunca reposan exactamente en el centro.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.1 a 0.15 es el rango estándar. Valores demasiado bajos comunmente 'permiten' drift en sticks desgastados; valores demasiado altos hacen el stick menos responsivo en movimientos sutiles.\n\n" +
                "Ejemplo General:\nTodos los motores de juego y SDKs de mando implementan zonas muertas. Unity recomienda aplicarlas manualmente para tener control total sobre el comportamiento del input analógico.",
                "Base Explanation:\nMinimum input threshold of the right joystick below which movement is ignored, preventing stick drift.\n\n" +
                "Technical Explanation:\nFloat between 0 and 0.5 that filters analog joystick input values. Any input whose magnitude is below this value is treated as zero. Necessary because physical analog sticks never rest exactly at center.\n\n" +
                "Usage Example:\nA value of 0.1 to 0.15 is the standard range. Values too low 'allow' drift on worn sticks; values too high make the stick less responsive for subtle movements.\n\n" +
                "General Example:\nAll game engines and gamepad SDKs implement dead zones. Unity recommends applying them manually to have full control over analog input behavior.", 0f, 48f);

            y = SubHeader(y, ancho, ES ? "— Botones de Acción  (Xbox / PlayStation)" : "— Action Buttons  (Xbox / PlayStation)");
            y = InfoBox(y, ancho,
                ES ? "Valores por defecto para Xbox.\nA=Cruz · B=Círculo · X=Cuadrado · LB=L1 · RB=R1 · LS=L3"
                   : "Default values for Xbox.\nA=Cross · B=Circle · X=Square · LB=L1 · RB=R1 · LS=L3");
            y = CampoKeyCode(y, ancho, "GamepadButtonToJump",
                "Botón Del Mando Para Saltar  (A / Cruz)", "Gamepad Button To Jump  (A / Cross)",
                "Explicación Base:\nBotón del mando asignado para ejecutar el salto del jugador.\n\n" +
                "Explicación Técnica:\nEn Unity, el botón A de Xbox corresponde a el JoystickButton0. Al pulsarlo se aplica la fuerza de salto configurada siempre que las condiciones se cumplan. Es el botón de acción más importante del mando.\n\n" +
                "Ejemplo de Uso:\nA / Cruz es el estándar universal en consolas para saltar. Su posición en el face cluster del mando permite pulsarlo con el pulgar sin reposicionar la mano.\n\n" +
                "Ejemplo General:\nEn prácticamente todos los juegos de consola desde los años 90, el botón inferior del face cluster (A en Xbox, Cruz en PlayStation, B en Switch) es la tecla de salto universal.",
                "Base Explanation:\nGamepad button assigned to execute the player's jump.\n\n" +
                "Technical Explanation:\nIn Unity, Xbox's A button corresponds to JoystickButton0. When pressed it applies the configured jump force as long as conditions are met. It is the most important action button on the gamepad.\n\n" +
                "Usage Example:\nA / Cross is the universal console standard for jumping. Its position in the face cluster allows pressing it with the thumb without repositioning the hand.\n\n" +
                "General Example:\nIn virtually all console games since the 1990s, the bottom button of the face cluster (A on Xbox, Cross on PlayStation, B on Switch) is the universal jump button.");
            y = CampoKeyCode(y, ancho, "GamepadButtonToCrouchAndGoProne",
                "Botón Del Mando Para Agacharse Y Acostarse  (B / Círculo)", "Gamepad Button To Crouch And Prone  (B / Circle)",
                "Explicación Base:\nBotón del mando para agacharse con un presionado corto y acostarse con un presionado largo.\n\n" +
                "Explicación Técnica:\nEl sistema distingue entre presionar durante un periodo corto y uno largo sobre el mismo botón para ejecutar dos acciones distintas. El umbral de tiempo entre ambas se configura en la sección de Tiempos de Pulsación.\n\n" +
                "Ejemplo de Uso:\nConsolidar el agachado y el acostado en un solo botón es esencial en mando para no saturar el esquema de controles. El presionado largo para acostado es intuitivo porque representa la acción más comprometedora de las dos.\n\n" +
                "Ejemplo General:\nEn COD en consola, B / Círculo maneja el agachado y el acostado según igual la duración del presionado, un esquema que se ha vuelto estándar en los FPS de consola.",
                "Base Explanation:\nGamepad button for crouching with a short press and going prone with a long press.\n\n" +
                "Technical Explanation:\nThe system distinguishes between a short press and a long press on the same button to execute two different actions. The time threshold between both is configured in the Press Timings section.\n\n" +
                "Usage Example:\nConsolidating crouch and prone into a single button is essential on gamepad to avoid saturating the control scheme. Long press for prone is intuitive because it represents the more committed of the two actions.\n\n" +
                "General Example:\nIn Call of Duty on console, B / Circle handles crouch and prone based on press duration, a scheme that has become standard in console FPS games.");
            y = CampoKeyCode(y, ancho, "GamepadButtonToPickUpOrDropAnObject",
                "Botón Del Mando Para Recoger O Soltar Un Objeto  (X / Cuadrado)", "Gamepad Button To Pick Up Or Drop An Object  (X / Square)",
                "Explicación Base:\nBotón del mando para recoger un objeto del entorno o soltar el que el jugador lleva en la mano.\n\n" +
                "Explicación Técnica:\nSi el jugador no lleva objeto, intenta recoger el más cercano dentro del rango. Si ya lleva uno, lo suelta en la posición actual.\n\n" +
                "Ejemplo de Uso:\nX / Cuadrado es la tecla de interacción estándar en muchos juegos de consola. Separarla del salto (A / Cruz) evita confusiones en momentos de alta tensión donde el jugador necesita reaccionar rápido.\n\n" +
                "Ejemplo General:\nEn FPS tradicionales de consola X / Cuadrado es la tecla de interacción y recogida por convención.",
                "Base Explanation:\nGamepad button to pick up an object from the environment or drop the one the player is holding.\n\n" +
                "Technical Explanation:\nIf the player is not holding an object, it attempts to pick up the nearest one within range. If they are already holding one, it drops it at the current position.\n\n" +
                "Usage Example:\nX / Square is the standard interaction button in many console games. Keeping it separate from jump (A / Cross) avoids confusion in high-tension moments where the player needs to react quickly.\n\n" +
                "General Example:\nIn traditional console FPS games X / Square is the interaction and pickup button by convention.");
            y = CampoKeyCode(y, ancho, "GamepadButtonToActivateObjectRotationInHand",
                "Botón Del Mando Para Activar El Modo De Rotación Del Objeto En Mano  (RB / R1)", "Gamepad Button To Activate Object Rotation Mode  (RB / R1)",
                "Explicación Base:\nBotón del mando que, mientras se mantiene pulsado, activa el modo de rotación del objeto en mano.\n\n" +
                "Explicación Técnica:\nMientras RB / R1 esté presionado si se está en modo mantener el modo rotar se activa.\n\n" +
                "Ejemplo de Uso:\nUsar un botón trasero para el modo rotación es muy ergonómico.\n\n" +
                "Ejemplo General:\nEn Garry's Mod con mando, el equivalente a rotar props usa los bumpers del mando de forma similar.",
                "Base Explanation:\nGamepad button that, while held, activates the rotation mode for the held object.\n\n" +
                "Technical Explanation:\nWhile RB / R1 is held if hold mode is active, rotation mode engages.\n\n" +
                "Usage Example:\nUsing a shoulder button for rotation mode is very ergonomic.\n\n" +
                "General Example:\nIn Garry's Mod with a controller, the equivalent of rotating props uses the gamepad bumpers similarly.");
            y = CampoKeyCode(y, ancho, "GamepadButtonForDash",
                "Botón Del Mando Para El Dash  (LB / L1)", "Gamepad Button To Dash  (LB / L1)",
                "Explicación Base:\nBotón del mando asignado para ejecutar el dash.\n\n" +
                "Explicación Técnica:\nAl pulsarlo activa el dash en la dirección del stick izquierdo siempre que las condiciones se cumplan. Según la configuración puede requerir presionar dos veces el botón para ejecutarse.\n\n" +
                "Ejemplo de Uso:\nLB / L1 es una posición cómoda para el dash ya que el índice izquierdo está naturalmente sobre él. Esto permite ejecutar el dash sin desplazar el pulgar del stick de movimiento.\n\n" +
                "Ejemplo General:\nEn Titanfall 2 en consola el dash se puede asignar a LB en el modo Evolucionado, permitiendo combinarlo fluidamente con el movimiento del stick izquierdo para hacer el dash en cualquier dirección sin interrumpir el apuntado.",
                "Base Explanation:\nGamepad button assigned to execute the dash.\n\n" +
                "Technical Explanation:\nWhen pressed, activates the dash in the left stick direction as long as conditions are met. Depending on configuration it may require a double press of the button or stick to execute.\n\n" +
                "Usage Example:\nLB / L1 is a comfortable position for dash since the left index finger naturally rests on it. This allows executing the dash without moving the thumb from the movement stick.\n\n" +
                "General Example:\nIn Titanfall 2 on console the dash can be executed with LB in Evolved mode, allowing it to be fluidly combined with left stick movement to execute the dash in any direction without interrupting aiming.");
            y = CampoKeyCode(y, ancho, "GamepadButtonToRun",
                "Botón Del Mando Para Correr  (LS / L3)", "Gamepad Button To Run  (LS  / L3)",
                "Explicación Base:\nBotón del mando asignado para activar el correr, mediante el 'clickeo' del stick izquierdo.\n\n" +
                "Explicación Técnica:\nEn Unity, el click del stick izquierdo (LS) corresponde a JoystickButton8. Al pulsarlo activa o desactiva el correr según la configuración de palanca/mantener del sistema de movimiento.\n\n" +
                "Ejemplo de Uso:\nLS es la posición estándar para correr en consola. Aunque no es la posición más ergonómica, es la convención más universal y los jugadores de consola ya la tenemos interiorizada.\n\n" +
                "Ejemplo General:\nEn la práctica, la totalidad de FPS de consola como COD, Halo o Battlefield, el correr se activa con LS como un estándar.",
                "Base Explanation:\nGamepad button assigned to activate running, via left stick click.\n\n" +
                "Technical Explanation:\nIn Unity, left stick click (LS) corresponds to JoystickButton8. When pressed it activates or deactivates sprint depending on the toggle/hold configuration of the movement system.\n\n" +
                "Usage Example:\nLS is the standard sprint position on console. Although not the most ergonomic, it is the most widespread convention and we console players have it internalized.\n\n" +
                "General Example:\nIn virtually all modern console FPS games like COD, Halo, or Battlefield, sprinting is activated with LS as the de facto genre standard.");

            y = SubHeader(y, ancho, ES ? "— Gatillos (ejes analógicos)" : "— Triggers (analog axes)");
            y = InfoBox(y, ancho,
                ES ? "LT = Zoom  |  RT = Lanzar objeto\nLeídos como ejes analógicos. Umbral: 0.3\nRegistrados automáticamente por el PlayerControllerSetup_ControladorDelJugadorSetup.cs"
                   : "LT = Zoom  |  RT = Throw object\nRead as analog axes. Threshold: 0.3\nAuto-registered by the PlayerControllerSetup_ControladorDelJugadorSetup.cs");
            y = CampoKeyCode(y, ancho, "GamepadButtonForZoom",
                "Botón Del Mando Para El Zoom  (LT / L2)", "Gamepad Button For Zoom  (LT / L2)",
                "Explicación Base:\nBoton(Gatillo) para ejecutar el zoom.\n\n" +
                "Explicación Técnica:\nEl sistema lee el Boton(Gatillo) asignado - LT como un eje analógico con umbral 0.3, registrado automáticamente por PlayerControllerSetup_ControladorDelJugadorSetup.cs y activa el zoom.\n\n" +
                "Ejemplo de Uso:\nEn casi todos los FPS en mando el apuntar/zoom se asigna a uno de los Gatillos.\n\n" +
                "Ejemplo General:\nEn juegos como Halo el apuntado en mando se asigna por default a el gatillo LT.",
                "Base Explanation:\nButton(Trigger) to execute zoom.\n\n" +
                "Technical Explanation:\nThe system reads the assigned Button(Trigger) - LT as an analog axis with a 0.3 threshold, auto-registered by PlayerControllerSetup_ControladorDelJugadorSetup.cs and enables zoom.\n\n" +
                "Usage Example:\nIn almost all FPS games on controller, aiming/zoom is assigned to one of the triggers.\n\n" +
                "General Example:\nIn games like Halo, aiming on a controller is assigned to the LT trigger by default.");
            y = CampoKeyCode(y, ancho, "GamepadButtonToThrowTheObjectInHand",
                "Botón Del Mando Para Lanzar El Objeto En Mano  (RT / R2)", "Gamepad Button To Throw Held Object  (RT / R2)",
                "Explicación Base:\nBoton(Gatillo) para lanzar el objeto en mano.\n\n" +
                "Explicación Técnica:\nEl sistema lee el Boton(Gatillo) asignado - RT como eje analógico con umbral 0.3, registrado automáticamente por PlayerControllerSetup_ControladorDelJugadorSetup.cs y lanza el objeto en mano.\n\n" +
                "Ejemplo de Uso:\nEn casi todos los juegos en general, asignar el gatillo RT a el Disparo/Lance de Objetos es ya una base.\n\n" +
                "Ejemplo General:\nEn juegos como Halo Infinite el RT es el gatillo designado por default para Disparar.",
                "Base Explanation:\nButton(Trigger) for throwing the held object.\n\n" +
                "Technical Explanation:\nThe system reads the assigned Button(Trigger) - RT as an analog axis with a 0.3 threshold, auto-registered by PlayerControllerSetup_ControladorDelJugadorSetup.cs and throws the object in hand.\n\n" +
                "Usage Example:\nIn most games in general, assigning the RT trigger to shooting/throwing objects is already a standard.\n\n" +
                "General Example:\nIn games like Halo Infinite, the RT is the trigger assigned by default for shooting.");

            y = SubHeader(y, ancho, ES ? "— Tiempos de Pulsación" : "— Press Timings");
            y = CampoFloat(y, ancho, "ButtonHoldTimeToBeConsideredAsProne",
                "Tiempo Manteniendo El Botón Pulsado Para Interpretarlo Como Acostarse", "Button Hold Time To Interpret As Prone",
                "Explicación Base:\nTiempo mínimo en segundos que debe mantenerse pulsado el botón de agachado/acostado para que el sistema lo interprete como la acción de acostarse en lugar de agacharse.\n\n" +
                "Explicación Técnica:\nFloat en segundos. Si el botón se suelta antes de este umbral, se ejecuta el agachado. Si se mantiene igual o más tiempo, se ejecuta el acostarse. Define la frontera entre las dos acciones que comparten un botón.\n\n" +
                "Ejemplo de Uso:\nUn valor de 0.3 a 0.5 segundos es el rango más cómodo. Demasiado bajo provoca cambios accidentales a acostado; demasiado alto hace la acción de acostarse frustrante y lenta de ejecutar.\n\n" +
                "Ejemplo General:\nEn Warzone el tiempo de mantenimiento para activar el acostado ronda los 0.35 segundos, un valor que equilibra la responsividad del agachado con la intencionalidad del acostado.",
                "Base Explanation:\nMinimum time in seconds the crouch/prone button must be held for the system to interpret it as going prone instead of crouching.\n\n" +
                "Technical Explanation:\nFloat in seconds. If the button is released before this threshold, crouch is executed. If held for equal or longer, prone is executed. It defines the boundary between the two actions that share the same button.\n\n" +
                "Usage Example:\nA value of 0.3 to 0.5 seconds is the most comfortable range. Too low causes accidental prone transitions; too high makes going prone frustrating and slow to execute.\n\n" +
                "General Example:\nIn Call of Duty: Warzone the hold time for prone from crouch is around 0.35 seconds, a value that balances crouch responsiveness with prone intentionality.", 0.1f, 48f);
            return y;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region CRÉDITOS · GUARDADO · DOCUMENTACIÓN  /  CREDITS · SAVE · DOCS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private float DibujarContenidoCreditos(float ancho)
        {
            float y = PAD;

            y = SubHeader(y, ancho, ES ? "— Asset" : "— Asset");
            y = FilaInfo(y, ancho + 300, "Nombre / Name", "FPC First Person Controller - CPP Controlador Primera Persona - 48AG");
            y = FilaInfo(y, ancho, "Versión / Version", "1.0");

            y = SubHeader(y, ancho, ES ? "— Autor" : "— Author");
            y = FilaInfo(y, ancho, ES ? "Nombre" : "Name", "- 48Assets&Games");
            y = FilaInfo(y, ancho, ES ? "Contacto" : "Contact", "- 48assetsandgames@gmail.com");

            y = SubHeader(y, ancho, "— Links");
            y = BotonLink(y, ancho, "▶  Unity Asset Store", "https://assetstore.unity.com/users/4674179105286");
            y = BotonLink(y, ancho, "▶  Itch Asset Page", "https://48assetsgames.itch.io/");

            return y + PAD;
        }

        private float DibujarContenidoDocumentacion(float ancho)
        {
            float y = PAD;

            y = SubHeader(y, ancho, ES ? "— Guía Rápida" : "— Quick Start");
            y = InfoBox(y, ancho,
                ES ? "1. Añadir el PlayerController_ControladorDelJugador.cs al GameObject del jugador.\n\n" +
                     "2. Crear el ScriptableObject PlayerConfiguration_ConfiguracionDelJugador.\n\n" +
                     "3. Asignar este ScriptableObject al campo Configuration_Configuracion del PlayerController_ControladorDelJugador.\n\n" +
                     "4. Crear un transform que fungira como el soporte de la cámara y crear una cámara.\n\n" +
                     "5. Asignar el transform a CameraSupport_SoporteDeLaCamara y la cámara a MainCamera_CamaraPrincipal en el PlayerController_ControladorDelJugador.\n\n" +
                     "6. Crear un Canvas con 3 imagenes, colocar una en el centro y reducir su escala, y las otras 2 escalarlas a lo largo y ubicarlas una encima de la otra, asegurarse de que 1 sea blanca y la otra negra.\n\n" +
                     "7. Asignar la imagen del centro a CrosshairOnHUD_CrosshairEnElHUD, la imagen de color negro asignarla a StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia y la de color blanco a CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual en el PlayerController_ControladorDelJugador.\n\n" +
                     "8. Configurar la Capa del Suelo en el apartado de Salto(04) en el bloque Mecanicas Base dentro de la Window Inspector.\n\n"
                   : "1. Add PlayerController_ControladorDelJugador to the player GameObject.\n\n" +
                     "2. Create the ScriptableObject PlayerConfiguration_ConfiguracionDelJugador.\n\n" +
                     "3. Assign this ScriptableObject to the Configuration_Configuracion field of the PlayerController_ControladorDelJugador.\n\n" +
                     "4. Create a Transform that will serve as the camera support and create a camera.\n\n" +
                     "5. Assign the Transform to CameraSupport_SoporteDeLaCamara and the camera to MainCamera_CamaraPrincipal in the PlayerController_ControladorDelJugador.\n\n" +
                     "6. Create a Canvas with 3 Images. Place one in the center and reduce its scale, and scale the other two horizontally and place them on top of each other. Make sure one is white and the other is black.\n\n" +
                     "7. Assign the center image to CrosshairOnHUD_CrosshairEnElHUD, assign the black image to StaminaBarBackgroundImage_ImagenDeFondoDeLaBarraDeResistencia, and the white image to CurrentStaminaBarImage_ImagenDeLaBarraDeResistenciaActual in the PlayerController_ControladorDelJugador.\n\n" +
                     "8. Configure the Ground Layer in the Jump (04) section in the block Base Mechanics in the Inspector Window.\n");

            y = SubHeader(y, ancho, ES ? "— Documentación en PDF" : "PDF Documentation");
            y = BotonLink(y, ancho, ES ? "— ¿Cómo Usar el Asset?" : "How to Use the Asset", ES ? "https://github.com/48AssetsAndGames/48A-01A-FPC-CPP/blob/main/Assets/48A%20-%2001%20-%20FPC%20CPP/Espa%C3%B1ol/ControladorPrimeraPersona/03___Documentacion/FPC%20CPP_Como%20Usar_V1.0_ES.pdf" : "https://github.com/48AssetsAndGames/48A-01A-FPC-CPP/blob/main/Assets/48A%20-%2001%20-%20FPC%20CPP/English/FirstPersonController/03___Documentation/FPC%20CPP_How%20To%20Use_V1.0_EN.pdf");
            y = BotonLink(y, ancho, ES ? "— Documentación Técnica" : "Technical Documentation", ES ? "https://github.com/48AssetsAndGames/48A-01A-FPC-CPP/blob/main/Assets/48A%20-%2001%20-%20FPC%20CPP/Espa%C3%B1ol/ControladorPrimeraPersona/03___Documentacion/FPC%20CPP_Documentaci%C3%B3n%20T%C3%A9cnica_V1.0_ES.pdf" : "https://github.com/48AssetsAndGames/48A-01A-FPC-CPP/blob/main/Assets/48A%20-%2001%20-%20FPC%20CPP/English/FirstPersonController/03___Documentation/FPC%20CPP_Technical%20Documentation_V1.0_EN.pdf");

            y += SEP_H;
            GUI.Label(new Rect(PAD * 2f, y, ancho - PAD * 4f, 20f), ES ? "Documentación completa INTERNA del asset - En Proceso." : "Complete INTERNAL asset documentation — In progress.", StLabelSub);
            y += 22f;

            return y + PAD;
        }

        private float DibujarContenidoGuardado(float ancho)
        {
            float y = PAD;

            y = SubHeader(y, ancho, ES ? "— Resetear" : "— Reset");
            y += 4f;

            Rect rBtn = new Rect(PAD * 2f, y, ancho - PAD * 4f, 28f);
            if (BotonColoreado(rBtn, ES ? "Resetear TODOS los valores a predeterminados" : "Reset ALL values to defaults", C_ROJO))
            {
                if (EditorUtility.DisplayDialog(ES ? "Confirmar Reset" : "Confirm Reset", ES ? "¿Resetear TODOS los valores?\nEsta acción no se puede deshacer." : "Reset ALL values to defaults?\nThis action cannot be undone.", ES ? "Resetear" : "Reset", ES ? "Cancelar" : "Cancel"))
                {
                    ResetearADefaults();
                }
            }
            y += 32f;

            y = SubHeader(y, ancho, ES ? "— Guardar como JSON" : "— Save as JSON");
            y += 4f;

            float anchoUtil = ancho - PAD * 4f;
            GUI.Label(new Rect(PAD * 2f, y, 55f, FILA_H), ES ? "Nombre:" : "Name:", StLabel);
            NombreNuevoJSON = EditorGUI.TextField(new Rect(PAD * 2f + 58f, y + 1f, anchoUtil - 58f - 70f, FILA_H - 2f), NombreNuevoJSON, new GUIStyle(EditorStyles.textField) { font = ObtenerFuente(), fontSize = 10, normal = { textColor = C_TEXTO } });

            bool nombreValido = !string.IsNullOrEmpty(NombreNuevoJSON);
            GUI.enabled = nombreValido;
            if (BotonColoreado(new Rect(PAD * 2f + anchoUtil - 66f, y + 1f, 64f, FILA_H - 2f), ES ? "Guardar" : "Save", C_AZUL))
            {
                GuardarJSON(NombreNuevoJSON);
                NombreNuevoJSON = "";
                ActualizarListaJSON();
            }
            GUI.enabled = true;
            y += FILA_H + 8f;

            y = SubHeader(y, ancho, ES ? "— Configuraciones Guardadas" : "— Saved Configurations");
            y += 4f;

            if (ArchivosJSON.Count == 0)
            {
                GUI.Label(new Rect(PAD * 2f, y, anchoUtil, FILA_H), ES ? "No hay configuraciones guardadas." : "No saved configurations.", StLabelSub);
                y += FILA_H + 4f;
            }
            else
            {
                foreach (string archivo in new List<string>(ArchivosJSON))
                {
                    string nombre = Path.GetFileNameWithoutExtension(archivo);

                    Rect rFila = new Rect(PAD * 2f, y, anchoUtil, FILA_H + 4f);
                    EditorGUI.DrawRect(rFila, new Color(0.09f, 0.09f, 0.10f, 1f));
                    DibujarBorde(rFila, C_BORDE_SUB);

                    GUI.Label(new Rect(rFila.x + 6f, rFila.y + 4f, rFila.width - 130f, FILA_H), nombre, StLabel);

                    if (BotonColoreado(new Rect(rFila.xMax - 122f, rFila.y + 3f, 58f, FILA_H - 2f), ES ? "Cargar" : "Load", C_VERDE))
                        CargarJSON(archivo);

                    if (BotonColoreado(new Rect(rFila.xMax - 60f, rFila.y + 3f, 54f, FILA_H - 2f), ES ? "Borrar" : "Delete", C_ROJO))
                    {
                        if (EditorUtility.DisplayDialog(ES ? "Eliminar" : "Delete", $"{(ES ? "¿Eliminar" : "Delete")} \"{nombre}\"?", ES ? "Eliminar" : "Delete", ES ? "Cancelar" : "Cancel"))
                        {
                            EliminarJSON(archivo);
                            ActualizarListaJSON();
                            break;
                        }
                    }

                    y += FILA_H + 8f;
                }
            }

            return y + PAD;
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region OPERACIONES JSON  /  JSON OPERATIONS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        internal void ActualizarListaJSON()
        {
            ArchivosJSON.Clear();
            string carpeta = ObtenerCarpetaJSON();
            if (!Directory.Exists(carpeta)) return;
            ArchivosJSON.AddRange(Directory.GetFiles(carpeta, "*.json"));
        }

        // En: Re-resolves the ground LayerMask by NAME ("Ground" / "Suelo") in the CURRENT project.
        //     Layer indices differ between projects, so this keeps the asset/Inspector correct after a transfer.
        //     Only overrides the value if a named ground layer actually exists in this project.
        // Es: Vuelve a resolver el LayerMask del suelo por NOMBRE ("Ground" / "Suelo") en el proyecto ACTUAL.
        //     Los índices de capa cambian entre proyectos, así que esto mantiene el asset/Inspector correcto
        //     tras un traspaso. Solo sobrescribe el valor si existe una capa de suelo con ese nombre en este proyecto.
        private void ReResolverMascaraDeSuelo()
        {
            if (Cfg == null) return;

            int slotGround = LayerMask.NameToLayer("Ground");
            int slotSuelo = LayerMask.NameToLayer("Suelo");
            LayerMask mask = 0;
            if (slotGround != -1) mask |= 1 << slotGround;
            if (slotSuelo != -1) mask |= 1 << slotSuelo;
            if (mask != 0) Cfg.LayersThatAreConsideredGround = mask;
        }

        private void GuardarJSON(string nombre)
        {
            if (Cfg == null) return;

            ReResolverMascaraDeSuelo();

            string carpeta = ObtenerCarpetaJSON();
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);
            string json = JsonUtility.ToJson(Cfg, true);
            File.WriteAllText(Path.Combine(carpeta, $"{nombre}.json"), json);
            AssetDatabase.Refresh();
        }
        private void CargarJSON(string ruta)
        {
            if (Cfg == null || !File.Exists(ruta)) return;
            Undo.RecordObject(Cfg, "Cargar Configuración JSON");
            JsonUtility.FromJsonOverwrite(File.ReadAllText(ruta), Cfg);
            // En: The JSON stores the raw layer-index bitmask from another project; re-resolve by name here
            //     so the mask points to THIS project's ground layer and the Inspector shows it correctly.
            // Es: El JSON guarda el bitmask por índice de otro proyecto; lo re-resolvemos por nombre aquí
            //     para que la máscara apunte a la capa de suelo de ESTE proyecto y el Inspector la muestre bien.
            ReResolverMascaraDeSuelo();
            EditorUtility.SetDirty(Cfg);
            SO = new SerializedObject(Cfg);
        }

        private void EliminarJSON(string ruta)
        {
            if (!File.Exists(ruta)) return;
            File.Delete(ruta);
            string meta = ruta + ".meta";
            if (File.Exists(meta)) File.Delete(meta);
            AssetDatabase.Refresh();
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region RESET A DEFAULTS  /  RESET TO DEFAULTS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private void ResetearADefaults()
        {
            if (Cfg == null) return;
            Undo.RecordObject(Cfg, "Reset a Defaults");

            Cfg.AllowThePlayerToWalk = true;
            Cfg.AllowThePlayerToRun = true;
            Cfg.AllowThePlayerToRunWhileCrouching = false;
            Cfg.AllowThePlayerToRunWhileProne = false;
            Cfg.AllowThePlayerToMoveInTheAir = false;
            Cfg.BaseSpeedOfThePlayer = 4f;
            Cfg.SpeedMultiplierWhileRunning = 2f;
            Cfg.HoldToRun = true;
            Cfg.EnableOmniDirectionalMovement = true;
            Cfg.ForwardOmnidirectionalMultiplierWhileStanding = 1.0f;
            Cfg.BackwardOmnidirectionalMultiplierWhileStanding = 0.5f;
            Cfg.LateralOmnidirectionalMultiplierWhileStanding = 0.8f;
            Cfg.ForwardOmnidirectionalMultiplierWhileCrouching = 0.65f;
            Cfg.BackwardOmnidirectionalMultiplierWhileCrouching = 0.4f;
            Cfg.LateralOmnidirectionalMultiplierWhileCrouching = 0.5f;
            Cfg.ForwardOmnidirectionalMultiplierWhileProne = 0.2f;
            Cfg.BackwardOmnidirectionalMultiplierWhileProne = 0.1f;
            Cfg.LateralOmnidirectionalMultiplierWhileProne = 0.15f;
            Cfg.ForwardOmnidirectionalMultiplierWhileInTheAir = 0.95f;
            Cfg.BackwardOmnidirectionalMultiplierWhileInTheAir = 0.75f;
            Cfg.LateralOmnidirectionalMultiplierWhileInTheAir = 0.85f;
            Cfg.AllowThePlayerToCrouch = true;
            Cfg.HoldToCrouch = false;
            Cfg.AllowThePlayerToGoProne = true;
            Cfg.HoldToProne = false;
            Cfg.EnableCooldownBetweenBodyStateTransitions = true;
            Cfg.BodyStateTransitionCooldownTime = 0.2f;
            Cfg.SpeedOfTheCapsuleColliderTransition = 8f;
            Cfg.CapsuleColliderHeightWhileStanding = 1.8f;
            Cfg.CapsuleColliderHeightWhileCrouching = 1.2f;
            Cfg.CapsuleColliderHeightWhileProne = 0.5f;
            Cfg.AllowThePlayerToJump = true;
            Cfg.HowManyJumps = 1;
            Cfg.AllowThePlayerToJumpWhileProne = true;
            Cfg.ForceAppliedWhenJumping = 6f;
            Cfg.GravityMultiplierDuringTheJump = 1.5f;
            Cfg.AdditionalGravityMultiplierDuringTheFall = 2.5f;
            Cfg.EnableVariableJump = true;
            Cfg.MaximumHeldTimeOfTheJumpInput = 0.3f;
            Cfg.ExtraForcePerSecondOfTheVariableJump = 15f;
            Cfg.RadiusOfTheGroundDetectionOverlapSphere = 0.25f;
            Cfg.DownwardOffsetOfTheGroundDetectionOverlapSphere = 0.05f;
            Cfg.EnableCoyoteTime = true;
            Cfg.DurationOfTheCoyoteTime = 0.15f;
            Cfg.EnableJumpBuffering = true;
            Cfg.DurationOfTheJumpBuffering = 0.15f;
            Cfg.EnableTheStaminaSystem = true;
            Cfg.MaximumPlayerStamina = 100f;
            Cfg.EnableStaminaCostWhenRunning = true;
            Cfg.StaminaCostPerSecondWhenRunning = 25f;
            Cfg.EnableStaminaCostWhenJumping = true;
            Cfg.StaminaCostWhenJumping = 16f;
            Cfg.EnableStaminaCostWhenCrouching = true;
            Cfg.StaminaCostWhenCrouching = 5f;
            Cfg.EnableStaminaCostWhenGoingProne = true;
            Cfg.StaminaCostWhenGoingProne = 5f;
            Cfg.EnableStaminaCostOnEachPostureTransition = false;
            Cfg.StaminaCostPerPostureTransition = 3f;
            Cfg.EnableExtraStaminaCostWhenJumpingFromProne = false;
            Cfg.ExtraStaminaCostWhenJumpingFromTheProneState = 8f;
            Cfg.DelayInSecondsBeforeStaminaStartsRegenerating = 1f;
            Cfg.StaminaRegenerationSpeedWhileThePlayerIsIdle = 15f;
            Cfg.StaminaRegenerationSpeedWhileThePlayerWalks = 10f;
            Cfg.ShowTheStaminaBarOnTheHUD = true;
            Cfg.EnableTheHeadBobbingSystem = true;
            Cfg.EnableHeadBobbingWhileThePlayerWalks = true;
            Cfg.IntensityOfHeadBobbingWhileThePlayerWalks = 0.08f;
            Cfg.FrequencyOfHeadBobbingWhileThePlayerWalks = 15f;
            Cfg.EnableHeadBobbingWhileThePlayerRuns = true;
            Cfg.IntensityOfHeadBobbingWhileThePlayerRuns = 0.2f;
            Cfg.FrequencyOfHeadBobbingWhileThePlayerRuns = 20f;
            Cfg.EnableReactiveHeadBobbingWhenJumpingAndLanding = true;
            Cfg.IntensityOfReactiveHeadBobbingWhenJumping = 0.08f;
            Cfg.IntensityOfReactiveHeadBobbingWhenLanding = 0.12f;
            Cfg.EnableReactiveHeadBobbingWhenCrouching = true;
            Cfg.IntensityOfReactiveHeadBobbingWhenCrouching = 0.2f;
            Cfg.EnableReactiveHeadBobbingWhenGoingProne = true;
            Cfg.IntensityOfReactiveHeadBobbingWhenGoingProne = 0.1f;
            Cfg.ReturnSpeedOfReactiveHeadBobbingToTheNeutralPosition = 10f;
            Cfg.EnableReactiveHeadBobbingWhenSliding = true;
            Cfg.IntensityOfReactiveHeadBobbingWhenSliding = 0.35f;
            Cfg.EnableReactiveHeadBobbingWhenDashing = true;
            Cfg.IntensityOfReactiveHeadBobbingWhenDashing = 0.5f;
            Cfg.EnableBreathingEffect = true;
            Cfg.BreathsPerMinuteAtRest = 15f;
            Cfg.BreathsPerMinuteWhenExhausted = 30f;
            Cfg.BreathingInhaleFraction = 0.3f;
            Cfg.BreathingBasePitchIntensity = 0.8f;
            Cfg.BreathingExhaustedPitchIntensity = 3f;
            Cfg.BreathingBaseTranslationIntensity = 0.005f;
            Cfg.BreathingExhaustedTranslationIntensity = 0.01f;
            Cfg.BreathingExhaustionFadeInSpeed = 1.8f;
            Cfg.BreathingExhaustionFadeOutSpeed = 0.8f;
            Cfg.BaseFieldOfViewOfTheCamera = 60f;
            Cfg.EnableSpeedFOVEffect = true;
            Cfg.SpeedFOVStartPercent = 0.8f;
            Cfg.SpeedFOVMaxPercent = 1.6f;
            Cfg.SpeedFOVMaxMultiplier = 1.4f;
            Cfg.SpeedFOVTransitionSpeed = 7f;
            Cfg.HorizontalMouseSensitivity = 2f;
            Cfg.VerticalMouseSensitivity = 2f;
            Cfg.UpperVerticalLimitOfTheCamera = 80f;
            Cfg.LowerVerticalLimitOfTheCamera = 80f;
            Cfg.ShowTheCrosshairOnTheHUD = true;
            Cfg.HeightOfTheCameraSupportWhileStanding = 1.6f;
            Cfg.HeightOfTheCameraSupportWhileCrouching = 0.95f;
            Cfg.HeightOfTheCameraSupportWhileProne = 0.4f;
            Cfg.SpeedOfTheCameraHeightTransition = 8f;
            Cfg.EnableTheZoomSystem = true;
            Cfg.HoldToZoom = true;
            Cfg.AllowZoomWhileHoldingAnObject = false;
            Cfg.CameraFieldOfViewDuringZoom = 20f;
            Cfg.SpeedOfTheFieldOfViewTransitionDuringZoom = 8f;
            Cfg.ReduceSensitivityDuringZoom = true;
            Cfg.SensitivityMultiplierDuringZoom = 0.3f;
            Cfg.EnableTheObjectInteractionSystem = true;
            Cfg.TakeIntoAccountTheMassOfTheObjectWhenThrowingIt = false;
            Cfg.TagOfPickableObjects = " ";
            Cfg.MaximumDistanceToPickUpAnObject = 2.5f;
            Cfg.DisableTheObjectColliderWhenPickingItUp = true;
            Cfg.SpeedOfTheObjectMovementTowardsTheAnchorPoint = 15f;
            Cfg.HoldToRotateTheObject = false;
            Cfg.RotationSpeedOfTheObjectInHand = 90f;
            Cfg.MinimumObjectThrowForce = 5f;
            Cfg.MaximumObjectThrowForce = 40f;
            Cfg.MaximumChargeTimeOfTheObjectThrow = 1.5f;
            Cfg.EnableTheSlidingSystem = true;
            Cfg.UseRealPhysicsInSliding = false;
            Cfg.AllowJumpingDuringSliding = false;
            Cfg.AllowInterruptingSliding = true;
            Cfg.RecoveryTimeAfterSliding = 0.4f;
            Cfg.AllowRunningImmediatelyAfterSliding = false;
            Cfg.EnableStaminaCostWhenSliding = true;
            Cfg.StaminaCostWhenSliding = 15f;
            Cfg.AllowSlidingOnAnySurface = true;
            Cfg.DurationOfSlidingInArcadeMode = 0.8f;
            Cfg.InitialSpeedMultiplierOfArcadeSliding = 2.5f;
            Cfg.MinimumSpeedToKeepSliding = 1f;
            Cfg.FrictionDuringPhysicalSliding = 0.3f;
            Cfg.SlopeAccelerationMultiplierDuringSliding = 1.5f;
            Cfg.ReduceCameraSensitivityDuringSliding = true;
            Cfg.CameraSensitivityMultiplierDuringSliding = 0.5f;
            Cfg.EnableTheDashSystem = true;
            Cfg.AllowDashInTheAir = true;
            Cfg.DashForce = 35f;
            Cfg.DurationOfTheDashImpulse = 0.2f;
            Cfg.CooldownBetweenDashUses = 2f;
            Cfg.RequireDoublePressForDashOnGamepad = false;
            Cfg.MaximumTimeBetweenTheTwoPressesForDashDoublePress = 0.3f;
            Cfg.AllowDashDuringSliding = true;
            Cfg.EnableStaminaCostWhenUsingDash = true;
            Cfg.StaminaCostWhenUsingDash = 20f;
            Cfg.KeyboardKeyToMoveForward = KeyCode.W;
            Cfg.KeyboardKeyToMoveBackward = KeyCode.S;
            Cfg.KeyboardKeyToMoveLeft = KeyCode.A;
            Cfg.KeyboardKeyToMoveRight = KeyCode.D;
            Cfg.KeyboardKeyToRun = KeyCode.LeftShift;
            Cfg.KeyboardKeyToJump = KeyCode.Space;
            Cfg.KeyboardKeyToCrouch = KeyCode.C;
            Cfg.KeyboardKeyToGoProne = KeyCode.V;
            Cfg.KeyboardKeyForZoom = KeyCode.Z;
            Cfg.KeyboardKeyToPickUpOrDropAnObject = KeyCode.E;
            Cfg.KeyboardKeyToActivateObjectRotationInHand = KeyCode.R;
            Cfg.KeyboardKeyToRotateObjectUp = KeyCode.UpArrow;
            Cfg.KeyboardKeyToRotateObjectDown = KeyCode.DownArrow;
            Cfg.KeyboardKeyToRotateObjectLeft = KeyCode.LeftArrow;
            Cfg.KeyboardKeyToRotateObjectRight = KeyCode.RightArrow;
            Cfg.MouseButtonToThrowTheObjectInHand = 0;
            Cfg.KeyboardKeyForDash = KeyCode.Q;
            Cfg.LeftStickDeadZone = 0.05f;
            Cfg.RightStickDeadZone = 0.05f;
            Cfg.GamepadButtonToJump = KeyCode.JoystickButton0;
            Cfg.GamepadButtonToCrouchAndGoProne = KeyCode.JoystickButton1;
            Cfg.GamepadButtonToPickUpOrDropAnObject = KeyCode.JoystickButton2;
            Cfg.GamepadButtonToActivateObjectRotationInHand = KeyCode.JoystickButton5;
            Cfg.GamepadButtonForDash = KeyCode.JoystickButton4;
            Cfg.GamepadButtonToRun = KeyCode.JoystickButton8;
            Cfg.GamepadButtonForZoom = KeyCode.JoystickButton9;
            Cfg.GamepadButtonToThrowTheObjectInHand = KeyCode.JoystickButton10;
            Cfg.ButtonHoldTimeToBeConsideredAsProne = 0.4f;

            EditorUtility.SetDirty(Cfg);
            SO = new SerializedObject(Cfg);
        }

        #endregion
        // ════════════════════════════════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════════════════════════════════
        #region HELPERS DE DIBUJO LOCALES  /  LOCAL DRAWING HELPERS
        // ════════════════════════════════════════════════════════════════════════════════════════════

        private float FilaInfo(float y, float ancho, string etiqueta, string valor)
        {
            float anchoUtil = ancho - PAD * 4f;
            GUI.Label(new Rect(PAD * 2f, y, 100f, FILA_H), etiqueta, StLabelSub);
            GUI.Label(new Rect(PAD * 2f + 104f, y, anchoUtil - 104f, FILA_H), valor, StLabel);
            return y + FILA_H + 2f;
        }

        private float BotonLink(float y, float ancho, string texto, string url)
        {
            float anchoUtil = ancho - PAD * 4f;
            if (BotonColoreado(new Rect(PAD * 2f, y, anchoUtil, 24f), texto, C_AZUL))
                Application.OpenURL(url);
            return y + 28f;
        }
    }

    #endregion
    // ════════════════════════════════════════════════════════════════════════════════════════════


    [UnityEditor.CustomEditor(typeof(PlayerConfiguration_ConfiguracionDelJugador))]
    public class FPC_CPPMiniEditor : UnityEditor.Editor
    {
        private const string PREF_IDIOMA = "CPPFPC_Idioma";

        private static Font _fuente;
        private static bool _fuenteBuscada = false;

        private static Font ObtenerFuente()
        {
            if (_fuenteBuscada) return _fuente;
            _fuenteBuscada = true;
            Font[] fuentes = Resources.LoadAll<Font>("Dico");
            if (fuentes != null && fuentes.Length > 0) { _fuente = fuentes[0]; return _fuente; }
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Font", new[] { "Assets/48A-01-FPC_CPP" });
            if (guids.Length > 0)
                _fuente = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
#endif
            return _fuente;
        }

        public override void OnInspectorGUI()
        {
            bool es = EditorPrefs.GetBool(PREF_IDIOMA, true);
            Font fnt = ObtenerFuente();

            GUIStyle stTitulo = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                normal = { textColor = Color.white },
                font = fnt
            };
            GUIStyle stSub = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.87f, 0.92f, 1f) },
                font = fnt
            };
            GUIStyle stIdioma = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.80f, 0.82f, 0.88f, 1f) },
                font = fnt
            };
            GUIStyle stBoton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                normal = { textColor = Color.white },
                font = fnt
            };
            GUIStyle stBotonIdioma = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                normal = { textColor = Color.white },
                font = fnt
            };

            EditorGUI.DrawRect(new Rect(0, 0, Screen.width, 260), new Color(0.06f, 0.06f, 0.07f, 1f));
            EditorGUI.DrawRect(new Rect(2f, 258f, Screen.width - 4f, 1f), new Color(0.95f, 0.95f, 1f, 1f));
            GUILayout.Space(14);

            GUILayout.Label(es ? " CPP - Controlador Primera Persona" : " FPC - First Person Controller", stTitulo);

            GUILayout.Space(6);
            GUILayout.Label(es ? "Abre la Window Inspector para configurar este asset." : "Open the full Window Inspector to configure this asset.", stSub);

            GUILayout.Space(10);

            Color ca = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.45f, 0.85f, 1f);
            if (GUILayout.Button(es ? "Abrir la Window Inspector del FPC CPP " : "Open the Window Inspector for the FPC CPP", stBoton, GUILayout.Height(32)))
            {
                FPC_CPP_Window.AbrirConAsset((PlayerConfiguration_ConfiguracionDelJugador)target);
            }
            GUI.backgroundColor = ca;

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.Label(es ? "Idioma:" : "Language:", stIdioma, GUILayout.Width(65));
            Color cEs = GUI.backgroundColor;
            GUI.backgroundColor = es ? new Color(0.12f, 0.38f, 0.70f, 1f) : new Color(0.15f, 0.15f, 0.18f, 1f);
            if (GUILayout.Button("Español", stBotonIdioma, GUILayout.Height(22)))
                EditorPrefs.SetBool(PREF_IDIOMA, true);
            GUI.backgroundColor = !es ? new Color(0.12f, 0.38f, 0.70f, 1f) : new Color(0.15f, 0.15f, 0.18f, 1f);
            if (GUILayout.Button("English", stBotonIdioma, GUILayout.Height(22)))
                EditorPrefs.SetBool(PREF_IDIOMA, false);
            GUI.backgroundColor = cEs;
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
        }
    }
}