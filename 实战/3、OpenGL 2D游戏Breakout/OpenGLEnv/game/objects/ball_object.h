//
//  ball_object_collisions.hpp
//  OpenGLEnv
//
//  Created by karos li on 2021/4/28.
//

#ifndef BALLOBJECT_H
#define BALLOBJECT_H

#include <glad/glad.h>
#include <glm/glm.hpp>

#include "texture.h"
#include "sprite_renderer.h"
#include "game_object.h"

// BallObject holds the state of the Ball object inheriting
// relevant state data from GameObject. Contains some extra
// functionality specific to Breakout's ball object that
// were too specific for within GameObject alone.
class BallObject : public GameObject
{
public:
    // Ball state - 球的状态
    GLfloat   Radius;
    GLboolean Stuck;// 球是否是固定在挡板上
    GLboolean Sticky;// 球是否可以黏在挡板上
    GLboolean PassThrough;// 球是否可以穿过实心球
    // Constructor(s)
    BallObject();
    BallObject(glm::vec2 pos, GLfloat radius, glm::vec2 velocity, Texture2D sprite);
    // Moves the ball, keeping it constrained within the window bounds (except bottom edge); returns new position
    glm::vec2 Move(GLfloat dt, GLuint window_width);
    // Resets the ball to original state with given position and velocity
    void      Reset(glm::vec2 position, glm::vec2 velocity);
};

#endif
