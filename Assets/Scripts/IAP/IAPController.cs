using System;
using System.Collections.Generic;
using System.Linq;
using AsyncInitialize;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace IAP
{
    /// <summary>
    ///     구매 결과
    /// </summary>
    public struct BuyResult
    {
        /// <summary>
        ///     실패 이유
        /// </summary>
        public enum FailedReason
        {
            None = -1, // 성공
            Unknown = 0, // 알 수 없는 오류
            NoProduct = 1, // 제품이 없음
            AnotherPurchaseInProgress = 2 // 다른 구매가 진행중
        }

        /// <summary>
        ///     생성자
        /// </summary>
        /// <param name="isSuccess">성공 여부</param>
        /// <param name="reason">실패 이유</param>
        private BuyResult(bool isSuccess, FailedReason reason)
        {
            IsSuccess = isSuccess;
            Reason = reason;
        }

        /// <summary>
        ///     실패 이유
        /// </summary>
        public FailedReason Reason { get; private set; }

        /// <summary>
        ///     성공 여부
        /// </summary>
        public bool IsSuccess { get; private set; }

        #region Factory

        /// <summary>
        ///     성공 인스턴스
        /// </summary>
        public static BuyResult Success => new(true, FailedReason.None);

        /// <summary>
        ///     알 수 없는 오류 인스턴스
        /// </summary>
        public static BuyResult UnknownError => new(false, FailedReason.Unknown);

        /// <summary>
        ///     제품이 없음 인스턴스
        /// </summary>
        public static BuyResult NoProductError => new(false, FailedReason.NoProduct);

        /// <summary>
        ///     다른 구매가 진행중 인스턴스
        /// </summary>
        public static BuyResult AnotherPurchaseInProgressError => new(false, FailedReason.AnotherPurchaseInProgress);

        #endregion
    }

    /// <summary>
    ///     IAP 컨트롤러
    ///     인앱 결제를 관리합니다.
    /// </summary>
    public class IAPController : PersistentMonoSingleton<IAPController>, IAsyncInit
    {
        /// <summary>
        ///     복원 중인지 여부
        /// </summary>
        private bool _isRestoring;

        /// <summary>
        ///     초기화 여부
        /// </summary>
        private bool _mbInitialized;

        /// <summary>
        ///     구매 중인지 여부
        /// </summary>
        private bool _mbPurchaseInProgress;

        /// <summary>
        ///     제품 이름 -
        /// </summary>
        private Dictionary<ShopProductNames, ProductBoughtNotifier> _mProductsMap = new();

        /// <summary>
        ///     초기화 성공 여부
        /// </summary>
        public bool IsInitializeSuccess { get; private set; }

        /// <summary>
        ///     구매 완료 이벤트
        /// </summary>
        public UnityEvent<Product> OnBought { get; } = new();

        /// <summary>
        ///     복원 완료 이벤트
        /// </summary>
        public UnityEvent<Product> OnRestore { get; } = new();

        /// <summary>
        ///     초기화 성공 이벤트
        /// </summary>
        public UnityEvent OnInitializeSuccess { get; } = new();

        /// <summary>
        ///     초기화 여부를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CustomizableAsyncOperation GetAsyncOperation()
        {
            return CustomizableAsyncOperation.Create(() => _mbInitialized,
                () => _mbInitialized ? 1 : 0);
        }

        /// <summary>
        ///     초기화 프로세스를 시작합니다.
        /// </summary>
        public void StartInitialize()
        {
            if (_mbInitialized) return;

            IAPManager.Instance.InitializeIAPManager(InitializeResult);

            _mbInitialized = true;
        }

        public void Reset()
        {
        }

        /// <summary>
        ///     초기화를 실패 했을 때
        /// </summary>
        private void OnInitializeFailed()
        {
        }

        /// <summary>
        ///     구매 복원
        /// </summary>
        public async UniTask RestorePurchases()
        {
            _isRestoring = true;
            IAPManager.Instance.RestorePurchases(ProductBought, () => { _isRestoring = false; });

            await UniTask.WaitUntil(() => _isRestoring == false,
                cancellationToken: this.GetCancellationTokenOnDestroy());

            if (IAPManager.Instance.Debug) Debug.Log("Restore purchases completed");
        }

        /// <summary>
        ///     제품 구매
        /// </summary>
        /// <param name="productName">제품 이름</param>
        /// <returns>구매 결과</returns>
        /// <exception cref="ArgumentOutOfRangeException">구매 결과가 없을 때</exception>
        public async UniTask<BuyResult> BuyProduct(ShopProductNames productName)
        {
            if (_mbPurchaseInProgress) return BuyResult.AnotherPurchaseInProgressError;
            if (IsConsumable(productName) == false && HasProduct(productName)) return BuyResult.Success;

            _mbPurchaseInProgress = true;
            var iapStatus = IAPOperationStatus.Fail;
            var resultMessage = string.Empty;
            IAPManager.Instance.BuyProduct(productName, (status, message, storeProduct) =>
            {
                ProductBought(status, message, storeProduct);
                iapStatus = status;
                resultMessage = message;
                _ = storeProduct;

                _mbPurchaseInProgress = false;
            });

            await UniTask.WaitUntil(() => !_mbPurchaseInProgress,
                cancellationToken: this.GetCancellationTokenOnDestroy());

            switch (iapStatus)
            {
                case IAPOperationStatus.Success:
                    if (IAPManager.Instance.Debug) Debug.Log($"Buy product ({productName}) completed");
                    return BuyResult.Success;
                case IAPOperationStatus.Fail:
                    if (IAPManager.Instance.Debug) Debug.Log($"Buy product failed: {resultMessage}");
                    return BuyResult.UnknownError;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     제품 구매 이벤트 구독
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <param name="action">구매 완료 여부 액션</param>
        /// <returns></returns>
        public IDisposable SubscribeBought(ShopProductNames product, Action<bool> action)
        {
            if (_mProductsMap.TryGetValue(product, out var storeProducts) == false)
            {
                Debug.LogError($"Product {product} not found");
                return null;
            }

            return storeProducts.SubscribeBought(action);
        }

        /// <summary>
        ///     StoreProduct를 Product로 변환합니다.
        /// </summary>
        /// <param name="storeProduct">스토어 제품</param>
        /// <returns>Product</returns>
        public Product ToProduct(StoreProduct storeProduct)
        {
            return IAPManager.Instance.GetProduct(storeProduct);
        }

        /// <summary>
        ///     제품 이름으로 제품을 가지고 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <returns>제품을 가지고 있는지 여부</returns>
        public bool HasProduct(ShopProductNames product)
        {
            return _mProductsMap.TryGetValue(product, out var hasProduct) && hasProduct.Bought;
        }

        /// <summary>
        ///     소비성 제품인지 여부를 반환합니다.
        /// </summary>
        /// <param name="product">제품 이름</param>
        /// <returns>소비성 제품인지 여부</returns>
        public bool IsConsumable(ShopProductNames product)
        {
            return IAPManager.Instance.GetProductType(product) == ProductType.Consumable;
        }

        /// <summary>
        ///     제품을 구입한 뒤 처리합니다.
        /// </summary>
        /// <param name="status">구매 상태</param>
        /// <param name="message">메시지</param>
        /// <param name="product">제품</param>
        /// <exception cref="NotImplementedException">구매 처리가 구현되지 않았을 때</exception>
        private void ProductBought(IAPOperationStatus status, string message, StoreProduct product)
        {
            if (status == IAPOperationStatus.Fail)
            {
                //en error occurred in the buy process, log the message for more details
                if (IAPManager.Instance.Debug) Debug.Log("Buy product failed: " + message);
                return;
            }

            if (IAPManager.Instance.Debug)
                Debug.Log("Buy product completed: " + product.localizedTitle + " receive value: " + product.value);

            var eShopProduct = IAPManager.Instance.ConvertNameToShopProduct(product.productName);

            switch (product.productType)
            {
                case ProductType.NonConsumable:
                    if (product.active)
                        _mProductsMap[eShopProduct].Bought = true;
                    break;
                case ProductType.Consumable:
                case ProductType.Subscription:
                default:
                    throw new NotImplementedException();
            }

            if (_isRestoring == false)
                OnBought?.Invoke(ToProduct(product));
            else
                OnRestore?.Invoke(ToProduct(product));
        }

        /// <summary>
        ///     초기화 결과를 설정합니다.
        /// </summary>
        /// <param name="status">상태</param>
        /// <param name="message">메시지</param>
        /// <param name="shopProducts">제품 리스트</param>
        private void InitializeResult(IAPOperationStatus status, string message, List<StoreProduct> shopProducts)
        {
            if (status == IAPOperationStatus.Success)
            {
                _mProductsMap = GetShopProductMap(shopProducts);
                IsInitializeSuccess = true;
                OnInitializeSuccess?.Invoke();
            }
            else
            {
                _mProductsMap = GetFailedProductsMap();
                OnInitializeFailed();
            }

            if (IAPManager.Instance.Debug) Debug.Log("Init status: " + status + " message " + message);
        }

        /// <summary>
        ///     제품 이름을 매핑한 딕셔너리를 반환합니다.
        /// </summary>
        /// <param name="shopProducts">제품 리스트</param>
        /// <returns>제품 이름을 매핑한 딕셔너리</returns>
        private static Dictionary<ShopProductNames, ProductBoughtNotifier> GetShopProductMap(
            IEnumerable<StoreProduct> shopProducts)
        {
            return shopProducts
                .Select(x => (IAPManager.Instance.ConvertNameToShopProduct(x.productName), x.active))
                .ToDictionary(x => x.Item1,
                    x => new ProductBoughtNotifier(x.Item1, x.active));
        }

        /// <summary>
        ///     초기화의 실패했을 때의 제품 이름을 매핑한 딕셔너리를 반환합니다.
        /// </summary>
        /// <returns>제품 이름을 매핑한 딕셔너리</returns>
        private Dictionary<ShopProductNames, ProductBoughtNotifier> GetFailedProductsMap()
        {
            return new Dictionary<ShopProductNames, ProductBoughtNotifier>();
        }

        /// <summary>
        ///     현지화된 제품 가격 문자열을 반환합니다.
        /// </summary>
        /// <param name="productName">제품 이름</param>
        /// <returns>현지화된 제품 가격 문자열</returns>
        public string GetLocalizedPrice(ShopProductNames productName)
        {
            return IAPManager.Instance.GetLocalizedPriceString(productName);
        }

        /// <summary>
        ///     현지화된 제품 제목을 반환합니다.
        /// </summary>
        /// <param name="productName">제품 이름</param>
        /// <returns>현지화된 제품 제목</returns>
        public string GetLocalizedTitle(ShopProductNames productName)
        {
            return IAPManager.Instance.GetLocalizedTitle(productName);
        }

        /// <summary>
        ///     현지화된 제품 설명을 반환합니다.
        /// </summary>
        /// <param name="productName">제품 이름</param>
        /// <returns>현지화된 제품 설명</returns>
        public string GetLocalizedDescription(ShopProductNames productName)
        {
            return IAPManager.Instance.GetLocalizedDescription(productName);
        }

        /// <summary>
        ///     구매된 제품 알림 클래스
        /// </summary>
        private class ProductBoughtNotifier
        {
            private readonly ReactiveProperty<bool> _mReactiveBought;

            /// <summary>
            ///     생성자
            /// </summary>
            /// <param name="name">제품 이름</param>
            /// <param name="bought">구매 여부</param>
            public ProductBoughtNotifier(ShopProductNames name, bool bought)
            {
                Name = name;
                _mReactiveBought = new ReactiveProperty<bool>(bought);
            }

            /// <summary>
            ///     구매 여부
            /// </summary>
            public bool Bought
            {
                get => _mReactiveBought.Value;
                set => _mReactiveBought.Value = value;
            }

            /// <summary>
            ///     제품 이름
            /// </summary>
            public ShopProductNames Name { get; private set; }

            /// <summary>
            ///     구매 알림 구독
            /// </summary>
            /// <param name="action">구매 여부를 전달하는 액션</param>
            /// <returns>Dispose 객체</returns>
            public IDisposable SubscribeBought(Action<bool> action)
            {
                return _mReactiveBought.Subscribe(action);
            }

            /// <summary>
            ///     구매 여부를 설정하고 강제로 알립니다.
            /// </summary>
            /// <param name="bought">구매 여부</param>
            public void SetBoughtAndForceNotify(bool bought)
            {
                _mReactiveBought.SetValueAndForceNotify(bought);
            }
        }
    }
}