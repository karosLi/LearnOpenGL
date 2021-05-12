#version 330 core
/**
 把两个纹理混合
 
 有了场景的HDR纹理和模糊处理的亮区纹理，我们只需把它们结合起来就能实现泛光或称光晕效果了。最终的像素着色器（大部分和HDR教程用的差不多）要把两个纹理混合：
 
 */
out vec4 FragColor;
//输出： https://learnopengl-cn.github.io/img/05/07/bloom.png

in vec2 TexCoords;

uniform sampler2D scene;// 场景HDR纹理
uniform sampler2D bloomBlur;// 模糊后的亮度纹理
uniform bool bloom;
uniform float exposure;

void main()
{
    const float gamma = 2.2;
    // 场景HDR纹理颜色
    vec3 hdrColor = texture(scene, TexCoords).rgb;
    // 模糊后的亮度纹理颜色
    vec3 bloomColor = texture(bloomBlur, TexCoords).rgb;
    
    if(bloom)// 是否开启泛光
        hdrColor += bloomColor; // additive blending - 添加泛光，要注意的是我们要在应用色调映射之前添加泛光效果。这样添加的亮区的泛光，也会柔和转换为LDR，光照效果相对会更好。
    
    // tone mapping - 色调映射，无损转成 LDR （低动态范围，即演示器可以识别的范围）
    vec3 result = vec3(1.0) - exp(-hdrColor * exposure);
    
    // also gamma correct while we're at it - gamma 修正，提升图像对比度
    result = pow(result, vec3(1.0 / gamma));
    FragColor = vec4(result, 1.0);
}
