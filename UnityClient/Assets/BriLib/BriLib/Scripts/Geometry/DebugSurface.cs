using UnityEngine;

namespace BriLib
{
    public class DebugSurface : MonoBehaviour
    {
        public float Scale = 1f;
        public Color Color = Color.yellow;

        private MeshFilter _filter;

        private void OnDrawGizmosSelected()
        {
            if (_filter == null)
            {
                _filter = GetComponent<MeshFilter>();
            }

            var normals = _filter.sharedMesh.normals;
            var verts = _filter.sharedMesh.vertices;
            for (int i = 0; i < normals.Length; i++)
            {
                Debug.DrawRay(verts[i], normals[i] * Scale, Color);
            }
        }
    }
}
