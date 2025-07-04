﻿/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System.Linq;
using UnityEngine;


namespace PluginMaster
{
    public class ToolProperties : UnityEditor.EditorWindow
    {
        #region COMMON
        private const string UNDO_MSG = "Tool properties";
        private Vector2 _mainScrollPosition = Vector2.zero;
        private GUIContent _updateButtonContent = null;
        private static ToolProperties _instance = null;

        [UnityEditor.MenuItem("Tools/Plugin Master/Prefab World Builder/Tool Properties...", false, 1130)]
        public static void ShowWindow() => _instance = GetWindow<ToolProperties>("Tool Properties");

        public static void RepainWindow()
        {
            if (_instance != null) _instance.Repaint();
        }

        public static void CloseWindow()
        {
            if (_instance != null) _instance.Close();
        }

        private void OnEnable()
        {
            if (BrushManager.settings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
            _updateButtonContent
                = new GUIContent(Resources.Load<Texture2D>("Sprites/Update"), "Update Temp Colliders");
            UnityEditor.Undo.undoRedoPerformed += Repaint;
        }

        private void OnDisable()
        {
            PWBCore.DestroyTempColliders();
            UnityEditor.Undo.undoRedoPerformed -= Repaint;
        }

        private void OnGUI()
        {
            if (_instance == null) _instance = this;
            using (var scrollView = new UnityEditor.EditorGUILayout.ScrollViewScope(_mainScrollPosition,
                false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUIStyle.none))
            {
                _mainScrollPosition = scrollView.scrollPosition;
#if UNITY_2021_2_OR_NEWER
#else
                if (PWBToolbar.instance == null) PWBToolbar.ShowWindow();
#endif
                if (ToolManager.tool == ToolManager.PaintTool.PIN) PinGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.BRUSH) BrushGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.ERASER) EraserGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.GRAVITY) GravityGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.EXTRUDE) ExtrudeGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.LINE) LineGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.SHAPE) ShapeGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.TILING) TilingGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.SELECTION) SelectionGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.CIRCLE_SELECT) CircleSelectGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.MIRROR) MirrorGroup();
                else if (ToolManager.tool == ToolManager.PaintTool.REPLACER) ReplacerGroup();
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }
        public static void ClearUndo()
        {
            if (_instance == null) return;
            UnityEditor.Undo.ClearUndo(_instance);
        }
        #endregion

        #region UNDO
        [SerializeField] private LineData _lineData = LineData.instance;
        [SerializeField] private TilingData _tilingData = TilingData.instance;
        [SerializeField] private MirrorSettings _mirrorSettings = MirrorManager.settings;
        [SerializeField] private ShapeData _shapeData = ShapeData.instance;
        [SerializeField] private TilingManager _tilingManager = TilingManager.instance as TilingManager;
        [SerializeField] private ShapeManager _shapeManager = ShapeManager.instance as ShapeManager;
        [SerializeField] private LineManager _lineManager = LineManager.instance as LineManager;
        public static void RegisterUndo(string commandName)
        {
            if (_instance != null) UnityEditor.Undo.RegisterCompleteObjectUndo(_instance, commandName);
        }
        #endregion

        #region TOOL PROFILE
        public class ProfileData
        {
            public readonly IToolManager toolManager = null;
            public readonly string profileName = string.Empty;
            public ProfileData(IToolManager toolManager, string profileName)
                => (this.toolManager, this.profileName) = (toolManager, profileName);
        }
        private void ToolProfileGUI(IToolManager toolManager)
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.helpBox))
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Tool Profile:");
                if (GUILayout.Button(toolManager.selectedProfileName,
                    UnityEditor.EditorStyles.popup, GUILayout.MinWidth(100)))
                {
                    GUI.FocusControl(null);
                    var menu = new UnityEditor.GenericMenu();
                    foreach (var profileName in toolManager.profileNames)
                        menu.AddItem(new GUIContent(profileName), profileName == toolManager.selectedProfileName,
                            SelectProfileItem, new ProfileData(toolManager, profileName));
                    menu.AddSeparator(string.Empty);
                    if (toolManager.selectedProfileName != ToolProfile.DEFAULT) menu.AddItem(new GUIContent("Save"),
                        false, SaveProfile, toolManager);
                    menu.AddItem(new GUIContent("Save As..."), false, SaveProfileAs,
                        new ProfileData(toolManager, toolManager.selectedProfileName));
                    if (toolManager.selectedProfileName != ToolProfile.DEFAULT)
                        menu.AddItem(new GUIContent("Delete Selected Profile"), false, DeleteProfile,
                            new ProfileData(toolManager, toolManager.selectedProfileName));
                    menu.AddItem(new GUIContent("Revert Selected Profile"), false, RevertProfile, toolManager);
                    menu.AddItem(new GUIContent("Factory Reset Selected Profile"), false,
                        FactoryResetProfile, toolManager);
                    menu.ShowAsContext();
                }
            }
        }

        private void SelectProfile(ProfileData profileData)
        {

            GUI.FocusControl(null);
            profileData.toolManager.selectedProfileName = profileData.profileName;
            Repaint();
            if (ToolManager.tool == ToolManager.PaintTool.MIRROR)
                UnityEditor.SceneView.lastActiveSceneView.LookAt(MirrorManager.settings.mirrorPosition);
            else if (ToolManager.tool == ToolManager.PaintTool.LINE)
                LineManager.settings.OnDataChanged();
            UnityEditor.SceneView.RepaintAll();
        }

        private void SelectProfileItem(object value) => SelectProfile(value as ProfileData);

        private void SaveProfile(object value)
        {

            var manager = value as IToolManager;
            manager.SaveProfile();
        }

        private void SaveProfileAs(object value)
        {
            var profiledata = value as ProfileData;
            SaveProfileWindow.ShowWindow(profiledata, OnSaveProfileDone);
        }

        private void OnSaveProfileDone(IToolManager toolManager, string profileName)
        {

            toolManager.SaveProfileAs(profileName);
            Repaint();
        }
        private class SaveProfileWindow : UnityEditor.EditorWindow
        {
            private IToolManager _toolManager = null;
            private string _profileName = string.Empty;
            private System.Action<IToolManager, string> OnDone;

            public static void ShowWindow(ProfileData data, System.Action<IToolManager, string> OnDone)
            {
                var window = GetWindow<SaveProfileWindow>(true, "Save Profile");
                window._toolManager = data.toolManager;
                window._profileName = data.profileName;
                window.OnDone = OnDone;
                window.minSize = window.maxSize = new Vector2(160, 50);
                UnityEditor.EditorGUIUtility.labelWidth = 70;
                UnityEditor.EditorGUIUtility.fieldWidth = 70;
            }

            private void OnGUI()
            {
                const string textFieldName = "NewProfileName";
                GUI.SetNextControlName(textFieldName);
                _profileName = UnityEditor.EditorGUILayout.TextField(_profileName).Trim();
                GUI.FocusControl(textFieldName);
                using (new UnityEditor.EditorGUI.DisabledGroupScope(_profileName == string.Empty))
                {
                    if (GUILayout.Button("Save"))
                    {
                        OnDone(_toolManager, _profileName);
                        Close();
                    }
                }
            }
        }

        private void DeleteProfile(object value)
        {

            var profiledata = value as ProfileData;
            profiledata.toolManager.DeleteProfile();
            if (ToolManager.tool == ToolManager.PaintTool.MIRROR)
                UnityEditor.SceneView.lastActiveSceneView.LookAt(MirrorManager.settings.mirrorPosition);
        }
        private void RevertProfile(object value)
        {

            var manager = value as IToolManager;
            manager.Revert();
            if (ToolManager.tool == ToolManager.PaintTool.MIRROR)
                UnityEditor.SceneView.lastActiveSceneView.LookAt(MirrorManager.settings.mirrorPosition);
        }
        private void FactoryResetProfile(object value)
        {

            var manager = value as IToolManager;
            manager.FactoryReset();
            if (ToolManager.tool == ToolManager.PaintTool.MIRROR)
                UnityEditor.SceneView.lastActiveSceneView.LookAt(MirrorManager.settings.mirrorPosition);
        }
        #endregion

        #region COMMON PAINT SETTINGS
        private static float _maxRadius = 50f;
        private static Vector3[] _dir =
        {
            Vector3.right, Vector3.left,
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back
        };
        private static string[] _dirNames = new string[] { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };

        private static readonly string[] _brushShapeOptions = { "Point", "Circle", "Square" };
        private static readonly string[] _spacingOptions = { "Auto", "Custom" };
        private void PaintSettingsGUI(IPaintOnSurfaceToolSettings paintOnSurfaceSettings,
            IPaintToolSettings paintSettings)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                void UpdateTempColliders()
                {
                    if (paintOnSurfaceSettings.paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
                    else PWBCore.DestroyTempColliders();
                }
                using (new UnityEditor.EditorGUI.DisabledGroupScope
                    (PWBCore.staticData.tempCollidersAction == PWBData.TempCollidersAction.NEVER_CREATE))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 150;
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var paintOnMeshesWithoutCollider
                                = UnityEditor.EditorGUILayout.ToggleLeft("Paint on meshes without collider",
                                paintOnSurfaceSettings.paintOnMeshesWithoutCollider);
                            if (check.changed)
                            {
                                paintOnSurfaceSettings.paintOnMeshesWithoutCollider = paintOnMeshesWithoutCollider;
                                UpdateTempColliders();
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }
                        using (new UnityEditor.EditorGUI.DisabledGroupScope
                            (!paintOnSurfaceSettings.paintOnMeshesWithoutCollider))
                            if (GUILayout.Button(_updateButtonContent, GUILayout.Width(21), GUILayout.Height(21)))
                                PWBCore.UpdateTempColliders();
                    }
                }
                UnityEditor.EditorGUIUtility.labelWidth = 110;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var paintOnPalettePrefabs = UnityEditor.EditorGUILayout.ToggleLeft("Paint on palette prefabs",
                        paintOnSurfaceSettings.paintOnPalettePrefabs);
                    var paintOnSelectedOnly = UnityEditor.EditorGUILayout.ToggleLeft("Paint on selected only",
                        paintOnSurfaceSettings.paintOnSelectedOnly);
                    if (check.changed)
                    {
                        paintOnSurfaceSettings.paintOnPalettePrefabs = paintOnPalettePrefabs;
                        paintOnSurfaceSettings.paintOnSelectedOnly = paintOnSelectedOnly;
                        UpdateTempColliders();
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            }
            PaintToolSettingsGUI(paintSettings);
        }

        private void ParentSettingsGUI(IPaintToolSettings paintSettings)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var autoCreateParent
                        = UnityEditor.EditorGUILayout.ToggleLeft("Create parent", paintSettings.autoCreateParent);
                    if (check.changed)
                    {
                        paintSettings.autoCreateParent = autoCreateParent;
                    }
                }
                if (!paintSettings.autoCreateParent)
                {
                    paintSettings.setSurfaceAsParent = UnityEditor.EditorGUILayout.ToggleLeft("Set surface as parent",
                        paintSettings.setSurfaceAsParent);
                    if (!paintSettings.setSurfaceAsParent)
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var parent = (Transform)UnityEditor.EditorGUILayout.ObjectField("Parent Transform:",
                                paintSettings.parent, typeof(Transform), true);
                            if (check.changed)
                            {
                                paintSettings.parent = parent;
                            }
                        }
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var createSubparent = UnityEditor.EditorGUILayout.ToggleLeft("Create sub-parents per palette",
                   paintSettings.createSubparentPerPalette);
                    if (check.changed)
                    {
                        paintSettings.createSubparentPerPalette = createSubparent;
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var createSubparent = UnityEditor.EditorGUILayout.ToggleLeft("Create sub-parents per tool",
                   paintSettings.createSubparentPerTool);
                    if (check.changed)
                    {
                        paintSettings.createSubparentPerTool = createSubparent;
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var createSubparent = UnityEditor.EditorGUILayout.ToggleLeft("Create sub-parents per brush",
                   paintSettings.createSubparentPerBrush);
                    if (check.changed)
                    {
                        paintSettings.createSubparentPerBrush = createSubparent;
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var createSubparent = UnityEditor.EditorGUILayout.ToggleLeft("Create sub-parents per prefab",
                   paintSettings.createSubparentPerPrefab);
                    if (check.changed)
                    {

                        paintSettings.createSubparentPerPrefab = createSubparent;
                    }
                }

            }
        }

        private void OverwriteLayerGUI(IPaintToolSettings paintSettings)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var overwritePrefabLayer = UnityEditor.EditorGUILayout.ToggleLeft("Overwrite prefab layer",
                        paintSettings.overwritePrefabLayer);
                    int layer = paintSettings.layer;
                    if (paintSettings.overwritePrefabLayer) layer = UnityEditor.EditorGUILayout.LayerField("Layer:",
                        paintSettings.layer);
                    if (check.changed)
                    {
                        paintSettings.overwritePrefabLayer = overwritePrefabLayer;
                        paintSettings.layer = layer;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            }
        }
        private void PaintToolSettingsGUI(IPaintToolSettings paintSettings)
        {
            ParentSettingsGUI(paintSettings);
            OverwriteLayerGUI(paintSettings);
        }

        private void RadiusSlider(CircleToolBase settings)
        {
            using (new GUILayout.HorizontalScope())
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    if (settings.radius > _maxRadius)
                        _maxRadius = Mathf.Max(Mathf.Floor(settings.radius / 10) * 20f, 10f);
                    UnityEditor.EditorGUIUtility.labelWidth = 60;
                    var radius = UnityEditor.EditorGUILayout.Slider("Radius:", settings.radius, 0.05f, _maxRadius);
                    if (check.changed)
                    {
                        settings.radius = radius;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                if (GUILayout.Button("|>", GUILayout.Width(20))) _maxRadius *= 2f;
                if (GUILayout.Button("|<", GUILayout.Width(20)))
                    _maxRadius = Mathf.Min(Mathf.Floor(settings.radius / 10f) * 10f + 10f, _maxRadius);
            }
        }
        private void BrushToolBaseSettingsGUI(BrushToolBase settings)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 60;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var brushShape = (BrushToolSettings.BrushShape)UnityEditor.EditorGUILayout.Popup("Shape:",
                        (int)settings.brushShape, _brushShapeOptions);
                    if (check.changed)
                    {
                        settings.brushShape = brushShape;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                if (settings.brushShape != BrushToolBase.BrushShape.POINT)
                {
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var randomize
                                = UnityEditor.EditorGUILayout.ToggleLeft("Randomize positions", settings.randomizePositions);
                            if (check.changed)
                            {
                                settings.randomizePositions = randomize;
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            UnityEditor.EditorGUIUtility.labelWidth = 80;
                            var randomness = UnityEditor.EditorGUILayout.Slider("Randomness:", settings.randomness, 0f, 1f);
                            if (check.changed)
                            {
                                settings.randomness = randomness;
                                UnityEditor.SceneView.RepaintAll();
                            }
                            UnityEditor.EditorGUIUtility.labelWidth = 60;
                        }
                    }
                    RadiusSlider(settings);
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var density = UnityEditor.EditorGUILayout.IntSlider("Density:", settings.density, 0, 100);
                    if (check.changed)
                    {
                        settings.density = density;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 90;
                        var spacingType = (BrushToolBase.SpacingType)UnityEditor.EditorGUILayout.Popup("Min Spacing:",
                            (int)settings.spacingType, _spacingOptions);
                        var spacing = settings.minSpacing;
                        using (new UnityEditor.EditorGUI.DisabledGroupScope(spacingType != BrushToolBase.SpacingType.CUSTOM))
                        {
                            spacing = UnityEditor.EditorGUILayout.FloatField("Value:", settings.minSpacing);
                        }
                        if (check.changed)
                        {
                            settings.spacingType = spacingType;
                            settings.minSpacing = spacing;
                            UnityEditor.SceneView.RepaintAll();
                        }
                    }
                }
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var orientAlongBrushstroke = UnityEditor.EditorGUILayout.ToggleLeft("Orient Along the Brushstroke",
                            settings.orientAlongBrushstroke);
                        var additionalAngle = settings.additionalOrientationAngle;
                        if (orientAlongBrushstroke)
                            additionalAngle = UnityEditor.EditorGUILayout.Vector3Field("Additonal angle:", additionalAngle);
                        if (check.changed)
                        {
                            settings.orientAlongBrushstroke = orientAlongBrushstroke;
                            settings.additionalOrientationAngle = additionalAngle;
                            UnityEditor.SceneView.RepaintAll();
                        }
                    }
                }
            }
        }

        private void EmbedInSurfaceSettingsGUI(SelectionToolBaseBasic settings)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (new UnityEditor.EditorGUI.DisabledGroupScope
                    (PWBCore.staticData.tempCollidersAction == PWBData.TempCollidersAction.NEVER_CREATE))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var createTempColliders = UnityEditor.EditorGUILayout.ToggleLeft("Create Temp Colliders",
                            settings.createTempColliders);
                            if (check.changed)
                            {
                                settings.createTempColliders = createTempColliders;
                                PWBCore.UpdateTempColliders();
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }

                        using (new UnityEditor.EditorGUI.DisabledGroupScope(!settings.createTempColliders))
                            if (GUILayout.Button(_updateButtonContent, GUILayout.Width(21), GUILayout.Height(21)))
                                PWBCore.UpdateTempColliders();
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    UnityEditor.EditorGUIUtility.labelWidth = 60;
                    var embedInSurface = UnityEditor.EditorGUILayout.ToggleLeft("Embed On the Surface",
                        settings.embedInSurface);
                    if (check.changed)
                    {
                        settings.embedInSurface = embedInSurface;
                        if (embedInSurface && settings is SelectionToolSettings) PWBIO.EmbedSelectionInSurface();
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                if (settings.embedInSurface)
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var embedAtPivotHeight = UnityEditor.EditorGUILayout.ToggleLeft("Embed At Pivot Height",
                            settings.embedAtPivotHeight);
                        if (check.changed)
                        {
                            settings.embedAtPivotHeight = embedAtPivotHeight;
                            if (settings.embedInSurface && settings is SelectionToolSettings) PWBIO.EmbedSelectionInSurface();
                            UnityEditor.SceneView.RepaintAll();
                        }
                    }
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 110;
                        var surfaceDistance = UnityEditor.EditorGUILayout.FloatField("Surface Distance:",
                            settings.surfaceDistance);
                        if (check.changed)
                        {
                            settings.surfaceDistance = surfaceDistance;
                            if (settings is SelectionToolSettings) PWBIO.EmbedSelectionInSurface();
                            UnityEditor.SceneView.RepaintAll();
                        }
                    }
                    if (settings is SelectionToolBase)
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var selectionSettings = settings as SelectionToolBase;
                            var rotateToTheSurface = UnityEditor.EditorGUILayout.ToggleLeft("Rotate To the Surface",
                                selectionSettings.rotateToTheSurface);
                            if (check.changed)
                            {
                                selectionSettings.rotateToTheSurface = rotateToTheSurface;
                                if (settings.embedInSurface && settings is SelectionToolSettings)
                                    PWBIO.EmbedSelectionInSurface();
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }

                    }
                }
            }
        }

        private struct BrushPropertiesGroupState
        {
            public bool brushPosGroupOpen;
            public bool brushRotGroupOpen;
            public bool brushScaleGroupOpen;
            public bool brushFlipGroupOpen;
        }

        private void OverwriteBrushPropertiesGUI(IPaintToolSettings settings,
            ref BrushPropertiesGroupState state)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var overwriteBrushProperties = UnityEditor.EditorGUILayout.ToggleLeft("Overwrite Brush Properties",
                        settings.overwriteBrushProperties);
                    if (check.changed)
                    {
                        settings.overwriteBrushProperties = overwriteBrushProperties;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                if (PaletteManager.selectedBrush != null)
                    settings.brushSettings.isAsset2D = PaletteManager.selectedBrush.isAsset2D;
                else settings.brushSettings.isAsset2D = false;
                if (settings.overwriteBrushProperties)
                    BrushProperties.BrushFields(settings.brushSettings,
                    ref state.brushPosGroupOpen, ref state.brushRotGroupOpen,
                    ref state.brushScaleGroupOpen, ref state.brushFlipGroupOpen, this, UNDO_MSG);
            }
        }

        private static readonly string[] _editModeTypeOptions = { "Line nodes", "Line position and rotation" };
        private void EditModeToggle(IPersistentToolManager persistentToolManager)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var editMode = UnityEditor.EditorGUILayout.ToggleLeft("Edit Mode", ToolManager.editMode);
                        if (check.changed)
                        {
                            ToolManager.editMode = editMode;
                            PWBIO.ResetLineRotation();
                            PWBIO.repaint = true;
                            UnityEditor.SceneView.RepaintAll();
                            PWBItemsWindow.RepainWindow();
                        }
                    }
                    if (persistentToolManager == LineManager.instance && ToolManager.editMode)
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var editModeType = (LineManager.EditModeType)UnityEditor.EditorGUILayout
                            .Popup((int)LineManager.editModeType, _editModeTypeOptions);
                            if (check.changed)
                            {
                                LineManager.editModeType = editModeType;
                                PWBIO.ResetLineRotation();
                                PWBIO.repaint = true;
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }
                    }
                }
                if (ToolManager.editMode)
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var applyBrushToexisting = UnityEditor.EditorGUILayout.ToggleLeft(
                            "Apply brush setings to Pre-existing objects", persistentToolManager.applyBrushToExisting);
                        if (check.changed)
                        {
                            persistentToolManager.applyBrushToExisting = applyBrushToexisting;
                            PWBIO.repaint = true;
                            UnityEditor.SceneView.RepaintAll();
                        }
                    }
                }
                if (ToolManager.editMode)
                {
                    if (GUILayout.Button("Open items window")) PWBItemsWindow.ShowWindow();
                }
            }
        }
        private void HandlePosition()
        {
            if (PWBIO.selectedPointIdx < 0) return;
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    PWBIO.handlePosition = UnityEditor.EditorGUILayout.Vector3Field("Handle position:", PWBIO.handlePosition);
                    if (check.changed) PWBIO.UpdateHandlePosition();
                }
            }
        }

        private void HandleRotation()
        {
            if (PWBIO.selectedPointIdx < 0) return;
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var eulerAngles = PWBIO.handleRotation.eulerAngles;
                    eulerAngles = UnityEditor.EditorGUILayout.Vector3Field("Handle rotation:", eulerAngles);
                    if (check.changed)
                    {
                        var newRotation = Quaternion.Euler(eulerAngles);
                        PWBIO.handleRotation = newRotation;
                        PWBIO.UpdateHandleRotation();
                    }
                }
            }
        }
        #endregion

        #region SELECTION BRUSH AND MODIFIER SETTINGS
        private static readonly string[] _modifierCommandOptions = { "All", "Palette Prefabs", "Brush Prefabs" };
        private void SelectionBrushGroup(ISelectionBrushTool settings, string actionLabel)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 60;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var command = (ModifierToolSettings.Command)UnityEditor.EditorGUILayout.Popup(actionLabel + ":",
                        (int)settings.command, _modifierCommandOptions);
                    if (check.changed)
                    {
                        settings.command = command;
                        PWBIO.UpdateOctree();
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var onlyTheClosest = UnityEditor.EditorGUILayout.ToggleLeft(actionLabel + " only the closest",
                        settings.onlyTheClosest);
                    if (check.changed)
                    {
                        settings.onlyTheClosest = onlyTheClosest;
                    }
                }
            }
        }

        private void ModifierGroup(IModifierTool settings, string actionLabel)
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 60;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var allButSelected = UnityEditor.EditorGUILayout.ToggleLeft(actionLabel + " all but selected",
                        settings.modifyAllButSelected);
                    if (check.changed)
                    {
                        settings.modifyAllButSelected = allButSelected;
                        PWBIO.UpdateOctree();
                    }
                }
            }
        }
        #endregion

        #region PIN
        private static readonly string[] _pinModeNames = { "Auto", "Paint on surface", "Paint on grid" };
        private static BrushPropertiesGroupState _pinOverwriteGroupState;
        private void PinGroup()
        {
            ToolProfileGUI(PinManager.instance);
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var mode = (PinSettings.PaintMode)UnityEditor.EditorGUILayout.Popup("Paint mode:",
                        (int)PinManager.settings.mode, _pinModeNames);
                    if (check.changed)
                    {
                        PinManager.settings.mode = mode;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var repeat = UnityEditor.EditorGUILayout.ToggleLeft("Repeat multi-brush item", PinManager.settings.repeat);
                    if (check.changed)
                    {
                        PinManager.settings.repeat = repeat;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var avoidOverlapping = UnityEditor.EditorGUILayout.ToggleLeft("Avoid overlapping",
                        PinManager.settings.avoidOverlapping);
                    if (check.changed)
                    {
                        PinManager.settings.avoidOverlapping = avoidOverlapping;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var snapRotationToGrid = UnityEditor.EditorGUILayout.ToggleLeft("Snap rotation to grid",
                        PinManager.settings.snapRotationToGrid);
                    if (check.changed)
                    {
                        PinManager.settings.snapRotationToGrid = snapRotationToGrid;
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 60;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var flattenTerrain
                        = UnityEditor.EditorGUILayout.ToggleLeft("Flatten the terrain", PinManager.settings.flattenTerrain);
                    if (check.changed)
                    {
                        PinManager.settings.flattenTerrain = flattenTerrain;
                    }
                }
                using (new UnityEditor.EditorGUI.DisabledGroupScope(!PinManager.settings.flattenTerrain))
                {
                    var flatteningSettings = PinManager.settings.flatteningSettings;
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var hardness = UnityEditor.EditorGUILayout.Slider("Hardness:", flatteningSettings.hardness, 0, 1);
                        if (check.changed)
                        {
                            flatteningSettings.hardness = hardness;
                        }
                    }
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var padding = UnityEditor.EditorGUILayout.FloatField("Padding:", flatteningSettings.padding);
                        if (check.changed)
                        {
                            flatteningSettings.padding = padding;
                        }
                    }
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var clearTrees = UnityEditor.EditorGUILayout.ToggleLeft("Clear trees", flatteningSettings.clearTrees);
                        if (check.changed)
                        {
                            flatteningSettings.clearTrees = clearTrees;
                        }
                    }
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var clearDetails
                            = UnityEditor.EditorGUILayout.ToggleLeft("Clear details", flatteningSettings.clearDetails);
                        if (check.changed)
                        {
                            flatteningSettings.clearDetails = clearDetails;
                        }
                    }
                }
            }
            PaintSettingsGUI(PinManager.settings, PinManager.settings);
            OverwriteBrushPropertiesGUI(PinManager.settings, ref _pinOverwriteGroupState);
        }
        #endregion

        #region BRUSH
        private static readonly string[] _heightTypeNames = { "Custom", "Radius" };
        private static readonly string[] _avoidOverlappingTypeNames = { "Disabled", "With Palette Prefabs",
            "With Brush Prefabs", "With Same Prefabs", "With All Objects" };
        private static BrushPropertiesGroupState _brushOverwriteGroupState;
        private void BrushGroup()
        {
            ToolProfileGUI(BrushManager.instance);
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                BrushManager.settings.showPreview = UnityEditor.EditorGUILayout.ToggleLeft("Show Brushstroke Preview",
                    BrushManager.settings.showPreview);
                if (BrushManager.settings.showPreview)
                    UnityEditor.EditorGUILayout.HelpBox("The brushstroke preview can cause slowdown issues.",
                        UnityEditor.MessageType.Info);
                UnityEditor.EditorGUILayout.LabelField("Brushstroke object count:", BrushstrokeManager.itemCount.ToString());
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                BrushToolBaseSettingsGUI(BrushManager.settings);
                UnityEditor.EditorGUIUtility.labelWidth = 150;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var avoidOverlapping = (BrushToolSettings.AvoidOverlappingType)
                        UnityEditor.EditorGUILayout.Popup("Avoid Overlapping:",
                        (int)BrushManager.settings.avoidOverlapping, _avoidOverlappingTypeNames);
                    if (check.changed)
                    {
                        BrushManager.settings.avoidOverlapping = avoidOverlapping;
                    }
                }
                if (BrushManager.settings.brushShape != BrushToolBase.BrushShape.POINT)
                {
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var heightType = (BrushToolSettings.HeightType)
                                UnityEditor.EditorGUILayout.Popup("Max Height From center:",
                                (int)BrushManager.settings.heightType, _heightTypeNames);
                            if (check.changed)
                            {
                                BrushManager.settings.heightType = heightType;
                                if (heightType == BrushToolSettings.HeightType.RADIUS)
                                    BrushManager.settings.maxHeightFromCenter = BrushManager.settings.radius;
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }
                        using (new UnityEditor.EditorGUI.DisabledGroupScope(
                            BrushManager.settings.heightType == BrushToolSettings.HeightType.RADIUS))
                        {
                            using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                            {
                                var maxHeightFromCenter = Mathf.Abs(UnityEditor.EditorGUILayout.FloatField("Value:",
                                    BrushManager.settings.maxHeightFromCenter));
                                if (check.changed)
                                {
                                    BrushManager.settings.maxHeightFromCenter = maxHeightFromCenter;
                                    UnityEditor.SceneView.RepaintAll();
                                }
                            }
                        }
                    }
                }
            }

            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                GUILayout.Label("Surface Filters", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUIUtility.labelWidth = 110;
                using (new GUILayout.HorizontalScope())
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var minSlope = BrushManager.settings.slopeFilter.min;
                        var maxSlope = BrushManager.settings.slopeFilter.max;
                        UnityEditor.EditorGUILayout.MinMaxSlider("Slope Angle:", ref minSlope, ref maxSlope, 0, 90);
                        minSlope = Mathf.Round(minSlope);
                        maxSlope = Mathf.Round(maxSlope);
                        GUILayout.Label("[" + minSlope.ToString("00") + "°," + maxSlope.ToString("00") + "°]");
                        if (check.changed)
                        {
                            BrushManager.settings.slopeFilter.v1 = minSlope;
                            BrushManager.settings.slopeFilter.v2 = maxSlope;
                            UnityEditor.SceneView.RepaintAll();
                        }
                    }
                }

                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var mask = UnityEditor.EditorGUILayout.MaskField("Layers:",
                        EditorGUIUtils.LayerMaskToField(BrushManager.settings.layerFilter),
                        UnityEditorInternal.InternalEditorUtility.layers);
                    if (check.changed)
                    {
                        BrushManager.settings.layerFilter = EditorGUIUtils.FieldToLayerMask(mask);
                        UnityEditor.SceneView.RepaintAll();
                    }
                }

                UnityEditor.EditorGUIUtility.labelWidth = 108;
                var field = EditorGUIUtils.MultiTagField.Instantiate("Tags:", BrushManager.settings.tagFilter, null);
                field.OnChange += OnBrushTagFilterChanged;

                bool terrainFilterChanged = false;
                var terrainFilter = EditorGUIUtils.ObjectArrayFieldWithButtons("Terrain Layers:",
                    BrushManager.settings.terrainLayerFilter, ref _terrainLayerFilterFoldout, out terrainFilterChanged);
                if (terrainFilterChanged)
                {
                    BrushManager.settings.terrainLayerFilter = terrainFilter.ToArray();
                    UnityEditor.SceneView.RepaintAll();
                }
            }
            PaintSettingsGUI(BrushManager.settings, BrushManager.settings);
            OverwriteBrushPropertiesGUI(BrushManager.settings, ref _brushOverwriteGroupState);
        }


        private bool _terrainLayerFilterFoldout = false;

        private void OnBrushTagFilterChanged(System.Collections.Generic.List<string> prevFilter,
            System.Collections.Generic.List<string> newFilter, string key)
        {

            BrushManager.settings.tagFilter = newFilter;
        }
        #endregion

        #region ERASER
        private void EraserGroup()
        {
            UnityEditor.EditorGUIUtility.labelWidth = 60;
            var settings = EraserManager.settings;
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox)) RadiusSlider(settings);
            var actionLabel = "Erase";
            SelectionBrushGroup(settings, actionLabel);
            ModifierGroup(settings, actionLabel);
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var outermostFilter = UnityEditor.EditorGUILayout.ToggleLeft("Outermost prefab filter",
                        settings.outermostPrefabFilter);
                    if (check.changed)
                    {
                        settings.outermostPrefabFilter = outermostFilter;
                    }
                }
                if (!settings.outermostPrefabFilter)
                    GUILayout.Label("When you delete a child of a prefab, the prefab will be unpacked.",
                        UnityEditor.EditorStyles.helpBox);
            }
        }
        #endregion

        #region GRAVITY
        private static BrushPropertiesGroupState _gravityOverwriteGroupState;
        private void GravityGroup()
        {
            ToolProfileGUI(GravityToolManager.instance);
            BrushToolBaseSettingsGUI(GravityToolManager.settings);
            UnityEditor.EditorGUIUtility.labelWidth = 120;
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                var settings = GravityToolManager.settings.Clone();
                var data = settings.simData;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    settings.height = UnityEditor.EditorGUILayout.FloatField("Height:", settings.height);
                    data.maxIterations = UnityEditor.EditorGUILayout.IntField("Max Iterations:", data.maxIterations);
                    data.maxSpeed = UnityEditor.EditorGUILayout.FloatField("Max Speed:", data.maxSpeed);
                    data.maxAngularSpeed = UnityEditor.EditorGUILayout.FloatField("Max Angular Speed:", data.maxAngularSpeed);
                    data.mass = UnityEditor.EditorGUILayout.FloatField("Mass:", data.mass);
                    data.drag = UnityEditor.EditorGUILayout.FloatField("Drag:", data.drag);
                    data.angularDrag = UnityEditor.EditorGUILayout.FloatField("Angular Drag:", data.angularDrag);
                    if (check.changed)
                    {
                        GravityToolManager.settings.Copy(settings);
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
                using (new UnityEditor.EditorGUI.DisabledGroupScope
                    (PWBCore.staticData.tempCollidersAction == PWBData.TempCollidersAction.NEVER_CREATE))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            var createTempColliders = UnityEditor.EditorGUILayout.ToggleLeft("Create Temp Colliders",
                            GravityToolManager.settings.createTempColliders);
                            if (check.changed)
                            {
                                GravityToolManager.settings.createTempColliders = createTempColliders;
                                PWBCore.UpdateTempColliders();
                                UnityEditor.SceneView.RepaintAll();
                            }
                        }

                        using (new UnityEditor.EditorGUI.DisabledGroupScope(!GravityToolManager.settings.createTempColliders))
                            if (GUILayout.Button(_updateButtonContent, GUILayout.Width(21), GUILayout.Height(21)))
                                PWBCore.UpdateTempColliders();

                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    data.ignoreSceneColliders = UnityEditor.EditorGUILayout.ToggleLeft("Ignore Scene Colliders",
                            data.ignoreSceneColliders);
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        data.changeLayer
                            = UnityEditor.EditorGUILayout.ToggleLeft("Change Layer Temporarily", data.changeLayer);
                        if (data.changeLayer)
                            data.tempLayer = UnityEditor.EditorGUILayout.LayerField("Temp layer:", data.tempLayer);
                    }
                    if (check.changed)
                    {
                        GravityToolManager.settings.Copy(settings);
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            }
            PaintToolSettingsGUI(GravityToolManager.settings);
            OverwriteBrushPropertiesGUI(GravityToolManager.settings, ref _gravityOverwriteGroupState);
        }
        #endregion

        #region LINE
        private static readonly string[] _lineModeNames = { "Auto", "Paint on surface", "Paint on the line" };
        private static readonly string[] _lineSpacingNames = { "Bounds", "Constant" };
        private static readonly string[] _lineAxesAlongTheLineNames = { "X", "Z" };
        private static string[] _shapeProjDirNames = new string[] { "+X", "-X", "+Y", "-Y", "+Z", "-Z", "Plane Axis" };

        private static int _lineProjDirIdx = 6;
        private static BrushPropertiesGroupState _lineOverwriteGroupState;
        private void LineBaseGUI<SETTINGS>(SETTINGS lineSettings) where SETTINGS : LineSettings
        {
            void OnValueChanged()
            {
                PWBIO.UpdateStroke();
                PWBIO.repaint = true;
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var mode = (PaintOnSurfaceToolSettingsBase.PaintMode)
                    UnityEditor.EditorGUILayout.Popup("Paint Mode:", (int)lineSettings.mode, _lineModeNames);
                    if (check.changed)
                    {
                        lineSettings.mode = mode;
                        OnValueChanged();
                    }
                }
                if (lineSettings is ShapeSettings)
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var parallelToTheSurface = UnityEditor.EditorGUILayout.ToggleLeft(
                            lineSettings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE
                            ? "Place objects perpendicular to the plane"
                            : "Place objects perpendicular to the surface",
                            lineSettings.perpendicularToTheSurface);
                        if (check.changed)
                        {
                            lineSettings.perpendicularToTheSurface = parallelToTheSurface;
                            OnValueChanged();
                        }
                    }
                }
                else
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var parallelToTheSurface
                            = UnityEditor.EditorGUILayout.ToggleLeft("Place objects perpendicular to the " +
                            (lineSettings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE ? "line" : "surface"),
                            lineSettings.perpendicularToTheSurface);
                        if (check.changed)
                        {
                            lineSettings.perpendicularToTheSurface = parallelToTheSurface;
                            OnValueChanged();
                        }
                    }
                }
                var dirNames = lineSettings is ShapeSettings ? _shapeProjDirNames : _dirNames;
                var shapeSettings = lineSettings as ShapeSettings;
                if (shapeSettings != null)
                {
                    _lineProjDirIdx = shapeSettings.projectInNormalDir ? _lineProjDirIdx = 6
                        : System.Array.IndexOf(_dir, lineSettings.projectionDirection);
                }
                else _lineProjDirIdx = System.Array.IndexOf(_dir, lineSettings.projectionDirection);
                if (_lineProjDirIdx == -1) _lineProjDirIdx = 3;

                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    _lineProjDirIdx = UnityEditor.EditorGUILayout.Popup("Projection Direction:", _lineProjDirIdx, dirNames);
                    if (check.changed)
                    {
                        if (shapeSettings != null) shapeSettings.projectInNormalDir = _lineProjDirIdx == 6;
                        lineSettings.projectionDirection = _lineProjDirIdx == 6
                            ? PWBIO.GetShapePlaneNormal() : _dir[_lineProjDirIdx];
                        OnValueChanged();
                    }
                }
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var objectsOrientedAlongTheLine
                    = UnityEditor.EditorGUILayout.ToggleLeft("Orient Along the Line",
                    lineSettings.objectsOrientedAlongTheLine);
                    if (check.changed)
                    {
                        lineSettings.objectsOrientedAlongTheLine = objectsOrientedAlongTheLine;
                        OnValueChanged();
                    }
                }
                if (lineSettings.objectsOrientedAlongTheLine)
                {
                    UnityEditor.EditorGUIUtility.labelWidth = 170;
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var axisOrientedAlongTheLine = UnityEditor.EditorGUILayout.Popup("Axis Oriented Along the Line:",
                        lineSettings.axisOrientedAlongTheLine == AxesUtils.Axis.X ? 0 : 1,
                        _lineAxesAlongTheLineNames) == 0 ? AxesUtils.Axis.X : AxesUtils.Axis.Z;
                        if (check.changed)
                        {
                            lineSettings.axisOrientedAlongTheLine = axisOrientedAlongTheLine;
                            OnValueChanged();
                        }
                    }
                }
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 120;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var spacingType = (LineSettings.SpacingType)
                    UnityEditor.EditorGUILayout.Popup("Spacing:", (int)lineSettings.spacingType, _lineSpacingNames);
                    if (check.changed)
                    {
                        lineSettings.spacingType = spacingType;
                        OnValueChanged();
                    }
                }
                if (lineSettings.spacingType == LineSettings.SpacingType.CONSTANT)
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var spacing = UnityEditor.EditorGUILayout.FloatField("Value:", lineSettings.spacing);
                        if (check.changed)
                        {
                            lineSettings.spacing = spacing;
                            OnValueChanged();
                        }
                    }
                }
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var gapSize = UnityEditor.EditorGUILayout.FloatField("Gap Size:", lineSettings.gapSize);
                    if (check.changed)
                    {
                        if (PaletteManager.selectedBrushIdx >= 0 && PaletteManager.selectedBrush != null)
                        {
                            var spacing = lineSettings.spacingType == LineSettings.SpacingType.CONSTANT
                                ? lineSettings.spacing : PaletteManager.selectedBrush.minBrushMagnitude;
                            var min = Mathf.Min(0, 0.05f - spacing);
                            gapSize = Mathf.Max(min, gapSize);
                        }
                        lineSettings.gapSize = gapSize;
                        OnValueChanged();
                    }
                }
            }
        }

        private void LineGroup()
        {
            ToolProfileGUI(LineManager.instance);
            EditModeToggle(LineManager.instance);
            HandlePosition();
            UnityEditor.EditorGUIUtility.labelWidth = 120;
            LineBaseGUI(LineManager.settings);
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var closed = UnityEditor.EditorGUILayout.ToggleLeft("Colsed Path", PWBIO.lineData.closed);
                    if (check.changed)
                    {
                        PWBIO.lineData.closed = closed;
                        PWBIO.UpdateStroke();
                        UnityEditor.SceneView.RepaintAll();
                        PWBIO.repaint = true;
                    }
                }
            }
            PaintSettingsGUI(LineManager.settings, LineManager.settings);
            OverwriteBrushPropertiesGUI(LineManager.settings, ref _lineOverwriteGroupState);
        }
        #endregion

        #region SHAPE
        private static readonly string[] _shapeTypeNames = { "Circle", "Polygon" };
        private static BrushPropertiesGroupState _shapeOverwriteGroupState;
        private static string[] _shapeDirNames = new string[] { "+X", "-X", "+Y", "-Y", "+Z", "-Z", "Normal to surface" };
        private void ShapeGroup()
        {
            UnityEditor.EditorGUIUtility.labelWidth = 100;
            ToolProfileGUI(ShapeManager.instance);
            EditModeToggle(ShapeManager.instance);
            HandlePosition();
            HandleRotation();
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var shapeType = (ShapeSettings.ShapeType)UnityEditor.EditorGUILayout.Popup("Shape:",
                        (int)ShapeManager.settings.shapeType, _shapeTypeNames);
                    if (check.changed)
                    {
                        ShapeManager.settings.shapeType = shapeType;
                        if (shapeType == ShapeSettings.ShapeType.CIRCLE)
                        {
                            ShapeData.instance.UpdateCircleSideCount();
                        }
                        ShapeData.instance.Update(true);
                        PWBIO.UpdateStroke();
                        PWBIO.repaint = true;
                    }
                }
                if (ShapeManager.settings.shapeType == ShapeSettings.ShapeType.POLYGON)
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var sideCount = UnityEditor.EditorGUILayout.IntSlider("Number of sides:",
                            ShapeManager.settings.sidesCount, 3, 12);
                        if (check.changed)
                        {
                            ShapeManager.settings.sidesCount = sideCount;
                            ShapeData.instance.UpdateIntersections();
                            PWBIO.UpdateStroke();
                            PWBIO.repaint = true;
                        }
                    }
                }

                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var normalDirIdx = ShapeManager.settings.axisNormalToSurface
                        ? 6 : System.Array.IndexOf(_dir, ShapeManager.settings.normal);
                    UnityEditor.EditorGUIUtility.labelWidth = 120;
                    normalDirIdx = UnityEditor.EditorGUILayout.Popup("Initial axis direction:", normalDirIdx, _shapeDirNames);
                    var axisNormalToSurface = normalDirIdx == 6;
                    if (check.changed)
                    {
                        ShapeManager.settings.axisNormalToSurface = axisNormalToSurface;
                        ShapeManager.settings.normal = normalDirIdx == 6 ? Vector3.up : _dir[normalDirIdx];
                        PWBIO.UpdateStroke();
                        PWBIO.repaint = true;
                    }
                }
            }
            UnityEditor.EditorGUIUtility.labelWidth = 120;
            LineBaseGUI(ShapeManager.settings);
            PaintSettingsGUI(ShapeManager.settings, ShapeManager.settings);
            OverwriteBrushPropertiesGUI(ShapeManager.settings, ref _shapeOverwriteGroupState);
        }
        #endregion

        #region TILING
        private static readonly string[] _tilingModeNames = { "Auto", "Paint on surface", "Paint on the plane" };
        private static readonly string[] _tilingCellTypeNames = { "Smallest object", "Biggest object", "Custom" };
        private static BrushPropertiesGroupState _tilingOverwriteGroupState;
        private void TilingGroup()
        {
            ToolProfileGUI(TilingManager.instance);
            EditModeToggle(TilingManager.instance);
            HandlePosition();
            if (!ToolManager.editMode)
            {
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    TilingManager.settings.showPreview = UnityEditor.EditorGUILayout.ToggleLeft("Show Preview",
                        TilingManager.settings.showPreview);
                    if (TilingManager.settings.showPreview)
                        UnityEditor.EditorGUILayout.HelpBox("If you experience slowdown issues, disable preview.",
                            UnityEditor.MessageType.Info);
                    UnityEditor.EditorGUILayout.LabelField("Object count:", BrushstrokeManager.itemCount.ToString());
                }
            }
            UnityEditor.EditorGUIUtility.labelWidth = 180;
            using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
            {
                var settings = TilingManager.settings;
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    settings.mode = (TilingSettings.PaintMode)UnityEditor.EditorGUILayout.Popup("Paint mode:",
                    (int)settings.mode, _tilingModeNames);
                    using (var angleCheck = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var eulerAngles = settings.rotation.eulerAngles;
                        eulerAngles = UnityEditor.EditorGUILayout.Vector3Field("Plane Rotation:", eulerAngles);
                        if (angleCheck.changed)
                        {
                            var newRotation = Quaternion.Euler(eulerAngles);
                            PWBIO.UpdateTilingRotation(newRotation);
                            settings.rotation = newRotation;
                        }
                    }
                    var axisIdx = UnityEditor.EditorGUILayout.Popup("Axis aligned with plane normal: ",
                        settings.axisAlignedWithNormal, _dirNames);
                    settings.axisAlignedWithNormal = axisIdx;
                }
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    UnityEditor.EditorGUIUtility.labelWidth = 76;
                    settings.cellSizeType = (TilingSettings.CellSizeType)
                        UnityEditor.EditorGUILayout.Popup("Cell size:", (int)settings.cellSizeType, _tilingCellTypeNames);
                    using (new UnityEditor.EditorGUI.DisabledGroupScope(
                        settings.cellSizeType != TilingSettings.CellSizeType.CUSTOM))
                    {
                        settings.cellSize = UnityEditor.EditorGUILayout.Vector2Field("", settings.cellSize);
                    }
                }
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    settings.spacing = UnityEditor.EditorGUILayout.Vector2Field("Spacing", settings.spacing);
                }
                if (check.changed)
                {
                    PWBIO.UpdateStroke();
                    UnityEditor.SceneView.RepaintAll();
                }
            }
            PaintSettingsGUI(TilingManager.settings, TilingManager.settings);
            OverwriteBrushPropertiesGUI(TilingManager.settings, ref _tilingOverwriteGroupState);
        }
        #endregion

        #region EXTRUDE
        private static readonly string[] _spaceOptions = { "Global", "Local" };
        private static readonly string[] _rotationOptions = { "First Object Selected", "Last Object Selected" };
        private static readonly string[] _extrudeSpacingOptions = { "Box Size", "Custom" };
        private static readonly string[] _addRotationOptions = { "Constant", "Random" };
        private void ExtrudeGroup()
        {
            ToolProfileGUI(ExtrudeManager.instance);
            UnityEditor.EditorGUIUtility.labelWidth = 60;
            using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
            {
                var extrudeSettings = ExtrudeManager.settings.Clone();
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    extrudeSettings.space = (Space)(UnityEditor.EditorGUILayout.Popup("Space:",
                        (int)extrudeSettings.space, _spaceOptions));
                    if (extrudeSettings.space == Space.Self)
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 150;
                        extrudeSettings.rotationAccordingTo = (ExtrudeSettings.RotationAccordingTo)UnityEditor
                            .EditorGUILayout.Popup("Set rotation according to:",
                            (int)extrudeSettings.rotationAccordingTo, _rotationOptions);
                    }
                }
                UnityEditor.EditorGUIUtility.labelWidth = 60;
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    extrudeSettings.spacingType = (ExtrudeSettings.SpacingType)UnityEditor.EditorGUILayout.Popup("Spacing:",
                        (int)extrudeSettings.spacingType, _extrudeSpacingOptions);
                    if (extrudeSettings.spacingType == ExtrudeSettings.SpacingType.BOX_SIZE)
                        extrudeSettings.multiplier
                            = UnityEditor.EditorGUILayout.Vector3Field("Multiplier:", extrudeSettings.multiplier);
                    else extrudeSettings.spacing
                            = UnityEditor.EditorGUILayout.Vector3Field("Value:", extrudeSettings.spacing);
                }
                if (extrudeSettings.space == Space.World)
                {
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 80;
                        extrudeSettings.addRandomRotation = UnityEditor.EditorGUILayout.Popup("Add Rotation:",
                            extrudeSettings.addRandomRotation ? 1 : 0, _addRotationOptions) == 1;
                        if (extrudeSettings.addRandomRotation)
                        {
                            extrudeSettings.randomEulerOffset = EditorGUIUtils.Range3Field(string.Empty,
                                extrudeSettings.randomEulerOffset);
                            using (new GUILayout.HorizontalScope())
                            {
                                extrudeSettings.rotateInMultiples = UnityEditor.EditorGUILayout.ToggleLeft
                                    ("Only in multiples of:", extrudeSettings.rotateInMultiples);
                                using (new UnityEditor.EditorGUI.DisabledGroupScope(!extrudeSettings.rotateInMultiples))
                                    extrudeSettings.rotationFactor
                                        = UnityEditor.EditorGUILayout.FloatField(extrudeSettings.rotationFactor);
                            }
                        }
                        else extrudeSettings.eulerOffset = UnityEditor.EditorGUILayout.Vector3Field(string.Empty,
                            extrudeSettings.eulerOffset);
                    }
                }

                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    extrudeSettings.sameParentAsSource
                        = UnityEditor.EditorGUILayout.ToggleLeft("Same parent as source", extrudeSettings.sameParentAsSource);
                    if (!extrudeSettings.sameParentAsSource)
                    {
                        extrudeSettings.autoCreateParent
                            = UnityEditor.EditorGUILayout.ToggleLeft("Create parent", extrudeSettings.autoCreateParent);
                        if (extrudeSettings.autoCreateParent) extrudeSettings.createSubparentPerPrefab
                                = UnityEditor.EditorGUILayout.ToggleLeft("Create sub-parent per prefab",
                                extrudeSettings.createSubparentPerPrefab);
                        else extrudeSettings.parent = (Transform)UnityEditor.EditorGUILayout.ObjectField("Parent Transform:",
                                extrudeSettings.parent, typeof(Transform), true);
                    }
                }
                using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                {
                    extrudeSettings.overwritePrefabLayer
                        = UnityEditor.EditorGUILayout.ToggleLeft("Overwrite prefab layer",
                        extrudeSettings.overwritePrefabLayer);
                    if (extrudeSettings.overwritePrefabLayer)
                        extrudeSettings.layer = UnityEditor.EditorGUILayout.LayerField("Layer:", extrudeSettings.layer);
                }

                if (check.changed)
                {
                    ExtrudeManager.settings.Copy(extrudeSettings);
                    UnityEditor.SceneView.RepaintAll();
                    PWBIO.ClearExtrudeAngles();
                }
            }
            EmbedInSurfaceSettingsGUI(ExtrudeManager.settings);
        }
        #endregion

        #region SELECTION TOOL
        private void SelectionGroup()
        {
            ToolProfileGUI(SelectionToolManager.instance);
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    UnityEditor.EditorGUIUtility.labelWidth = 90;
                    var handleSpace = (Space)(UnityEditor.EditorGUILayout.Popup("Handle Space:",
                        (int)SelectionToolManager.settings.handleSpace, _spaceOptions));
                    if (SelectionManager.topLevelSelection.Length > 1) SelectionToolManager.settings.boxSpace = Space.World;
                    var boxSpace = SelectionToolManager.settings.boxSpace;
                    using (new UnityEditor.EditorGUI.DisabledGroupScope(SelectionManager.topLevelSelection.Length > 1))
                    {
                        boxSpace = (Space)(UnityEditor.EditorGUILayout.Popup("Box Space:",
                            (int)SelectionToolManager.settings.boxSpace, _spaceOptions));
                    }
                    if (check.changed)
                    {
                        SelectionToolManager.settings.handleSpace = handleSpace;
                        SelectionToolManager.settings.boxSpace = boxSpace;
                        PWBIO.ResetSelectionRotation();
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                GUILayout.Label("Selection Filters", UnityEditor.EditorStyles.boldLabel);
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    UnityEditor.EditorGUIUtility.labelWidth = 90;
                    var paletteFilter = UnityEditor.EditorGUILayout.ToggleLeft("Prefabs from selected palette only",
                        SelectionToolManager.settings.paletteFilter);
                    var brushFilter = UnityEditor.EditorGUILayout.ToggleLeft("Prefabs from selected brush only",
                        SelectionToolManager.settings.brushFilter);
                    var layerMask = UnityEditor.EditorGUILayout.MaskField("Layers:",
                        EditorGUIUtils.LayerMaskToField(SelectionToolManager.settings.layerFilter),
                        UnityEditorInternal.InternalEditorUtility.layers);
                    var tagField = EditorGUIUtils.MultiTagField.Instantiate("Tags:",
                        SelectionToolManager.settings.tagFilter, null);
                    tagField.OnChange += OnSelectionTagFilterChanged;
                    if (check.changed)
                    {
                        SelectionToolManager.settings.paletteFilter = paletteFilter;
                        SelectionToolManager.settings.brushFilter = brushFilter;
                        SelectionToolManager.settings.layerFilter = EditorGUIUtils.FieldToLayerMask(layerMask);
                        PWBIO.ApplySelectionFilters();
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            }
            EmbedInSurfaceSettingsGUI(SelectionToolManager.settings);
        }

        private void OnSelectionTagFilterChanged(System.Collections.Generic.List<string> prevFilter,
            System.Collections.Generic.List<string> newFilter, string key)
        {

            SelectionToolManager.settings.tagFilter = newFilter;
            PWBIO.ApplySelectionFilters();
            UnityEditor.SceneView.RepaintAll();
        }
        #endregion

        #region CIRCLE SELECT
        private void CircleSelectGroup()
        {
            UnityEditor.EditorGUIUtility.labelWidth = 60;
            var settings = CircleSelectManager.settings;
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox)) RadiusSlider(settings);
            SelectionBrushGroup(settings, actionLabel: "Select");

            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var outermostFilter = UnityEditor.EditorGUILayout.ToggleLeft("Outermost prefab filter",
                        settings.outermostPrefabFilter);
                    if (check.changed)
                    {
                        settings.outermostPrefabFilter = outermostFilter;
                    }
                }
            }
        }
        #endregion

        #region MIRROR
        private static readonly string[] _mirrorActionNames = { "Transform", "Create" };
        private void MirrorGroup()
        {
            ToolProfileGUI(MirrorManager.instance);
            using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
            {
                var mirrorSettings = new MirrorSettings();
                MirrorManager.settings.Clone(mirrorSettings);
                using (var mirrorCheck = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 80;
                        mirrorSettings.mirrorPosition = UnityEditor.EditorGUILayout.Vector3Field("Position:",
                            mirrorSettings.mirrorPosition);
                        mirrorSettings.mirrorRotation = Quaternion.Euler(UnityEditor.EditorGUILayout.Vector3Field("Rotation:",
                            mirrorSettings.mirrorRotation.eulerAngles));
                    }
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 110;
                        mirrorSettings.invertScale
                            = UnityEditor.EditorGUILayout.ToggleLeft("Invert scale", mirrorSettings.invertScale);
                        mirrorSettings.reflectRotation
                            = UnityEditor.EditorGUILayout.ToggleLeft("Reflect rotation", mirrorSettings.reflectRotation);
                        mirrorSettings.action = (MirrorSettings.MirrorAction)UnityEditor.EditorGUILayout.Popup("Action:",
                            (int)mirrorSettings.action, _mirrorActionNames);
                    }
                    if (mirrorCheck.changed) UnityEditor.SceneView.RepaintAll();
                }

                if (mirrorSettings.action == MirrorSettings.MirrorAction.CREATE)
                {
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        mirrorSettings.sameParentAsSource = UnityEditor.EditorGUILayout.ToggleLeft("Same parent as source",
                            mirrorSettings.sameParentAsSource);
                        if (!mirrorSettings.sameParentAsSource)
                        {
                            mirrorSettings.autoCreateParent
                                = UnityEditor.EditorGUILayout.ToggleLeft("Create parent", mirrorSettings.autoCreateParent);
                            if (mirrorSettings.autoCreateParent)
                                mirrorSettings.createSubparentPerPrefab
                                    = UnityEditor.EditorGUILayout.ToggleLeft("Create sub-parent per prefab",
                                    mirrorSettings.createSubparentPerPrefab);
                            else mirrorSettings.parent
                                    = (Transform)UnityEditor.EditorGUILayout.ObjectField("Parent Transform:",
                                    mirrorSettings.parent, typeof(Transform), true);
                        }
                    }
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        mirrorSettings.overwritePrefabLayer = UnityEditor.EditorGUILayout.ToggleLeft("Overwrite prefab layer",
                            mirrorSettings.overwritePrefabLayer);
                        if (mirrorSettings.overwritePrefabLayer)
                            mirrorSettings.layer = UnityEditor.EditorGUILayout.LayerField("Layer:", mirrorSettings.layer);
                    }
                }
                if (check.changed)
                {
                    MirrorManager.settings.Copy(mirrorSettings);
                    UnityEditor.SceneView.RepaintAll();
                }
            }
            EmbedInSurfaceSettingsGUI(MirrorManager.settings);
        }
        #endregion

        #region REPLACER
        private static BrushPropertiesGroupState _replacerOverwriteGroupState;
        private static readonly string[] _replacerModeOptions = { "Target Center", "Target Pivot", "On Surface" };
        private void ReplacerGroup()
        {
            UnityEditor.EditorGUIUtility.labelWidth = 60;
            var settings = ReplacerManager.settings;
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox)) RadiusSlider(settings);
            var actionLabel = "Replace";
            SelectionBrushGroup(settings, actionLabel);
            ModifierGroup(settings, actionLabel);
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var positionMode = (ReplacerSettings.PositionMode)UnityEditor.EditorGUILayout.Popup("Position:",
                        (int)settings.positionMode, _replacerModeOptions);
                    if (check.changed)
                    {
                        settings.positionMode = positionMode;
                    }
                }
                var keepTargetSize = settings.keepTargetSize;
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    keepTargetSize = UnityEditor.EditorGUILayout.ToggleLeft("Keep target size", settings.keepTargetSize);

                    if (check.changed)
                    {
                        settings.keepTargetSize = keepTargetSize;
                    }
                }
                if (keepTargetSize)
                {
                    using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                    {
                        var maintainProportions = UnityEditor.EditorGUILayout.ToggleLeft("Maintain proportions",
                            settings.maintainProportions);
                        if (check.changed)
                        {
                            settings.maintainProportions = maintainProportions;
                        }
                    }
                }
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    var outermostFilter = UnityEditor.EditorGUILayout.ToggleLeft("Outermost prefab filter",
                        settings.outermostPrefabFilter);
                    if (check.changed) settings.outermostPrefabFilter = outermostFilter;
                }
                if (!settings.outermostPrefabFilter)
                    GUILayout.Label("When you replace a child of a prefab, the prefab will be unpacked.",
                        UnityEditor.EditorStyles.helpBox);
            }
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                settings.sameParentAsTarget = UnityEditor.EditorGUILayout.ToggleLeft("Same Parent as the target",
                    settings.sameParentAsTarget);
                if (!settings.sameParentAsTarget) ParentSettingsGUI(ReplacerManager.settings);
            }
            OverwriteLayerGUI(ReplacerManager.settings);
            OverwriteBrushPropertiesGUI(ReplacerManager.settings, ref _replacerOverwriteGroupState);
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.helpBox))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Replace all selected"))
                {
                    PWBIO.ReplaceAllSelected();
                    UnityEditor.SceneView.RepaintAll();
                }
                GUILayout.FlexibleSpace();
            }
        }
        #endregion
    }
}