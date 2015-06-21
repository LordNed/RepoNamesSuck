#version 330

// Vertex Data Input
in vec3 vertexPos;
in vec3 color0;
in vec2 texCoord0;

// Output
out vec4 outColor;
out vec2 outTexCoord0;

uniform mat4 modelview;
uniform vec3 inColor;

void main()
{
	outColor = vec4(color0,1);
	outTexCoord0 = texCoord0;

	gl_Position = modelview * vec4(vertexPos, 1.0);
}