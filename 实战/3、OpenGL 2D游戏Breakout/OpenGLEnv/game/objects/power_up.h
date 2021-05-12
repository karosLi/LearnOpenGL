//
//  power_up.hpp
//  OpenGLEnv
//
//  Created by karos li on 2021/5/11.
//

/**
 每个道具以字符串的形式定义它的类型，持有表示它有效时长的持续时间与表示当前是否被激活的属性。在Breakout中，我们将添加4种增益道具与2种负面道具：
 
 https://learnopengl-cn.github.io/img/06/Breakout/08/powerups.png
 
 Speed: 增加小球20%的速度
 Sticky: 当小球与玩家挡板接触时，小球会保持粘在挡板上的状态直到再次按下空格键，这可以让玩家在释放小球前找到更合适的位置
 Pass-Through: 非实心砖块的碰撞处理被禁用，使小球可以穿过并摧毁多个砖块
 Pad-Size-Increase: 增加玩家挡板50像素的宽度
 Confuse: 短时间内激活confuse后期特效，迷惑玩家
 Chaos: 短时间内激活chaos后期特效，使玩家迷失方向
 
 */

#ifndef POWER_UP_H
#define POWER_UP_H
#include <string>

#include <glad/glad.h>
#include <glm/glm.hpp>

#include "game_object.h"

// The size of a PowerUp block
const glm::vec2 SIZE(60, 20);
// Velocity a PowerUp block has when spawned
const glm::vec2 VELOCITY(0.0f, 150.0f);


// PowerUp inherits its state and rendering functions from
// GameObject but also holds extra information to state its
// active duration and whether it is activated or not.
// The type of PowerUp is stored as a string.
class PowerUp : public GameObject
{
public:
    // PowerUp State - 道具类型
    std::string Type;
    GLfloat     Duration;
    GLboolean   Activated;
    // Constructor
    PowerUp(std::string type, glm::vec3 color, GLfloat duration, glm::vec2 position, Texture2D texture)
        : GameObject(position, SIZE, texture, color, VELOCITY), Type(type), Duration(duration), Activated() { }
};

#endif
