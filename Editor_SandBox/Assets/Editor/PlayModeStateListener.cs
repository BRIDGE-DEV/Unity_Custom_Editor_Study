using UnityEditor;

[InitializeOnLoad]
public static class PlayModeStateListener
{
    static PlayModeStateListener()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                if (ReInput.IsAutoRecording && !ReInput.IsReInputing)
                {
                    ReInput.OnStartRecordingButtonClicked();
                }

                break; 
            case PlayModeStateChange.ExitingPlayMode:
                ReInput.IsReInputing = false;
                EditorUtility.RequestScriptReload();

                break;
        }
    }
}