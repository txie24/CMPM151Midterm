using UnityEngine;
using extOSC; // Required for extOSC

public class OSCManager : MonoBehaviour
{
    public static OSCManager Instance;

    [Header("OSC Component Reference")]
    [SerializeField] private OSCTransmitter transmitter;

    private void Start()
    {
        // Start the background music loop when the game begins
        Invoke(nameof(InitializeMusic), 0.1f);
    }

private void InitializeMusic()
    {
        SendTrigger("/musicStart"); 
        
        EvaluateMusicIntensity(); 
    }

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

public void EvaluateMusicIntensity()
    {
        int targetIntensity = 0;
        
        // 1. Check Generators (Intensity 1)
        var generators = FindObjectsByType<GeneratorShakeToggle>(FindObjectsSortMode.None);
        foreach(var gen in generators) 
        { 
            if (gen.IsOn) 
            {
                targetIntensity = 1;
                break; 
            }
        }

        // 2. Check Orbs (Intensity 2) - This will overwrite the Generator if an Orb is on!
        var orbs = FindObjectsByType<OrbParticleToggle>(FindObjectsSortMode.None);
        foreach(var orb in orbs) 
        { 
            if (orb.IsOn) 
            {
                targetIntensity = 2;
                break; 
            }
        }

        // Send the final calculated intensity to Pure Data
        SendInt("/musicIntensity", targetIntensity);
    }

    private void OnApplicationQuit()
    {
        if (transmitter == null) return;

        // Send a 0 to /musicStart. 
        // In your MC.pd patch, this routes to [s musicRun] and turns off the [metro 750]
        SendInt("/musicStart", 0);

        SendFloat("/portalDistance", 0f);
        // Force the game to stay alive for 100 milliseconds
        // This guarantees the UDP packet is sent over the network before the game closes
        System.Threading.Thread.Sleep(100);
        
        Debug.Log("Sent music stop command and shutting down.");
    }
}