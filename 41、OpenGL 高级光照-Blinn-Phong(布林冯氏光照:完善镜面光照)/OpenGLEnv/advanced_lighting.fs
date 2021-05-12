#version 330 core

/**
 Phong模型中
 在镜面高光区域的边缘出现了一道很明显的断层。出现这个问题的原因是观察向量和反射向量间的夹角不能大于90度。如果点积的结果为负数，镜面光分量会变为0.0。
 
 https://learnopengl-cn.github.io/img/05/01/advanced_lighting_phong_limit.png
 https://learnopengl-cn.github.io/img/05/01/advanced_lighting_over_90.png
 

 Blinn-Phong 模型
 Blinn-Phong 模型与冯氏模型非常相似，但是它对镜面光模型的处理上有一些不同，让我们能够解决之前提到的问题。Blinn-Phong模型不再依赖于反射向量，而是采用了所谓的半程向量(Halfway Vector)，即光线与视线夹角一半方向上的一个单位向量。当半程向量与法线向量越接近时，镜面光分量就越大。
 https://learnopengl-cn.github.io/img/05/01/advanced_lighting_halfway_vector.png
 
 当视线正好与（现在不需要的）反射向量对齐时，半程向量就会与法线完美契合。所以当观察者视线越接近于原本反射光线的方向时，镜面高光就会越强。
 
 现在，不论观察者向哪个方向看，半程向量与表面法线之间的夹角都不会超过90度（除非光源在表面以下）。它产生的效果会与冯氏光照有些许不同，但是大部分情况下看起来会更自然一点，特别是低高光的区域。Blinn-Phong着色模型正是早期固定渲染管线时代时OpenGL所采用的光照模型。
 
 
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/01%20Advanced%20Lighting/
 */
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

uniform sampler2D floorTexture;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform bool blinn;

void main()
{
    // 物体颜色 = (环境光照强度 * 材质颜色环境光照反射分量颜色 + 漫反射光照强度 *  材质颜色漫反射光照反射分量颜色 * 漫反射因子diff + 镜面光照强度 * 材质颜色镜面光照分量颜色 * 镜面光照因子spec) * 衰减 * 聚光范围
    
    vec3 color = texture(floorTexture, fs_in.TexCoords).rgb;
    // ambient - 环境光照强度 0.05
    vec3 ambient = 0.05 * color;
    
    // 光方向向量：片段到光源的向量
    // 光的方向向量是光源位置向量与片段位置向量之间的向量差（光源位置就是灯的位置，片段位置就是变换后的顶点位置，他们都是以世界坐标系原点出发的向量，所以光源位置向量减去片段位置向量是等于片段到光源的向量，虽然反向是反的，但是可以平移该向量让他的起始点与法向量起始点交接在一起）
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    // 法向量
    vec3 normal = normalize(fs_in.Normal);
    // diffuse - 漫反射光照强度 1
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;
    // specular
    // 摄像机的方向向量：片段到摄像机（观察者）的向量
    // 摄像机的方向向量是摄像机位置向量与片段位置向量之间的向量差，即 片段到摄像机（观察者）的向量
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;
    
    if(blinn)
    {
        // 所以根据平行四边形法则，光方向向量加上摄像机方向向量就可以得到半程方向向量，最后把半程方向向量进行标准化，只需要方向即可
        vec3 halfwayDir = normalize(lightDir + viewDir);
        spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    }
    else
    {
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = pow(max(dot(viewDir, reflectDir), 0.0), 8.0);
    }
    // specular - 镜面光照强度 0.3 * 镜面光照因子spec * 材质颜色镜面光照分量颜色
    vec3 specular = vec3(0.3) * spec * color; // assuming bright white light color
    FragColor = vec4(ambient + diffuse + specular, 1.0);
}
