using UnityEngine;
using UnityEngine.Tilemaps;

public class SkeletonKingTileGate : MonoBehaviour
{
    [SerializeField] private CharacterStats skeletonKingStats;
    [SerializeField] private Tilemap blockingTilemap;
    [SerializeField] private Tilemap visualTilemap;
    [SerializeField] private Vector3Int[] gateCells;
    [SerializeField, Range(0f, 1f)] private float openedAlpha = 0f;

    private TileBase[] cachedTiles;
    private Matrix4x4[] cachedTransforms;
    private Color[] cachedColors;
    private bool isOpened;

    private void Awake()
    {
        CacheGateTiles();
    }

    private void OnEnable()
    {
        if (skeletonKingStats != null)
            skeletonKingStats.onHealthChanged += TryOpenGate;
    }

    private void Start()
    {
        TryOpenGate();
    }

    private void OnDisable()
    {
        if (skeletonKingStats != null)
            skeletonKingStats.onHealthChanged -= TryOpenGate;
    }

    private void CacheGateTiles()
    {
        if (blockingTilemap == null || gateCells == null)
            return;

        cachedTiles = new TileBase[gateCells.Length];
        cachedTransforms = new Matrix4x4[gateCells.Length];
        cachedColors = new Color[gateCells.Length];

        for (int i = 0; i < gateCells.Length; i++)
        {
            Vector3Int cell = gateCells[i];
            cachedTiles[i] = blockingTilemap.GetTile(cell);
            cachedTransforms[i] = blockingTilemap.GetTransformMatrix(cell);
            cachedColors[i] = blockingTilemap.GetColor(cell);
        }
    }

    private void TryOpenGate()
    {
        if (isOpened || skeletonKingStats == null || skeletonKingStats.currentHealth > 0)
            return;

        OpenGate();
    }

    private void OpenGate()
    {
        if (blockingTilemap == null || gateCells == null)
            return;

        if (cachedTiles == null || cachedTiles.Length != gateCells.Length)
            CacheGateTiles();

        isOpened = true;

        for (int i = 0; i < gateCells.Length; i++)
        {
            Vector3Int cell = gateCells[i];
            TileBase tile = cachedTiles != null && i < cachedTiles.Length ? cachedTiles[i] : blockingTilemap.GetTile(cell);

            if (tile == null)
                continue;

            if (visualTilemap != null && openedAlpha > 0f)
            {
                visualTilemap.SetTile(cell, tile);
                visualTilemap.SetTransformMatrix(cell, cachedTransforms[i]);
                visualTilemap.SetTileFlags(cell, TileFlags.None);

                Color color = cachedColors[i];
                color.a = openedAlpha;
                visualTilemap.SetColor(cell, color);
            }

            blockingTilemap.SetTile(cell, null);
        }

        blockingTilemap.RefreshAllTiles();

        if (visualTilemap != null)
            visualTilemap.RefreshAllTiles();
    }
}
