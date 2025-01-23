using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class DownloadList : ScriptableObject
{
    public string title = "DownloadList";
    public List<Content> list;

    void OnEnable()
    {
        if (list == null)
        {
            string json = Get();
            Debug.Log(json);

            list = JsonConvert.DeserializeObject<List<Content>>(json);
            Debug.Log(list.Count);

            //list = new List<Content>();

            //for (int i = 0; i < jd.Count; i++)
            //{
            //    Content content = new Content();
            //    content.filename = jd[i].ToString();
            //    list.Add(content);
            //}
        }
    }

    public void Add(Content content)
    {
        list.Add(content);
    }

    public void Remove(Content content)
    {
        list.Remove(content);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    static string Get()
    {
        string result = "";
        string url = "https://download.setsuodu.com/PokemonModels.php";

        //HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
        request.Method = "GET";

        //如果是发送HTTPS请求
        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request.ProtocolVersion = HttpVersion.Version10;
        }

        HttpWebResponse response = request.GetResponse() as HttpWebResponse;

        Stream receiveStream = response.GetResponseStream();
        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
        // Pipes the stream to a higher level stream reader with the required encoding format. 
        StreamReader readStream = new StreamReader(receiveStream, encode);

        char[] read = new char[256];
        // Reads 256 characters at a time.    
        int count = readStream.Read(read, 0, 256);

        while (count > 0)
        {
            // Dumps the 256 characters on a string and displays the string to the console.
            string str = new string(read, 0, count);
            //Debug.Log(str);
            count = readStream.Read(read, 0, 256);
            result += str;
        }

        // Releases the resources of the response.
        response.Close();
        // Releases the resources of the Stream.
        readStream.Close();

        return result;
    }

    private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        return true; //总是接受
    }
}

[System.Serializable]
public class Content
{
    public string filename;
}

public class CreateScriptableObject : Editor
{
    public static void CreateAsset<Type>() where Type : ScriptableObject
    {
        Type item = ScriptableObject.CreateInstance<Type>();

        string root = Application.dataPath + "/Resources";
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/" + typeof(Type) + ".asset");

        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }

    [MenuItem("Assets/Create/CreateDownloadList")]
    public static void CreateDownloadList()
    {
        CreateAsset<DownloadList>();
    }
}