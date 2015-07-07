#version 130
in vec4 outColor;
in vec2 outTexCoord0;

uniform sampler2D tex;
out vec4 outputColor;

void main()
{
	outputColor = texture(tex, outTexCoord0) * outColor;
}