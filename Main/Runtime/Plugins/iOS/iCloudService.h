#import <Foundation/Foundation.h>

@interface iCloudService : NSObject
+ (void)saveStringToiCloud:(NSString *)key value:(NSString *)value;
+ (void)retrieveStringFromiCloud:(NSString *)key completion:(void (^)(NSString *value, NSError *error))completion;
@end
