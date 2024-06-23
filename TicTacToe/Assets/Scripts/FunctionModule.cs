using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionModule : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	//checks the state of each row, column and diagonal (RCD)
	//if the sum for any of the RCD's equal the determinant parameter,
	////returns true
	//EX: if determinant passed in = 2, would check if AI has 2 crosses on any of the RCDs
	//    if so, returns true
	//EX: if determinant passed in = 3, would check if AI has 3 crosses on any of the RCDs
	public bool CheckRCDState(int[,] board, int determinant) {

		//sum each row
		int sumRow1 = board[0, 0] + board[0, 1] + board[0, 2];
		int sumRow2 = board[1, 0] + board[1, 1] + board[1, 2];
		int sumRow3 = board[2, 0] + board[2, 1] + board[2, 2];

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
		if (sumDiag1 == determinant || sumDiag2 == determinant) {
			//Debug.Log("Sum of Diag is 2, AI should move to win");
			return true;
		}

		return false;
	}
}
