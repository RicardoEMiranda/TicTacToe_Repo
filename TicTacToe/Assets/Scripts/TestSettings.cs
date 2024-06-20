using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSettings : MonoBehaviour {

    [SerializeField] private GameObject GamePanel;
    [SerializeField] private GameObject StartingPanel;
    [SerializeField] private GameObject TicTacToeAI;

    //private TicTacToeAI ticTacToeAI;

    [SerializeField] private bool TestSettingsActive;

    // Start is called before the first frame update
    void Start()   {
        
        if(TestSettingsActive) {
            GamePanel.SetActive(true);
            //StartingPanel.SetActive(false);
            //ticTacToeAI = TicTacToeAI.GetComponent<TicTacToeAI>();
            //ticTacToeAI.StartAI(0);
        }

    }

    //public void OnResetClick() {
    //   ticTacToeAI.StartAI(0);
    //}
}
