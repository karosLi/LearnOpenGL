#version 330 core
/**
 阴影映射，应用深度/阴影贴图
 
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
    // 为了得到片段的当前深度，我们简单获取投影向量的z坐标，它等于来自光的透视视角的片段的深度。
    float currentDepth = projCoords.z;
    
    // check whether current frag pos is in shadow
    // 实际的对比就是简单检查currentDepth是否高于closetDepth，如果是，那么片段就在阴影中。
    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;
    
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
