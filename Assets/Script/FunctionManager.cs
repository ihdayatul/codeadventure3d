using System.Collections.Generic;
using UnityEngine;

public class FunctionManager : MonoBehaviour
{
    public static FunctionManager Instance { get; private set; }

    [Header("Function Storage")]
    public List<FunctionDefinitionBlock> definedFunctions = new List<FunctionDefinitionBlock>();

    void Awake()
    {
        // ✅ Pastikan GameObject ini root (tidak punya parent)
        if (transform.parent != null)
        {
            transform.SetParent(null);  // Lepas dari parent, jadikan root
            Debug.Log("FunctionManager dipindahkan ke root Hierarchy.");
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Sekarang aman
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Daftarkan fungsi (dipanggil saat blok definisi di-drop ke panel FUNCTION)
    public void RegisterFunction(FunctionDefinitionBlock func)
    {
        if (!definedFunctions.Contains(func))
        {
            definedFunctions.Add(func);
            Debug.Log($"✅ Fungsi '{func.functionName}' terdaftar. Total: {definedFunctions.Count}");
        }
    }

    // Hapus fungsi (saat blok definisi dihapus)
    public void UnregisterFunction(FunctionDefinitionBlock func)
    {
        if (definedFunctions.Contains(func))
        {
            definedFunctions.Remove(func);
            Debug.Log($"❌ Fungsi '{func.functionName}' dihapus. Total: {definedFunctions.Count}");
        }
    }

    // Cari fungsi berdasarkan nama
    public FunctionDefinitionBlock GetFunction(string name)
    {
        return definedFunctions.Find(f => f.functionName == name);
    }

    // Dapatkan semua nama fungsi (untuk dropdown)
    public List<string> GetAllFunctionNames()
    {
        List<string> names = new List<string>();
        foreach (var f in definedFunctions)
        {
            names.Add(f.functionName);
        }
        return names;
    }

    // Hapus semua fungsi (saat reset level)
    public void ClearAllFunctions()
    {
        definedFunctions.Clear();
    }
}