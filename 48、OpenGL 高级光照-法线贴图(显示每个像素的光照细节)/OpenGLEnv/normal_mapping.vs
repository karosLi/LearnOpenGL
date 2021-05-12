#version 330 core
layout (location = 0) in vec3 aPos;// 模型空间顶点
layout (location = 1) in vec3 aNormal;// 法线向量 N
layout (location = 2) in vec2 aTexCoords;// 纹理坐标
layout (location = 3) in vec3 aTangent;// 切线向量 T
layout (location = 4) in vec3 aBitangent;// 副切线向量 B

out VS_OUT {
    vec3 FragPos;// 世界空间顶点位置
    vec2 TexCoords;//纹理坐标
    
    vec3 TangentLightPos;// 切线空间光源位置
    vec3 TangentViewPos;// 切线空间摄像机位置
    vec3 TangentFragPos;// 切线空间顶点位置
} vs_out;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

uniform vec3 lightPos;// 世界空间光源位置
uniform vec3 viewPos;// 世界空间摄像机位置

void main()
{
    // 计算世界空间顶点位置
    vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
    vs_out.TexCoords = aTexCoords;
    
    // 法线变换矩阵，把 T,B,N 向量从模型空间转到世界空间
    mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normalMatrix * aTangent);
    vec3 N = normalize(normalMatrix * aNormal);
    // re-orthogonalize T with respect to N
    // 格拉姆-施密特正交化过程（Gram-Schmidt process）的数学技巧，我们可以对TBN向量进行重正交化，这样每个向量就又会重新垂直了。目的是法向贴图应用到这些表面时将切线向量平均化可以获得更好更平滑的结果
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);
    // 创建TBN矩阵
    mat3 TBN = mat3(T, B, N);
    
    /**
     现在我们有了TBN矩阵，如果来使用它呢？通常来说有两种方式使用它，我们会把这两种方式都说明一下：

     1、我们直接使用TBN矩阵，这个矩阵可以把切线坐标空间的向量转换到世界坐标空间。因此我们把它传给片段着色器中，把通过采样得到的法线坐标左乘上TBN矩阵，转换到世界坐标空间中，这样所有法线和其他光照变量就在同一个坐标系中了。
     2、我们也可以使用TBN矩阵的逆矩阵，这个矩阵可以把世界坐标空间的向量转换到切线坐标空间。因此我们使用这个矩阵右乘其他光照变量，把他们转换到切线空间，这样法线和其他光照变量再一次在一个坐标系中了。
     
     注意：对于TBN矩阵，由于它是一个正交矩阵，即 TBN * TBN的转置 = 单位矩阵，而我们知道 TBN * TNB 的逆矩阵 = 单位矩阵，所以对于正交矩阵，它的特性是 TBN的转置 = TNB 的逆矩阵，由于 转置的计算量是小于逆矩阵的，所以下面的代码中我们使用TNB的转置矩阵
     */
    
    // 这里我们使用第二种使用方式，让矩阵把世界坐标空间的向量转换到切线坐标空间
    /**
     将向量从世界空间转换到切线空间有个额外好处，我们可以把所有相关向量在顶点着色器中转换到切线空间，不用在像素着色器中做这件事。这是可行的，因为lightPos和viewPos不是每个fragment运行都要改变，对于fs_in.FragPos，我们也可以在顶点着色器计算它的切线空间位置。基本上，不需要把任何向量在像素着色器中进行变换，而第一种方法中就是必须的，因为采样出来的法线向量对于每个像素着色器都不一样。
     
     所以现在不是把TBN矩阵的逆矩阵发送给像素着色器，而是将切线空间的光源位置，观察位置以及顶点位置发送给像素着色器。这样我们就不用在像素着色器里进行矩阵乘法了。这是一个极佳的优化，因为顶点着色器通常比像素着色器运行的少。这也是为什么这种方法是一种更好的实现方式的原因。
     */
    TBN = transpose(TBN);
    vs_out.TangentLightPos = TBN * lightPos;
    vs_out.TangentViewPos  = TBN * viewPos;
    vs_out.TangentFragPos  = TBN * vs_out.FragPos;
        
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
