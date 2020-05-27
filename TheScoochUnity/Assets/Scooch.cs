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
    public float _introTime;

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
    public AudioSource _alert;
    public AudioSource _chill;
    public AudioSource _busted;
    public AudioSource _smooch;
    public AudioSource _highScore;

    [Header("Anim")]
    public float _eyeSpeed;

    public UnityEvent _OnWin;
    public UnityEvent _OnLose;

    public Text _curTime;
    public Text _recordTime;

    public Text _progText;

    public enum GameStates { INTRO, PLAY, WIN, LOSE}
    [Header("No edit")]
    public GameStates _state = GameStates.INTRO;
    bool _moved;

    //timing /scoring
    float _gameTimer = 0;

    public enum StealthStates { CHILL, SUSPECT, BUSTED}
    public StealthStates _stealthState;
    public float _lastKnownPos;
    public float _curProgress;
    public float _susTimer;
    float _startPos;
    // Start is called before the first frame update
    void Start()
    {
        _lastKnownPos = transform.localPosition.x;
        _eyes = _girl.transform.GetChild(0);
        if (PlayerPrefs.HasKey("record"))
        {
            _recordTime.text = "Record time: " + PlayerPrefs.GetFloat("record").ToString("#.##");
        }
        _startPos = transform.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case GameStates.INTRO:
                if(Time.timeSinceLevelLoad > _introTime)
                {
                    _state = GameStates.PLAY;
                }
                break;
            case GameStates.PLAY:
                _moved = false;
                _gameTimer += Time.deltaTime;
                _curTime.text = "Current time: " + _gameTimer.ToString("#.##");
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
                _girl.color = Color.Lerp(_susColor, _normColor, _susTimer / _susTime);
                if(_susTimer > _gracePeriod && _moved)
                {
                    Lose();
                }
                if(_susTimer > _susTime)
                {
                    _girl.color = _normColor;
                    _stealthState = StealthStates.CHILL;
                    _chill.Play();
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

        float progPercent = Mathf.CeilToInt(Mathf.InverseLerp(_startPos, _winPos, transform.localPosition.x)*100f);
        _progText.text = progPercent.ToString("#") + "%";

        //sus logic
        if (Random.value * _curProgress > _susRange)
        {
            _stealthState = StealthStates.SUSPECT;
            _girl.color = _susColor;
            _susTimer = 0;
            _lastKnownPos = transform.localPosition.x;
            _susTime = Random.Range(_minSus, _maxSus);
            _alert.Play();
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
        _smooch.Play();
        _state = GameStates.WIN;
        _girl.color = _normColor;
        _stealthState = StealthStates.BUSTED; //just to get the eye movement
        _OnWin.Invoke();

        //score logic
        if (PlayerPrefs.HasKey("record"))
        {
            float hs = PlayerPrefs.GetFloat("record");
            if(_gameTimer > hs)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetFloat("record", _gameTimer);
                StartCoroutine(RecordBlink());
            }
        }
        else
        {
            PlayerPrefs.SetFloat("record", _gameTimer);
            StartCoroutine(RecordBlink());
        }
    }

    private void Lose()
    {
        _busted.Play();
        _girl.color = _bustedColor;
        _stealthState = StealthStates.BUSTED;
        _state = GameStates.LOSE;
        _OnLose.Invoke();
    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator RecordBlink() {
        _highScore.Play();
        yield return null;
        _recordTime.text = "Record time: " + _gameTimer.ToString("#.##");
        float timer = 0;
        Color newC;
        while (true)
        {
            newC = Color.white * Mathf.PingPong(timer, 1f);
            newC.a = 1f;
            _recordTime.color = newC;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
