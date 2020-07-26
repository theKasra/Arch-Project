using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class Program : MonoBehaviour
{
    [SerializeField] Toggle memoryKB, memoryMB;
    [SerializeField] Toggle cacheKB, cacheMB;

    [SerializeField] InputField memorySizeInput;
    [SerializeField] InputField cacheSizeInput;
    [SerializeField] InputField blockSizeInput;
    [SerializeField] InputField addressInput;
    [SerializeField] InputField cacheAccessTimeInput;
    [SerializeField] InputField missPenaltyTimeInput;
    [SerializeField] InputField setsInput;
    [SerializeField] InputField waysInput;

    [SerializeField] Text resultText;

    [SerializeField] Canvas mainCanvas;
    [SerializeField] Canvas instructionsCanvas;
    [SerializeField] Canvas errorCanvas;
    [SerializeField] Text errorText;

    private long memorySize, cacheSize;
    private int blockSize;
    private float cacheAccessTime, missPenaltyTime;
    private string hexAddress;

    private string binAddress;

    private int[] dmCache;
    private bool[,] tagIndexTable;

    private List<string> LRU;
    private int LRUlimit;

    private int[][] jaggedArray;
    private int sets;
    private int ways;

    private int runCounter;

    private bool stepClicked, calculateClicked;
    private float accessTime;
    private int accessTimesCount = 0;
    private bool isHit, isMiss, isValid;
    private int hitCount, missCount;

    // Start is called before the first frame update
    void Start()
    {
        runCounter = 0;
        instructionsCanvas.enabled = true;
        mainCanvas.enabled = false;
        errorCanvas.enabled = false;
    }

    // Converts hexadecimal address to binary address
    private string Hex2Bin(string hex)
    {
        return binAddress = string.Join(string.Empty,
            hex.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).
            PadLeft(4, '0')));
    }
    // Converts a binary string to a decimal integer
    private int Bin2Dec(string bin)
    {
        int strBit;
        int result = 0;

        for (int i = 0; i < bin.Length; i++)
        {
            strBit = int.Parse(bin[i].ToString());
            if (strBit == 1)
            {
                result += (int)Mathf.Pow(2, bin.Length - 1 - i);
            }
        }
        return result;
    }


    // Sets inputs
    private void SetInputs()
    {
        try
        {
            if (memoryKB.isOn == true)
            {
                memorySize = long.Parse(memorySizeInput.text) * 1024;
            }

            else if (memoryMB.isOn == true)
            {
                memorySize = long.Parse(memorySizeInput.text) * 1024 * 1024;
            }

            if (cacheKB.isOn == true)
            {
                cacheSize = long.Parse(cacheSizeInput.text) * 1024;
            }

            else if (cacheMB.isOn == true)
            {
                cacheSize = long.Parse(cacheSizeInput.text) * 1024 * 1024;
            }



            hexAddress = addressInput.text;
            Hex2Bin(hexAddress);

            cacheAccessTime = float.Parse(cacheAccessTimeInput.text);
            missPenaltyTime = float.Parse(missPenaltyTimeInput.text);
            blockSize = int.Parse(blockSizeInput.text);

            if (SceneManager.GetActiveScene().name == "Direct Map" && runCounter == 0)
            {
                tagIndexTable = new bool[(int)Math.Pow(2, TagBitCount()), (int)Math.Pow(2, IndexBitCount())];
                dmCache = new int[cacheSize];
                runCounter++;
            }

            else if (SceneManager.GetActiveScene().name == "Fully Associative" && runCounter == 0)
            {
                LRUlimit = (int)Math.Pow(2, TagBitCount());
                LRU = new List<string>(LRUlimit);
                runCounter++;
            }

            else if (SceneManager.GetActiveScene().name == "Set Associative")
            {
                sets = int.Parse(setsInput.text);
                ways = int.Parse(waysInput.text);

                if (runCounter == 0)
                {
                    jaggedArray = new int[sets][];

                    for (int i = 0; i < sets; i++)
                    {
                        jaggedArray[i] = new int[ways];
                    }
                    
                    setsInput.enabled = false;
                    waysInput.enabled = false;
                    runCounter++;
                }
            }
        }
        catch (Exception e)
        {
            errorText.text = e.Message;
            ShowErrorCanvas();
            throw;
        }

        
    }


    // Counts total amount of address bits
    public int AddressBitCount()
    {
        return (int)Mathf.Log(memorySize, 2);
    }
    // Counts the amount of offset bits
    public int OffsetBitCount()
    {
        return (int)Mathf.Log(blockSize, 2);
    }
    // Counts the amount of index bits
    public int IndexBitCount()
    {
        if (SceneManager.GetActiveScene().name == "Direct Map")
        {
            return (int)Mathf.Log(cacheSize / blockSize, 2);
        }

        else if (SceneManager.GetActiveScene().name == "Fully Associative")
        {
            return 0;
        }

        else
        {
            return (int)Mathf.Log(sets, 2);
        }
    }
    // Counts the amount of tag bits
    public int TagBitCount()
    {
        return AddressBitCount() - (IndexBitCount() + OffsetBitCount());
    }


    // Separates tag bits as a substring
    private string Tag()
    {
        return binAddress.Substring(0, TagBitCount());
    }
    // Separates index bits as a substring
    private string Index()
    {
        return binAddress.Substring(TagBitCount(), IndexBitCount());
    }
    // Separates offset bits as a substring
    private string Offset()
    {
        return binAddress.Substring(TagBitCount() + IndexBitCount(), OffsetBitCount());
    }


    private void DirectMap()
    {
        int index = Bin2Dec(Index());
        int tag = Bin2Dec(Tag());

        if(dmCache[index] == 0)
        {
            isValid = false;
            isMiss = true;
            isHit = false;
            missCount++;
            dmCache[index] = 1;
            tagIndexTable[tag, index] = true;
        }

        else
        {
            isValid = true;
            if(tagIndexTable[tag, index] == true)
            {
                isHit = true;
                isMiss = false;
                hitCount++;
            }

            else
            {
                isMiss = true;
                isHit = false;
                missCount++;
                tagIndexTable[tag, index] = true;
            }
        }
    }
    private void FullyAssociative()
    {
        string tag = Tag();
        
        if(LRU.Contains(tag))
        {
            isValid = true;
            isHit = true;
            isMiss = false;
            hitCount++;

            // most recently used most be at the top
            LRU.RemoveAt(LRU.IndexOf(tag));
            LRU.Insert(0, tag);
        }

        else
        {
            if(LRU.Count == LRUlimit)   // Full
            {
                // Victimize
                LRU.RemoveAt(LRUlimit);
                LRU.Insert(0, tag);
                isValid = false;
                isMiss = true;
                isHit = false;
                missCount++;
            }

            else
            {
                LRU.Add(tag);
                isValid = false;
                isMiss = true;
                isHit = false;
                missCount++;
            }
        }
    }
    private void SetAssociative()
    {
        int index = Bin2Dec(Index());
        int offset = Bin2Dec(Offset());
        int tag = Bin2Dec(Tag());

        if(jaggedArray[index][offset] == tag)
        {
            isValid = true;
            isHit = true;
            isMiss = false;
            hitCount++;
        }

        else
        {
            for(int i=0; i<ways; i++)
            {
                if(jaggedArray[index][i] == 0)
                {
                    isValid = false;
                    isMiss = true;
                    isHit = false;
                    missCount++;
                    jaggedArray[index][i] = tag;
                }
            }
        }
    }

    // Step and Calculate button functions
    public void Step()
    {
        stepClicked = true;
        calculateClicked = false;

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1: DM(); break;
            case 2: SA(); break;
            case 3: FA(); break;
        }
    }
    public void Calculate()
    {
        calculateClicked = true;
        stepClicked = false;

        accessTime = cacheAccessTime + (MissRate() * missPenaltyTime);

        DisplayResults();
    }


    private void DisplayResults()
    {
        resultText.fontStyle = FontStyle.Normal;
        resultText.color = Color.black;
        resultText.fontSize = 25;
        resultText.alignment = TextAnchor.UpperLeft;

        if (stepClicked)
        {
            resultText.text = "Address in binary format:\n" + binAddress + "\n\n" +
            "Tag bits: " + Tag() + "\n\n" +
            "Index bits: " + Index() + "\n\n" +
            "Block offset bits: " + Offset() + "\n\n" +
            "Hit: " + isHit + "\n" + "Miss: " + isMiss + "\n\n" +
            "Validity: " + isValid;
        }

        else if (calculateClicked)
        {
            resultText.text = "Access times count: " + accessTimesCount + "\n\n" +
                "Hit count: " + hitCount + "\n" + "Hit rate: " + HitRate() + "\n\n" +
                "Miss count: " + missCount + "\n" + "Miss rate: " + MissRate() + "\n\n" +
                "Access Time: " + accessTime;
        }
    }


    // returns Hit rate and Miss rate
    private float HitRate()
    {
        return hitCount / (float)accessTimesCount;
    }
    private float MissRate()
    {
        return missCount / (float)accessTimesCount;
    }


    private void DM()
    {
        SetInputs();
        DirectMap();
        DisplayResults();
        accessTimesCount++;
    }
    private void FA()
    {
        SetInputs();
        FullyAssociative();
        DisplayResults();
        accessTimesCount++;
    }
    private void SA()
    {
        SetInputs();
        SetAssociative();
        DisplayResults();
        accessTimesCount++;
    }


    // Button functions
    public void ShowInstructions()
    {
        mainCanvas.enabled = false;
        instructionsCanvas.enabled = true;
    }
    public void HideInstructions()
    {
        instructionsCanvas.enabled = false;
        mainCanvas.enabled = true;
    }
    public void ShowErrorCanvas()
    {
        mainCanvas.enabled = false;
        errorCanvas.enabled = true;
    }
    public void ErrorDone()
    {
        errorCanvas.enabled = false;
        mainCanvas.enabled = true;
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void Quit()
    {
        Application.Quit();
    }

}
