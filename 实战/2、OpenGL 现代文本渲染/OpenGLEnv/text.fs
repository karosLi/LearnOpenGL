#version 330 core
in vec2 TexCoords;
out vec4 color;

uniform sampler2D text;// 单颜色通道的字形位图纹理
uniform vec3 textColor;// 颜色

void main()
{
    /**
     我们首先从位图纹理中采样颜色值，由于纹理数据中仅存储着红色分量，我们就采样纹理的r分量来作为取样的alpha值。
     通过变换颜色的alpha值，最终的颜色在字形背景颜色上会是透明的，而在真正的字符像素上是不透明的。我们也将RGB颜色与textColor这个uniform相乘，来变换文本颜色。
     */
    // 采样字形的每个纹理像素的透明度
    vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
    color = vec4(textColor, 1.0) * sampled;
}
