using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle}
public enum Winner { none, player, ai };

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
	private bool gameOver;
	Winner winner;


	[SerializeField] private bool createTestBoard, board1, board2;
	//private bool boardStateDeclared;
	//private int[] blockingGrid;
	private int[,] scoreBoard;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

    private void Start() {
		turn = 0;
		_isPlayerTurn = true;
		//blockingGrid = new int[2] { -1,-1};
		scoreBoard = new int[3,3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
		winner = Winner.none;


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

	}

    public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	void Update() {

		if (!_isPlayerTurn && !aiFinished && !gameOver) {
			//Debug.Log("AI's Turn");

			//Opening Move for AI
			//Player moves on odd turns (turn =1, 3, 5, 7 9). AI moves on even turns (turn=2,4,6,8)
			/*if(turn==2 && !_isPlayerTurn && !aiFinished) {
				if (scoreBoard[1, 1] == -1) {
					int[] keyGrids = new int[2] { 0, 2 };
					int keyRow = keyGrids[UnityEngine.Random.Range(0, 2)];
					int keyCol = keyGrids[UnityEngine.Random.Range(0, 2)];
					AiSelects(keyRow, keyCol);
				} else {
					AiSelects(1, 1);
                }
            }*/


			//check board for potential winning move & block
			//if isWinningMove (true or false), get empty grid and AiSelects(emptyGridX, emptyGridY)

			/*if (turn>=4) {
				int winningRow;
				int winningCol;
				bool hasWinningMove = CheckIfWinningMove(scoreBoard, out winningRow, out winningCol);
				if (hasWinningMove && !gameOver) {
					Debug.Log("Winning Move Detected");
					Debug.Log("Winnng Row: " + winningRow + "  Winning Column: " + winningCol);
					AiSelects(winningRow, winningCol);
					//hasWinningMove = false;
				} else if (!hasWinningMove && !gameOver) {
					//else, use FindNextMove and MyMiniMaxMethod to determine next move

					ai_moveGrid = FindNextMove(boardState);
					//Debug.Log("Next Move is:  X-" + ai_moveGrid[0] + "  Y-" + ai_moveGrid[1]);
					//AiSelects(1, 1);
					AiSelects(ai_moveGrid[0], ai_moveGrid[1]);
				}
			}*/

			int winningRow;
			int winningCol;
			bool hasWinningMove = CheckIfWinningMove(scoreBoard, out winningRow, out winningCol);
			if (hasWinningMove && !gameOver) {
				Debug.Log("Winning Move Detected");
				Debug.Log("Winnng Row: " + winningRow + "  Winning Column: " + winningCol);
				AiSelects(winningRow, winningCol);
				//hasWinningMove = false;
			} else if(!hasWinningMove && !gameOver) {
				//else, use FindNextMove and MyMiniMaxMethod to determine next move

				ai_moveGrid = FindNextMove(boardState);
				//Debug.Log("Next Move is:  X-" + ai_moveGrid[0] + "  Y-" + ai_moveGrid[1]);
				//AiSelects(1, 1);
				AiSelects(ai_moveGrid[0], ai_moveGrid[1]);
			}
		}

		gameOver = CheckIfGameOver(scoreBoard, out winner);
		if (gameOver) {
			_isPlayerTurn = true;
			aiFinished = true;
			Debug.Log("Winner is: " + winner);
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
		if(boardState[coordX,coordY] == TicTacToeState.none && turn!=2 && !gameOver) {
			turn += 1;
			Debug.Log("Turn: " + turn);
            SetVisual(coordX, coordY, playerState);
			boardState[coordX, coordY] = TicTacToeState.circle;
			boardRep[coordX, coordY] = " o";
			scoreBoard[coordX, coordY] = -1;
			PrintBoard(boardRep);
			PrintScoreBoard(scoreBoard);

			//send false and AIDelay( ) sets _isPlayerTurn to false after 1.5 second delay
			StartCoroutine(AIDelay(false));
			//_isPlayerTurn = false;
			//turn += 1;
		}


	}

	public void AiSelects(int coordX, int coordY){

		//check that grid state is none (blank) before allowing player to select
		//this will prevent the player form placing more than one cirlce in a chosen grid
		if (boardState[coordX, coordY] == TicTacToeState.none && !gameOver) {
			turn += 1;
			Debug.Log("Turn: " + turn);
			//Debug.Log("Ai Selects X: " + coordX + "  Y: " + coordY);
			SetVisual(coordX, coordY, aiState);
			boardState[coordX, coordY] = TicTacToeState.cross;
			boardRep[coordX, coordY] = " x";
			scoreBoard[coordX, coordY] = 1;

			PrintBoard(boardRep);
			PrintScoreBoard(scoreBoard);

			//send true and AIDelay( ) sets _isPlayerTurn to true after 1.5 second delay
			StartCoroutine(AIDelay(true));
			//_isPlayerTurn = true;
			//turn += 1;
		}

	}

	private bool CheckIfGameOver(int[,] board, out Winner winner) {
		
		//check rows for either ai wins - xxx or for player wins - 000
		for (int row = 0; row < _gridSize; row++) {
			int sumRow = 0;
			
			for (int col = 0; col < _gridSize; col++) {
				sumRow += board[row, col];
			}

			if (sumRow == -3) {
				winner = Winner.player;
				return true;
			} else if(sumRow == 3) {
				winner = Winner.ai;
				return true;
            }
		}

		//check columns for either ai wins or player wins
		for (int col = 0; col < _gridSize; col++) {
			int sumCol = 0;

			for (int row = 0; row < _gridSize; row++) {
				sumCol += board[row, col];
			}

			if (sumCol == -3) {
				winner = Winner.player;
				return true;
			} else if (sumCol == 3) {
				winner = Winner.ai;
				return true;
			}
		}

		//check first diagonal (top left to lower right)
		int sumDiag1 = 0;

		for (int i = 0; i < _gridSize; i++) {
			sumDiag1 += board[i, i];
		}

		if (sumDiag1 == -3) {
			winner = Winner.player;
			return true;
		} else if(sumDiag1 == 3) {
			winner = Winner.ai;
			return true;
        }


		//check second diagonal (top right to lower left)
		int sumDiag2 = 0;

		for (int i = 0; i < _gridSize; i++) {
			sumDiag2 += board[i, 2 - i];
		}

		if (sumDiag2 == -3) {
			winner = Winner.player;
			return true;
		} else if(sumDiag2 == 3) {
			winner = Winner.ai;
			return true;
        }

		winner = Winner.none;
		return false;
	}

	private bool CheckIfWinningMove(int[,] board, out int winningRow, out int winningCol) {

		//check rows to see if player has 2 pieces and an open slot
		//return true if this is the case and store empty grid to winningRow and winningCol array variable
		for(int row=0; row<_gridSize; row++) {
			int sumRow = 0;
			int emptyCol = -1;

			for(int col=0; col<_gridSize; col++) {
				sumRow += board[row, col];

				if(board[row,col] == 0) {
					emptyCol = col;
                }
            }

			if(sumRow == 2 && emptyCol !=-1) {
				winningRow = row;
				winningCol = emptyCol;
				Debug.Log("Has winning move at X: " + winningRow + "  Y: " + winningCol);
				return true;
            }
        }

		//check cols to see if player has 2 pieces and an open slot
		//return true if this is the case and store empty grid to winningRow and winningCol array variable
		for (int col = 0; col < _gridSize; col++) {
			int sumCol = 0;
			int emptyRow = -1;

			for (int row = 0; row < _gridSize; row++) {
				sumCol += board[row, col];

				if (board[row, col] == 0) {
					emptyRow = row;
				}
			}

			if (sumCol == 2 && emptyRow != -1) {
				winningRow = emptyRow;
				winningCol = col;
				Debug.Log("Has winning move at X: " + winningRow + "  Y: " + winningCol);
				return true;
			}
		}

		//Check main diagonal (top left to bottom right) to see if player has 2 pieces and an open slot
		//return true if this is the case and store empty grid to emptyRowDiagonal1 and emptyColDiagonal1 array
		int sumDiag1 = 0;
		int emptyRowDiagonal1 = -1, emptyColDiagonal1 = -1;

		for(int i=0; i<_gridSize; i++) {
			sumDiag1 += board[i,i];

			if(board[i,i] == 0) {
				emptyRowDiagonal1 = i;
				emptyColDiagonal1 = i;
            }
        }

		if(sumDiag1 == 2 && emptyRowDiagonal1 != -1) {
			winningRow = emptyRowDiagonal1;
			winningCol = emptyColDiagonal1;
			Debug.Log("Has winning move at X: " + winningRow + "  Y: " + winningCol);
			return true;
        }

		//Check anti-diagonal (top right to bottom left) to see if player has 2 pieces and an open slot
		//return true if this is the case and store empty grid to emptyRowDiagonal2 and emptyColDiagonal2 array
		int sumDiag2 = 0;
		int emptyRowDiagonal2 = -1, emptyColDiagonal2 = -1;

		for (int i = 0; i < _gridSize; i++) {
			sumDiag2 += board[i, 2-i];

			if (board[i, 2-i] == 0) {
				emptyRowDiagonal2 = i;
				emptyColDiagonal2 = 2-i;
			}
		}

		if (sumDiag2 == 2 && emptyRowDiagonal2 != -1) {
			winningRow = emptyRowDiagonal2;
			winningCol = emptyColDiagonal2;
			Debug.Log("Has winning move at X: " + winningRow + "  Y: " + winningCol);
			return true;
		}

		//if not winning move found
		winningRow = -1;
		winningCol = -1;
		Debug.Log("Winning move not detected");
		return false;
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

	void PrintScoreBoard(int[,] board) {
		string boardString = "";

		for (int row = 0; row < 3; row++) {
			for (int col = 0; col < 3; col++) {

				boardString += board[row, col].ToString();
				if (col < 2) {
					boardString += " | ";
				}
			}
			boardString += "\n";

			if (row < 2) {
				//boardString += "---+---+---\n";
			}
		}
		Debug.Log(boardString);
	}

}
