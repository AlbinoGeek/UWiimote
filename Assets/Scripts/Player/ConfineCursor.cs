using UnityEngine;

public class ConfineCursor : MonoBehaviour {
  public bool setCursorOnStart = true;
  public CursorLockMode defaultMode = CursorLockMode.None;

  public bool hideCursorOnConfined = true;
  public bool hideCursorOnLocked = true;

  CursorLockMode currentState;

  public static void StaticSetMode(CursorLockMode type) {
    Cursor.lockState = type;
  }

  public void SetMode(CursorLockMode type) {
    this.currentState = type;
    ConfineCursor.StaticSetMode(type);

    if (hideCursorOnConfined && currentState == CursorLockMode.Confined)
      Cursor.visible = false;
    else if (hideCursorOnLocked && currentState == CursorLockMode.Locked)
      Cursor.visible = false;
    else
      Cursor.visible = true;
  }

  // Use this for initialization
  void Start () {
    if (setCursorOnStart)
      this.SetMode(this.defaultMode);
	}
}
