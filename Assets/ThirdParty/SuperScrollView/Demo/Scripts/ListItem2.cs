using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuperScrollView
{
    public class ListItem2 : MonoBehaviour
    {
        public Text mNameText;
        public Image mIcon;
        public Image[] mStarArray;
        public Text mDescText;
        public Text mDescText2;
        public Color32 mRedStarColor = new Color32(249, 227, 101, 255);
        public Color32 mGrayStarColor = new Color32(215, 215, 215, 255);
        public GameObject mContentRootObj;
        int mItemDataIndex = -1;
        public LoopListView2 mLoopListView;
        [SerializeField] Sprite placeholder;

        public void Init()
        {
            for(int i = 0;i<mStarArray.Length;++i)
            {
                int index = i;
                ClickEventListener listener = ClickEventListener.Get(mStarArray[i].gameObject);
                listener.SetClickEventHandler(delegate (GameObject obj) { OnStarClicked(index); });
            }
        }

        void OnStarClicked(int index)
        {
            ItemData data = DataSourceMgr.Get.GetItemDataByIndex(mItemDataIndex);
            if(data == null)
            {
                return;
            }
            if(index == 0 && data.mStarCount == 1)
            {
                data.mStarCount = 0;
            }
            else
            {
                data.mStarCount = index + 1;
            }
            SetStarCount(data.mStarCount);
        }

        public void SetStarCount(int count)
        {
            int i = 0;
            for(; i<count;++i)
            {
                mStarArray[i].color = mRedStarColor;
            }
            for (; i < mStarArray.Length; ++i)
            {
                mStarArray[i].color = mGrayStarColor;
            }
        }

        public void SetItemData(ItemData itemData,int itemIndex)
        {
            mItemDataIndex = itemIndex;
            mNameText.text = itemData.mName;
            mDescText.text = itemData.mFileSize.ToString() + "KB";
            mDescText2.text = itemData.mDesc;

            // 通过一个特效从placeholder切到要加载的图
            //mIcon.sprite = ResManager.Get.GetSpriteByIndex(itemIndex); //还没有图
            mIcon.sprite = placeholder;
            if (itemIndex < ResManager.instance.urlList.Count)
            {
                StartCoroutine(SetSprite(itemIndex));
            }

            SetStarCount(itemData.mStarCount);
        }

        IEnumerator SetSprite(int itemIndex)
        {
            yield return new WaitUntil(()=> ResManager.Get.GetSpriteByIndex(itemIndex) != null);

            // 通过一个特效从placeholder切到要加载的图
            mIcon.sprite = ResManager.Get.GetSpriteByIndex(itemIndex);
        }
    }
}
