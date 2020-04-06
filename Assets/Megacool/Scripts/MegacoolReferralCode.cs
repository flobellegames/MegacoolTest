/// <summary>
/// Identifies a specific share.
/// </summary>
public class MegacoolReferralCode {

    /// <summary>
    /// Identifies the user that created the share.
    /// </summary>
    /// <value>The invite identifier.</value>
    public string UserId { get; private set; }

    /// <summary>
    /// Identifies the specific share from the given user, or an empty string if only the
    /// <c>UserId</c>was present in the link.
    /// </summary>
    /// <value>The share identifier.</value>
    public string ShareId { get; private set; }

    public MegacoolReferralCode(string userId, string shareId) {
        UserId = userId;
        ShareId = shareId;
    }

    public override string ToString() {
        return string.Format("{0}{1}", UserId, ShareId);
    }

    public string ToString(bool verbose) {
        if (!verbose) {
            return ToString();
        }
        return string.Format("MegacoolReferralCode(UserId=\"{0}\", ShareId=\"{1}\")",
            UserId, ShareId);
    }
}
