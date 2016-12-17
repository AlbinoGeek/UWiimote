using System.Collections;
using UnityEngine;
using WiimoteApi;

namespace Mewzor
{
    public class InputManager : MonoBehaviour
    {
        private Transform OBJECTHOLDER;

        public bool PlayerReady = false;

        public Color PrimaryColor;
        public Color SecondaryColor;
        
        public int Health = 5;

        public float ConsumeEventEvery = .01f;
        public float MotionSpeed = 60f;

        public int PlayerID = 1;

        private GUIStyle style;

        private float averageX;
        private float averageZ;

        private bool invokingScan = false;

        private Vector3 lookRotation = Vector3.zero;

        private float lookPitch = 0f;
        private float lookYaw = 0f;

        private int colorDelay = 1;
        private int fireDelay = 30;
        private int readyDelay = 1;

        private Wiimote wiimote;
        private bool wiimoteReady = false;

        private GameManager gm;

        private Transform mound;
        private Transform barrelMount;
        private int nextBarrel = 1;

        private Vector3 heading;
        private Vector3 bearing;

        private new Rigidbody rigidbody;

        #region Unity
        /// <summary>
        /// start the consumption of events after 1s
        /// </summary>
        private void Start()
        {
            this.rigidbody = this.GetComponent<Rigidbody>();
            this.OBJECTHOLDER = GameObject.Find("Holder").transform;

            this.mound = this.transform.Find("Mound");
            this.barrelMount = this.mound.Find("BarrelMount");

            this.gm = GameObject.Find("Control").GetComponent<GameManager>();

            this.style = new GUIStyle();
            this.style.alignment = TextAnchor.MiddleCenter;
            this.style.fontSize = 36;

            InvokeRepeating("DecrementTimers", 1f, .1f);
            InvokeRepeating("ConsumeWiimoteEvents", 1f, this.ConsumeEventEvery);

            this.ChangeColor();
        }

        /// <summary>
        /// clean up the wiimote on exit
        /// </summary>
        private void OnApplicationQuit()
        {
            if (this.wiimote != null)
            {
                WiimoteManager.Cleanup(this.wiimote);
                this.wiimote = null;
            }
        }

        private void OnGUI()
        {
            // Pre-Game UI
            if (this.gm.State == GameManager.GameState.Waiting)
            {
                this.style.fontStyle = this.PlayerReady ? FontStyle.Bold : FontStyle.Italic;
                this.style.normal.textColor = this.PlayerReady ? Color.green : Color.red;

                int x = 0;
                string text = this.PlayerReady ? "Ready" : "Not Ready";
                if (!this.wiimoteReady || this.wiimote == null)
                {
                    this.style.normal.textColor = Color.yellow + Color.blue;
                    text = "No Mote!";
                }

                if (this.PlayerID == 2)
                {
                    x = Screen.width - 300;
                }

                GUI.Label(new Rect(x, 0, 300, 50), text, this.style);
            }
        }

        /// <summary>
        /// actually move the character on physics frames
        /// </summary>
        private void FixedUpdate()
        {
            // If we have a heading, apply it
            if (this.heading != Vector3.zero)
            {
                Vector3 target = this.transform.TransformDirection(this.heading);
                this.rigidbody.position = Vector3.MoveTowards(this.rigidbody.position, this.rigidbody.position + target, Time.fixedDeltaTime * 2.5f);
            }
            
            // If we have a bearing, apply it
            if (this.bearing != Vector3.zero)
            {
                this.rigidbody.rotation = Quaternion.Lerp(this.rigidbody.rotation, this.rigidbody.rotation * Quaternion.Euler(this.bearing * 3f), Time.fixedDeltaTime * 40f);
            }
        }

        /// <summary>
        /// take controls and move camera
        /// </summary>
        private void Update()
        {
            // If we just died
            if (this.Health == 0)
            {
                StartCoroutine("Die");
                this.Health--;
            }

            // If we are dead and playing
            if (this.Health <= 0 && this.gm.State == GameManager.GameState.Playing)
            {
                return;
            }

            this.lookYaw = this.transform.rotation.eulerAngles.y + this.lookRotation.y;
            this.lookPitch = this.transform.rotation.eulerAngles.x + this.lookRotation.x;

            // Apply our Yaw
            Vector3 rot = this.mound.rotation.eulerAngles;
            rot.y = this.lookYaw;
            this.mound.rotation = Quaternion.Lerp(this.mound.rotation, Quaternion.Euler(rot), .5f);

            // Apply our Pitch
            rot = this.barrelMount.rotation.eulerAngles;
            rot.x = this.lookPitch;
            this.barrelMount.rotation = Quaternion.Lerp(this.barrelMount.rotation, Quaternion.Euler(rot), .5f);

            // Take controls during update
            if (this.wiimote != null && this.wiimoteReady)
            {
                // Wait mode controls
                if (this.gm.State == GameManager.GameState.Waiting)
                {
                    // Toggle ready status with +
                    if (this.wiimote.Button.plus && this.readyDelay <= 0)
                    {
                        this.PlayerReady = !this.PlayerReady;
                        this.readyDelay = 3;
                    }

                    // Change our color with 2
                    if (this.wiimote.Button.two && !this.PlayerReady && this.colorDelay <= 0)
                    {
                        this.ChangeColor();
                    }
                }

                // Play mode controls
                if (this.gm.State == GameManager.GameState.Playing)
                {
                    // Move with the d-pad
                    if (this.wiimote.Button.d_up)
                    {
                        this.heading = Vector3.forward;
                    }
                    else if (this.wiimote.Button.d_down)
                    {
                        this.heading = Vector3.back;
                    }
                    else
                    {
                        this.heading = Vector3.zero;
                    }
                        
                    if (this.wiimote.Button.d_left)
                    {
                        this.bearing = Vector3.down;
                    }
                    else if (this.wiimote.Button.d_right)
                    {
                        this.bearing = Vector3.up;
                    }
                    else
                    {
                        this.bearing = Vector3.zero;
                    }

                    // Fire with the A button
                    if (this.wiimote.Button.a && this.fireDelay <= 0)
                    {
                        this.fireDelay = 7;
                        this.Fire();
                    }
                }
            }
        }

        /// <summary>
        /// once a frame decide if our timers are ready
        /// </summary>
        private void LateUpdate()
        {
            // If we're not scanning at the moment,
            if (!this.invokingScan)
            {
                // and our reference to the wiimote is invalid or not ready
                if (this.wiimote == null || !this.wiimoteReady)
                {
                    // Scan for a wiimote
                    this.invokingScan = true;
                    InvokeRepeating("ScanForWiimotes", 1f, 1f);
                }
            }
        }
        #endregion
        
        private IEnumerator Die()
        {
            for (int i = 0; i < 15; i++)
            {
                this.transform.position += Vector3.up * .5f;
                yield return new WaitForSeconds(.1f);
            }
            this.gameObject.SetActive(false);
            yield return null;
        }

        /// <summary>
        /// decreases timers over time
        /// </summary>
        private void DecrementTimers()
        {
            if (this.colorDelay != 0)
            {
                this.colorDelay--;
            }
            if (this.readyDelay != 0)
            {
                this.readyDelay--;
            }
            if (this.fireDelay != 0)
            {
                this.fireDelay--;
            }
        }

        /// <summary>
        /// fires our guns
        /// </summary>
        private void Fire()
        {
            Transform barrel = this.barrelMount.Find("Barrel " + this.nextBarrel);
            Transform tip = barrel.Find("Tip");

            var prefab = Resources.Load("Bullet", typeof(GameObject));
            GameObject go = (GameObject)Instantiate(prefab, tip.position, tip.rotation);
            go.transform.parent = this.OBJECTHOLDER;
            go.GetComponent<MeshRenderer>().material.color = this.PrimaryColor;
            go.GetComponent<Rigidbody>().velocity = barrel.TransformDirection(Vector3.up * 5f);

            // Reset barrel
            this.nextBarrel++;
            if (this.nextBarrel > 2)
            {
                this.nextBarrel = 1;
                this.fireDelay += 7;
            }
        }

        /// <summary>
        /// randomizes our primary and secondary color
        /// </summary>
        private void ChangeColor()
        {
            this.PrimaryColor = new Color(Random.value, Random.value, Random.value);
            this.SecondaryColor = new Color(Random.value, Random.value, Random.value);

            this.GetComponent<MeshRenderer>().material.color = this.PrimaryColor;
            this.mound.GetComponent<MeshRenderer>().material.color = this.SecondaryColor;

            MeshRenderer[] barrels = this.barrelMount.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < barrels.Length; i++)
            {
                barrels[i].material.color = this.SecondaryColor;
            }

            this.colorDelay = 3;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConsumeWiimoteEvents()
        {
            // Don't check if we're not ready yet
            if (this.wiimote == null || !this.wiimoteReady)
            {
                return;
            }

            // Checks how many new events we have to process
            var ret = this.wiimote.ReadWiimoteData();

            // Process every wiimote event that's available
            do
            {
                var accel = this.wiimote.Accel.GetCalibratedAccelData();
                float accel_x = accel[1];
                float accel_z = accel[0];

                // Bring our averages for calibration
                this.averageX = Mathf.Lerp(this.averageX, accel_x, this.ConsumeEventEvery / 5f);
                this.averageZ = Mathf.Lerp(this.averageZ, accel_z, this.ConsumeEventEvery / 5f);

                // Pull values from wiimote
                this.lookRotation.x = this.averageX * 40f - 40f;
                this.lookRotation.y = this.averageZ * 70f - 35f;

                // Clamp the camera
                this.lookRotation.x = Mathf.Clamp(this.lookRotation.x, -50, 5);
                this.lookRotation.y = Mathf.Clamp(this.lookRotation.y, -70, 70);
                this.lookRotation.z = 0;

                // We processed one event
                ret--;
            } while (ret > 0);
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void ScanForWiimotes()
        {
            WiimoteManager.FindWiimotes();
            if (WiimoteManager.HasWiimote())
            {
                // If we don't have a WiiMote this high, keep trying
                if (WiimoteManager.Wiimotes.Count < this.PlayerID)
                {
                    return;
                }
                
                this.wiimoteReady = true;
                this.wiimote = WiimoteManager.Wiimotes[this.PlayerID - 1];
                this.wiimote.SendPlayerLED(this.PlayerID == 1, this.PlayerID == 2, this.PlayerID == 3, this.PlayerID == 4);
                this.wiimote.SendDataReportMode(
                    InputDataType.REPORT_BUTTONS_ACCEL_EXT16
                );

                this.invokingScan = false;
                CancelInvoke("ScanForWiimotes");
            }
        }
    }
}
