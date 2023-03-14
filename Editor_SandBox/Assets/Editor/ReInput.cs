using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public class ReInput : EditorWindow
{
#region Const
    private const string KEYINFOLIST_FILE_NAME = "KeyInfoList.json";
    private const string PLAYER_PREFS_KEY_AUTO_RECORDING = "REINPUT_AUTO_RECORDING";
    private const string PLAYER_PREFS_KEY_REINPUT = "REINPUT_REINPUT";
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
    private GameObject outputStore;

    public static bool IsAutoRecording
    {
        get => PlayerPrefs.GetInt(PLAYER_PREFS_KEY_AUTO_RECORDING, 0) != 0;
        private set => PlayerPrefs.SetInt(PLAYER_PREFS_KEY_AUTO_RECORDING, value ? 1 : 0);
    }

    public static bool IsReInputing
    {
        get => PlayerPrefs.GetInt(PLAYER_PREFS_KEY_REINPUT, 0) != 0;
        set => PlayerPrefs.SetInt(PLAYER_PREFS_KEY_REINPUT, value ? 1 : 0);
    }
#endregion

#region Initialization
    [MenuItem("Window/ReInput")]
    public static void ShowWindow()
    {
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
                height = Length.Percent(85)
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
        
        var startRecordingRadioButton = rootVisualElement.Q<RadioButton>("radiobutton-startRecording");
        
        startRecordingRadioButton.RegisterCallback<ChangeEvent<bool>>((evt) =>
        {
            if (!evt.newValue)
            {
                return;
            }

            startRecordingRadioButton.value = OnStartRecordingButtonClicked();
        });
        
        var stopRecordingRadioButton = rootVisualElement.Q<RadioButton>("radiobutton-stopRecording");
        
        stopRecordingRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            stopRecordingRadioButton.value = evt.newValue;

            if (evt.newValue)
            {
                OnStopRecordingButtonClicked();                
            }
        });

        var autoRecordingToggle = rootVisualElement.Q<Toggle>("toggle-autoRecording");
        autoRecordingToggle.value = IsAutoRecording;

        autoRecordingToggle.RegisterValueChangedCallback(evt =>
        {
            IsAutoRecording = evt.newValue;
        });
        
        var startReInputRadioButton = rootVisualElement.Q<RadioButton>("radiobutton-startReInput");
        
        startReInputRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (!evt.newValue)
            {
                return;
            }

            IsReInputing = true;
            startReInputRadioButton.value = OnStartReInputButtonClicked();
        });
        
        var stopReInputRadioButton = rootVisualElement.Q<RadioButton>("radiobutton-stopReInput");
        
        stopReInputRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            stopReInputRadioButton.value = evt.newValue;

            if (!evt.newValue)
            {
                return;
            }

            IsReInputing = false;
            OnStopReInputButtonClicked();
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
    
    public static bool OnStartRecordingButtonClicked()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError($"[ReInput] You can only used this button when you playing in editor.");
            return false;
        }

        if (inputStore != null)
        {
            Debug.LogError($"[ReInput] It's already playing input now!");
            return false;
        }
        
        Debug.Log($"[ReInput] Start Recording!");

        var inputStorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AssetPath}/Runtime/InputStore.prefab");
        inputStore = Instantiate(inputStorePrefab);

        return true;
    }
    
    private static void OnStopRecordingButtonClicked()
    {
        if (inputStore == null)
        {
            Debug.LogError($"[ReInput] There's no input system now.");
            return;
        }
        
        Debug.Log($"[ReInput] Stop Recording!");

        Destroy(inputStore);
        inputStore = null;
    }
    
    private bool OnStartReInputButtonClicked()
    {
        if (outputStore != null)
        {
            Debug.LogError($"[ReInput] It's already playing output now!");
            return false;
        }
        
        Debug.Log($"[ReInput] Start ReInput!");
        
        EditorApplication.EnterPlaymode();

        var outputStorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AssetPath}/Runtime/OutputStore.prefab");
        outputStore = Instantiate(outputStorePrefab);

        return true;
    }
    
    private void OnStopReInputButtonClicked()
    {
        if (outputStore == null)
        {
            Debug.LogError($"[ReInput] There's no output system now.");
            return;
        }

        Debug.Log($"[ReInput] Stop ReInput!");

        Destroy(outputStore);
        outputStore = null;
    }
#endregion
}