using System.Collections.Generic;
using UnityEngine;

public class UserInputStore : MonoBehaviour
{
    private KeyInfoList keyInfoList;

    private List<KeyCode> activeInputs;
    private FileDataHandler fileDataHandler;

    private const string fileName = "KeyInfoList";
    private void Awake()
    {
        DontDestroyOnLoad(this);

        keyInfoList = new KeyInfoList();
        keyInfoList.keyInfos = new List<KeyInfo>();
        
        activeInputs = new List<KeyCode>();
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
    }

    public void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach(KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code))
                {
                    keyInfoList.keyInfos.Add(new KeyInfo(code, Time.time, true));
                    
                    activeInputs.Add(code);
                }
            }
        }

        if (activeInputs.Count > 0)
        {
            for (int i = 0; i < activeInputs.Count; i++)
            {
                if (Input.GetKeyUp(activeInputs[i]))
                {
                    keyInfoList.keyInfos.Add(new KeyInfo(activeInputs[i], Time.time, false));
                    activeInputs.Remove(activeInputs[i]);
                }
            }
        }
    }

    private void OnDisable()
    {
        fileDataHandler.Save(keyInfoList);
        
        Destroy(this);
    }
}