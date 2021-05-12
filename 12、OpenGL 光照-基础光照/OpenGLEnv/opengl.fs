#version 330 core
out vec4 FragColor;

in vec3 Normal;// 法向量
in vec3 FragPos;// 片段位置，片段就是指模型变换后的顶点位置

uniform vec3 objectColor;// 物体颜色
uniform vec3 lightColor;// 光源颜色
uniform vec3 lightPos; // 光源位置
uniform vec3 viewPos; // 摄像机或者观察者位置

void main()
{
    /**
     // https://learnopengl-cn.github.io/02%20Lighting/02%20Basic%20Lighting/
     
     冯氏光照模型(Phong Lighting Model)。冯氏光照模型的主要结构由3个分量组成：环境(Ambient)、漫反射(Diffuse)和镜面(Specular)光照。下面这张图展示了这些光照分量看起来的样子：
     https://learnopengl-cn.github.io/img/02/02/basic_lighting_phong.png
     
     环境光照(Ambient Lighting)：即使在黑暗的情况下，世界上通常也仍然有一些光亮（月亮、远处的光），所以物体几乎永远不会是完全黑暗的。为了模拟这个，我们会使用一个环境光照常量，它永远会给物体一些颜色。环境光照影响的是物体的所有面。
     漫反射光照(Diffuse Lighting)：模拟光源对物体的方向性影响(Directional Impact)。它是冯氏光照模型中视觉上最显著的分量。物体的某一部分越是正对着光源，它就会越亮。漫反射光照影响的是物体被光源照射到的面，光源越是正对着物体，物体表面会越亮。
     镜面光照(Specular Lighting)：模拟有光泽物体上面出现的亮点。镜面光照的颜色相比于物体的颜色会更倾向于光的颜色。镜面光照影响的也是物体被光源照射到的面，反射光源越是正对着人眼，物体表面亮点会越亮。
     */
    
    /// 环境光照
    // 环境光照因子
    float ambientStrength = 0.1;
    // 环境光照分量
    vec3 ambient = ambientStrength * lightColor;

//    vec3 result = ambient * objectColor;
//    FragColor = vec4(result, 1.0);
    
    /// 漫反射光照
    // https://learnopengl-cn.github.io/img/02/02/diffuse_light.png
    // 光的方向向量是光源位置向量与片段位置向量之间的向量差（光源位置就是灯的位置，片段位置就是变换后的顶点位置，他们都是以世界坐标系原点出发的向量，所以光源位置向量减去片段位置向量是等于片段到光源的向量，虽然反向是反的，但是可以平移该向量让他的起始点与法向量起始点交接在一起）
    vec3 lightDir = normalize(lightPos - FragPos);
    
    // 标准化法向量，法向量是垂直于平面上的每一个顶点的
    vec3 norm = normalize(Normal);
    
    // 然后通过让法向量和光源方向向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明漫反射光照影响越大
    // 如果两个向量之间的角度大于90度，点乘的结果就会变成负数，这样会导致漫反射分量变为负数。为此，我们使用max函数返回两个参数之间较大的参数，从而保证漫反射分量不会变成负数。负数颜色的光照是没有定义的，所以最好避免它，除非你是那种古怪的艺术家。
    float diff = max(dot(norm, lightDir), 0.0);
    
    // 结果值再乘以光的颜色，得到漫反射分量。两个向量之间的角度越大，漫反射分量就会越小：
    // 漫反射分量
    vec3 diffuse = diff * lightColor;
    
    
    /// 镜面光照
    // https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_theory.png
    // 我们通过反射法向量周围光的方向来计算反射向量。然后我们计算反射向量和视线方向的角度差，如果夹角越小，那么镜面光的影响就会越大。它的作用效果就是，当我们去看光被物体所反射的那个方向的时候，我们会看到一个高光。
    // 观察向量是镜面光照附加的一个变量，我们可以使用观察者世界空间位置和片段的位置来计算它。之后，我们计算镜面光强度，用它乘以光源的颜色，再将它加上环境光和漫反射分量。
    // 我们选择在世界空间进行光照计算，但是大多数人趋向于在观察空间进行光照计算。在观察空间计算的好处是，观察者的位置总是(0, 0, 0)，所以这样你直接就获得了观察者位置。可是我发现在学习的时候在世界空间中计算光照更符合直觉。如果你仍然希望在观察空间计算光照的话，你需要将所有相关的向量都用观察矩阵进行变换（记得也要改变法线矩阵）。
    // 现在我们已经获得所有需要的变量，可以计算高光强度了。首先，我们定义一个镜面强度(Specular Intensity)变量，给镜面高光一个中等亮度颜色，让它不要产生过度的影响。
    
    // 镜面强度 如果我们把它设置为1.0f，我们会得到一个非常亮的镜面光分量，这对于一个珊瑚色的立方体来说有点太多了
    float specularStrength = 0.5;
    
    // 计算视线方向向量，和对应的沿着法线轴的反射向量：
    // 摄像机的方向向量是摄像机位置向量与片段位置向量之间的向量差，即 片段到摄像机（观察者）的向量
    vec3 viewDir = normalize(viewPos - FragPos);
    
    // 通过法向量和光源方向向量，计算一个反射光源的向量（片段到反射光源的向量）；注意：reflect 函数要求第一个向量是从光源指向片段位置的向量，但是lightDir当前正好相反，是从片段指向光源
    vec3 reflectDir = reflect(-lightDir, norm);
    
    // 通过摄像机方向向量和反射光源向量做点乘，就得到两个向量的夹角的余弦值，余弦值越大，说明夹角越小，夹角越小说明镜面光照影响越大
    // 我们先计算视线方向与反射方向的点乘（并确保它不是负值），然后取它的32次幂。这个32是高光的反光度(Shininess)。一个物体的反光度越高，反射光的能力越强，散射得越少，高光点就会越小。在下面的图片里，你会看到不同反光度的视觉效果影响：https://learnopengl-cn.github.io/img/02/02/basic_lighting_specular_shininess.png
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    
    // 镜面光照分量
    vec3 specular = specularStrength * spec * lightColor;
    
    /// 有了环境光照分量，漫反射光照分量和镜面光照分量，我们把它们相加，然后把结果乘以物体的颜色，来获得片段最后的输出颜色。
    vec3 result = (ambient + diffuse + specular) * objectColor;
    FragColor = vec4(result, 1.0);
}
