//
//  particle_generator.cpp
//  OpenGLEnv
//
//  Created by karos li on 2021/5/11.
//

#include "particle_generator.h"

ParticleGenerator::ParticleGenerator(Shader shader, Texture2D texture, GLuint amount)
    : shader(shader), texture(texture), amount(amount)
{
    this->init();
}

/**
 在每一帧里面，我们都会用一个起始变量来产生一些新的粒子并且对每个粒子（还活着的）更新它们的值。
 */
void ParticleGenerator::Update(GLfloat dt, GameObject &object, GLuint newParticles, glm::vec2 offset)
{
    // Add new particles
    for (GLuint i = 0; i < newParticles; ++i)
    {
        int unusedParticle = this->firstUnusedParticle();
        this->respawnParticle(this->particles[unusedParticle], object, offset);
    }
    // Update all particles
    for (GLuint i = 0; i < this->amount; ++i)
    {
        Particle &p = this->particles[i];
        p.Life -= dt; // reduce life
        if (p.Life > 0.0f)
        {    // particle is alive, thus update
            // 粒子的位置与球的速度方向是相反的，p.Velocity 是 粒子的初始速度=球的速度
            p.Position -= p.Velocity * dt;
            p.Color.a -= dt * 2.5;
        }
    }
}

// Render all particles
void ParticleGenerator::Draw()
{
    /**
     对于每个粒子，我们一一设置他们的uniform变量offse和color，绑定纹理，然后渲染2D四边形的粒子
     
     我们在这看到了两次调用函数glBlendFunc。当要渲染这些粒子的时候，我们使用GL_ONE替换默认的目的因子模式GL_ONE_MINUS_SRC_ALPHA，这样，这些粒子叠加在一起的时候就会产生一些平滑的发热效果，就像在这个教程前面那样使用混合模式来渲染出火焰的效果也是可以的，这样在有大多数粒子的中心就会产生更加灼热的效果。
     */
    // Use additive blending to give it a 'glow' effect
    glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    this->shader.Use();
    for (Particle particle : this->particles)
    {
        if (particle.Life > 0.0f)
        {
            this->shader.SetVector2f("offset", particle.Position);
            this->shader.SetVector4f("color", particle.Color);
            this->texture.Bind();
            glBindVertexArray(this->VAO);
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        }
    }
    // Don't forget to reset to default blending mode
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
}

void ParticleGenerator::init()
{
    // Set up mesh and attribute properties
    GLuint VBO;
    GLfloat particle_quad[] = {
        // 位置      // 纹理坐标
        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 0.0f, 0.0f,

        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f
    };
    /**
     因为正交投影矩阵的影响，[0,0]在屏幕左上角
     6个顶点都是在 x, y 的正方向上

               |
               |
     ----------------------------------> x
               |                |
               |                |
               |----------- |
               |
               y
     */
    glGenVertexArrays(1, &this->VAO);
    glGenBuffers(1, &VBO);
    glBindVertexArray(this->VAO);
    // Fill mesh buffer
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(particle_quad), particle_quad, GL_STATIC_DRAW);
    // Set mesh attributes
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 4, GL_FLOAT, GL_FALSE, 4 * sizeof(GLfloat), (GLvoid*)0);
    glBindVertexArray(0);

    // Create this->amount default particle instances
    for (GLuint i = 0; i < this->amount; ++i)
        this->particles.push_back(Particle());
}


/**
 这个函数存储了它找到的上一个消亡的粒子的索引值，由于下一个消亡的粒子索引值总是在上一个消亡的粒子索引值的右边，所以我们首先从它存储的上一个消亡的粒子索引位置开始查找，如果我们没有任何消亡的粒子，我们就简单的做一个线性查找，如果没有粒子消亡就返回索引值0，结果就是第一个粒子被覆盖，需要注意的是，如果是最后一种情况，就意味着你粒子的生命值太长了，在每一帧里面需要产生更少的粒子，或者你只是没有保留足够的粒子，
 */
// Stores the index of the last particle used (for quick access to next dead particle)
GLuint lastUsedParticle = 0;
GLuint ParticleGenerator::firstUnusedParticle()
{
    // First search from last used particle, this will usually return almost instantly
    for (GLuint i = lastUsedParticle; i < this->amount; ++i){
        if (this->particles[i].Life <= 0.0f){
            lastUsedParticle = i;
            return i;
        }
    }
    // Otherwise, do a linear search
    for (GLuint i = 0; i < lastUsedParticle; ++i){
        if (this->particles[i].Life <= 0.0f){
            lastUsedParticle = i;
            return i;
        }
    }
    // All particles are taken, override the first one (note that if it repeatedly hits this case, more particles should be reserved)
    lastUsedParticle = 0;
    return 0;
}

void ParticleGenerator::respawnParticle(Particle &particle, GameObject &object, glm::vec2 offset)
{
    /**
     一旦粒子数组中第一个消亡的粒子被发现的时候，我们就通过调用RespawnParticle函数更新它的值，函数接受一个Particle对象，一个GameObject对象和一个offset向量:
     */
    GLfloat random = ((rand() % 100) - 50) / 10.0f;// [0, 99] => [-50, 49] => [-5, 4.9]
    particle.Position = object.Position + random + offset;
    
    GLfloat rColor = 0.5 + ((rand() % 100) / 100.0f);// 0.5 + [0, 99] => 0.5 + [0, 0.99]
    particle.Color = glm::vec4(rColor, rColor, rColor, 1.0f);
    
    particle.Life = 1.0f;
    particle.Velocity = object.Velocity * 0.1f;
}
