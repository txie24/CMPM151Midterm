using UnityEngine;
using extOSC; // Required for extOSC

public class OSCManager : MonoBehaviour
{
    public static OSCManager Instance;

    [Header("OSC Component Reference")]
    [SerializeField] private OSCTransmitter transmitter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Auto-get transmitter if attached to the same GameObject
        if (transmitter == null) transmitter = GetComponent<OSCTransmitter>();
    }

    // Method to send a simple trigger (Used by Crystal & Machine)
    public void SendTrigger(string address)
    {
        if (transmitter == null) return;

        var message = new OSCMessage(address);
        // Pure Data likes receiving a simple 'bang' or a 1 to trigger sounds
        message.AddValue(OSCValue.Int(1)); 
        transmitter.Send(message);
        Debug.Log($"OSC Sent Trigger: {address}");
    }

    // Method to send an integer (Used by Music Intensity Zones)
    public void SendInt(string address, int value)
    {
        if (transmitter == null) return;

        var message = new OSCMessage(address);
        message.AddValue(OSCValue.Int(value));
        transmitter.Send(message);
        Debug.Log($"OSC Sent Int: {address} ({value})");
    }

    // Method to send a float (Used by Portal Distance)
    public void SendFloat(string address, float value)
    {
        if (transmitter == null) return;

        var message = new OSCMessage(address);
        message.AddValue(OSCValue.Float(value));
        transmitter.Send(message);
    }
}