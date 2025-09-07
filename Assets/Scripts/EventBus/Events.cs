

public struct GrantExpEvent : IHasSourceId
{
    public int SourceId { get; }
    public int Amount { get; }
    public GrantExpEvent(int sourceId, int amount) { SourceId = sourceId; Amount = amount; }
}

// Đã cộng EXP (sau khi xử lý)
public struct ExpChangedEvent : IHasSourceId
{
    public int SourceId { get; }
    public int TotalExp { get; }
    public int Level { get; }
    public int ExpInLevel { get; }
    public int ExpRequiredThisLevel { get; }
    public float Progress01 { get; } //Dự định dùng hiển thị tiến trình level của nhân vật
    public ExpChangedEvent(int sourceId, int totalExp, int level, int inLevel, int req, float p)
    {
        SourceId = sourceId; TotalExp = totalExp; Level = level;
        ExpInLevel = inLevel; ExpRequiredThisLevel = req; Progress01 = p;
    }
}

// Lên cấp
public struct LevelUpEvent : IHasSourceId
{
    public int SourceId { get; }
    public int NewLevel { get; }
    public int LevelsGained { get; } //nếu lên nhiều cấp 1 lần
    public LevelUpEvent(int sourceId, int newLevel, int levelsGained)
    { SourceId = sourceId; NewLevel = newLevel; LevelsGained = levelsGained; }
}
