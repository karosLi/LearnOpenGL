//
//  RelativePath.cpp
//  OpenGLEnv
//
//  Created by karos li on 2021/4/6.
// https://www.jianshu.com/p/749a781fef94

#include <stdio.h>

#include "RelativePath.h"

#include <iostream>
#include <fstream>
#include <string>

using namespace std;

char * getAppPath()
{
    printf("%s, %d\n", __func__, __LINE__);
    
    ifstream inputFile;
    string countries;
    
    // This makes relative paths work in C++ in Xcode by changing directory to the Resources folder inside the .app bundle
#ifdef __APPLE__
    CFBundleRef mainBundle = CFBundleGetMainBundle();
    CFURLRef resourcesURL = CFBundleCopyResourcesDirectoryURL(mainBundle);
    char path[PATH_MAX];
    if (!CFURLGetFileSystemRepresentation(resourcesURL, TRUE, (UInt8 *)path, PATH_MAX)) {
        // error!
        
    }
    CFRelease(resourcesURL);
    chdir(path);
    
    std::string sourcePath(path);
    
#if TARGET_OS_IPHONE || TARGET_OS_TV
    // /var/containers/Bundle/Application/8A7E3671-5BE7-4C02-867B-58C53A391397/iOSTest.app/model.bundle/load.bin
    std::cout << "Current Path: " << sourcePath << std::endl;
//    std::cout << "Current Path: " << (sourcePath + "/model.bundle/load.bin") << std::endl;
    
#elif TARGET_OS_MAC
    // /Users/xxxxxx/Library/Developer/Xcode/DerivedData/MacTest-fnhmrwbqmbccbxhjywjehgdoqfib/Build/Products/Debug/MacTest.app/Contents/Resources/model.bundle/load.bin
    std::cout << "Current Path: " << sourcePath << std::endl;
//    std::cout << "Current Path: " << (sourcePath + "/model.bundle/load.bin") << std::endl;
    
#endif
    
#endif

    
//    inputFile.open(sourcePath + "/load.bin"); // The name of the file you set up.
//
//    while(inputFile >> countries) {
//        cout << countries << endl;
//    }
//
//    inputFile.close();
    
    return path;
}
