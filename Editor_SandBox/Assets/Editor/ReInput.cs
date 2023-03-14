using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public class ReInput : EditorWindow
{
#region Const
    private const string KEYINFOLIST_FILE_NAME = "KeyInfoList.json";
    private const string PLAYER_PREFS_KEY_IS_RECORDING = "REINPUT_RECORDING";
    private const string PLAYER_PREFS_KEY_IS_REINPUT = "REINPUT_REINPUT";
#endregion

#region File Manage
    private KeyInfoList keyInfoList;
    private FileDataHandler fileDataHandler;

    private static string assetPath;

    private static string AssetPath
    {
        get
        {
            return assetPath ??= AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:Script {nameof(ReInput)}")[0]).Split("/Editor/ReInput")[0];
        }
    }
#endregion

#region Key Input Manage
    private static GameObject inputStore;
    private static GameObject outputStore;
    
    private static RadioButtonGroup recordingButtonGroup;
    private static RadioButtonGroup reInputButtonGroup;

    public static bool IsRecording
    {
        get => PlayerPrefs.GetInt(PLAYER_PREFS_KEY_IS_RECORDING, 0) == 1;
        set => PlayerPrefs.SetInt(PLAYER_PREFS_KEY_IS_RECORDING, value ? 1 : 0);
    }

    public static bool IsReInputing
    {
        get => PlayerPrefs.GetInt(PLAYER_PREFS_KEY_IS_REINPUT, 0) == 1;
        set => PlayerPrefs.SetInt(PLAYER_PREFS_KEY_IS_REINPUT, value ? 1 : 0);
    }
#endregion

#region Initialization
    [MenuItem("Window/ReInput")]
    public static void ShowWindow()
    {
        IsRecording = false;
        IsReInputing = false;

        var reInput = GetWindow<ReInput>("Custom Package - ReInput");
        reInput.titleContent = new GUIContent("ReInput");
    }

    private void OnEnable()
    {
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, KEYINFOLIST_FILE_NAME);
        keyInfoList = fileDataHandler.Load();

        rootVisualElement.Add(CreateKeyInfoList());
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{AssetPath}/Editor/ReInput.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();

        rootVisualElement.Add(labelFromUXML);
        
        BindingUXMLElements();
    }

    private VisualElement CreateKeyInfoList()
    {
        var keyInfoListElement = new VisualElement
        {
            name = "keyInfo-list",
            style =
            {
                width = StyleKeyword.Auto,
                height = StyleKeyword.Auto,
                backgroundColor = Color.black
            }
        };

        foreach (var keyInfo in keyInfoList.keyInfos)
        {
            keyInfoListElement.contentContainer.Add(CreateKeyInfoFoldout(keyInfo));
        }
        
        var scrollView = new ScrollView
        {
            style =
            {
                width = StyleKeyword.Auto,
                height = Length.Percent(90)
            }
        };

        scrollView.contentContainer.Add(keyInfoListElement);
        
        return scrollView;
    }

    private VisualElement CreateKeyInfoFoldout(KeyInfo keyInfo)
    {
        var keyInfoElement = new VisualElement
        {
            name = "keyInfo-element"
        };

        var keyCodeField = new TextField("Key")
        {
            value = keyInfo.key.ToString()
        };

        keyCodeField.RegisterValueChangedCallback(evt => {
            var targetKeyInfo = keyInfoList.keyInfos.Find(info => info.time.Equals(keyInfo.time));
            targetKeyInfo.key = (KeyCode)Enum.Parse(typeof(KeyCode), evt.newValue);
        });

        keyInfoElement.Add(keyCodeField);

        var timeField = new FloatField("Time")
        {
            value = keyInfo.time
        };

        timeField.RegisterValueChangedCallback(evt => {
            var targetKeyInfo = keyInfoList.keyInfos.Find(info => info.time.Equals(keyInfo.time));
            targetKeyInfo.time = evt.newValue;
        });

        keyInfoElement.Add(timeField);

        var keyStatusField = new Toggle("KeyStatus")
        {
            value = keyInfo.keyStatus
        };

        keyStatusField.RegisterValueChangedCallback(evt => {
            var targetKeyInfo = keyInfoList.keyInfos.Find(info => info.time.Equals(keyInfo.time));
            targetKeyInfo.keyStatus = evt.newValue;
        });

        keyInfoElement.Add(keyStatusField);

        var foldout = new Foldout
        {
            text = keyInfo.key.ToString(),
            value = true
        };

        foldout.contentContainer.Add(keyInfoElement);

        return foldout;
    }
#endregion

#region UI Event Binding
    private void BindingUXMLElements()
    {
        var saveButton = rootVisualElement.Q<Button>("button-saveData");
        saveButton.clicked += OnSaveButtonClicked;
        
        var resetButton = rootVisualElement.Q<Button>("button-resetData");
        resetButton.clicked += OnResetButtonClicked;

        recordingButtonGroup = rootVisualElement.Q<RadioButtonGroup>("radiobuttongroup-recording");
        
        recordingButtonGroup.choices = new List<string> { "Start", "Stop" };
        recordingButtonGroup.value = 1;
        
        recordingButtonGroup.RegisterValueChangedCallback(evt =>
        {
            if (IsReInputing)
            {
                Debug.LogError($"[ReInput] It's already reInputing now!");
                recordingButtonGroup.value = evt.previousValue;
                return;
            }

            IsRecording = evt.newValue == 0;

            if (IsRecording)
            {
                if (EditorApplication.isPlaying)
                {
                    OnStartRecordingButtonClicked();
                }
                else
                {
                    EditorApplication.EnterPlaymode();                    
                }
            }
            else
            {
                EditorApplication.ExitPlaymode();
                OnStopRecordingButtonClicked();
            }
        });
        
        reInputButtonGroup = rootVisualElement.Q<RadioButtonGroup>("radiobuttongroup-reInput");
        
        reInputButtonGroup.choices = new List<string> { "Start", "Stop" };
        reInputButtonGroup.value = 1;
        
        reInputButtonGroup.RegisterValueChangedCallback(evt =>
        {
            if (IsRecording)
            {
                Debug.LogError($"[ReInput] It's already recording now!");
                reInputButtonGroup.value = evt.previousValue;
                return;
            }
            
            IsReInputing = evt.newValue == 0;

            if (IsReInputing)
            {
                if (EditorApplication.isPlaying)
                {
                    OnStartReInputButtonClicked();
                }
                else
                {
                    EditorApplication.EnterPlaymode();                    
                }
            }
            else
            {
                EditorApplication.ExitPlaymode();
                OnStopReInputButtonClicked();
            }
        });
    }

    private void OnSaveButtonClicked()
    {
        try
        {
            keyInfoList.keyInfos.Sort((info1, info2) => info1.time.CompareTo(info2.time));
            fileDataHandler.Save(keyInfoList);
            
            Debug.Log("[ReInput] Save Success!");
            
            EditorUtility.RequestScriptReload();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    private void OnResetButtonClicked()
    {
        try
        {
            keyInfoList.keyInfos.Clear();
            fileDataHandler.Save(keyInfoList);
            
            Debug.Log("[ReInput] Reset Success!");
            
            EditorUtility.RequestScriptReload();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static void OnStartRecordingButtonClicked()
    {
        if (inputStore != null || outputStore != null)
        {
            return;
        }
        
        Debug.Log($"[ReInput] Start Recording!");

        var inputStorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AssetPath}/Runtime/InputStore.prefab");
        inputStore = Instantiate(inputStorePrefab);
        
        recordingButtonGroup.value = 0;

        reInputButtonGroup.SetEnabled(false);
    }
    
    public static void OnStopRecordingButtonClicked()
    {
        if (inputStore == null)
        {
            Debug.LogError($"[ReInput] There's no input system now.");
            return;
        }
        
        Debug.Log($"[ReInput] Stop Recording!");

        Destroy(inputStore);
        inputStore = null;

        recordingButtonGroup.value = 1;

        reInputButtonGroup.SetEnabled(true);
    }
    
    public static void OnStartReInputButtonClicked()
    {
        if (inputStore != null || outputStore != null)
        {
            return;
        }
        
        Debug.Log($"[ReInput] Start ReInput!");

        var outputStorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AssetPath}/Runtime/OutputStore.prefab");
        outputStore = Instantiate(outputStorePrefab);
        
        reInputButtonGroup.value = 0;

        recordingButtonGroup.SetEnabled(false);
    }
    
    public static void OnStopReInputButtonClicked()
    {
        if (outputStore == null)
        {
            Debug.LogError($"[ReInput] There's no output system now.");
            return;
        }

        Debug.Log($"[ReInput] Stop ReInput!");

        Destroy(outputStore);
        outputStore = null;
        
        reInputButtonGroup.value = 1;

        recordingButtonGroup.SetEnabled(true);
    }
#endregion
}