//
//  RelativePath.h
//  OpenGLEnv
//
//  Created by karos li on 2021/4/6.
//

#ifndef RelativePath_h
#define RelativePath_h

#ifdef __cplusplus
extern"C" {
#endif

#include <stdio.h>

#ifdef __APPLE__
#include "CoreFoundation/CoreFoundation.h"
#endif
    
/**
 用法
 
 const char *appPath = getAppPath();
 std::string appPathStr(appPath);
 std::string fullPath = appPathStr + "/container.jpeg";
 */
char * getAppPath();
    
#ifdef __cplusplus
}
#endif

#endif /* RelativePath_h */
