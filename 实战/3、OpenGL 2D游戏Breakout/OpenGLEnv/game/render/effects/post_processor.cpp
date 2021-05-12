//
//  post_processor.cpp
//  OpenGLEnv
//
//  Created by karos li on 2021/5/11.
//

/**
 总结一下这些渲染步骤：

 绑定至多重采样的帧缓冲
 和往常一样渲染游戏
 将多重采样的帧缓冲内容传输至一个普通的帧缓冲中（这个帧缓冲使用了一个纹理作为其颜色缓冲附件）
 解除绑定（绑定回默认的帧缓冲）
 在后期处理着色器中使用来自普通帧缓冲的颜色缓冲纹理
 渲染屏幕大小的四边形作为后期处理着色器的输出
 我们的后期处理着色器允许使用三种特效：shake, confuse和chaos。

 shake：轻微晃动场景并附加一个微小的模糊效果。
 shake：反转场景中的颜色并颠倒x轴和y轴。
 chaos: 利用边缘检测卷积核创造有趣的视觉效果，并以圆形旋转动画的形式移动纹理图片，实现“混沌”特效。
 */

#include "post_processor.h"

#include <iostream>

PostProcessor::PostProcessor(Shader shader, GLuint width, GLuint height)
    : PostProcessingShader(shader), Texture(), Width(width), Height(height), Confuse(GL_FALSE), Chaos(GL_FALSE), Shake(GL_FALSE)
{
    // Initialize renderbuffer/framebuffer object
    glGenFramebuffers(1, &this->MSFBO);
    glGenFramebuffers(1, &this->FBO);
    glGenRenderbuffers(1, &this->RBO);
    
    // Initialize renderbuffer storage with a multisampled color buffer (don't need a depth/stencil buffer)
    glBindFramebuffer(GL_FRAMEBUFFER, this->MSFBO);
    glBindRenderbuffer(GL_RENDERBUFFER, this->RBO);
//    glRenderbufferStorageMultisample(GL_RENDERBUFFER, 8, GL_RGB, width, height); // Allocate storage for render buffer object
    // 指定 4 个采样点
    glRenderbufferStorageMultisample(GL_RENDERBUFFER, 4, GL_RGB, width, height); // Allocate storage for render buffer object
    glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_RENDERBUFFER, this->RBO); // Attach MS render buffer object to framebuffer
    if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        std::cout << "ERROR::POSTPROCESSOR: Failed to initialize MSFBO" << std::endl;
        
    // Also initialize the FBO/texture to blit multisampled color-buffer to; used for shader operations (for postprocessing effects)
    glBindFramebuffer(GL_FRAMEBUFFER, this->FBO);
    this->Texture.Generate(width, height, NULL);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, this->Texture.ID, 0); // Attach texture to framebuffer as its color attachment
    if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        std::cout << "ERROR::POSTPROCESSOR: Failed to initialize FBO" << std::endl;
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    
    // Initialize render data and uniforms
    this->initRenderData();
    this->PostProcessingShader.Use();
    this->PostProcessingShader.SetInteger("scene", 0, GL_TRUE);
    GLfloat offset = 1.0f / 300.0f;
    GLfloat offsets[9][2] = {
        { -offset,  offset  },  // 左上
        {  0.0f,    offset  },  // 中上
        {  offset,  offset  },  // 右上
        { -offset,  0.0f    },  // 左中
        {  0.0f,    0.0f    },  // 正中
        {  offset,  0.0f    },  // 右中
        { -offset, -offset  },  // 左下
        {  0.0f,   -offset  },  // 中下
        {  offset, -offset  }   // 右下
    };
    glUniform2fv(glGetUniformLocation(this->PostProcessingShader.ID, "offsets"), 9, (GLfloat*)offsets);
    GLint edge_kernel[9] = {
        -1, -1, -1,
        -1,  8, -1,
        -1, -1, -1
    };
    glUniform1iv(glGetUniformLocation(this->PostProcessingShader.ID, "edge_kernel"), 9, edge_kernel);
    GLfloat blur_kernel[9] = {
        1.0 / 16, 2.0 / 16, 1.0 / 16,
        2.0 / 16, 4.0 / 16, 2.0 / 16,
        1.0 / 16, 2.0 / 16, 1.0 / 16
    };
    glUniform1fv(glGetUniformLocation(this->PostProcessingShader.ID, "blur_kernel"), 9, blur_kernel);
}

void PostProcessor::BeginRender()
{
    glBindFramebuffer(GL_FRAMEBUFFER, this->MSFBO);
    glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT);
}
void PostProcessor::EndRender()
{
    // Now resolve multisampled color-buffer into intermediate FBO to store to texture
    glBindFramebuffer(GL_READ_FRAMEBUFFER, this->MSFBO);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, this->FBO);
    // 位块传输
    glBlitFramebuffer(0, 0, this->Width, this->Height, 0, 0, this->Width, this->Height, GL_COLOR_BUFFER_BIT, GL_NEAREST);
    glBindFramebuffer(GL_FRAMEBUFFER, 0); // Binds both READ and WRITE framebuffer to default framebuffer
}

void PostProcessor::Render(GLfloat time)
{
    // Set uniforms/options
    this->PostProcessingShader.Use();
    this->PostProcessingShader.SetFloat("time", time);
    this->PostProcessingShader.SetInteger("confuse", this->Confuse);
    this->PostProcessingShader.SetInteger("chaos", this->Chaos);
    this->PostProcessingShader.SetInteger("shake", this->Shake);
    // Render textured quad
    glActiveTexture(GL_TEXTURE0);
    this->Texture.Bind();
    glBindVertexArray(this->VAO);
    glDrawArrays(GL_TRIANGLES, 0, 6);
    glBindVertexArray(0);
}

void PostProcessor::initRenderData()
{
    // Configure VAO/VBO
    GLuint VBO;
    GLfloat vertices[] = {
        // Pos        // Tex
        -1.0f, -1.0f, 0.0f, 0.0f,
         1.0f,  1.0f, 1.0f, 1.0f,
        -1.0f,  1.0f, 0.0f, 1.0f,

        -1.0f, -1.0f, 0.0f, 0.0f,
         1.0f, -1.0f, 1.0f, 0.0f,
         1.0f,  1.0f, 1.0f, 1.0f
    };
    glGenVertexArrays(1, &this->VAO);
    glGenBuffers(1, &VBO);

    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

    glBindVertexArray(this->VAO);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 4, GL_FLOAT, GL_FALSE, 4 * sizeof(GL_FLOAT), (GLvoid*)0);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);
}
