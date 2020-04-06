//
//  MCLMegacoolConfig.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>

#import "MCLDelegate.h"

NS_ASSUME_NONNULL_BEGIN

/**
 Configure core aspects of the SDK.
 */
@interface MCLMegacoolConfig : NSObject

/**
 If wrapping Megacool from a different tool, like a game engine or your custom layer, set a name
 for this wrapper and a version. This helps us track down bugs by seeing behavioral differences
 between versions.

 @param wrapper the name of the wrapper, like `@"Unity"` or `@"ACMECorpEngine"`.
 @param version the version of the wrapper, like `@"1.0"` or `@"2.3-rc4"`.
 */
-(void)setWrapper:(NSString *)wrapper version:(NSString *)version;


/**
 The delegate that should receive callbacks for different events in the SDK.
 */
@property(nonatomic, weak, nullable) id<MCLDelegate> delegate;


/**
 The base url to use when building the absolute share urls.

 This defaults to `mgcl.co/<app-identifier>`. The absolute url for a share will be
 `https://<base-url><share-url>?_m=<referral-code>`.

 The url must have at least a `host` to be valid.

 This is a premium feature that also requires configuration on the dashboard to work properly.
 */
@property(nonatomic, nullable, strong) NSURL *baseUrl;

@end

NS_ASSUME_NONNULL_END
