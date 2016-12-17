using UnityEngine;
using WiimoteApi;

public class UseWiimote : MonoBehaviour
{
    public Vector3 targetRotation;
    public float averageX = 0;
    public float averageZ = 0;

    public bool UpdateCamera = false;

    public float WiiDelay = 1f / 60f;
    
    public bool ScanForWiimotes = true;
    private Wiimote mote = null;
    private bool moteReady = false;

    private AnimatedPhysics ap;
    private FlipCamera fc;

    // HACK: WHAT AM I EVEN DOING
    // TODO: FIX THIS
    private Vector3 crotation;
    private float chuckLastX;
    private float chuckLastY;
    private Transform nunchuck;
    private Interact nunchuckTouch;

    #region Unity
    /// <summary>
    /// on Start , call Scan every .2 seconds
    /// </summary>
    private void Start()
    {
        this.nunchuck = GameObject.Find("Nunchuck").transform;
        this.nunchuckTouch = this.nunchuck.GetComponent<Interact>();

        if (this.ScanForWiimotes)
        {
            InvokeRepeating("ScanForRemotes", .2f, .2f);
        }

        this.ap = GetComponent<AnimatedPhysics>();
        this.fc = GetComponent<FlipCamera>();
    }
    
    private void Update()
    {
        // If the remote is not yet available, don't process it's data
        if (this.mote == null)
        {
            return;
        }

        // HACK: USE THE POWER OF THE MIND TO ROTATE AN OBJECT BEFORE ME
        if (this.mote.current_ext == ExtensionController.NUNCHUCK)
        {
            var data = this.mote.Nunchuck.accel;

            // Catch when nunchuck is sending 0s still
            if (data[0] < 100 || data[1] < 100)
            {
                return;
            }

            // Deal with both rotations for the "curosr"
            this.crotation.y -= (this.chuckLastY - data[0]) / 2f;
            this.crotation.x -= (this.chuckLastX - data[1]) / 2f;
            this.chuckLastY = data[0];
            this.chuckLastX = data[1];

            this.nunchuck.rotation = Quaternion.Lerp(this.nunchuck.rotation, this.transform.rotation * Quaternion.Euler(this.crotation), 5f * Time.deltaTime);

            // TODO: MOVE TO INIT
            this.nunchuckTouch.Target = this.fc.camera;
        }
    }

    /// <summary>
    /// once a frame update our camera's rotation
    /// </summary>
    private void LateUpdate()
    {
        if (this.UpdateCamera)
        {
            Quaternion target = this.transform.rotation * Quaternion.Euler(targetRotation);
            Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, target, 15f);
            
            if (this.mote != null)
            {
                // Nunchuck controls
                if (this.mote.current_ext == ExtensionController.NUNCHUCK)
                {
                    float[] stick = this.mote.Nunchuck.GetStick01();
                    this.ap.GoForward = stick[1] > 0.6f;
                    this.ap.GoLeft = stick[0] < 0.4f;
                    this.ap.GoRight = stick[0] > 0.6f;
                }
                else
                {
                    // Wiimote solo controls
                    this.ap.GoForward = this.mote.Button.d_up;
                    this.ap.GoLeft = this.mote.Button.d_left;
                    this.ap.GoRight = this.mote.Button.d_right;
                }

                this.ap.DoJump = this.mote.Button.a;
                this.nunchuckTouch.WantsToGrab = this.mote.Button.b;
                
                if (this.mote.Button.plus)
                {
                    this.fc.ChangeCamera = true;
                }
            }
        }
    }

    /// <summary>
    /// clean up the wiimote on exit
    /// </summary>
    private void OnApplicationQuit()
    {
        if (this.mote != null)
        {
            WiimoteManager.Cleanup(this.mote);
            this.mote = null;
        }
    }
    #endregion

    /// <summary>
    /// called every .2 seconds until a Wiimote is found
    /// </summary>
    private void ScanForRemotes()
    {
        this.ScanForWiimotes = true;
        WiimoteManager.FindWiimotes();
        if (WiimoteManager.HasWiimote())
        {
            this.ScanForWiimotes = false;
            CancelInvoke("ScanForRemotes");
            Invoke("EnableMote", .5f);
        }
    }
    
    /// <summary>
    /// after a delay enable the mote
    /// </summary>
    private void EnableMote()
    {
        this.targetRotation = Camera.main.transform.rotation.eulerAngles;
        Debug.Log("Found a Wiimote!  Setting it up...");
        this.mote = WiimoteManager.Wiimotes[0];
        this.mote.SendPlayerLED(true, false, false, false);
        this.mote.SendDataReportMode(
            InputDataType.REPORT_BUTTONS_ACCEL_EXT16
        );

        this.moteReady = true;

        InvokeRepeating("UseMoteData", .5f, this.WiiDelay);
    }

    private void UseMoteData()
    {
        // If we're not scanning, but don't have a Wiimote; scan.
        if (!this.ScanForWiimotes && this.mote == null)
        {
            Debug.LogWarning("No Wiimote found (connection lost?)  Reconnecting.");
            InvokeRepeating("ScanForRemotes", .1f, .1f);
            this.ScanForWiimotes = true;
        }

        // Use the mote data
        if (this.mote != null && moteReady)
        {
            // Checks how many new events we have to process
            var ret = this.mote.ReadWiimoteData();

            // Process every wiimote event that's available
            do
            {
                var accel = this.mote.Accel.GetCalibratedAccelData();
                float accel_x = accel[1];
                float accel_z = accel[0];

                // Bring our averages for calibration
                this.averageX = Mathf.Lerp(this.averageX, accel_x, this.WiiDelay / 5f);
                this.averageZ = Mathf.Lerp(this.averageZ, accel_z, this.WiiDelay / 5f);
                
                // Pull values from wiimote
                this.targetRotation.x = this.averageX * 40f - 20f;
                this.targetRotation.y = this.averageZ * 40f - 20f;
                
                // Clamp the camera
                this.targetRotation.x = Mathf.Clamp(this.targetRotation.x, -30, 50);
                this.targetRotation.y = Mathf.Clamp(this.targetRotation.y, -60, 60);
                this.targetRotation.z = 0;
                
                // We processed one event
                ret--;
            } while (ret > 0);
        }
    }
}
