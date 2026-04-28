using UnityEngine;
using System.Collections.Generic;
using UnityOSC;

public class SnakeSoundManager : MonoBehaviour
{
    private const string CLIENT_NAME = "PureData";

    void Awake()
    {
        // Initialize the OSC Client: (Client Name, IP Address, Port)
        OSCHandler.Instance.Init();
        OSCHandler.Instance.CreateClient(CLIENT_NAME, System.Net.IPAddress.Parse("127.0.0.1"), 8000);
    }

    // Call this method from your Snake's "Eat" logic
    public void TriggerAppleEatSound()
    {
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/apple/eat", "bang");
    }

    public void SetChannelVolume(string channel, float volume)
    {
        // Address becomes /apple/pulse1, /apple/tri, etc.
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/apple/" + channel, volume);
    }

    public void TriggerGameOverSound()
    {
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/game/over", "bang");
    }

    public void SendAppleCount(int count)
    {
        // Sends the integer to drive the BPM math in Pure Data
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/apple/count", count);
    }

    public void TriggerGameStart()
    {
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/game/start", "toggle");
    }

    public void TriggerGameRestart()
    {
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/game/restart", "bang");
    }

    private void OnApplicationQuit()
    {
        OSCHandler.Instance.SendMessageToClient(CLIENT_NAME, "/game/over", "bang");
    }


}