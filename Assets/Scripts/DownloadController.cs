using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class DownloadController : MonoBehaviour
{
    public static DownloadController instance;

    //private const string url = "http://www.manew.com/template/manew2016/images/logo.png";
    private const string url = "https://download.setsuodu.com/face3.jpg";

    [SerializeField] Button m_startButton;
    [SerializeField] string fileName;
    [SerializeField] string filePath;
    Thread thread;

    void Awake()
    {
        instance = this;

        m_startButton.onClick.AddListener(OnDownload);
    }

    void OnDestroy()
    {
        thread.Abort();

        m_startButton.onClick.RemoveListener(OnDownload);
    }

    void Start()
    {
        fileName = url.Split('/')[url.Split('/').Length - 1];
        filePath = Application.persistentDataPath + "/" + fileName;

        Debug.Log(GetLength(url));
    }

    void Update()
    {

    }

    void OnDownload()
    {
        MyThread mt = new MyThread(url, filePath, OnProgressChanged, OnCompleted);
        thread = new Thread(new ThreadStart(mt.DownLoadImage));
        thread.Start();
        Debug.Log("saved in: " + filePath);
    }

    void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string progress = string.Format("正在下载文件，完成进度{0}%  {1}/{2}(字节)", e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
        Debug.Log(progress);
    }

    void OnCompleted(object sender, AsyncCompletedEventArgs e)
    {
        string log = string.Format("主线程接收回调 OnCompleted " + e.UserState);
        Debug.Log(log);
    }

    // 获取下载文件的大小
    public static long GetLength(string url)
    {
        Debug.Log(url);

        //HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
        request.Method = "HEAD";

        //如果是发送HTTPS请求
        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request.ProtocolVersion = HttpVersion.Version10;
        }

        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        return response.ContentLength;
    }

    private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        return true; //总是接受
    }
}

public class MyThread
{
    public string _url;
    public string _filePath;
    public float _progress { get; private set; } //下载进度
    public bool _isDone { get; private set; } //是否下载完成
    public Action<object, DownloadProgressChangedEventArgs> _onProgressChanged;
    public Action<object, AsyncCompletedEventArgs> _onFileComplete;

    public MyThread(string url, string filePath, Action<object, DownloadProgressChangedEventArgs> progress, Action<object, AsyncCompletedEventArgs> complete)
    {
        _url = url;
        _filePath = filePath;
        _onProgressChanged = progress;
        _onFileComplete = complete;
    }

    public void DownLoadImage()
    {
        if (!File.Exists(_filePath))
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(_onProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(_onFileComplete);

            //webClient.DownloadFile(_url, _filePath); //同步
            Uri _uri = new Uri(_url);
            webClient.DownloadFileAsync(_uri, _filePath); //异步
        }
    }

    // 流写入本地
    private static byte[] SaveBytes(string filepath)
    {
        using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate))
        {
            byte[] bytes = new byte[(int)fs.Length];
            int read = fs.Read(bytes, 0, bytes.Length);

            fs.Dispose();
            fs.Close();

            return bytes;
        }
    }

    // 流计算MD5值
    private static string GetMD5Hash(byte[] bytedata)
    {
        try
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(bytedata);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5Hash() fail,error:" + ex.Message);
        }
    }
}
