//
//  MCLReferralCode.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/**
 The referral code is the magic part tacked on to URLs shared through the SDK that allows us
 to credit a user for inviting others. You do not need to create these yourself.

 A referral code consists of two parts:
 - `userId`: 8 characters that uniquely identifies the user/app combination.
 - `shareId`: 2+ characters that identifies a given share. This enables us to associate each share
   with a GIF.
 */
@interface MCLReferralCode : NSObject

/**
 The device/app specific part of the URL. Can be used to identify other users of the app.
 */
@property(nonatomic, readonly, strong) NSString *userId;


/**
 The share-specific part of the URL, used to track one specific share.

 Might be an empty string if the link the referral code came from was truncated to only include the
 `userId`.
 */
@property(nonatomic, readonly, strong) NSString *shareId;

@end

NS_ASSUME_NONNULL_END
