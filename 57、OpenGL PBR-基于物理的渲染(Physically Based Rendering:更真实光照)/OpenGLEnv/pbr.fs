#version 330 core
/**
 PBR
 基于物理的渲染(Physically Based Rendering)
 基于物理的渲染仍然只是对基于物理原理的现实世界的一种近似，这也就是为什么它被称为基于物理的着色(Physically based Shading) 而非物理着色(Physical Shading)的原因。判断一种PBR光照模型是否是基于物理的，必须满足以下三个条件（不用担心，我们很快就会了解它们的）：

 - 基于微平面(Microfacet)的表面模型。
 - 能量守恒。
 - 应用基于物理的BRDF（一种基于物理的一组纹理列表）。
 
 
 
 重要名词：
 - 立体角(Solid Angle)(描述的是光源)
    投射到单位球体上的一个截面的大小或者面积(比如5平方米)
 - 辐射通量(Radiant flux)(描述的是光源)
    辐射通量Φ表示的是一个光源向某个立体角（比如5平方米）所输出的能量，单位为瓦特
 - 辐射强度(Radiant Intensity)(描述的是光源)
    单位球面上，一个光源向每单位立体角（每平方米）所投送的辐射通量，单位为瓦特/平方米。即单位化的辐射通量。公式：辐射强度 = 辐射通量 / 立体角 https://learnopengl-cn.github.io/img/07/01/radiance.png
 - 辐射率(Radiance)(描述的是被辐射的物体)
    一个某个大小的辐射强度光源在面积A（可以看作点p），立体角ω上的辐射出的总能量，即量化单一方向上发射来的光线的强度。公式：L=d2Φ/dAdωcosθ https://learnopengl-cn.github.io/img/07/01/radiance.png
 - 辐照度(Irradiance)(描述的是被辐射的物体)
    所有投射到点p上的光线（辐射率）的总和
 - 反射率公式
    1、围绕所有入射辐射率的总和，也就是辐照度来计算，所以我们需要计算的就不只是是单一的一个方向上的入射光，而是一个以点p为球心的半球领域Ω内所有方向上的入射光。一个半球领域(Hemisphere)可以描述为以平面法线n为轴所环绕的半个球体：https://learnopengl-cn.github.io/img/07/01/hemisphere.png
    2、先计算所有投射到点p上的光线（辐射率）的总和，再计算所有从点p反射出的（反射方向/观察方向：ωo）光线（辐射率）的总和

 
 反射率方程（单条光线投射点p而出射的辐射率）：
 Lo(p,ωo)=∫fr(p,ωi,ωo)Li(p,ωi)n⋅ωi*dωi
          Ω
 
 变量拆解：
 i：入射
 o：出射/反射
 ωi：入射方向
 ωo：反射方向
 p：光线投射点
 fr(p,ωi,ωo)：BRDF，双向反射分布函数(Bidirectional Reflective Distribution Function) ，它的作用是基于表面材质属性来对入射辐射率进行缩放或者加权
 Li(p,ωi)：光线从ωi方向投射到点p上的辐射率，辐射率的大小受入射角影响，越是接近垂直，辐射率越大
 n⋅ωi：入射角，即半球法线与入射方向的夹角cos值
 dωi：立体角，投射到单位球体上的一个截面的大小或者面积，可以想象成带有体积的方向
 Ω：半球领域
 Lo(p,ωo)：所有从点p反射出的（反射方向/观察方向：ωo）光线（辐射率）的总和
 
 
 完整的 Cook-Torrance 反射率方程（单条光线投射点p而出射的辐射率）：
 Lo(p,ωo)=∫(kd*c/π+ks*DFG/4(ωo⋅n)(ωi⋅n))Li(p,ωi)n⋅ωi*dωi
          Ω
 变量拆解：
 kd：光能折射能量比率
 c：反照率
 π：圆周率
 ks：光能反射能量比率
 DFG：正态分布函数*几何函数*菲涅尔方程
 n⋅ωi/ωi⋅n：入射角，即半球法线与入射方向的夹角cos值
 ωo⋅n：出射角，，即半球法线与出射方向的夹角cos值
 Li(p,ωi)：光线从ωi方向投射到点p上的辐射率，辐射率的大小受入射角影响，越是接近垂直，辐射率越大
 dωi：立体角，投射到单位球体上的一个截面的大小或者面积，可以想象成带有体积的方向
 Ω：半球领域
 Lo(p,ωo)：所有从点p反射出的（反射方向/观察方向：ωo）光线（辐射率）的总和
 
 
 
 
 下面只做了解：
 
 事实上，当涉及到辐射率时，我们通常关心的是所有投射到点p上的光线的总和，而这个和就称为辐射照度或者辐照度(Irradiance)。在理解了辐射率和辐照度的概念之后，让我们再回过头来看看反射率方程：

 Lo(p,ωo)=∫Ωfr(p,ωi,ωo)Li(p,ωi)n⋅ωidωi
 我们知道在渲染方程中L代表通过某个无限小的立体角ωi在某个点上的辐射率，而立体角可以视作是入射方向向量ωi。注意我们利用光线和平面间的入射角的余弦值cosθ来计算能量，亦即从辐射率公式L转化至反射率公式时的n⋅ωi。用ωo表示观察方向，也就是出射方向，反射率公式计算了点p在ωo方向上被反射出来的辐射率Lo(p,ωo)的总和。或者换句话说：Lo表示了从ωo方向上观察，光线投射到点p上反射出来的辐照度。

 基于反射率公式是围绕所有入射辐射率的总和，也就是辐照度来计算的，所以我们需要计算的就不只是是单一的一个方向上的入射光，而是一个以点p为球心的半球领域Ω内所有方向上的入射光。一个半球领域(Hemisphere)可以描述为以平面法线n为轴所环绕的半个球体：
 https://learnopengl-cn.github.io/img/07/01/hemisphere.png
 
 反射率方程概括了在半球领域Ω内，碰撞到了点p上的所有入射方向ωi 上的光线的辐射率，并受到fr的约束，然后返回观察方向上反射光的Lo
 
 
 BRDF
 双向反射分布函数，它接受入射（光）方向ωi，出射（观察）方向ωo，平面法线n以及一个用来表示微平面粗糙程度的参数a作为函数的输入参数。
 BRDF可以近似的求出每束光线对一个给定了材质属性的平面上最终反射出来的光线所作出的贡献程度。举例来说，如果一个平面拥有完全光滑的表面（比如镜面），那么对于所有的入射光线ωi（除了一束以外）而言BRDF函数都会返回0.0 ，只有一束与出射光线ωo拥有相同（被反射）角度的光线会得到1.0这个返回值。
 BRDF基于我们之前所探讨过的微平面理论来近似的求得材质的反射与折射属性。对于一个BRDF，为了实现物理学上的可信度，它必须遵守能量守恒定律，也就是说反射光线的总和永远不能超过入射光线的总量。严格上来说，同样采用ωi和ωo作为输入参数的 Blinn-Phong光照模型也被认为是一个BRDF。然而由于Blinn-Phong模型并没有遵循能量守恒定律，因此它不被认为是基于物理的渲染。现在已经有很好几种BRDF都能近似的得出物体表面对于光的反应，但是几乎所有实时渲染管线使用的都是一种被称为Cook-Torrance BRDF模型。
 
 
 Cook-Torrance BRDF兼有漫反射和镜面反射两个部分：
 fr=kd*flambert+ks*fcook−torrance
 这里的kd是早先提到过的入射光线中被折射部分的能量所占的比率，而ks是被反射部分的比率。BRDF的左侧表示的是漫反射部分，这里用flambert来表示。它被称为Lambertian漫反射，这和我们之前在漫反射着色中使用的常数因子类似，用如下的公式来表示：

 
 flambert=c/π
 c表示表面颜色（回想一下漫反射表面纹理）。除以π是为了对漫反射光进行标准化，因为前面含有BRDF的积分方程是受π影响的（我们会在IBL的教程中探讨这个问题的）。
 目前存在着许多不同类型的模型来实现BRDF的漫反射部分，大多看上去都相当真实，但是相应的运算开销也非常的昂贵。不过按照Epic公司给出的结论，Lambertian漫反射模型已经足够应付大多数实时渲染的用途了。

 
 BRDF的镜面反射部分要稍微更高级一些，它的形式如下所示：
 fcook−torrance=DFG4(ωo⋅n)(ωi⋅n)
 
 Cook-Torrance BRDF的镜面反射部分包含三个函数，此外分母部分还有一个标准化因子 。字母D，F与G分别代表着一种类型的函数，各个函数分别用来近似的计算出表面反射特性的一个特定部分。三个函数分别为正态分布函数(Normal Distribution Function)，菲涅尔方程(Fresnel Rquation)和几何函数(Geometry Function)：
 - 正态分布函数：估算在受到表面粗糙度的影响下，取向方向与中间向量一致的微平面的数量。这是用来估算微平面的主要函数。
 - 几何函数：描述了微平面自成阴影的属性。当一个平面相对比较粗糙的时候，平面表面上的微平面有可能挡住其他的微平面从而减少表面所反射的光线。
 - 菲涅尔方程：菲涅尔方程描述的是在不同的表面角下表面所反射的光线所占的比率。
 
 
 以上的每一种函数都是用来估算相应的物理参数的，而且你会发现用来实现相应物理机制的每种函数都有不止一种形式。它们有的非常真实，有的则性能高效。你可以按照自己的需求任意选择自己想要的函数的实现方法。英佩游戏公司的Brian Karis对于这些函数的多种近似实现方式进行了大量的研究（http://graphicrants.blogspot.nl/2013/08/specular-brdf-reference.html）。我们将会采用Epic Games在Unreal Engine 4中所使用的函数，其中D使用Trowbridge-Reitz GGX，F使用Fresnel-Schlick近似(Fresnel-Schlick Approximation)，而G使用Smith’s Schlick-GGX。
 
 
 正态分布函数（法线分布函数，更好理解）：计算微平面上与中间向量H（在观察方向向量与光线方向向量的中间向量的一半）取向一致的比率
 正态分布函数D，或者说镜面分布，从统计学上近似的表示了与某些（中间）向量h取向一致的微平面的比率。举例来说，假设给定向量h，如果我们的微平面中有35%与向量h取向一致，则正态分布函数或者说NDF将会返回0.35。目前有很多种NDF都可以从统计学上来估算微平面的总体取向度，只要给定一些粗糙度的参数以及一个我们马上将会要用到的参数Trowbridge-Reitz GGX：
 NDFGGXTR(n,h,α)=α2π((n⋅h)2(α2−1)+1)2
 在这里h表示用来与平面上微平面做比较用的中间向量，而a表示表面粗糙度。

 如果我们把h当成是不同粗糙度参数下，平面法向量和光线方向向量之间的中间向量的话，我们可以得到如下图示的效果：
 https://learnopengl-cn.github.io/img/07/01/ndf.png
 当粗糙度很低（也就是说表面很光滑）的时候，与中间向量取向一致的微平面会高度集中在一个很小的半径范围内。由于这种集中性，NDF最终会生成一个非常明亮的斑点。但是当表面比较粗糙的时候，微平面的取向方向会更加的随机。你将会发现与h向量取向一致的微平面分布在一个大得多的半径范围内，但是同时较低的集中性也会让我们的最终效果显得更加灰暗。
 
 
 
 几何函数：计算微平面上相互遮蔽的比率
 几何函数从统计学上近似的求得了微平面间相互遮蔽的比率，这种相互遮蔽会损耗光线的能量。
 https://learnopengl-cn.github.io/img/07/01/geometry_shadowing.png
 与NDF类似，几何函数采用一个材料的粗糙度参数作为输入参数，粗糙度较高的表面其微平面间相互遮蔽的概率就越高。我们将要使用的几何函数是GGX与Schlick-Beckmann近似的结合体，因此又称为Schlick-GGX：
 GSchlickGGX(n,v,k)=n⋅v(n⋅v)(1−k)+k
 这里的k是α基于几何函数是针对直接光照还是针对IBL光照的重映射(Remapping) :

 kdirect=(α+1)28
 kIBL=α22
 注意，根据你的引擎把粗糙度转化为α的方式不同，得到α的值也有可能不同。在接下来的教程中，我们将会广泛的讨论这个重映射是如何起作用的。
 为了有效的估算几何部分，需要将观察方向（几何遮蔽(Geometry Obstruction)）和光线方向向量（几何阴影(Geometry Shadowing)）都考虑进去。我们可以使用史密斯法(Smith’s method)来把两者都纳入其中：

 G(n,v,l,k)=Gsub(n,v,k)Gsub(n,l,k)
 使用史密斯法与Schlick-GGX作为Gsub可以得到如下所示不同粗糙度的视觉效果：
 https://learnopengl-cn.github.io/img/07/01/geometry.png
 几何函数是一个值域为[0.0, 1.0]的乘数，其中白色或者说1.0表示没有微平面阴影，而黑色或者说0.0则表示微平面彻底被遮蔽。


 
 菲涅尔方程：计算被反射光线与折射光线的比率
 菲涅尔（发音为Freh-nel）方程描述的是被反射的光线对比光线被折射的部分所占的比率，这个比率会随着我们观察的角度不同而不同。当光线碰撞到一个表面的时候，菲涅尔方程会根据观察角度告诉我们被反射的光线所占的百分比。利用这个反射比率和能量守恒原则，我们可以直接得出光线被折射的部分以及光线剩余的能量。

 当垂直观察的时候，任何物体或者材质表面都有一个基础反射率(Base Reflectivity)，但是如果以一定的角度往平面上看的时候所有反光都会变得明显起来。你可以自己尝试一下，用垂直的视角观察你自己的木制/金属桌面，此时一定只有最基本的反射性。但是如果你从近乎90度（译注：应该是指和法线的夹角）的角度观察的话反光就会变得明显的多。如果从理想的90度视角观察，所有的平面理论上来说都能完全的反射光线。这种现象因菲涅尔而闻名，并体现在了菲涅尔方程之中。

 菲涅尔方程是一个相当复杂的方程式，不过幸运的是菲涅尔方程可以用Fresnel-Schlick近似法求得近似解：

 FSchlick(h,v,F0)=F0+(1−F0)(1−(h⋅v))5
 F0表示平面的基础反射率，它是利用所谓折射指数(Indices of Refraction)或者说IOR计算得出的。然后正如你可以从球体表面看到的那样，我们越是朝球面掠角的方向上看（此时视线和表面法线的夹角接近90度）菲涅尔现象就越明显，反光就越强：
 https://learnopengl-cn.github.io/img/07/01/fresnel.png
 菲涅尔方程还存在一些细微的问题。其中一个问题是Fresnel-Schlick近似仅仅对电介质或者说非金属表面有定义。对于导体(Conductor)表面（金属），使用它们的折射指数计算基础折射率并不能得出正确的结果，这样我们就需要使用一种不同的菲涅尔方程来对导体表面进行计算。由于这样很不方便，所以我们预先计算出平面对于法向入射（F0）的反应（处于0度角，好像直接看向表面一样）然后基于相应观察角的Fresnel-Schlick近似对这个值进行插值，用这种方法来进行进一步的估算。这样我们就能对金属和非金属材质使用同一个公式了。

 平面对于法向入射的响应或者说基础反射率可以在一些大型数据库中找到，比如这个（http://refractiveindex.info/）。下面列举的这一些常见数值就是从Naty Hoffman的课程讲义中所得到的：
 这里可以观察到的一个有趣的现象，所有电介质材质表面的基础反射率都不会高于0.17，这其实是例外而非普遍情况。导体材质表面的基础反射率起点更高一些并且（大多）在0.5和1.0之间变化。此外，对于导体或者金属表面而言基础反射率一般是带有色彩的，这也是为什么F0要用RGB三原色来表示的原因（法向入射的反射率可随波长不同而不同）。这种现象我们只能在金属表面观察的到。
 
 金属表面这些和电介质表面相比所独有的特性引出了所谓的金属工作流的概念。也就是我们需要额外使用一个被称为金属度(Metalness)的参数来参与编写表面材质。金属度用来描述一个材质表面是金属还是非金属的。
 
 通过预先计算电介质与导体的F0值，我们可以对两种类型的表面使用相同的Fresnel-Schlick近似，但是如果是金属表面的话就需要对基础反射率添加色彩。
 我们为大多数电介质表面定义了一个近似的基础反射率。F0取最常见的电解质表面的平均值，这又是一个近似值。不过对于大多数电介质表面而言使用0.04作为基础反射率已经足够好了，而且可以在不需要输入额外表面参数的情况下得到物理可信的结果。然后，基于金属表面特性，我们要么使用电介质的基础反射率要么就使用F0来作为表面颜色。因为金属表面会吸收所有折射光线而没有漫反射，所以我们可以直接使用表面颜色纹理来作为它们的基础反射率。


 Cook-Torrance反射率方程
 随着Cook-Torrance BRDF中所有元素都介绍完毕，我们现在可以将基于物理的BRDF纳入到最终的反射率方程当中去了：

 Lo(p,ωo)=∫Ω(kdcπ+ksDFG4(ωo⋅n)(ωi⋅n))Li(p,ωi)n⋅ωidωi
 这个方程现在完整的描述了一个基于物理的渲染模型，它现在可以认为就是我们一般意义上理解的基于物理的渲染也就是PBR。如果你还没有能完全理解我们将如何把所有这些数学运算结合到一起并融入到代码当中去的话也不必担心。在下一个教程当中，我们将探索如何实现反射率方程来在我们渲染的光照当中获得更加物理可信的结果，而所有这些零零星星的碎片将会慢慢组合到一起来。
 
 
 
 编写PBR材质
 在了解了PBR后面的数学模型之后，最后我们将通过说明美术师一般是如何编写一个我们可以直接输入PBR的平面物理属性的来结束这部分的讨论。PBR渲染管线所需要的每一个表面参数都可以用纹理来定义或者建模。使用纹理可以让我们逐个片段的来控制每个表面上特定的点对于光线是如何响应的：不论那个点是金属的，粗糙或者平滑，也不论表面对于不同波长的光会有如何的反应。

 在下面你可以看到在一个PBR渲染管线当中经常会碰到的纹理列表，还有将它们输入PBR渲染器所能得到的相应的视觉输出：
 https://learnopengl-cn.github.io/img/07/01/textures.png
 
 
 反照率：反照率(Albedo)纹理为每一个金属的纹素(Texel)（纹理像素）指定表面颜色或者基础反射率。这和我们之前使用过的漫反射纹理相当类似，不同的是所有光照信息都是由一个纹理中提取的。漫反射纹理的图像当中常常包含一些细小的阴影或者深色的裂纹，而反照率纹理中是不会有这些东西的。它应该只包含表面的颜色（或者折射吸收系数）。

 法线：法线贴图纹理和我们之前在法线贴图教程中所使用的贴图是完全一样的。法线贴图使我们可以逐片段的指定独特的法线，来为表面制造出起伏不平的假象。

 金属度：金属(Metallic)贴图逐个纹素的指定该纹素是不是金属质地的。根据PBR引擎设置的不同，美术师们既可以将金属度编写为灰度值又可以编写为1或0这样的二元值。

 粗糙度：粗糙度(Roughness)贴图可以以纹素为单位指定某个表面有多粗糙。采样得来的粗糙度数值会影响一个表面的微平面统计学上的取向度。一个比较粗糙的表面会得到更宽阔更模糊的镜面反射（高光），而一个比较光滑的表面则会得到集中而清晰的镜面反射。某些PBR引擎预设采用的是对某些美术师来说更加直观的光滑度(Smoothness)贴图而非粗糙度贴图，不过这些数值在采样之时就马上用（1.0 – 光滑度）转换成了粗糙度。

 AO：环境光遮蔽(Ambient Occlusion)贴图或者说AO贴图为表面和周围潜在的几何图形指定了一个额外的阴影因子。比如如果我们有一个砖块表面，反照率纹理上的砖块裂缝部分应该没有任何阴影信息。然而AO贴图则会把那些光线较难逃逸出来的暗色边缘指定出来。在光照的结尾阶段引入环境遮蔽可以明显的提升你场景的视觉效果。网格/表面的环境遮蔽贴图要么通过手动生成，要么由3D建模软件自动生成。

 美术师们可以在纹素级别设置或调整这些基于物理的输入值，还可以以现实世界材料的表面物理性质来建立他们的材质数据。这是PBR渲染管线最大的优势之一，因为不论环境或者光照的设置如何改变这些表面的性质是不会改变的，这使得美术师们可以更便捷的获取物理可信的结果。在PBR渲染管线中编写的表面可以非常方便的在不同的PBR渲染引擎间共享使用，不论处于何种环境中它们看上去都会是正确的，因此看上去也会更自然。
 
 
 
 
 
 
 https://learnopengl-cn.github.io/07%20PBR/01%20Theory/
 https://learnopengl-cn.github.io/07%20PBR/02%20Lighting/
 */

out vec4 FragColor;// 输出片段颜色
in vec2 TexCoords;// 纹理坐标
in vec3 WorldPos;// 世界空间中的片段位置，即点p
in vec3 Normal;// 世界空间中的法向量

// material parameters - 材质参数
uniform vec3 albedo;// 反照率(Albedo) - 平面颜色或者基础反射率
uniform float metallic; // 金属度(Metallic) - 金属质地
uniform float roughness;// 粗糙度(Roughness) - 越粗糙，镜面反射高光更宽阔且更模糊，否则更集中且清晰
uniform float ao;// 环境光遮蔽(Ambient Occlusion) - 额外阴影因子

// lights - 点光源位置和颜色
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

uniform vec3 camPos;// 摄像机位置

const float PI = 3.14159265359; // 圆周率
// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / max(denom, 0.0000001); // prevent divide by zero for roughness=0.0 and NdotH=1.0
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}
// ----------------------------------------------------------------------------
void main()
{
    // 半球领域法向量
    vec3 N = normalize(Normal);
    // 点p到摄像机位置的方向向量
    vec3 V = normalize(camPos - WorldPos);

    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0
    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)
    /**
     计算基础反射率，用于菲涅尔方程：计算被反射光线与折射光线的比率
     
     mix这个函数是GLSL中一个特殊的线性插值函数，他将前两个参数的值基于第三个参数按照以下公式进行插值：genType mix (genType x, genType y, float a) 返回线性混合的x和y，如：x⋅(1−a)+y⋅a。
     */
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    
    /**
     我们会采用总共4个点光源来直接表示场景的辐照度。为了满足反射率方程，我们循环遍历每一个光源，计算他们独立的辐射率然后求和，接着根据BRDF和光源的入射角来缩放该辐射率。
     
     完整的 Cook-Torrance 反射率方程：
     Lo(p,ωo)=∫(kd*c/π+ks*DFG/4(ωo⋅n)(ωi⋅n))Li(p,ωi)n⋅ωi*dωi
              Ω
     
     Cook-Torrance BRDF兼有漫反射和镜面反射两个部分：
     fr=kd*flambert+ks*fcook−torrance
     
     flambert=c/π
     fcook−torrance=DFG/4(ωo⋅n)(ωi⋅n)
     
     */
    // reflectance equation - 反射率方程
    vec3 Lo = vec3(0.0);
    for(int i = 0; i < 4; ++i)// 4个点光源
    {
        // calculate per-light radiance - 计算每个光源的辐射率
        // 点p到光源的方向向量
        vec3 L = normalize(lightPositions[i] - WorldPos);
        // 光源和摄像机之间的半程向量（始于点p）
        vec3 H = normalize(V + L);
        // 光源和点p的距离
        float distance = length(lightPositions[i] - WorldPos);
        
        // 光源衰减因子
        float attenuation = 1.0 / (distance * distance);
        // 初始化辐射率
        vec3 radiance = lightColors[i] * attenuation;
        
        /**
         fr(p,ωi,ωo)：BRDF，双向反射分布函数(Bidirectional Reflective Distribution Function) ，兼有漫反射和镜面反射两个部分，它的作用是基于表面材质属性来对入射辐射率进行缩放或者加权。
         
         Cook-Torrance BRDF - 计算完整的 Cook-Torrance specular BRDF项，公式如下：
         
         fr=kd*flambert+ks*fcook−torrance
         
         flambert=c/π
         fcook−torrance=DFG/4(ωo⋅n)(ωi⋅n)
         */
        
        // 正态分布函数（法线分布函数，更好理解）：计算微平面上与中间向量H（在观察方向向量与光线方向向量的半程向量）取向一致的比率
        float NDF = DistributionGGX(N, H, roughness);
        // 几何函数：计算微平面上相互遮蔽的比率
        float G   = GeometrySmith(N, V, L, roughness);
        // 菲涅尔方程：计算被反射光线与折射光线的比率
        vec3 F    = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);
        
        // Cook-Torrance BRDF 方程的分子
        vec3 nominator    = NDF * G * F;
        // Cook-Torrance BRDF 方程的分母
        float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
        // 计算 fcook−torrance 得到 BRDF 镜面反射部分
        vec3 specular = nominator / max(denominator, 0.001); // prevent divide by zero for NdotV=0.0 or NdotL=0.0 - 注意我们在分母项中加了一个0.001为了避免出现除零错误
        
        // kS is equal to Fresnel - kS 是光能反射能量比率
        vec3 kS = F;
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS; // 根据能量守恒，得到是光能折射能量比率 kD = 1 - kS
        // multiply kD by the inverse metalness such that only non-metals
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        kD *= 1.0 - metallic;// 因为金属不会折射光线，因此不会有漫反射。所以如果表面是金属[0,1]的，我们会把系数kD变为0

        // scale light by NdotL - 计算入射角的cos值，越是接近垂直，辐射率越大，否则越小
        float NdotL = max(dot(N, L), 0.0);

        // BRDF 漫反射 = kD * albedo / PI。BRDF = 漫反射 + 镜面反射
        // add to outgoing radiance Lo - 计算 BRDF 和 入射角缩放辐射率
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;  // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
    }
    /**
     计算每个光源对点p出射光线的辐射率，相加就得到了所有光源对点p出射光线的辐照度
     
     最终的结果Lo，或者说是出射光线的辐射率，实际上是反射率方程的在半球领域Ω的积分的结果。但是我们实际上不需要去求积，因为对于所有可能的入射光线方向我们知道只有4个方向的入射光线会影响片段(像素)的着色。因为这样，我们可以直接循环N次计算这些入射光线的方向(N也就是场景中光源的数目)。
     */
    
    
    // ambient lighting (note that the next IBL tutorial will replace
    // this ambient lighting with environment lighting).
    // 根据反照率和遮光因子添加一个环境光照
    vec3 ambient = vec3(0.03) * albedo * ao;

    // 辐射度与环境光照相加得到物体颜色
    vec3 color = ambient + Lo;

    // HDR tonemapping - HDR (高动态范围)色调映射到 LDR (低动态范围，显示器)
    color = color / (color + vec3(1.0));
    // gamma correct - 伽马校正提升图像对比度
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, 1.0);
}
