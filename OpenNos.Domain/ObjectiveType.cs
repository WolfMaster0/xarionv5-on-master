namespace OpenNos.Domain
{
    public enum ObjectiveType : byte
    {
        KillMonster = 1,
        CollectItem = 2,
        KillMonsterAndCollectItem = 3,
        DeliverItem = 4,
        CaptureMonster = 5,
        CaptureMonster2 = 6,
        CompleteTimespace = 7,
        Produce = 8,
        AimedKill = 9,
        Aimed
            = 10,
        EarnPointInTimeSpace=11,
        Talk = 12,
        CollectItemInTimeSpace = 13,
        TalkWhenItemInInventory = 14,
        EquipAndTalk = 15,
        Unused16 = 16,
        KillMonsterAndCollectItem2 = 17,
        DeliverGold = 18,
        WalkPosition = 19,
        CollectItemFromNpc = 20,
        UseItemOnNpcMonster=21,
        TalkGetInformation = 22,
        Unusued23 = 23,
        InspectNpcMonster = 24,
        CompleteRaid = 25,
        KillMonsterLevelLimit = 26
    }
}