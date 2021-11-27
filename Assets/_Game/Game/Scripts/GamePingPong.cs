using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GamePingPong : MonoBehaviour
{
    [Header("Game Variables")]
    [SerializeField] private Material mat;
    [SerializeField] private Color colorBarTop;
    
    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI textPlayer1Points;
    [SerializeField]private TextMeshProUGUI textPlayer1Life;
    [SerializeField]private TextMeshProUGUI textPlayer2Points;
    [SerializeField]private TextMeshProUGUI textPlayer2Life;
    [SerializeField]private TextMeshProUGUI textAccumulatedPoints;
    [SerializeField]private TextMeshProUGUI textWin;
    [SerializeField]private GameObject pausePanel;
    [SerializeField]private GameObject gamePanel;
    [SerializeField]private GameObject menuPanel;
    [SerializeField]private GameObject winPanel;

    [Header("Conditionals Variables")] 
    [SerializeField] private bool isStarted;
    [SerializeField] private bool isPause;

    [Header("Player 1")] 
    [Header("Game Variables")]
    [SerializeField] private int player1Life = 3;
    [SerializeField] private int player1Points = 0;
    
    [Header("Player 2")] 
    [Space(20)]
    [SerializeField] private int player2Life = 3;
    [SerializeField] private int player2Points = 0;
    
    //Private Variables
    private Vector2 _canvasSize;
    
    private float _player1YBottom = -1;
    private float _player1YUp = 1;

    private float _player2YBottom = -1;
    private float _player2YUp = 1;

    private const float PlayersVelocity = 0.1f;
    private const float PlayersUpdateVelocity = 0.05f;

    private Coroutine _coroutineMovePlayer1;
    private Coroutine _coroutineMovePlayer2;
    private Coroutine _updateGame;
    
    private float _squareX = 0;
    private float _squareY = 0;

    private bool _invertSquareX;
    private bool _invertSquareY;

    private float _squareVelocityX = 0.1f;
    private float _squareVelocityY = 0.1f;

    private int _accumulatedPoints = 1;

    #region Unity Methods

    private void Awake()
    {
        var inputMaster = new InputMaster();
        inputMaster.Enable();
        inputMaster.Player1.Movement.started += ctx => OnPlayerMove(ctx.ReadValue<Vector2>(), 1, true);
        inputMaster.Player1.Movement.canceled += ctx => OnPlayerMove(ctx.ReadValue<Vector2>(), 1, false);
        inputMaster.Player1.Pause.performed += ctx => PauseGame();
        inputMaster.Player2.Movement.started += ctx => OnPlayerMove(ctx.ReadValue<Vector2>(), 2, true);
        inputMaster.Player2.Movement.canceled += ctx => OnPlayerMove(ctx.ReadValue<Vector2>(), 2, false);
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        player1Life = 3;
        player2Life = 3;
        player1Points = 0;
        player2Points = 0;
        _accumulatedPoints = 1;
        isPause = false;
        UpdatePlayersInfo();
        ResetSquare();
        _canvasSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    private void OnPlayerMove(Vector2 direction, int player, bool isStarted)
    {
        switch (player)
        {
            case 1:
                if (isStarted)
                    _coroutineMovePlayer1 = StartCoroutine(PlayerMove(direction, player));
                else
                    StopCoroutine(_coroutineMovePlayer1);
                break;
            case 2:
                if (isStarted)
                    _coroutineMovePlayer2 = StartCoroutine(PlayerMove(direction, player));
                else
                    StopCoroutine(_coroutineMovePlayer2);
                break;
        }
    }

    private IEnumerator PlayerMove(Vector2 direction, int player)
    {
        var fixedVelocity = PlayersVelocity * (direction.y < 0 ? -1 : 1);
        
        while (true)
        {
            switch (player)
            {
                case 1:
                    _player2YBottom += fixedVelocity;
                    _player2YUp += fixedVelocity;
                    break;
                case 2:
                    _player1YBottom += fixedVelocity;
                    _player1YUp += fixedVelocity;
                    break;
            }

            yield return new WaitForSeconds(PlayersUpdateVelocity);
        }
    }

    private void Update()
    {
        if (isPause)
            return;
            
        if (player1Life == 0)
        {
            isPause = true;
            winPanel.SetActive(true);
            textWin.text = $"Player 2 ganhou com {player1Points} pontos!";
        } else if (player2Life == 0)
        {
            isPause = true;
            winPanel.SetActive(true);
            textWin.text = $"Player 1 ganhou com {player1Points} pontos!";
        }
    }

    private IEnumerator NewUpdate()
    {
        while (true)
        {
            UpdatePlayersInfo();

            _canvasSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
            Move();
            Collision();
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    private void OnPostRender()
    {
        if (!isStarted)
        {
            return;
        }
        
        GameBackground();
        BarPlayer1();
        BarLeft();
        BarPlayer2();
        BarRight();
        BarTop();
        Square();
    }
    #endregion
    
    #region UI Methods
    public void StartGame()
    {
        Initialize();
        isStarted = true;
        _updateGame = StartCoroutine(NewUpdate());
    }

    public void PauseGame()
    {
        if (!isStarted) return;

        isPause = !isPause;
        pausePanel.SetActive(isPause);
    }

    public void ExitGame()
    {
        StopCoroutine(_updateGame);
        isPause = true;
        isStarted = false;
        winPanel.SetActive(false);
        pausePanel.SetActive(false);
        gamePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void UpdatePlayersInfo()
    {
        textAccumulatedPoints.text = _accumulatedPoints.ToString();
        textPlayer1Life.text = player1Life.ToString();
        textPlayer2Life.text = player2Life.ToString();
        textPlayer1Points.text = player1Points.ToString();
        textPlayer2Points.text = player2Points.ToString();
    }
    #endregion

    #region Game Methods
    void Square()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(Color.red);

        GL.Vertex3(_squareX, _squareY, 0);
        GL.Vertex3(_squareX, _squareY+.5f, 0);
        GL.Vertex3(_squareX+.5f, _squareY+.5f, 0);
        GL.Vertex3(_squareX+.5f, _squareY, 0);

        GL.End();
        GL.PopMatrix();
    }

    void BarPlayer1()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(colorBarTop);

        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x * (-1) + 1, _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x * (-1) + 1, _canvasSize.y, 0);

        GL.End();
        GL.PopMatrix();
    }
    
    void BarPlayer2()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(colorBarTop);

        GL.Vertex3(_canvasSize.x, _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x - 1, _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x - 1, _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x, _canvasSize.y * (-1), 0);

        GL.End();
        GL.PopMatrix();
    }
    
    void BarLeft()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);

        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x * (-1), _player1YUp, 0);
        GL.Vertex3(_canvasSize.x * (-1) + 1, _player1YUp, 0);
        GL.Vertex3(_canvasSize.x * (-1) + 1, _canvasSize.y, 0);
        
        GL.Vertex3(_canvasSize.x * (-1), _player1YBottom, 0);
        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x * (-1) + 1, _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x * (-1) + 1, _player1YBottom, 0);

        GL.End();
        GL.PopMatrix();
    }
    
    void BarRight()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);

        GL.Vertex3(_canvasSize.x, _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x, _player2YUp, 0);
        GL.Vertex3(_canvasSize.x - 1, _player2YUp, 0);
        GL.Vertex3(_canvasSize.x - 1, _canvasSize.y, 0);
        
        GL.Vertex3(_canvasSize.x, _player2YBottom, 0);
        GL.Vertex3(_canvasSize.x - 1, _player2YBottom, 0);
        GL.Vertex3(_canvasSize.x - 1, _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x, _canvasSize.y * (-1), 0);

        GL.End();
        GL.PopMatrix();
    }

    void GameBackground()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);

        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y * (-1), 0);
        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x, _canvasSize.y, 0);
        GL.Vertex3(_canvasSize.x, _canvasSize.y * (-1), 0);

        GL.End();
        GL.PopMatrix();
    }
    
    void BarTop()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(colorBarTop);

        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y , 0);
        GL.Vertex3(_canvasSize.x * (-1), _canvasSize.y - 2, 0);
        GL.Vertex3(_canvasSize.x, _canvasSize.y - 2, 0);
        GL.Vertex3(_canvasSize.x, _canvasSize.y, 0);

        GL.End();
        GL.PopMatrix();
    }

    void ResetSquare()
    {
        _squareVelocityX = .1f;
        _squareVelocityY = .1f;
        _squareX = 0;
        _squareY = 0;
    }
    
    void Move()
    {
        if (!isStarted) return;
        if (isPause) return;

        if (_squareX + .5f > _canvasSize.x && !_invertSquareX)
            _invertSquareX = true;
        else if (!_invertSquareX)
            _squareX += _squareVelocityX;

        if (_squareX < _canvasSize.x * (-1) && _invertSquareX)
            _invertSquareX = false;
        else if (_invertSquareX)
            _squareX -= _squareVelocityX;
        
        
        if (_squareY + .5f > _canvasSize.y - 2 && !_invertSquareY)
            _invertSquareY = true;
        else if (!_invertSquareY)
            _squareY += _squareVelocityY;

        if (_squareY < _canvasSize.y * (-1) && _invertSquareY)
            _invertSquareY = false;
        else if (_invertSquareY)
            _squareY -= _squareVelocityY;
    }
    void Collision()
    {
        if ((_squareX + 1f) >= (_canvasSize.x - .5f) && (_squareY + 0.2f >= _player2YBottom && _squareY + .5f <= _player2YUp) && !_invertSquareX)
        {
            _invertSquareX = !_invertSquareX;
            _accumulatedPoints += 1;
            _squareVelocityX *= 1.1f;
            _squareVelocityY *= 1.05f;
        } else if (_squareX >= _canvasSize.x - 0.8f)
        {
            player2Life--;
            player1Points += _accumulatedPoints;
            ResetSquare();
            _accumulatedPoints = 1;
        }
        
        if ((_squareX - .5f) <= (_canvasSize.x * (-1) + .5f) && (_squareY + 0.2f >= _player1YBottom && _squareY + .5f <= _player1YUp) && _invertSquareX)
        {
            _invertSquareX = !_invertSquareX;
            _accumulatedPoints += 1;
            _squareVelocityX *= 1.1f;
            _squareVelocityY *= 1.05f;
        } else if (_squareX <= _canvasSize.x * (-1) + 0.8f)
        {
            player1Life--;
            player2Points += _accumulatedPoints;
            ResetSquare();
            _accumulatedPoints = 1;
        }
    }
    #endregion
}
