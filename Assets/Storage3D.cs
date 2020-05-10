
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

using static ExtraMath;

[Serializable]
public class Storage3D
{
  byte[] raw;
  [SerializeField]
  byte[] selectedCell;

  [SerializeField]
  int width, height, depth, cellsize;
  ///<summary>A container for 3d voxel data</summary>
  ///<param name="w">Width of the container</param>
  ///<param name="h">Height of the container</param>
  ///<param name="d">Depth of the container</param>
  ///<param name="cellsize">Size in bytes per cell</param>
  public Storage3D(int w, int h, int d, int cellsize)
  {
    this.width = w;
    this.height = h;
    this.depth = d;
    this.cellsize = cellsize;
    this.raw = new byte[w * h * d * cellsize];
    this.selectedCell = new byte[cellsize];
  }
  ///<summary>Get depth of this container</summary>
  public int getDepth()
  {
    return this.depth;
  }
  ///<summary>Get height of this container</summary>
  public int getHeight()
  {
    return this.height;
  }
  ///<summary>Get width of this container</summary>
  public int getWidth()
  {
    return this.width;
  }
  ///<summary>Get byte count of cells for this container</summary>
  public int getCellSize()
  {
    return this.cellsize;
  }
  ///<summary>Get a cell by its 3d location</summary>
  ///<param name="x">Position along width</param>
  ///<param name="y">Position along height</param>
  ///<param name="z">Position along depth</param>
  ///<returns>byte[cellsize]</returns>
  [MethodImpl(MethodImplOptions.Synchronized)]
  public byte[] getCell(int x, int y, int z, bool checksafe = true)
  {
    if (checksafe)
    {
      if (!this.isCellInBounds(x, y, z))
      {
        throw new System.InvalidOperationException("coordinate out of bounds: " + x + ", " + y + ", " + z + " allowed: 0 - " + this.width + ", 0 - " + this.height + ", 0 - " + this.depth);
      }
    }
    int ind = ThreeDimToIndex(x, y, z, this.width, this.height);
    return this.getCell(ind, false);
  }

  ///<summary>Set a cell by its 3d location</summary>
  ///<param name="x">Position along width</param>
  ///<param name="y">Position along height</param>
  ///<param name="z">Position along depth</param>
  ///<param name="cell">Data to set</param>
  ///<param name="checksafe">Run a check to see if in bounds first</param>
  [MethodImpl(MethodImplOptions.Synchronized)]
  public void setCell(int x, int y, int z, byte[] cell, bool checksafe = true)
  {
    if (checksafe)
    {
      if (!this.isCellInBounds(x, y, z))
      {
        throw new System.InvalidOperationException("coordinate out of bounds: " + x + ", " + y + ", " + z + " allowed: 0 - " + this.width + ", 0 - " + this.height + ", 0 - " + this.depth);
      }
    }
    int ind = ThreeDimToIndex(x, y, z, this.width, this.height);
    this.setCell(ind, cell, false);
  }

  ///<summary>Set a cell by its 1d raw index</summary>
  ///<param name="index">Position in raw array</param>
  ///<param name="cell">Data to set</param>
  ///<param name="checksafe">Run a check to see if in bounds first</param>
  [MethodImpl(MethodImplOptions.Synchronized)]
  public void setCell(int index, byte[] cell, bool checksafe = true)
  {
    index *= this.cellsize;
    if (checksafe)
    {
      if (index < 0 || index > this.raw.Length)
      {
        throw new System.IndexOutOfRangeException("index: " + index + ", max: " + this.raw.Length);
      }
    }
    for (int i = 0; i < cell.Length; i++)
    {
      this.raw[index + i] = cell[i];
    }
  }

  ///<summary>Check if a 3d index is in bounds for this storage</summary>
  ///<param name="x">Position along width</param>
  ///<param name="y">Position along height</param>
  ///<param name="z">Position along depth</param>
  ///<returns>true if in bounds</returns>
  public bool isCellInBounds(int x, int y, int z)
  {
    return (
      integerWithin(x, 0, this.width - 1) &&
      integerWithin(y, 0, this.height - 1) &&
      integerWithin(z, 0, this.depth - 1)
    );
  }

  ///<summary>Get a cell by its 1d index, note: multiples index by cellsize</summary>
  ///<param name="index">Position along raw array</param>
  ///<returns>byte[cellsize]</returns>
  public byte[] getCell(int index, bool checksafe = true)
  {
    if (checksafe)
    {
      if (index < 0 || index > this.raw.Length) throw new IndexOutOfRangeException("index:" + index + ", max:" + this.raw.Length);
    }
    int offset = index * this.cellsize;
    for (int i = 0; i < this.cellsize; i++)
    {
      this.selectedCell[i] = this.raw[offset + i];
    }
    return this.selectedCell;
  }
}
