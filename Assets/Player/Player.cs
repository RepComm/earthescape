using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;

using static ExtraMath;

public enum PlayerCameraMode
{
  FirstPersonPlayer,
  ThirdPersonPlayer,
  Build
}

public class Player : MonoBehaviour
{
  public PlayerCameraMode mode = PlayerCameraMode.Build;

  //Physics variables
  public Rigidbody rb;
  public BoxCollider bc;

  public float currentMoveSpeed = 1.0f;

  //Camera math variables
  float mmx = 0f;
  float mmy = 0f;
  float rotationX = 0f;
  public float maxVerRotation = 85f;

  public float sensitivity = 0.015f;
  public float moveSpeed = 3.0f;
  public Vector3 direction = new Vector3();

  public int placeBlockType = 1;

  //Block math variables
  public float placementDelay = 0.2f;
  public float lastPlacement = 0f;

  private RaycastHit hit;
  private Ray ray;
  int layer_mask;

  public Camera mainCamera;

  [SerializeField]
  int placeAxis = 0;
  [SerializeField]
  int placeAxisAmount = 0;
  [SerializeField]
  int placeBlockDir = 0;

  Client client;
  MessageDispatcher dispatcher;

  // Use this for initialization
  void Start()
  {
    if (rb == null) rb = GetComponent<Rigidbody>();
    if (bc == null) bc = GetComponent<BoxCollider>();
    if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
    layer_mask = LayerMask.GetMask("VoxelBuild");

    this.SetCameraMode(PlayerCameraMode.Build);
    client = new Client();
    client.Connect("localhost", 10209);

    dispatcher = new MessageDispatcher(client);
    dispatcher.messageReceivedEvent += this.OnMessage;
  }

  private void OnMessage (object sender, EventArgs e) {
    OnMessageEventArgs args = (OnMessageEventArgs)e;
    if (args.message["type"] == "block-break") {
      Vector3 pickPoint = new Vector3(
        args.message["x"].AsFloat,
        args.message["y"].AsFloat,
        args.message["z"].AsFloat
      );
      VoxelChunk.DEBUG_INSTANCE.setBlockWorld(pickPoint, 0, 0);
      VoxelChunk.DEBUG_INSTANCE.build();
    } else if (args.message["type"] == "block-place") {
      Vector3 pickPoint = new Vector3(
        args.message["x"].AsFloat,
        args.message["y"].AsFloat,
        args.message["z"].AsFloat
      );
      VoxelChunk.DEBUG_INSTANCE.setBlockWorld(pickPoint, args.message["blocktype"].AsInt, args.message["direction"].AsInt);
      VoxelChunk.DEBUG_INSTANCE.build();
    } else if (args.message["type"] == "connect") {
      Debug.Log("Connected");
    }
  }

  // Update is called once per frame
  void Update()
  {
    this.dispatcher.pullEvents();
    if (Input.GetButtonUp("escape"))
    {
      Cursor.lockState = CursorLockMode.None;
    }
    switch (this.mode)
    {
      case PlayerCameraMode.FirstPersonPlayer:
        break;
      case PlayerCameraMode.ThirdPersonPlayer:
        break;
      case PlayerCameraMode.Build:
        this.DoBuildModeInput();
        break;
    }
  }

  public PlayerCameraMode SetCameraMode(PlayerCameraMode mode)
  {
    PlayerCameraMode old = this.mode;
    switch (mode)
    {
      case PlayerCameraMode.FirstPersonPlayer:
        Debug.Log("Set to 1p mode");
        this.bc.enabled = false;
        this.rb.isKinematic = false;
        break;
      case PlayerCameraMode.ThirdPersonPlayer:
        Debug.Log("Set to 3p mode");
        break;
      case PlayerCameraMode.Build:
        Debug.Log("Set to build mode");
        this.bc.enabled = true;
        this.rb.isKinematic = true;
        break;
    }
    return old;
  }

  public void setBlockTypeInHand(int type)
  {
    this.placeBlockType = type;
  }

  void DoBuildModeInput()
  {
    if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
    {
      if (Cursor.lockState != CursorLockMode.Locked)
      {
        Cursor.lockState = CursorLockMode.Locked;
        return; //Don't process mouse input on this click, it produces annoying behaviour
      }
    }
    if (Cursor.lockState != CursorLockMode.Locked) return;
    this.mmx = Input.GetAxis("mouse_x") * this.sensitivity;
    this.mmy = Input.GetAxis("mouse_y") * this.sensitivity;

    this.rotationX -= this.mmy * 180;
    this.rotationX = Mathf.Clamp(this.rotationX, -this.maxVerRotation, this.maxVerRotation);

    transform.localRotation = Quaternion.Euler(
      this.rotationX,
      (mmx * 180) + transform.localRotation.eulerAngles.y,
      0f
    );

    if (this.lastPlacement <= 0f)
    {
      if (Input.GetMouseButton(0))
      {
        this.lastPlacement += this.placementDelay;

        this.ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(this.ray, out this.hit, layer_mask))
        {
          if (this.hit.transform.parent)
          {
            VoxelChunk voxelChunk = this.hit.transform.parent.GetComponent<VoxelChunk>();

            if (voxelChunk != null)
            {
              Vector3 dir = this.hit.point - this.ray.origin;
              dir.Normalize();
              Vector3 pickPoint = this.hit.point + (dir / 8);
              // Debug.DrawLine(this.ray.origin, this.hit.point, Color.white, 60);
              // Debug.DrawLine(this.hit.point, pickPoint, Color.red, 60);
              voxelChunk.setBlockWorld(pickPoint, 0, 0);
              voxelChunk.build();
              
              JSONObject packet = new JSONObject();
              packet.Add("type", new JSONString("block-break"));
              packet.Add("x", new JSONNumber(Math.Round(pickPoint.x, 4)));
              packet.Add("y", new JSONNumber(Math.Round(pickPoint.y, 4)));
              packet.Add("z", new JSONNumber(Math.Round(pickPoint.z, 4)));

              this.client.SendJSON( packet );
            }
          }
        }
      }
      if (Input.GetMouseButton(1))
      {
        this.lastPlacement += this.placementDelay;
        Cursor.lockState = CursorLockMode.Locked;
        this.ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(this.ray, out this.hit, layer_mask))
        {
          if (this.hit.transform.parent)
          {
            VoxelChunk voxelChunk = this.hit.transform.parent.GetComponent<VoxelChunk>();

            if (voxelChunk != null)
            {
              Vector3 dir = this.hit.point - this.ray.origin;
              dir.Normalize();
              Vector3 pickPoint = this.hit.point - (dir / 8);
              placeBlockDir = MeshBuilder.rotateInfoToByte(placeAxis, placeAxisAmount);
              voxelChunk.setBlockWorld(pickPoint, placeBlockType, placeBlockDir);
              voxelChunk.build();

              JSONObject packet = new JSONObject();
              packet.Add("type", new JSONString("block-place"));
              packet.Add("x", new JSONNumber(Math.Round(pickPoint.x, 4)));
              packet.Add("y", new JSONNumber(Math.Round(pickPoint.y, 4)));
              packet.Add("z", new JSONNumber(Math.Round(pickPoint.z, 4)));
              packet.Add("blocktype", new JSONNumber(placeBlockType));
              packet.Add("direction", new JSONNumber(placeBlockDir));
              this.client.SendJSON( packet );
            }
          }
        }
      }
    }
    else
    {
      this.lastPlacement -= Time.deltaTime;
    }

    if (Input.GetButtonUp("g"))
    {
      this.placeBlockType++;
    }
    else if (Input.GetButtonUp("f"))
    {
      this.placeBlockType--;
      if (this.placeBlockType < 0) this.placeBlockType = 0;
    }
    if (Input.GetButtonUp("c"))
    {
      this.placeAxis++;
      if (this.placeAxis > 3) this.placeAxis = 0;
    }
    if (Input.GetButtonUp("v"))
    {
      this.placeAxisAmount++;
      if (this.placeAxisAmount > 3) this.placeAxisAmount = 0;
    }

    if (Input.GetButtonDown("shift"))
    {
      currentMoveSpeed = moveSpeed * 2.0f;
    }
    if (Input.GetButtonUp("shift"))
    {
      currentMoveSpeed = moveSpeed;
    }

    if (Input.GetButton("forward"))
    {
      transform.Translate(Vector3.forward * Time.deltaTime * currentMoveSpeed);
    }
    if (Input.GetButton("backward"))
    {
      transform.Translate(-Vector3.forward * Time.deltaTime * currentMoveSpeed);
    }
    if (Input.GetButton("left"))
    {
      transform.Translate(-Vector3.right * Time.deltaTime * currentMoveSpeed);
    }
    if (Input.GetButton("right"))
    {
      transform.Translate(Vector3.right * Time.deltaTime * currentMoveSpeed);
    }
  }

  void DoFirstPersonPlayerModeInput()
  {
    if (Input.GetButton("Backward"))
    {
      //transform.Translate(Vector3.forward*Time.deltaTime*currentMoveSpeed);
      rb.AddForce(Vector3.forward * Time.deltaTime * currentMoveSpeed);
    }
    if (Input.GetButton("Forward"))
    {
      rb.AddRelativeForce(-Vector3.forward * Time.deltaTime * currentMoveSpeed);
    }
    if (Input.GetButton("Right"))
    {
      rb.AddRelativeForce(-Vector3.right * Time.deltaTime * currentMoveSpeed);
    }
    if (Input.GetButton("Left"))
    {
      rb.AddRelativeForce(Vector3.right * Time.deltaTime * currentMoveSpeed);
    }
  }
}
