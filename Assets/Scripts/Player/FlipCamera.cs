using UnityEngine;

public class FlipCamera : MonoBehaviour
{
    [HideInInspector]
    public new Transform camera;
    
    public bool UpdateCamera = true;

    public bool ChangeCamera = false;

    private Transform camFPS;
    private Transform camTPS;

    private bool cameraType = false;

    #region Unity
    /// <summary>
    /// obtains references to cameras
    /// </summary>
    private void Awake()
    {
        this.camFPS = GameObject.Find("CamPos_FPS").transform;
        this.camTPS = GameObject.Find("CamPos_TPS").transform;

        this.camera = this.camFPS;
    }

    /// <summary>
    /// takes input to switch cameras
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) || this.ChangeCamera)
        {
            this.cameraType = !this.cameraType;
            if (this.cameraType)
            {
                this.camera = this.camTPS;
            }
            else
            {
                this.camera = this.camFPS;
            }

            this.ChangeCamera = false;
        }
    }

    /// <summary>
    /// moves selected camera to us
    /// </summary>
    private void LateUpdate()
    {
        Camera.main.transform.position = this.camera.position;

        if (this.UpdateCamera)
        {
            Camera.main.transform.rotation = this.camera.rotation;
        }
    }
    #endregion
}
