//
//  Megacool.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>
#import <ImageIO/ImageIO.h>
#import <UIKit/UIKit.h>
#import "Availability.h"

#import "MCLConstants.h"
#import "MCLDelegate.h"
#import "MCLLinkClickedEvent.h"
#import "MCLMegacoolConfig.h"
#import "MCLPreview.h"
#import "MCLPreviewConfig.h"
#import "MCLPreviewData.h"
#import "MCLReceivedShareOpenedEvent.h"
#import "MCLRecordingConfig.h"
#import "MCLReferralCode.h"
#import "MCLSentShareOpenedEvent.h"
#import "MCLShare.h"
#import "MCLShareConfig.h"

#ifndef MEGACOOL_SDK_VERSION
#define MEGACOOL_SDK_VERSION @"5.0.2"
#endif

// clang-format off
#if __IPHONE_12_0
// Xcode 10 changed the signature of application:continueUserActivity:restorationHandler,
// causing mismatched type warnings. Thus check for Xcode 10 (where iOS 12 was introduced)
// and provide a separate macro to avoid the warning.
#define MEGACOOL_UNIVERSAL_LINK_HANDLER                                               \
    /*iOS 9+ Universal Link handler */                                                \
    -(BOOL)application:(UIApplication *)application                                   \
        continueUserActivity:(NSUserActivity *)userActivity                           \
        restorationHandler:(void (^)(                                                 \
            NSArray<id<UIUserActivityRestoring>> * __nullable restorableObjects)      \
        )restorationHandler                                                           \
    {                                                                                 \
        return [Megacool continueUserActivity:userActivity];                          \
    }

#else

#define MEGACOOL_UNIVERSAL_LINK_HANDLER                                               \
    /*iOS 9+ Universal Link handler */                                                \
    -(BOOL)application:(UIApplication *)application                                   \
    continueUserActivity:(NSUserActivity *)userActivity                               \
    restorationHandler:(void (^)(NSArray *restorableObjects))restorationHandler       \
    {                                                                                 \
        return [Megacool continueUserActivity:userActivity];                          \
    }

#endif

#define MEGACOOL_DEFAULT_LINK_HANDLERS                                                \
    /*iOS 9 deep link handler*/                                                       \
    -(BOOL)application:(UIApplication *)application                                   \
        openURL:(NSURL *)url                                                          \
        options:(NSDictionary<NSString *, id> *)options                               \
    {                                                                                 \
        return [[Megacool sharedMegacool] openURL:url options:options];               \
    }                                                                                 \
                                                                                      \
    /*iOS 4-8 deep link handler */                                                    \
    -(BOOL)application:(UIApplication *)application                                   \
        openURL:(NSURL *)url                                                          \
        sourceApplication:(NSString *)sourceApplication                               \
        annotation:(id)annotation                                                     \
    {                                                                                 \
        return [[Megacool sharedMegacool]                                             \
            openURL:url                                                               \
            sourceApplication:sourceApplication                                       \
        ];                                                                            \
    }                                                                                 \
                                                                                      \
    MEGACOOL_UNIVERSAL_LINK_HANDLER                                                   \
                                                                                      \
    /* Background upload/download handler (gif upload and fallback image download) */ \
    -(void)application:(UIApplication *)application                                   \
        handleEventsForBackgroundURLSession:(NSString *)identifier                    \
        completionHandler:(void (^)(void))completionHandler                           \
    {                                                                                 \
        [Megacool                                                                     \
            handleEventsForBackgroundURLSession:identifier                            \
            completionHandler:completionHandler                                       \
        ];                                                                            \
    }
// clang-format on

NS_ASSUME_NONNULL_BEGIN

typedef void (^MCLMegacoolConfigBlock)(MCLMegacoolConfig *config);
typedef void (^MCLShareStateCallback)(NSArray<MCLShare *> *shares);
typedef BOOL (^MCLShareFilterCallback)(MCLShare *share);
typedef void (^MCLShareConfigBlock)(MCLShareConfig *config);
typedef void (^MCLPreviewConfigBlock)(MCLPreviewConfig *config);
typedef void (^MCLRecordingConfigBlock)(MCLRecordingConfig *config);
typedef void (^MCLReferralCodeCallback)(NSString *referralCode);


/**
 * This is the main interface to the functionality the Megacool SDK offers. Make sure you call
 * `-[Megacool start]` as early as possible in the app lifecycle for referrals to be detected
 * early and reliably.
 */
@interface Megacool : NSObject


/**
 Returns the singleton instance of Megacool. Must be used to call the Megacool methods.

 @return `nil` if `startWithAppConfig` hasn't been called yet, otherwise the singleton.
 */
+ (nullable Megacool *)sharedMegacool;


/**
 Initializes the SDK. Get your app config from your Megacool Dashboard.

 @param appConfig NSString with the format `"prefix.appSecret"`
 */
+ (void)startWithAppConfig:(NSString *)appConfig;


/**
 Initializes the SDK with a config block.

 @param appConfig app config from the dashboard at https://dashboard.megacool.co
 @param configBlock a block that further configures start-up details of the SDK, like adding a
 delegate to listen to referral events.
 */
+ (instancetype)startWithAppConfig:(NSString *)appConfig
                    andConfigBlock:(nullable MCLMegacoolConfigBlock)configBlock;


/**
 Handle background upload/download tasks

 This is necessary to properly handle uploading of gifs in the background, and to
 download new fallback images defined through the dashboard.

 @return Whether we handled the events for the given identifier or whether it should be passed on
 to another handler.
 */
+ (BOOL)handleEventsForBackgroundURLSession:(NSString *)identifier
                          completionHandler:(void (^)(void))completionHandler;


#pragma mark - Link handling

/**
 Extract the URL from a link click. Add to ` application:openURL:options: ` in
 AppDelegate

 Add this to `- application:openURL:options:` in your AppDelegate. If the URL was
 a Megacool URL, the event handler will be called with a `MCLEventLinkClicked` event.

 @param url The url that was clicked. Megacool URLs will be either
 https://<base-url><share-url>?_m=<referral-code> or
 yourscheme:///<share-url>?_m=<referral-code>.

 @param options A dictionary of URL handling options.
 */
- (BOOL)openURL:(NSURL *)url options:(NSDictionary<NSString *, id> *)options;


/**
 Extract the URL from a link click. Add to ` application:openURL:sourceApplication: `
 in AppDelegate if you support versions below iOS 9

 Add this to `- application:openURL:options:` in your AppDelegate. If the URL was
 a Megacool URL, the event handler will be called with a `MCLEventLinkClicked` event.

 @param url The url that was clicked. Megacool URLs will be either
 https://<base-url><share-url>?_m=<referral-code> or
 yourscheme:///<share-url>?_m=<referral-code>.

 @param sourceApplication The `bundleId` of the sending application.
 */
- (BOOL)openURL:(NSURL *)url sourceApplication:(nullable NSString *)sourceApplication;


/**
 Allow Megacool to handle a universal link opening the app, returning whether it was from a
 Megacool link or not. Add this to `-application:continueUserActivity:restorationHandler:`
 in your AppDelegate.

 @param userActivity that caused the app to open.
 */
+ (BOOL)continueUserActivity:(NSUserActivity *)userActivity;


#pragma mark - Recording and capture


/**
 Start recording a GIF from a view.

 This will keep a buffer of 50 frames (default). The frames are overwritten until
 `stopRecording` gets called.

 @param view UIView you want to record
 */
- (void)startRecording:(UIView *)view;


/**
 Start customized GIF recording

 This will keep a buffer of 50 frames (default). The frames are overwritten until
 `stopRecording` gets called.

 @param view UIView you want to record
 @param configBlock MCLRecordingConfigBlock to customize the recording.
 */
- (void)startRecording:(UIView *)view withConfig:(nullable MCLRecordingConfigBlock)configBlock;


/**
 Capture a single frame

 Capture a single frame of the UIView to the buffer. The buffer size is 50 frames
 (default) and the oldest frames will be deleted if the method gets called more than 50 times.
 The total number of frames can be customized by setting the `maxFrames` property on the
 `sharedMegacool` instance. If you want a different strategy for handling too many frames, look at
 `kMCLOverflowStrategyTimelapse`.

 @param view UIView you want to capture a frame from
 */
- (void)captureFrame:(UIView *)view;

/*!
 @brief Capture a customized frame for a GIF

 Capture a single frame of the UIView to the buffer. The buffer size is 50 frames
 (default) and the oldest frames will be deleted if the method gets called more than 50 times.
 The total number of frames can be customized by setting the `maxFrames` property on the
 `sharedMegacool` instance.

 If the overflow strategy is timelapse, we recommend setting `maxFrames` to a value where
 `floor`(maxFrames*4/3) is even, this maximizes the number of frames at any point in time present
 in the timalpse.

 @param view UIView you want to capture a frame from.
 @param configBlock MCLRecordingConfigBlock to customize the recording.
 */
- (void)captureFrame:(UIView *)view withConfig:(nullable MCLRecordingConfigBlock)configBlock;


/*!
 Capture a customized frame for a GIF and indicate whether the frame should be added.
 Relevant for recordings using `kMCLOverflowStrategyTimelapse`.

 Capture a single frame of the UIView to the buffer. The buffer size is 50 frames
 (default) and the oldest frames will be deleted if the method gets called more than 50 times.
 The total number of frames can be customized by setting the `maxFrames` property on the
 `sharedMegacool` instance.

 If the overflow strategy is timelapse, we recommend setting `maxFrames` to a value where
 `floor`(maxFrames*4/3) is even, this maximizes the number of frames at any point in time present
 in the timalpse.

 @param view UIView you want to capture a frame from.
 @param configBlock MCLRecordingConfigBlock to customize the recording.
 @param force Set this to YES if you want to guarantee that a frame is added to the timelapse
 */
- (void)captureFrame:(UIView *)view
          withConfig:(nullable MCLRecordingConfigBlock)configBlock
            forceAdd:(BOOL)force;


/*!
 Take a screenshot and share it immediately.

 This is a helper around `captureFrame` and `presentShare` when you only want to share a
 screenshot.

 Note that if this method is called while a recording is ongoing, the screenshot is likely to
 be missing from the share. To stay on the safe side, leave a couple hundred ms between
 stopping a recording and sharing a screenshot.

 @param view UIView you want to capture a frame from.
 */
- (void)shareScreenshot:(UIView *)view;


/*!
 Take a screenshot and share it immediately.

 This is a helper around `captureFrame` and `presentShare` when you only want to share a
 screenshot.

 Note that if this method is called while a recording is ongoing, the screenshot is likely to
 be missing from the share. To stay on the safe side, leave a couple hundred ms between
 stopping a recording and sharing a screenshot.

 @param view UIView you want to capture a frame from.
 @param recordingConfigBlock MCLRecordingConfigBlock to customize the screenshot recording.
 @param shareConfigBlock MCLShareConfigBlock to customize the share.
*/
- (void)shareScreenshot:(UIView *)view
    withRecordingConfig:(nullable MCLRecordingConfigBlock)recordingConfigBlock
        withShareConfig:(nullable MCLShareConfigBlock)shareConfigBlock;


/**
 Pauses the current recording for resumption later.

 The frames captured so far will be stored on disk and can be resumed later by calling
 `startRecording` / `captureFrame` with the same `"recordingId"`.
*/
- (void)pauseRecording;


/**
 Stop recording the UIView set in startRecording.

 This method should be called after `startRecording` or `captureFrame` to
 mark the recording as completed. A completed recording can not have any more frames added to it,
 calling `captureFrame` or `startRecording` with the same `recordingId` as a completed recording
 will overwrite the old one.
 */
- (void)stopRecording;


/**
 Delete a recording.

 Will remove any frames of the recording in memory and on disk. Both completed and
 incomplete recordings will take space on disk, thus particularly if you're using
 `keepCompletedRecordings=YES` you might want to provide an interface to your users for removing
 recordings they don't care about anymore to free up space for new recordings.

 @param recordingId the id of the recording to delete, or `nil` to delete the default recording.
 */
- (void)deleteRecording:(nullable NSString *)recordingId;


/**
 Log a significant event for highlightRecording

 For highlight recording strategy only. This will make note of the time that a
 significant event occurs so that highlight recording can select the most interesting
 part of the gameplay
 */
- (void)registerScoreChange;


/**
 Log a the change in point value for highlightRecording

 For highlight recording strategy only. This will make note of the time that a
 significant event occurs along with the point value associated with it so that highlight
 recording can select the most interesting part of the gameplay

 @param scoreDelta the change in point value
 */
- (void)registerScoreChange:(int)scoreDelta;


/**
 Get the number of frames for the default recording

 This method is used to know if frames exist in the default recording, before showing a
 preview or displaying a share button.

 @return The number of frames in the default recording, or -1 if it doesn't exist.
 */
- (int)getNumberOfFrames;


/**
 Get the number of frames in a specific recording.

 This method is used to know if frames exist in the current recording, before showing a
 preview or displaying a share button.

 @param recordingId to get the number of frames from a specific recording, or `nil` to get for the
 default recording.
 @return The number of frames in the given recording, or -1 if it doesn't exist.
 */
- (int)getNumberOfFrames:(nullable NSString *)recordingId;


/**
 Get the currently active capture method.
 */
- (MCLCaptureMethod)captureMethod;


/**
 Set how screen captures should be performed, using the default scale factor. Call this as soon
 as possible after start. Calling this more than once leads to undefined behavior.

 @note This ia a low-level API that's usually not neeed.

 @param captureMethod capture method to use for recording
 */
- (void)setCaptureMethod:(MCLCaptureMethod)captureMethod;


/**
 Set how screen captures should be performed, using the default scale factor. Call this as soon
 as possible after start. Calling this more than once leads to undefined behavior.

 The default scale factor is 0.5 for screens whose longest side is < 1500 in length, or 0.25 for
 anything larger. If the resulting dimensions are less than 200 for either width or height, then the
 scale factor is increased to ensure a minimum of 200 or more in both dimensions. By passing in a
 value for ScaleFactor, you override this behavior. It's important to keep in mind that while a
 larger scale factor will produce encoded media with a higher resolution, it will make captures and
 encoding slower, and also increase the size of the encoded media, which will increase both disk and
 network usage. In any case, we will round up the scaled dimensions to be divisible by 16, as this
 is a requirement for many MP4 encoders.

 @note This ia a low-level API that's usually not neeed.

 @param captureMethod capture method to use for recording
 @param scaleFactor a custom scale factor to use for encoding recordings
 */
- (void)setCaptureMethod:(MCLCaptureMethod)captureMethod withScaleFactor:(double)scaleFactor;


/**
 Get the total score for the given recording.

 By observing this value you can learn what scores are average and which are good in your game,
 and use this to only prompt the user to share if it was a high-scoring recording, or promote
 high-scoring recordings in the game or use it to set the share text.

 The score will be 0 if the recording doesn't use the highlight overflow strategy, or if
 `registerScoreChange` has never been called.

 @param recordingId The recording to fetch the score for. Fetches the default if `nil`.
 @return The score for the given recording, or -1 if the recording couldn't be found.
 */
- (int)getRecordingScore:(nullable NSString *)recordingId;


#pragma mark - Preview


/**
 Get a view that previews the default recording.

 Call `startAnimating` on the returned `MCLPreview` to start the animation. Use
 `getPreviewWithConfig` to customize which recording to preview, or set where the view should
 appear. You can also set the `frame` on the returned view to position it.

 @return MCLPreview for the default recording.
 */
- (MCLPreview *)getPreview;


/**
 Render preview of GIF with a given frame size that can be showed before sharing.

 Call `startAnimating` on the returned `MCLPreview` to start the animation.

 @param configBlock Configuration block to configure the preview.
 @return MCLPreview for the GIF specified in `config.recordingId`, or the default if not given.
 Will return `nil` if the requested recording has zero frames or does not exist.
 */
- (nullable MCLPreview *)getPreviewWithConfig:(nullable MCLPreviewConfigBlock)configBlock;


/**
 Get the data needed to build your own preview.

 This is a low-level interface intended for custom engines, you're probably better
 off using the `getPreview`/`getPreviewWithConfig` methods.

 @param recordingId which recording to get preview data for, or `nil` for the default.
 @return MCLPreviewData for the given recording, or `nil` if there's no such recording.
 */
- (nullable MCLPreviewData *)getPreviewDataForRecording:(nullable NSString *)recordingId;


#pragma mark - Shares


/**
 Present the default share modal from the top view controller.

 Shows a native share modal view with supported sharing channels like SMS, Twitter,
 Facebook etc.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShare];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShare;


/**
 Present the default share modal from the top view controller.

 Shows a native share modal view with supported sharing channels like SMS, Twitter,
 Facebook etc.

 @param configBlock configures how the share is performed.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareWithConfig:^(MCLShareConfig *config) {
        config.recordingId = @"level1";
    }];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareWithConfig:(nullable MCLShareConfigBlock)configBlock;


/**
 Present the default share modal view from a given view controller.

 Shows a native share modal view with supported sharing channels like SMS, Twitter,
 Facebook etc.

 @param configBlock configures how the share is performed.
 @param viewController The view controller that will present the share modal view. If `nil` the top
        view controller from the application delegate will be used.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareWithConfig:^(MCLShareConfig *config) {
        config.recordingId = @"level1";
    } inViewController:self];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareWithConfig:(nullable MCLShareConfigBlock)configBlock
              inViewController:(nullable UIViewController *)viewController;


/**
 Present a native iMessage share view

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareToMessages];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareToMessages;


/**
 Present a native iMessage share view

 @param configBlock configures how the share is performed.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareToMessagesWithConfig:^(MCLShareConfig *config) {
        config.recordingId = @"level1";
    }];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareToMessagesWithConfig:(nullable MCLShareConfigBlock)configBlock;


/**
 Present a native iMessage share view from a given view controller.

 @param configBlock configures how the share is performed.
 @param viewController The view controller that will present the iMessage view. If `nil` the top
        view controller from the application delegate will be used.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareToMessagesWithConfig:^(MCLShareConfig *config) {
        config.recordingId = @"level1";
    } inViewController:self];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareToMessagesWithConfig:(nullable MCLShareConfigBlock)configBlock
                        inViewController:(nullable UIViewController *)viewController;


/**
 Present a native Mail share view

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareToMail];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareToMail;

/**
 Present a native Mail share view

 @param configBlock configures how the share is performed.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareToMailWithConfig:^(MCLShareConfig *config) {
        config.recordingId = @"level1";
    }];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareToMailWithConfig:(nullable MCLShareConfigBlock)configBlock;


/**
 Present a native Mail share view from a given view controller.

 @param configBlock configures how the share is performed.
 @param viewController The view controller that will present the Mail share view. If `nil` the top
        view controller from the application delegate will be used.

 @note This method must be called from the main thread of your application.

 On some devices this might block for some time, to prevent user frustration we recommend showing a
 spinner and calling this method from a selector with
 <tt>[self performSelector:withObject:afterDelay:]</tt>, using a delay of .1 (otherwise some
 devices won't show the spinner at all before blocking). You should then stop and remove the
 spinner once this method returns. Example:

 @code
 - (void)share {
    [self.view addSubview:self.spinner];
    [self.spinner startAnimating];
    self.spinner.center = self.view.center;
    [self performSelector:@selector(shareAfterSpinner)
               withObject:nil
               afterDelay:.1];
 }

 - (void)shareAfterSpinner {
    [[Megacool sharedMegacool] presentShareToMailWithConfig:^(MCLShareConfig *config) {
        config.recordingId = @"level1";
    } inViewController:self];
    [self.spinner stopAnimating];
    [self.spinner removeFromSuperview];
 }
 @endcode
 */
- (void)presentShareToMailWithConfig:(nullable MCLShareConfigBlock)configBlock
                    inViewController:(nullable UIViewController *)viewController;


/**
 Get the shares sent by the user.

 The locally cached shares will be returned immediately and are useful for determining
 how many shares have been sent and when that was done. A network request will query
 the backend for an updated status on the shares to see if anything has happened since last time,
 and passed to the callback if given.

 By default this returns the most recent 25 shares, use `-getShares:withFilter` to
 customize which shares should be updated.

 Calling this method will also trigger a check for any new `MCLSentShareOpenedEvent`s for the user.

 @param callback a callback that will receive the updated shares, or `nil`. This will be called on
 an arbitrary non-main dispatch queue.
 @return the 25 most recent shares.
 */
- (NSArray<MCLShare *> *)getShares:(nullable MCLShareStateCallback)callback;


/**
 Get the shares sent by the user, with filtering.

 Use this instead of `-getShares:` when you want a specific subset of the shares, to keep network
 traffic to a minimum. The locally cached shares will be returned immediately and are useful
 for determining how many shares have been sent and when that was done. A network request will query
 the backend for an updated status on the shares to see if anything has happened since last time,
 and passed to the callback if given.

 Calling this method will also trigger a check for any new `MCLSentShareOpenedEvent`s for the user.

 If `filter` is `nil` the 25 most recent shares will be returned and updated. If you want to get an
 updated state for shares from the last week that haven't yet generated an install you could pass a
 filter like this:

 @code
 ^BOOL(MCLShare *share) {
    return fabs([share.createdAt timeIntervalSinceNow]) < 60*60*24*7
        && share.state != MCLShareStateInstalled;
 }
 @endcode

 Note that only 25 shares can be updated in a single HTTP request, requesting more than 25 shares to
 be updated will cause several network requests and increase the latency before the callback is
 called. Keep the share count below 25 to keep bandwidth usage low and the response quick.

 @param callback a callback that will receive the updated shares, or `nil`. This will be called on
 an arbitrary non-main dispatch queue.
 @param filter a filter determines which shares should be updated, return `YES` if the share should
 be updated and `NO` otherwise. A default filter will be used if `nil`.
 @return the shares available locally for which the filter returns `YES`
 */
- (NSArray<MCLShare *> *)getShares:(nullable MCLShareStateCallback)callback
                        withFilter:(nullable MCLShareFilterCallback)filter;


/**
 Delete local share objects

 Local shares are also useful to show the users how many shares they've sent and what
 their statuses are. But at a certain time you might want to delete old ones.

 @param filter is used to delete old share objects you don't want local anymore. I.e. shares that
 have been installed and are over 2 days old can be deleted. For each share, return `YES/NO`
 if it should be deleted.
 */
- (void)deleteSharesMatchingFilterFromDevice:(nullable MCLShareFilterCallback)filter;


/**
 Get the user id for this user/app.

 The user id will be passed to the callback immediately if known, otherwise the
 callback will be called when we've received the user id from the backend, which happens on the
 first session, and will be stored locally after that.
 */
- (void)getUserId:(MCLReferralCodeCallback)callback;


#pragma mark - Configuration

/**
 Set the default share config. This will be merged with the config given to
 `presentShareWithConfig:`, if any.

 Set to `nil` to reset.
 */
@property(nonatomic, null_resettable, strong) MCLShareConfig *defaultShareConfig;


/**
 The default recording config. This will be merged with the config given to
 `startRecording:withConfig` or `captureFrame:withConfig`, if any.

 Note that the config is mutable, but changes made to the default config after a call to
 startRecording or captureFrame will have no effect until the next recording is started.

 Set to `nil` to reset.
 */
@property(nonatomic, null_resettable, strong) MCLRecordingConfig *defaultRecordingConfig;


/**
 Set the type of GIF color table to use. The default is `kMCLGIFColorTableDynamic`.

 The resulting GIF can only hold 256 different colors, this property determines how those colors
 are selected.

 This only has any effect when sharing to apps where .gif gives a better experience than .mp4. Try
 sharing to email or messages to see the impact of this.
 */
@property(nonatomic) MCLGIFColorTable gifColorTable;


/**
 Set which Metal texture to capture from.
 */
@property(nonatomic, nullable, strong) id<MTLTexture> metalCaptureTexture;


/**
 Whether to keep completed recordings around.

 The default is `NO`, which means that all completed recordings will be deleted
 whenever a new recording is started with either `captureFrame` or `startRecording`. Setting this
 to `YES` means we will never delete a completed recording, which is what you want if you want to
 enable players to browse previous GIFs they've created. A completed recording will still be
 overwritten if a new recording is started with the same `recordingId.`
 */
@property(nonatomic) BOOL keepCompletedRecordings;


#pragma mark - Low-level capture API


/**
 Low-level API for custom engines: Call this from the rendering thread to initialize the capture.

 Calling this is optional, but reduces the overhead of the first call to
 `-[Megacool notifyRenderComplete]`.
 */
- (void)initRenderThread;


/**
 Low-level API for custom engines: Call this after issuing all drawing commands for a frame on the
 rendering thread.

 Note that the first call to this function have to do some more work to initialize the capture
 if {@link #initRenderThread()} hasn't been called, thus you might want to call that during
 startup of the app to prevent any lag when recording starts.
 */
- (void)notifyRenderComplete;


#pragma mark - Debugging/development


/**
 Turn on / off debug mode. In debug mode calls to the SDK are stored and can be submitted to
 the core developers using `submitDebugDataWithMessage` later.
 */
+ (void)setDebug:(BOOL)debug;


/**
 Get whether debugging is currently enabled or not.
 */
+ (BOOL)debug;


/**
 Disable a set of features

 If something fails or is not desired on a class of devices, some features can be
 disabled remotely through the dashboard. To be able to test behavior when this is the case, or to
 always force a given feature to be disabled, you can call this function with a list of the features
 you want to disable.

 Features disabled through this call will not be visible or configurable through the dashboard. A
 feature will be disabled if it's disabled either through this call or remotely or both. This call
 will overwrite any effects of previous calls, thus calling `disableFeatures:kMCLFeatureGifs`
 followed by `disableFeatures:kMCLFeatureAnalytics` would leave only analytics disabled.
 Combine features with the bitwise OR operator `|` to disable multiple features, like
 `disableFeatures:kMCLFeatureAnalytics|kMCLFeatureGifUpload`.

 The supported features you can disable is:
 * GIFs: Disable all recording, frame capturing and subsequent creation of the GIF. Available
 as the constant `kMCLFeatureGifs.`
 * GIF uploading: By default all GIFs created are uploaded to our servers, which is
 necessaryto be able to share GIFs to Facebook and Twitter, and for you to see them in the
 dashboard. This does have some networking overhead though, so if you're very constrained in terms
 of bandwidth you can disable this. Include the constant `kMCLFeatureGifUpload.`
 * Analytics: To be able to determine whether an install or an app open event came from a
 link we have to submit some events to our servers to be able to match the event to a link click
 detected by us. If you are very concerned about
 your user's privacy or don't want to incur the extra networking required, you can disable this
 feature. Note that users will not be credited for inviting friends if this feature is off. Include
 the constant `kMCLFeatureAnalytics.`
 */
- (void)disableFeatures:(MCLFeature)features;


/**
 Submit debug data to the core developers along with a message explaining the expected
 outcome and what was observed instead.

 Remember to set `Megacool.debug = YES;` on startup if you intend to use this
 call, otherwise the report will nots contain call traces and it'll be harder to reproduce your
 issue.
 */
- (void)submitDebugDataWithMessage:(NSString *)message;


/**
 Resets the device identity, enabling it to receive events with `isFirstSession`=YES again.

 Use this if you're testing the invite flow and you want to wipe previous data from the
 device. This will issue your device a new identity, which means it can receive
 `recievedShareOpened` events again with `isFirstSession` set to `YES`, and enabling you to click
 previous links sent by the same device, mitigating the need for multiple devices to test invites.

 This method should be called before `-startWithAppConfig:`, otherwise the changes will not have
 any effect until the next session.
 */
+ (void)resetIdentity;
@end

NS_ASSUME_NONNULL_END
