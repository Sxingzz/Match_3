using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    public static Item[] Items {  get; private set; }

    // RuntimeInitializeOnLoadMethod
    // đảm bảo rằng các dữ liệu quan trọng như đối tượng Item được tải vào trước khi trò chơi bắt đầu.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() => Items = Resources.LoadAll<Item>("Items/");


}
