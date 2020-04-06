//
//  MCLShare.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>

#import "MegacoolFFI.h"
#import "MCLReferralCode.h"

NS_ASSUME_NONNULL_BEGIN

/**
 Different states a share can be in. All shares start as `sent`, becomes `clicked` when it has had at
 least one link click, and then becomes either `opened` or `installed`.

 Note that the state only moves in one direction, towards the best outcome that has come from it.
 Thus a share that receives several link clicks where some lead to re-engagements (opened) and
 some to an install, once the first install happens the state will never change from `installed`.

 To keep more granualar track of how many installs or re-engagements a share has generated you
 need to listen to the {@link SentShareOpenedEvent} and keep count yourself.
*/
typedef NS_ENUM(NSInteger, MCLShareState) {
    /**
     The share has been sent. This is the default state of a share until something happens with
     the link.
     */
    MCLShareStateSent = MCL_SHARE_STATE_SENT,

    /**
     The share link has been clicked. If this state remains, assume that the receiver didn't
     complete an install or didn't have the app installed already.

     Note that depending on where the share was sent this might not mean the recipient clicked on
     it consciously, it might have been "clicked" automatically by the app to generate a link
     preview or similar. It's likely the preview was seen by the recipient, but the recipient
     hasn't necessarily expressed intent to click the share.
     */
    MCLShareStateClicked = MCL_SHARE_STATE_CLICKED,

    /**
     The share led to at least one existing user opening the app (re-engagement).

     The share can have led to several re-engagements, but no new installs.
     */
    MCLShareStateOpened = MCL_SHARE_STATE_OPENED,

    /**
     The share led to at least one new user installing the app.

     The share can have generated multiple installs and re-engagements in this state.
     */
    MCLShareStateInstalled = MCL_SHARE_STATE_INSTALLED,
};


/**
 MCLShare holds information about a share, like which state it has (MCLShareStateSent,
 MCLShareStateClicked, MCLShareStateOpened or MCLShareStateInstalled), the URL and custom data.
 */
@interface MCLShare : NSObject

/**
 Unique referral code for each share which is part of the link.

 The referral code is assigned when the share is shared, thus not available when you've
 instantiated the share yourself.
 */
@property(readonly, strong) MCLReferralCode *referralCode;


/**
 How far the share has come towards generating an install.
 */
@property(readonly) MCLShareState state;


/**
 Timestamp of when the share was created
 */
@property(readonly, strong) NSDate *createdAt;


/**
 Timestamp of the last update to the share
 */
@property(readonly, strong) NSDate *updatedAt;


/**
 Data associated with the share object
 */
@property(readonly, strong) NSDictionary<NSString *, NSObject *> *data;


/**
 The URL for the share.

 For navigation purposes inside the app you should probably use
 `MCLLinkClickedEvent.url` instead, as that's computed locally and thus faster and works offline.

 Note that a url set with `MCLShareConfig.url` when the share was created might be `"/"` (the
 default) on a share received from `MCLReceivedShareOpenedEvent` or `SentShareOpenedEvent` if the
 network request to create the share failed.
 */
@property(nonatomic, readonly, strong) NSURL *URL;

@end

NS_ASSUME_NONNULL_END
