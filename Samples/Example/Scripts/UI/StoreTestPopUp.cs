﻿using System;
using System.Collections.Generic;
using System.Linq;
using RGN;
using RGN.Model;
using RGN.Model.Response;
using RGN.Modules;
using RGN.Modules.Currency;
using RGN.Modules.Inventory;
using RGN.Modules.VirtualItems;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyGamesNetwork.Sample.UI
{
    public class StoreTestPopUp : AbstractPopup
    {
        [SerializeField] private GameObject itemTemplate;
        [SerializeField] private Transform itemContent;
        [SerializeField] private Button earnAdsRewardButton;

        private List<StoreTestPopUpItem> items = new List<StoreTestPopUpItem>();

        public override void Show(bool isInstant, Action onComplete)
        {
            base.Show(isInstant, onComplete);

            earnAdsRewardButton.onClick.RemoveAllListeners();
            earnAdsRewardButton.onClick.AddListener(HandleEarnAdsRewardButton);

            Init();
        }

        private async void Init()
        {
            itemTemplate.SetActive(false);

            foreach (StoreTestPopUpItem item in items)
            {
                Destroy(item.gameObject);
            }
            items.Clear();

            UIRoot.singleton.ShowPopup<SpinnerPopup>();

            VirtualItemModule virtualItemModule = RGNCoreBuilder.I.GetModule<VirtualItemModule>();

            var virtualItems = await virtualItemModule.GetVirtualItems();
            Debug.Log(virtualItems.allItems.Count);

            foreach (RGNVirtualItem virtualItem in virtualItems.allItems)
            {
                GameObject itemGO = Instantiate(itemTemplate, itemContent);
                itemGO.SetActive(true);

                StoreTestPopUpItem item = itemGO.GetComponent<StoreTestPopUpItem>();
                item.Init(virtualItem);
                item.OnBuyButtonClick += OnBuyButtonClick;

                items.Add(item);
            }

            UIRoot.singleton.HidePopup<SpinnerPopup>();
        }

        private async void OnBuyButtonClick(string itemId)
        {
            UIRoot.singleton.ShowPopup<SpinnerPopup>();

            VirtualItemModule virtualItemModule = RGNCoreBuilder.I.GetModule<VirtualItemModule>();
            List<string> itemList = new List<string>();
            itemList.Add(itemId);
            BuyVirtualItemResponseData buyItemData = await virtualItemModule.BuyVirtualItems(itemList);
            
            PopupMessage popupMessage = new PopupMessage()
            {
                Message = $"success: {buyItemData.isSuccess} {buyItemData.message}"
            };
            GenericPopup genericPopup = UIRoot.singleton.GetPopup<GenericPopup>();
            genericPopup.ShowMessage(popupMessage);
            UIRoot.singleton.ShowPopup<GenericPopup>();
            
            UIRoot.singleton.HidePopup<SpinnerPopup>();
            
            Init();
        }

        private async void HandleEarnAdsRewardButton()
        {
            UIRoot.singleton.ShowPopup<SpinnerPopup>();

            CurrencyModule inAppPurchaseModule = RGNCoreBuilder.I.GetModule<CurrencyModule>();
            EarnAdsRewardRequestData earnAdsRewardResult = await inAppPurchaseModule.EarnAdsReward(new List<RGNCurrency>()
            {
                new RGNCurrency()
                {
                    name = "rgntestCoin",
                    quantity = 25
                }
            });

            PopupMessage popupMessage = new PopupMessage()
            {
                Message = $"success: {earnAdsRewardResult.reward.Count}"
            };
            GenericPopup genericPopup = UIRoot.singleton.GetPopup<GenericPopup>();
            genericPopup.ShowMessage(popupMessage);
            UIRoot.singleton.ShowPopup<GenericPopup>();

            UIRoot.singleton.HidePopup<SpinnerPopup>();
        }

        public void OnCloseClick()
        {
            Hide(true, null);
        }
    }
}