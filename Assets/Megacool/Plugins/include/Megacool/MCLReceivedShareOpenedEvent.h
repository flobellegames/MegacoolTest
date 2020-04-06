//
//  MCLReceivedShareOpenedEvent.h
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
 This device clicked on a share sent by someone else.

 Note that this event does not fire when clicking your own links.

 @see https://docs.megacool.co/learn/links for more info on when different properties are available
 on the event.
 */
@interface MCLReceivedShareOpenedEvent : NSObject

/**
 Whether this is an install event or a re-engagement.
 */
@property(nonatomic, readonly) BOOL isFirstSession;


/**
 The userId for the user that sent the share.
 */
@property(nonatomic, readonly, strong) NSString *senderUserId;


/**
 The share object that lead to the event.

 This can be nil if the url didn't contain enough information to identify the specific share, but
 we could identify the sender.
 */
@property(nonatomic, nullable, readonly, strong) MCLShare *share;


/**
 When the event occured.
 */
@property(nonatomic, readonly, strong) NSDate *createdAt;

@end

NS_ASSUME_NONNULL_END
