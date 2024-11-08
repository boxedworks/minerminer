using System.Collections.Generic;
using UnityEngine;

//
public interface IInfoable
{

  public string _Info { get; }
  public RectTransform _Transform { get; }

}

//
public interface IHasInfoables
{
  public InfoData _InfoData { get; }
}

//
public class InfoData
{
  public List<IInfoable> _Infos;
  public IInfoable _DefaultInfo;
}

//
public class SimpleInfoable : IInfoable
{
  public string _Description;
  public string _Info { get { return _Description; } }

  public GameObject _GameObject;
  public RectTransform _Transform { get { return _GameObject == null ? null : _GameObject.transform as RectTransform; } }
}

//