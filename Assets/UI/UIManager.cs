using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  public static UIManager instance;
  [SerializeField]
  Canvas uiCanvas;
  [SerializeField]
  RectTransform uiInventory;
  [SerializeField]
  ScrollRect uiInventoryView;
  [SerializeField]
  RectTransform uiInventoryContent = null;

  [SerializeField]
  GameObject uiInventoryItemPrefab = null;

  List<GameObject> uiInventoryItems = new List<GameObject>();

  [SerializeField]
  Player player = null;

  public void addItemToInventory (string name, int index) {
    GameObject nItem = Instantiate(uiInventoryItemPrefab);
    nItem.GetComponentInChildren<Text>().text = name;
    Button b = nItem.GetComponent<Button>();
    b.onClick.AddListener(()=>player.setBlockTypeInHand(index));
    nItem.transform.SetParent(uiInventoryContent.transform);
    RectTransform r = nItem.GetComponent<RectTransform>();
    r.localScale = new Vector3(1, 1, 1);
    uiInventoryItems.Add(nItem);
  }

  void Start () {
    UIManager.instance = this;
  }
}
