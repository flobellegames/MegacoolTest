//
//  MCLDelegate.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#ifndef MCLDelegate_h
#define MCLDelegate_h

#import "MCLLinkClickedEvent.h"
#import "MCLReceivedShareOpenedEvent.h"
#import "MCLSentShareOpenedEvent.h"

NS_ASSUME_NONNULL_BEGIN

/**
 Implement this protocol to get callbacks for events happening in the SDK. All methods are optional.
 */
@protocol MCLDelegate<NSObject>

@optional
/**
 Optional callback when a user has completed a share.

 Integration:

 1) Add MCLDelegate to your interface: `\@interface ViewController ()<MCLDelegate>`

 2) Pass the delegate to the SDK during startup:
 @code
 -[Megacool startWithAppConfig:@"<appConfig>" andConfigBlock:^(MCLMegacoolConfig *config) {
    config.delegate = self;
 }];
 @endcode

 3) Implement the method somewhere in your code:

 @code
 - (void)megacoolDidCompleteShare {
    NSLog(@"Reward Player");
 }
 @endcode

 @note This method being called does not imply that anyone has actually joined the
 game from the share, only that the share was probably sent to some app. Not all apps report this
 correctly.
 */
- (void)megacoolDidCompleteShare;


@optional
/**
 Optional callback when a user has aborted (dismissed) a share.

 Integration:

 1) Add MCLDelegate to your interface: `\@interface ViewController ()<MCLDelegate>`

 2) Pass the delegate to the SDK during startup:
 @code
 -[Megacool startWithAppConfig:@"<appConfig>" andConfigBlock:^(MCLMegacoolConfig *config) {
    config.delegate = self;
 }];
 @endcode

 3) Add the method somewhere in your code:

 ```
 - (void)megacoolDidDismissShareView{
    NSLog(@"Dismissed share modal");
 }
 ```
 */
- (void)megacoolDidDismissShareView;


@optional
/**
 A link was clicked by the user.

 This event is purely local to the device and thus both available offline and very fast, and thus
 suitable for navigation within the app (with sanity checking of destinations).
 */
- (void)megacoolLinkClicked:(MCLLinkClickedEvent *)event;


@optional
/**
 This device clicked on a share sent by someone else.

 This event comes from the server when it detects that the user has clicked on a share. This
 happens during installs or re-engagements (check `MCLReceivedShareOpenedEvent.isFirstSession` to
 determine which type of session you're in) and can be used to automatically connect the user to the
 person who sent the share in the game (using `MCLReceivedShareOpenedEvent.senderUserId`).
 */
- (void)megacoolReceivedShareOpened:(MCLReceivedShareOpenedEvent *)event;


@optional
/**
 A share sent by this user was clicked on by someone else.

 Use this event to reward the user for referring a friend. Use
 `MCLSentShareOpenedEvent.isFirstSession` to tell if the referral resulted in a new install
 or a re-engagement, or use `MCLSentShareOpenedEvent.receiverUserId` to automatically connect the
 users in the app.
 */
- (void)megacoolSentShareOpened:(MCLSentShareOpenedEvent *)event;

@end

NS_ASSUME_NONNULL_END

#endif /* MCLDelegate_h */
