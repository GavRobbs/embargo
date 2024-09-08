public interface ITaskable {
    ITask CurrentTask { get; set; }
    bool Busy { get; }
    void ClearTask();

    void CancelTask();
}