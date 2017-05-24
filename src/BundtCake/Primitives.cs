using GlmNet;

namespace BundtCake
{
    static class Primitives
    {
        public static Mesh CreateCube8()
        {
            var cubeMesh = new Mesh();

            cubeMesh.VertexPositions = new vec3[]
            {
                // front
                // bottom left
                new vec3(-1.0f, -1.0f,  1.0f),
                // bottom right
                new vec3( 1.0f, -1.0f,  1.0f),
                // top right
                new vec3( 1.0f,  1.0f,  1.0f),
                // top left
                new vec3(-1.0f,  1.0f,  1.0f),
                // back
                // bottom left
                new vec3(-1.0f, -1.0f,  -1.0f),
                // bottom right
                new vec3( 1.0f, -1.0f,  -1.0f),
                // top right
                new vec3( 1.0f,  1.0f,  -1.0f),
                // top left
                new vec3(-1.0f,  1.0f,  -1.0f),
            };

            cubeMesh.Indices = new uint[]
            {
                // front
                0, 1, 2,
                2, 3, 0,
                // top
                1, 5, 6,
                6, 2, 1,
                // back
                7, 6, 5,
                5, 4, 7,
                // bottom
                4, 0, 3,
                3, 7, 4,
                // left
                4, 5, 1,
                1, 0, 4,
                // right
                3, 2, 6,
                6, 7, 3
            };

            cubeMesh.Colors = new vec3[]
            {
                // front colors
                new vec3(1.0f, 0.0f, 0.0f),
                new vec3(0.0f, 1.0f, 0.0f),
                new vec3(0.0f, 0.0f, 1.0f),
                new vec3(1.0f, 1.0f, 1.0f),
                // back colors
                new vec3(1.0f, 0.0f, 0.0f),
                new vec3(0.0f, 1.0f, 0.0f),
                new vec3(0.0f, 0.0f, 1.0f),
                new vec3(1.0f, 1.0f, 1.0f)
            };

            cubeMesh.TexCoords = new vec2[]
            {
                // front tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                // back tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
            };

            return cubeMesh;
        }

        public static Mesh CreateCube36()
        {
            var cubeMesh = new Mesh();

            cubeMesh.VertexPositions = new vec3[]
            {
                // Front face
                new vec3(-1.0f, -1.0f,  1.0f),
                new vec3(1.0f, -1.0f,  1.0f),
                new vec3(1.0f,  1.0f,  1.0f),
                new vec3(-1.0f,  1.0f,  1.0f),

                // Back face
                new vec3(-1.0f, -1.0f, -1.0f),
                new vec3(-1.0f,  1.0f, -1.0f),
                new vec3(1.0f,  1.0f, -1.0f),
                new vec3(1.0f, -1.0f, -1.0f),
            
                // Top face
                new vec3(-1.0f,  1.0f, -1.0f),
                new vec3(-1.0f,  1.0f,  1.0f),
                new vec3(1.0f,  1.0f,  1.0f),
                new vec3(1.0f,  1.0f, -1.0f),
            
                // Bottom face
                new vec3(-1.0f, -1.0f, -1.0f),
                new vec3(1.0f, -1.0f, -1.0f),
                new vec3(1.0f, -1.0f,  1.0f),
                new vec3(-1.0f, -1.0f,  1.0f),

                // Right face
                new vec3(1.0f, -1.0f, -1.0f),
                new vec3(1.0f,  1.0f, -1.0f),
                new vec3(1.0f,  1.0f,  1.0f),
                new vec3(1.0f, -1.0f,  1.0f),
            
                // Left face
                new vec3(-1.0f, -1.0f, -1.0f),
                new vec3(-1.0f, -1.0f,  1.0f),
                new vec3(-1.0f,  1.0f,  1.0f),
                new vec3(-1.0f,  1.0f, -1.0f)
            };

            cubeMesh.Indices = new uint[]
            {
                0,  1,  2,      0,  2,  3,    // front
                4,  5,  6,      4,  6,  7,    // back
                8,  9,  10,     8,  10, 11,   // top
                12, 13, 14,     12, 14, 15,   // bottom
                16, 17, 18,     16, 18, 19,   // right
                20, 21, 22,     20, 22, 23    // left
            };

            cubeMesh.Colors = new vec3[]
            {
                // Front face
                new vec3(-1.0f, -1.0f,  1.0f),
                new vec3(1.0f, -1.0f,  1.0f),
                new vec3(1.0f,  1.0f,  1.0f),
                new vec3(-1.0f,  1.0f,  1.0f),

                // Back face
                new vec3(-1.0f, -1.0f, -1.0f),
                new vec3(-1.0f,  1.0f, -1.0f),
                new vec3(1.0f,  1.0f, -1.0f),
                new vec3(1.0f, -1.0f, -1.0f),
            
                // Top face
                new vec3(-1.0f,  1.0f, -1.0f),
                new vec3(-1.0f,  1.0f,  1.0f),
                new vec3(1.0f,  1.0f,  1.0f),
                new vec3(1.0f,  1.0f, -1.0f),
            
                // Bottom face
                new vec3(-1.0f, -1.0f, -1.0f),
                new vec3(1.0f, -1.0f, -1.0f),
                new vec3(1.0f, -1.0f,  1.0f),
                new vec3(-1.0f, -1.0f,  1.0f),

                // Right face
                new vec3(1.0f, -1.0f, -1.0f),
                new vec3(1.0f,  1.0f, -1.0f),
                new vec3(1.0f,  1.0f,  1.0f),
                new vec3(1.0f, -1.0f,  1.0f),
            
                // Left face
                new vec3(-1.0f, -1.0f, -1.0f),
                new vec3(-1.0f, -1.0f,  1.0f),
                new vec3(-1.0f,  1.0f,  1.0f),
                new vec3(-1.0f,  1.0f, -1.0f)
            };

            cubeMesh.TexCoords = new vec2[]
            {
                // front tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                // back tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                // front tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                // back tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                // front tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                // back tex
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
            };

            return cubeMesh;
        }
    }
}