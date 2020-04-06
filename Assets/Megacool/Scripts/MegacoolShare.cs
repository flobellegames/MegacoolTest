using System;
using System.Collections.Generic;

using Json = MegacoolThirdParty_MiniJSON.Json;

/// <summary>
/// A share sent from the Megacool SDK. You don't create shares directly, you create a
/// <c>MegacoolShareConfig</c> and pass it to either <c>Megacool.Instance.DefaultShareConfig</c> or
/// <c>Megacool.Instance.Share()</c>, or you might receive shares sent from others by clicking on
/// them.
/// </summary>
/// <seealso cref="MegacoolShareConfig"/>
public class MegacoolShare {

    /// <summary>
    /// Different states a share can be in. All shares start as <c>SENT</c>, becomes <c>CLICKED</c>
    /// when it has had at least one link click, and then becomes either <c>OPENED</c> or
    /// <c>INSTALLED</c>.
    /// </summary>
    /// <description>
    /// <para>
    /// Note that the state only moves in one direction, towards the best outcome that has come from
    /// it. Thus a share that receives several link clicks where some lead to re-engagements
    /// (<c>OPENED</c>) and some to an install, once the first install happens the state will never
    /// change from <c>INSTALLED</c>.
    /// </para>
    /// <para>
    /// To keep more granualar track of how many installs or re-engagements a share has generated
    /// you need to listen to the <c>Megacool.Instance.OnSentShareOpened</c> delegate and keep count
    /// yourself.
    /// </para>
    /// </description>
    public enum ShareState {
        /// <summary>
        /// The share has been sent. This is the default state of a share until something happens
        /// with the link.
        /// </summary>
        SENT = 0,

        /// <summary>
        /// The share link has been clicked. This can lead to <c>OPENED</c> or <c>INSTALLED</c> if
        /// the clicker finishes the process. If this state remains, assume that the receiver didn't
        /// complete an install.
        /// </summary>
        /// <description>
        /// Note that depending on where the share was sent this might not mean the recipient
        /// clicked on it consciously, it might have been "clicked" automatically by the app to
        /// generate a link preview or similar. It's likely the preview was seen by the recipient,
        /// but the recipient hasn't necessarily expressed intent to open the app.
        /// </description>
        CLICKED,

        /// <summary>
        /// The share led to at least one existing user opening the app. The share can have led to
        /// several re-engagements, but no new installs.
        /// </summary>
        OPENED,

        /// <summary>
        /// The share led to at least one new user installing the app. The share can have generated
        /// multiple installs and re-engagements in this state.
        /// </summary>
        INSTALLED
    }

    /// <summary>
    /// Get the unique referral code for the share.
    /// </summary>
    /// <value>The share's referral code.</value>
    public MegacoolReferralCode ReferralCode { get; private set; }

    /// <summary>
    /// How far the share has come towards generating an install.
    /// </summary>
    /// <value>The state of the share.</value>
    public ShareState State { get; private set; }

    /// <summary>
    /// When the share was created.
    /// </summary>
    /// <value>The time the share was created.</value>
    public DateTime CreatedAt { get; private set; }


    /// <summary>
    /// When the share object was last updated (changed state). This is the same as <c>CreatedAt</c>
    /// if the share has never been updated.
    /// </summary>
    /// <value>The time the share was last updated.</value>
    public DateTime UpdatedAt { get; private set; }


    /// <summary>
    /// Data associated with the share object to customize the user experience or adding additional
    /// information for each share.
    /// </summary>
    /// <description>
    /// Note that properties set in the <c>MegacoolShareConfig.Data</c> when the share was created
    /// might not be available when the share is received from a
    /// <c>MegacoolReceivedShareOpenedEvent</c> or <c>MegacoolSentShareOpenedEvent</c> if the
    /// network request to create the share failed. Thus you should always assume a key might be
    /// missing from the data.
    /// </description>
    /// <value>The data.</value>
    public Dictionary<string, object> Data { get; private set; }


    /// <summary>
    /// Get the URL that is associated with the share object.
    /// </summary>
    /// <description>
    /// <para>
    /// Note that a url set with <c>MegacoolShareConfig.Url</c> when the share was created might be
    /// <c>"/"</c> (the default) on a share received from <c>MegacoolReceivedShareOpenedEvent</c> or
    /// <c>MegacoolSentShareOpenedEvent</c> if the network request to create the share failed.
    /// </para>
    /// <para>
    /// For navigation purposes inside the app you should probably use
    /// <c>MegacoolLinkClickedEvent.Url</c> instead, as that's computed locally and thus faster and
    /// works offline.
    /// </para>
    /// </description>
    /// <value>The URL of the share.</value>
    /// <seealso cref="MegacoolLinkClickedEvent"/>
    public Uri Url { get; private set; }


    public MegacoolShare(MegacoolReferralCode referralCode, ShareState state,
            DateTime createdAt, DateTime updatedAt, Dictionary<string, object> data, Uri url) {
        ReferralCode = referralCode;
        State = state;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Data = data;
        Url = url;
    }

    public override string ToString() {
        return string.Format("MegacoolShare(State={0}, ReferralCode={1}, Url=\"{2}\", Data={3}, " +
            "CreatedAt={4}, UpdatedAt={5})",
            State, ReferralCode.ToString(true), Url, Data == null ? null : Json.Serialize(Data),
            CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"), UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}
