#version 330 core

/**
 几何着色器：接受一个图元（点/线/其他），输出的是一个新的图元（点/线/其他）
 
 爆破物体
 
 当我们说爆破一个物体时，我们并不是指要将宝贵的顶点集给炸掉，我们是要将每个三角形沿着法向量的方向移动一小段时间。效果就是，整个物体看起来像是沿着每个三角形的法线向量爆炸一样。爆炸三角形的效果在纳米装模型上看起来像是这样的：
 https://learnopengl-cn.github.io/img/04/09/geometry_shader_explosion.png
 
 **/

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

in VS_OUT {
    vec2 texCoords;
} gs_in[];

out vec2 TexCoords;

uniform float time;

/**
 这个函数会返回一个新的向量，它是位置向量沿着法线向量进行位移之后的结果：
 
 函数本身应该不是非常复杂。sin函数接收一个time参数，它根据时间返回一个-1.0到1.0之间的值。因为我们不想让物体向内爆炸(Implode)，我们将sin值变换到了[0, 1]的范围内。
 最终的结果会乘以normal向量，并且最终的direction向量会被加到位置向量上。
 */
vec4 explode(vec4 position, vec3 normal)
{
    float magnitude = 2.0;
    vec3 direction = normal * ((sin(time) + 1.0) / 2.0) * magnitude;
    return position + vec4(direction, 0.0);
}

/**
 我们使用叉乘来获取垂直于其它两个向量的一个向量。如果我们能够获取两个平行于三角形表面的向量a和b，我们就能够对这两个向量进行叉乘来获取法向量了
 这里我们使用减法获取了两个平行于三角形表面的向量a和b。因为两个向量相减能够得到这两个向量之间的差值，并且三个点都位于三角平面上，对任意两个向量相减都能够得到一个平行于平面的向量。
 
 注意，如果我们交换了cross函数中a和b的位置，我们会得到一个指向相反方向的法向量——这里的顺序很重要！
 */
vec3 GetNormal()
{
    vec3 a = vec3(gl_in[0].gl_Position) - vec3(gl_in[1].gl_Position);
    vec3 b = vec3(gl_in[2].gl_Position) - vec3(gl_in[1].gl_Position);
    return normalize(cross(a, b));
}

void main() {
    /**
     每次我们调用EmitVertex时，gl_Position中的向量会被添加到图元中来。当EndPrimitive被调用时，所有发射出的(Emitted)顶点都会合成为指定的输出渲染图元。在一个或多个EmitVertex调用之后重复调用EndPrimitive能够生成多个图元。
     */
    
    // 计算法向量
    vec3 normal = GetNormal();

    gl_Position = explode(gl_in[0].gl_Position, normal);
    TexCoords = gs_in[0].texCoords;
    EmitVertex();
    gl_Position = explode(gl_in[1].gl_Position, normal);
    TexCoords = gs_in[1].texCoords;
    EmitVertex();
    gl_Position = explode(gl_in[2].gl_Position, normal);
    TexCoords = gs_in[2].texCoords;
    EmitVertex();
    EndPrimitive();
}
