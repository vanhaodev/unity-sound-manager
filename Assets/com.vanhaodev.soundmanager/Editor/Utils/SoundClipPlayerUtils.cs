using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
{
    public class SoundClipPlayerUtils
    {
        private const float UPDATE_INTERVAL = 0.4f;

        private AudioSource _previewSource;
        private bool _isPlaying = false;
        private AudioClip _currentClip;
        private SoundClipSO _so;

        private bool _isDragging = false;
        private double _lastUpdateTime;

        private Editor _editor;

        public void SetSO(SoundClipSO so)
        {
            if (_so == so) return;
            _so = so;

            if (_previewSource == null)
            {
                GameObject go = GameObject.Find("__EditorAudioPreview__");
                if (go == null)
                {
                    go = new GameObject("__EditorAudioPreview__");
                    go.hideFlags = HideFlags.HideAndDontSave;
                }

                _previewSource = go.GetComponent<AudioSource>();
                if (_previewSource == null)
                    _previewSource = go.AddComponent<AudioSource>();

                _previewSource.playOnAwake = false;
                _previewSource.loop = false;
            }

            UpdateClip(); // load lần đầu
        }

        public void SetRepaintTarget(Editor editor)
        {
            _editor = editor;
        }

        public void OnGUI()
        {
            if (_so == null)
                return;

            if (_previewSource != null)
                _previewSource.volume = _so.Volume;
            

            // Background container
            Rect bgRect = EditorGUILayout.BeginVertical(GUI.skin.box);
    
            // Custom darker background
            EditorGUI.DrawRect(bgRect, new Color(0.15f, 0.15f, 0.15f));
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (_currentClip == null)
            {
                EditorGUILayout.HelpBox("Clip not loaded. Press Refresh.", MessageType.Info);
                DrawRefreshButton();

                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                return;
            }

            DrawControls();
            DrawProgress();
            DrawRefreshButton();

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Back", GUILayout.Width(60)))
            {
                if (_previewSource.clip != null)
                    _previewSource.time = Mathf.Max(0f, _previewSource.time - 5f);
            }

            if (!_isPlaying && GUILayout.Button("Play", GUILayout.Width(70)))
            {
                if (_currentClip == null) return;

                _previewSource.clip = _currentClip;

                if (_previewSource.time >= _currentClip.length - 0.05f)
                {
                    _previewSource.time = 0f;
                }

                _previewSource.Play();

                _isPlaying = true;
                _lastUpdateTime = 0;
                EditorApplication.update += Update;
            }
            else if (_isPlaying && GUILayout.Button("Pause", GUILayout.Width(70)))
            {
                _previewSource.Pause();
                _isPlaying = false;
                EditorApplication.update -= Update;
            }

            if (GUILayout.Button("Forward", GUILayout.Width(70)))
            {
                if (_previewSource.clip != null)
                {
                    float maxTime = _previewSource.clip.length - 0.01f;
                    _previewSource.time = Mathf.Min(maxTime, _previewSource.time + 5f);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProgress()
        {
            if (_previewSource == null || _previewSource.clip == null)
                return;

            float length = _previewSource.clip.length;
            float currentTime = _previewSource.time;

            SoundClipPlayerGUI.DrawTimeBar(currentTime, length);

            Rect rect = GUILayoutUtility.GetRect(1, 9, GUILayout.ExpandWidth(true));

            float progress = length > 0 ? currentTime / length : 0f;

            SoundClipPlayerGUI.DrawProgressBar(rect, progress);

            HandleSeek(rect, length);
        }

        private void HandleSeek(Rect rect, float length)
        {
            Event e = Event.current;

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && rect.Contains(e.mousePosition))
            {
                float t = Mathf.Clamp01((e.mousePosition.x - rect.x) / rect.width);
                float newTime = t * length;

                if (_previewSource.clip != null &&
                    Mathf.Abs(_previewSource.time - newTime) > 0.05f)
                {
                    _previewSource.time = newTime;
                }

                _isDragging = true;
                e.Use();
            }

            if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
        }

        private void DrawRefreshButton()
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh Clip", GUILayout.Width(120)))
            {
                UpdateClip();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void UpdateClip()
        {
            if (_previewSource == null) return;

            if (_previewSource.isPlaying)
                _previewSource.Stop();

            _currentClip = _so.Clip;
            _previewSource.clip = _currentClip;

            if (_currentClip != null && _currentClip.length > 0f)
                _previewSource.time = 0f;

            _isPlaying = false;
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (!_isPlaying || _previewSource == null)
                return;

            if (!_previewSource.isPlaying)
            {
                _isPlaying = false;
                _previewSource.time = 0f;
                EditorApplication.update -= Update;
                return;
            }

            double time = EditorApplication.timeSinceStartup;

            if (!_isDragging && time - _lastUpdateTime < UPDATE_INTERVAL)
                return;

            _lastUpdateTime = time;

            // ✅ chỉ repaint inspector hiện tại
            if (_editor != null && _editor.target != null)
            {
                _editor.Repaint();
            }
        }

        public void Dispose()
        {
            if (_previewSource != null)
            {
                _previewSource.Stop();
                Object.DestroyImmediate(_previewSource.gameObject);
                _previewSource = null;
            }

            _isPlaying = false;
            EditorApplication.update -= Update;
        }
    }

    public static class SoundClipPlayerGUI
    {
        public static void DrawProgressBar(Rect rect, float progress)
        {
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

            Rect fill = new Rect(rect.x, rect.y, rect.width * progress, rect.height);
            EditorGUI.DrawRect(fill, new Color(0.3f, 0.7f, 1f));
        }

        public static void DrawTimeBar(float current, float max)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(FormatTime(current), GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            GUILayout.Label(FormatTime(max), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        private static string FormatTime(float time)
        {
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);
            return $"{min:00}:{sec:00}";
        }
    }
}