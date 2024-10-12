// Script that interacts with JS of the website to access localStorage

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class UserDataManager
{
    private const string SAVE_PATH = "chessly/lastsaved/";

    public static string PrefixKey(string key)
    {
        return SAVE_PATH + key;
    }

    public static void SetString(string key, string data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        saveData(PrefixKey(key), data);
#else
        PlayerPrefs.SetString(key, data);
#endif
    }

    public static void SetInt(string key, int data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        saveData(PrefixKey(key), data.ToString());
#else
        PlayerPrefs.SetInt(key, data);
#endif
    }

    public static void SetFloat(string key, float data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        saveData(PrefixKey(key), data.ToString());
#else
        PlayerPrefs.SetFloat(key, data);
#endif
    }

    public static string GetString(string key, string defaultValue = "")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        IntPtr dataPtr = loadData(PrefixKey(key));  // Get the pointer to the string data
        string data = Marshal.PtrToStringUTF8(dataPtr);  // Convert the pointer to a C# string
        if (string.IsNullOrEmpty(data))
        {
            return defaultValue;
        }
        return data;
#else
        return PlayerPrefs.GetString(key, defaultValue);
#endif
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var data = GetString(key);
        if (!string.IsNullOrEmpty(data) && int.TryParse(data, out int result))
        {
            return result;
        }
        return defaultValue;
#else
        return PlayerPrefs.GetInt(key, defaultValue);
#endif
    }

    public static float GetFloat(string key, float defaultValue = 0)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var data = GetString(key);
        if (!string.IsNullOrEmpty(data) && float.TryParse(data, out float result))
        {
            return result;
        }
        return defaultValue;
#else
        return PlayerPrefs.GetFloat(key, defaultValue);
#endif
    }

    public static bool HasKey(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var data = GetString(key);
        return !string.IsNullOrEmpty(data);
#else
        return PlayerPrefs.HasKey(key);
#endif
    }

    public static void DeleteKey(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        deleteKey(PrefixKey(key));
#else
        PlayerPrefs.DeleteKey(key);
#endif
    }

    public static void DeleteAllKeys(string prefix)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        deleteAllKeys(PrefixKey(prefix));
#else
        PlayerPrefs.DeleteAll();
#endif
    }

    // DLL Imports for interacting with the WebGL JavaScript library
    [DllImport("__Internal")]
    private static extern void saveData(string key, string data);

    [DllImport("__Internal")]
    private static extern IntPtr loadData(string key);  // Use IntPtr to handle pointer return type

    [DllImport("__Internal")]
    private static extern void deleteKey(string key);  // Changed to void

    [DllImport("__Internal")]
    private static extern void deleteAllKeys(string prefix);  // Changed to void
}
