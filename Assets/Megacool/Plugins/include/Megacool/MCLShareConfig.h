//
//  MCLShareConfig.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//


#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "MCLShare.h"
#import "MCLConstants.h"

NS_ASSUME_NONNULL_BEGIN

/**
 Pass this to `-[Megacool presentShareWithConfig:]` to configure how a share is performed.

 @note This class does not have atomic properties for efficiency, thus it's not thread-safe.
 */
@interface MCLShareConfig : NSObject <NSCopying>

/**
 Which recording to share. Will use default if nil.
 */
@property(nonatomic, null_resettable, strong) NSString *recordingId;


/**
 Set the text to be shared on different channels. The user will
 usually have the ability to customize the message before sending.
 */
@property(nonatomic, strong, null_resettable) NSString *message;


/**
 A fallback to share if there are no frames in the given recording
 or something fails. Mutually exclusive with `fallbackImage.`

 Note that the filename of the file given here will be preserved when shared, thus f. ex when sharing to email this will be the name of the
 attached file.
 */
@property(nonatomic, nullable, strong) NSURL *fallbackImageUrl;

/**
 A fallback to to share if there are no frames in the given recording
 or something fails. Mutually exclusive with `fallbackImageUrl.`
 */
@property(nonatomic, nullable, strong) UIImage *fallbackImage;

/**
 Customize the url included in the share to navigate to a specific section of your app.

 The url given here can be extracted from the `MCLLinkClickedEvent` on the receiving side for
 navigation purposes.

 Note that only the path and query parameters will be respected. This will be combined with the
 `MCLMegacoolConfig.baseUrl` and the referral code to form the absolute url that will be shared. If
 set to f. ex `[NSURL URLWithString:@"/level2?difficulty=ludicruous"]`, the absolute url will
 end up as `https://<base-url>/level2?difficulty=ludicruous&_m=<referral-code>`.
 */
@property(nonatomic, nullable, strong) NSURL *url;

/**
 Additional data to include with the share. This will be available to the receiver in the receivedShareOpened event.
 */
@property(nonatomic, nullable, strong) NSDictionary<NSString *, NSObject *> *data;


/**
 On iPads, which view should present the popover.
 */
@property(nonatomic, nullable, strong) UIView *popoverSourceView;


/**
 On iPads, where in the `popoverSourceView` the popover should be presented.
 */
@property(nonatomic) CGRect popoverSourceRect;


/**
 On which sides of the popover the source arrow should be placed. This can be used to force the
 popover to be over or under the `popoverSourceRect`.

 XOR the permitted locations together. Defaults to `UIPopoverArrowDirectionAny`.
 */
@property(nonatomic) UIPopoverArrowDirection popoverPermittedArrowDirections;


/**
 Set sharing strategy.

 What type of data should be prioritized when sharing to channels
 that support @em either media @em or links.
 */
@property(nonatomic) MCLSharingStrategy strategy;


NS_ASSUME_NONNULL_END

@end
