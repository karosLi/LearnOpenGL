//
//  main.cpp
//  OpenGLEnv
//  OpenGL
//  Created by karos li on 2021/3/17.

#include <glad/glad.h>
#include <GLFW/glfw3.h>

#include "game.h"
#include "resource_manager.h"

// GLFW function declerations
void key_callback(GLFWwindow* window, int key, int scancode, int action, int mode);

// The Width of the screen
const GLuint SCREEN_WIDTH = 800;
// The height of the screen
const GLuint SCREEN_HEIGHT = 600;

Game Breakout(SCREEN_WIDTH, SCREEN_HEIGHT);

int main(int argc, char *argv[])
{
    // glfw: initialize and configure
    // ------------------------------
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    glfwWindowHint(GLFW_RESIZABLE, GL_FALSE);

#ifdef __APPLE__
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif
    
    // glfw window creation
    // --------------------
    GLFWwindow* window = glfwCreateWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Breakout", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    glfwSetKeyCallback(window, key_callback);

    // tell GLFW to capture our mouse
    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

    // glad: load all OpenGL function pointers
    // ---------------------------------------
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }

    // OpenGL configuration
    glViewport(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    glEnable(GL_CULL_FACE);
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

    // Initialize game
    Breakout.Init();

    // DeltaTime variables
    GLfloat deltaTime = 0.0f; // deltaTime 帧间隔
    GLfloat lastFrame = 0.0f;

    // Start Game within Menu State
    Breakout.State = GAME_ACTIVE;

//    while (!glfwWindowShouldClose(window))
//    {
//        // Calculate delta time
//        GLfloat currentFrame = glfwGetTime();
//        deltaTime = currentFrame - lastFrame;
//        lastFrame = currentFrame;
//        glfwPollEvents();
//
//        //deltaTime = 0.001f;
//        // Manage user input - 管理用户点击按键
//        Breakout.ProcessInput(deltaTime);
//
//        // Update Game state
//        Breakout.Update(deltaTime);
//
//        // Render
//        glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
//        glClear(GL_COLOR_BUFFER_BIT);
//        Breakout.Render();
//
//        glfwSwapBuffers(window);
//    }

    while (!glfwWindowShouldClose(window))
    {
        // Calculate delta time - per-frame time logic 计算每一帧的绘制时间
        GLfloat currentFrame = glfwGetTime();
        deltaTime = currentFrame - lastFrame;
        
        if (deltaTime >= 1.0/60.0) // 限制在 1秒钟 60 帧
        {
            glfwPollEvents();

            //deltaTime = 0.001f;
            // Manage user input - 管理用户点击按键
            Breakout.ProcessInput(deltaTime);

            // Update Game state
            Breakout.Update(deltaTime);

            // Render
            glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            glClear(GL_COLOR_BUFFER_BIT);
            Breakout.Render();

            glfwSwapBuffers(window);
            
            lastFrame = currentFrame;
        }
    }

    // Delete all resources as loaded using the resource manager
    ResourceManager::Clear();

    glfwTerminate();
    return 0;
}

void key_callback(GLFWwindow* window, int key, int scancode, int action, int mode)
{
    // When a user presses the escape key, we set the WindowShouldClose property to true, closing the application
    if (key == GLFW_KEY_ESCAPE && action == GLFW_PRESS)// ESC 杀掉进程
        glfwSetWindowShouldClose(window, GL_TRUE);
    if (key >= 0 && key < 1024)
    {
        if (action == GLFW_PRESS)// 按下某个按键
            Breakout.Keys[key] = GL_TRUE;
        else if (action == GLFW_RELEASE)// 释放某个按键
            Breakout.Keys[key] = GL_FALSE;
            Breakout.KeysProcessed[key] = GL_FALSE;
    }
}

