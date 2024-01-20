using UnityEngine;

namespace PeterHan.PLib.UI;

public abstract class UIComponentBase : IUIComponent
{
    public string Name { get; protected set; }

    public GameObject BuiltObject { get; protected set; }

    public abstract GameObject Build();

    public event PUIDelegates.OnRealize OnRealize;

    protected void InvokeOnRealize(GameObject realized)
    {
        OnRealize?.Invoke(realized);
    }
}