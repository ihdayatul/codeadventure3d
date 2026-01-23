using UnityEngine;

[ExecuteInEditMode]
public class GridTextureGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 10; // Jumlah kotak grid
    public Color gridColor = Color.white;
    public float lineWidth = 2f; // Ketebalan garis dalam pixel
    public Color backgroundColor = new Color(0, 0, 0, 0); // Transparan

    [Header("Texture Settings")]
    public int textureSize = 1024; // Ukuran texture (power of 2)
    public string savePath = "Assets/Textures/GridTexture.png";

    private Texture2D gridTexture;

    [ContextMenu("Generate Grid Texture")]
    public void GenerateGridTexture()
    {
        // Buat texture baru
        gridTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        // Hitung ukuran per grid dalam pixel
        float cellSize = (float)textureSize / gridSize;
        int lineWidthPixels = Mathf.Max(1, Mathf.RoundToInt(lineWidth));

        // Isi texture dengan background
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                gridTexture.SetPixel(x, y, backgroundColor);
            }
        }

        // Gambar garis vertikal
        for (int i = 0; i <= gridSize; i++)
        {
            int xPos = Mathf.RoundToInt(i * cellSize);
            for (int w = 0; w < lineWidthPixels; w++)
            {
                int currentX = Mathf.Clamp(xPos + w, 0, textureSize - 1);
                for (int y = 0; y < textureSize; y++)
                {
                    gridTexture.SetPixel(currentX, y, gridColor);
                }
            }
        }

        // Gambar garis horizontal
        for (int i = 0; i <= gridSize; i++)
        {
            int yPos = Mathf.RoundToInt(i * cellSize);
            for (int w = 0; w < lineWidthPixels; w++)
            {
                int currentY = Mathf.Clamp(yPos + w, 0, textureSize - 1);
                for (int x = 0; x < textureSize; x++)
                {
                    gridTexture.SetPixel(x, currentY, gridColor);
                }
            }
        }

        gridTexture.Apply();

        // Simpan sebagai PNG
        byte[] pngData = gridTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath, pngData);

        Debug.Log($"Grid texture berhasil dibuat: {savePath}");

        // Refresh asset database
        UnityEditor.AssetDatabase.Refresh();
    }

    void Start()
    {
        GenerateGridTexture();
    }
}