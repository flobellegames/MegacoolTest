using UnityEngine;

/// <summary>
/// Attach this script to a camera you want to record from.
/// </summary>
class MegacoolRenderingCamera : MonoBehaviour {
    private Camera megacoolCamera;
    private GameObject cameraGameObject;
    private Camera cameraCopy;
    private double timeToNextCapture;
    private const int MCTR = 0x6d637472;

    public double TimeBetweenCaptures { private get; set; }


    void Start() {
        megacoolCamera = GetComponent<Camera>();
        // Since we cannot re-render the camera while it's in use (which it usually is during OnPreRender,
        // OnRenderImage and OnPostRender, leaving only OnGUI which usually has some performance hit, we add an extra
        // camera that mirrors the camera we're attached to.
        cameraGameObject = new GameObject();
        cameraCopy = cameraGameObject.AddComponent<Camera>();
        cameraCopy.enabled = false;
    }

    void OnPreRender() {
        timeToNextCapture -= Time.unscaledDeltaTime;

        if (!(Megacool.Instance._IsRecording && timeToNextCapture <= 0) && !Megacool.Instance._RenderThisFrame) {
            return;
        }

        if (!Megacool.Instance._TextureReady.WaitOne(0)) {
            return;
        }

        Megacool.Instance._RenderThisFrame = false;

        cameraCopy.CopyFrom(megacoolCamera);
        cameraCopy.targetTexture = Megacool.Instance._RenderTexture;
        cameraCopy.Render();

        Megacool.Instance._IssuePluginEvent(MCTR);

        timeToNextCapture = TimeBetweenCaptures;
    }
}
