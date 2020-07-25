﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class Simulator : MonoBehaviour
{
    [SerializeField] Toggle memoryKB, memoryMB;

    [SerializeField] InputField memorySizeInput;
    [SerializeField] InputField addressInput;
    [SerializeField] InputField cacheAccessTimeInput;
    [SerializeField] InputField missPenaltyTimeInput;
    [SerializeField] Text resultText;
    [SerializeField] Canvas mainCanvas;
    [SerializeField] Canvas instructionsCanvas;
    [SerializeField] Canvas errorCanvas;
    [SerializeField] Text errorText;
    

    private string hexAddress;
    private string binAddress;

    private float cacheAccessTime;
    private float missPenaltyTime;

    private float accessTime;

    private char[] binAddressChars;
    private int[] binAddressArray;

    private int addressCount;
    private long memorySize;

    private int offsetCount;
    private string offsetString;

    private string indexString;

    private int tagCount;
    private string tagString;

    private int accessTimesCount = 0;

    private bool isHit, isMiss;
    private int hitCount, missCount;
    private float hitRate, missRate;

    public string errorMessage;

    private int runCounter = 0;

    private bool isValid;

    private bool stepClicked, calculateClicked;

    List<string> LRU;
    int faCounter = 0, LRUlimit = 0;

    Cache cache;

    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Start")
        {
            return;
        }

        else
        {
            cache = FindObjectOfType<Cache>();
            mainCanvas.enabled = false;
            errorCanvas.enabled = false;

            LRU = new List<string>(cache.cache.Length);

            PlayerPrefs.DeleteAll();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Calculates total amount of address bits by Log of memory size
    public int AddressBitCount()
    {
        return addressCount = (int)Mathf.Log(memorySize, 2);
    }

    // Calculates the amount of offset bits by Log of block size
    public int OffsetBitCount()
    {
        return offsetCount = (int)Mathf.Log(cache.GetBlockSize(), 2);
    }

    // Calculates the amount of tag bits
    public int TagBitCount()
    {
        return tagCount = addressCount - cache.CacheSizeBitCount();
    }

    // Converts hexadecimal address to binary address (string)
    private void Hex2Bin()
    {
        binAddress = String.Join(String.Empty,
            hexAddress.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
    }

    /*  This function generates an integer array of binary address.
     *  It stores the hex address which by the time is already converted
     *  into a binary address string.
     */
    private void GenerateBinaryAddressArray()
    {
        binAddressChars = binAddress.ToCharArray();
        binAddressArray = new int[AddressBitCount()];

        for (int i = 0; i < binAddressArray.Length; i++)
        {
            binAddressArray[i] = 0;
        }

        for (int i = binAddressChars.Length-1; i >= 0; i--)
        {
            binAddressArray[i] = (int)Char.GetNumericValue(binAddressChars[i]);
        }
    }

    // Separates tag bits as a substring and stores it
    private void SetTag()
    {
        tagString = binAddress.Substring(0, tagCount);
    }

    // Separates index bits as a substring and stores it
    private void SetIndex()
    {
        indexString = binAddress.Substring(tagCount, cache.IndexBitCount());
    }

    // Separates offset bits as a substring and stores it
    private void SetOffset()
    {
        offsetString = binAddress.Substring(cache.IndexBitCount() + tagCount, offsetCount);
    }

    // Calculates hit rate
    private void SetHitRate()
    {
        hitRate = hitCount / (float)accessTimesCount;
    }

    // Calculates miss rate
    private void SetMissRate()
    {
        missRate = missCount / (float)accessTimesCount;
    }
    
    // Converts a binary string to a decimal integer
    private int Bin2Dec(string str)
    {
        int strBit;
        int result = 0;

        for(int i=0; i<str.Length; i++)
        {
            strBit = Int32.Parse(str[i].ToString());
            if(strBit == 1)
            {
                result += (int)Mathf.Pow(2, str.Length - 1 - i);
            }
        }

        return result;
        
    }

    // Maps the address from memory to cache (Direct Map)
    private void DirectMap()
    {
        int allocatedBlock = Bin2Dec(indexString);
        int mappedAddress = allocatedBlock % cache.cache.Length;
        int tagValue = Bin2Dec(tagString);

        if (cache.cache[mappedAddress] == 0)
        {
            isValid = false;
            isMiss = true;
            isHit = false;
            missCount++;
            cache.cache[mappedAddress] = 1;
            PlayerPrefs.SetInt(mappedAddress.ToString(), tagValue);
        }

        else if(cache.cache[mappedAddress] == 1)
        {
            isValid = true;
            if (PlayerPrefs.GetInt(mappedAddress.ToString()) == tagValue)
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
                PlayerPrefs.SetInt(mappedAddress.ToString(), tagValue);
            }
        }

        PlayerPrefs.Save();
    }

    // Fully Associative, on spectrum from Head to Tail of LRU list, head = most recent
    private void FullyAssociative()
    {
        //int address = Bin2Dec(binAddress);

        //if(LRU.Contains(binAddress))
        //{
        //    // move most recently used to the front
        //    LRU.Remove(binAddress);
        //    LRU.Insert(0, binAddress);
        //    hitCount++;
        //    isHit = true;
        //    isMiss = false;
        //    isValid = true;
        //}

        //else
        //{
        //    if(LRUlimit == cache.cache.Length)
        //    {
        //        for(int i=0; i<cache.cache.Length; i++)
        //        {
        //            if(cache.cache[i] == Bin2Dec(LRU[LRUlimit]))
        //            {
        //                cache.cache[i] = address;
        //            }
        //        }

        //        // victimize
        //        LRU.RemoveAt(LRU.Count);
        //        LRU.Insert(0, binAddress);
        //        missCount++;
        //        isMiss = true;
        //        isHit = false;
        //        isValid = false;
        //    }

        //    else
        //    {
        //        LRU.Insert(0, binAddress);
        //        //cache.cache[LRUlimit] = address;

        //        for(int i=0; i<cache.GetBlockSize(); i++)
        //        {
        //            cache.cache[LRUlimit] = address;
        //            address++;
        //            LRUlimit++;
        //        }

        //        //LRUlimit++;
        //        missCount++;
        //        isMiss = true;
        //        isHit = false;
        //        isValid = false;
        //    }
        //}
        
    }

    // Semi Associative
    private void SetAssociative()
    {

    }

    // Sets inputs
    private void SetInputs()
    {
        if (memoryKB.isOn == true)
        {
            memorySize = long.Parse(memorySizeInput.text) * 1024;
        }

        else if (memoryMB.isOn == true)
        {
            memorySize = long.Parse(memorySizeInput.text) * 1024 * 1024;
        }

        if(MemorySizeCheck())
        {
            hexAddress = addressInput.text;
            cacheAccessTime = int.Parse(cacheAccessTimeInput.text);
            missPenaltyTime = int.Parse(missPenaltyTimeInput.text);
        }
    }

    // Calculates parameters
    private void Quantify()
    {
        Hex2Bin();
        GenerateBinaryAddressArray();
        addressCount = AddressBitCount();

        if (MemorySizeCheck() && AddressLengthCheck())
        {
            if(SceneManager.GetActiveScene().name == "Fully Associative")
            {
                //
                tagCount = TagBitCount();
                offsetCount = OffsetBitCount();
                tagCount = TagBitCount();
                offsetCount = OffsetBitCount();
                offsetString = binAddress.Substring(cache.IndexBitCount() + tagCount, offsetCount);
                tagString = binAddress.Substring(0, tagCount + cache.IndexBitCount());
            }

            else
            {
                offsetCount = OffsetBitCount();
                tagCount = TagBitCount();
                SetTag();
                SetIndex();
                SetOffset();
            }
        }
    }


    // Core action of simulation
    private void RunDMSimulation()
    {
        DirectMap();
        DisplayResults();
    }
    private void RunSASimulation()
    {
        SetAssociative();
        DisplayResults();
    }
    private void RunFASimulation()
    {
        FullyAssociative();
        DisplayResults();
    }


    // Shows the results in a correct format
    private void DisplayResults()
    {
        if(CheckInputFields())
        {
            resultText.fontStyle = FontStyle.Normal;
            resultText.color = Color.black;
            resultText.fontSize = 25;
            resultText.alignment = TextAnchor.UpperLeft;

            if(stepClicked)
            {
                resultText.text = "Address in binary format:\n" + binAddress + "\n\n" +
                "Tag bits: " + tagString + "\n\n" +
                "Index bits: " + indexString + "\n\n" +
                "Block offset bits: " + offsetString + "\n\n" +
                "Hit: " + IsHit() + "\n" + "Miss: " + IsMiss() + "\n\n" +
                "Validity: " + isValid;
                accessTimesCount++;
            }

            else if(calculateClicked)
            {
                resultText.text = "Access times count: " + accessTimesCount + "\n\n" +
                    "Hit count: " + hitCount + "\n" + "Hit rate: " + hitRate + "\n\n" +
                    "Miss count: " + missCount + "\n" + "Miss rate: " + missRate + "\n\n" + 
                    "Access Time: " + accessTime;
            }
            
        }

        else
        {
            ShowErrorCanvas();
        }
    }


    // Verifying data layers
    private bool CheckInputFields()
    {
        if (string.IsNullOrEmpty(memorySizeInput.text) || string.IsNullOrEmpty(addressInput.text) ||
            string.IsNullOrEmpty(cacheAccessTimeInput.text) || string.IsNullOrEmpty(missPenaltyTimeInput.text))
        {
            errorMessage = "Please fill all the blank fields";
            errorText.text = errorMessage;
            ShowErrorCanvas();

            return false;
        }

        else
        {
            SetInputs();
            Quantify();

            return true;
        }

    }
    private bool MemorySizeCheck()
    {
        if (memorySize < cache.GetCacheSize())
        {
            errorMessage = "Cache must be smaller than memory size";
            errorText.text = errorMessage;
            ShowErrorCanvas();

            return false;
        }

        else
        {
            return true;
        }
    }
    private bool AddressLengthCheck()
    {
        if (binAddress.Length != addressCount)
        {
            errorMessage = "Enter your address correctly";
            errorText.text = errorMessage;
            ShowErrorCanvas();

            return false;
        }

        else
        {
            return true;
        }
    }


    public int GetOffsetCount()
    {
        return offsetCount;
    }
    public void ShowErrorCanvas()
    {
        errorCanvas.enabled = true;
    }
    private bool IsHit()
    {
        return isHit;
    }
    private bool IsMiss()
    {
        return isMiss;
    }


    private void StepDM()
    {
        stepClicked = true;
        calculateClicked = false;

        if (cache.VerifyInputs())
        {
            cache.CacheInit();

            if (CheckInputFields())
            {
                if (runCounter == 0 && AddressLengthCheck())
                {
                    cache.InitCacheArray();
                    runCounter++;
                }

                RunDMSimulation();
            }
        }

        else
        {
            DisplayResults();
        }
    }
    private void StepSA()
    {

    }
    private void StepFA()
    {
        stepClicked = true;
        calculateClicked = false;

        if (cache.VerifyInputs())
        {
            cache.CacheInit();

            if (CheckInputFields())
            {
                if (runCounter == 0 && AddressLengthCheck())
                {
                    cache.InitCacheArray();
                    runCounter++;
                }

                RunFASimulation();
            }
        }

        else
        {
            DisplayResults();
        }
    }


    // Button functions
    public void Step()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1: StepDM(); break;
            case 2: StepSA(); break;
            case 3: StepFA(); break;
        }
    }
    public void Calculate()
    {
        calculateClicked = true;
        stepClicked = false;

        if (cache.VerifyInputs())
        {
            cache.CacheInit();

            if (CheckInputFields())
            {
                SetHitRate();
                SetMissRate();

                accessTime = cacheAccessTime + (missRate * missPenaltyTime);

                DisplayResults();
            }
        }
    }
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
    public void ErrorDone()
    {
        errorCanvas.enabled = false;
        mainCanvas.enabled = true;
    }
    public void GoToDirectMap()
    {
        SceneManager.LoadScene("Direct Map");
    }
    public void GoToSetAssociative()
    {
        SceneManager.LoadScene("Set Associative");
    }
    public void GoToFullyAssociative()
    {
        SceneManager.LoadScene("Fully Associative");
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
