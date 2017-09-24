using UnityEngine;

public class PolyExtrude : MonoBehaviour
{
    public Vector2[] points = null;
    public Vector2[] extrusionProfile = null;

    public Vector2 CenterPoint
    {
        get
        {
            Vector2 averagePoint = Vector2.zero;
            foreach (Vector2 point in points)
            {
                averagePoint += point;
            }

            return averagePoint / points.Length;
        }
    }

    private Mesh _mesh = null;
    private MeshFilter _filter = null;

    private int VertexCount
    {
        get
        {
            return points.Length * (((extrusionProfile.Length - 1) * 4) + 2) + 4;
        }
    }

    private int PolyCount
    {
        get
        {
            return (2 * points.Length * extrusionProfile.Length);
        }
    }

    private int IndexCount
    {
        get
        {
            return PolyCount * 3;
        }
    }

    private void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        if (points.Length < 3)
        {
            return;
        }

        if (_mesh == null)
        {
            _mesh = new Mesh() { name = string.Format("{0}-Mesh", name) };
        }

        Vector3[] vertices;
        Vector3[] normals;
        Vector2[] uvs;
        int[] triangles;

        RebuildMeshData(out vertices, out normals, out uvs, out triangles);

        _mesh.vertices = vertices;
        _mesh.normals = normals;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;

        _mesh.UploadMeshData(false);

        if (_filter == null)
        {
            _filter = GetComponent<MeshFilter>();
        }

        _filter.mesh = _mesh;
    }

    private void RebuildMeshData(out Vector3[] vertices, out Vector3[] normals, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[VertexCount];
        normals = new Vector3[VertexCount];
        uvs = new Vector2[VertexCount];
        triangles = new int[IndexCount];

        Vector3[,] positions;
        Vector3[,] faceNormals;
        RebuildEdgeCoordinates(out positions, out faceNormals);

        int vertexIndex = 0;
        int triangleIndex = 0;

        Vector3 averageBottomPoint = Vector3.zero;
        for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
        {
            averageBottomPoint += positions[pointIndex, 0];
        }
        averageBottomPoint /= points.Length;

        //Build bottom face
        int centerPointIndex = vertexIndex;
        vertices[centerPointIndex] = averageBottomPoint;
        normals[centerPointIndex] = Vector3.down;
        uvs[vertexIndex] = new Vector2(0.0f, 0.0f);
        vertexIndex++;

        for (int pointIndex = points.Length - 1; pointIndex >= 0; --pointIndex, ++vertexIndex)
        {
            vertices[vertexIndex] = positions[pointIndex, 0];
            normals[vertexIndex] = Vector3.down;
            uvs[vertexIndex] = new Vector2(0.0f, 0.0f);

            triangles[triangleIndex++] = centerPointIndex;
            triangles[triangleIndex++] = vertexIndex;
            triangles[triangleIndex++] = vertexIndex + 1;
        }

        vertices[vertexIndex] = positions[points.Length - 1, 0];
        normals[vertexIndex] = Vector3.down;
        uvs[vertexIndex] = new Vector2(0.0f, 0.0f);
        vertexIndex++;

        Vector3 averageTopPoint = Vector3.zero;
        for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
        {
            averageTopPoint += positions[pointIndex, extrusionProfile.Length - 1];
        }
        averageTopPoint /= points.Length;

        Vector2 uvCenter = new Vector2(0.5f, 0.5f);

        //Build top face
        centerPointIndex = vertexIndex;
        vertices[centerPointIndex] = averageTopPoint;
        normals[centerPointIndex] = Vector3.up;
        uvs[vertexIndex] = uvCenter;
        vertexIndex++;

        for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex, ++vertexIndex)
        {
            vertices[vertexIndex] = positions[pointIndex, extrusionProfile.Length - 1];
            normals[vertexIndex] = Vector3.up;
            uvs[vertexIndex] = GetPolarCoordinate(uvCenter, pointIndex / ((float)points.Length), 0.45f);

            triangles[triangleIndex++] = centerPointIndex;
            triangles[triangleIndex++] = vertexIndex;
            triangles[triangleIndex++] = vertexIndex + 1;
        }

        vertices[vertexIndex] = positions[0, extrusionProfile.Length - 1];
        normals[vertexIndex] = Vector3.up;
        uvs[vertexIndex] = GetPolarCoordinate(uvCenter, 1.0f, 0.45f);
        vertexIndex++;

        //Build edge loop
        for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
        {
            int nextPointIndex = (pointIndex + 1) % points.Length;

            for (int extrusionIndex = 0; extrusionIndex < extrusionProfile.Length - 1; ++extrusionIndex, vertexIndex += 4)
            {
                vertices[vertexIndex] = positions[nextPointIndex, extrusionIndex];
                vertices[vertexIndex + 1] = positions[nextPointIndex, extrusionIndex + 1];
                vertices[vertexIndex + 2] = positions[pointIndex, extrusionIndex + 1];
                vertices[vertexIndex + 3] = positions[pointIndex, extrusionIndex];

                Vector3 normal = faceNormals[pointIndex, extrusionIndex];
                normals[vertexIndex] = normal;
                normals[vertexIndex + 1] = normal;
                normals[vertexIndex + 2] = normal;
                normals[vertexIndex + 3] = normal;

                uvs[vertexIndex] = GetPolarCoordinate(uvCenter, (nextPointIndex / (float)points.Length), Mathf.Lerp(0.5f, 0.45f, extrusionIndex / (extrusionProfile.Length - 1.0f)));
                uvs[vertexIndex + 1] = GetPolarCoordinate(uvCenter, (nextPointIndex / (float)points.Length), Mathf.Lerp(0.5f, 0.45f, (extrusionIndex + 1) / (extrusionProfile.Length - 1.0f)));
                uvs[vertexIndex + 2] = GetPolarCoordinate(uvCenter, (pointIndex / (float)points.Length), Mathf.Lerp(0.5f, 0.45f, (extrusionIndex + 1) / (extrusionProfile.Length - 1.0f)));
                uvs[vertexIndex + 3] = GetPolarCoordinate(uvCenter, (pointIndex / (float)points.Length), Mathf.Lerp(0.5f, 0.45f, extrusionIndex / (extrusionProfile.Length - 1.0f)));

                triangles[triangleIndex++] = vertexIndex;
                triangles[triangleIndex++] = vertexIndex + 1;
                triangles[triangleIndex++] = vertexIndex + 2;
                triangles[triangleIndex++] = vertexIndex;
                triangles[triangleIndex++] = vertexIndex + 2;
                triangles[triangleIndex++] = vertexIndex + 3;
            }
        }
    }

    private void RebuildEdgeCoordinates(out Vector3[,] positions, out Vector3[,] faceNormals)
    {
        positions = new Vector3[points.Length, extrusionProfile.Length];
        faceNormals = new Vector3[points.Length, extrusionProfile.Length - 1];

        for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
        {
            Vector2 point = points[pointIndex];
            Vector2 previousPoint = points[(pointIndex + (points.Length - 1)) % points.Length];
            Vector2 nextPoint = points[(pointIndex + 1) % points.Length];

            Vector2 previousVector = (previousPoint - point).normalized;
            Vector2 nextVector = (nextPoint - point).normalized;

            Vector2 inVector = (previousVector + nextVector).normalized;

            Vector3 extrudeRight = new Vector3(inVector.x, 0.0f, inVector.y);

            Vector3 faceOutward = Vector3.Cross(nextVector, Vector3.forward);
            faceOutward.z = faceOutward.y;
            faceOutward.y = 0.0f;

            for (int extrusionIndex = 0; extrusionIndex < extrusionProfile.Length; ++extrusionIndex)
            {
                Vector2 extrusion = extrusionProfile[extrusionIndex];
                positions[pointIndex, extrusionIndex] = new Vector3(point.x, 0.0f, point.y) + (extrudeRight * extrusion.x) + (Vector3.up * extrusion.y);

                if (extrusionIndex > 0)
                {
                    Vector2 previousExtrusion = extrusionProfile[extrusionIndex - 1];
                    Vector2 extrusionNormal = Vector3.Cross(extrusion - previousExtrusion, Vector3.back).normalized;

                    Vector3 normal = (faceOutward * extrusionNormal.x) + (Vector3.up * extrusionNormal.y);
                    faceNormals[pointIndex, extrusionIndex - 1] = normal.normalized;
                }
            }
        }
    }

    private Vector2 GetPolarCoordinate(Vector2 center, float normalisedX, float radius)
    {
        return center + new Vector2(Mathf.Cos(normalisedX * 2.0f * Mathf.PI), Mathf.Sin(normalisedX * 2.0f * Mathf.PI)) * radius;
    }
}
