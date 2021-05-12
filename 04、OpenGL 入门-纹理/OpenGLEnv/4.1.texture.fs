#version 330 core
out vec4 FragColor;

in vec3 ourColor;
in vec2 TexCoord;

// texture samplers
uniform sampler2D texture1;
uniform sampler2D texture2;

void main()
{
    /**
     纹理颜色与顶点颜色混合
     */
//    FragColor = texture(texture1, TexCoord) * vec4(ourColor, 1.0);
    
    // linearly interpolate between both textures (80% container, 20% awesomeface)
    /**
     GLSL内建的mix函数需要接受两个值作为参数，并对它们根据第三个参数进行线性插值。如果第三个值是0.0，它会返回第一个输入；如果是1.0，会返回第二个输入值。0.2会返回80%的第一个输入颜色和20%的第二个输入颜色，即返回两个纹理的混合色。
     */
    FragColor = mix(texture(texture1, TexCoord), texture(texture2, TexCoord), 0.2);
}
