using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour {

	int _aiLevel;

	TicTacToeState[,] boardState;

	[SerializeField]
	private bool _isPlayerTurn;

	[SerializeField]
	private int _gridSize = 3;

	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.circle;
	TicTacToeState aiState = TicTacToeState.cross;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	ClickTrigger[,] _triggers;

	//MyVariables
	int[,] testBoard;
	bool aiMoveToWin;
	bool aiMoveToBlock;
	int turn = 0;
	int[] winningGrid = new int[2] { -1, -1 };
	private bool gameOver;
	private TicTacToeState winner;
	private int[] openingMoveGrid;
	private int[,] board;
	private float delay;
	private float startTime;
	[SerializeField] private bool playerIsWaiting;
	[SerializeField] private GameObject go_audioSource;
	[SerializeField] private GameObject go_audioSourceGameLoop;
	[SerializeField] private GameObject go_audioSourceWaiting;
	private AudioSource audioSourceSFX;
	private AudioSource audioSourceGameLoop;
	private AudioSource audioSourceWaiting;

	[SerializeField] private AudioClip buttonClick;
	[SerializeField] private AudioClip audioGameLoop;
	[SerializeField] private AudioClip waiting;
	[SerializeField] private AudioClip click;
	[SerializeField] private AudioClip win;
	[SerializeField] private AudioClip lose;

	[SerializeField] private Text winLoseDrawText;

	[SerializeField] private GameObject gamePanel;
	private bool hasPlayedWaitingSound;
	private bool hasPlayedGameOver;
	private bool waitingSFXStarted = false;
	private TicTacToeState emptyState = TicTacToeState.none;
	private GameObject[,] gamePieces = new GameObject[3, 3];
	private bool winnerFound;

	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

    private void Start() {
		//testBoard = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
		turn = 1;
		winner = TicTacToeState.none;
		_isPlayerTurn = true;
		openingMoveGrid = new int[2] { -1, -1 };
		board = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
		winLoseDrawText.text = "";
		delay = 1.5f;
		playerIsWaiting = false;
		audioSourceSFX = go_audioSource.GetComponent<AudioSource>();
		audioSourceGameLoop = go_audioSourceGameLoop.GetComponent<AudioSource>();
		audioSourceWaiting = go_audioSourceWaiting.GetComponent<AudioSource>();
		hasPlayedWaitingSound = false;
		winnerFound = false;
	}

    public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();

		//play button click
		audioSourceGameLoop.clip = audioGameLoop;
		audioSourceGameLoop.Play();

		audioSourceSFX.clip = buttonClick;
		audioSourceSFX.Play();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];
		onGameStarted.Invoke();

	}

    private void Update() {

		gameOver = CheckIfGameOver(board, out winner);
		aiMoveToWin = CheckStep(board, 2);
		aiMoveToBlock = CheckStep(board, -2);
		//Debug.Log("Move to win? : " + aiMoveToWin);

		if(playerIsWaiting && !waitingSFXStarted && turn!=10 && !gameOver) {
			audioSourceWaiting.enabled = true;
			waitingSFXStarted = true;
        }

		if(!playerIsWaiting && turn!=10 && !gameOver) {
			audioSourceWaiting.enabled = false;
			waitingSFXStarted = false;
		}

		if (turn == 2) {
			int[] openingMoveCoordinates = MakeOpeningMove(board);
			StartCoroutine(AIDelay(openingMoveCoordinates[0], openingMoveCoordinates[1]));
			//AiSelects(openingMoveCoordinates[0], openingMoveCoordinates[1]);
		}

		if (gameOver && !winnerFound) {
			//Debug.Log("Game Over. Winner is: " + winner);
			//playerIsWaiting = true;
			//_isPlayerTurn = false;
			if (winner == TicTacToeState.circle) {
				winLoseDrawText.text = "YOU WIN!\n Click RETRY to play again";
				winnerFound = true;
				if(!hasPlayedGameOver) {
					audioSourceSFX.clip = win;
					audioSourceSFX.Play();
					hasPlayedGameOver = true;
				}
					
			} else if (winner == TicTacToeState.cross) {
				winLoseDrawText.text = "AI WINS!\n Click RETRY to try again";
				winnerFound = true;
				if (!hasPlayedGameOver) {
					audioSourceSFX.clip = lose;
					audioSourceSFX.Play();
					hasPlayedGameOver = true;
				}
	
			}
			gamePanel.SetActive(true);
		} else if (!gameOver && turn == 10) {
			winLoseDrawText.text = "TIE!\n Click RETRY to try again";
				if (!hasPlayedGameOver) {
					audioSourceSFX.clip = lose;
					audioSourceSFX.Play();
					hasPlayedGameOver = true;
				}
			gamePanel.SetActive(true);
		}

		if (aiMoveToWin && !gameOver) {
			//find row, col or diagonal and corresponding empty slot
			winningGrid = GetGrid(board, 2);
			//Debug.Log("Winning Grid is  X: " + winningGrid[0] + "  Y: " + winningGrid[1]);
				
			//move AI into that empty slot
			StartCoroutine(AIDelay(winningGrid[0], winningGrid[1]));
			//AiSelects(winningGrid[0], winningGrid[1]);
		}

		if (aiMoveToBlock && !aiMoveToWin && !gameOver) {
			//Debug.Log("Ai should move to block");
			winningGrid = GetGrid(board, -2);
			StartCoroutine(AIDelay(winningGrid[0], winningGrid[1]));
			//AiSelects(winningGrid[0], winningGrid[1]);

		}

		if (!aiMoveToBlock && !aiMoveToWin && turn >= 4 && !gameOver) {
			//use a MiniMax algorithm
			int[] bestMove = GetBestMove();

			if (bestMove != null) {
				StartCoroutine(AIDelay(bestMove[0], bestMove[1]));
				//AiSelects(bestMove[0], bestMove[1]);
			}
		}
		
    }

	private IEnumerator AIDelay(int xCoord, int yCoord) {
		yield return new WaitForSeconds(1.5f);
		AiSelects(xCoord, yCoord);
		
	}
		
	private int[] GetBestMove() {
		int bestScore = -1000;
		int[] bestMove = null;

		for(int i=0; i<_gridSize; i++) {
			for(int j=0; j<_gridSize; j++) {
				if(board[i,j] ==0) {
					board[i, j] = 1;
					int score = MyMiniMax(board, 0, false);
					board[i, j] = 0;

					if(score>bestScore) {
						bestScore = score;
						bestMove = new int[] { i, j };
                    }
                }
            }
        }

		return bestMove;
    }

	private int MyMiniMax(int[,] board, int depth, bool isMaximizing) {
		if(isMaximizing) {

			int bestScore = -1000;
			for(int i=0; i<_gridSize; i++) {
				for(int j=0; j<_gridSize; j++) {
					if(board[i,j] ==0) {
						board[i, j] = 1;
						int score = MyMiniMax(board, depth + 1, false);
						board[i, j] = 0;
						bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }

			return bestScore;

        } else {

			int bestScore = 1000;
			for (int i = 0; i < _gridSize; i++) {
				for (int j = 0; j < _gridSize; j++) {
					if (board[i, j] == 0) {
						board[i, j] = -1;
						int score = MyMiniMax(board, depth + 1, true);
						board[i, j] = 0;
						bestScore = Mathf.Max(score, bestScore);
					}
				}
			}
			return bestScore;
		}
    }


	private int[] MakeOpeningMove(int[,] board) {
		int[] openingMoveCoordinates = new int[2] { -1, -1 };
		if(board[1,1] == -1) {
			int[] keyCoordinates = new int[2] { 0, 2 };
			openingMoveCoordinates[0] = keyCoordinates[UnityEngine.Random.Range(0, 2)];
			openingMoveCoordinates[1] = keyCoordinates[UnityEngine.Random.Range(0, 2)];
		} else if(board[1,1] == 0) {
			openingMoveCoordinates[0] = 1;
			openingMoveCoordinates[1] = 1;
        }

		return openingMoveCoordinates;
    }

	private bool CheckIfGameOver(int[,] board, out TicTacToeState winner) {

		//check rows, if sum of row = 3, return true
		//winner = TicTacToeState.cross
		//if sum row = -3, return true, winner = TicTacToeState.cirlce

		for (int row = 0; row < _gridSize; row++) {
			int sumRow = 0;
			for (int col = 0; col < _gridSize; col++) {
				sumRow += board[row, col];
			}
			if (sumRow == 3) {
				winner = TicTacToeState.cross;
				return true;
			} else if (sumRow == -3) {
				winner = TicTacToeState.circle;
				return true;
			} 
		}

		//check columns
		for (int col = 0; col < _gridSize; col++) {
			int sumCol = 0;
			for (int row = 0; row < _gridSize; row++) {
				sumCol += board[row, col];
			}
			if (sumCol == 3) {
				winner = TicTacToeState.cross;
				return true;
			} else if (sumCol == -3) {
				winner = TicTacToeState.circle;
				return true;
			}
		}

		//check diagonals
		int sumDiag1 = 0;
		for (int i = 0; i < _gridSize; i++) {
			sumDiag1 += board[i, i];
		}
		if (sumDiag1 == 3) {
			winner = TicTacToeState.cross;
			return true;
		} else if(sumDiag1==-3) {
			winner = TicTacToeState.circle;
			return true;
		}

		int sumDiag2 = 0;
		for (int i = 0; i < _gridSize; i++) {
			sumDiag2 += board[2 - i, i];
		}
		if (sumDiag2 == 3) {
			winner = TicTacToeState.cross;
			return true;
			//Debug.Log("Winner is AI");
		} else if(sumDiag2==-3) {
			winner = TicTacToeState.circle;
			return true;
        }

		winner = TicTacToeState.none;
		return false;
    }

	private int[] GetGrid(int[,] board, int valueDeterminant) {
		int[] winningGrid = new int[2] { -1, -1 };

		//check rows, set winningGrid to [row, col]
		for (int row = 0; row < _gridSize; row++) {
			int sumRow = 0;
			for(int col=0; col<_gridSize; col++) {
				sumRow += board[row, col];
				if (sumRow == valueDeterminant) {
					//if pass in 2 for valueDeterminant, will be true 
					//when sumRow == 2
					int keyRow = row;
					for(int i=0; i<_gridSize; i++) {
						if(board[keyRow, i] == 0) {
							winningGrid = new int[2] { keyRow, i};
                        }
                    }
				}
            }
		}

		//check columns, set winningGrid to [row, col]
		//check rows, set winningGrid to [row, col]
		for (int col = 0; col < _gridSize; col++) {
			int sumCol = 0;
			for (int row = 0; row < _gridSize; row++) {
				sumCol += board[row, col];
				//Debug.Log("Sum Column: " + sumCol);
				if (sumCol == valueDeterminant) {
					int keyCol = col;
					//Debug.Log("Key Column: " + keyCol);
					for(int i=0; i<_gridSize; i++) {
						if(board[i, keyCol] == 0) {
							winningGrid = new int[2] { i, keyCol };
							//Debug.Log("Key Grid: " + winningGrid[0] + " " + winningGrid[1]);
                        }
                    }
				}
			}
		}

		//check diagonals
		int sumDiag1 = 0;
		for(int i =0; i<_gridSize; i++) {
			sumDiag1 += board[i, i];
        }
		if (sumDiag1 == valueDeterminant) {
			for (int j = 0; j < _gridSize; j++) {
				if (board[j, j] == 0) {
					winningGrid = new int[2] { j, j };
				}
			}
		}

		int sumDiag2 = 0;
		for(int i=0; i<_gridSize; i++) {
			sumDiag2 += board[2 - i, i];
        }
		if(sumDiag2== valueDeterminant) {
			for(int j=0; j<_gridSize; j++) {
				if(board[2-j,j] == 0) {
					winningGrid = new int[2] { 2 - j, j };
                }
            }
        }

		return winningGrid;
    }

    private bool CheckStep(int[,] board, int determinant) {

		//sum each row
		int sumRow1 = board[0,0] + board[0,1] + board[0,2];
		int sumRow2 = board[1,0] + board[1,1] + board[1,2];
		int sumRow3 = board[2,0] + board[2,1] + board[2,2];

		//sum each column
		int sumCol1 = board[0, 0] + board[1, 0] + board[2, 0];
		int sumCol2 = board[0, 1] + board[1, 1] + board[2, 1];
		int sumCol3 = board[0, 2] + board[1, 2] + board[2, 2];

		//sum each diagonal
		int sumDiag1 = board[2, 0] + board[1, 1] + board[0, 2];
		int sumDiag2 = board[0, 0] + board[1, 1] + board[2, 2];

		//check if rows sum to 2
		if (sumRow1 == determinant || sumRow2 == determinant || sumRow3 == determinant) {
			//Debug.Log("Sum of Row is 2, AI should move to win");
			return true;
        }

		//check if columns sum to 2
		if (sumCol1 == determinant || sumCol2 == determinant || sumCol3 == determinant) {
			//Debug.Log("Sum of Col is 2, AI should move to win");
			return true;
		}

		//check if diagonals sum to 2
		if(sumDiag1 == determinant || sumDiag2 == determinant) {
			//Debug.Log("Sum of Diag is 2, AI should move to win");
			return true;
        }

		return false;
    }

	public void PlayerSelects(int coordX, int coordY){

		if(_isPlayerTurn && !playerIsWaiting && board[coordX, coordY] != -1 && board[coordX, coordY] != 1) {
			turn += 1;
			SetVisual(coordX, coordY, playerState);
			board[coordX, coordY] = -1;

			audioSourceSFX.clip = click;
			audioSourceSFX.Play();

			_isPlayerTurn = false;
			playerIsWaiting = true;
		}
		
	}

	public void AiSelects(int coordX, int coordY){

		if(!_isPlayerTurn && playerIsWaiting && board[coordX, coordY] != -1 && board[coordX, coordY] != 1 && !gameOver) { 
			turn += 1;
			SetVisual(coordX, coordY, aiState);
			board[coordX, coordY] = 1;

			audioSourceSFX.clip = click;
			audioSourceSFX.Play();

			_isPlayerTurn = true;
			playerIsWaiting = false;
		}
	}

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);

	}


}
