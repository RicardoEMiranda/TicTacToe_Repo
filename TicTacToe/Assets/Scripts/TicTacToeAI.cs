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
			if(turn == 2) {
				AiSelects(turn2x, turn2y);
            }

			if(turn ==4) {
				AiSelects(turn4x,turn4y);
            }

			if(turn == 6) {

				isWinningMove = CheckIfWinningMove();
				Debug.Log("Is Winning Move? " + isWinningMove);

				if(isWinningMove) {
					int[] emptyGridCoordinates = MakeWinningMove();
					AiSelects(emptyGridCoordinates[0], emptyGridCoordinates[1]);
                }

				//check sum of each row. If -20, find row and column with the empty grid
				//and send coordinates to AI for blocking
				/*if(rowSums[0] == -20) {
					row = 0;
					int[] array = ReturnRowArrayValues(gridValues, 0);
					col = FindGridForWin(array);
                } 

				if(rowSums[1] == -20) {
					row = 1;
					int[] array = ReturnRowArrayValues(gridValues, 1);
					col = FindGridForWin(array);
				}

				if(rowSums[2] == -20) {
					row = 2;
					int[] array = ReturnRowArrayValues(gridValues, 2);
					col = FindGridForWin(array);
				}

				//check sum of each column. If -20, find row and column with the empty grid
				//and send coordinates to AI for blocking
				if (colSums[0] == -20) {
					col = 0;
					int[] array = ReturnColArrayValues(gridValues, 0);
					row = FindGridForWin(array);
				}

				if (colSums[1] == -20) {
					col = 1;
					int[] array = ReturnColArrayValues(gridValues, 1);
					row = FindGridForWin(array);
				}

				if (colSums[2] == -20) {
					col = 2;
					int[] array = ReturnColArrayValues(gridValues, 2);
					row = FindGridForWin(array);
				}

				//check sum of diagonal going from bottom to top, if -20 
				//find row and column with the empty grid and send coordinates to AI for blocking
				if (diagSums[0] == -20) {
					int[] array = ReturnBTDiagonalArrayValues(gridValues);
					if(array[0] == 0) {
						row = 2;
						col = 0;
                    } else if(array[1] == 0) {
						row = 1;
						col = 1;
                    } else if(array[2] == 0) {
						row = 0;
						col = 2;
                    } else {
						Debug.Log("Diagonal Array exception");
                    }
                }

				//check sum of diagonal going from top to bottom , if -20 
				//find row and column with the empty grid and send coordinates to AI for blocking
				if (diagSums[1] == -20) {
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

				int[] emptyGrid = new int[2] { row, col };
				AiSelects(row, col);*/
			
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
		
		turn += 1;
		Debug.Log("Turn: " + turn);
		SetVisual(coordX, coordY, playerState);
		UpdateGridValues(coordX, coordY, playerState);
		rowSums = CheckRowSum(gridValues);
		colSums = CheckColSum(gridValues);
		diagSums = CheckDiagSum(gridValues);
		_isPlayerTurn = false;

		Debug.Log("Row1 Sum: " + rowSums[0] + "  Row2 Sums: " + rowSums[1] + " Row3 Sums: " + rowSums[2]);
		Debug.Log("Col1 Sum: " + colSums[0] + "  Col2 Sums: " + colSums[1] + " Col3 Sums: " + colSums[2]);
		Debug.Log("Diag1 Sum: " + diagSums[0] + "  Diag2 Sum: " + diagSums[1]);
	}

	public void AiSelects(int coordX, int coordY){

		turn += 1;
		Debug.Log("Turn: " + turn);
		SetVisual(coordX, coordY, aiState);
		UpdateGridValues(coordX, coordY, aiState);
		rowSums = CheckRowSum(gridValues);
		colSums = CheckColSum(gridValues);
		diagSums = CheckDiagSum(gridValues);
		_isPlayerTurn = true;

		Debug.Log("Row1 Sum: " + rowSums[0] + "  Row2 Sums: " + rowSums[1] + " Row3 Sums: " + rowSums[2]);
		Debug.Log("Col1 Sum: " + colSums[0] + "  Col2 Sums: " + colSums[1] + " Col3 Sums: " + colSums[2]);
		Debug.Log("Diag1 Sum: " + diagSums[0] + "  Diag2 Sum: " + diagSums[1]);
	}

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
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

	private int FindGridForWin(int[] vals) {
		int result = -1;
		for(int i =0; i<3; i++) {
			if(vals[i] == 0) {
				result = i;
				break;
            }
        }
		return result;
    }

	private int FindDiagGridForWin(int[] array) {
		int result = -1;
		for(int i=0; i<3; i++) {
			if(array[i] == 0) {
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

	private int[] MakeWinningMove() {
		//check sum of each row. If -20, find row and column with the empty grid
		//and send coordinates to AI for blocking
		if (rowSums[0] == -20) {
			row = 0;
			int[] array = ReturnRowArrayValues(gridValues, 0);
			col = FindGridForWin(array);
		}

		if (rowSums[1] == -20) {
			row = 1;
			int[] array = ReturnRowArrayValues(gridValues, 1);
			col = FindGridForWin(array);
		}

		if (rowSums[2] == -20) {
			row = 2;
			int[] array = ReturnRowArrayValues(gridValues, 2);
			col = FindGridForWin(array);
		}

		//check sum of each column. If -20, find row and column with the empty grid
		//and send coordinates to AI for blocking
		if (colSums[0] == -20) {
			col = 0;
			int[] array = ReturnColArrayValues(gridValues, 0);
			row = FindGridForWin(array);
		}

		if (colSums[1] == -20) {
			col = 1;
			int[] array = ReturnColArrayValues(gridValues, 1);
			row = FindGridForWin(array);
		}

		if (colSums[2] == -20) {
			col = 2;
			int[] array = ReturnColArrayValues(gridValues, 2);
			row = FindGridForWin(array);
		}

		//check sum of diagonal going from bottom to top, if -20 
		//find row and column with the empty grid and send coordinates to AI for blocking
		if (diagSums[0] == -20) {
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
		if (diagSums[1] == -20) {
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

		int[] emptyGridCoordinates = new int[2] { row, col };
		return emptyGridCoordinates;
	}
}
