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

	//My Variables
	private int[,] gridValues = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
	private int[] sumOfEachRow = { 0, 0, 0 };
	private int[] sumOfEachCol = { 0, 0, 0 };
	private int[] sumOfDiag = { 0, 0 };

	private int[] rowSums = new int[3];
	private int[] colSums = new int[3];
	private int[] diagSums = new int[2];
	private int row, col;
	private int turn = 0;
	private bool isWinningMove;
	private bool aiDelayFinished;
	private bool isAIDelaying;
	private bool gameOver;

	[SerializeField] private int turn2x, turn2y, turn4x, turn4y, turn6x, turn6y;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

    private void Start() {
		turn += 1;
    }

    private void Update() {
        if(!_isPlayerTurn) {

				if (turn == 2) {
					//This assumes AI will always go second
					//if a corner has been taken (0,0)(0,2)(2,0) or (2,2), take the center grid
					if (gridValues[0, 0] == -1 || gridValues[0, 2] == -1 || gridValues[2, 0] == -1 || gridValues[2, 2] == -1) {
						//Debug.Log("Corner grid has been taken on first move. AI Takes center grid");
						AiSelects(1, 1);
					} else if (gridValues[1, 1] == -1) {
						//Debug.Log("Player took center position on first move. Take a random corner grid.");

						//possible corner grids are at (0,0), (0,2), (2,0) or (2,2)
						//row value is either 0 or 2, col value is either 0 or 2
						int[] cornerGrids = new int[2] { 0, 2 };

						//sets cornerGrids[0]=0 or cornerGrids[1]=2 for the corner row
						int randomCornerRow = cornerGrids[UnityEngine.Random.Range(0, 2)];
						int randomCornerCol = cornerGrids[UnityEngine.Random.Range(0, 2)];

						AiSelects(randomCornerRow, randomCornerCol);
					} else {
						//Debug.Log("Player selected other than center or corner grid on opening move");
						//Player has positioned his cirlce in one of the outside center grids (1,3, 5 or 7)
						//AI picks a random corner grid for best outcome
						//possible corner grids are at (0,0), (0,2), (2,0) or (2,2)
						//row value is either 0 or 2, col value is either 0 or 2
						int[] cornerGrids = new int[2] { 0, 2 };

						//sets cornerGrids[0]=0 or cornerGrids[1]=2 for the corner row
						int randomCornerRow = cornerGrids[UnityEngine.Random.Range(0, 2)];
						int randomCornerCol = cornerGrids[UnityEngine.Random.Range(0, 2)];

						AiSelects(randomCornerRow, randomCornerCol);
					}
				}

				if (turn == 4 || turn == 6 || turn ==8) {
					//AiSelects(turn4x,turn4y);
					isWinningMove = CheckIfWinningMove();
					//Debug.Log("Is Winning Move? " + isWinningMove);
					if (isWinningMove) {
						int[] emptyGridCoordinates = MakeWinningMove(-20);
						AiSelects(emptyGridCoordinates[0], emptyGridCoordinates[1]);

						//Since this is a winning move, check 
						//Check winning row, column or diagonal equals -30 or -3
						int[] winState = CheckWinState();
						if(winState[0] == 1) {
							Debug.Log("Game Over");
						}

					}

					if (!isWinningMove) {

						int[] emptyGridCoordinates = MakeWinningMove(-2);
						if (gridValues[emptyGridCoordinates[0], emptyGridCoordinates[1]] != -10) {

							AiSelects(emptyGridCoordinates[0], emptyGridCoordinates[1]);
							//Debug.Log("Grid Coordinates X: " + emptyGridCoordinates[0] + "Grid Coordinates Y: " + emptyGridCoordinates[1]);
						} else {
							int[] nextCoordinates = MakeWinningMove(-10);
							AiSelects(nextCoordinates[0], nextCoordinates[1]);
						}

					}

			    }
		}
    }

    public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
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

		//checks that grid value is 0, therefore no cross or cirlce prefab instantiated
		//already in this grid before carrying out SetVisual method
		if (gridValues[coordX, coordY] == 0) {
			turn += 1;
			Debug.Log("Turn: " + turn);
			SetVisual(coordX, coordY, playerState);
			UpdateGridValues(coordX, coordY, playerState);
			rowSums = CheckRowSum(gridValues);
			colSums = CheckColSum(gridValues);
			diagSums = CheckDiagSum(gridValues);

			StartCoroutine(AIDelay(false));
			//_isPlayerTurn = false;

			Debug.Log("Row1 Sum: " + rowSums[0] + "  Row2 Sums: " + rowSums[1] + " Row3 Sums: " + rowSums[2]);
			Debug.Log("Col1 Sum: " + colSums[0] + "  Col2 Sums: " + colSums[1] + " Col3 Sums: " + colSums[2]);
			Debug.Log("Diag1 Sum: " + diagSums[0] + "  Diag2 Sum: " + diagSums[1]);
		}
		
	}

	public void AiSelects(int coordX, int coordY){
		
		//checks that grid value is 0, therefore no cross or cirlce prefab instantiated
		//already in this grid before carrying out SetVisual method
		if(gridValues[coordX, coordY] == 0) {
			turn += 1;
			Debug.Log("Turn: " + turn);
			SetVisual(coordX, coordY, aiState);
			UpdateGridValues(coordX, coordY, aiState);
			rowSums = CheckRowSum(gridValues);
			colSums = CheckColSum(gridValues);
			diagSums = CheckDiagSum(gridValues);

			StartCoroutine(AIDelay(true));
			//_isPlayerTurn = true;

			Debug.Log("Row1 Sum: " + rowSums[0] + "  Row2 Sums: " + rowSums[1] + " Row3 Sums: " + rowSums[2]);
			Debug.Log("Col1 Sum: " + colSums[0] + "  Col2 Sums: " + colSums[1] + " Col3 Sums: " + colSums[2]);
			Debug.Log("Diag1 Sum: " + diagSums[0] + "  Diag2 Sum: " + diagSums[1]);
		}
	
	}

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState) {
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}

	private IEnumerator AIDelay(bool condition) {
		//isAIDelaying = true;
		yield return new WaitForSeconds(1.5f);
		//aiDelayFinished = true;
		//isAIDelaying = false;
		_isPlayerTurn = condition;
	}

	private void UpdateGridValues(int row, int col, TicTacToeState state) {
		if(state == TicTacToeState.cross) {
			gridValues[row, col] = -10;
        } else if(state == TicTacToeState.circle) {
			gridValues[row, col] = -1;
        }
    }

	private int[] CheckRowSum(int[, ] values) {
		int[] rowSum = { 0, 0, 0 };
		for(int row=0; row<3; row++) {
			for(int col=0; col<3; col++) {
				rowSum[row] += values[row, col];
            }
        }
		return rowSum;
    }

	private int[] CheckColSum(int[,] values) {
		int[] colSum = { 0, 0, 0 };
		for(int col= 0; col<3; col++) {
			for(int row=0; row<3; row++) {
				colSum[col] += values[row, col];
            }
        }
		return colSum;
    }

	private int[] CheckDiagSum(int[,] values) {
		int[] diagSum = { 0, 0 };
		diagSum[0] = values[2, 0] + values[1, 1] + values[0, 2];
		diagSum[1] = values[0, 0] + values[1, 1] + values[2, 2];
		return diagSum;
    }

	private int FindEmptyGrid(int[] vals) {
		int result = -1;
		for(int i =0; i<3; i++) {
			if(vals[i] == 0) {
				result = i;
				break;
            }
        }
		return result;
    }

	private int[] ReturnRowArrayValues(int[,] gridValues, int row) {
		int[] array = new int[3];
		for(int col = 0; col<3; col++) {
			array[col] = gridValues[row, col];
        }
		return array;
    }

	private int[] ReturnColArrayValues(int[, ] gridValues, int col) {
		int[] array = new int[3];
		for (int row = 0; row < 3; row++) {
			array[row] = gridValues[row, col];
		}
		return array;
	}

	private int[] ReturnBTDiagonalArrayValues(int[,] gridValues) {
		int[] array = new int[3];
		array = new int[3] { gridValues[2, 0], gridValues[1, 1], gridValues[0, 2]};

		return array;
    }

	private int[] ReturnTBDiagonalArrayValues(int[,] gridValues) {
		int[] array = new int[3];
		array = new int[3] { gridValues[0, 0], gridValues[1, 1], gridValues[2, 2] };

		return array;
	}

	private bool CheckIfWinningMove() {
		for(int i=0; i<3; i++) {
			if(colSums[i] == -20 || rowSums[i] == -20) {
				return true; 
            } 
        }

		if(diagSums[0] == -20 || diagSums[1] == -20) {
			return true;
        }

		return false;
    }

	private int[] MakeWinningMove(int moveDeterminant) {


		if (!gameOver) {
			//check sum of each row. If -20, find row and column with the empty grid
			//and send coordinates to AI for blocking
			if (rowSums[0] == moveDeterminant) {

				row = 0;
				int[] array = ReturnRowArrayValues(gridValues, 0);
				col = FindEmptyGrid(array);
			}

			if (rowSums[1] == moveDeterminant) {
				row = 1;
				int[] array = ReturnRowArrayValues(gridValues, 1);
				col = FindEmptyGrid(array);
			}

			if (rowSums[2] == moveDeterminant) {
				row = 2;
				int[] array = ReturnRowArrayValues(gridValues, 2);
				col = FindEmptyGrid(array);
			}

			//check sum of each column. If -20, find row and column with the empty grid
			//and send coordinates to AI for blocking
			if (colSums[0] == moveDeterminant) {
				col = 0;
				int[] array = ReturnColArrayValues(gridValues, 0);
				row = FindEmptyGrid(array);
			}

			if (colSums[1] == moveDeterminant) {
				col = 1;
				int[] array = ReturnColArrayValues(gridValues, 1);
				row = FindEmptyGrid(array);
			}

			if (colSums[2] == moveDeterminant) {
				col = 2;
				int[] array = ReturnColArrayValues(gridValues, 2);
				row = FindEmptyGrid(array);
			}

			//check sum of diagonal going from bottom to top, if -20 
			//find row and column with the empty grid and send coordinates to AI for blocking
			if (diagSums[0] == moveDeterminant) {
				int[] array = ReturnBTDiagonalArrayValues(gridValues);
				if (array[0] == 0) {
					row = 2;
					col = 0;
				} else if (array[1] == 0) {
					row = 1;
					col = 1;
				} else if (array[2] == 0) {
					row = 0;
					col = 2;
				} else {
					Debug.Log("Diagonal Array exception");
				}
			}

			//check sum of diagonal going from top to bottom , if -20 
			//find row and column with the empty grid and send coordinates to AI for blocking
			if (diagSums[1] == moveDeterminant) {
				int[] array = ReturnTBDiagonalArrayValues(gridValues);
				if (array[0] == 0) {
					row = 0;
					col = 0;
				} else if (array[1] == 0) {
					row = 1;
					col = 1;
				} else if (array[2] == 0) {
					row = 2;
					col = 2;
				} else {
					Debug.Log("Diagonal Array exception");
				}
			}
			
		}
		int[] emptyGridCoordinates = new int[2] { row, col };
		return emptyGridCoordinates;

	}

	private int[] CheckWinState() {
		int gameOverState = 0;
		int playerWinState = -1;
		int[] result = new int[2] { gameOverState, playerWinState };


		if(rowSums[0] == -30 || rowSums[1] == -30 || rowSums[2] == -30) {
			gameOverState = 1;
			playerWinState = 1;
			result[0] = gameOverState;
			result[1] = playerWinState;
        } else if(rowSums[0] == -3 || rowSums[1] == -3 || rowSums[2] == -3) {
			gameOverState = 1;
			playerWinState = 0;
			result[0] = gameOverState;
			result[1] = playerWinState;
		}

		if(colSums[0] == -30 || colSums[1] == -30 || colSums[2] == -30) {
			gameOverState = 1;
			playerWinState = 1;
			result[0] = gameOverState;
			result[1] = playerWinState;
		} else if (rowSums[0] == -3 || rowSums[1] == -3 || rowSums[2] == -3) {
			gameOverState = 1;
			playerWinState = 0;
			result[0] = gameOverState;
			result[1] = playerWinState;
		}

		if (diagSums[0] == -30 || diagSums[1] == -30) {
			gameOverState = 1;
			playerWinState = 1;
			result[0] = gameOverState;
			result[1] = playerWinState;
		} else if (rowSums[0] == -3 || rowSums[1] == -3 || rowSums[2] == -3) {
			gameOverState = 1;
			playerWinState = 0;
			result[0] = gameOverState;
			result[1] = playerWinState;
		}

		//gameOverState (0 - false, game not over), (1-true, game Over)
		//0 player wins, 1 AI wins
		//return {1,1} means 1-true (game over) and 1-AI wins
		return result;
    }
}
