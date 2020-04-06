using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// Handles how capture is performed. Can be manually attached to a Camera to capture from that instead of the main
/// camera.
/// </summary>
public class MegacoolManager : MonoBehaviour {
    private const int MCRC = 0x6d637263;
    private const int MCWF = 0x6d637766;
    private Coroutine writeCoroutine = null;
    private Nullable<MegacoolCaptureMethod> previousCaptureMethod = null;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    private WaitForEndOfFrame endOfFrame;
#endif

    public void Awake() {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        endOfFrame = new WaitForEndOfFrame();
#endif
    }

    public void StartWrites(double timeBetweenCaptures) {
        // Make sure old cameras are cleaned if the capture method changes
        if (previousCaptureMethod != null && previousCaptureMethod != Megacool.Instance.CaptureMethod) {
            RemoveCameras();
        }
        if (Megacool.Instance.CaptureMethod == MegacoolCaptureMethod.BLIT) {
            InitializeBlittingCamera(timeBetweenCaptures);
        } else if (Megacool.Instance.CaptureMethod == MegacoolCaptureMethod.RENDER){
            InitializeRenderingCamera(timeBetweenCaptures);
        } else if (Megacool.Instance.CaptureMethod == MegacoolCaptureMethod.SCREEN) {
            RemoveCameras();
        }
        previousCaptureMethod = Megacool.Instance.CaptureMethod;

        StopWrites();
        if (Megacool.Instance.CaptureMethod == MegacoolCaptureMethod.SCREEN) {
            writeCoroutine = StartCoroutine(StartWriteCoroutine(timeBetweenCaptures));
        }
    }

    public void StopWrites() {
        if (writeCoroutine != null) {
            StopCoroutine(writeCoroutine);
            writeCoroutine = null;
        }
    }

    private void InitializeRenderingCamera(double timeBetweenCaptures) {
        if (!gameObject.GetComponent<MegacoolRenderingCamera>()) {
            MegacoolRenderingCamera cam = gameObject.AddComponent<MegacoolRenderingCamera>();
            cam.TimeBetweenCaptures = timeBetweenCaptures;
        }
    }

    private void InitializeBlittingCamera (double timeBetweenCaptures) {
        if (!gameObject.GetComponent<MegacoolBlittingCamera>()) {
            MegacoolBlittingCamera cam = gameObject.AddComponent<MegacoolBlittingCamera>();
            cam.TimeBetweenCaptures = timeBetweenCaptures;
        }
    }

    private void RemoveCameras() {
        MegacoolBlittingCamera blitCamera = gameObject.GetComponent<MegacoolBlittingCamera>() ;
        if (blitCamera) {
            Destroy(blitCamera);
        }
        MegacoolRenderingCamera renderCamera = gameObject.GetComponent<MegacoolRenderingCamera>();
        if (renderCamera) {
            Destroy(renderCamera);
        }
    }


    public IEnumerator StartWriteCoroutine(double timeBetweenCaptures) {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        if (timeBetweenCaptures == 0) {
            yield return endOfFrame;
            Megacool.Instance._IssuePluginEvent(MCWF);
            yield break;
        }
        while (true) {
            yield return endOfFrame;
            Megacool.Instance._IssuePluginEvent(MCRC);
        }
#else
        yield break;
#endif
    }

}
