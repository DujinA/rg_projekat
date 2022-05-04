using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGL;
using SharpGL.SceneGraph.Primitives;

namespace AssimpSample.Services
{
    class SafetyWalls
    {
        private World _world;

        public SafetyWalls(World world)
        {
            _world = world;
        }

        public void RightSafetyWall(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Scale(1.0f, 50.0f, 3.5f);
            gl.Translate(21.0f, 0.0f, 1.0f);

            Cube rightWall = new Cube();
            rightWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
        }

        public void LeftSafetyWall(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Scale(1.0f, -50.0f, 3.5f);
            gl.Translate(-21.0f, 0.0f, 1.0f);

            Cube leftWall = new Cube();
            leftWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
        }
    }
}
