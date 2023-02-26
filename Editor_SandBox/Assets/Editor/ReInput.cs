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
            Debug.Log($"[KHW] {evt.newValue}");
        });
    }

    private VisualElement CreateKeyInfoList()
    {
        var keyInfoListElement = new VisualElement
        {
            name = "keyInfo-list"
        };

        // // The "makeItem" function is called when the
        // // ListView needs more items to render.
        // Func<VisualElement> makeItem = () => new Label();
        //
        // // As the user scrolls through the list, the ListView object
        // // recycles elements created by the "makeItem" function,
        // // and invoke the "bindItem" callback to associate
        // // the element with the matching data item (specified as an index in the list).
        // Action<VisualElement, int> bindItem = (e, i) =>
        // {
        //     (e as Label).text = keyInfoList.keyInfos[i].key.ToString();
        //     e.contentContainer.Add(CreateKeyInfoFoldout(keyInfoList.keyInfos[i]));
        // };
        //
        // // Provide the list view with an explicit height for every row
        // // so it can calculate how many items to actually display
        // const int itemHeight = 16;
        //
        // var listView = new ListView(keyInfoList.keyInfos, itemHeight, makeItem, bindItem);
        //
        // listView.selectionType = SelectionType.Multiple;
        // listView.reorderMode = ListViewReorderMode.Animated;
        //
        // listView.onItemsChosen += objects => Debug.Log(objects);
        // listView.onSelectionChange += objects => Debug.Log(objects);
        //
        // listView.style.flexGrow = 1.0f;

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
    
    private void OnStartRecordingButtonClicked()
    {
        Debug.Log($"Start Recording Button");
    }
    
    private void OnStopRecordingButtonClicked()
    {
        Debug.Log($"Stop Recording Button");
    }
}