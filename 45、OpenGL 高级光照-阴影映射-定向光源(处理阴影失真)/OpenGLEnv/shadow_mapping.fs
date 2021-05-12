#version 330 core
/**
 阴影映射，应用深度/阴影贴图
 
 阴影失真问题，使用阴影偏移/深度偏移解决
 阴影锯齿问题，使用 PCF 多次深度采样加权求出阴影值来解决
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/03%20Shadows/01%20Shadow%20Mapping/
 */

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;// 世界空间顶点位置
    vec3 Normal;// 世界空间法线向量
    vec2 TexCoords;//纹理坐标
    vec4 FragPosLightSpace; // 光空间顶点位置
} fs_in;

uniform sampler2D diffuseTexture;// 漫反射贴图
uniform sampler2D shadowMap;// 深度/阴影贴图

uniform vec3 lightPos;// 光源位置
uniform vec3 viewPos;// 摄像机方向

/**
 首先要检查一个片段是否在阴影中，把光空间片段位置转换为裁切空间的标准化设备坐标。
 当我们在顶点着色器输出一个裁切空间顶点位置到gl_Position时，OpenGL自动进行一个透视除法，将裁切空间坐标的范围-w到w转为-1到1，这要将x、y、z元素除以向量的w元素来实现。由于裁切空间的FragPosLightSpace并不会通过gl_Position传到片段着色器里，我们必须自己做透视除法：
 
 当使用正交投影矩阵，顶点w元素仍保持不变，所以这一步实际上毫无意义。可是，当使用透视投影的时候就是必须的了，所以为了保证在两种投影矩阵下都有效就得留着这行
 */
float ShadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide - 执行透视除法，得到NDC标准设备坐标，x,y,z都在 [-1,1] 的范围内
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    
    // transform to [0,1] range - 转换 x,y,z 到在 [0,1] 的范围内
    /**
     因为来自深度贴图的深度在0到1的范围，我们也打算使用projCoords从深度贴图中去采样，所以我们将NDC坐标变换为0到1的范围。 （译者注：这里的意思是，上面的projCoords的xyz分量都是[-1,1]（下面会指出这对于远平面之类的点才成立），而为了和深度贴图的深度相比较，z分量需要变换到[0,1]；为了作为从深度贴图中采样的坐标，xy分量也需要变换到[0,1]。所以整个projCoords向量都需要变换到[0,1]范围。）
     */
    projCoords = projCoords * 0.5 + 0.5;
    
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    // 有了这些投影坐标，我们就能从深度贴图中采样得到0到1的结果，从第一个渲染阶段的projCoords坐标直接对应于变换过的NDC坐标。我们将得到光的位置视野下最近的深度。类似于从深度缓冲区取出深度值。
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    // get depth of current fragment from light's perspective
    // 为了得到当前片段的当前深度，我们简单获取投影向量的z坐标，它等于来自光的透视视角的片段的深度。
    float currentDepth = projCoords.z;
    
//    // check whether current frag pos is in shadow
//    // 实际的对比就是简单检查currentDepth是否高于closetDepth，如果是，那么片段就在阴影中。
//    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;
    
    /**
     阴影失真问题
     
     大神讲解 - 王十一
     https://www.zhihu.com/question/49090321
     
     处理方式：阴影偏移（深度偏移），让当前片段的深度值减小，currentDepth - bias
     
     使用了偏移量后，所有采样点都获得了比表面深度更小的深度值，这样整个表面就正确地被照亮，没有任何阴影。我们可以这样实现这个偏移：
     float bias = 0.005;
     float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
     
     一个0.005的偏移就能帮到很大的忙，但是有些表面坡度很大，仍然会产生阴影失真。有一个更加可靠的办法能够根据表面朝向光线的角度更改偏移量：使用点乘：
     这里我们有一个偏移量的最大值0.05，和一个最小值0.005，它们是基于表面法线和光照方向的。这样像地板这样的表面几乎与光源垂直，得到的偏移就很小，而比如立方体的侧面这种表面得到的偏移就更大。
     
     
     阴影偏移缺点：
     使用阴影偏移的一个缺点是你对物体的实际深度应用了平移。偏移有可能足够大，以至于可以看出阴影相对实际物体位置的偏移，你可以从下图看到这个现象（这是一个夸张的偏移值）：
     https://learnopengl-cn.github.io/img/05/03/01/shadow_mapping_peter_panning.png
     这个阴影失真叫做悬浮(Peter Panning) 因为物体看起来轻轻悬浮在表面之上（译注Peter Pan就是童话彼得潘，而panning有平移、悬浮之意，而且彼得潘是个会飞的男孩…）。
     
     我们可以使用一个叫技巧解决大部分的Peter panning问题：当渲染深度贴图时候使用正面剔除（front face culling）你也许记得在面剔除教程中OpenGL默认是背面剔除。但是一般也不这么处理，因为阴影偏移基本解决大部分问题。
     */
    // calculate bias (based on depth map resolution and slope)
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);
    // check whether current frag pos is in shadow - 阴影偏移，减小当前片段的深度值
//     float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    
    /**
     采样过多问题 - 无需处理，实际跑的demo中，并没有采样过多的问题
     无论你喜不喜欢还有一个视觉差异，就是光的视锥不可见的区域一律被认为是处于阴影中，不管它真的处于阴影之中。出现这个状况是因为超出光的视锥的投影坐标（x,y,z）比1.0大，这样采样的深度纹理就会超出他默认的0到1的范围。根据纹理环绕方式，我们将会得到不正确的深度结果，它不是基于真实的来自光源的深度值。
     https://learnopengl-cn.github.io/img/05/03/01/shadow_mapping_outside_frustum.png
     你可以在图中看到，光照有一个区域，超出该区域就成为了阴影；这个区域实际上代表着深度贴图的大小，这个贴图投影到了地板上。发生这种情况的原因是我们之前将深度贴图的环绕方式设置成了GL_REPEAT。
     处理方式：无需处理，实际跑的demo中，并没有采样过多的问题
     */
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    /**
     仍有一部分是黑暗区域。那里的坐标超出了光的正交视锥的远平面。你可以看到这片黑色区域总是出现在光源视锥的极远处。
     当一个点比光的远平面还要远时，它的投影坐标的z坐标大于1.0。当我们把当前片段坐标的z元素和深度贴图的值（[0,1]）进行了对比；它总是返回 true，所以这些超出光源是视锥体远平面的片段总会是黑色的
     处理方式：解决这个问题也很简单，只要投影向量的z坐标大于1.0，我们就把shadow的值强制设为0.0
     
     这些结果意味着，只有在深度贴图范围以内的被投影的fragment坐标才有阴影，所以任何超出范围的都将会没有阴影。由于在游戏中通常这只发生在远处，就会比我们之前的那个明显的黑色区域效果更真实。
    */
    float shadow = 0;
    if (projCoords.z > 1.0) {
        shadow = 0.0;
    } else {
        /**
         阴影锯齿问题
         
         阴影现在已经附着到场景中了，不过这仍不是我们想要的。如果你放大看阴影，阴影映射对分辨率的依赖很快变得很明显。
         因为深度贴图有一个固定的分辨率，多个片段对应于一个纹理像素。结果就是多个片段会从深度贴图的同一个深度值进行采样，这几个片段便得到的是同一个阴影，这就会产生锯齿边。
         你可以通过增加深度贴图的分辨率的方式来降低锯齿块，也可以尝试尽可能的让光的视锥接近场景。
         
         处理方式：PCF，另一个（并不完整的）解决方案叫做PCF（percentage-closer filtering），这是一种多个不同过滤方式的组合，它产生柔和阴影，使它们出现更少的锯齿块和硬边。
         核心思想是从深度贴图中多次采样，每一次采样的纹理坐标都稍有不同。
         每个独立的样本可能在也可能不再阴影中。所有的次生结果接着结合在一起，进行平均化，我们就得到了柔和阴影。
         
         一个简单的PCF的实现是简单的从纹理像素四周对深度贴图采样，然后把结果平均起来：
         */
        vec2 texelSize = 1.0 / textureSize(shadowMap, 0);// 这个textureSize返回一个给定采样器纹理的0级mipmap的vec2类型的宽和高，用1除以它返回一个单独纹理像素的大小
        // 我们用以对纹理坐标进行偏移，确保每个新样本，来自不同的深度值，这里我们采样得到9个值，它们在投影坐标的x和y值的周围，为阴影阻挡进行测试，并最终通过样本的总数目将结果平均化。
        
        // 利用投影坐标，分别以投影坐标为中心在周围生成 9 个深度采样点，
        // 然后分别比较 9 个采样点的深度值和当前片段的投影坐标的偏移深度，
        // 如果当前片段的偏移深度大于采样点的深度值，则加上认为是这个当前这个这样结果是会产生阴影的，最后除以9来讲结果平均化
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
                
                // 阴影偏移，减小当前片段的深度值
                shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;
            }
        }
        shadow /= 9.0;
    }
        
        
    return shadow;
}

void main()
{
    // 片段着色器使用Blinn-Phong光照模型渲染场景
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightColor = vec3(0.3);
    // ambient 环境光照分量 = 环境光照强度 * 材质环境光照反射分量颜色，为什么下面的计算又乘了一遍 材质环境光照反射分量颜色
//    vec3 ambient = 0.3 * color;
    // 所以这里我认为应该是如下：
    vec3 ambient = vec3(0.3);
    // diffuse
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;
    // specular
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;
    vec3 halfwayDir = normalize(lightDir + viewDir);
    spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
    vec3 specular = spec * lightColor;
    
    
    // calculate shadow - 计算阴影，计算出一个shadow值，当fragment在阴影中时是1.0，在阴影外是0.0
    float shadow = ShadowCalculation(fs_in.FragPosLightSpace);
    // 然后，diffuse和specular颜色会乘以这个阴影元素。由于阴影不会是全黑的（由于散射），我们把ambient分量从乘法中剔除。
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
    
    FragColor = vec4(lighting, 1.0);
}
