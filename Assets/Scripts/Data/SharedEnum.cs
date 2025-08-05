namespace ProjectIdle
{
    public enum EItemType
    {
        None = 0, // 기본값: 아이템 없음
        Equipment, // 장비 아이템
        Consumable, // 소비 아이템
        Material, // 재료 아이템
        Currency, // 통화 아이템
        QuestItem, // 퀘스트 아이템
        Special, // 특별한 아이템 (예: 이벤트 아이템)
    }

    public enum ECurrencyID
    {
        Gold,
        Gems
    }
        
    public enum EItemRarity
    {
        Common, // 일반 아이템
        Uncommon, // 희귀 아이템
        Rare, // 아주 희귀 아이템
        Epic, // 전설 아이템
        Legendary, // 신화 아이템
        Mythic // 신화급 아이템 (가장 높은 등급)
    }
}