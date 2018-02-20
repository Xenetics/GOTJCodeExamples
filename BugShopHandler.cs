using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using VoxelBusters.NativePlugins;

/// <summary>
/// Date Created: 01/26/16 ->
/// Handles events on the Bug Shop Menu Screen
/// </summary>
public class BugShopHandler : MonoBehaviour
{
    private static BugShopHandler instance = null;
    public static BugShopHandler Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        ShopItems = new List<GameObject>();
    }

    /// <summary> Title of the shop type </summary>
    [SerializeField]
    private Text m_TitleText;
    /// <summary> flavour text of the page </summary>
    [SerializeField]
    private Text m_FlavourText;
    /// <summary> The game ob ject that will be parent to the Item objects </summary>
    [SerializeField]
    private RectTransform m_ItemScrollObject;
    /// <summary> This is the prefab that will be used to display the Items available </summary>
    [SerializeField]
    private GameObject m_ItemPrefab;
    /// <summary> Container for all the Items we load into the scrolling panel </summary>
    [HideInInspector]
    public List<GameObject> ShopItems;
    /// <summary> Offset for the spacing between the listings in list mode </summary>
    private const float c_RectOffset = 20;
    /// <summary> Current Selected Item </summary>
    [HideInInspector]
    public BugShopItem SelectedItem;
    /// <summary> In app purchase prices </summary>
    public string[] Prices;
    /// <summary> In app purchase symbols </summary>
    public string[] Symbols;
    /// <summary> Is this canvas loaded </summary>
    private bool m_FirstLoad = false;
    /// <summary> curtain to prevent multiple clicks on ios </summary>
    [SerializeField]
    private GameObject m_Curtain;

    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        NotificationManager.Instance.SetLoadingText(true);
        NPBinding.Billing.RequestForBillingProducts(NPSettings.Billing.Products);
#endif
    }

    /// <summary> Build the Shop Inventory with rectangle objects and adds them to the scroller </summary>
    private void ListItems()
    {
        m_ItemScrollObject.sizeDelta        = new Vector2(m_ItemScrollObject.sizeDelta.x
                                                        , c_RectOffset
                                                        + ((m_ItemPrefab.GetComponent<RectTransform>().sizeDelta.y + c_RectOffset) * (PowerUpManager.Instance.BugShopListings.Count + ((GameManager.Instance.GetShouldPlayAds()) ? (1) : (0)))));
        m_ItemScrollObject.localPosition    = new Vector2(m_ItemScrollObject.localPosition.x
                                                        , m_ItemScrollObject.parent.position.y
                                                        - (m_ItemScrollObject.parent.GetComponent<RectTransform>().sizeDelta.y * 0.5f)
                                                        - (m_ItemScrollObject.sizeDelta.y * 0.5f) + c_RectOffset);

        for (int i = 0; i < PowerUpManager.Instance.BugShopListings.Count; i++)
        {
            if ((PowerUpManager.Instance.BugShopListings[i].Title.Contains("Ad-Free") && GameManager.Instance.GetShouldPlayAds())
                || (PowerUpManager.Instance.BugShopListings[i].Title.Contains("Key") && !GameManager.Instance.DoesOwnKey())
                || (PowerUpManager.Instance.BugShopListings[i].Title.Contains("Fun") && GameManager.Instance.GetShouldPlayAds() && !GameManager.Instance.DoesOwnKey())
                || PowerUpManager.Instance.BugShopListings[i].Title.Contains("Friend")
                || PowerUpManager.Instance.BugShopListings[i].Title.Contains("Restore"))
            {
                GameObject temp = Instantiate(m_ItemPrefab);
                temp.name = PowerUpManager.Instance.BugShopListings[i].Title + "_" + m_ItemPrefab.name;
                temp.GetComponent<RectTransform>().sizeDelta = new Vector2(m_ItemScrollObject.GetComponent<RectTransform>().sizeDelta.x
                                                                            - (c_RectOffset * 2f)
                                                                            , temp.GetComponent<RectTransform>().sizeDelta.y);
                temp.transform.SetParent(m_ItemScrollObject.transform.parent);
                temp.transform.localScale = new Vector3(1, 1, 1);


                if (ShopItems.Count == 0)
                {
                    temp.GetComponent<RectTransform>().localPosition = new Vector2(m_ItemScrollObject.localPosition.x
                                                                                    + c_RectOffset
                                                                                    , m_ItemScrollObject.anchoredPosition.y
                                                                                    + (m_ItemScrollObject.sizeDelta.y * 0.5f)
                                                                                    - (temp.GetComponent<RectTransform>().sizeDelta.y * 0.5f)
                                                                                    - c_RectOffset * 4);
                    temp.transform.SetParent(m_ItemScrollObject.transform);
                }
                else
                {
                    temp.transform.SetParent(m_ItemScrollObject.transform);
                    temp.GetComponent<RectTransform>().localPosition = new Vector2(ShopItems[ShopItems.Count - 1].GetComponent<RectTransform>().localPosition.x
                                                                                    , ShopItems[ShopItems.Count - 1].GetComponent<RectTransform>().localPosition.y
                                                                                    - temp.GetComponent<RectTransform>().sizeDelta.y - c_RectOffset);
                }

                temp.GetComponent<BugShopItemObject>().ItemIndex = i;

                ShopItems.Add(temp);
            }
            ResetScroll();
            NotificationManager.Instance.SetLoadingText(false);
        }
    }

    /// <summary> Sets scroll rect to the starting position </summary>
    public void ResetScroll()
    {
        m_ItemScrollObject.gameObject.transform.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1.0f;
    }

#if UNITY_ANDROID || UNITY_IOS
    /// <summary> Sends purchase request </summary>
    public void BuyItem(int ItemIndex)
    {
        SelectedItem = PowerUpManager.Instance.BugShopListings[ItemIndex];
        NPBinding.Billing.BuyProduct(NPSettings.Billing.Products[ItemIndex].ProductIdentifier);
    }

    private void OnEnable()
    {
        // Registering for event
        Billing.DidReceiveTransactionInfoEvent += DidReceiveTransactionInfo;
		Billing.DidFinishRequestForBillingProductsEvent	+= OnDidFinishProductsRequest;
        Billing.DidFinishRestoringPurchasesEvent += OnDidFinishRestoringPurchases;
    }
    private void OnDisable()
    {
        // Unregistering event
        Billing.DidReceiveTransactionInfoEvent -= DidReceiveTransactionInfo;
		Billing.DidFinishRequestForBillingProductsEvent	-= OnDidFinishProductsRequest;
        Billing.DidFinishRestoringPurchasesEvent -= OnDidFinishRestoringPurchases;
    }

    private void DidReceiveTransactionInfo(BillingTransaction[] _transactionList, string _error)
    {
        if (_transactionList != null)
        {
            for (int i = 0; i < _transactionList.Length; i++)
            {
                if (_transactionList[i].VerificationState == eBillingTransactionVerificationState.SUCCESS)
                {
                    if (_transactionList[i].TransactionState == eBillingTransactionState.PURCHASED)
                    {
                        if (SelectedItem.BugAmount != 0)
                        {
                            GameManager.Instance.EarnBugs(SelectedItem.BugAmount);
                            GameManager.Instance.googleAnalytics.LogTransaction(_transactionList[i].TransactionIdentifier, "Buy Bugs", SelectedItem.Cost, 0, 0);
                            GameManager.Instance.googleAnalytics.LogItem(_transactionList[i].TransactionIdentifier, SelectedItem.Title, "", "", SelectedItem.Cost, 1);
                            int n = (int)GameManager.Instance.TotalBugs;
                            string s = "";
                            if (n == 0)
                            {
                                s = "0";
                            }
                            else if (n <= 100)
                            {
                                s = "1-100";
                            }
                            else if (n <= 500)
                            {
                                s = "101-500";
                            }
                            else if (n <= 2000)
                            {
                                s = "501-2000";
                            }
                            else if (n <= 5000)
                            {
                                s = "2001-5000";
                            }
                            else if (n <= 10000)
                            {
                                s = "5001-10000";
                            }
                            else
                            {
                                s = "10000+";
                            }
                            if (!AchievementMonitor.Instance.AParams.firstPurchase)
                            {
                                GameManager.Instance.googleAnalytics.LogEvent(new EventHitBuilder().SetEventCategory("Shop").SetEventAction("Bugs Collected").SetEventLabel(_transactionList[i].ProductIdentifier).SetEventValue((long)GameManager.Instance.TotalBugs).SetCustomMetric(1, AchievementMonitor.Instance.AParams.totalMinutesPlayed.ToString()).SetCustomMetric(2, GameManager.Instance.TotalBugs.ToString()).SetCustomDimension(6, GameManager.Instance.TotalBugs.ToString()).SetCustomDimension(7, s));
                                AchievementMonitor.Instance.AParams.firstPurchase = true;
                                GameManager.saveData.AParams = AchievementMonitor.Instance.AParams;
                            }
                            else
                            {
                                GameManager.Instance.googleAnalytics.LogEvent(new EventHitBuilder().SetEventCategory("Shop").SetEventAction("Bugs Collected").SetEventLabel(_transactionList[i].ProductIdentifier).SetEventValue((long)GameManager.Instance.TotalBugs).SetCustomDimension(6, GameManager.Instance.TotalBugs.ToString()).SetCustomDimension(7, s));
                            }
                        }
                        else if (SelectedItem.Title.Contains("Ad-Free"))
                        {
                            GameManager.Instance.TurnAdsOff();
                            GameManager.Instance.googleAnalytics.LogTransaction(_transactionList[i].TransactionIdentifier, "Cash Shop", SelectedItem.Cost, 0, 0);
                            GameManager.Instance.googleAnalytics.LogItem(_transactionList[i].TransactionIdentifier, SelectedItem.Title, "", "", SelectedItem.Cost, 1);
                            GameManager.Instance.googleAnalytics.LogEvent("Shop", "IAP","Ad-Free", 1);
                            m_FirstLoad = true;
                            ResetFirstLoad();
                        }
                        else if (SelectedItem.Title.Contains("Friend"))
                        {
                            PowerUpManager.Instance.AddTempPower(99);
                            GameManager.Instance.googleAnalytics.LogTransaction(_transactionList[i].TransactionIdentifier, "Cash Shop", SelectedItem.Cost, 0, 0);
                            GameManager.Instance.googleAnalytics.LogItem(_transactionList[i].TransactionIdentifier, SelectedItem.Title, "", "", SelectedItem.Cost, 1);
                            GameManager.Instance.googleAnalytics.LogEvent("Shop", "IAP", "99 Friends", 1);
                            m_FirstLoad = true;
                            ResetFirstLoad();
                        }
                        else if (SelectedItem.Title.Contains("Key"))
                        {
                            GameManager.Instance.BuyKey();
                            GameManager.Instance.googleAnalytics.LogTransaction(_transactionList[i].TransactionIdentifier, "Cash Shop", SelectedItem.Cost, 0, 0);
                            GameManager.Instance.googleAnalytics.LogItem(_transactionList[i].TransactionIdentifier, SelectedItem.Title, "", "", SelectedItem.Cost, 1);
                            GameManager.Instance.googleAnalytics.LogEvent("Shop", "IAP", "Master Key", 1);
                            m_FirstLoad = true;
                            ResetFirstLoad();
                        }
                        else if (SelectedItem.Title.Contains("Fun"))
                        {
                            PowerUpManager.Instance.AddTempPower(99);
                            GameManager.Instance.TurnAdsOff();
                            GameManager.Instance.BuyKey();
                            GameManager.Instance.googleAnalytics.LogTransaction(_transactionList[i].TransactionIdentifier, "Cash Shop", SelectedItem.Cost, 0, 0);
                            GameManager.Instance.googleAnalytics.LogItem(_transactionList[i].TransactionIdentifier, SelectedItem.Title, "", "", SelectedItem.Cost, 1);
                            GameManager.Instance.googleAnalytics.LogEvent("Shop", "IAP", "Fun Pack", 1);
                            m_FirstLoad = true;
                            ResetFirstLoad();
                        }

                        GameSaveSystem.Instance.SaveGame();
                    }
                    else if (_transactionList[i].TransactionState == eBillingTransactionState.RESTORED)
                    {
                        // Your code to handle restored products
                    }
                }
            }
        }
        CurtainOn(false);
    }

    public void RestorePurchases()
    {
        NPBinding.Billing.RestorePurchases();
    }

    private void OnDidFinishRestoringPurchases(BillingTransaction[] t_transactions, string t_error)
    {
        if (t_transactions != null)
        {
            foreach (BillingTransaction item in t_transactions)
            {
                RestorePurchase(item.ProductIdentifier);
                GameSaveSystem.Instance.SaveGame();
            }
        }
    }

    private void RestorePurchase(string item)
    {
#if UNITY_ANDROID
        switch (item)
        {
            case "400bugs":
                GameManager.Instance.EarnBugs(5000);
                break;
            case "1200bugs":
                GameManager.Instance.EarnBugs(15000);
                break;
            case "3000bugs":
                GameManager.Instance.EarnBugs(45000);
                break;
            case "10000bugs":
                GameManager.Instance.EarnBugs(100000);
                break;
            case "5_remove_ads":
                GameManager.Instance.TurnAdsOff();
                m_FirstLoad = true;
                ResetFirstLoad();
                NotificationManager.Instance.SetLoadingText(false);
                GameSaveSystem.Instance.SaveGame();
                break;
            case "z_99_friends":
                PowerUpManager.Instance.AddTempPower(99);
                break;
            case "z_master_key":
                GameManager.Instance.BuyKey();
                break;
            case "z_fun_pack":
                PowerUpManager.Instance.AddTempPower(99);
                GameManager.Instance.TurnAdsOff();
                GameManager.Instance.BuyKey();
                break;
        }
#elif UNITY_IOS
        switch (item)
        {
            case "IAP1":
            case "1_5000_bugs_fr":
                GameManager.Instance.EarnBugs(5000);
                break;
            case "IAP2":
            case "2_15000_bugs_fr":
                GameManager.Instance.EarnBugs(15000);
                break;
            case "IAP3":
            case "3_45000_bugs_fr":
                GameManager.Instance.EarnBugs(45000);
                break;
            case "IAP4":
            case "4_100000_bugs_fr":
                GameManager.Instance.EarnBugs(100000);
                break;
            case "IAP5":
            case "IAP5fr":
                GameManager.Instance.TurnAdsOff();
                m_FirstLoad = true;
                ResetFirstLoad();
                NotificationManager.Instance.SetLoadingText(false);
                GameSaveSystem.Instance.SaveGame();
                break;
            case "IAP6":
            case "IAP6fr":
                PowerUpManager.Instance.AddTempPower(99);
                break;
            case "IAP7":
            case "IAP7fr":
                GameManager.Instance.BuyKey();
                break;
            case "IAP8":
            case "IAP8fr":
                PowerUpManager.Instance.AddTempPower(99);
                GameManager.Instance.TurnAdsOff();
                GameManager.Instance.BuyKey();
                break;
        }
#endif
    }

    /// <summary> Clears the list of shop items for a rebuild </summary>
    private void ClearList()
    {
        int count = ShopItems.Count;
        for (int i = 0; i < count; ++i)
        {
            Destroy(ShopItems[0]);
            ShopItems.RemoveAt(0);
        }
    }

    /// <summary> resets first load </summary>
    public void ResetFirstLoad()
    {
        if (m_FirstLoad)
        {
            ClearList();
            NotificationManager.Instance.SetLoadingText(true);
            m_FirstLoad = false;
            ListItems();
            ResetScroll();
        }
    }

    /// <summary> Handles the currency localized </summary>
    /// <param name="_regProductsList"> the product list </param>
    /// <param name="_error"> the error returned </param>
    private void OnDidFinishProductsRequest(BillingProduct[] _regProductsList, string _error)
    {
        Prices = new string[_regProductsList.Length];
        if (_error != null)
        {
            Debug.LogError("Error = " + _error);
        }
        else
        {
            int loopCount = 0;
            foreach (BillingProduct _eachProduct in _regProductsList)
            {
                Prices[loopCount] = _eachProduct.LocalizedPrice;
                loopCount++;
            }

            if (ShopItems.Count > 0)
            {
                for (int i = 0; i < ShopItems.Count; ++i)
                {
                    ShopItems[i].GetComponent<BugShopItemObject>().Init();
                }
            }
        }

        m_FirstLoad = true;
        ResetFirstLoad();
        NotificationManager.Instance.SetLoadingText(false);
    }
#endif

    /// <summary> turns the curtain on and off for purchases </summary>
    public void CurtainOn(bool maybe)
    {
        m_Curtain.SetActive(maybe);
    }
}
