//
//  OpenGLEnvTests.m
//  OpenGLEnvTests
//
//  Created by karos li on 2021/3/17.
//

/**
 依赖包下载
 https://glad.dav1d.de/
 https://www.glfw.org/download.html
 https://github.com/nothings/stb/blob/master/stb_image.h
 https://glm.g-truc.net/0.9.8/index.html
 
 imgui 调试窗口
 https://github.com/ocornut/imgui
 https://github.com/0kk470/learnOpenGL/blob/master/HelloMaterial.cpp
 
 教程源码和资源
 https://github.com/JoeyDeVries/LearnOpenGL
 
 音频库
 https://www.ambiera.com/irrklang/downloads.html
 MacApp 集成 irrklang 出现的链接问题解决
 https://www.ambiera.com/irrklang/faq.html#buildingmacos
 但是实际测试结果：只要加载 MP3 动态库就会报错，另外两个库不会报错，可见 irrklang 对 MAC 支持不友好，所以还是先去掉了
 dyld: Library not loaded: /usr/local/lib/libikpMP3.dylib
 
 */

#import <XCTest/XCTest.h>

@interface OpenGLEnvTests : XCTestCase

@end

@implementation OpenGLEnvTests

- (void)setUp {
    // Put setup code here. This method is called before the invocation of each test method in the class.
}

- (void)tearDown {
    // Put teardown code here. This method is called after the invocation of each test method in the class.
}

- (void)testExample {
    // This is an example of a functional test case.
    // Use XCTAssert and related functions to verify your tests produce the correct results.
}

- (void)testPerformanceExample {
    // This is an example of a performance test case.
    [self measureBlock:^{
        // Put the code you want to measure the time of here.
    }];
}

@end
