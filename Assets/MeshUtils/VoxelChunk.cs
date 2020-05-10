
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
  Storage3D storage;
  byte[] dataToSet;

  [SerializeField]
  VoxelDefinitions definitions = null;

  MeshBuilder meshBuilder;
  MeshBuilder collisionBuilder;

  MeshFilter visualMeshContainer;
  [SerializeField]
  GameObject rayCollisionGameObject = null;
  MeshCollider rayCollisionMeshContainer;
  MeshCollider physicsCollisionMeshContainer;

  [SerializeField]
  int width = 8;
  [SerializeField]
  int height = 8;
  [SerializeField]
  int depth = 8;

  int bytesPerCell = 2;

  void Awake()
  {
    dataToSet = new byte[bytesPerCell];
    storage = new Storage3D(width, height, depth, bytesPerCell);
    visualMeshContainer = GetComponent<MeshFilter>();
    rayCollisionMeshContainer = rayCollisionGameObject.GetComponent<MeshCollider>();
    physicsCollisionMeshContainer = GetComponent<MeshCollider>();

    meshBuilder = new MeshBuilder();
    collisionBuilder = new MeshBuilder();
  }

  void Start()
  {
    this.setBlockLocal(3, 0, 3, 1, 0);
    this.setBlockLocal(3, 0, 4, 1, 0);
    this.setBlockLocal(4, 0, 4, 1, 0);
    this.setBlockLocal(4, 0, 3, 1, 0);
    this.build();
  }

  void Update () {
    this.rayCollisionGameObject.transform.position = this.transform.position;
    this.rayCollisionGameObject.transform.rotation = this.transform.rotation;
  }

  public bool canPlaceHereLocal(int x, int y, int z)
  {
    return this.storage.isCellInBounds(x, y, z);
  }

  public bool isFilledLocal(int x, int y, int z)
  {
    if (this.canPlaceHereLocal(x, y, z))
    {
      return this.getBlockLocal(x, y, z)[0] != 0;
    }
    return false;
  }

  public void setBlockLocal(int x, int y, int z, int blocktype, int direction)
  {
    if (this.canPlaceHereLocal(x, y, z))
    {
      this.dataToSet[0] = (byte)blocktype;
      this.dataToSet[1] = (byte)direction;
      this.storage.setCell(
        x,
        y,
        z,
        dataToSet
      );
    }
  }

  public void setBlockLocal(Vector3 localPos, int blocktype, int direction)
  {
    this.setBlockLocal(
      (int)localPos.x,
      (int)localPos.y,
      (int)localPos.z,
      blocktype,
      direction
    );
  }

  public byte[] getBlockLocal(int x, int y, int z)
  {
    this.dataToSet = this.storage.getCell(
      x,
      y,
      z
    );
    return this.dataToSet;
  }

  public byte[] getBlockLocal(Vector3 localPos)
  {
    return this.getBlockLocal(
      (int)localPos.x,
      (int)localPos.y,
      (int)localPos.z
    );
  }

  public void setBlockWorld(Vector3 pos, int blocktype, int direction)
  {
    pos = this.worldToVoxelPoint(pos);
    this.setBlockLocal(pos, blocktype, direction);
  }

  public void build()
  {
    //Clear the mesh builders
    this.meshBuilder.clear();
    this.collisionBuilder.clear();

    byte[] cell;
    byte blocktype = 0;
    Mesh blockMesh;
    Vector3 offset = new Vector3();
    byte rotateData = 0;
    byte axis = 0;
    byte axisAmount = 0;

    //TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT
    bool[] faces = new bool[6];

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        for (int z = 0; z < depth; z++)
        {
          //Retrieve the cell from xyz coords
          cell = this.storage.getCell(x, y, z);

          //Get block type and mesh data
          blocktype = cell[0]; //First byte of cell is block type

          //Dont fill air
          if (blocktype != 0)
          {
            blockMesh = this.definitions.getPrefabMesh(blocktype);

            //Get block rotation data
            rotateData = cell[1];
            axis = MeshBuilder.byteToRotateAxis(rotateData);
            axisAmount = MeshBuilder.byteToRotateAmount(rotateData);

            offset.Set(x + 0.5f, y + 0.5f, z + 0.5f);

            this.meshBuilder.pushMeshOffset(
              blockMesh,
              offset,
              axis,
              axisAmount
            );

            //TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT
            faces[0] = !this.isFilledLocal(x, y + 1, z);
            faces[1] = !this.isFilledLocal(x, y - 1, z);
            faces[2] = !this.isFilledLocal(x, y, z - 1);
            faces[3] = !this.isFilledLocal(x, y, z + 1);
            faces[4] = !this.isFilledLocal(x - 1, y, z);
            faces[5] = !this.isFilledLocal(x + 1, y, z);
            offset.Set(x, y, z);
            this.collisionBuilder.cube(offset, Vector3.one, faces);
          }
        }
      }
    }
    this.visualMeshContainer.mesh = this.meshBuilder.make(visualMeshContainer.mesh);
    this.visualMeshContainer.mesh.MarkDynamic();
    this.visualMeshContainer.mesh.Optimize();
    this.rayCollisionMeshContainer.sharedMesh = this.collisionBuilder.make(this.rayCollisionMeshContainer.sharedMesh);
    this.physicsCollisionMeshContainer.sharedMesh = this.rayCollisionMeshContainer.sharedMesh;
  }

  public Vector3 worldToVoxelPoint(Vector3 worldPoint)
  {
    //Subtract ship's offset in the world
    worldPoint -= this.transform.position;

    //Subtract ship's rotation from the point
    //Works by rotating around the inverse of ship's quaternion from standpoint of ship's position
    worldPoint = ExtraMath.RotatePointAroundPoint(
      worldPoint,
      this.transform.position,
      Quaternion.Inverse(this.transform.rotation)
    );
    return worldPoint;
  }
}