using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
  [SerializeField]
  private Ship ship;
  private bool local = false;
  void Start()
  {
    this.ship = this.GetComponent<Ship>();
  }

  public bool Local {
    get {
      return this.local;
    }
    set {
      this.local = value;
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
