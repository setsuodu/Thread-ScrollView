using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuperScrollView
{
    public class PullToLoadMoreDemoScript : MonoBehaviour
    {
        public LoopListView2 mLoopListView;
        [SerializeField] LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
        float mDataLoadedTipShowLeftTime = 0;
        float mLoadingTipItemHeight = 100;
        [SerializeField, Tooltip("加载增量")] int mLoadMoreCount = 10;

        void Start()
        {
            // totalItemCount +1 because the last "load more" banner is also a item.
            mLoopListView.InitListView(DataSourceMgr.Get.TotalItemCount + 1, OnGetItemByIndex);
            mLoopListView.mOnBeginDragAction = OnBeginDrag;
            mLoopListView.mOnDragingAction = OnDraging;
            mLoopListView.mOnEndDragAction = OnEndDrag;
        }

        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (index < 0)
            {
                return null;
            }

            LoopListViewItem2 item = null;
            if (index == 0)
            {
                item = listView.NewListViewItem("ItemPrefab2");
                UpdateLoadingTip(item);
                return item;
            }
            else if (index == DataSourceMgr.Get.TotalItemCount)
            {
                item = listView.NewListViewItem("ItemPrefab0");
                UpdateLoadingTip(item);
                return item;
            }

            ItemData itemData = DataSourceMgr.Get.GetItemDataByIndex(index);
            if (itemData == null)
            {
                return null;
            }

            item = listView.NewListViewItem("ItemPrefab1");
            ListItem2 itemScript = item.GetComponent<ListItem2>();
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                itemScript.Init();
            }
            if (index == DataSourceMgr.Get.TotalItemCount - 1)
            {
                item.Padding = 0;
            }
            itemScript.SetItemData(itemData, index);
            return item;
        }

        void UpdateLoadingTip(LoopListViewItem2 item)
        {
            if (item == null)
            {
                return;
            }

            ListItem0 itemScript0 = item.GetComponent<ListItem0>();
            if (itemScript0 == null)
            {
                return;
            }

            if (mLoadingTipStatus == LoadingTipStatus.None)
            {
                itemScript0.mRoot.SetActive(false);
                item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
                //Debug.Log("1->0");
            }
            else if (mLoadingTipStatus == LoadingTipStatus.WaitRelease)
            {
                itemScript0.mRoot.SetActive(true);
                itemScript0.mText.text = "Release to Load More";
                itemScript0.mArrow.SetActive(true);
                itemScript0.mWaitingIcon.SetActive(false);
                item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight);
                //Debug.Log("2->" + mLoadingTipItemHeight);
            }
            else if (mLoadingTipStatus == LoadingTipStatus.WaitLoad)
            {
                itemScript0.mRoot.SetActive(true);
                itemScript0.mArrow.SetActive(false);
                itemScript0.mWaitingIcon.SetActive(true);
                itemScript0.mText.text = "Loading ...";
                item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight);
                //Debug.Log("3->" + mLoadingTipItemHeight);
            }
        }

        void OnBeginDrag()
        {

        }

        void OnDraging()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }
            if (mLoadingTipStatus != LoadingTipStatus.None && mLoadingTipStatus != LoadingTipStatus.WaitRelease)
            {
                return;
            }
            ///*
            // 加载更多
            LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(DataSourceMgr.Get.TotalItemCount);
            if (item != null)
            {
                LoopListViewItem2 item1 = mLoopListView.GetShownItemByItemIndex(DataSourceMgr.Get.TotalItemCount - 1);
                if (item1 == null)
                {
                    return;
                }

                float y = mLoopListView.GetItemCornerPosInViewPort(item1, ItemCornerEnum.LeftBottom).y;
                if (y + mLoopListView.ViewPortSize >= mLoadingTipItemHeight)
                {
                    if (mLoadingTipStatus != LoadingTipStatus.None)
                    {
                        return;
                    }
                    mLoadingTipStatus = LoadingTipStatus.WaitRelease;
                    UpdateLoadingTip(item);
                }
                else
                {
                    if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
                    {
                        Debug.Log("Draging ==>> RETURN");
                        //return;
                    }
                    else
                    {
                        mLoadingTipStatus = LoadingTipStatus.None;
                        UpdateLoadingTip(item);
                    }
                }
            }
            else
            {
                Debug.Log("Draging ==>> NULL");
            }

            /*
            //下拉刷新
            LoopListViewItem2 _item = mLoopListView.GetShownItemByItemIndex(0);
            if (_item != null)
            {
                ScrollRect sr = mLoopListView.ScrollRect;
                Vector3 pos = sr.content.localPosition;
                if (pos.y < -mLoadingTipItemHeight)
                {
                    if (mLoadingTipStatus != LoadingTipStatus.None)
                    {
                        return;
                    }
                    //Debug.Log("<<<<"); //刷新
                    mLoadingTipStatus = LoadingTipStatus.WaitRelease;
                    UpdateLoadingTip(_item);
                    _item.CachedRectTransform.localPosition = new Vector3(0, mLoadingTipItemHeight, 0);
                }
                else
                {
                    if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
                    {
                        return;
                    }
                    //Debug.Log(">>>>"); //放弃
                    mLoadingTipStatus = LoadingTipStatus.None;
                    UpdateLoadingTip(_item);
                    _item.CachedRectTransform.localPosition = new Vector3(0, 0, 0);
                }
            }
            */
        }

        void OnEndDrag()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }
            if (mLoadingTipStatus != LoadingTipStatus.None && mLoadingTipStatus != LoadingTipStatus.WaitRelease)
            {
                return;
            }

            ///*
            // 上拉加载
            LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(DataSourceMgr.Get.TotalItemCount);
            if (item != null)
            {
                mLoopListView.OnItemSizeChanged(item.ItemIndex);
                if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
                {
                    Debug.Log("上拉 ==>> RETURN");
                    //return;
                }
                else
                {
                    Debug.Log("上拉加载");
                    mLoadingTipStatus = LoadingTipStatus.WaitLoad;
                    UpdateLoadingTip(item);
                    DataSourceMgr.Get.RequestLoadMoreDataList(mLoadMoreCount, OnDataSourceLoadMoreFinished);
                }
            }
            else
            {
                Debug.Log("End ==>> NULL");
            }
            //*/

            // 下拉刷新
            LoopListViewItem2 _item = mLoopListView.GetShownItemByItemIndex(0);
            if (_item != null)
            {
                mLoopListView.OnItemSizeChanged(_item.ItemIndex);
                if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
                {
                    Debug.Log("下拉 ==>> RETURN");
                    //return;
                }
                else
                {
                    Debug.Log("下拉刷新");
                    mLoadingTipStatus = LoadingTipStatus.WaitLoad;
                    UpdateLoadingTip(_item);
                    DataSourceMgr.Get.RequestRefreshDataList(OnDataSourceRefreshFinished);
                }
            }
        }

        void OnDataSourceLoadMoreFinished()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }

            if (mLoadingTipStatus == LoadingTipStatus.WaitLoad)
            {
                mLoadingTipStatus = LoadingTipStatus.None;
                Debug.Log(DataSourceMgr.Get.TotalItemCount + 1);
                mLoopListView.SetListItemCount(DataSourceMgr.Get.TotalItemCount + 1, false);
                mLoopListView.RefreshAllShownItem();
            }
        }

        void OnDataSourceRefreshFinished()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }

            if (mLoadingTipStatus == LoadingTipStatus.WaitLoad)
            {
                mLoadingTipStatus = LoadingTipStatus.Loaded;
                mDataLoadedTipShowLeftTime = 0.7f;
                LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(0);
                if (item == null)
                {
                    return;
                }
                UpdateLoadingTip(item);
                mLoopListView.RefreshAllShownItem();
            }
        }

        void Update()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }
            if (mLoadingTipStatus == LoadingTipStatus.Loaded)
            {
                mDataLoadedTipShowLeftTime -= Time.deltaTime;
                if (mDataLoadedTipShowLeftTime <= 0)
                {
                    mLoadingTipStatus = LoadingTipStatus.None;
                    LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(0);
                    if (item == null)
                    {
                        return;
                    }
                    UpdateLoadingTip(item);
                    item.CachedRectTransform.localPosition = new Vector3(0, -mLoadingTipItemHeight, 0);
                    mLoopListView.OnItemSizeChanged(0);
                }
            }
        }

        /*
        void OnJumpBtnClicked()
        {
            int itemIndex = 0;
            if (int.TryParse(mScrollToInput.text, out itemIndex) == false)
            {
                return;
            }
            if (itemIndex < 0)
            {
                return;
            }
            mLoopListView.MovePanelToItemIndex(itemIndex, 0);
        }
        */
    }
}
