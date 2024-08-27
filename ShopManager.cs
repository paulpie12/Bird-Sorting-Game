using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;

public class ShopManager : BaseManager
{
    public override UniTask StartManager()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask StopManager()
    {
        return UniTask.CompletedTask;
    }
}
