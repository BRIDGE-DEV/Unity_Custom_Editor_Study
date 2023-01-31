using UnityEngine;

[System.Serializable]
public class KeyInfo
{
    // public KeyCode Key
    // {
    //     get { return key;}
    //     set { key = value; }
    // }
    // public float Time
    // {
    //     get { return time;}
    //     set { time = value; }
    // }
    // public bool KeyStatus
    // {
    //     get { return keyStatus;}
    //     private set { keyStatus = value; }
    // }

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