using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using MegacoolThirdParty_MiniJSON;

/// <summary>
/// Pass to Megacool.Share(MegacoolShareConfig) to configure how shares are made.
/// </summary>
public class MegacoolShareConfig {

    private string recordingId;
    /// <summary>
    /// Identifier of the recording to share.
    /// </summary>
    public string RecordingId {
        get {
            return recordingId ?? MegacoolConfiguration.DEFAULT_RECORDING_ID;
        }
        set {
            recordingId = value;
        }
    }


    private string message;
    /// <summary>
    /// Set the predefined text in the share.
    /// </summary>
    /// <remarks>
    /// Not all apps supports setting a predefined text in the share.
    /// </remarks>
    public string Message {
        get {
            return message ?? MegacoolConfiguration.DEFAULT_SHARE_MESSAGE;
        }
        set {
            message = value;
        }
    }


    private MegacoolSharingStrategy? strategy;
    /// <summary>
    /// Set whether to prioritize media or links when sharing to channels that support either but
    /// not both.
    /// </summary>
    public MegacoolSharingStrategy Strategy {
        get {
            return strategy ?? MegacoolConfiguration.DEFAULT_SHARING_STRATEGY;
        }
        set {
            strategy = value;
        }
    }


    /// <summary>
    /// Path to a fallback file to share in case the given recording doesn't exist, relative to the
    /// StreamingAssets directory.
    /// </summary>
    /// <remarks>On iOS the filename given here will be preserved when shared. Thus when sharing to
    /// f. ex email the attached filename will be same and thus user visible.</remarks>
    public string FallbackImage { get; set; }


    /// <summary>
    /// Set extra share data that will be present on the received MegacoolShare.
    /// </summary>
    /// <remarks>
    /// This has to be JSON serializable.
    /// </remarks>
    public Dictionary<string, object> Data { get; set; }


    /// <summary>
    /// Customize the link shared.
    /// </summary>
    /// <description>
    /// This should be a relative URL of the form <c>"/some/path?key=value"</c>, since only the
    /// path, query and fragment will be included. This will be combined with the base url set in
    /// the config panel and the referral code to build the absolute url for the share:
    /// <c>%https://<base-url><url>?_m=<referal-code></c>
    /// </description>
    public Uri Url { get; set; }


    private string modalTitle;
    /// <summary>
    /// Customize the title of the modal that presents the apps the user can share to.
    /// </summary>
    /// <description>
    /// The default is <code>"Share GIF"</code>. On iOS the share modal doesn't have a title.
    /// </description>
    /// <value>The title of the share modal.</value>
    public string ModalTitle {
        get {
            return modalTitle ?? MegacoolConfiguration.DEFAULT_SHARE_MODAL_TITLE;
        }
        set {
            modalTitle = value;
        }
    }


    private Rect? modalLocation;
    /// <summary>
    /// iPads only: Customize the location of the share modal. If unset the modal will be in the
    /// upper left corner.
    /// </summary>
    /// <description>
    /// The coordinates follow the Unity GUI convention, where (0, 0) is the upper left corner and
    /// X increasing to the right and Y increasing downwards.
    /// </description>
    public Rect ModalLocation {
        get {
            return modalLocation ?? new Rect();
        }
        set {
            modalLocation = value;
        }
    }


    /// <summary>
    /// Possible directions for the arrow on the iPad share modal.
    /// </summary>
    [Flags]
    public enum ModalArrowDirection : int {
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        Any = Up | Down | Left | Right,
    }


    private ModalArrowDirection? modalPermittedArrowDirections;
    /// <summary>
    /// iPads only: Set where the arrow on the share modal can be, and by extension where the share
    /// modal will be positioned relative to the location set in <c>ModalLocation</c>. XOR the
    /// permitted directions together. Defaults to <c>ModalArrowDirection.Any</c>.
    /// </summary>
    public ModalArrowDirection ModalPermittedArrowDirections {
        get {
            return modalPermittedArrowDirections ?? ModalArrowDirection.Any;
        }
        set {
            modalPermittedArrowDirections = value;
        }
    }


    /// <summary>
    /// Create a new configuration object to customize sharing.
    /// </summary>
    public MegacoolShareConfig() {
    }


    public void _LoadDefaults(MegacoolConfiguration config) {
        if (!_HasMessage() && !string.IsNullOrEmpty(config.sharingText)) {
            Message = config.sharingText;
        }
        if (FallbackImage == null && !string.IsNullOrEmpty(config.sharingFallback)) {
            FallbackImage = config.sharingFallback;
        }
        if (!_HasModalTitle() && !string.IsNullOrEmpty(config.shareModalTitle)) {
            ModalTitle = config.shareModalTitle;
        }
        if (!_HasStrategy()) {
            Strategy = config.sharingStrategy;
        }
    }


    public bool _HasRecordingId() {
        return recordingId != null;
    }


    public bool _HasMessage() {
        return message != null;
    }


    public bool _HasStrategy() {
        return strategy != null;
    }


    public bool _HasModalTitle() {
        return modalTitle != null;
    }


    public bool _HasModalLocation() {
        return modalLocation != null;
    }


    public bool _HasModalPermittedArrowDirections() {
        return modalPermittedArrowDirections != null;
    }


    public override string ToString() {
        var arrowLocations = "Any";
        if (ModalPermittedArrowDirections != ModalArrowDirection.Any) {
            var permittedArrowLocations = new List<string>();
            foreach (ModalArrowDirection direction in Enum.GetValues(typeof(ModalArrowDirection))) {
                if (direction == ModalArrowDirection.Any) {
                    continue;
                }
                if ((direction & ModalPermittedArrowDirections) != 0) {
                    permittedArrowLocations.Add(Enum.GetName(typeof(ModalArrowDirection), direction));
                }
            }
            arrowLocations = string.Join("|", permittedArrowLocations.ToArray());
        }
        return string.Format("MegacoolShareConfig(Message={0}, Url={1}, RecordingId={2}, " +
            "Strategy={3}, Fallback={4}, Data={5}, ModalTitle={6}, ModalLocation={7}, " +
            "ModalPermittedArrowDirections={8})",
            _HasMessage() ? string.Format("\"{0}\"", message) : string.Format("default(\"{0}\")", Message),
            Url,
            _HasRecordingId() ? string.Format("\"{0}\"", recordingId) : string.Format("default(\"{0}\")", RecordingId),
            _HasStrategy() ? strategy.ToString() : string.Format("default({0})", Strategy),
            FallbackImage,
            Data == null ? null : Json.Serialize(Data),
            _HasModalTitle() ? string.Format("\"{0}\"", modalTitle) : string.Format("default(\"{0}\")", ModalTitle),
            _HasModalLocation() ?
                string.Format("Rect({0}, {1}, {2}, {3})", ModalLocation.x, ModalLocation.y, ModalLocation.width, ModalLocation.height)
                : "default(Rect(0, 0, 0, 0))",
            _HasModalPermittedArrowDirections() ? arrowLocations : "default(Any)"
        );
    }
}
