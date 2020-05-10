
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDefinitions : MonoBehaviour {
  [SerializeField]
  GameObject[] voxelPrefabs = null; //Meant to be assigned in the editor

  public GameObject getPrefab (int index) {
    return this.voxelPrefabs[index];
  }

  public void setPrefab (int index, GameObject prefab) {
    this.voxelPrefabs[index] = prefab;
  }

  public Mesh getPrefabMesh (int index) {
    return this.voxelPrefabs[index].GetComponent<MeshFilter>().sharedMesh;
  }

}