using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    public Tile[,] Tiles {  get; private set; }

    public int Width => Tiles.GetLength(0); // column
    public int Height => Tiles.GetLength(1); // row

    private readonly List<Tile> _selection = new List<Tile>(); // readonly: chỉ có thể được gán giá trị một lần 
    private const float TweenDuration = 0.25f;

    [SerializeField]
    private AudioClip collectSound;

    [SerializeField]
    private AudioSource audioSource;

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

                tile.item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }

        CheckInitialPop();
    }

    private async void CheckInitialPop()
    {
        while (CanPop())
        {
            await Pop();
        }
    }

    public async void Select(Tile tile) // async là một phương thức bất đồng bộ
    {
        if (!_selection.Contains(tile))
        {
            if(_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) != -1)
                {
                    _selection.Add(tile);
                }
            }
            else
            {
                _selection.Add(tile);
            }
        }

        if(_selection.Count < 2) return;

        Debug.Log($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]); // await: đợi một phương thức bất đồng bộ hoàn thành

        if (CanPop())
        {
            Pop();  
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

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

    private bool CanPop()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private async Task Pop()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0;x < Width; x++)
            {
                var tile = Tiles[x, y];

                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2)
                {
                    continue;
                }

                var deflateSequence = DOTween.Sequence();

                foreach(var connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
                }

                audioSource.PlayOneShot(collectSound);

                ScoreCounter.Instance.Score += tile.item.value * connectedTiles.Count;

                await deflateSequence.Play()
                                     .AsyncWaitForCompletion();

                var inflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }

                await inflateSequence.Play() 
                                     .AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }

}
