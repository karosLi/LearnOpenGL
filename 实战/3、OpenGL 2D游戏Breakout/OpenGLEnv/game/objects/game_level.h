//
//  game_level.hpp
//  OpenGLEnv
//
//  Created by karos li on 2021/4/27.
//

#ifndef GAMELEVEL_H
#define GAMELEVEL_H

#include <vector>

#include <glad/glad.h>
#include <glm/glm.hpp>

#include "game_object.h"
#include "sprite_renderer.h"
#include "resource_manager.h"

/**
 由于关卡数据从外部文本中加载，所以我们需要提出某种关卡的数据结构，以下是关卡数据在文本文件中可能的表示形式的一个例子
 1 1 1 1 1 1
 2 2 0 0 2 2
 3 3 4 4 3 3
 
 在这里一个关卡被存储在一个矩阵结构中，每个数字代表一种类型的砖块，并以空格分隔。在关卡代码中我们可以假定每个数字代表什么：

 数字0：无砖块，表示关卡中空的区域
 数字1：一个坚硬的砖块，不可被摧毁
 大于1的数字：一个可被摧毁的砖块，不同的数字区分砖块的颜色
 
 https://learnopengl-cn.github.io/img/06/Breakout/04/levels-example.png
 
 */
class GameLevel
{
public:
    std::vector<GameObject> Bricks;

    GameLevel() { }
    // 从文件中加载关卡
    void Load(const GLchar *file, GLuint levelWidth, GLuint levelHeight);
    // 渲染关卡
    void Draw(SpriteRenderer &renderer);
    // 检查一个关卡是否已完成 (所有非坚硬的瓷砖均被摧毁)
    GLboolean IsCompleted();
private:
    // 由砖块数据初始化关卡
    void init(std::vector<std::vector<GLuint>> tileData, GLuint levelWidth, GLuint levelHeight);
};

#endif
