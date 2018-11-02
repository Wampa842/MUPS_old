# PmxSharp material directives
## Contents
- [Overview](#overview)
- [Syntax and usage](#syntax-and-usage)
- [Directives](#directives)
	- [set](#set)
	- [copy](#copy)
	- [keyword](#keyword)
	- [rename](#rename)
	- [delete](#delete)
	- [visible](#visible)
	- [queue](#queue)
	- [rendermode](#rendermode)
- [Example](#example)
- [Appendix](#appendix)

## Overview
Material directives are small commands that are executed in order to more completely utilize Unity's Standard shader. They give the artist a greater range of material properties to play with, which in turn gives greater flexibility to the materials they create, beyond the features of regular PMX materials.

When PmxSharp loads a material, it does very few things on its own. Beyond setting the color and texture, not much can be done automatically because the properties of a PMX material and Unity's Standard material don't translate well into each other. Directives provide a simple interface for better integration of PMX models into the graphics of Unity Engine.

## Syntax and usage

	[directive parameter parameter...]

Directives are written in the material's note field and are executed after the material is created (with one exception).

Individual directives must be surrounded by square brackets. The directive name and the parameters are separated by the space character or other whitespace characters. They are case insensitive **unless noted otherwise.** Directives don't necessarily have to be on different lines.

By default, the note field's contents are not parsed. To enable directives, define a block using the `[begin]` and `[end]` keywords. The parser will only look for directives inside the block, by trying to parse anything between a pair of square brackets. There can only be one block in each material's note field. Having more than one `[begin]` keywords will cause an error on import.

If only `[begin]` is defined, the parser will look for directives from that point to the end of the text content. Defining only `[end]` without a `[begin]`, or defining `[begin]` following an `[end]` will cause the `[end]` keyword to be ignored.

Any text that is inside a block, but is not surrounded by square brackets, is ignored.


## Directives
### set
	[set <type> <name> <parameters...>]
Set causes the importer to set the material's property called `<name>` of type `<type>` to the value defined by the rest of the parameters. **The `name` parameter is case-sensitive.** The following types and syntaxes are supported:

|Type|Syntax|Note
|---|---|---
|Float|`[set float name value]`|Correct format is `12.34`. Use decimal point, don't use F suffix.
|Integer|`[set int name value]`
|Color RGBA|`[set color name R G B A]`|Values must be between 0 and 1.
|Color RGB|`[set color name R G B]`|This assumes alpha is 1.
|Texture|`[set texture name path]`\*|This loads the file at `path` and sets it as the specified texture.
|Keyword|`[set keyword name value]`|See below.

<sub>\*: The `path` parameter may contain whitespace characters - it will include the complete string ending with the character before the closing bracket. Leading and trailing whitespaces will be trimmed.</sub>

The `keyword` type is used when a frequently used property needs special attention. Keywords have their own individual types. The following keywords are available:

|Keyword|Type|Note
|---|---|---
|opacity|float|Sets the opacity without affecting the color.

An error will be thrown if the destination's type is not the same as the given type, the parameters cannot be parsed as the given type, or the property cannot be assigned to.

Example:

	[set color _Color 1 0 0 1]
    [set keyword opacity 0.3]
    
The first directive sets the material's main color to red. The second one sets its opacity to 30%.

---
### copy
	[copy <source> <destination>]
Copies the value marked by `source` into the property that has the name `destination`. Mis-matching source and destination types will cause an error. **The `destination` parameter is case-sensitive.** The following keywords (and types) are available:

|Name|Type|Note
|---|---|---
|japanese|string|The Japanese name.
|english|string|The English name.
|diffuse|color|Diffuse color that reacts to light.
|ambient|color|Ambient color that doesn't react to light - used as emissive in MMD.
|emissive|color|Same as `ambient`.
|specular|color|The specular/reflective color.
|smoothness|float|Defines how light is reflected by the specular color.
|exponent|float|Same as `smoothness`.
|roughness|float|Calculated from `smoothness`: `roughness = 1 - smoothness`.
|edge|color|The pencil outline color.
|edgesize|float|The outline's width.
|diffusepath|string|The diffuse texture's path.
|diffusetex|texture|The diffuse texture bitmap.
|spherepath|string|The sphere texture's path.
|spheretex|texture|The sphere texture bitmap.
|sphereblend|integer|Sphere blending mode (0: disabled, 1: multiplicative, 2: additive, 3: sub-texture).
|toonindex|integer|The internal toon texture's index - only if indexed toon is used.
|toonpath|string|The toon texture's path - only if custom toon is used.
|toontex|texture|The toon texture bitmap - only if custom toon is used.

Example:

	[copy exponent _Smoothness]

This copies the PMX material's smoothness to the Standard material's property.

---
### keyword
	[keyword enable|disable|on|off <name>]
Enables or disables the material's specified keyword. **The `name` parameter is case-sensitive.** The following keywords are available for the Standard material:
|Keyword|Note
|---|---
|_EMISSION|Use emissive color for bloom effect.

Example:

	[keyword enable _EMISSION]

This directive enables emission (bloom) on the Standard material.

---
### rename
	[rename value]
Forces the importer to rename the GameObject that holds the material's sub-object to the specified string. This can be useful if the encoding of the material's real name is causing issues.

The `value` parameter may contain whitespace characters - it will include the complete string ending with the character before the closing bracket. Leading and trailing whitespaces will be trimmed.

---
### delete
	[delete]
Causes the sub-object to be skipped completely. Use it if the sub-object has no use outside MMD.

---
### visible
	[visible true|false]
Causes the sub-object to be visible/hidden by enabling/disabling its MeshRenderer component.

### queue
	[queue <number>]
Sets the material's place in the Unity render queue. The higher the value, the later it gets rendered.

Example:

	[queue 2200]

This will cause the material to be rendered after geometry (2000), but before alpha test materials (2450).

---
### rendermode
	[rendermode opaque|cutout|fade|transparent]
    [rendermode cutout|fade|transparent auto]
This directive is executed before the material is created. It decides which render mode should be used for the material and chooses the preset accordingly.

The first variant simply forces the importer to use the specified preset.

The second variant, where the `auto` parameter is defined, makes the importer choose. If the color isn't opaque, or the material has a texture and at least one pixel in it isn't opaque, it uses the preset specified in the second parameter. Otherwise it uses opaque.

**Warning:** automatic detection will cause the importer program to loop through all pixels in the texture. In the case of a 4096-by-4096 texture, that means almost 17 million pixels. Be mindful of performance if you want to use this.

Example:

	[rendermode transparent auto]
	
This will cause the importer to automatically choose between the opaque or transparent presets, which is the importer's default behaviour if the `rendermode` directive is not defined.

## Example
	This is a complete example of a PmxSharp material directive block.
    [begin]
    [rendermode transparent]
    [keyword on _EMISSION]
    [copy ambient _EmissionColor]
    [copy smoothness _Glossiness]
    [set float _Metallic 0.0]
    [set texture _BumpMap textures/main_n.png]
    [set float _BumpScale 0.6]
    [end]
    These directives create a transparent non-metallic emissive normal-mapped material.
	
	
## Appendix
MUPS, by default, uses the Standard Unity shader with the smoothness workflow.

The Standard shader defines the following properties:
|Name|Type|Description
|---|---|---
|_Color|color|The albedo/base color.
|_MainTex|texture|The albedo/base texture.
|_Cutoff|float range (0 to 1)|The alpha threshold between opaque and transparent pixels. **Only in Cutout rendering mode.**
|_Glossiness|float range (0 to 1)|PBR smoothness, defines the reflectiveness of the surface.
|_Metallic|float range (0 to 1)|PBR metallic, defines how the surface reflection blends between dielectric (0) and metallic (1) reflection.
|_MetallicGlossMap|texture|PBR texture that uses two channels to define a pixel's smoothness and metallic values. Metallic is always the alpha channel. See _SmoothnessTextureChannel for smoothness.
|_GlossMapScale|float range (0 to 1)|The scale or strength of the metallic-smoothness texture.
|_SpecularHighlights|float|?
|_GlossyReflections|float|?
|_BumpMap|texture|Normal map in the +Y (OpenGL) format (red points right, green points up).
|_BumpScale|float|Normal map strength.
|_ParallaxMap|texture|Height map.
|_Parallax|float range (0 to 1)|Height map scale.
|_OcclusionMap|texture|Occlusion texture.
|_OcclusionStrength|float range (0 to 1)|Occlusion map strength.
|_EmissionColor|color|Emissive (self-illumination) color.
|_EmissionMap|texture|Emissive color texture.
|_DetailMask|texture|Clipping mask for the detail textures.
|_DetailAlbedoMap|texture|Detail base color texture.
|_DetailNormalMap|texture|Detail normal map.
|_DetailNormalMapScale|float|Detail normal map strength.
|_UVSec|float|Additional UV set for the detail maps. The number's integer part defines which UV set to use.
|_Mode|float|Integer part defines the rendering mode. 0: opaque, 1: cutoff, 2: fade, 3: transparent. Has no effect on its own.
|_SrcBlend|float|Defines how the texture is blended on render. Used when setting the rendering mode.
|_DstBlend|float|Similar in function to _SrcBlend.
|_ZWrite|float|Defines whether the shader should write into the depth buffer. Zero means the shader doesn't write into the depth buffer, which is used in transparent materials.


The Standard shader has the following keywords:
|Name|Enabled by default|Description
|---|---|---
|_EMISSION|no|Enables emissive (self-illumination) color and texture.