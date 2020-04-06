using System;

/// <summary>
/// A share sent from this device caused the app to be opened by someone else.
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
/// <seealso cref="MegacoolReceivedShareOpenedEvent"/>
/// <seealso cref="MegacoolLinkClickedEvent"/>
public class MegacoolSentShareOpenedEvent {
    /// <summary>
    /// Whether the share led to an install or a re-engagement.
    /// </summary>
    /// <value><c>true</c> if the event was an install, otherwise <c>false</c>.</value>
    public bool IsFirstSession { get; private set; }


    /// <summary>
    /// Get the receiver's user identifier.
    /// </summary>
    /// <value>The receiver user identifier.</value>
    /// <seealso cref="MegacoolReferralCode.UserId"/>
    public string ReceiverUserId { get; private set; }


    /// <summary>
    /// Get the share that was sent, or <c>null</c>.
    /// </summary>
    /// <description>
    /// The share can be <c>null</c> either if there was an error submitting the share to the backend,
    /// or if the referral code in the link was truncated either intentionally or accidentally by
    /// the user, but the sender was still identifiable.
    /// </description>
    /// <value>The share if valid for the link clicked, otherwise <c>null</c>.</value>
    public MegacoolShare Share { get; private set; }


    /// <summary>
    /// Get the time the event happened.
    /// </summary>
    /// <value>The time the event happened.</value>
    public DateTime CreatedAt { get; private set; }


    public MegacoolSentShareOpenedEvent(bool isFirstSession, string receiverUserId,
            MegacoolShare share, DateTime createdAt) {
        IsFirstSession = isFirstSession;
        ReceiverUserId = receiverUserId;
        Share = share;
        CreatedAt = createdAt;
    }


    public override string ToString() {
        return string.Format("MegacoolSentShareOpenedEvent(IsFirstSession={0}, " +
            "ReceiverUserId=\"{1}\", CreatedAt={2}, Share={3})",
            IsFirstSession ? "true" : "false", ReceiverUserId,
            CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"), Share == null ? null : Share.ToString());
    }
}
