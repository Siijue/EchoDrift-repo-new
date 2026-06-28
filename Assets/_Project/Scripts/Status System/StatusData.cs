public class StatusData
{
    public StatusType Type { get; }
    public float Duration {  get; }
    public float TimeLeft { get; private set; }

    // custom params
    public float DemageForSeconds { get; }
    public float SpeedMultiplier { get; }
    public bool BlockMovement { get; }
    public bool BlockJump { get; }
    public bool ExitOnDash { get; }

    public bool IsExpired => TimeLeft <= 0f;

    public StatusData(
        StatusType type,
        float duration,
        float damageForSeconds = 0f,
        float speedMultiplier = 1f,
        bool blockMovement = false,
        bool blockJump = false,
        bool exitOnDash = false)
    {
        Type = type;
        Duration = duration;
        TimeLeft = duration;
        DemageForSeconds = damageForSeconds;
        SpeedMultiplier = speedMultiplier;
        BlockMovement = blockMovement;
        BlockJump = blockJump;
        ExitOnDash = exitOnDash;
    }

    public void Tick(float deltaTime) => TimeLeft -= deltaTime;

    public static StatusData CreateCough() => new StatusData(type: StatusType.Cough, duration: 1f, blockMovement: true, blockJump: true);
    public static StatusData CreateBurn() => new StatusData(type: StatusType.Burn, duration: 6f, damageForSeconds: 0.25f);
    public static StatusData CreateSlow() => new StatusData(type: StatusType.Slow, duration: 1f, blockMovement: true, blockJump: true);
    public static StatusData CreateRoot() => new StatusData(type: StatusType.Root, duration: 3f, blockMovement: true, blockJump: true, exitOnDash: true);
}
