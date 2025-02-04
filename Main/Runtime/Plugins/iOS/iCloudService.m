#import "iCloudService.h"
#import <CloudKit/CloudKit.h>

@implementation iCloudService

+ (void)saveStringToiCloud:(NSString *)key value:(NSString *)value {
    CKContainer *container = [CKContainer defaultContainer];
    CKDatabase *database = [container privateCloudDatabase];
    
    CKRecordID *recordID = [[CKRecordID alloc] initWithRecordName:key];
    CKRecord *record = [[CKRecord alloc] initWithRecordType:@"StringRecord" recordID:recordID];
    record[@"value"] = value;
    
    [database saveRecord:record completionHandler:^(CKRecord *record, NSError *error) {
        if (error) {
            NSLog(@"Error saving to iCloud: %@", error.localizedDescription);
        } else {
            NSLog(@"Successfully saved to iCloud");
        }
    }];
}

+ (void)retrieveStringFromiCloud:(NSString *)key completion:(void (^)(NSString *value, NSError *error))completion {
    CKContainer *container = [CKContainer defaultContainer];
    CKDatabase *database = [container privateCloudDatabase];
    
    CKRecordID *recordID = [[CKRecordID alloc] initWithRecordName:key];
    
    [database fetchRecordWithID:recordID completionHandler:^(CKRecord *record, NSError *error) {
        if (error) {
            NSLog(@"Error retrieving from iCloud: %@", error.localizedDescription);
            if (completion) {
                completion(nil, error);
            }
        } else {
            NSString *retrievedValue = record[@"value"];
            if (completion) {
                completion(retrievedValue, nil);
            }
        }
    }];
}

@end
