using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scooch : MonoBehaviour
{
    public float _moveSpeed;
    public float _winPos;

    public enum GameStates { PLAY, WIN }
    public GameStates _state = GameStates.PLAY;

    public enum StealthStates { CHILL, SUSPECT, BUSTED}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case GameStates.PLAY:
                if (Input.anyKeyDown)
                {
                    Move();
                }
                break;
            case GameStates.WIN:
                break;
            default:
                break;
        }
    }

    private void Move()
    {
        transform.localPosition += Vector3.left * _moveSpeed;
        if(transform.localPosition.x<= _winPos)
        {
            Win();
        }
    }

    private void Win()
    {
        Debug.Log("Win");
        _state = GameStates.WIN;
    }
}
