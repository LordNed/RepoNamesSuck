#version 330

in vec3 vertexPos;
in vec3 color;
out vec4 outColor;


uniform mat4 modelview;
uniform vec3 inColor;

void main()
{
	outColor = vec4(color,1);
	gl_Position = modelview * vec4(vertexPos, 1.0);
}