//
//  game.cpp
//  OpenGLEnv
//
//  Created by karos li on 2021/4/27.
//

/**
 需要强调的是这类碰撞检测和处理方式是不完美的。它只能计算每帧内可能发生的碰撞并且只能计算在该时间步时物体所在的各位置；这意味着如果一个物体拥有一个很大的速度以致于在一帧内穿过了另一个物体，它将看起来像是从来没有与另一个物体碰撞过。因此如果出现掉帧或出现了足够高的速度，这一碰撞检测方案将无法应对。

 （我们使用的碰撞方案）仍然会出现这几个问题：

 如果球运动得足够快，它可能在一帧内完整地穿过一个物体，而不会检测到碰撞。
 如果球在一帧内同时撞击了一个以上的物体，它将会检测到两次碰撞并两次反转速度；这样不改变它的原始速度。
 撞击到砖块的角时会在错误的方向反转速度，这是因为它在一帧内穿过的距离会引发VectorDirection返回水平方向还是垂直方向的差别。
 但是，本教程目的在于教会读者们图形学和游戏开发的基础知识。因此，这里的碰撞方案可以服务于此目的；它更容易理解且在正常的场景中可以较好地运作。需要记住的是存在有更好的（更复杂）碰撞方案，在几乎所有的场景中都可以很好地运作（包括可移动的物体）如分离轴定理(Separating Axis Theorem)。

 值得庆幸的是，有大量实用并且常常很高效的物理引擎（使用时间步无关的碰撞方案）可供您在游戏中使用。如果您希望在这一系统中有更深入的探索或需要更高级的物理系统又不理解其中的数学机理，Box2D是一个实现了物理系统和碰撞检测的可以用在您的应用程序中的完美的2D物理库
 http://box2d.org/about/
 */

#include "game.h"

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

//irrklang 不支持 MAC
//#include <irrklang/irrKlang.h>
//using namespace irrklang;

#include "resource_manager.h"
#include "sprite_renderer.h"
#include "game_object.h"
#include "ball_object.h"
#include "particle_generator.h"
#include "post_processor.h"
#include "text_renderer.h"

// Game-related State data - 四边形渲染对象（可以渲染正方形，长方形和球形）
SpriteRenderer  *Renderer;

/// 挡板
GameObject      *Player;

/// 球相关
// 初始化球的速度
const glm::vec2 INITIAL_BALL_VELOCITY(100.0f, -350.0f);
// 球的半径
const GLfloat BALL_RADIUS = 12.5f;
BallObject     *Ball;

// 粒子发射器
ParticleGenerator   *Particles;

// 后处理-特效
PostProcessor     *Effects;
GLfloat            ShakeTime = 0.0f;

// 音效
//ISoundEngine *SoundEngine = createIrrKlangDevice();

// 文本
TextRenderer  *Text;

Game::Game(unsigned int width, unsigned int height)
    : State(GAME_ACTIVE), Keys(), Width(width), Height(height), Lives(3)
{
    
}

Game::~Game()
{
    delete Renderer;
    delete Player;
    delete Ball;
    delete Particles;
    delete Effects;
}

void Game::Init()
{
    // 设置投影矩阵
    /**
     T left, T right, T bottom, T top, T zNear, T zFar
     前面的四个参数依次指定了投影平截头体的左、右、下、上边界。这个投影矩阵把所有在0到800之间的x坐标变换到-1到1之间，并把所有在0到600之间的y坐标变换到-1到1之间。这里我们指定了平截头体顶部的y坐标值为0，底部的y坐标值为600。所以，这个场景的左上角坐标为(0,0)，右下角坐标为(800,600)，就像屏幕坐标那样。观察空间坐标直接对应最终像素的坐标。
     https://learnopengl-cn.github.io/img/06/Breakout/03/projection.png
     这样我们就可以指定所有的顶点坐标为屏幕上的像素坐标了，这对2D游戏来说相当直观。
     */
    /// 左上角为原点
    glm::mat4 projection = glm::ortho(0.0f,
                                      static_cast<float>(this->Width),
                                      static_cast<float>(this->Height),
                                      0.0f,
                                      -1.0f, 1.0f);
    /// 左下角为原点
//    projection = glm::ortho(0.0f,
//                            static_cast<float>(this->Width),
//                            0.0f,
//                            static_cast<float>(this->Height),
//                            -1.0f, 1.0f);
//    projection = glm::ortho(static_cast<float>(this->Width * -0.5),
//                            static_cast<float>(this->Width * 0.5),
//                            static_cast<float>(this->Height * 0.5),
//                            static_cast<float>(this->Height * -0.5),
//                            -1.0f, 1.0f);
    
    
    // configure shaders
    // 加载着色器
    ResourceManager::LoadShader("sprite.vs", "sprite.fs", nullptr, "sprite");
    ResourceManager::LoadShader("particle.vs", "particle.fs", nullptr, "particle");
    ResourceManager::LoadShader("post_processing.vs", "post_processing.fs", nullptr, "postprocessing");
    
    // 配置着色器
    Shader spriteShader = ResourceManager::GetShader("sprite");
    spriteShader.Use();
    spriteShader.SetInteger("image", 0);
    spriteShader.SetMatrix4("projection", projection);
    
    Shader particleShader = ResourceManager::GetShader("particle");
    particleShader.Use();
    particleShader.SetInteger("image", 0);
    particleShader.SetMatrix4("projection", projection);
    
    
    /// 加载纹理
    ResourceManager::LoadTexture("awesomeface.png", GL_TRUE, "face");
    ResourceManager::LoadTexture("background.jpg", GL_FALSE, "background");
    ResourceManager::LoadTexture("block.png", GL_FALSE, "block");
    ResourceManager::LoadTexture("block_solid.png", GL_FALSE, "block_solid");
    ResourceManager::LoadTexture("paddle.png", GL_TRUE, "paddle");
    ResourceManager::LoadTexture("particle.png", GL_TRUE, "particle");
    // 道具纹理
    ResourceManager::LoadTexture("powerup_speed.png", true, "powerup_speed");
    ResourceManager::LoadTexture("powerup_sticky.png", true, "powerup_sticky");
    ResourceManager::LoadTexture("powerup_increase.png", true, "powerup_increase");
    ResourceManager::LoadTexture("powerup_confuse.png", true, "powerup_confuse");
    ResourceManager::LoadTexture("powerup_chaos.png", true, "powerup_chaos");
    ResourceManager::LoadTexture("powerup_passthrough.png", true, "powerup_passthrough");
    
    // 创建精灵渲染对象
    Renderer = new SpriteRenderer(spriteShader);
    // 创建粒子发射器渲染对象
    Particles = new ParticleGenerator(
        ResourceManager::GetShader("particle"),
        ResourceManager::GetTexture("particle"),
        500
    );
    // 创建特效处理渲染对象
    Effects = new PostProcessor(ResourceManager::GetShader("postprocessing"), this->Width, this->Height);
    // 创建文本渲染对象
    Text = new TextRenderer(this->Width, this->Height);
        Text->Load("OCRAEXT.TTF", 24);
    
    // 加载关卡
    GameLevel one; one.Load("one.lvl", this->Width, this->Height / 2);
    GameLevel two; two.Load("two.lvl", this->Width, this->Height / 2);
    GameLevel three; three.Load("three.lvl", this->Width, this->Height / 2);
    GameLevel four; four.Load("four.lvl", this->Width, this->Height / 2);
    this->Levels.push_back(one);
    this->Levels.push_back(two);
    this->Levels.push_back(three);
    this->Levels.push_back(four);
    this->Level = 0;
    
    // 配置挡板
    glm::vec2 playerPos = glm::vec2(this->Width / 2.0f - PLAYER_SIZE.x / 2.0f, this->Height - PLAYER_SIZE.y);
    Player = new GameObject(playerPos, PLAYER_SIZE, ResourceManager::GetTexture("paddle"));
    
    // 配置球
    glm::vec2 ballPos = playerPos + glm::vec2(PLAYER_SIZE.x / 2 - BALL_RADIUS, -BALL_RADIUS * 2);
    Ball = new BallObject(ballPos, BALL_RADIUS, INITIAL_BALL_VELOCITY,
            ResourceManager::GetTexture("face"));
    
    // 播放音效
//    SoundEngine->play2D("breakout.mp3", GL_TRUE);
    
//    Effects->Shake = GL_TRUE;
//    Effects->Confuse = GL_TRUE;
//    Effects->Chaos = GL_TRUE;
}

void Game::ProcessInput(float dt)
{
    if (this->State == GAME_ACTIVE)// 游戏中
    {
        GLfloat velocity = PLAYER_VELOCITY * dt;
        /**
         移动挡板
         当挡板的x值小于0，它将移动出游戏场景的最左侧，所以我们只允许挡板的x值大于0时向左移动。对于右侧边缘我们做相同的处理，但我们必须比较场景的右侧边缘与挡板的右侧边缘，即场景宽度减去挡板宽度
         */
        if (this->Keys[GLFW_KEY_A])// 按了 A，表示左移
        {
            if (Player->Position.x >= 0) {
                // 左移移动挡板
                Player->Position.x -= velocity;
                if (Ball->Stuck)// 球固定在挡板上时，球要跟挡板一起移动
                    Ball->Position.x -= velocity;
            }
        }
        if (this->Keys[GLFW_KEY_D])// 按了 D，表示右移
        {
            if (Player->Position.x <= this->Width - Player->Size.x) {
                // 右移移动挡板
                Player->Position.x += velocity;
                if (Ball->Stuck)// 球固定在挡板上时，球要跟挡板一起移动
                    Ball->Position.x += velocity;
            }
        }
        if (this->Keys[GLFW_KEY_SPACE])// 按下空格表示游戏开始
            Ball->Stuck = false;
    }
    
    if (this->State == GAME_MENU)// 菜单
    {
        if (this->Keys[GLFW_KEY_ENTER] && !this->KeysProcessed[GLFW_KEY_ENTER])
        {
            this->State = GAME_ACTIVE;
            this->KeysProcessed[GLFW_KEY_ENTER] = GL_TRUE;
        }
        if (this->Keys[GLFW_KEY_W] && !this->KeysProcessed[GLFW_KEY_W])
        {
            this->Level = (this->Level + 1) % 4;
            this->KeysProcessed[GLFW_KEY_W] = GL_TRUE;
        }
        if (this->Keys[GLFW_KEY_S] && !this->KeysProcessed[GLFW_KEY_S])
        {
            if (this->Level > 0)
                --this->Level;
            else
                this->Level = 3;
            this->KeysProcessed[GLFW_KEY_S] = GL_TRUE;
        }
    }
    
    if (this->State == GAME_WIN)// 游戏胜利
    {
        if (this->Keys[GLFW_KEY_ENTER])
        {
            this->KeysProcessed[GLFW_KEY_ENTER] = GL_TRUE;
            Effects->Chaos = GL_FALSE;
            this->State = GAME_MENU;
        }
    }
}

void Game::Update(float dt)
{
    // 更新球位置
    Ball->Move(dt, this->Width);
    
    // 检测碰撞
    this->DoCollisions();
    
    // 更新粒子，生成2个新粒子，新复用的粒子的偏移量为球半径的一半
    Particles->Update(dt, *Ball, 2, glm::vec2(Ball->Radius / 2));
    
    // 更新道具位置和持续时间
    this->UpdatePowerUps(dt);
    
    // 减少抖动时间
    if (ShakeTime > 0.0f)
    {
        ShakeTime -= dt;
        if (ShakeTime <= 0.0f)
            Effects->Shake = GL_FALSE;
    }
    
    // 游戏结束检测
    if (Ball->Position.y >= this->Height) // 球是否接触底部边界？
    {
        --this->Lives;
        // 玩家是否已失去所有生命值? : 游戏结束
        if (this->Lives == 0)
        {
            this->ResetLevel();
            this->State = GAME_MENU;
        }
        this->ResetPlayer();
    }
    
    // 游戏完成
    if (this->State == GAME_ACTIVE && this->Levels[this->Level].IsCompleted())
    {
        this->ResetLevel();
        this->ResetPlayer();
        Effects->Chaos = GL_TRUE;
        this->State = GAME_WIN;
    }
}

void Game::Render()
{
//    Texture2D texture = ResourceManager::GetTexture("face");
//    Renderer->DrawSprite(texture, glm::vec2(200, 200), glm::vec2(300, 400), 45.0f, glm::vec3(0.0f, 1.0f, 0.0f));
//    Renderer->DrawSprite(texture, glm::vec2(0, 0), glm::vec2(300, 400), 0.0f, glm::vec3(0.0f, 1.0f, 0.0f));
    
    
    if(this->State == GAME_ACTIVE || this->State == GAME_MENU || this->State == GAME_WIN)// 底部游戏渲染
    {
        // Begin rendering to postprocessing quad
        Effects->BeginRender();
        
            // 绘制背景
            Texture2D texture = ResourceManager::GetTexture("background");
            Renderer->DrawSprite(texture,
                glm::vec2(0, 0), glm::vec2(this->Width, this->Height), 0.0f
            );
            // 绘制关卡
            this->Levels[this->Level].Draw(*Renderer);
            // 绘制挡板
            Player->Draw(*Renderer);
            // 绘制道具
            for (PowerUp &powerUp : this->PowerUps)
                if (!powerUp.Destroyed)
                    powerUp.Draw(*Renderer);
            // 绘制粒子
            Particles->Draw();
            // 绘制球
            Ball->Draw(*Renderer);
        
        // End rendering to postprocessing quad
        Effects->EndRender();
        // Render postprocessing quad
        Effects->Render(glfwGetTime());
        
        /// 文本绘制
        std::stringstream ss; ss << this->Lives;
        Text->RenderText("Lives:" + ss.str(), 5.0f, 5.0f, 1.0f);
    }
    
    if (this->State == GAME_MENU)// 菜单
    {
        Text->RenderText("Press ENTER to start", 250.0f, Height / 2, 1.0f);
        Text->RenderText("Press W or S to select level", 245.0f, Height / 2 + 20.0f, 0.75f);
    }
    
    if (this->State == GAME_WIN)// 游戏胜利
    {
        Text->RenderText(
            "You WON!!!", 320.0, Height / 2 - 20.0, 1.0, glm::vec3(0.0, 1.0, 0.0)
        );
        Text->RenderText(
            "Press ENTER to retry or ESC to quit", 130.0, Height / 2, 1.0, glm::vec3(1.0, 1.0, 0.0)
        );
    }
}

// collision detection
bool CheckCollision(GameObject &one, GameObject &two);// AABB 碰撞检测
Collision CheckCollision(BallObject &one, GameObject &two); // AABB - Circle collision - AABB 与圆碰撞检测
Direction VectorDirection(glm::vec2 closest);
void ActivatePowerUp(PowerUp &powerUp);

// 碰撞检测
void Game::DoCollisions()
{
    // 盒子和球
    for (GameObject &box : this->Levels[this->Level].Bricks)
    {
        if (!box.Destroyed)
        {
            Collision collision = CheckCollision(*Ball, box);
            if (std::get<0>(collision)) // 如果发生了碰撞
            {
                // 如果砖块不是实心就销毁砖块
                if (!box.IsSolid) {
                    box.Destroyed = GL_TRUE;
                
                    // 生成道具
                    this->SpawnPowerUps(box);
                }
                else
                {
                    // 如果是实心的砖块则激活shake特效
                    ShakeTime = 0.05f;
                    Effects->Shake = true;
                }
                // 碰撞处理
                Direction dir = std::get<1>(collision);
                glm::vec2 diff_vector = std::get<2>(collision);
                if (Ball->PassThrough && box.IsSolid)// 如果球可以穿过实心砖块，则不需要处理碰撞
                {
                    continue;
                }
                
                if (dir == LEFT || dir == RIGHT) // 水平方向碰撞
                {
                    Ball->Velocity.x = -Ball->Velocity.x; // 反转水平速度
                    // 重定位; 因为球和方块相撞肯定会有重叠距离，所以需要复位到碰撞之前
                    GLfloat penetration = Ball->Radius - std::abs(diff_vector.x);
                    if (dir == LEFT)
                        Ball->Position.x += penetration; // 将球右移
                    else
                        Ball->Position.x -= penetration; // 将球左移
                }
                else // 垂直方向碰撞
                {
                    Ball->Velocity.y = -Ball->Velocity.y; // 反转垂直速度
                    // 重定位; 因为球和方块相撞肯定会有重叠距离，所以需要复位到碰撞之前
                    GLfloat penetration = Ball->Radius - std::abs(diff_vector.y);
                    if (dir == UP)
                        Ball->Position.y += penetration; // 将球下移
                    else
                        Ball->Position.y -= penetration; // 将球上移
                        
                }
            }
        }
    }
    
    // 球和挡板
    Collision result = CheckCollision(*Ball, *Player);
    if (!Ball->Stuck && std::get<0>(result))
    {
        // 检查碰到了挡板的哪个位置，并根据碰到哪个位置来改变速度
        GLfloat centerBoard = Player->Position.x + Player->Size.x / 2;
        GLfloat centerBall = (Ball->Position.x + Ball->Radius);
        GLfloat distance = centerBall - centerBoard;
        // 基于撞击挡板的点与（挡板）中心的距离来改变球的水平速度。撞击点距离挡板的中心点越远，则水平方向的速度就会越大。
        GLfloat percentage = distance / (Player->Size.x / 2);
        // 依据结果移动
        GLfloat strength = 2.0f;
        glm::vec2 oldVelocity = Ball->Velocity;
        
        /**
         注意旧的速度被存储为oldVelocity。之所以要存储旧的速度是因为我们只更新球的速度矢量中水平方向的速度并保持它的y速度不变。这将意味着矢量的长度会持续变化，其产生的影响是如果球撞击到挡板的边缘则会比撞击到挡板中心有更大(也因此更强)的速度矢量。为此新的速度矢量会正交化然后乘以旧速度矢量的长度。这样一来，球的力量和速度将总是一一致的，无论它撞击到挡板的哪个地方。
         */
        Ball->Velocity.x = INITIAL_BALL_VELOCITY.x * percentage * strength;
        //Ball->Velocity.y = -Ball->Velocity.y; 因为球可能碰撞入板里，那么球可能在接下来几帧，持续翻转垂直方向的速度，所以这里需要对速度的绝对值取反，来确保速度一直是向上的
        Ball->Velocity.y = -1 * abs(Ball->Velocity.y);
        Ball->Velocity = glm::normalize(Ball->Velocity) * glm::length(oldVelocity);
        
        // Sticky 道具：实现球黏在挡板上的效果；若Stikcy效果被激活，那么小球则会在与挡板接触时粘在上面，玩家不得不再次按下空格键才能释放它。
        Ball->Stuck = Ball->Sticky;
    }
    
    // 挡板和道具 - 对所有未被销毁的道具，我们检查它是否接触到了屏幕底部或玩家挡板，无论哪种情况我们都销毁它，但当道具与玩家挡板接触时，激活这个道具。
    for (PowerUp &powerUp : this->PowerUps)
    {
        if (!powerUp.Destroyed)
        {
            if (powerUp.Position.y >= this->Height)
                powerUp.Destroyed = GL_TRUE;
            
            if (CheckCollision(*Player, powerUp))
            {   // 道具与挡板接触，激活它！
                ActivatePowerUp(powerUp);
                powerUp.Destroyed = GL_TRUE;
                powerUp.Activated = GL_TRUE;
            }
        }
    }
}

/**
 AABB 碰撞
 AABB代表的是轴对齐碰撞箱(Axis-aligned Bounding Box)，碰撞箱是指与场景基础坐标轴（2D中的是x和y轴）对齐的长方形的碰撞外形。与坐标轴对齐意味着这个长方形没有经过旋转并且它的边线和场景中基础坐标轴平行（例如，左右边线和y轴平行）。
 
 其中一种定义AABB的方式是获取左上角点和右下角点的位置。我们定义的GameObject类已经包含了一个左上角点位置（它的Position vector）并且我们可以通过把左上角点的矢量加上它的尺寸（Position + Size）很容易地计算出右下角点。
 那么我们如何判断碰撞呢？当两个碰撞外形进入对方的区域时就会发生碰撞，例如定义了第一个物体的碰撞外形以某种形式进入了第二个物体的碰撞外形。对于AABB来说很容易判断，因为它们是与坐标轴对齐的：对于每个轴我们要检测两个物体的边界在此轴向是否有重叠。因此我们只是简单地检查两个物体的水平边界是否重合以及垂直边界是否重合。如果水平边界和垂直边界都有重叠那么我们就检测到一次碰撞。
 
 https://learnopengl-cn.github.io/img/06/Breakout/05/02/collisions_overlap.png
 */
bool CheckCollision(GameObject &one, GameObject &two) // AABB - AABB collision
{
    /**
     我们检查第一个物体的最右侧是否大于第二个物体的最左侧并且第二个物体的最右侧是否大于第一个物体的最左侧；垂直的轴向与此相似。
     */
    // x轴方向碰撞？
    bool collisionX = one.Position.x + one.Size.x >= two.Position.x &&
        two.Position.x + two.Size.x >= one.Position.x;
    // y轴方向碰撞？
    bool collisionY = one.Position.y + one.Size.y >= two.Position.y &&
        two.Position.y + two.Size.y >= one.Position.y;
    // 只有两个轴向都有碰撞时才碰撞
    return collisionX && collisionY;
}

/**
 优化：AABB与圆碰撞检测
 
 因为球本身是圆形的，而碰撞的代码中将球视为长方形框，因此常常会出现球碰撞了砖块但此时球精灵还没有接触到砖块。
 
 使用圆形碰撞外形而不是AABB来代表球会更合乎常理。因此我们在球对象中包含了Radius变量，为了定义圆形碰撞外形，我们需要的是一个位置矢量和一个半径。
 https://learnopengl-cn.github.io/img/06/Breakout/05/02/collisions_circle.png
 
 这意味着我们不得不修改检测算法，因为当前的算法只适用于两个AABB的碰撞。检测圆和AABB碰撞的算法会稍稍复杂，关键点如下：我们会找到AABB上距离圆最近的一个点，如果圆到这一点的距离小于它的半径，那么就产生了碰撞。
 
 难点在于获取AABB上的最近点P¯。下图展示了对于任意的AABB和圆我们如何计算该点：
 这里 向量D的起始点应该是 B，终点是 C
 https://learnopengl-cn.github.io/img/06/Breakout/05/02/collisions_aabb_circle.png
*/
Collision CheckCollision(BallObject &one, GameObject &two) // AABB - Circle collision
{
    // 获取圆的中心
    glm::vec2 center(one.Position + one.Radius);
    // 计算AABB的信息（中心、半边长）
    glm::vec2 aabb_half_extents(two.Size.x / 2, two.Size.y / 2);
    glm::vec2 aabb_center(
        two.Position.x + aabb_half_extents.x,
        two.Position.y + aabb_half_extents.y
    );
    // 获取两个中心的差矢量
    glm::vec2 difference = center - aabb_center;
    // 限制运算把一个值限制在给定范围内 clamp(float value, float min, float max)
    glm::vec2 clamped = glm::clamp(difference, -aabb_half_extents, aabb_half_extents);
    // AABB_center加上clamped这样就得到了碰撞箱上距离圆最近的点closest
    glm::vec2 closest = aabb_center + clamped;
    // 获得圆心center和最近点closest的矢量（起点是 C(center)，终点是 P(最近点closest)），
    difference = closest - center;
    // 并判断是否 length <= radius
    if (glm::length(difference) <= one.Radius)// 说明有碰撞
        // 是否有碰撞，球碰撞点发生的方向，碰撞的距离
        return std::make_tuple(GL_TRUE, VectorDirection(difference), difference);
    else
        return std::make_tuple(GL_FALSE, UP, glm::vec2(0, 0));
}

/**
 点乘可以得到两个正交化的矢量的夹角。如果我们定义指向北、南、西和东的四个矢量，然后计算它们和给定矢量的夹角会怎么样？由这四个方向矢量和给定的矢量点乘积的结果中的最高值（点乘积的最大值为1.0f，代表0度角）即是矢量的方向。
 */
Direction VectorDirection(glm::vec2 target)
{
    /**
     表示的是碰撞发生在球的哪个方向
     target 是碰撞点向量(从圆心出发)， 可以想象成把碰撞点向量平移到左上角原点，然后在左上角原点上定义4个方向，然后根据下面计算的点乘结果判断哪个碰撞点向量离哪个方向更近。
     max 越大说明夹角越小，说明圆心到最近点的方向向量越靠近哪个方向。
     */
    glm::vec2 compass[] = {
        glm::vec2(0.0f, -1.0f),  // 方向上
        glm::vec2(1.0f, 0.0f),  // 方向右
        glm::vec2(0.0f, 1.0f), // 方向下
        glm::vec2(-1.0f, 0.0f)  // 方向左
    };
    GLfloat max = 0.0f;
    GLuint best_match = -1;
    for (GLuint i = 0; i < 4; i++)
    {
        GLfloat dot_product = glm::dot(glm::normalize(target), compass[i]);
        if (dot_product > max)
        {
            max = dot_product;
            best_match = i;
        }
    }
    return (Direction)best_match;
}

/// 道具处理

/**
 是否存在同类型已经生效的道具
 
 当一个道具在激活状态时，另一个道具与挡板发生了接触。在这种情况下我们有超过1个在当前PowerUps容器中处于激活状态的道具。然后，当这些道具中的一个被停用时，我们不应使其效果失效因为另一个相同类型的道具仍处于激活状态。出于这个原因，我们使用isOtherPowerUpActive检查是否有同类道具处于激活状态。只有当它返回false时，我们才停用这个道具的效果。这样，给定类型的道具的持续时间就可以延长至最近一次被激活后的持续时间。
 */
bool IsOtherPowerUpActive(std::vector<PowerUp> &powerUps, std::string type)
{
    for (const PowerUp &powerUp : powerUps)
    {
        if (powerUp.Activated)
            if (powerUp.Type == type)
                return true;
    }
    return false;
}

// 生成道具的概率
GLboolean ShouldSpawn(GLuint chance)
{
    GLuint random = rand() % chance;
    return random == 0;
}
/**
 生成道具
 
 这样的SpawnPowerUps函数以一定几率（1/75普通道具，1/15负面道具）生成一个新的PowerUp对象，并设置其属性。每种道具有特殊的颜色使它们更具有辨识度，同时根据类型决定其持续时间的秒数，若值为0.0f则表示它持续无限长的时间
 */
void Game::SpawnPowerUps(GameObject &block)
{
    if (ShouldSpawn(75)) // 1/75的几率
        this->PowerUps.push_back(
             PowerUp("speed", glm::vec3(0.5f, 0.5f, 1.0f), 0.0f, block.Position, ResourceManager::GetTexture("powerup_speed")
         ));
    if (ShouldSpawn(75))
        this->PowerUps.push_back(
            PowerUp("sticky", glm::vec3(1.0f, 0.5f, 1.0f), 20.0f, block.Position, ResourceManager::GetTexture("powerup_sticky")
        ));
    if (ShouldSpawn(75))
        this->PowerUps.push_back(
            PowerUp("pass-through", glm::vec3(0.5f, 1.0f, 0.5f), 10.0f, block.Position, ResourceManager::GetTexture("powerup_passthrough")
        ));
    if (ShouldSpawn(75))
        this->PowerUps.push_back(
            PowerUp("pad-size-increase", glm::vec3(1.0f, 0.6f, 0.4), 0.0f, block.Position, ResourceManager::GetTexture("powerup_increase")
        ));
    if (ShouldSpawn(15)) // 负面道具被更频繁地生成
        this->PowerUps.push_back(
            PowerUp("confuse", glm::vec3(1.0f, 0.3f, 0.3f), 15.0f, block.Position, ResourceManager::GetTexture("powerup_confuse")
        ));
    if (ShouldSpawn(15))
        this->PowerUps.push_back(
            PowerUp("chaos", glm::vec3(0.9f, 0.25f, 0.25f), 15.0f, block.Position, ResourceManager::GetTexture("powerup_chaos")
        ));
}
                   
 /**
  激活道具
  */
 void ActivatePowerUp(PowerUp &powerUp)
 {
     // 根据道具类型发动道具
     if (powerUp.Type == "speed")
     {
         Ball->Velocity *= 1.2;
     }
     else if (powerUp.Type == "sticky")
     {
         Ball->Sticky = GL_TRUE;
         Player->Color = glm::vec3(1.0f, 0.5f, 1.0f);
     }
     else if (powerUp.Type == "pass-through")
     {
         Ball->PassThrough = GL_TRUE;
         Ball->Color = glm::vec3(1.0f, 0.5f, 0.5f);
     }
     else if (powerUp.Type == "pad-size-increase")
     {
         Player->Size.x += 50;
     }
     else if (powerUp.Type == "confuse")
     {
         if (!Effects->Chaos)
             Effects->Confuse = GL_TRUE; // 只在chaos未激活时生效，chaos同理
     }
     else if (powerUp.Type == "chaos")
     {
         if (!Effects->Confuse)
             Effects->Chaos = GL_TRUE;
     }
 }
             
 /**
  保证道具生成后可以移动，并且在它们的持续时间用尽后失效，否则道具将永远保持激活状态。

  在游戏的UpdatePowerUps函数中，我们根据道具的速度移动它，并减少已激活道具的持续时间，每当时间减少至小于0时，我们令其失效，并恢复相关变量的状态
  */
 void Game::UpdatePowerUps(GLfloat dt)
 {
     for (PowerUp &powerUp : this->PowerUps)
     {
         powerUp.Position += powerUp.Velocity * dt;
         if (powerUp.Activated)
         {
             powerUp.Duration -= dt;

             if (powerUp.Duration <= 0.0f)
             {
                 // 之后会将这个道具移除
                 powerUp.Activated = GL_FALSE;
                 // 停用效果
                 if (powerUp.Type == "sticky")
                 {
                     if (!IsOtherPowerUpActive(this->PowerUps, "sticky"))
                     {   // 仅当没有其他sticky效果处于激活状态时重置，以下同理
                         Ball->Sticky = GL_FALSE;
                         Player->Color = glm::vec3(1.0f);
                     }
                 }
                 else if (powerUp.Type == "pass-through")
                 {
                     if (!IsOtherPowerUpActive(this->PowerUps, "pass-through"))
                     {
                         Ball->PassThrough = GL_FALSE;
                         Ball->Color = glm::vec3(1.0f);
                     }
                 }
                 else if (powerUp.Type == "confuse")
                 {
                     if (!IsOtherPowerUpActive(this->PowerUps, "confuse"))
                     {
                         Effects->Confuse = GL_FALSE;
                     }
                 }
                 else if (powerUp.Type == "chaos")
                 {
                     if (!IsOtherPowerUpActive(this->PowerUps, "chaos"))
                     {
                         Effects->Chaos = GL_FALSE;
                     }
                 }
             }
         }
     }
    /**
     我们通过将相关元素重置来停用它。我们还将PowerUp的Activated属性设为false，在UpdatePowerUps结束时，我们通过循环PowerUps容器，若一个道具被销毁切被停用，则移除它。我们在算法开头使用remove_if函数，通过给定的lamda表达式消除这些对象。
     
     remove_if函数将lamda表达式为true的元素移动至容器的末尾并返回一个迭代器指向应被移除的元素范围的开始部分。容器的erase函数接着擦除这个迭代器指向的元素与容器末尾元素之间的所有元素。
     */
     this->PowerUps.erase(std::remove_if(this->PowerUps.begin(), this->PowerUps.end(),
         [](const PowerUp &powerUp) { return powerUp.Destroyed && !powerUp.Activated; }
     ), this->PowerUps.end());
 }

void Game::ResetLevel()
{
    if (this->Level == 0)this->Levels[0].Load("one.lvl", this->Width, this->Height * 0.5f);
    else if (this->Level == 1)
        this->Levels[1].Load("two.lvl", this->Width, this->Height * 0.5f);
    else if (this->Level == 2)
        this->Levels[2].Load("three.lvl", this->Width, this->Height * 0.5f);
    else if (this->Level == 3)
        this->Levels[3].Load("four.lvl", this->Width, this->Height * 0.5f);
    
    this->Lives = 3;
}

void Game::ResetPlayer()
{
    // Reset player/ball stats
    Player->Size = PLAYER_SIZE;
    Player->Position = glm::vec2(this->Width / 2 - PLAYER_SIZE.x / 2, this->Height - PLAYER_SIZE.y);
    Ball->Reset(Player->Position + glm::vec2(PLAYER_SIZE.x / 2 - BALL_RADIUS, -(BALL_RADIUS * 2)), INITIAL_BALL_VELOCITY);
}
