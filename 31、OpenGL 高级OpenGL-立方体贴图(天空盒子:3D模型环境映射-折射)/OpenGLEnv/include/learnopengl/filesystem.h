//
//  filesystem.h
//  OpenGLEnv
//
//  Created by karos li on 2021/4/14.
//

#ifndef FILESYSTEM_H
#define FILESYSTEM_H

#include <string>
#include <cstdlib>
#include <CoreFoundation/CoreFoundation.h>

class FileSystem
{
private:
  typedef std::string (*Builder) (const std::string& path);

public:
  static std::string getPath(const std::string& path)
  {
    static std::string(*pathBuilder)(std::string const &) = getPathBuilder();
    return (*pathBuilder)(path);
  }

private:
  static std::string const & getRoot()
  {
    CFBundleRef mainBundle = CFBundleGetMainBundle();
    CFURLRef resourcesURL = CFBundleCopyResourcesDirectoryURL(mainBundle);
    char path[PATH_MAX];
    if (!CFURLGetFileSystemRepresentation(resourcesURL, TRUE, (UInt8 *)path, PATH_MAX)) {
      // error!
    }
    CFRelease(resourcesURL);
    
    static char const * envRoot = getenv("LOGL_ROOT_PATH");
    static char const * givenRoot = (envRoot != nullptr ? envRoot : path);
    static std::string root = (givenRoot != nullptr ? givenRoot : "");
    return root;
  }

  //static std::string(*foo (std::string const &)) getPathBuilder()
  static Builder getPathBuilder()
  {
    if (getRoot() != "")
      return &FileSystem::getPathRelativeRoot;
    else
      return &FileSystem::getPathRelativeBinary;
  }

  static std::string getPathRelativeRoot(const std::string& path)
  {
    return getRoot() + std::string("/") + path;
  }

  static std::string getPathRelativeBinary(const std::string& path)
  {
    return "../../../" + path;
  }


};

// FILESYSTEM_H
#endif
