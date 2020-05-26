using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scooch : MonoBehaviour
{
    public float _moveSpeed;
    public float _winPos;
    public float _susRange;
    public float _gracePeriod;
    public float _minSus, _maxSus, _susTime;

    public Image _girl;
    public Color _susColor;
    public Color _normColor;
    public Color _bustedColor;
    public Transform _eyes;
    public float _normEyePos;
    public float _susEyePos;
    public float _bustedEyePos;

    [Header("Audio")]
    public AudioSource _scooch;
    public Vector2 _scoochAudioRange;

    [Header("Anim")]
    public float _eyeSpeed;

    public UnityEvent _OnWin;
    public UnityEvent _OnLose;

    public enum GameStates { PLAY, WIN, LOSE}
    [Header("No edit")]
    public GameStates _state = GameStates.PLAY;
    bool _moved;



    public enum StealthStates { CHILL, SUSPECT, BUSTED}
    public StealthStates _stealthState;
    public float _lastKnownPos;
    public float _curProgress;
    public float _susTimer;
    // Start is called before the first frame update
    void Start()
    {
        _lastKnownPos = transform.localPosition.x;
        _eyes = _girl.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case GameStates.PLAY:
                _moved = false;
                if (Input.anyKeyDown)
                {
                    Move();
                    _moved = true;
                }
                break;
            case GameStates.WIN:
                break;
            default:
                break;
        }
        
        switch (_stealthState)
        {
            case StealthStates.CHILL:
                _eyes.localPosition = Vector3.right*Mathf.Lerp(_eyes.localPosition.x, _normEyePos, _eyeSpeed * Time.deltaTime);
                break;
            case StealthStates.SUSPECT:
                _eyes.localPosition = Vector3.right * Mathf.Lerp(_eyes.localPosition.x, _susEyePos, _eyeSpeed * Time.deltaTime);
                _susTimer += Time.deltaTime;
                if(_susTimer > _gracePeriod && _moved)
                {
                    Lose();
                }
                if(_susTimer > _susTime)
                {
                    _girl.color = _normColor;
                    _stealthState = StealthStates.CHILL;
                }
                break;
            case StealthStates.BUSTED:
                _eyes.localPosition = Vector3.right * Mathf.Lerp(_eyes.localPosition.x, _bustedEyePos, _eyeSpeed * Time.deltaTime);
                break;
            default:
                break;
        }

        
    }

    private void Move()
    {
        transform.localPosition += Vector3.left * _moveSpeed;
        _curProgress = Mathf.Abs(transform.localPosition.x - _lastKnownPos);

        //sus logic
        if (Random.value * _curProgress > _susRange)
        {
            _stealthState = StealthStates.SUSPECT;
            _girl.color = _susColor;
            _susTimer = 0;
            _lastKnownPos = transform.localPosition.x;
            _susTime = Random.Range(_minSus, _maxSus);
        }
        
        //win logic
        if(transform.localPosition.x<= _winPos)
        {
            Win();
        }

        _scooch.pitch = Random.Range(_scoochAudioRange.x, _scoochAudioRange.y);
        _scooch.Play();
    }

    private void Win()
    {
        Debug.Log("Win");
        _state = GameStates.WIN;
        _stealthState = StealthStates.BUSTED; //just to get the eye movement
        _OnWin.Invoke();
    }

    private void Lose()
    {
        _girl.color = _bustedColor;
        _stealthState = StealthStates.BUSTED;
        _state = GameStates.LOSE;
        _OnLose.Invoke();
    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }
}
