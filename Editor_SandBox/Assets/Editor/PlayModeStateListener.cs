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
                if (ReInput.IsRecording)
                {
                    ReInput.OnStartRecordingButtonClicked();
                }
                else if (ReInput.IsReInputing)
                {
                    ReInput.OnStartReInputButtonClicked();
                }
                break; 
            case PlayModeStateChange.ExitingPlayMode:
                EditorUtility.RequestScriptReload();

                if (ReInput.IsRecording)
                {
                    ReInput.OnStopRecordingButtonClicked();
                }
                else if (ReInput.IsReInputing)
                {
                    ReInput.OnStopReInputButtonClicked();
                }

                ReInput.IsRecording = false;
                ReInput.IsReInputing = false;
                break;
        }
    }
}