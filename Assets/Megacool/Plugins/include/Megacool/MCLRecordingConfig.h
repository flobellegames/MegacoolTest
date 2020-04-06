//
//  MCLRecordingConfig.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "MCLConstants.h"

/**
 Pass this to `-[Megacool startRecording:withConfig:]` to configure how recordings are made.

 @note This class does not have atomic properties for efficiency, thus it's not thread-safe.
 */
@interface MCLRecordingConfig : NSObject <NSCopying>
NS_ASSUME_NONNULL_BEGIN

/**
 How to handle frames when the count surpasses `maxFrames.`
 */
@property(nonatomic) MCLOverflowStrategy overflowStrategy;


/**
  An identifier for this recording, useful when you might have multiple in-progress
 recordings.

 Can be used to retrieve the same recording later, for previews or shares, or to resume
 a paused recording.
 */
@property(nonatomic, null_resettable, strong) NSString *recordingId;


/**
 Max number of frames in a recording. If set to `1` the recording will be shared as a still image
 instead of a gif or mp4.

 Default is 50 frames. What happens when a recording grows above the `maxFrames` limit
 is determined by the `MCLOverflowStrategy`.
 */
@property(nonatomic) int maxFrames;


/**
 Set the capture frame rate. The default is 10.
 */
@property(nonatomic) int frameRate;


/**
 Set the playback frame rate. Set this higher or lower than the capture frame rate to speed up or
 slow down the recording.

 The default is 20% more than the capture frame rate.
 */
@property(nonatomic) int playbackFrameRate;


/**
 Set the location of the highest scoring moment (the peak) in a highlight recording. Must be between
 `0` and `1`, the default is `0.7`. Set the score throughout the game with
 `-[Megacool registerScoreChange]`. The recording must be using the highlight overflow strategy for
 this to have any effect.

 For example, in a recording with 10 frames, a peak location of `0.2` means that the highest
 scoring frame will occur near the beginning at frame 2, and a peak location of `0.8` means that the
 peak will occur near the end at frame 8.
 */
@property(nonatomic) double peakLocation;


/**
 Set a delay (in milliseconds) on the last frame in the animation.

 Default is 1000 (1s). If set to 0 the last frame will have the same duration as the
 rest of the frames, ie. 1/playbackFrameRate.
 */
@property(nonatomic) int lastFrameDelay;


/**
 Set a default custom overlay for the last frame in the animation.

 This can also be set as a URL if you have the image on disk with
 `lastFrameOverlayUrl.`
 */
@property(nonatomic, strong, nullable) UIImage *lastFrameOverlay;


/**
 Set a default custom overlay for the last frame in the animation, by URL.

 Same as `lastFrameOverlay`, but given as URL to a local filesystem path
 instead of `UIImage.`
 */
@property(nonatomic, strong, nullable) NSURL *lastFrameOverlayUrl;

NS_ASSUME_NONNULL_END
@end
