using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI_ : MonoBehaviour
{

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

	//My Variables
	private string[,] boardRep;
	private int[] ai_moveGrid;
	private int turn;
	private bool aiFinished;

	[SerializeField] private bool createTestBoard, board1, board2;
	private bool boardStateDeclared;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

    private void Start() {
		turn = 1;
		_isPlayerTurn = true;


		//initialize boardState
		boardState = new TicTacToeState[3, 3];
		boardRep = new string[3, 3];
		for(int row=0; row<3; row++) {
			for(int col=0; col<3; col++) {
				boardState[row, col] = TicTacToeState.none;
				boardRep[row, col] = " . ";
            }
        }
		PrintBoard(boardRep);

		/*if(createTestBoard && board1) {
			boardState[0, 0] = TicTacToeState.cross;
			boardState[0, 1] = TicTacToeState.circle;
			boardState[0, 2] = TicTacToeState.none;

			boardState[1, 0] = TicTacToeState.circle;
			boardState[1, 1] = TicTacToeState.cross;
			boardState[1, 2] = TicTacToeState.none;

			boardState[2, 0] = TicTacToeState.none;
			boardState[2, 1] = TicTacToeState.none;
			boardState[2, 2] = TicTacToeState.circle;
		}

		if (createTestBoard && board2) {
			boardState[0, 0] = TicTacToeState.cross;
			boardState[0, 1] = TicTacToeState.circle;
			boardState[0, 2] = TicTacToeState.cross;

			boardState[1, 0] = TicTacToeState.cross;
			boardState[1, 1] = TicTacToeState.circle;
			boardState[1, 2] = TicTacToeState.circle;

			boardState[2, 0] = TicTacToeState.circle;
			boardState[2, 1] = TicTacToeState.cross;
			boardState[2, 2] = TicTacToeState.circle;
		}*/
	}

    public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	void Update() {

		/*if(createTestBoard && !boardStateDeclared) {
			bool boardFull = CheckBoardIsFull(boardState);

			if(boardFull) {
				Debug.Log("Board is full");

			} else if(!boardFull) {
				Debug.Log("Board is not full");

			} else {
				Debug.Log("Check Board Full method for exception.");
            }
			boardStateDeclared = true;
        }*/

		if(!_isPlayerTurn && !aiFinished) {
			Debug.Log("AI's Turn");
			ai_moveGrid = FindNextMove(boardState);
			//Debug.Log("Next Move is:  X-" + ai_moveGrid[0] + "  Y-" + ai_moveGrid[1]);
			//AiSelects(1, 1);
			AiSelects(ai_moveGrid[0], ai_moveGrid[1]);
        }
		

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


    public void PlayerSelects(int coordX, int coordY){

		//check that grid state is none (blank) before allowing player to select
		//this will prevent the player form placing more than one cirlce in a chosen grid
		if(boardState[coordX,coordY] == TicTacToeState.none) {
			SetVisual(coordX, coordY, playerState);
			boardState[coordX, coordY] = TicTacToeState.circle;
			boardRep[coordX, coordY] = " o";
			PrintBoard(boardRep);

			//send false and AIDelay( ) sets _isPlayerTurn to false after 1.5 second delay
			StartCoroutine(AIDelay(false));
			//_isPlayerTurn = false;
			//turn += 1;
		}


	}

	public void AiSelects(int coordX, int coordY){

		//check that grid state is none (blank) before allowing player to select
		//this will prevent the player form placing more than one cirlce in a chosen grid
		if (boardState[coordX, coordY] == TicTacToeState.none) {
			Debug.Log("Ai Selects X: " + coordX + "  Y: " + coordY);
			SetVisual(coordX, coordY, aiState);
			boardState[coordX, coordY] = TicTacToeState.cross;
			boardRep[coordX, coordY] = " x";
			PrintBoard(boardRep);

			//send true and AIDelay( ) sets _isPlayerTurn to true after 1.5 second delay
			StartCoroutine(AIDelay(true));
			//_isPlayerTurn = true;
			//turn += 1;
		}

	}

	private int[] FindNextMove(TicTacToeState[,] board) {
		int bestVal = int.MinValue;
		int[] bestMove = { -1, -1 };

		for(int row=0; row<_gridSize; row++) {
			for(int col=0; col<_gridSize; col++) {

				//evaluate value of moving X to this current empty slot
				if(board[row,col] == TicTacToeState.none) {
					board[row, col] = TicTacToeState.cross;
					int moveValue = MyMiniMaxMethod(board, 0, true);
					Debug.Log("Move Value: " + moveValue);
					board[row, col] = TicTacToeState.none;

					if(moveValue>bestVal) {
						bestMove[0] = row;
						bestMove[1] = col;
						bestVal = moveValue;
                    }
                }
            }
        }
		Debug.Log(" Line 209, X: " + bestMove[0] + "  Y" + bestMove[1]);
		return bestMove;
    }

	private int MyMiniMaxMethod(TicTacToeState[,] board, int depth, bool isMaximizing) {
		int score = EvaluateBoard(board);

		if(score ==10) {
			return score;
        }

		if(isMaximizing) {
			int best = int.MinValue;

			for (int row = 0; row < _gridSize; row++) {
				for (int col = 0; col < _gridSize; col++) {
					if (board[row, col] == TicTacToeState.none) {
						board[row, col] = TicTacToeState.cross;
						int value = MyMiniMaxMethod(board, depth + 1, true);
						best = Mathf.Max(best, value);
						board[row, col] = TicTacToeState.none;
					}
				}
			}
			return best - depth;
		} else {
			int best = int.MaxValue;
			for (int row = 0; row < _gridSize; row++) {
				for (int col = 0; col < _gridSize; col++) {
					if (board[row, col] == TicTacToeState.none) {
						board[row, col] = TicTacToeState.cross;
						int value = MyMiniMaxMethod(board, depth + 1, false);
						best = Mathf.Max(best, value);
						board[row, col] = TicTacToeState.none;
					}
				}
			}
			return best + depth;
		}
		
    }

	private int EvaluateBoard(TicTacToeState[,] board) {
		//will take the depth+1 board state passed from MiniMax function
		//and return +10 if AI wins with the prospective move,
		//-10 if the player wins with the prospective move or 
		//0 if no one wins.

		for (int row = 0; row < _gridSize; row++) {
			if(board[row,0] == board[row,1] && board[row,1] == board[row,2]) {
				if(board[row, 0] == TicTacToeState.cross) {
					return +10;
                } else if(board[row,0] == TicTacToeState.circle) {
					return -10;
                } 
            }	
        }

		for(int col=0; col<_gridSize; col++) {
			if(board[0,col] == board[1,col] && board[1,col] == board[2, col]) {
				if(board[0,col] == TicTacToeState.cross) {
					return 10;
                } else if(board[0,col] == TicTacToeState.circle) {
					return 10;
                }
            }
        }

		if(board[2,0] == board[1,1] && board[1,1] == board[0,2]) {
			if(board[2,0] == TicTacToeState.cross) {
				return 10;
            } else if(board[2,0] == TicTacToeState.circle) {
				return -10;
            }
        }

		if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 0]) {
			if (board[0, 0] == TicTacToeState.cross) {
				return 10;
			} else if (board[0, 0] == TicTacToeState.circle) {
				return -10;
			}
		}

		//if neither AI or player have a row, return 0 for neutral move
		return 0;
    }

	private bool CheckBoardIsFull(TicTacToeState[,] board) {
		for(int row=0; row<_gridSize; row++) {
			for(int col=0; col<_gridSize; col++) {
				if(board[row,col] == TicTacToeState.none) {
					return false;
                }
            }
        }
		return true;
    }

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}

	IEnumerator AIDelay(bool condition) {
		aiFinished = condition;
		yield return new WaitForSeconds(1.5f);
		_isPlayerTurn = condition;
		turn += 1;
    }
	
	void PrintBoard(string[,] board) {
		string boardString = "";
		
		for(int row=0; row<3; row++) {
			for(int col=0; col<3; col++) {
				boardString += board[row, col];
				if(col<2) {
					boardString += " | ";
                }
            }
			boardString += "\n";

			if(row<2) {
				//boardString += "---+---+---\n";
            }
        }
		Debug.Log(boardString);
    }

}
