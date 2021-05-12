#version 330 core
/**
 使用Uniform缓冲
 
 设置一次可以在多个着色器程序里使用，是一个共享内存的缓冲区
 
 Uniform缓冲对象比起独立的uniform有很多好处。第一，一次设置很多uniform会比一个一个设置多个uniform要快很多。第二，比起在多个着色器中修改同样的uniform，在Uniform缓冲中修改一次会更容易一些。最后一个好处可能不会立即显现，如果使用Uniform缓冲对象的话，你可以在着色器中使用更多的uniform。OpenGL限制了它能够处理的uniform数量，这可以通过GL_MAX_VERTEX_UNIFORM_COMPONENTS来查询。当使用Uniform缓冲对象时，最大的数量会更高。所以，当你达到了uniform的最大数量时（比如再做骨骼动画(Skeletal Animation)的时候），你总是可以选择使用Uniform缓冲对象。
 
 https://learnopengl-cn.github.io/04%20Advanced%20OpenGL/08%20Advanced%20GLSL/
 */

layout (location = 0) in vec3 aPos;

layout (std140) uniform Matrices
{
    mat4 projection;
    mat4 view;
};
uniform mat4 model;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
