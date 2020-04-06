//
//  MCLPreview.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#import <UIKit/UIKit.h>

/**
 A view that previews a recording.

 Created by `-[Megacool getPreview]`. The view is initially transparent, but
 becomes visible when we have loaded at least one frame to show.
 */
@interface MCLPreview : UIView

/**
 The number of frames for the preview.

 @note This number can potentially change.
 */
@property(readonly) int numberOfFrames;

/**
 Start the animation.
 */
- (void)startAnimating;

/**
 Stop the animation
 */
- (void)stopAnimating;

@end
