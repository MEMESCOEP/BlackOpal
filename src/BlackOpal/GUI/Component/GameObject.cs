using System.Numerics;
using PrismAPI.Graphics.Rasterizer;

namespace BlackOpal.GUI.Component
{
    internal class GameObject
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Mesh ObjectMesh;
    }
}
