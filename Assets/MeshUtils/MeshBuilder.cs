using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static ExtraMath;

public class MeshBuilder
{
  public Mesh mesh;
  public List<Vector2> uvs;
  public List<int> tris;
  public List<Vector3> verts;
  public List<Vector3> normals;

  public MeshBuilder()
  {
    this.uvs = new List<Vector2>();
    this.tris = new List<int>();
    this.verts = new List<Vector3>();
    this.normals = new List<Vector3>();
  }

  public void pushMeshOffset(Mesh mesh, Vector3 offset, byte axis, byte axisAmount)
  {
    //Get the triangle indexes into an array as accessing member will create new arrays..
    int[] mtris = mesh.triangles;

    //Same as above for vertices
    Vector3[] mverts = mesh.vertices;

    //Same as above for uvs
    Vector2[] muvs = mesh.uv;

    //Get offset in triangle indexes so we render correct triangles to verticies
    int triIndOffset = this.verts.Count;

    for (int i = 0; i < mtris.Length; i++)
    {
      //Push each onto the list + original offset in indexes of the mesh
      this.tris.Add(triIndOffset + mtris[i]);
    }
    Vector3 v;
    //Loop through all the verticies
    for (int i = 0; i < mverts.Length; i++)
    {
      v = mverts[i];
      //Rotate the verticies around their origin
      if (axis == 1)
      {
        v = ExtraMath.RotatePointAroundPoint(v, Vector3.zero, axisAmount * 90.0f, 0, 0);
      }
      else if (axis == 2)
      {
        v = ExtraMath.RotatePointAroundPoint(v, Vector3.zero, 0, axisAmount * 90.0f, 0);
      }
      else if (axis == 3)
      {
        v = ExtraMath.RotatePointAroundPoint(v, Vector3.zero, 0, 0, axisAmount * 90.0f);
      }
      //Move vert to block offset
      v += offset;
      //Push the modified vert onto the list
      this.verts.Add(v);
    }
    //Loop through all the uv coordinates
    for (int i = 0; i < muvs.Length; i++)
    {
      //Push uvs into the list
      this.uvs.Add(muvs[i]);
    }
  }

  public void pushMesh(Mesh mesh)
  {
    this.pushMeshOffset(mesh, Vector3.zero, 0, 0);
  }

  public void tri(Vector3 one, Vector3 two, Vector3 three)
  {
    this.tri(one, two, three,
      new Vector2(0f, 0f),
      new Vector2(1f, 0f),
      new Vector2(1f, 0f)
    );
  }

  public void tri(Vector3 one, Vector3 two, Vector3 three, Vector2 uv_one, Vector2 uv_two, Vector2 uv_three)
  {
    int index = this.verts.Count;
    this.verts.Add(one);
    this.verts.Add(two);
    this.verts.Add(three);

    this.tri(index, index + 1, index + 2);

    this.uvs.Add(uv_one);
    this.uvs.Add(uv_two);
    this.uvs.Add(uv_three);

    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
  }

  public void tri(int one, int two, int three)
  {
    this.tris.Add(one);
    this.tris.Add(three);
    this.tris.Add(two);
  }

  public void quad(Vector3 one, Vector3 two, Vector3 three, Vector3 four)
  {
    this.quad(one, two, three, four,
      new Vector2(0f, 0f),
      new Vector2(1f, 0f),
      new Vector2(1f, 1f),
      new Vector2(0f, 1f)
    );
  }

  public void quad(Vector3 one, Vector3 two, Vector3 three, Vector3 four, Vector2 uv_one, Vector2 uv_two, Vector2 uv_three, Vector2 uv_four)
  {
    int index = this.verts.Count;
    this.verts.Add(one);
    this.verts.Add(two);
    this.verts.Add(three);
    this.verts.Add(four);

    this.tri(index, index + 1, index + 2);
    this.tri(index + 2, index + 3, index);

    this.uvs.Add(uv_one);
    this.uvs.Add(uv_two);
    this.uvs.Add(uv_three);
    this.uvs.Add(uv_four);

    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
    this.normals.Add(
      new Vector3(1f, 1f, 1f)
    );
  }

  public void cube(Vector3 pos, Vector3 size)
  {
    bool[] faces = { true, true, true, true, true, true };
    this.cube(pos, size, faces);
  }

  //TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT
  public void cube(Vector3 pos, Vector3 size, bool[] faces)
  {
    //TOP
    if (faces[0])
    {
      this.quad(
        new Vector3(pos.x, pos.y + size.y, pos.z),
        new Vector3(pos.x + size.x, pos.y + size.y, pos.z),
        new Vector3(pos.x + size.x, pos.y + size.y, pos.z + size.z),
        new Vector3(pos.x, pos.y + size.y, pos.z + size.z)
      );
    }
    //BOTTOM
    if (faces[1])
    {
      this.quad(
        new Vector3(pos.x + size.x, pos.y, pos.z),
        pos,
        new Vector3(pos.x, pos.y, pos.z + size.z),
        new Vector3(pos.x + size.x, pos.y, pos.z + size.z)
      );
    }
    //FRONT
    if (faces[2])
    {
      this.quad(
        pos,
        new Vector3(pos.x + size.x, pos.y, pos.z),
        new Vector3(pos.x + size.x, pos.y + size.y, pos.z),
        new Vector3(pos.x, pos.y + size.y, pos.z)
      );
    }
    //BACK
    if (faces[3])
    {
      this.quad(
        new Vector3(pos.x + size.x, pos.y, pos.z + size.z),
        new Vector3(pos.x, pos.y, pos.z + size.z),
        new Vector3(pos.x, pos.y + size.y, pos.z + size.z),
        new Vector3(pos.x + size.x, pos.y + size.y, pos.z + size.z)
      );
    }
    //LEFT
    if (faces[4])
    {
      this.quad(
        new Vector3(pos.x, pos.y, pos.z + size.z),
        pos,
        new Vector3(pos.x, pos.y + size.y, pos.z),
        new Vector3(pos.x, pos.y + size.y, pos.z + size.z)
      );
    }
    //RIGHT
    if (faces[5])
    {
      this.quad(
        new Vector3(pos.x + size.x, pos.y, pos.z),
        new Vector3(pos.x + size.x, pos.y, pos.z + size.z),
        new Vector3(pos.x + size.x, pos.y + size.y, pos.z + size.z),
        new Vector3(pos.x + size.x, pos.y + size.y, pos.z)
      );
    }
  }

  ///<summary>Converts rotation info to a byte</summary>
  public static byte rotateInfoToByte(int axis = 0, int amount = 0)
  {
    if (axis > 3) throw new System.Exception("Axis cannot be over 3");
    if (amount > 3) throw new System.Exception("Amount cannot be over 3");
    int result = axis << 6; //Pack axis as first two bits
    result |= amount << 4; //Pack amount as next two bits
    return (byte)result;
  }

  ///<summary>Extracts axis from rotate byte data</summary>
  public static byte byteToRotateAxis(int data)
  {
    return (byte)((data & 0b11000000) >> 6); //Extracts first 2 bits
  }

  ///<summary>Extracts amount from rotate byte data</summary>
  public static byte byteToRotateAmount(int data)
  {
    return (byte)((data & 0b00110000) >> 4); //Extracts bits 3 and 4
  }

  public Mesh make(Mesh mesh)
  {

    if (mesh == null)
    {
      this.mesh = new Mesh();
    }
    else
    {
      this.mesh = mesh;
    }
    this.mesh.Clear();

    this.mesh.vertices = this.verts.ToArray();
    this.mesh.uv = this.uvs.ToArray();
    this.mesh.triangles = this.tris.ToArray();

    this.mesh.RecalculateNormals();

    return this.mesh;
  }

  public void clear()
  {
    this.tris.Clear();
    this.uvs.Clear();
    this.verts.Clear();
    this.normals.Clear();
  }
}
