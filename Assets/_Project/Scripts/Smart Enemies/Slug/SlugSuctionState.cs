using UnityEngine;

public class SlugSuctionState : ISlugState
{
    private readonly SlugAI _own;
    private float _timer;
    private StatusDataSystem _playerStatuses;
    private PlayerHealth _playerHealth;
    private Transform _playerTransform;
    private Vector3 suctionCenter;

    public SlugSuctionState(SlugAI own) => _own = own;

    public void Enter()
    {
        _timer = 0f;

        if (_own.playerTransfrom != null)
        {
            _playerTransform = _own.playerTransfrom;
            _playerHealth = _playerTransform.GetComponent<PlayerHealth>();
            _playerStatuses = _playerTransform.GetComponent<StatusDataSystem>();
        }
        if (_playerTransform == null) return;

        suctionCenter = _own.transform.position;

        _own.rb.linearVelocity = Vector2.zero;

        _playerStatuses?.AddStatus(new StatusData(
            type: StatusType.Root,
            duration: _own.suctionDuration,
            blockMovement: true,
            blockJump: true,
            exitOnDash: true));

        _playerHealth?.TakeDamage(_own.suctionDamage);
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        if (_playerTransform != null && _playerStatuses != null && _playerStatuses.HasStatus(StatusType.Root))
        {
            _playerTransform.position = Vector3.Lerp(_playerTransform.position, suctionCenter, Time.deltaTime * 8f);
        }
        else
        {
            Spit();
            return;
        }

        if (_timer >= _own.suctionDuration) Spit();
    }

    public void Exit() { }

    private void Spit()
    {
        _playerStatuses?.RemoveStatus(StatusType.Root);

        if (_playerTransform != null)
        {
            PlayerController controller = _playerTransform.GetComponent<PlayerController>();
            Vector2 spitDir = ((Vector2)_playerTransform.position - (Vector2)_own.transform.position).normalized;

            //if (spitDir.magnitude < 0.1f) spitDir = Vector2.right;
            controller?.ApplyKnockback(spitDir, 6f);
        }

        _own.TransitionTo(_own.GetChase());
    }
}