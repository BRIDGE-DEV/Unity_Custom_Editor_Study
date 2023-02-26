using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public class ReInput : EditorWindow
{
    private const string fileName = "KeyInfoList.json";

    private KeyInfoList keyInfoList;
    private FileDataHandler fileDataHandler;

    private static GameObject inputStore;
    private GameObject outputStore;
    public static bool IsAutoRecording { get; private set; }

    [MenuItem("Window/ReInput")]
    public static void ShowWindow()
    {
        var reInput = GetWindow<ReInput>("Custom Package - ReInput");
        reInput.titleContent = new GUIContent("ReInput");
    }

    private void OnEnable()
    {
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);

        keyInfoList = fileDataHandler.Load();
        rootVisualElement.Add(CreateKeyInfoList());
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ReInput.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        rootVisualElement.Add(labelFromUXML);
        
        var saveButton = rootVisualElement.Q<Button>("button-saveJson");
        saveButton.clicked += OnSaveButtonClicked;
        
        var startRecordingButton = rootVisualElement.Q<Button>("button-startRecording");
        startRecordingButton.clicked += OnStartRecordingButtonClicked;
        
        var stopRecordingButton = rootVisualElement.Q<Button>("button-stopRecording");
        stopRecordingButton.clicked += OnStopRecordingButtonClicked;

        var autoRecordingToggle = rootVisualElement.Q<Toggle>("toggle-autoRecoding");

        autoRecordingToggle.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"[KHW] toggleValue : {evt.newValue}");

            IsAutoRecording = evt.newValue;
        });
        
        var startReInputButton = rootVisualElement.Q<Button>("button-startReInput");
        startRecordingButton.clicked += OnStartReInputButtonClicked;
        
        var stopReInputButton = rootVisualElement.Q<Button>("button-stopReInput");
        stopRecordingButton.clicked += OnStopReInputButtonClicked;
    }

    private VisualElement CreateKeyInfoList()
    {
        var keyInfoListElement = new VisualElement
        {
            name = "keyInfo-list"
        };

        foreach (var keyInfo in keyInfoList.keyInfos)
        {
            keyInfoListElement.Add(CreateKeyInfoFoldout(keyInfo));
        }
        
        var foldOut = new Foldout
        {
            text = "Input Key List",
            value = false
        };

        foldOut.contentContainer.Add(keyInfoListElement);

        // return listView;
        return foldOut;
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
            value = false
        };

        foldout.contentContainer.Add(keyInfoElement);

        return foldout;
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
    
    public static void OnStartRecordingButtonClicked()
    {
        Debug.Log($"[ReInput] Start Recording!");

        var inputStorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/JA/InputStore.prefab");
        inputStore = Instantiate(inputStorePrefab);
    }
    
    public static void OnStopRecordingButtonClicked()
    {
        Debug.Log($"[ReInput] Stop Recording!");
        
        Destroy(inputStore);
    }
    
    private void OnStartReInputButtonClicked()
    {
        Debug.Log($"[ReInput] Start ReInput!");

        var outputStorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/JA/OutputStore.prefab");
        outputStore = Instantiate(outputStorePrefab);
    }
    
    private void OnStopReInputButtonClicked()
    {
        Debug.Log($"[ReInput] Stop ReInput!");
        
        Destroy(outputStore);
    }
}

[InitializeOnLoad]
public static class PlayModeStateListener
{
    static PlayModeStateListener()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state is PlayModeStateChange.EnteredPlayMode)
        {
            if (ReInput.IsAutoRecording)
            {
                ReInput.OnStartRecordingButtonClicked();
            }
        }
    }
}