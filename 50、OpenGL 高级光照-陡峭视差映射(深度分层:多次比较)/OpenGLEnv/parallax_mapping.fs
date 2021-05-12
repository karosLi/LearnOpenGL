#version 330 core
/**
 视差贴图（又称高度贴图）
 
 视差贴图(Parallax Mapping)技术和法线贴图差不多，但它有着不同的原则。和法线贴图一样视差贴图能够极大提升表面细节，使之具有深度感。
 
 如果说法线贴图是法向量的偏移，那么高度贴图就是纹理坐标的偏移，所以视差贴图和光照无关
 
 更好理解的一篇
 https://blog.csdn.net/Jaihk662/article/details/108147539
 https://blog.csdn.net/Jaihk662/article/details/108195475
 
 
 视差贴图属于位移贴图(Displacement Mapping)技术的一种，它对根据储存在纹理中的几何信息对顶点进行位移或偏移。一种实现的方式是比如有1000个顶点，根据纹理中的数据对平面特定区域的顶点的高度进行位移。这样的每个纹理像素包含了高度值纹理叫做高度贴图。一张简单的砖块表面的高度贴图如下所示：
 https://learnopengl-cn.github.io/img/05/05/parallax_mapping_height_map.png
 
 整个平面上的每个顶点都根据从高度贴图采样出来的高度值进行位移，根据材质的几何属性平坦的平面变换成凹凸不平的表面。例如一个平坦的平面利用上面的高度贴图进行置换能得到以下结果：
 https://learnopengl-cn.github.io/img/05/05/parallax_mapping_plane_heightmap.png
 
 上面的那个表面使用视差贴图技术渲染，位移贴图技术不需要额外的顶点数据来表达深度，它像法线贴图一样采用一种聪明的手段欺骗用户的眼睛。
 
 
 原理：
 视差贴图背后的思想是修改纹理坐标使一个fragment的表面看起来比实际的更高或者更低，所有这些都根据观察方向和高度贴图。为了理解它如何工作，看看下面砖块表面的图片：
 这里粗糙的红线代表高度贴图中的数值的立体表达，向量V¯代表观察方向。如果平面进行实际位移，观察者会在点B看到表面。然而我们的平面没有实际上进行位移，观察方向将在点A与平面接触。视差贴图的目的是，在A位置上的fragment不再使用点A的纹理坐标而是使用点B的。随后我们用点B的纹理坐标采样，观察者就像看到了点B一样。

 这个技巧就是描述如何从点A得到点B的纹理坐标。视差贴图尝试通过对从fragment到观察者的方向向量V¯进行缩放的方式解决这个问题，缩放的大小是A处fragment的高度。所以我们将V¯的长度缩放为高度贴图在点A处H(A)采样得来的值。下图展示了经缩放得到的向量P¯：
 https://learnopengl-cn.github.io/img/05/05/parallax_mapping_scaled_height.png
 我们随后选出P¯以及这个向量与平面对齐的坐标作为纹理坐标的偏移量。这能工作是因为向量P¯是使用从高度贴图得到的高度值计算出来的，所以一个fragment的高度越高位移的量越大。
 
 这个技巧在大多数时候都没问题，但点B是粗略估算得到的。当表面的高度变化很快的时候，看起来就不会真实，因为向量P¯最终不会和B接近，就像下图这样：
 https://learnopengl-cn.github.io/img/05/05/parallax_mapping_incorrect_p.png
 
 将fragment到观察者的向量V¯转换到切线空间中，经变换的P¯向量的x和y元素将于表面的切线和副切线向量对齐。由于切线和副切线向量与表面纹理坐标的方向相同，我们可以用P¯的x和y元素作为纹理坐标的偏移量，这样就不用考虑表面的方向了。
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/05%20Parallax%20Mapping/
 */

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;// 世界空间顶点位置
    vec2 TexCoords;//纹理坐标
    
    vec3 TangentLightPos;// 切线空间光源位置
    vec3 TangentViewPos;// 切线空间摄像机位置
    vec3 TangentFragPos;// 切线空间顶点位置
} fs_in;

uniform sampler2D diffuseMap;// 漫反射贴图纹理
uniform sampler2D normalMap;// 法线贴图纹理
uniform sampler2D depthMap;// 深度阴影贴图纹理

uniform float heightScale;// 视差参数，值越大，效果越明显

/**
 计算偏移后的纹理坐标
 
 1、视差贴图(高度贴图)和法线贴图一样，都是处于切线空间
 2、通过高度贴图采样高度值（实际是深度值），缩放片段到摄像机的方向向量，得到该向量在 TB 轴上的 x,y 值（偏移量）

 */
//vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
//{
//    /**
//     实际上我们使用的高度贴图内部存的是一个深度值，如果对应的像素是黑色的，意味着此纹理的高度为0，也就是默认值，如果一张高度图为全黑那就相当于没有高度图一样，像素点越接近白色，就代表着当前像素点越“凹”。
//
//     下面的计算逻辑是按这个图来的，虽然是深度值，但是我们先当高度值来计算
//     https://learnopengl-cn.github.io/img/05/05/parallax_mapping_scaled_height.png
//     */
//
//    // bricks2_disp.jpeg 是一张存着深度值的高度贴图，其实叫做深度贴图更合适，只不过人们习惯叫了高度贴图
//    // 从高度贴图采样深度值 [0,1]，越大就越表示越凹
//    float height =  texture(depthMap, texCoords).r;
//
//    // 缩放观察向量，得到点 p，点 p 所在的 x，y 值就是原始纹理坐标的偏移量
//    vec2 p = viewDir.xy * (height * heightScale);
//    /**
//     有一个地方需要注意，就是viewDir.xy除以viewDir.z那里。因为viewDir向量是经过了标准化的，viewDir.z会在0.0到1.0之间的某处。当viewDir大致平行于表面时，它的z元素接近于0.0，除法会返回比viewDir垂直于表面的时候更大的P¯向量。所以，从本质上，相比正朝向表面，当带有角度地看向平面时，我们会更大程度地缩放P¯的大小，从而增加纹理坐标的偏移；这样做在视角上会获得更大的真实度。
//     有些人更喜欢不在等式中使用viewDir.z，因为普通的视差贴图会在角度上产生不尽如人意的结果；这个技术叫做有偏移量限制的视差贴图（Parallax Mapping with Offset Limiting）。选择哪一个技术是个人偏好问题，但我倾向于普通的视差贴图。
//     */
//    p = p / viewDir.z;
//
//    /**
//     https://learnopengl-cn.github.io/img/05/05/parallax_mapping_scaled_height.png
//     由于目前 p 是一个偏移量，我们通过纹理坐标减去偏移量就可以得到在下面深度视角下的新的纹理坐标
//
//     https://learnopengl-cn.github.io/img/05/05/parallax_mapping_depth.png
//
//
//     如果是坐标纹理加上偏移量得到是在高度视角下的新的纹理坐标，这就需要提供的高度贴图存储是一个高度值才行
//     */
//    return texCoords - p;
//}

/**
 陡峭视差映射(Steep Parallax Mapping)是视差映射的扩展，原则是一样的，但不是使用一个样本而是多个样本来确定向量P¯到B。即使在陡峭的高度变化的情况下，它也能得到更好的结果，原因在于该技术通过增加采样的数量提高了精确性。
 
 陡峭视差映射的基本思想是将总深度范围划分为同一个深度/高度的多个层。从每个层中我们沿着P¯方向移动采样纹理坐标，直到我们找到一个采样低于当前层的深度值。看看下面的图片：
 https://learnopengl-cn.github.io/img/05/05/parallax_mapping_steep_parallax_mapping_diagram.png
 

 陡峭视差贴图同样有自己的问题。因为这个技术是基于有限的样本数量的，我们会遇到锯齿效果以及图层之间有明显的断层：
 https://learnopengl-cn.github.io/img/05/05/parallax_mapping_steep_artifact.png
 
 */
vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{
    // number of depth layers
    /**
     当垂直看一个表面的时候纹理时位移比以一定角度看时的小。我们可以在垂直看时使用更少的样本，以一定角度看时增加样本数量：
     这里我们得到viewDir和正z方向的点乘，使用它的结果根据我们看向表面的角度调整样本数量（注意正z方向等于切线空间中的表面的法线）。如果我们所看的方向平行于表面，我们就是用32层。
     
     mix(x, y, a) 返回线性混合的 x 和 y，如：x * (1−a) + y * a
     */
    const float minLayers = 8;
    const float maxLayers = 32;
    float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir)));// 深度层数
    // calculate the size of each layer
    float layerDepth = 1.0 / numLayers;// 每层深度大小
    // depth of current layer
    float currentLayerDepth = 0.0;// 当前层的深度
    // the amount to shift the texture coordinates per layer (from vector P)
    vec2 P = viewDir.xy / viewDir.z * heightScale;// 定义向量 P，向量 P 就是总位移
    vec2 deltaTexCoords = P / numLayers;// 向量除法，得到沿着 P 向量方向每前进一层的偏移量
  
    // get initial values
    vec2  currentTexCoords     = texCoords;// 初始化纹理坐标
    float currentDepthMapValue = texture(depthMap, currentTexCoords).r;// 第一层纹理坐标采样的深度值
      
    while(currentLayerDepth < currentDepthMapValue)// 直到当前层纹理坐标采样的深度值小于等于当前层的深度才停止，也就是只要找到第一个当前采样的深度在当前层深度之上就停止遍历
    {
        // shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;// 因为是减法，所以是沿着 P 向量方向，逐步从上放下，获取下一个纹理坐标
        // get depthmap value at current texture coordinates
        currentDepthMapValue = texture(depthMap, currentTexCoords).r;// 获取当前层纹理坐标采样的深度值
        // get depth of next layer
        currentLayerDepth += layerDepth;// 获取下一层的深度
    }
    
    return currentTexCoords;
}

void main()
{
    // offset texture coordinates with Parallax Mapping - 在切线空间计算摄像机方向向量，即片段到摄像机的方向向量
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = fs_in.TexCoords;
    
    // 通过从高度贴图采样的高度值，重新计算纹理坐标
    texCoords = ParallaxMapping(fs_in.TexCoords,  viewDir);
    // 偏移后的纹理坐标必须在 [0,1] 之间
    if (texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard;
    
    // obtain normal from normal map in range [0,1] - 从法线贴图获取法线向量
    vec3 normal = texture(normalMap, texCoords).rgb;
    
    // transform normal vector to range [-1,1] - 法线贴图纹理中存的是 [0,1] 的法线向量值，需要转成 [-1,1]
    normal = normalize(normal * 2.0 - 1.0);  // this normal is in tangent space
    
    // 片段着色器使用Blinn-Phong光照模型渲染场景
    // 获取漫反射纹理颜色
    vec3 color = texture(diffuseMap, texCoords).rgb;
    // ambient 环境光照分量 = 环境光照强度 * 材质环境光照反射分量颜色
    vec3 ambient = 0.1 * color;
    // 把从法线贴图采样出来的法线向量与已经被转成切线空间的光源位置和片段位置，一起做计算，求出在切线空间的光源方向
    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;
    // specular
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 specular = vec3(0.2) * spec;
    FragColor = vec4(ambient + diffuse + specular, 1.0);
}
