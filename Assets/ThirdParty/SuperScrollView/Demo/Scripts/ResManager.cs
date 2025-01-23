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

namespace SuperScrollView
{
    public class ResManager : MonoBehaviour
    {
        public static ResManager instance = null;

        Dictionary<string, Sprite> spriteObjDict = new Dictionary<string, Sprite>();

        public List<string> urlList;
        public List<string> pathList;
        public List<Sprite> spriteList = new List<Sprite>();

        public List<byte[]> bytesList = new List<byte[]>();

        private string outputFolder
        {
            get
            {
                return "C:/Users/user/Desktop/";
            }
        }

        void Awake()
        {
            instance = null;
            InitData();
        }

        void Update()
        {
            if (bytesList.Count > 0)
            {
                Debug.Log(bytesList.Count + "|" + bytesList[0].Length);

                Texture2D t2d = new Texture2D(128, 128);
                t2d.LoadImage(bytesList[0]);
                t2d.Apply();

                Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
                spriteList.Add(sp);

                bytesList.RemoveAt(0);
            }
        }

        public static ResManager Get
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindObjectOfType<ResManager>();
                }
                return instance;
            }
        }

        void InitData()
        {
            spriteObjDict.Clear();

            string baseUrl = @"https://download.setsuodu.com/Pokemon Models/";
            DownloadList downloadList = Resources.Load<DownloadList>("DownloadList");
            //Debug.Log(downloadList.list.Count);
            for (int i = 0; i < downloadList.list.Count; i++)
            {
                string fileurl = baseUrl + downloadList.list[i].filename;
                urlList.Add(fileurl);
            }
        }

        public Sprite GetSpriteByName(string spriteName)
        {
            Sprite ret = null;
            if (spriteObjDict.TryGetValue(spriteName, out ret))
            {
                return ret;
            }
            return ret;
        }

        public Sprite GetSpriteByIndex(int index)
        {
            Sprite ret = null;
            if (spriteList.Count - 1 >= index)
            {
                ret = spriteList[index];
            }
            return ret;
        }

        public string GetSpriteNameByIndex(int index)
        {
            if (index < 0 || index >= urlList.Count)
            {
                return "";
            }
            return urlList[index];
        }

        public string GetUrl(int index)
        {
            if (index < 0 || index >= urlList.Count)
            {
                return "";
            }

            string url = urlList[index];
            long fileSize = GetLength(url); //必须运行
            //Debug.Log(fileSize);

            string[] filename = url.Split('/');
            string filepath = Path.Combine(outputFolder, filename[filename.Length - 1]);
            pathList.Add(filepath);

            //Debug.Log(url + "\n" + pathList[index]);
            MyThread mt = new MyThread(url, pathList[index], OnProgressChanged, OnCompleted);
            Thread thread = new Thread(new ThreadStart(mt.DownLoadImage));
            thread.Start();
            //Debug.Log("saved in: " + filepath);

            return urlList[index];
        }

        void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string progress = string.Format("正在下载文件，完成进度{0}%  {1}/{2}(字节)", e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
            //Debug.Log(progress);
        }

        void OnCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Debug.Log("主线程完成");
        }

        // 获取下载文件的大小
        public static long GetLength(string url)
        {
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
            // 按 UrlList 的顺序
            if (!File.Exists(_filePath))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(_url, _filePath);
            }

            FileStream fs = new FileStream(_filePath, FileMode.OpenOrCreate);
            byte[] bytes = new byte[(int)fs.Length];
            int read = fs.Read(bytes, 0, bytes.Length);
            fs.Dispose();
            fs.Close();

            ResManager.instance.bytesList.Add(bytes);
            Debug.Log("流写入本地:" + bytes.Length);
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
}
