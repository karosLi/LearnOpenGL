#version 330 core
/**
 泛光
 
 泛光步骤：
 1、使用 HDR 帧缓冲
 2、绘制场景到纹理
 3、提取亮色纹理
 4、模糊亮色纹理
 5、混合场景纹理和模糊后的亮色纹理
 
 光晕效果可以使用一个后处理特效泛光来实现。泛光使所有明亮区域产生光晕效果。下面是一个使用了和没有使用光晕的对比
 https://learnopengl-cn.github.io/img/05/07/bloom_example.png
 
 Bloom是我们能够注意到一个明亮的物体真的有种明亮的感觉。泛光可以极大提升场景中的光照效果，并提供了极大的效果提升，尽管做到这一切只需一点改变。
 Bloom和HDR结合使用效果很好。常见的一个误解是HDR和泛光是一样的，很多人认为两种技术是可以互换的。但是它们是两种不同的技术，用于各自不同的目的上。可以使用默认的8位精确度的帧缓冲，也可以在不使用泛光效果的时候，使用HDR。只不过在有了HDR之后再实现泛光就更简单了。
 
 为实现泛光，我们像平时那样渲染一个有光场景，提取出场景的HDR颜色缓冲以及只有这个场景明亮区域可见的图片。被提取的带有亮度的图片接着被模糊，结果被添加到HDR场景上面。
 我们来一步一步解释这个处理过程。我们在场景中渲染一个带有4个立方体形式不同颜色的明亮的光源。带有颜色的发光立方体的亮度在1.5到15.0之间。如果我们将其渲染至HDR颜色缓冲，场景看起来会是这样的：
 https://learnopengl-cn.github.io/img/05/07/bloom_scene.png
 
 我们得到这个HDR颜色缓冲纹理，提取所有超出一定亮度的fragment。这样我们就会获得一个只有fragment超过了一定阈限的颜色区域：
 https://learnopengl-cn.github.io/img/05/07/bloom_extracted.png
 
 我们将这个超过一定亮度阈限的纹理进行模糊。泛光效果的强度很大程度上是由被模糊过滤器的范围和强度所决定。
 https://learnopengl-cn.github.io/img/05/07/bloom_blurred.png
 
 最终的被模糊化的纹理就是我们用来获得发出光晕效果的东西。这个已模糊的纹理要添加到原来的HDR场景纹理之上。因为模糊过滤器的应用明亮区域发出光晕，所以明亮区域在长和宽上都有所扩展。
 https://learnopengl-cn.github.io/img/05/07/bloom_small.png
 
 泛光本身并不是个复杂的技术，但很难获得正确的效果。它的品质很大程度上取决于所用的模糊过滤器的质量和类型。简单地改改模糊过滤器就会极大的改变泛光效果的品质。
 https://learnopengl-cn.github.io/img/05/07/bloom_steps.png
 
 
 
 
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/07%20Bloom/
 */

layout (location = 0) out vec4 FragColor;// 输出到颜色缓冲 0
layout (location = 1) out vec4 BrightColor;// 输出到颜色缓冲 1
//输出： https://learnopengl-cn.github.io/img/05/07/bloom_attachments.png

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

struct Light {
    vec3 Position;
    vec3 Color;
};

uniform Light lights[4];
uniform sampler2D diffuseTexture;// 一个纹理采样器在这个场景对应两个纹理（地板和箱子）
uniform vec3 viewPos;

void main()
{
    /**
     这也说明了为什么泛光在HDR基础上能够运行得很好。因为HDR中，我们可以将颜色值指定超过1.0这个默认的范围，我们能够得到对一个图像中的亮度的更好的控制权。没有HDR我们必须将阈限设置为小于1.0的数，虽然可行，但是亮部很容易变得很多，这就导致光晕效果过重。
     */
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    // ambient
    vec3 ambient = 0.0 * color;
    // lighting
    vec3 lighting = vec3(0.0);
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    for(int i = 0; i < 4; i++)// 多光源光照影响
    {
        // diffuse
        vec3 lightDir = normalize(lights[i].Position - fs_in.FragPos);
        float diff = max(dot(lightDir, normal), 0.0);
        vec3 result = lights[i].Color * diff * color;
        // attenuation (use quadratic as we have gamma correction)
        float distance = length(fs_in.FragPos - lights[i].Position);
        result *= 1.0 / (distance * distance);
        lighting += result;
                
    }
    vec3 result = ambient + lighting;
    // 所有颜色都需要输出到 颜色缓冲 0
    FragColor = vec4(result, 1.0);
    
    // check whether result is higher than some threshold, if so, output as bloom threshold color
    // 获取物体的亮度
    float brightness = dot(result, vec3(0.2126, 0.7152, 0.0722));
    // 所有片段都输出到 颜色缓冲 1
    if(brightness > 1.0)// 如果亮度大于 1，则输出当前片段颜色，否则用黑色填充
        BrightColor = vec4(result, 1.0);
    else// 否则填充黑色
        BrightColor = vec4(0.0, 0.0, 0.0, 1.0);
}
