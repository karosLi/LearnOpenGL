#version 330 core

/**
 几何着色器：接受一个图元（点/线/其他），输出的是一个新的图元（点/线/其他）
 
 比如当前这个几何着色器，是接受一个点图元作为输入，而这个图元是由一个点构成的图元，然而输出的是一个新的图元，新的图元是由5个顶点组成的三角形带（房子）
 
 
 这个输入布局修饰符可以从顶点着色器接收下列任何一个图元值：
 points：绘制GL_POINTS图元时（1）。
 lines：绘制GL_LINES或GL_LINE_STRIP时（2）
 lines_adjacency：GL_LINES_ADJACENCY或GL_LINE_STRIP_ADJACENCY（4）
 triangles：GL_TRIANGLES、GL_TRIANGLE_STRIP或GL_TRIANGLE_FAN（3）
 triangles_adjacency：GL_TRIANGLES_ADJACENCY或GL_TRIANGLE_STRIP_ADJACENCY（6）
 
 
 输出布局修饰符也可以接受几个图元值：
 points
 line_strip
 triangle_strip
 
 线条(Line Strip)：https://learnopengl-cn.github.io/img/04/09/geometry_shader_line_strip.png
 几何着色器同时希望我们设置一个它最大能够输出的顶点数量（如果你超过了这个值，OpenGL将不会绘制多出的顶点），这个也可以在out关键字的布局修饰符中设置。


 */

// 声明从顶点着色器输入的图元类型
layout (points) in;

// 指定几何着色器输出的图元类型
layout (triangle_strip, max_vertices = 5) out;

in VS_OUT {
    vec3 color;
} gs_in[];

out vec3 fColor;

void build_house(vec4 position)
{
    /**
     每次我们调用EmitVertex时，gl_Position中的向量会被添加到图元中来。当EndPrimitive被调用时，所有发射出的(Emitted)顶点都会合成为指定的输出渲染图元。在一个或多个EmitVertex调用之后重复调用EndPrimitive能够生成多个图元。
     https://learnopengl-cn.github.io/img/04/09/geometry_shader_house.png
     
     */
    fColor = gs_in[0].color; // gs_in[0] since there's only one input vertex
    gl_Position = position + vec4(-0.2, -0.2, 0.0, 0.0); // 1:bottom-left
    EmitVertex();
    gl_Position = position + vec4( 0.2, -0.2, 0.0, 0.0); // 2:bottom-right
    EmitVertex();
    gl_Position = position + vec4(-0.2,  0.2, 0.0, 0.0); // 3:top-left
    EmitVertex();
    gl_Position = position + vec4( 0.2,  0.2, 0.0, 0.0); // 4:top-right
    EmitVertex();
    gl_Position = position + vec4( 0.0,  0.4, 0.0, 0.0); // 5:top
    fColor = vec3(1.0, 1.0, 1.0);// 最后一个顶点的颜色设置为白色
    EmitVertex();
    EndPrimitive();
}

void main() {
    /**
     GLSL提供给我们一个内建(Built-in)变量，gl_in 被声明为一个接口块（Interface Block)，要注意的是，它被声明为一个数组，因为大多数的渲染图元包含多于1个的顶点，而几何着色器的输入是一个图元的所有顶点。
     in gl_Vertex
     {
         vec4  gl_Position;
         float gl_PointSize;
         float gl_ClipDistance[];
     } gl_in[];
     */
    /**
     这里就可以知道，一个图元是由一个点组成，所以 gl_in 数组只会有一个值
     glDrawArrays(GL_POINTS, 0, 4);
     */
    build_house(gl_in[0].gl_Position);
}
