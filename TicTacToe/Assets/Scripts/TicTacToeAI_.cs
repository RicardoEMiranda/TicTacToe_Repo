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
	private int turn;
	private bool aiFinished;

	[SerializeField] int aiX1, aiY1, aiX2, aiY2, aiX3, aiY3, aiX4, aiY4;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

    private void Start() {
		turn = 1;
		_isPlayerTurn = true;
		aiX1 = 0;
		aiY1 = 0;
		aiX2 = 1;
		aiY2 = 1;
		aiX3 = 2;
		aiY3 = 2;
		aiX4 = 2;
		aiY4 = 1;


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

		if (!_isPlayerTurn && !aiFinished) {
			if (turn == 2) {
				Debug.Log("Turn: " + turn);
				AiSelects(aiX1, aiY1);
			}

			if (turn == 4 && !aiFinished) {
				Debug.Log("Turn: " + turn);
				AiSelects(aiX2, aiY2);
			}

			if (turn == 6 && !aiFinished) {
				Debug.Log("Turn: " + turn);
				AiSelects(aiX3, aiY3);
			}

			if (turn == 8 && !aiFinished) {
				Debug.Log("Turn: " + turn);
				AiSelects(aiX4, aiY4);
			}
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

		SetVisual(coordX, coordY, playerState);
		boardState[coordX, coordY] = TicTacToeState.circle;
		boardRep[coordX, coordY] = " o";
		PrintBoard(boardRep);

		//send false and AIDelay( ) sets _isPlayerTurn to false after 1.5 second delay
		StartCoroutine(AIDelay(false));
		//_isPlayerTurn = false;
		//turn += 1;
		
	}

	public void AiSelects(int coordX, int coordY){

		SetVisual(coordX, coordY, aiState);
		boardState[coordX, coordY] = TicTacToeState.cross;
		boardRep[coordX, coordY] = " x";
		PrintBoard(boardRep);

		//send true and AIDelay( ) sets _isPlayerTurn to true after 1.5 second delay
		StartCoroutine(AIDelay(true));
		//_isPlayerTurn = true;
		//turn += 1;

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
