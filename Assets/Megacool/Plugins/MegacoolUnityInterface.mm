//
//  MegacoolUnityInterface.h
//  Unity-iPhone
//
//  Created by Nicolaj Petersen on 07/18/17.
//

#import "AppDelegateListener.h"
#import "Megacool.h"

@interface MegacoolUnityInterface : NSObject<AppDelegateListener>
+ (MegacoolUnityInterface *)sharedInstance;
@end

static MegacoolUnityInterface *_instance = [MegacoolUnityInterface sharedInstance];

@implementation MegacoolUnityInterface

#pragma mark Object Initialization

+ (MegacoolUnityInterface *)sharedInstance {
    return _instance;
}

+ (void)initialize {
    if (!_instance) {
        _instance = [[MegacoolUnityInterface alloc] init];
    }
}

- (id)init {
    if (_instance != nil) {
        return _instance;
    }

    if ((self = [super init])) {
        _instance = self;

        UnityRegisterAppDelegateListener(self);
    }
    return self;
}

#pragma mark - App (Delegate) Lifecycle

// didBecomeActive: and onOpenURL: are called by Unity's AppController
// because we implement <AppDelegateListener> and registered via
// UnityRegisterAppDelegateListener(...) above.
- (void)onOpenURL:(NSNotification *)notification {
    NSURL *url = notification.userInfo[@"url"];
    [[Megacool sharedMegacool] openURL:url
                     sourceApplication:notification.userInfo[@"sourceApplication"]];
}

@end
