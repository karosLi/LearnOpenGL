#version 330 core
/**
 几何着色器是负责将所有世界空间的顶点变换到6个不同的光空间的着色器。
 */

layout (triangles) in;// 输入的是要绘制的某一个立方体的某一个面的一个三角形图元
layout (triangle_strip, max_vertices=18) out;// 虽然我们最终提交了 6 个三角形图元，但是对几何着色器来说是输出一个三角形带图元，18 个顶点

uniform mat4 shadowMatrices[6];

out vec4 FragPos; // FragPos from GS (output per emitvertex) - 输出到片段着色器，每 EmitVertex 一次，就输出一次

void main()
{
    /**
     EmitVertex：每次我们调用EmitVertex时，gl_Position中的向量会被添加到图元中来。
     EndPrimitive：当EndPrimitive被调用时，所有发射出的(Emitted)顶点都会合成为指定的输出渲染图元。在一个或多个EmitVertex调用之后重复调用EndPrimitive能够生成多个图元。
     */
    
    /**
     外部是一次渲染调用，内部是6次渲染调用，提升了性能
     1、遍历深度立方体贴图的面，并设置哪个面为图元要渲染到的面（仅在我们将立方体贴图纹理附加到活动帧缓冲区时才有效。右、左、上、下、近、远）
     2、绑定到指定输出面后，遍历输入三角形图元的顶点，并把所有顶点变换到面所属的光空间里，把变换后的顶点加入到图元（注意：变换只是调整了观察方向，并不代表点本身发生了位移，所以只有属于自己光空间的顶点才能被看到，因为其他光空间的顶点被裁剪掉了，那也就不用更新深度值了）
     3、提交图元，并渲染图元到指定的输出面（只会把属于自己面的图元渲染到属于自己面的光空间里，不属于的不会渲染）
     */
    for(int face = 0; face < 6; ++face)
    {
        gl_Layer = face; // built-in variable that specifies to which face we render.
        for(int i = 0; i < 3; ++i) // for each triangle's vertices
        {
            // 世界坐标的顶点
            FragPos = gl_in[i].gl_Position; // 同一组三角形的顶点，做了 6 次光空间变换
            // 将世界坐标中的三角形的顶点变换到光空间，只是观察方向发生了变化，点本身并没有发生位移
            gl_Position = shadowMatrices[face] * FragPos;
            EmitVertex();
        }
        
        // 每End一次，就渲染图元。
        EndPrimitive();
    }
}
