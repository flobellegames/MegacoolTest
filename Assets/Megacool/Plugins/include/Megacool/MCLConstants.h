//
//  MCLConstants.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <Foundation/Foundation.h>

#import "MegacoolFFI.h"

/**
 How the colors in the GIF should be computed.
 */
typedef NS_ENUM(int, MCLGIFColorTable) {
    /**
     Compute the color table dynamically based on the frames.

     This is the default, and yields better visual quality than kMCLGIFColorTableFixed
     but a bit slower. Visually this should be about the same as kMCLGIFColorTableAnalyze,
     performance depends a bit on the specific recording, but should be roughly equal, but with less
     memory use.
     */
    kMCLGIFColorTableDynamic = MCL_GIF_COLOR_TABLE_DYNAMIC,

    /**
     Use a fixed color palette.

     This is the fastest option, but usually doens't look as good if the recording
     contains a lot of nuanced colors or gradients.
     */
    kMCLGIFColorTableFixed = MCL_GIF_COLOR_TABLE_FIXED,

    /**
     Compute the color table by analyzing the frames first.

     This is a bit slower than kMCLGIFColorTableFixed, but results in better
     reproduction of the colors. This value is iOS only, the two others will give the same results
     on iOS and Android.
     */
    kMCLGIFColorTableAnalyzeFirst = MCL_GIF_COLOR_TABLE_ANALYZE_FIRST,
};

/**
 What type of data should be prioritized when sharing
 */
typedef NS_ENUM(unsigned long, MCLSharingStrategy) {
    /**
     Prioritize media.
     */
    kMCLSharingStrategyMedia = 0,

    /**
     Prioritize links (this is the default).
     */
    kMCLSharingStrategyLink,
};

/**
 An overflow strategy determines how a recording will handle new frames when the total surpasses the
 configured `MCLRecordingConfig.maxFrames`.
 */
typedef NS_ENUM(unsigned long, MCLOverflowStrategy) {
    /**
     Keep only the last `MCLRecordingConfig.maxFrames` frames.

     This works well when the best part of the game is in the final moments.
     */
    kMCLOverflowStrategyLatest = 0,

    /**
     Gradually speed up the recording when new frames arrive, making sure the entire segment from
     start to finish is included.

     This works well when it’s interesting to see how something evolved throughout the game, like a
     city in Sim City, or a drawing in a coloring book.

     Note that the actual max number of frames when using this strategy can vary between
     `1.33*maxFrames` and `0.67*maxFrames`.
     */
    kMCLOverflowStrategyTimelapse,

    /**
     Include the part of the recording with the highest intensity as determined by calls to
     `-[Megacool registerScoreChange]`.

     This works well when it’s not known up front when something interesting will happen, like in a
     first-person shooter. Set the `MCLRecordingConfig.peakLocation` to determine where in the
     recording the most intense point should be located.

     @see `-[Megacool registerScoreChange]`
     @see `MCLRecordingConfig.peakLocation`
     */
    kMCLOverflowStrategyHighlight,
};

// Features that can be disabled
// NB: The values declared here have to corresepond to what is defined by the API
typedef NS_OPTIONS(unsigned long, MCLFeature) {
    // clang-format off
    kMCLFeatureNone           = 0,
    kMCLFeatureGifs           = 1 << 0,
    kMCLFeatureAnalytics      = 1 << 1,
    kMCLFeatureGifUpload      = 1 << 2,
    kMCLFeatureGifPersistency = 1 << 3,
    kMCLFeatureShareNoCopyGif = 1 << 4,
    kMCLFeatureShareCopyLink  = 1 << 5,
    // clang-format on
};

/**
 How screen captures are performed.
 */
typedef NS_ENUM(unsigned long, MCLCaptureMethod) {
    /**
     Re-draw the given view (this is the default)
     */
    kMCLCaptureMethodView = MCL_CAPTURE_METHOD_VIEW,


    /**
     Capture via OpenGL ES 2. Setting this requires also setting a texture to capture from using the
     native function `void mcl_set_capture_texture(void *texture_pointer)` with the GLint texture
     name of a texture that holds the data to be captured.

     You have to call `-[Megacool notifyRenderComplete]` at some point during the graphics pipeline
     to make this work.
     */
    kMCLCaptureMethodOpenGLES2 = MCL_CAPTURE_METHOD_OPENGLES2,


    /**
     Capture via OpenGL ES 3.

     You have to call `-[Megacool notifyRenderComplete]` at some point during the graphics pipeline
     to make this work.
     */
    kMCLCaptureMethodOpenGLES3 = MCL_CAPTURE_METHOD_OPENGLES3,


    /**
     Capture via Metal. Setting this requires also setting a texture to capture from using the
     native function `void mcl_set_capture_texture(void *texture_pointer)` with the `id<MTLTexture>`
     that holds the data to be captured.

     You have to call `-[Megacool notifyRenderComplete]` at some point during the graphics pipeline
     to make this work.
     */
    kMCLCaptureMethodMetal = MCL_CAPTURE_METHOD_METAL,
};
