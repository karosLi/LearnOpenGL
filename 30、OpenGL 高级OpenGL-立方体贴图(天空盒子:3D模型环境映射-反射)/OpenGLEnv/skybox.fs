#version 330 core
/**
 立方体贴图 - 环境映射
 
 我们现在将整个环境映射到了一个纹理对象上了，能利用这个信息的不仅仅只有天空盒。通过使用环境的立方体贴图，我们可以给物体反射和折射的属性。这样使用环境立方体贴图的技术叫做环境映射(Environment Mapping)，其中最流行的两个是反射(Reflection)和折射(Refraction)。
 
 反射
 反射这个属性表现为物体（或物体的一部分）反射它周围环境，即根据观察者的视角，物体的颜色或多或少等于它的环境。镜子就是一个反射性物体：它会根据观察者的视角反射它周围的环境。
 反射的原理并不难。下面这张图展示了我们如何计算反射向量，并如何使用这个向量来从立方体贴图中采样：
 https://learnopengl-cn.github.io/img/04/06/cubemaps_reflection_theory.png
 
 我们根据观察方向向量I¯和物体的法向量N¯，来计算反射向量R¯。我们可以使用GLSL内建的reflect函数来计算这个反射向量。最终的R¯向量将会作为索引/采样立方体贴图的方向向量，返回环境的颜色值。最终的结果是物体看起来反射了天空盒。


 
 
 https://learnopengl-cn.github.io/04%20Advanced%20OpenGL/06%20Cubemaps/
 */

out vec4 FragColor;

in vec3 TexCoords;// 代表3D纹理坐标的方向向量，来自于顶点着色器中的变换之前的顶点位置


uniform samplerCube skybox;// 立方体贴图的纹理采样器

void main()
{
    FragColor = texture(skybox, TexCoords);
}
