//
//  MCLSentShareOpenedEvent.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>

#import "MCLShare.h"

NS_ASSUME_NONNULL_BEGIN

/**
 A share sent from this device caused the app to be opened by someone else.

 Note that this event does not fire when clicking your own links.

 @see https://docs.megacool.co/learn/links for more details about when different properties are
 available on the event.
 */
@interface MCLSentShareOpenedEvent : NSObject

/**
 The share that was clicked on.

 This can be null if the url didn't contain enough information to identify the specific share, but
 we could identify the receiver.
 */
@property(nonatomic, nullable, readonly, strong) MCLShare *share;


/**
 The userId for the person that opened the share.
 */
@property(nonatomic, readonly, strong) NSString *receiverUserId;


/**
 Whether opening the share resulted in an install or a re-engagement for the receiver.

 Use this to reward the sender for the referral.
 */
@property(nonatomic, readonly) BOOL isFirstSession;


/**
 When the event occured.
 */
@property(nonatomic, readonly, strong) NSDate *createdAt;

@end

NS_ASSUME_NONNULL_END
