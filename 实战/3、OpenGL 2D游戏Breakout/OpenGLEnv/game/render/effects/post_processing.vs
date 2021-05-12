#version 330 core
/**
 基于uniform是否被设置为true，顶点着色器可以执行不同的分支。如果chaos或confuse被设置为true，顶点着色器将操纵纹理坐标来移动场景（以圆形动画变换纹理坐标或反转纹理坐标）。因为我们将纹理环绕方式设置为了GL_REPEAT，所以chaos特效会导致场景在四边形的各个部分重复。除此之外，如果shake被设置为true，它将微量移动顶点位置。需要注意的是，chaos与confuse不应同时为true，而shake则可以与其他特效一起生效。

 当任意特效被激活时，除了偏移顶点的位置和纹理坐标，我们也希望创造显著的视觉效果。
 */

layout (location = 0) in vec4 vertex; // <vec2 position, vec2 texCoords>

out vec2 TexCoords;

uniform bool  chaos;
uniform bool  confuse;
uniform bool  shake;
uniform float time;

void main()
{
    gl_Position = vec4(vertex.xy, 0.0f, 1.0f);
    vec2 texture = vertex.zw;
    if(chaos)
    {
        // 圆形动画变换纹理坐标
        float strength = 0.3;
        vec2 pos = vec2(texture.x + sin(time) * strength, texture.y + cos(time) * strength);
        TexCoords = pos;
    }
    else if(confuse)
    {
        // 反转纹理坐标
        TexCoords = vec2(1.0 - texture.x, 1.0 - texture.y);
    }
    else
    {
        TexCoords = texture;
    }
    if (shake)
    {
        float strength = 0.01;
        gl_Position.x += cos(time * 10) * strength;
        gl_Position.y += cos(time * 15) * strength;
    }
}  
