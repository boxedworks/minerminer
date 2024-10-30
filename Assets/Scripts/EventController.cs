using UnityEngine;
using System;

namespace Controllers
{
  public class EventController
  {
    //
    public static EventController s_Singleton;

    //
    Action _onSellInventory;

    //
    public EventController()
    {
      s_Singleton = this;
    }
  }

}