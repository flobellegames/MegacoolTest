using System;

/// <summary>
/// This device clicked on a share sent by someone else.
/// </summary>
/// <remarks>
/// <para>
/// Note that this event does not fire when clicking your own links.
/// </para>
/// <para>
/// See <a href="https://docs.megacool.co/learn/links">docs.megacool.co/learn/links</a> for more
/// info on when different properties are available on the event.
/// </para>
/// </remarks>
/// <seealso cref="MegacoolSentShareOpenedEvent"/>
/// <seealso cref="MegacoolLinkClickedEvent"/>
public class MegacoolReceivedShareOpenedEvent {

    /// <summary>
    /// Whether the event represents an install or a re-engagement.
    /// </summary>
    /// <value><c>true</c> if it was an install, otherwise <c>false</c>.</value>
    public bool IsFirstSession { get; private set; }


    /// <summary>
    /// Get the identifier for the sending user.
    /// </summary>
    /// <value>The sender's user identifier.</value>
    /// <seealso cref="MegacoolReferralCode.UserId"/>
    public string SenderUserId { get; private set; }


    /// <summary>
    /// Get the share that was received, or <c>null</c>.
    /// </summary>
    /// <description>
    /// The share can be <c>null</c> either if there was an error submitting the share to the
    /// backend, or if the referral code in the link was truncated either intentionally or
    /// accidentally by the user, but the sender was still identifiable.
    /// </description>
    /// <value>The share if valid for the link clicked, otherwise <c>null</c>.</value>
    public MegacoolShare Share { get; private set; }


    /// <summary>
    /// Get the time the event happened.
    /// </summary>
    /// <value>The time the event happened.</value>
    public DateTime CreatedAt { get; private set; }


    public MegacoolReceivedShareOpenedEvent(bool isFirstSession, string senderUserId,
            MegacoolShare share, DateTime createdAt) {
        IsFirstSession = isFirstSession;
        SenderUserId = senderUserId;
        Share = share;
        CreatedAt = createdAt;
    }


    public override string ToString() {
        return string.Format("MegacoolReceivedShareOpenedEvent(IsFirstSession={0}, " +
            "SenderUserId=\"{1}\", CreatedAt={2}, Share={3})",
            IsFirstSession ? "true" : "false", SenderUserId,
            CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"), Share == null ? null : Share.ToString());
    }
}
