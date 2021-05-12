#version 330 core
/**
 法线贴图，可以让物体表面显示更多的光照细节
 
 由于法线贴图中存储的法线向量是始终指向正Z的，当法线贴图应用在立方体的正前方时，是没有问题的，如果应用在上方，那么面法线方向与法线贴图中存的法线向量方向不一致（因为法线贴图中的法线向量始终指向正Z），而不能达到预期的视觉效果
 
 所以需要引入切线空间。
 
 切线空间公式推导：没有推导出来，感觉有点难。
 切线空间的理解：
 1、一种建立在每一个顶点上正交坐标系，TBN 坐标线，N 表示顶点法线方向，始终指向 Z 轴，T 表示切线（表示 X 轴），B 表示副切线 （表示 Y 轴）
 2、目的是通过 TBN 矩阵，把世界空间中的点转换到切线空间（因为从法线贴图采样的法线向量本身就是属于切线空间，这样结合外部转入到切线空间的摄像机位置和顶点位置，就可以在切线空间进行计算)
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/04%20Normal%20Mapping/
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

void main()
{
    // obtain normal from normal map in range [0,1] - 从法线贴图获取法线向量
    vec3 normal = texture(normalMap, fs_in.TexCoords).rgb;
    
    // transform normal vector to range [-1,1] - 法线贴图纹理中存的是 [0,1] 的法线向量值，需要转成 [-1,1]
    normal = normalize(normal * 2.0 - 1.0);  // this normal is in tangent space
    
    // 片段着色器使用Blinn-Phong光照模型渲染场景
    // 获取漫反射纹理颜色
    vec3 color = texture(diffuseMap, fs_in.TexCoords).rgb;
    // ambient 环境光照分量 = 环境光照强度 * 材质环境光照反射分量颜色
    vec3 ambient = 0.1 * color;
    // 把从法线贴图采样出来的法线向量与已经被转成切线空间的光源位置和片段位置，一起做计算，求出在切线空间的光源方向
    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;
    // specular
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 specular = vec3(0.2) * spec;
    FragColor = vec4(ambient + diffuse + specular, 1.0);
}
