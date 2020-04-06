using System;

/// <summary>
/// A link was clicked by the user.
/// </summary>
/// <remarks>
/// <para>
/// This event is purely local to the device and thus both available offline and very fast, and thus
/// suitable for navigation within the app (with sanity checking of destinations).
/// </para>
/// <para>
/// For more details on which properties are available on events when clicking links, see
/// <a href="https://docs.megacool.co/learn/links">docs.megacool.co/learn/links</a>.
/// </para>
/// </remarks>
/// <seealso cref="MegacoolSentShareOpenedEvent"/>
/// <seealso cref="MegacoolReceivedShareOpenedEvent"/>
public class MegacoolLinkClickedEvent {

    /// <summary>
    /// Get the url that was clicked.
    /// </summary>
    /// <description>
    /// This is always a relative url like "/some/path" or "/" and
    /// mirrors what was set in <code>MegacoolShareConfig.Url</code> when the share was created.
    /// </description>
    /// <value>The clicked url.</value>
    public Uri Url { get; private set; }


    /// <summary>
    /// Get the referral code from the link clicked, or <c>null</c> if there was none.
    /// </summary>
    /// <value>The referral code if there was one, or <c>null</c>.</value>
    public MegacoolReferralCode ReferralCode { get; private set; }


    public MegacoolLinkClickedEvent(Uri url, MegacoolReferralCode referralCode) {
        Url = url;
        ReferralCode = referralCode;
    }


    public override string ToString() {
        return string.Format("MegacoolLinkClickedEvent(Url=\"{0}\", ReferralCode={1})",
            Url, ReferralCode == null ? "null" : ReferralCode.ToString(true));
    }

}
