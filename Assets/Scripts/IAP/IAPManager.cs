using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace IAP
{
    public class IAPManager : PersistentMonoSingleton<IAPManager>, IDetailedStoreListener
    {
        private static IStoreController _storeController;
        private static IExtensionProvider _storeExtensionProvider;

        private ConfigurationBuilder _builder;
        private UnityAction<IAPOperationStatus, string, StoreProduct> _onCompleteMethod;
        private UnityAction<IAPOperationStatus, string, List<StoreProduct>> _onInitComplete;
        private UnityAction _restoreDone;
        private List<StoreProduct> _shopProducts;
        internal bool Debug;

        /// <summary>
        ///     초기화가 완료된 후 호출되는 IStoreListener 이벤트 핸들러
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="extensions"></param>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            if (Debug) UnityEngine.Debug.Log("OnInitialized");

            _storeController = controller;
            _storeExtensionProvider = extensions;

            StartCoroutine(InitializeProducts());
        }

        /// <summary>
        ///     구매 실패 시 호출되는 IStoreListener 이벤트 핸들러
        /// </summary>
        /// <param name="product"></param>
        /// <param name="failureDescription"></param>
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            if (Debug)
                UnityEngine.Debug.Log(this + "제품 구매에 실패 : " + product.metadata.localizedTitle +
                                      " 이유: " +
                                      failureDescription.reason);

            _onCompleteMethod?.Invoke(IAPOperationStatus.Fail,
                product.metadata.localizedTitle + " 실패했습니다. 이유: " + failureDescription.reason,
                null);
        }


        /// <summary>
        ///     초기화 실패 시 호출되는 IStoreListener 이벤트 핸들러
        /// </summary>
        /// <param name="error"></param>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            if (Debug) UnityEngine.Debug.Log("OnInitializeFailed");

            _onInitComplete(IAPOperationStatus.Fail, error.ToString(), null);
        }

        /// <summary>
        ///     초기화 실패 시 호출되는 IStoreListener 이벤트 핸들러
        /// </summary>
        /// <param name="error"></param>
        /// <param name="message"></param>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            if (Debug) UnityEngine.Debug.Log(this + "OnInitializedFailed, 에러: " + error + " 메세지: " + message);

            _onInitComplete(IAPOperationStatus.Fail, error.ToString(), null);
        }

        /// <summary>
        ///     구매 실패 시 호출되는 IStoreListener 이벤트 핸들러
        /// </summary>
        /// <param name="product"></param>
        /// <param name="reason"></param>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            if (Debug)
                UnityEngine.Debug.Log(this + "제품 구매에 실패 : " + product.metadata.localizedTitle +
                                      " 이유 : " +
                                      reason);

            _onCompleteMethod?.Invoke(IAPOperationStatus.Fail,
                product.metadata.localizedTitle + " 실패, 이유 : " + reason,
                null);
        }


        /// <summary>
        ///     구매가 완료되면 호출되는 IStoreListener 이벤트 핸들러
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            if (Debug) UnityEngine.Debug.Log(this + "상품 구매 프로세스 " + e.purchasedProduct.definition.id);

            for (var i = 0; i < _shopProducts.Count; i++)
                if (string.Equals(e.purchasedProduct.definition.id, _shopProducts[i].GetStoreID(),
                        StringComparison.Ordinal))
                {
                    IAPSecurityException exception;
                    var validPurchase =
                        ReceiptIsValid(_shopProducts[i].productName, e.purchasedProduct.receipt, out exception);
                    if (validPurchase)
                    {
                        _shopProducts[i].receipt = e.purchasedProduct.receipt;
                        if (_shopProducts[i].productType == ProductType.Subscription ||
                            _shopProducts[i].productType == ProductType.NonConsumable) _shopProducts[i].active = true;
                        if (_onCompleteMethod != null)
                            _onCompleteMethod(IAPOperationStatus.Success, "구매 성공", _shopProducts[i]);
                    }
                    else
                    {
                        _onCompleteMethod?.Invoke(IAPOperationStatus.Fail,
                            "유효하지 않은 영수증 " + exception.Message + exception.Data, null);
                    }

                    break;
                }

            return PurchaseProcessingResult.Complete;
        }


        /// <summary>
        ///     스토어가 초기화되었는지 확인
        /// </summary>
        /// <returns>true if shop was already initialized</returns>
        public bool IsStoreInitialized()
        {
            return _storeController != null && _storeExtensionProvider != null;
        }

        /// <summary>
        ///     제품을 구매하려면 이 메서드를 호출합니다.
        /// </summary>
        /// <param name="productName">설정 창에서 생성된 열거형 멤버</param>
        /// <param name="onCompleteMethod">초기화를 위해 구매한 제품 세부 정보를 반환하는 콜백 메서드</param>
        public void BuyProduct(ShopProductNames productName,
            UnityAction<IAPOperationStatus, string, StoreProduct> onCompleteMethod)
        {
            if (Debug) UnityEngine.Debug.Log(this + "구매 프로세스 시작 : " + productName);

            _onCompleteMethod = onCompleteMethod;

            for (var i = 0; i < _shopProducts.Count; i++)
                if (_shopProducts[i].productName == productName.ToString())
                    BuyProductID(_shopProducts[i].GetStoreID());
        }


        /// <summary>
        ///     이전에 구매한 제품 복원 (iOS에서만 필요)
        /// </summary>
        /// <param name="onCompleteMethod">복원 프로세스가 완료되면 호출됩니다.</param>
        public void RestorePurchases(UnityAction<IAPOperationStatus, string, StoreProduct> onCompleteMethod)
        {
            if (!IsStoreInitialized())
            {
                if (Debug) UnityEngine.Debug.Log(this + "복원 구매 실패. 초기화되지 않았습니다.");

                _restoreDone?.Invoke();
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                if (Debug) UnityEngine.Debug.Log(this + "복원구매 시작 ...");

                _onCompleteMethod = onCompleteMethod;
                var apple = _storeExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result, error) =>
                {
                    if (Debug)
                        UnityEngine.Debug.Log(this + "복원 구매: " + result +
                                              ". 추가 메시지가 없으면 복원할 수 있는 구매가 없는 것입니다.");

                    if (_restoreDone != null)
                    {
                        _restoreDone();
                    }
                    else
                    {
                        if (Debug)
                            // 복원 실패.
                            UnityEngine.Debug.Log(this + "복원 실패 " + error);
                    }
                });
            }
            else
            {
                if (Debug)
                    UnityEngine.Debug.Log(this + "복원구매 실패. 이 플랫폼에서는 지원되지 않습니다. 현재 = " +
                                          Application.platform);

                _restoreDone?.Invoke();
                onCompleteMethod?.Invoke(IAPOperationStatus.Fail,
                    "이 플랫폼에서는 지원되지 않습니다. 현재 = " + Application.platform, null);
            }
        }

        /// <summary>
        ///     이전에 구매한 제품 복원 (iOS에서만 필요)
        /// </summary>
        /// <param name="onCompleteMethod">복원된 제품 리스트를 반환하는 콜백 메서드</param>
        /// <param name="restoreDone">복원 프로세스가 완료되면 호출됩니다.</param>
        public void RestorePurchases(UnityAction<IAPOperationStatus, string, StoreProduct> onCompleteMethod,
            UnityAction restoreDone)
        {
            _restoreDone = restoreDone;
            RestorePurchases(onCompleteMethod);
        }


        /// <summary>
        ///     주어진 제품의 유형을 반환합니다.
        /// </summary>
        /// <param name="product"></param>
        /// <returns>소모품/비소모품/구독 서비스</returns>
        public ProductType GetProductType(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).productType;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return 0;
        }

        public string GetReceipt(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).receipt;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return "";
        }


        /// <summary>
        ///     제품 보상 받기
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <returns>받은 게임 내 화폐의 양</returns>
        public int GetValue(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).value;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return 0;
        }


        /// <summary>
        ///     제품의 가격 및 통화 코드를 문자열로 가져옵니다.
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <returns>제품의 현지화된 가격 및 통화</returns>
        public string GetLocalizedPriceString(ShopProductNames product)
        {
            if (IsStoreInitialized())
            {
                if (_shopProducts != null)
                    return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString()))
                        .localizedPriceString;
                UnityEngine.Debug.LogError(
                    "사용 가능한 제품 없음, Window-> IAP로 이동하여 제품을 정의합니다.");
            }
            else
            {
                UnityEngine.Debug.LogError(
                    "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            }

            return "-";
        }


        /// <summary>
        ///     현지 통화로 표시된 제품 십진수 가격 가져오기
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <returns></returns>
        public int GetPrice(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).price;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return 0;
        }


        /// <summary>
        ///     제품 통화를 ISO 4217 형식(예: GBP 또는 USD)으로 가져옵니다.
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <returns></returns>
        public string GetIsoCurrencyCode(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).isoCurrencyCode;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return "-";
        }


        /// <summary>
        ///     스토어에서 설명 받기
        /// </summary>
        /// <param name="product">상점 상품 이름</param>
        /// <returns></returns>
        public string GetLocalizedDescription(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString()))
                    .localizedDescription;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return "-";
        }

        /// <summary>
        ///     스토어에서 타이틀 가져오기
        /// </summary>
        /// <param name="product">상점 상품 이름</param>
        /// <returns></returns>
        public string GetLocalizedTitle(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).localizedTitle;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return "-";
        }


        /// <summary>
        ///     제품의 상태를 가져옵니다.
        /// </summary>
        /// <param name="product"></param>
        /// <returns>제품이 이미 구매된 경우 True입니다.</returns>
        public bool IsActive(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString())).active;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return false;
        }


        /// <summary>
        ///     구독에 대한 추가 정보 얻기
        /// </summary>
        /// <param name="product">구독 제품</param>
        /// <returns>구독에 사용할 수 있는 모든 정보</returns>
        public SubscriptionInfo GetSubscriptionInfo(ShopProductNames product)
        {
            if (IsStoreInitialized())
                return _shopProducts.First(cond => string.Equals(cond.productName, product.ToString()))
                    .subscriptionInfo;

            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return null;
        }


        /// <summary>
        ///     주어진 이름을 열거형 멤버로 변환합니다.
        /// </summary>
        /// <param name="name">변환할 문자열</param>
        /// <returns>열거형 멤버</returns>
        public ShopProductNames ConvertNameToShopProduct(string name)
        {
            return (ShopProductNames)Enum.Parse(typeof(ShopProductNames), name);
        }

        public Product GetProduct(StoreProduct product)
        {
            if (IsStoreInitialized())
                return _storeController.products.WithID(product.GetStoreID());
            UnityEngine.Debug.LogError(
                "초기화되지 않음 -> 다른 모든 작업보다 먼저 IAPManager.Instance.InitializeIAPManager()를 호출합니다.");
            return null;
        }


        /// <summary>
        ///     제품 구매 프로세스를 초기화합니다.
        /// </summary>
        /// <param name="productId"></param>
        private void BuyProductID(string productId)
        {
            if (Debug) UnityEngine.Debug.Log(this + "ID로 제품 구매: " + productId);

            if (IsStoreInitialized())
            {
                var product = _storeController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    _storeController.InitiatePurchase(product);
                }
                else
                {
                    if (Debug)
                        UnityEngine.Debug.Log(this +
                                              "BuyProductID: 실패. 제품을 찾을 수 없거나 구매할 수 없습니다.");

                    if (_onCompleteMethod != null)
                        _onCompleteMethod(IAPOperationStatus.Fail,
                            "제품을 찾을 수 없거나 구매할 수 없습니다.", null);
                }
            }
            else
            {
                if (Debug) UnityEngine.Debug.Log(this + "BuyProductID: 실패. 스토어가 초기화되지 않았습니다.");

                if (_onCompleteMethod != null)
                    _onCompleteMethod(IAPOperationStatus.Fail, "스토어가 초기화되지 않았습니다.", null);
            }
        }


        /// <summary>
        ///     Unity IAP 초기화
        /// </summary>
        private void InitializePurchasing()
        {
            if (IsStoreInitialized())
            {
                _onInitComplete(IAPOperationStatus.Success, "이미 초기화됨", null);
                return;
            }

            _builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

#if UNITY_ANDROID
            // Google Play 특정 설정
            _builder.Configure<IGooglePlayConfiguration>().SetMaxConnectionAttempts(int.MaxValue - 1);
#endif

            for (var i = 0; i < _shopProducts.Count; i++)
                _builder.AddProduct(_shopProducts[i].GetStoreID(), _shopProducts[i].GetProductType());

            if (Debug) UnityEngine.Debug.Log("UNITY IAP 초기화");

            UnityPurchasing.Initialize(this, _builder);
        }

        private IEnumerator InitializeProducts()
        {
            yield return new WaitForSeconds(1);
            for (var i = 0; i < _shopProducts.Count; i++)
            {
                var product = _storeController.products.WithID(_shopProducts[i].GetStoreID());

                if (Debug)
                    UnityEngine.Debug.Log(this + product.metadata.localizedTitle + " is available " +
                                          product.availableToPurchase);

                if (_shopProducts[i].productType == ProductType.Subscription)
                    if (product is { hasReceipt: true })
                        if (ReceiptIsValid(_shopProducts[i].productName, product.receipt, out var exception))
                        {
                            _shopProducts[i].active = true;
                            string introJson = null;
                            var p = new SubscriptionManager(product, introJson);
                            _shopProducts[i].subscriptionInfo = p.getSubscriptionInfo();
                            _shopProducts[i].receipt = product.receipt;
                        }

                if (_shopProducts[i].productType == ProductType.NonConsumable)
                    if (product is { hasReceipt: true })
                        if (ReceiptIsValid(_shopProducts[i].productName, product.receipt, out var exception))
                            _shopProducts[i].active = true;

                if (product is not { availableToPurchase: true }) continue;

                _shopProducts[i].localizedPriceString = product.metadata.localizedPriceString;
                _shopProducts[i].price = decimal.ToInt32(product.metadata.localizedPrice);
                _shopProducts[i].isoCurrencyCode = product.metadata.isoCurrencyCode;
                _shopProducts[i].localizedDescription = product.metadata.localizedDescription;
                _shopProducts[i].localizedTitle = product.metadata.localizedTitle;
            }

            _onInitComplete(IAPOperationStatus.Success, "Success", _shopProducts);
        }

        /// <summary>
        ///     영수증 유효성 검사
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="receipt"></param>
        /// <param name="exception"></param>
        /// <returns>영수증이 유효하면 true</returns>
        private bool ReceiptIsValid(string productName, string receipt, out IAPSecurityException exception)
        {
            exception = null;
            var validPurchase = true;
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
        try
        {
            var validator =
                new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

            validator.Validate(receipt);
            if (debug)
            {
                Debug.Log(this + " Receipt is valid for " + productName);
                ScreenWriter.Write(this + " Receipt is valid for " + productName);
            }
        }
        catch (IAPSecurityException ex)
        {
            exception = ex;
            if (debug)
            {
                Debug.Log(this + " Receipt is NOT valid for " + productName);
                ScreenWriter.Write(this + " Receipt is NOT valid for " + productName);
            }

            validPurchase = false;
        }
#endif
            return validPurchase;
        }


        /// <summary>
        ///     스토어 제품 초기화, 게임 시작 시 이 메서드를 한 번 호출합니다.
        /// </summary>
        /// <param name="initComplete">콜백 메서드는 모든 스토어 제품 목록을 반환하며, 초기화에는 이 메서드를 사용합니다.</param>
        public void InitializeIAPManager(UnityAction<IAPOperationStatus, string, List<StoreProduct>> initComplete)
        {
            var settings = Resources.Load<IAPSettings>("IAPData");
            if (settings == null)
            {
                UnityEngine.Debug.LogError(
                    "사용 가능한 제품 없음, Window-> IAP로 이동하여 제품을 정의합니다.");
                return;
            }

            _shopProducts = settings.shopProducts;
            Debug = settings.debug;
            if (Debug) UnityEngine.Debug.Log(this + "초기화 시작됨");

            if (_storeController != null) return;

            _onInitComplete = initComplete;
            InitializePurchasing();
        }
    }
}