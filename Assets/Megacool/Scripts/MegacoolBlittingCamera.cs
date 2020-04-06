using UnityEngine;

/// <summary>
/// Attach this script to a camera you want to duplicate the frames from. This does not re-render the frame, but might
/// cause performance degradations on certain GPUs that doesn't implement efficient blitting.
/// </summary>
class MegacoolBlittingCamera : MonoBehaviour {
    private double timeToNextCapture;
    private const int MCTR = 0x6d637472;


    public double TimeBetweenCaptures { private get; set; }


    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        timeToNextCapture -= Time.unscaledDeltaTime;

        if (!(Megacool.Instance._IsRecording && timeToNextCapture <= 0) && !Megacool.Instance._RenderThisFrame) {
            Graphics.Blit(src, dest);
            return;
        }

        if (!Megacool.Instance._TextureReady.WaitOne(0)) {
            Graphics.Blit(src, dest);
            return;
        }

        Megacool.Instance._RenderThisFrame = false;

        Megacool.Instance._RenderTexture.MarkRestoreExpected();
        Graphics.Blit(src, Megacool.Instance._RenderTexture);
        Megacool.Instance._IssuePluginEvent(MCTR);
        timeToNextCapture = TimeBetweenCaptures;

        // This has to happen after the other blit, otherwise you might end up with UI flickering
        Graphics.Blit(src, dest);
    }
}
