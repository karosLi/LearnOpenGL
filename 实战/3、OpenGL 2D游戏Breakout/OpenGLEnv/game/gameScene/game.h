//
//  game.hpp
//  OpenGLEnv
//
//  Created by karos li on 2021/4/27.
//

/**
 优化
 这些教程的内容和目前已完成的游戏代码的关注点都在于如何尽可能简单地阐述概念，而没有深入地优化细节。因此，很多性能相关的考虑都被忽略了。为了在游戏的帧率开始下降时可以提高性能，我们将列出一些现代的2D OpenGL游戏中常见的改进方案。

 渲染精灵表单/纹理图谱(Sprite sheet / Texture atlas)：代替使用单个渲染精灵渲染单个纹理的渲染方式，我们将所有需要用到的纹理组合到单个大纹理中（如同位图字体），并用纹理坐标来选择合适的精灵与纹理。切换纹理状态是非常昂贵的操作，而使用这种方法让我们几乎可以不用在纹理间进行切换。除此之外，这样做还可以让GPU更有效率地缓存纹理，获得更快的查找速度。（译注：cache的局部性原理）
 实例化渲染：代替一次只渲染一个四边形的渲染方式，我们可以将想要渲染的所有四边形批量化，并使用实例化渲染在一次<>draw call中成批地渲染四边形。这很容易实现，因为每个精灵都由相同的顶点组成，不同之处只有一个模型矩阵(Model Matrix)，我们可以很容易地将其包含在一个实例化数组中。这样可以使OpenGL每帧渲染更多的精灵。实例化渲染也可以用来渲染粒子和字符字形。
 三角形带(Triangle Strips)：代替每次渲染两个三角形的渲染方式，我们可以用OpenGL的TRIANGLE_STRIP渲染图元渲染它们，只需4个顶点而非6个。这节约了三分之一需要传递给GPU的数据量。
 空间划分(Space partition)算法：当检查可能发生的碰撞时，我们将小球与当前关卡中的每一个砖块进行比较，这么做有些浪费CPU资源，因为我们可以很容易得知在这一帧中，大多数砖块都不会与小球很接近。使用BSP，八叉树(Octress)或k-d(imension)树等空间划分算法，我们可以将可见的空间划分成许多较小的区域，并判断小球是否在这个区域中，从而为我们省去大量的碰撞检查。对于Breakout这样的简单游戏来说，这可能是过度的，但对于有着更复杂的碰撞检测算法的复杂游戏，这些算法可以显著地提高性能。
 最小化状态间的转换：状态间的变化（如绑定纹理或切换着色器）在OpenGL中非常昂贵，因此你需要避免大量的状态变化。一种最小化状态间变化的方法是创建自己的状态管理器来存储OpenGL状态的当前值（比如绑定了哪个纹理），并且只在需要改变时进行切换，这可以避免不必要的状态变化。另外一种方式是基于状态切换对所有需要渲染的物体进行排序。首先渲染使用着色器A的所有对象，然后渲染使用着色器B的所有对象，以此类推。当然这可以扩展到着色器、纹理绑定、帧缓冲切换等。
 这些应该可以给你一些关于，我们可以用什么样的的高级技巧进一步提高2D游戏性能地提示。这也让你感受到了OpenGL的强大功能。通过亲手完成大部分的渲染，我们对整个渲染过程有了完整的掌握，从而可以实现对过程的优化。如果你对Breakout的性能并不满意，你可以把这些当做练习。
 */

#ifndef GAME_H
#define GAME_H

#include <tuple>
#include <glad/glad.h>
#include <GLFW/glfw3.h>

#include "game_level.h"
#include "power_up.h"

// 代表了游戏的当前状态
enum GameState {
    GAME_ACTIVE,
    GAME_MENU,
    GAME_WIN
};

// Represents the four possible (collision) directions - 碰撞方向
enum Direction {
    UP,
    RIGHT,
    DOWN,
    LEFT
};
// Defines a Collision typedef that represents collision data - 定义元组
typedef std::tuple<bool, Direction, glm::vec2> Collision; // <collision?, what direction?, difference vector center - closest point>

// Initial size of the player paddle - 初始化挡板的大小
const glm::vec2 PLAYER_SIZE(100.0f, 20.0f);
// Initial velocity of the player paddle - 初始化挡板的速率
const float PLAYER_VELOCITY(500.0f);

class Game
{
    public:
        // 游戏状态
        GameState  State;
        GLboolean  Keys[1024];// 外部输入的按键数组，按下就是 true，释放就是 false
        GLboolean  KeysProcessed[1024];// 外部输入的按键数组，记录按键是否又被处理
        GLuint     Width, Height;// 游戏窗口宽高
        GLuint     Lives;// 玩家生命值
        std::vector<GameLevel>  Levels;// 关卡数组
        unsigned int            Level;// 当前关卡
        std::vector<PowerUp>  PowerUps;// 道具
        // 构造函数/析构函数
        Game(GLuint width, GLuint height);
        ~Game();
        // 初始化游戏状态（加载所有的着色器/纹理/关卡）
        void Init();
        // 根据按键输入更新位移
        void ProcessInput(GLfloat dt);
        // 根据时间更新位置
        void Update(GLfloat dt);
        // 渲染画面
        void Render();
        // 碰撞检测
        void DoCollisions();
        // 生成道具
        void SpawnPowerUps(GameObject &block);
        // 更新所有激活的道具
        void UpdatePowerUps(GLfloat dt);
        // 重置游戏
        void ResetLevel();
        void ResetPlayer();
};

#endif
