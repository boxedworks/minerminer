using Controllers;
using UnityEngine;

public class JavaScriptListener : MonoBehaviour{

  public void WindowClosing(){
    SaveController.Save();
  }

}