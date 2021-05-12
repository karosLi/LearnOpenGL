#version 330 core
/**
 SSAO
 屏幕空间环境光遮蔽(Screen-Space Ambient Occlusion, SSAO)
 
 好文章
 https://www.qiujiawei.com/ssao/
 
 https://learnopengl-cn.github.io/img/05/09/ssao_hemisphere.png
 法向半球体(Normal-oriented Hemisphere)周围采样，我们将不会考虑到片段底部的几何体.它消除了环境光遮蔽灰蒙蒙的感觉，从而产生更真实的结果。这个SSAO教程将会基于法向半球法和John Chapman出色的SSAO教程。http://john-chapman-graphics.blogspot.com/2013/01/ssao-tutorial.html
 
 SSAO背后的原理很简单：在每个片段的法向半球体周围采样，根据片段周边深度值计算一个遮蔽因子(Occlusion Factor)。
 这个遮蔽因子之后会被用来减少或者抵消片段的环境光照分量。遮蔽因子是通过采集片段周围半球型核心的多个深度样本，并和当前片段深度值对比而得到的。高于片段深度值样本的个数就是我们想要的遮蔽因子。
 
 几何体内灰色的深度样本都是高于片段深度值的，他们会增加遮蔽因子；几何体内样本个数越多，片段获得的环境光照也就越少。
 
 
 样本缓冲
 SSAO需要获取几何体的信息，因为我们需要一些方式来确定一个片段的遮蔽因子。对于每一个片段，我们将需要这些数据：

 1、逐片段位置向量
 2、逐片段的法线向量
 3、逐片段的反射颜色
 4、采样核心
 5、用来旋转采样核心的随机旋转矢量
 通过使用一个逐片段观察空间位置，我们可以将一个采样半球核心对准片段的观察空间表面法线。对于每一个核心样本我们会采样线性深度纹理来比较结果。采样核心会根据旋转矢量稍微偏转一点；我们所获得的遮蔽因子将会之后用来限制最终的环境光照分量。
 https://learnopengl-cn.github.io/img/05/09/ssao_overview.png
 
 
 */
out float FragColor;

in vec2 TexCoords;

/**
 入参：
 G缓冲纹理(位置和法向量)，噪声纹理和法向半球核心样本
 */
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D texNoise;
uniform vec3 samples[64];

// parameters (you'd probably want to use them as uniforms to more easily tweak the effect)
int kernelSize = 64;
float radius = 0.5;// 可以调整的参数，控制采样半径范围
float bias = 0.025;// bias 是深度偏移值 bias太小就会出现acne现象 建议0.025

// tile noise texture over screen based on screen dimensions divided by noise size - 屏幕的平铺噪声纹理会根据屏幕分辨率除以噪声大小的值来决定。屏幕 = 800x600
const vec2 noiseScale = vec2(800.0/4.0, 600.0/4.0);

uniform mat4 projection;

void main()
{
    // get input for SSAO algorithm
    vec3 fragPos = texture(gPosition, TexCoords).xyz;
    vec3 normal = normalize(texture(gNormal, TexCoords).rgb);
    
    // 获取随机旋转向量并单位化
    vec3 randomVec = normalize(texture(texNoise, TexCoords * noiseScale).xyz);
    /**
     https://www.qiujiawei.com/ssao/
     
     计算TBN矩阵最为精妙的就是第一步：tangent向量的计算。因为最终要构造出的TBN的z方向是normal的方向，所以未知数就是相应的x、y方向，而因为正交矩阵的一个基可以用另外2个基做叉乘得到，所以未知的y方向（bitangent）等于normal和tangent的cross。真正要算的只有x的方向：tangent向量。

     tangent向量，必然和normal正交，但方向和randomVec有关（所以randomVec才被称为旋转向量）。

     首先，randomVec和normal的角度关系需要先计算出来，方法就是做点积：

     dot(randomVec, normal) = cosθ
     cosθ就是一个投影系数，用cosθ去乘以randomVec就得到normal在randomVec上的投影（方向相同，长度不等），同理，用cosθ去乘以normal就得到randomVec在normal上的投影。

     再看一下上面的代码：

         vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal)); // TBN的x方向
     可以看出normal * dot(randomVec, normal) 就是指randomVec在normal上的投影，有了这个投影点r向量后，就可以用randomVec - r，得到垂直于normal的tangent向量。记得需要再单位化。

     */
    // create TBN change-of-basis matrix: from tangent-space to view-space
    // 计算TBN，用TBN左乘samplePos就可以把samplePos从tangent space转换到view space
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));// TBN的x方向
    vec3 bitangent = cross(normal, tangent);// TBN的y方向
    mat3 TBN = mat3(tangent, bitangent, normal);// normal 是 TBN的z方向，最终得到 TBN 矩阵
    // iterate over the sample kernel and calculate occlusion factor
    // 计算遮蔽因子
    float occlusion = 0.0;
    for(int i = 0; i < kernelSize; ++i)// 64 个采样点
    {
        // get sample position
        vec3 samplePos = TBN * samples[i]; // from tangent to view-space - 把采样点从tangent space转到view space
        // 做偏移，得到真正的采样坐标(view space)
        samplePos = fragPos + samplePos * radius;
        
        // project sample position (to sample texture) (to get position on screen/texture)
        /// 从view space转到screen space
        vec4 offset = vec4(samplePos, 1.0);
        offset = projection * offset; // from view to clip-space - 从观察者空间转到裁剪空间
        offset.xyz /= offset.w; // perspective divide - 透视除法得到NDC坐标（normalized device coordinates）x,y,z都在[-1，1]
        offset.xyz = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0 - 映射NDC到[0.0, 1.0]，从而可以采样GBuffer的纹理
        
        // get sample depth - 获得GBuffer中该片段的深度值
        float sampleDepth = texture(gPosition, offset.xy).z; // get depth value of kernel sample
        
        // range check & accumulate - 深度比较 （bias是调整值 bias太小就会出现acne现象 建议0.025）
        /*
         其中的rangeCheck步骤是比较特殊的处理，它解决的是这么个情况：对于一个物体的轮廓处的fragment，在做采样计算时会把后面的远处的fragment也拉进来测试。所以要计算fragPos.z - sampleDepth，求出当前fragment的深度以及被采样点的深度的距离，距离过大就说明不是近邻的会遮蔽自己的fragment。距离越大，rangeCheck就会越接近0，从而_occlusion值也会削弱。（注意fragPos.z - sampleDepth小于等于radius）
         */
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
        // 高于片段深度值样本的个数就是我们想要的遮蔽因子
        occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
    }
    
    // 几何体内灰色的深度样本都是高于片段深度值的，他们会增加遮蔽因子；几何体内样本个数越多，片段获得的环境光照也就越少。
    // 遮蔽率越接近0，颜色更黑，遮蔽率越接近1，颜色则贴近原来的色
    occlusion = 1.0 - (occlusion / kernelSize);
    
    FragColor = occlusion;
}
