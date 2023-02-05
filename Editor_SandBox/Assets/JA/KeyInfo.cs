using UnityEngine;

[System.Serializable]
public struct KeyInfo
{
    public KeyCode key;
    public float time;
    public bool keyStatus;

    public KeyInfo(KeyCode key, float time, bool status)
    {
        this.key = key;
        this.time = time;
        this.keyStatus = status;
    }
}