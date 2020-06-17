using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cache : MonoBehaviour
{
    [SerializeField] Toggle cacheKB, cacheMB;

    [SerializeField] InputField cacheSizeInput;
    [SerializeField] InputField blockSizeInput;

    public int[] cache;

    private long cacheSize;
    private int blockSize;
    private int indexCount;
    private int cacheSizeCount;

    private string errorMessage;

    Simulator simulator;


    // Start is called before the first frame update
    void Start()
    {
        simulator = FindObjectOfType<Simulator>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    // Calculates the amount of index bits
    public int IndexBitCount()
    {
        return indexCount = (int)Mathf.Log(cacheSize / blockSize, 2);
    }

    // Calculates the amount of cache size bits
    public int CacheSizeBitCount()
    {
        return cacheSizeCount = simulator.GetOffsetCount() + IndexBitCount();
    }

    // Creates an array with size of (#index + #offset)
    public void InitCacheArray()
    {
        cache = new int[IndexBitCount() + simulator.OffsetBitCount()];

        for (int i = 0; i < cache.Length; i++)
        {
            cache[i] = 0;
        }
    }

    // Cache initialization
    public void CacheInit()
    {
        SetInputs();
        Quantify();
    }

    // Sets inputs
    private void SetInputs()
    {
        if (cacheKB.isOn == true)
        {
            cacheSize = long.Parse(cacheSizeInput.text) * 1024;
        }

        else if (cacheMB.isOn == true)
        {
            cacheSize = long.Parse(cacheSizeInput.text) * 1024 * 1024;
        }

        blockSize = int.Parse(blockSizeInput.text);
    }

    // Calculates parameters
    private void Quantify()
    {
        cacheSizeCount = CacheSizeBitCount();
        indexCount = IndexBitCount();
    }

    public bool VerifyInputs()
    {
        if(string.IsNullOrEmpty(cacheSizeInput.text) || string.IsNullOrEmpty(blockSizeInput.text))
        {
            simulator.errorMessage = "Please fill all the blank fields";
            simulator.ShowErrorCanvas();
            return false;
        }

        else
        {
            return true;
        }
    }

    public int GetBlockSize()
    {
        return blockSize;
    }

    public long GetCacheSize()
    {
        return cacheSize;
    }

}
