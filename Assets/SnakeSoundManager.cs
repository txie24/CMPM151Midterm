using UnityEngine;
using System.Collections.Generic;
using UnityOSC;

public class SnakeSoundManager : MonoBehaviour
{
    private const string CLIENT_NAME = "PureData";

    void Start()
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

}