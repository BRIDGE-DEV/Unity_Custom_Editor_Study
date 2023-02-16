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
        ReInput reInput = GetWindow<ReInput>("Custom Package - ReInput");
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
        
        Button saveButton = rootVisualElement.Q<Button>("save-button");
        saveButton.clicked += OnSaveButtonClicked;
    }

    private VisualElement CreateKeyInfoList()
    {
        var keyInfoListElement = new VisualElement();
        keyInfoListElement.name = "keyInfo-list";

        foreach (KeyInfo keyInfo in keyInfoList.keyInfos)
        {
            keyInfoListElement.Add(CreateKeyInfoFoldout(keyInfo));
        }
        
        var foldOut = new Foldout();
        foldOut.text = "Input Key List";
        foldOut.value = false;
        foldOut.contentContainer.Add(keyInfoListElement);

        return foldOut;
    }

    private Foldout CreateKeyInfoFoldout(KeyInfo keyInfo)
    {
        var keyInfoElement = new VisualElement();
        keyInfoElement.name = "keyInfo-element";
        
        var keyCodeField = new TextField("Key");
        keyCodeField.value = keyInfo.key.ToString();
        keyCodeField.RegisterValueChangedCallback(evt => {
            var targetKeyInfo = keyInfoList.keyInfos.Find(info => info.time.Equals(keyInfo.time));
            targetKeyInfo.key = (KeyCode)Enum.Parse(typeof(KeyCode), evt.newValue);
        });

        keyInfoElement.Add(keyCodeField);

        var timeField = new FloatField("Time");
        timeField.value = keyInfo.time;
        timeField.RegisterValueChangedCallback(evt => {
            var targetKeyInfo = keyInfoList.keyInfos.Find(info => info.time.Equals(keyInfo.time));
            targetKeyInfo.time = evt.newValue;
        });

        keyInfoElement.Add(timeField);

        var keyStatusField = new Toggle("KeyStatus");
        keyStatusField.value = keyInfo.keyStatus;
        keyStatusField.RegisterValueChangedCallback(evt => {
            var targetKeyInfo = keyInfoList.keyInfos.Find(info => info.time.Equals(keyInfo.time));
            targetKeyInfo.keyStatus = evt.newValue;
        });

        keyInfoElement.Add(keyStatusField);
        
        var foldout = new Foldout();

        foldout.text = keyInfo.key.ToString();
        foldout.value = true;
        foldout.contentContainer.Add(keyInfoElement);

        return foldout;
    }

    private void OnSaveButtonClicked()
    {
        try
        {
            fileDataHandler.Save(keyInfoList);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}