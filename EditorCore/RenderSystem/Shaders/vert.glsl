#version 130

// Vertex Data Input
in vec3 RawPosition;
in vec4 RawColor0;
in vec2 RawTex0;

// Output
out vec4 Color0;
out vec3 Tex0;

uniform mat4 ModelMtx;
uniform mat4 ViewMtx;
uniform mat4 ProjMtx;

void main()
{
	mat4 MVP = ProjMtx * ViewMtx * ModelMtx;
	gl_Position = MVP * vec4(RawPosition, 1);
	Color0 = RawColor0;
	Tex0 = vec3(RawTex0, 1);
}