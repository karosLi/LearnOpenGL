#version 330 core
/**
 片段着色器将来自几何着色器的FragPos、光的位置向量和视锥的远平面值作为输入。这里我们把fragment和光源之间的距离，映射到0到1的范围，把它写入为fragment的深度值。
 */

in vec4 FragPos; // 世界坐标的顶点

uniform vec3 lightPos;// 点光源位置
uniform float far_plane;// 点光源空间，视椎体远界面

void main()
{
    // get distance between fragment and light source - 片段到点光源的距离，片段的点光源的距离范围是 [0,far_plane]
    float lightDistance = length(FragPos.xyz - lightPos);
    
    // map to [0;1] range by dividing by far_plane - 这里我们把fragment和光源之间的距离，映射到0到1的范围
    lightDistance = lightDistance / far_plane;
    
    // write this as modified depth - 写入深度值
    gl_FragDepth = lightDistance;
}
