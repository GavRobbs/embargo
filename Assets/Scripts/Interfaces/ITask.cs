public interface ITask {
    void OnTaskEnter();
    void OnTaskUpdate(float dt);
    void OnTaskExit();
    void OnTaskCancel();

    float Progress { get; }

    void Cancel();
}