using HMUI;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static EnvironmentSizeData;

namespace NotesPredictor
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class NotesPredictorController : MonoBehaviour
    {
        public static NotesPredictorController Instance { get; private set; }
        private CurvedTextMeshPro textMesh = new GameObject("Text").AddComponent<CurvedTextMeshPro>();
        //GameObject blueParent = new GameObject("blueParent");
        //GameObject redParent = new GameObject("redParent");

        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (Instance != null)
            {
                Plugin.Log?.Warn($"Instance of {GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            Instance = this;
            Plugin.Log?.Debug($"{name}: Awake()");
        }
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {


            //text
            gameObject.AddComponent<Canvas>();
            textMesh.transform.SetParent(transform);
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.transform.eulerAngles = new Vector3(0, 0, 0);
            textMesh.transform.position = new Vector3(0, 2f, 3f);
            textMesh.color = Color.white;
            textMesh.fontSize = 0.2f;
            textMesh.text = "";

            StartCoroutine(InitCoroutine());

            IEnumerator InitCoroutine()
            {
                Material noGlowMat = null;
                while (noGlowMat == null)
                {
                    noGlowMat = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlow").FirstOrDefault();
                    yield return null;
                }

                gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<CurvedCanvasSettings>().SetRadius(0f);

                GameObject pair = new GameObject("pair");
                pair.transform.SetParent(transform);
                pair.transform.localPosition = new Vector3(0, .5f, -1f);

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Custom/SimpleLit"));
                renderer.material.color = Color.magenta;
                cube.name = "cube";
                cube.transform.SetParent(pair.transform);
                cube.transform.localPosition = new Vector3(0, 0, 0);
                cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                cube.SetActive(true);
                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                renderer = stick.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Custom/SimpleLit"));
                renderer.material.color = Color.white;
                stick.name = "stick";
                stick.transform.SetParent(pair.transform);
                stick.transform.localPosition = new Vector3(0, .27f, 0);
                stick.transform.localScale = new Vector3(0.01f, .4f, 0.01f);
                stick.SetActive(true);


                SceneManager.activeSceneChanged += (Scene _, Scene next) =>
                {
                    if (next.name == "GameCore")
                    {
                        Plugin.Log.Debug("GameCore Scene Started");
                        GameObject blueParent = new GameObject("blueParent");
                        GameObject redParent = new GameObject("redParent");
                        Vector3 bluePos = new Vector3(.3f, 1.8f, 1.5f);
                        Vector3 redPos = new Vector3(-.3f, 1.8f, 1.5f);
                        StartCoroutine(GameCoreCoroutine());
                        IEnumerator GameCoreCoroutine()
                        {
                            while (true)
                            {
                                PauseController pauseController = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault(x => x.isActiveAndEnabled);
                                BeatmapObjectManager beatmapObjectManager = pauseController?.GetField<BeatmapObjectManager, PauseController>("_beatmapObjectManager");
                                if (beatmapObjectManager != null)
                                {
                                    beatmapObjectManager.noteWasSpawnedEvent += (NoteController noteController) =>
                                    {
                                        GameObject notePair = Instantiate(pair);
                                        GameObject noteCube = notePair.transform.GetChild(0).gameObject;
                                        GameObject noteStick = notePair.transform.GetChild(1).gameObject;
                                        float x = 0;
                                        switch ((int)noteController.noteData.lineIndex)
                                        {
                                            case 0:
                                                x = -0.9f;
                                                break;
                                            case 1:
                                                x = -0.3f;
                                                break;
                                            case 2:
                                                x = 0.3f;
                                                break;
                                            case 3:
                                                x = 0.9f;
                                                break;
                                            default:
                                                Destroy(notePair);
                                                break;
                                        }
                                        float y = 0;
                                        switch (noteController.noteData.noteLineLayer)
                                        {
                                            case NoteLineLayer.Base:
                                                y = 0.6f;
                                                break;
                                            case NoteLineLayer.Upper:
                                                y = 1.2f;
                                                break;
                                            case NoteLineLayer.Top:
                                                y = 1.8f;
                                                break;
                                            default:
                                                Destroy(notePair);
                                                break;
                                        }
                                        //Plugin.Log.Debug("x" + x.ToString());
                                        //Plugin.Log.Debug("y" + y.ToString());
                                        float rz = 0;
                                        switch (noteController.noteData.cutDirection)
                                        {
                                            case NoteCutDirection.Down:
                                                rz = 0;
                                                break;
                                            case NoteCutDirection.Up:
                                                rz = 180;
                                                break;
                                            case NoteCutDirection.Right:
                                                rz = 90;
                                                break;
                                            case NoteCutDirection.Left:
                                                rz = -90;
                                                break;
                                            case NoteCutDirection.DownLeft:
                                                rz = -45;
                                                break;
                                            case NoteCutDirection.DownRight:
                                                rz = 45;
                                                break;
                                            case NoteCutDirection.UpLeft:
                                                rz = -135;
                                                break;
                                            case NoteCutDirection.UpRight:
                                                rz = 135;
                                                break;
                                            case NoteCutDirection.Any:
                                                Vector3 old_pos = new Vector3(0, 0, 0);
                                                switch (noteController.noteData.colorType)
                                                {
                                                    case ColorType.ColorA:
                                                        old_pos = redPos;
                                                        break;
                                                    case ColorType.ColorB:
                                                        old_pos = bluePos;
                                                        break;
                                                    default:
                                                        Destroy(notePair);
                                                        break;
                                                }
                                                Vector3 from = new Vector3(0, 1f, 0);
                                                Vector3 to = old_pos - new Vector3(x, y, 1.5f);
                                                Vector3 axis = new Vector3(0, 0, 1f);
                                                float Angle = Vector3.SignedAngle(from, to, axis);
                                                rz = Angle;
                                                break;
                                            default:
                                                noteStick = notePair.transform.GetChild(1).gameObject;
                                                noteStick.SetActive(false);
                                                rz = 0;
                                                break;
                                        }
                                        switch (noteController.noteData.colorType)
                                        {
                                            case ColorType.ColorA:
                                                noteCube.GetComponent<Renderer>().material.color = Color.red;
                                                noteStick.GetComponent<Renderer>().material.color = new Color(1f, .3f, .3f);
                                                break;
                                            case ColorType.ColorB:
                                                noteCube.GetComponent<Renderer>().material.color = Color.blue;
                                                noteStick.GetComponent<Renderer>().material.color = new Color(.3f, .3f, 1f);
                                                break;
                                            default:
                                                Destroy(notePair);
                                                break;
                                        }
                                        noteCube.transform.localPosition = new Vector3(0, 0, 0);
                                        noteCube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                                        noteStick.transform.localPosition = new Vector3(0, .4f, 0);
                                        noteStick.transform.localScale = new Vector3(0.02f, .4f, 0.02f);
                                        notePair.transform.position = new Vector3(x, y, 1.5f);
                                        notePair.transform.eulerAngles = new Vector3(0, 0, rz);
                                        switch (noteController.noteData.colorType)
                                        {
                                            case ColorType.ColorA:
                                                noteCube.GetComponent<Renderer>().material.color = Color.red;
                                                noteStick.GetComponent<Renderer>().material.color = new Color(1f, .3f, .3f);
                                                notePair.transform.SetParent(redParent.transform);
                                                redPos = notePair.transform.position;
                                                break;
                                            case ColorType.ColorB:
                                                noteCube.GetComponent<Renderer>().material.color = Color.blue;
                                                noteStick.GetComponent<Renderer>().material.color = new Color(.3f, .3f, 1f);
                                                notePair.transform.SetParent(blueParent.transform);
                                                bluePos = notePair.transform.position;
                                                break;
                                            default:
                                                Destroy(noteCube);
                                                break;
                                        }
                                        notePair.SetActive(false);
                                        Destroy(notePair, 1.25f);
                                    };
                                    break;
                                }
                                yield return null;
                            }
                        }
                    }
                };
            }

        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {
            //Plugin.Log.Debug("Update run");
            GameObject blueParent = GameObject.Find("blueParent");
            if (blueParent != null)
            {
                //Plugin.Log.Debug("blueParent name is " + blueParent.name);
                int blueCount = blueParent.transform.childCount;
                GameObject[] blueChildren = new GameObject[blueCount];
                for (int i = 0; i < blueCount; i++)
                {
                    GameObject notePair = blueParent.transform.GetChild(i).gameObject;
                    GameObject noteCube = notePair.transform.GetChild(0).gameObject;
                    Vector3 nowscale = noteCube.transform.localScale;
                    nowscale *= 1.04f;
                    noteCube.transform.localScale = nowscale;
                    if (i < 2)
                    {
                        notePair.SetActive(true);
                    }
                    else
                    {
                        notePair.SetActive(false);
                    }
                }
            }
            GameObject redParent = GameObject.Find("redParent");
            if (redParent != null)
            {
                //Plugin.Log.Debug("redParent name is " + redParent.name);
                int redCount = redParent.transform.childCount;
                GameObject[] redChildren = new GameObject[redCount];
                for (int i = 0; i < redParent.transform.childCount; i++)
                {
                    GameObject notePair = redParent.transform.GetChild(i).gameObject;
                    GameObject noteCube = notePair.transform.GetChild(0).gameObject;
                    Vector3 nowscale = noteCube.transform.localScale;
                    nowscale *= 1.04f;
                    noteCube.transform.localScale = nowscale;
                    if (i < 2)
                    {
                        notePair.SetActive(true);
                    }
                    else
                    {
                        notePair.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {
        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            if (Instance == this)
                Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion
    }
}
