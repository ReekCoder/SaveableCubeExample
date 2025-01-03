using System;
using SaveSystemExtension.Variables;

namespace SaveCubeExample
{
    [Serializable]
    public class CubeData
    {
        public SavedVector3 cubePosition;
        public SavedQuaternion cubeRotation;
        public SavedColor cubeColor;
    }
}
