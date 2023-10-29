using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    private static readonly int VelocityUp = Animator.StringToHash("VelocityUp");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    private Rigidbody2D _rigidbody;
    private int _jumps;
    private Animator _animator;

    public GameController gameController;
    private Vector2? _fingerDownPos;

    private bool _isCrouching = false;
    private float _crouchingTime;

    public CapsuleCollider2D normalHitbox;
    public CapsuleCollider2D crouchingHitbox;

    public AudioSource jumpSound;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }
    private bool _onGround;

    void Jump()
    {
        UndoCrouch();
        if(_jumps <= 0) return;
        _rigidbody.velocity = Vector2.up * 7.5f;
        _jumps--;
        jumpSound.enabled = true;
        jumpSound.Play();
    }

    void Crouch()
    {
        if (_onGround && !_isCrouching)
        {
            _isCrouching = true;
            _crouchingTime = Time.time;
        }
        if (!_onGround && _rigidbody.velocity.y > -7.5f)
        {
            _rigidbody.velocity = Vector2.down * 7.5f;
        }
    }

    void UndoCrouch()
    {
        _isCrouching = false;
    }
    
    void Update()
    {
        if (!gameController.GameIsRunning() && gameController.gameOverTime < Time.time - 0.5)
        {
            if (Input.GetKeyDown(KeyCode.Return) 
                || Input.touches.Length > 0)
            {
                gameController.RestartGame();
            }
            return;
        }

        if (_isCrouching && _crouchingTime < (Time.time - 1))
        {
            UndoCrouch();
        }
        
        _animator.SetBool(OnGround, _onGround);
        _animator.SetBool(VelocityUp, _rigidbody.velocity.y > 0);
        _animator.SetBool(IsCrouching, _isCrouching);

        normalHitbox.enabled = !_isCrouching;
        crouchingHitbox.enabled = _isCrouching;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Jump();
        } else if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Crouch();
        }
        
        foreach (var touch in Input.touches) {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _fingerDownPos = touch.position;
                    break;
                case TouchPhase.Ended:
                    _fingerDownPos = null;
                    break;
                case TouchPhase.Moved:
                    if (_fingerDownPos != null)
                    {
                        var pos = touch.position;
                        var dx = pos.x - _fingerDownPos.Value.x;
                        var dy = pos.y - _fingerDownPos.Value.y;
                        if (Mathf.Abs(dy) > Mathf.Abs(dx) && Mathf.Abs(dy) > 20.0f)
                        {
                            _fingerDownPos = null;
                            if(dy > 0) Jump();
                            else Crouch();
                        }
                    }
                    break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("obstacle"))
        {
            gameController.StopGame();
        }
    }

    public void StopAnimation()
    {
        _animator.enabled = false;
        _rigidbody.simulated = false;
    }
    public void StartAnimation()
    {
        _animator.enabled = true;
        _rigidbody.simulated = true;
    }
    
    void OnCollisionExit2D(Collision2D other){
        if (!other.gameObject.CompareTag("obstacle"))
        {
            _onGround = false;
        }
    }
    
    void OnCollisionEnter2D(Collision2D other) {
        _jumps = 2;
        _onGround = true;        
    }
}
