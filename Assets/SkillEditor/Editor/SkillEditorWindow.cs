using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
//using System.Timers;

public class SkillEditorWindow : EditorWindow {

    #region define
    private const string PREVIEW_MODE = "Preview Mode";

    private const string TITLE = "SkillEditor";
    private const string MENU_ITEM = "Window/SkillEditorWindow %#d";
    private const string GUI_SKIN_MENU_ITEM = "Window/GUISkinEditor";
    #endregion

    #region UI
    private Texture settingsImage = null;
    private Texture rescaleImage = null;
    private Texture zoomInImage = null;
    private Texture zoomOutImage = null;
    private Texture snapImage = null;
    private Texture rollingEditImage = null;
    private Texture rippleEditImage = null;
    private Texture pickerImage = null;
    private Texture refreshImage = null;
    private Texture titleImage = null;
    private Texture cropImage = null;
    private Texture scaleImage = null;

    private const float TOOLBAR_HEIGHT = 17f;
    private const string PRO_SKIN = "Director_LightSkin";
    private const string FREE_SKIN = "Director_DarkSkin";

    private const string SETTINGS_ICON = "Director_SettingsIcon";
    private const string HORIZONTAL_RESCALE_ICON = "Director_HorizontalRescaleIcon";
    private const string PICKER_ICON = "Director_PickerIcon";
    private const string REFRESH_ICON = "Director_RefreshIcon";
    private const string MAGNET_ICON = "Director_Magnet";
    private const string ZOOMIN_ICON = "Director_ZoomInIcon";
    private const string ZOOMOUT_ICON = "Director_ZoomOutIcon";
    private const string TITLE_ICON = "SkillEditor_Title";

    // 绘制帧率;
    private const float FRAME_LIMITER = 1 / 60f;
    // 累计经理时间;
    private double accumulatedTime = 0f;
    //int popupSelection = 0;
    #endregion

    #region 文本定义
    const string CREATE = "创建";
    const float CREATE_BTN_WIDTH = 60;
    #endregion

    private DateTime previousTime;


    [MenuItem(MENU_ITEM, false, 10)]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SkillEditorWindow));
    }

    void Awake()
    {
        loadTextures();

#if UNITY_5 && !UNITY_5_0
        GUIContent titleContent = EditorGUIUtility.IconContent("WelcomeScreen.UnityBasicsLogo");
        titleContent.text = TITLE;
        base.titleContent = titleContent;//new GUIContent(TITLE, titleImage);
#else
        base.title = TITLE;
#endif
        this.minSize = new Vector2(400f, 250f);
    }

    void OnEnable()
    {
        EditorApplication.update = (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(SkillEditorUpdate));
        EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(PlaymodeStateChanged));

        previousTime = DateTime.Now;
    }
    
    void OnGUI()
    {
        //Rect toolbarArea = new Rect(0, 0, base.position.width, TOOLBAR_HEIGHT);
        //Rect controlArea = new Rect(0, TOOLBAR_HEIGHT, base.position.width, base.position.height - TOOLBAR_HEIGHT);

        updateToolbar();

//         if (GUILayout.Button("GUISkin编辑器", GUILayout.Width(120)))
//         {
//         }
    }

    /// <summary>
    /// 跟新菜单;
    /// </summary>
    void updateToolbar()
    {
//         EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
// 
//         //创建菜单下拉按钮;
//         if (GUILayout.Button(CREATE, EditorStyles.toolbarDropDown, GUILayout.Width(CREATE_BTN_WIDTH)))
//         {
//             GenericMenu createMenu = new GenericMenu();
//             createMenu.AddItem(new_cutscene, false, openCutsceneCreatorWindow);
// 
//             if (cutscene != null)
//             {
//                 createMenu.AddSeparator(string.Empty);
// 
//                 foreach (Type type in DirectorHelper.GetAllSubTypes(typeof(TrackGroup)))
//                 {
//                     TrackGroupContextData userData = getContextDataFromType(type);
//                     string text = string.Format(userData.Label);
//                     createMenu.AddItem(new GUIContent(text), false, new GenericMenu.MenuFunction2(AddTrackGroup), userData);
//                 }
//             }
// 
//             createMenu.DropDown(new Rect(5, TOOLBAR_HEIGHT, 0, 0));
//         }
// 
//         // btn
//         if (GUILayout.Button(zoomOutImage, EditorStyles.toolbarButton, GUILayout.Width(24)))
//         {
//             directorControl.ZoomOut();
//         }
// 
//         // toggle
//         Color temp = GUI.color;
//         GUI.color = directorControl.InPreviewMode ? Color.red : temp;
//         directorControl.InPreviewMode = GUILayout.Toggle(directorControl.InPreviewMode, PREVIEW_MODE, EditorStyles.toolbarButton, GUILayout.Width(150));
//         GUI.color = temp;
//         GUILayout.Space(10);
// 
//         EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 100fps
    /// </summary>
    void SkillEditorUpdate()
    {
        // Restrict the Repaint rate
        double delta = (DateTime.Now - previousTime).TotalSeconds;
        previousTime = DateTime.Now;
        if (delta > 0)
        {
            accumulatedTime += delta;
        }
        if (accumulatedTime >= FRAME_LIMITER)
        {
            base.Repaint();
            accumulatedTime -= FRAME_LIMITER;
        }

//         if (cutscene != null)
//         {
//             if (!Application.isPlaying && cutscene.State == Cutscene.CutsceneState.PreviewPlaying)
//             {
//                 cutscene.UpdateCutscene((float)delta);
//             }
//         }
    }

    /// <summary>
    /// play mode changed
    /// </summary>
    void PlaymodeStateChanged()
    { }

    #region 资源加载
    /// <summary>
    /// Load textures from Resources folder.
    /// </summary>
    private void loadTextures()
    {
        string suffix = EditorGUIUtility.isProSkin ? "_Light" : "_Dark";
        string missing = " is missing from Resources folder.";

        titleImage = Resources.Load<Texture>(TITLE_ICON + suffix);
        if (titleImage == null)
        {
            Debug.Log(TITLE_ICON + suffix + missing);
        }

        settingsImage = Resources.Load<Texture>(SETTINGS_ICON + suffix);
        if (settingsImage == null)
        {
            Debug.Log(SETTINGS_ICON + suffix + missing);
        }

        rescaleImage = Resources.Load<Texture>(HORIZONTAL_RESCALE_ICON + suffix);
        if (rescaleImage == null)
        {
            Debug.Log(HORIZONTAL_RESCALE_ICON + suffix + missing);
        }

        zoomInImage = Resources.Load<Texture>(ZOOMIN_ICON + suffix);
        if (zoomInImage == null)
        {
            Debug.Log(ZOOMIN_ICON + suffix + missing);
        }

        zoomOutImage = Resources.Load<Texture>(ZOOMOUT_ICON + suffix);
        if (zoomOutImage == null)
        {
            Debug.Log(ZOOMOUT_ICON + suffix + missing);
        }

        snapImage = Resources.Load<Texture>(MAGNET_ICON + suffix);
        if (snapImage == null)
        {
            Debug.Log(MAGNET_ICON + suffix + missing);
        }

        rollingEditImage = Resources.Load<Texture>("Director_RollingIcon");
        if (rollingEditImage == null)
        {
            Debug.Log("Rolling edit icon missing from Resources folder.");
        }

        rippleEditImage = Resources.Load<Texture>("Director_RippleIcon");
        if (rippleEditImage == null)
        {
            Debug.Log("Ripple edit icon missing from Resources folder.");
        }

        pickerImage = Resources.Load<Texture>(PICKER_ICON + suffix);
        if (pickerImage == null)
        {
            Debug.Log(PICKER_ICON + suffix + missing);
        }

        refreshImage = Resources.Load<Texture>(REFRESH_ICON + suffix);
        if (refreshImage == null)
        {
            Debug.Log(REFRESH_ICON + suffix + missing);
        }

        cropImage = Resources.Load<Texture>("Director_Resize_Crop" + suffix);
        if (cropImage == null)
        {
            Debug.Log("Director_Resize_Crop" + suffix + missing);
        }

        scaleImage = Resources.Load<Texture>("Director_Resize_Scale" + suffix);
        if (scaleImage == null)
        {
            Debug.Log("Director_Resize_Crop" + suffix + missing);
        }
    }
    #endregion
}
