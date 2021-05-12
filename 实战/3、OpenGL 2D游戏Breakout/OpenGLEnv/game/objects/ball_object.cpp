//
//  ball_object_collisions.cpp
//  OpenGLEnv
//
//  Created by karos li on 2021/4/28.
//

#include "ball_object.h"

BallObject::BallObject()
    : GameObject(), Radius(12.5f), Stuck(true), Sticky(GL_FALSE), PassThrough(GL_FALSE)  { }

BallObject::BallObject(glm::vec2 pos, GLfloat radius, glm::vec2 velocity, Texture2D sprite)
    :  GameObject(pos, glm::vec2(radius * 2, radius * 2), sprite, glm::vec3(1.0f), velocity), Radius(radius), Stuck(true), Sticky(GL_FALSE), PassThrough(GL_FALSE) { }

glm::vec2 BallObject::Move(GLfloat dt, GLuint window_width)
{
    // If not stuck to player board - 如果没有被固定在挡板上
    if (!this->Stuck)
    {
        /**
         移动球，并控制反弹效果
         */
        // Move the ball - 移动球
        this->Position += this->Velocity * dt;
        // Then check if outside window bounds and if so, reverse velocity and restore at correct position
        // 检查是否在窗口边界以外，如果是的话反转速度并恢复到正确的位置
        if (this->Position.x <= 0.0f)// 不超过窗口左边界
        {
            this->Velocity.x = -this->Velocity.x;
            this->Position.x = 0.0f;
        }
        else if (this->Position.x + this->Size.x >= window_width)// 不超过窗口右边界
        {
            this->Velocity.x = -this->Velocity.x;
            this->Position.x = window_width - this->Size.x;
        }
        if (this->Position.y <= 0.0f)// 不超过窗口上边界
        {
            this->Velocity.y = -this->Velocity.y;
            this->Position.y = 0.0f;
        }
    }
    return this->Position;
}

// Resets the ball to initial Stuck Position (if ball is outside window bounds)
void BallObject::Reset(glm::vec2 position, glm::vec2 velocity)
{
    this->Position = position;
    this->Velocity = velocity;
    this->Stuck = true;
    this->Sticky = GL_FALSE;
    this->PassThrough = GL_FALSE;
}
