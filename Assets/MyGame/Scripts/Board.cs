using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    public Tile[,] Tiles {  get; private set; }

    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    private readonly List<Tile> _selection = new List<Tile>(); // readonly: chỉ có thể được gán giá trị một lần 
    private const float TweenDuration = 0.25f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //Max(): Là một phương thức mở rộng của Enumerable trong System.Linq, được sử dụng để trả về giá trị lớn nhất trong một chuỗi các giá trị.
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }
    }

    public async void Select(Tile tile) // async là một phương thức bất đồng bộ
    {
        if (!_selection.Contains(tile))
        {
            _selection.Add(tile);
        }

        if(_selection.Count < 2) return;

        Debug.Log($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]); // await: đợi một phương thức bất đồng bộ hoàn thành

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2) // async là một phương thức bất đồng bộ trả về 1 task
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1.transform.DOMove(icon2Transform.position, TweenDuration))
                .Join(icon2.transform.DOMove(icon1Transform.position,TweenDuration));

        await sequence.Play() 
                      .AsyncWaitForCompletion();//AsyncWaitForCompletion() trả vế 1 task khi hoàn thành sequence

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.item;

        tile1.item = tile2.item;
        tile2.item = tile1Item;

    }


}
