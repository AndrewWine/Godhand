public interface IProgression
{
    int Level { get; }
    int MaxLevel { get; }

    int TotalExp { get; }
    int ExpRequiredThisLevel { get; }      // 0 nếu không áp dụng
    int ExpInCurrentLevel { get; }         // 0 nếu không áp dụng
    float Progress01 { get; }              // 1 nếu không áp dụng

    void AddExp(int amount);               // Bỏ qua nếu không áp dụng
}
