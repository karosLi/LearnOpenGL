#version 330 core
/// 物体颜色
// 光照强度 = [环境光照强度，漫反射光照强度，镜面光照强度]
// 材质反射颜色 = [环境光照分量颜色，漫反射光照分量颜色，镜面光照分量颜色]；材质反射颜色可以是反射颜色或者纹理贴图采样出来的反射颜色
// 衰减 = 根据距离计算出来的光源衰减值，先快后慢的衰减
// 聚光范围 = 根据切光角定义被聚光照亮的锥形范围
// 物体颜色 = (环境光照强度 * 材质颜色环境光照反射分量颜色 + 漫反射光照强度 *  材质颜色漫反射光照反射分量颜色 * 漫反射因子diff + 镜面光照强度 * 材质颜色镜面光照分量颜色 * 镜面光照因子spec) * 衰减 * 聚光范围


/// 材质
// ambient 材质向量定义了在环境光照下这个物体反射得是什么颜色，通常这是和物体颜色相同的颜色。
// diffuse 材质向量定义了在漫反射光照下物体的颜色。（和环境光照一样）漫反射颜色也要设置为我们需要的物体颜色。
// specular 材质向量设置的是镜面光照对物体的颜色影响（或者甚至可能反射一个物体特定的镜面高光颜色）。最后，shininess影响镜面高光的散射/半径。
// 如果材质取自纹理，那纹理的颜色就是材质颜色
struct Material {
    sampler2D diffuse;// 漫反射贴图
    sampler2D specular;// 镜面贴图
    float shininess;// 反光度(Shininess)，影响镜面高光的散射/半径
};
uniform Material material;


/// 光源类型
// 定向光（太阳）：是无限远的定向光源，它的所有光线都有着相同的方向（平行），它与光源的位置是没有关系的，比如太阳
// 点光源（灯泡）：点光源是处于世界中某一个位置的光源，它会朝着所有方向发光，但光线会随着距离逐渐衰减。想象作为投光物的灯泡和火把，它们都是点光源。
// 聚光（手电筒）：聚光是位于环境中某个位置的光源，它只朝一个特定方向而不是所有方向照射光线。这样的结果就是只有在聚光方向的特定半径内的物体才会被照亮，其它的物体都会保持黑暗。聚光很好的例子就是路灯或手电筒。

/// 定向光
struct DirLight {
    vec3 direction;// 定向光方向向量

    vec3 ambient;//环境光照强度
    vec3 diffuse;//漫反射光照强度
    vec3 specular;//镜面光照强度
};
uniform DirLight dirLight;


/// 点光源
struct PointLight {
    vec3 position;// 点光源位置

    /// 点光源衰减项：常数项Kc、一次项Kl和二次项Kq。
    float constant;
    float linear;
    float quadratic;

    vec3 ambient;//环境光照强度
    vec3 diffuse;//漫反射光照强度
    vec3 specular;//镜面光照强度
};
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];

/// 聚光
struct SpotLight {
    vec3 position;// 聚光的位置（来计算光的方向向量）
    vec3 direction;// 聚光的方向向量
    float cutOff;// 内切光角
    float outerCutOff;// 外切光角，外切光角 cos 值是小于 内切光角 cos 值，但是角度（弧度）是大于内切光角的角度（弧度）的，用于创造一个聚光边缘平滑过渡的效果：来让光从内圆锥逐渐减暗，直到外圆锥的边界
  
    /// 聚光衰减项：常数项Kc、一次项Kl和二次项Kq。
    float constant;
    float linear;
    float quadratic;
  
    vec3 ambient;//环境光照强度
    vec3 diffuse;//漫反射光照强度
    vec3 specular;//镜面光照强度
};
uniform SpotLight spotLight;


uniform vec3 viewPos; // 摄像机或者观察者位置

in vec3 FragPos;// 片段位置，片段就是指模型变换后的顶点位置
in vec3 Normal;// 法向量，法向量是垂直于平面上的每一个顶点
in vec2 TexCoords;// 纹理坐标

out vec4 FragColor;// 输出颜色


/// 函数原型声明
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    /**
     线性代数里，向量*向量得到是一个标量，而在 GLSL 中向量*向量指的是分量之间相乘（和向量间加法类似），不是点乘也不是矩阵乘法。只有与矩阵相关的一些*运算才定义为矩阵乘法
     */
    
    // 标准化法向量
    vec3 norm = normalize(Normal);
    
    // 摄像机的方向向量是摄像机位置向量与片段位置向量之间的向量差，即 片段到摄像机（观察者）的向量
    vec3 viewDir = normalize(viewPos - FragPos);

    // 第一阶段：定向光照
    vec3 result = CalcDirLight(dirLight, norm, viewDir);
    // 第二阶段：点光源
    for(int i = 0; i < NR_POINT_LIGHTS; i++)
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
    // 第三阶段：聚光
    result += CalcSpotLight(spotLight, norm, FragPos, viewDir);

    FragColor = vec4(result, 1.0);
}


/// 计算定向光对片段颜色的贡献
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    // 光的方向向量取反得到是片段到定向光的方向向量
    vec3 lightDir = normalize(-light.direction);
    
    /// 漫反射着色
    // https://learnopengl-cn.github.io/img/02/02/diffuse_light.png
    // 通过让法向量和光源方向向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明漫反射光照影响越大
    // 如果两个向量之间的角度大于90度，点乘的结果就会变成负数，这样会导致漫反射分量变为负数。为此，我们使用max函数返回两个参数之间较大的参数，从而保证漫反射分量不会变成负数。负数颜色的光照是没有定义的，所以最好避免它，除非你是那种古怪的艺术家。
    float diff = max(dot(normal, lightDir), 0.0);
    
    /// 镜面光着色
    // https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_theory.png
    // 我们通过反射法向量周围光的方向来计算反射向量。然后我们计算反射向量和视线方向的角度差，如果夹角越小，那么镜面光的影响就会越大。它的作用效果就是，当我们去看光被物体所反射的那个方向的时候，我们会看到一个高光。
    // 观察向量是镜面光照附加的一个变量，我们可以使用观察者世界空间位置和片段的位置来计算它。之后，我们计算镜面光强度，用它乘以光源的颜色，再将它加上环境光和漫反射分量。
    // 我们选择在世界空间进行光照计算，但是大多数人趋向于在观察空间进行光照计算。在观察空间计算的好处是，观察者的位置总是(0, 0, 0)，所以这样你直接就获得了观察者位置。可是我发现在学习的时候在世界空间中计算光照更符合直觉。如果你仍然希望在观察空间计算光照的话，你需要将所有相关的向量都用观察矩阵进行变换（记得也要改变法线矩阵）。
    // 现在我们已经获得所有需要的变量，可以计算高光强度了。首先，我们定义一个镜面强度(Specular Intensity)变量，给镜面高光一个中等亮度颜色，让它不要产生过度的影响。
    // 计算视线方向向量，和对应的沿着法线轴的反射向量：
   
    // 通过法向量和光源方向向量，计算一个反射光源的向量（片段到反射光源的向量）；注意：reflect 函数要求第一个向量是从光源指向片段位置的向量，但是lightDir当前正好相反，是从片段指向光源
    vec3 reflectDir = reflect(-lightDir, normal);
    
    // 通过摄像机方向向量和反射光源向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明镜面光照影响越大
    // 我们先计算视线方向与反射方向的点乘（并确保它不是负值），然后取它的32次幂。这个32是高光的反光度(Shininess)。一个物体的反光度越高，反射光的能力越强，散射得越少，高光点就会越小。在下面的图片里，你会看到不同反光度的视觉效果影响：https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_shininess.png
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    // 合并结果：物体颜色分量 = 光照分量强度 * 材质颜色的反射分量
    vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, TexCoords));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));
    
    return (ambient + diffuse + specular);
}


/// 计算点光源对片段颜色的贡献
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    // 光的方向向量是光源位置向量与片段位置向量之间的向量差（光源位置就是灯的位置，片段位置就是变换后的顶点位置，他们都是以世界坐标系原点出发的向量，所以光源位置向量减去片段位置向量是等于片段到光源的向量，虽然反向是反的，但是可以平移该向量让他的起始点与法向量起始点交接在一起）
    vec3 lightDir = normalize(light.position - fragPos);
    
    /// 漫反射着色
    // https://learnopengl-cn.github.io/img/02/02/diffuse_light.png
    // 通过让法向量和光源方向向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明漫反射光照影响越大
    // 如果两个向量之间的角度大于90度，点乘的结果就会变成负数，这样会导致漫反射分量变为负数。为此，我们使用max函数返回两个参数之间较大的参数，从而保证漫反射分量不会变成负数。负数颜色的光照是没有定义的，所以最好避免它，除非你是那种古怪的艺术家。
    float diff = max(dot(normal, lightDir), 0.0);
    
    /// 镜面光着色
    // https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_theory.png
    // 我们通过反射法向量周围光的方向来计算反射向量。然后我们计算反射向量和视线方向的角度差，如果夹角越小，那么镜面光的影响就会越大。它的作用效果就是，当我们去看光被物体所反射的那个方向的时候，我们会看到一个高光。
    // 观察向量是镜面光照附加的一个变量，我们可以使用观察者世界空间位置和片段的位置来计算它。之后，我们计算镜面光强度，用它乘以光源的颜色，再将它加上环境光和漫反射分量。
    // 我们选择在世界空间进行光照计算，但是大多数人趋向于在观察空间进行光照计算。在观察空间计算的好处是，观察者的位置总是(0, 0, 0)，所以这样你直接就获得了观察者位置。可是我发现在学习的时候在世界空间中计算光照更符合直觉。如果你仍然希望在观察空间计算光照的话，你需要将所有相关的向量都用观察矩阵进行变换（记得也要改变法线矩阵）。
    // 现在我们已经获得所有需要的变量，可以计算高光强度了。首先，我们定义一个镜面强度(Specular Intensity)变量，给镜面高光一个中等亮度颜色，让它不要产生过度的影响。
    // 计算视线方向向量，和对应的沿着法线轴的反射向量：
   
    // 通过法向量和光源方向向量，计算一个反射光源的向量（片段到反射光源的向量）；注意：reflect 函数要求第一个向量是从光源指向片段位置的向量，但是lightDir当前正好相反，是从片段指向光源
    vec3 reflectDir = reflect(-lightDir, normal);
    
    // 通过摄像机方向向量和反射光源向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明镜面光照影响越大
    // 我们先计算视线方向与反射方向的点乘（并确保它不是负值），然后取它的32次幂。这个32是高光的反光度(Shininess)。一个物体的反光度越高，反射光的能力越强，散射得越少，高光点就会越小。在下面的图片里，你会看到不同反光度的视觉效果影响：https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_shininess.png
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    
    /// 衰减
    // 光源衰减值，先快后慢的衰减 http://wiki.ogre3d.org/tiki-index.php?page=-Point+Light+Attenuation
    // http://wiki.ogre3d.org/img/wiki_up/Attenuation_Graph.jpg
    // 在这里d代表了片段距光源的距离。接下来为了计算衰减值，我们定义3个（可配置的）项：常数项Kc、一次项Kl和二次项Kq。
    // 常数项通常保持为1.0，它的主要作用是保证分母永远不会比1小，否则的话在某些距离上它反而会增加强度，这肯定不是我们想要的效果。
    // 一次项会与距离值相乘，以线性的方式减少强度。
    // 二次项会与距离的平方相乘，让光源以二次递减的方式减少强度。二次项在距离比较小的时候影响会比一次项小很多，但当距离值比较大的时候它就会比一次项更大了
    
    // 计算点光源与片段之间的距离
    float distance    = length(light.position - fragPos);
    // 根据点光源衰减公式计算出衰减值
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    // 合并结果：物体颜色分量 = 光照分量强度 * 材质颜色的反射分量
    vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, TexCoords));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));
    
    // 应用光照衰减
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;
    
    return (ambient + diffuse + specular);
}


/// 计算聚光对片段颜色的贡献
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    // 光的方向向量是光源位置向量与片段位置向量之间的向量差（光源位置就是灯的位置，片段位置就是变换后的顶点位置，他们都是以世界坐标系原点出发的向量，所以光源位置向量减去片段位置向量是等于片段到光源的向量，虽然反向是反的，但是可以平移该向量让他的起始点与法向量起始点交接在一起）
    vec3 lightDir = normalize(light.position - fragPos);
    
    /// 漫反射着色
    // https://learnopengl-cn.github.io/img/02/02/diffuse_light.png
    // 通过让法向量和光源方向向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明漫反射光照影响越大
    // 如果两个向量之间的角度大于90度，点乘的结果就会变成负数，这样会导致漫反射分量变为负数。为此，我们使用max函数返回两个参数之间较大的参数，从而保证漫反射分量不会变成负数。负数颜色的光照是没有定义的，所以最好避免它，除非你是那种古怪的艺术家。
    float diff = max(dot(normal, lightDir), 0.0);
    
    
    /// 镜面光着色
    // https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_theory.png
    // 我们通过反射法向量周围光的方向来计算反射向量。然后我们计算反射向量和视线方向的角度差，如果夹角越小，那么镜面光的影响就会越大。它的作用效果就是，当我们去看光被物体所反射的那个方向的时候，我们会看到一个高光。
    // 观察向量是镜面光照附加的一个变量，我们可以使用观察者世界空间位置和片段的位置来计算它。之后，我们计算镜面光强度，用它乘以光源的颜色，再将它加上环境光和漫反射分量。
    // 我们选择在世界空间进行光照计算，但是大多数人趋向于在观察空间进行光照计算。在观察空间计算的好处是，观察者的位置总是(0, 0, 0)，所以这样你直接就获得了观察者位置。可是我发现在学习的时候在世界空间中计算光照更符合直觉。如果你仍然希望在观察空间计算光照的话，你需要将所有相关的向量都用观察矩阵进行变换（记得也要改变法线矩阵）。
    // 现在我们已经获得所有需要的变量，可以计算高光强度了。首先，我们定义一个镜面强度(Specular Intensity)变量，给镜面高光一个中等亮度颜色，让它不要产生过度的影响。
    // 计算视线方向向量，和对应的沿着法线轴的反射向量：
   
    // 通过法向量和光源方向向量，计算一个反射光源的向量（片段到反射光源的向量）；注意：reflect 函数要求第一个向量是从光源指向片段位置的向量，但是lightDir当前正好相反，是从片段指向光源
    vec3 reflectDir = reflect(-lightDir, normal);
    
    // 通过摄像机方向向量和反射光源向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明镜面光照影响越大
    // 我们先计算视线方向与反射方向的点乘（并确保它不是负值），然后取它的32次幂。这个32是高光的反光度(Shininess)。一个物体的反光度越高，反射光的能力越强，散射得越少，高光点就会越小。在下面的图片里，你会看到不同反光度的视觉效果影响：https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_shininess.png
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    
    /// 衰减
    // 光源衰减值，先快后慢的衰减 http://wiki.ogre3d.org/tiki-index.php?page=-Point+Light+Attenuation
    // http://wiki.ogre3d.org/img/wiki_up/Attenuation_Graph.jpg
    // 在这里d代表了片段距光源的距离。接下来为了计算衰减值，我们定义3个（可配置的）项：常数项Kc、一次项Kl和二次项Kq。
    // 常数项通常保持为1.0，它的主要作用是保证分母永远不会比1小，否则的话在某些距离上它反而会增加强度，这肯定不是我们想要的效果。
    // 一次项会与距离值相乘，以线性的方式减少强度。
    // 二次项会与距离的平方相乘，让光源以二次递减的方式减少强度。二次项在距离比较小的时候影响会比一次项小很多，但当距离值比较大的时候它就会比一次项更大了
    
    // 计算点光源与片段之间的距离
    float distance = length(light.position - fragPos);
    // 根据点光源衰减公式计算出衰减值
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    
    /// 聚光是用一个世界空间位置、一个方向和一个切光角(Cutoff Angle)来表示的，切光角指定了聚光的半径（译注：是圆锥的半径不是距光源距离那个半径）。对于每个片段，我们会计算片段是否位于聚光的切光方向之间（也就是在锥形内），如果是的话，我们就会相应地照亮片段。下面这张图会让你明白聚光是如何工作的：
    // https://learnopengl-cn.github.io/img/02/05/light_casters_spotlight_angles.png
    // LightDir：从片段指向光源的向量。
    // SpotDir：聚光所指向的方向。
    // Phiϕ：指定了聚光半径的切光角。落在这个角度之外的物体都不会被这个聚光所照亮。
    // Thetaθ：LightDir向量和SpotDir向量之间的夹角。在聚光内部的话θ值应该比ϕ值小。
    // 所以我们要做的就是计算LightDir向量和SpotDir向量之间的点积（还记得它会返回两个单位向量夹角的余弦值吗？），并将它与切光角ϕ值对比。
    // spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction));
    
    /// 计算聚光范围和处理 平滑/软化边缘 根据公式 https://learnopengl-cn.github.io/02%20Lighting/05%20Light%20casters/#_9
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    
    // 合并结果：物体颜色分量 = 光照分量强度 * 材质颜色的反射分量
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));
    
    // 应用光照衰减和聚光效果
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    
    return (ambient + diffuse + specular);
}
