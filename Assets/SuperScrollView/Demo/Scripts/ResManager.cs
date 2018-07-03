using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperScrollView
{
    public class ResManager : MonoBehaviour
    {
        public static ResManager instance = null;

        Dictionary<string, Sprite> spriteObjDict = new Dictionary<string, Sprite>();

        public List<string> urlList;
        public List<string> pathList;
        public List<byte[]> bytesList = new List<byte[]>();
        public List<Sprite> spriteList = new List<Sprite>();

        void Awake()
        {
            instance = null;
            InitData();
        }

        void Update()
        {
            if (bytesList.Count > 0)
            {
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

            TextAsset asset = Resources.Load<TextAsset>("list");
            string content = asset.text;
            string[] array = content.Split('\n');
            for (int i = 0; i < array.Length; i++)
            {
                string fileurl = array[i];
                if (!fileurl.EndsWith("g"))
                {
                    fileurl = array[i].Substring(0, array[i].Length - 1);
                }
                urlList.Add(fileurl);

                /*
                string[] filename = fileurl.Split('/');
                string filepath = "C:/Users/user/Desktop/" + filename[filename.Length - 1];
                if (!filepath.EndsWith("g"))
                {
                    Debug.Log(i);
                    filepath = filepath.Substring(0, filepath.Length - 1);
                }
                pathList.Add(filepath);
                */

                //MyThread mt = new MyThread(urlList[i], pathList[i]);
                //Thread thread = new Thread(new ThreadStart(mt.DownLoadImage));
                //thread.Start();
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

        // Example

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

            string[] filename = urlList[index].Split('/');
            string filepath = "C:/Users/user/Desktop/" + filename[filename.Length - 1];
            if (!filepath.EndsWith("g"))
            {
                filepath = filepath.Substring(0, filepath.Length - 1);
            }
            pathList.Add(filepath);

            //Debug.Log(urlList[index] + "\n" + pathList[index]);
            MyThread mt = new MyThread(urlList[index], pathList[index]);
            Thread thread = new Thread(new ThreadStart(mt.DownLoadImage));
            thread.Start();

            return urlList[index];
        }
    }

    public class MyThread
    {
        public string _url;
        public string _filePath;
        public Texture2D t2d;

        public MyThread(string url, string filePath)
        {
            _url = url;
            _filePath = filePath;
        }

        public void DownLoadImage()
        {
            WebClient web = new WebClient();
            web.DownloadFile(_url, _filePath);
            ResManager.instance.bytesList.Add(SaveBytes(_filePath));
        }

        public byte[] SaveBytes(string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                int read = fs.Read(bytes, 0, bytes.Length);

                fs.Dispose();
                fs.Close();

                return bytes;
            }
        }
    }
}
