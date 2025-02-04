using System.Collections.Generic;
using UnityEngine;

namespace Dennis.Tools
{
#if UNITY_EDITOR
    using UnityEditor;

    public class SelectionTrackerWindow : EditorWindow
    {
        #region Fields

        private static List<Object> SelectionHistory = new List<Object>();
        private static int CurrentIndex = -1;

        private static int MaxHistoryCount = 20;

        private Vector2 _scrollPosition;
        private static Texture2D SelectedIcon;
        private static readonly Color SelectedColor = new Color(0.3f, 0.6f, 1.0f, 1.0f);

        #endregion

        #region Unity Lifecycle

        [MenuItem("Tools/Selection Tracker Window")]
        public static void ShowWindow()
        {
            GetWindow<SelectionTrackerWindow>("Selection Tracker Window");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            SelectedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Image/selected_icon.png");
            if (SelectedIcon == null)
            {
                Debug.LogWarning("SelectedIcon not found. Please check the path.");
            }
        }

        private void OnDisable()
        {
            ClearHistory();
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawHistoryNavigation();
            DrawSelectionHistory();
        }

        #endregion

        #region UI Drawing

        private void DrawHeader()
        {
            GUILayout.Space(10);
            GUILayout.Label("Selection Tracker", EditorStyles.boldLabel);
            GUILayout.Label("This tool helps track your object selection history.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);

            MaxHistoryCount = EditorGUILayout.IntField("Max History Count:", MaxHistoryCount);
            MaxHistoryCount = Mathf.Clamp(MaxHistoryCount, 1, 100);
            GUILayout.Space(10);
        }

        private void DrawHistoryNavigation()
        {
            EditorGUILayout.BeginHorizontal();

            DrawNavigationButton("Back", NavigateBackward, () => CurrentIndex > 0);
            DrawNavigationButton("Forward", NavigateForward, () => CurrentIndex < SelectionHistory.Count - 1);

            GUILayout.FlexibleSpace();

            DrawNavigationButton("Clear", ClearHistory, () => SelectionHistory.Count > 0);

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void DrawSelectionHistory()
        {
            GUILayout.Label("Selection History", EditorStyles.boldLabel);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < SelectionHistory.Count; i++)
            {
                DrawSelectionItem(i);
            }

            GUILayout.EndScrollView();
        }

        private void DrawSelectionItem(int index)
        {
            string itemName = SelectionHistory[index]?.name ?? "Unnamed";
            bool isSelected = (index == CurrentIndex);

            GUIStyle itemStyle = isSelected ? EditorStyles.label : EditorStyles.miniButton;
            Color defaultColor = GUI.color;

            EditorGUILayout.BeginHorizontal();

            if (isSelected)
            {
                GUILayout.Label(SelectedIcon, GUILayout.Width(20), GUILayout.Height(20));
                GUILayout.Label($" {itemName} ", itemStyle);
            }
            else
            {
                if (GUILayout.Button($" {itemName} ", itemStyle))
                {
                    SetCurrentSelection(index);
                }
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = defaultColor;
        }

        private void DrawNavigationButton(string label, System.Action onClick, System.Func<bool> isEnabled)
        {
            using (new EditorGUI.DisabledScope(!isEnabled()))
            {
                if (GUILayout.Button(label, GUILayout.Height(30)))
                {
                    onClick.Invoke();
                }
            }
        }

        #endregion

        #region Selection Management

        private static void OnSelectionChanged()
        {
            if (Selection.activeObject == null) return;
            if (CurrentIndex >= 0 && SelectionHistory[CurrentIndex] == Selection.activeObject) return;

            if (CurrentIndex < SelectionHistory.Count - 1)
            {
                SelectionHistory.RemoveRange(CurrentIndex + 1, SelectionHistory.Count - CurrentIndex - 1);
            }

            SelectionHistory.Add(Selection.activeObject);

            if (SelectionHistory.Count > MaxHistoryCount)
            {
                SelectionHistory.RemoveAt(0);
            }

            CurrentIndex = SelectionHistory.Count - 1;
            EditorWindowUtils.GetWindowWithoutFocus<SelectionTrackerWindow>()?.Repaint();
        }

        private static void SetCurrentSelection(int index)
        {
            if (SelectionHistory == null || SelectionHistory.Count == 0) return;
            if (index < 0 || index >= SelectionHistory.Count) return;

            CurrentIndex = index;
            FocusOnProjectView(SelectionHistory[CurrentIndex]);
        }

        private static void ClearHistory()
        {
            SelectionHistory.Clear();
            CurrentIndex = -1;
        }

        #endregion

        #region Navigation Methods

        private static void NavigateBackward()
        {
            if (CurrentIndex <= 0) return;

            CurrentIndex--;
            FocusOnProjectView(SelectionHistory[CurrentIndex]);
        }

        private static void NavigateForward()
        {
            if (CurrentIndex >= SelectionHistory.Count - 1) return;

            CurrentIndex++;
            FocusOnProjectView(SelectionHistory[CurrentIndex]);
        }

        #endregion

        #region Utility Methods

        private static void FocusOnProjectView(Object obj)
        {
            if (obj == null || Selection.activeObject == obj) return;

            Selection.activeObject = obj;
            EditorApplication.ExecuteMenuItem("Window/General/Project");
        }

        #endregion
    }
#endif
}
