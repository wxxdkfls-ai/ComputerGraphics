using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MuseumOfLies.Common
{
    /// <summary>
    /// Scene change with a short fade (≈0.4 s) on a black quad in front of the eyes.
    /// No camera movement — the view only darkens and comes back (comfort guideline).
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public float fadeSeconds = 0.4f;
        static SceneTransition _instance;
        Material _mat;
        Transform _quad;
        float _alpha;

        public static bool CanLoad(string scene) => Application.CanStreamedLevelBeLoaded(scene);

        public static void Load(string scene)
        {
            if (_instance == null)
            {
                var go = new GameObject("[SceneTransition]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SceneTransition>();
            }
            _instance.StartCoroutine(_instance.Run(scene));
        }

        IEnumerator Run(string scene)
        {
            yield return Fade(0f, 1f);
            var op = SceneManager.LoadSceneAsync(scene);
            while (!op.isDone) yield return null;
            yield return null; // let the new rig/camera initialise
            yield return Fade(1f, 0f);
        }

        IEnumerator Fade(float from, float to)
        {
            for (float t = 0; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                SetAlpha(Mathf.Lerp(from, to, t / fadeSeconds));
                yield return null;
            }
            SetAlpha(to);
        }

        void SetAlpha(float a)
        {
            _alpha = a;
            EnsureQuad();
            if (_quad == null) return;
            _quad.gameObject.SetActive(a > 0.001f);
            _mat.SetColor("_BaseColor", new Color(0, 0, 0, a));
        }

        void EnsureQuad()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (_quad != null && _quad.parent == cam.transform) return;
            if (_mat == null)
            {
                _mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                _mat.SetFloat("_Surface", 1f);            // transparent
                _mat.SetFloat("_Blend", 0f);
                _mat.SetFloat("_ZWrite", 0f);
                _mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                _mat.renderQueue = 4000;
                _mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
            }
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(q.GetComponent<Collider>());
            q.name = "FadeQuad";
            q.GetComponent<Renderer>().sharedMaterial = _mat;
            q.transform.SetParent(cam.transform, false);
            q.transform.localPosition = new Vector3(0, 0, Mathf.Max(cam.nearClipPlane * 1.5f, 0.06f));
            q.transform.localScale = Vector3.one * 2f;
            _quad = q.transform;
        }
    }
}
