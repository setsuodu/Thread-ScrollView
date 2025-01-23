using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using PimDeWitte.UnityMainThreadDispatcher;

public class DownloadController : MonoBehaviour
{
    private const string SAMEPLE_IMAGE_URL = "https://k.sinaimg.cn/n/sports/transform/180/w640h340/20250113/4877-d887f7373f49677ec3a2505750441f61.jpg/w640h340z1l0t0q75a7d.jpg";

    [SerializeField] Button m_StartBtn;
    [SerializeField] RawImage m_Image;
    [SerializeField] string fileName;
    [SerializeField] string filePath;
    Thread thread;
    public static Action OnLoadImage;

    void Awake()
    {
        m_StartBtn.onClick.AddListener(OnDownload);
        OnLoadImage += LoadImage;
    }

    void OnDestroy()
    {
        thread?.Abort();

        m_StartBtn.onClick.RemoveListener(OnDownload);
    }

    void Start()
    {
        fileName = SAMEPLE_IMAGE_URL.Split('/')[SAMEPLE_IMAGE_URL.Split('/').Length - 1];
        filePath = Application.persistentDataPath + "/" + fileName;
        Debug.Log($"file size: {GetLength(SAMEPLE_IMAGE_URL)} byte");
    }

    void OnDownload()
    {
        MyThread mt = new MyThread(SAMEPLE_IMAGE_URL, filePath, OnProgressChanged, OnCompleted);
        thread = new Thread(new ThreadStart(mt.DownLoadImage));
        thread.Start();
        Debug.Log("saved in: " + filePath);
    }

    void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string progress = string.Format("正在下载文件，完成进度{0}%  {1}/{2}(byte)", e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
        Debug.Log(progress);
    }

    void OnCompleted(object sender, AsyncCompletedEventArgs e)
    {
        string log = string.Format("主线程接收回调 OnCompleted " + e.UserState);
        Debug.Log(log);

        // 这里是线程里，需要到主线程中更新UI
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            // UI更新代码
            DownloadController.OnLoadImage();
        });
    }

    public void LoadImage()
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Debug.Log($"image fileData = {fileData.Length}");
        // 创建Texture2D对象
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        m_Image.texture = texture;
    }

    // 获取下载文件的大小，确保数据完整性
    static long GetLength(string url)
    {
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        request.Method = "HEAD";
        Debug.Log(url);

        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            ServicePointManager.ServerCertificateValidationCallback = 
                new RemoteCertificateValidationCallback((sender, certificate, chain, errors) => { return true; });
            request.ProtocolVersion = HttpVersion.Version10;
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        return response.ContentLength;
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
        else
        {
            // 这里是线程里，需要到主线程中更新UI
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                // UI更新代码
                DownloadController.OnLoadImage();
            });
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