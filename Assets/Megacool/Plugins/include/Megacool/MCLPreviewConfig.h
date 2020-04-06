//
//  MCLPreviewConfig.h
//
//  An online version of the documentation can be found at https://docs.megacool.co
//
//  Do you feel like this reading docs? (╯°□°）╯︵ ┻━┻  Give us a shout if there's anything that's
//  unclear and we'll try to brighten the experience. We can sadly not replace flipped tables.
//

#ifndef MCLPreviewConfig_h
#define MCLPreviewConfig_h

/**
 Pass this to `-[Megacool getPreviewWithConfig:]` to customize the preview.

 @note This class does not have atomic properties for efficiency, thus it's not thread-safe.
 */
@interface MCLPreviewConfig : NSObject

/**
 Which recording to create a preview for.

 Will use the default recording if nil.
 */
@property(nonatomic, null_resettable, strong) NSString *recordingId;

/**
 Where in the view the preview should be.
 */
@property(nonatomic) CGRect previewFrame;

@end

#endif /* MCLPreviewConfig_h */
