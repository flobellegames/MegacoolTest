//
//  MCLLinkClickedEvent.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>

#import "MCLReferralCode.h"

NS_ASSUME_NONNULL_BEGIN

extern NSString *const MCLEventDataURL;
extern NSString *const MCLEventDataReferralCode;

/**
 A link was clicked by the user.

 This event is purely local to the device and thus both available offline and very fast, and thus
 suitable for navigation within the app (with sanity checking of destinations).

 @see For more details on which properties are available on events when clicking links, see
 https://docs.megacool.co/learn/links
 */
@interface MCLLinkClickedEvent : NSObject

/**
 The link that was clicked.

 This will be a relative url, and mirror what you set as `MCLShareConfig.url` when creating the
 share.
 */
@property(nonatomic, readonly, strong) NSURL *url;


/**
 The referral code for the share.

 From this you can get the userId of the user that sent the share, from `MCLReferralCode.userId`.
 Note that the referral code can be null if it was removed partially or entirely from the url by the user.
 */
@property(nonatomic, nullable, readonly, strong) MCLReferralCode *referralCode;

@end

NS_ASSUME_NONNULL_END
