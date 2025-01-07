using System.Collections;
using System.Collections.Generic;
using RookWorks;
using UnityEngine;
using TMPro;

public class CommandTests : MonoBehaviour
{
    public TMP_InputField InputField;
    public TextMeshProUGUI Text;
    public StockfishInterface Interface;
    public string Command;

    [ContextMenu("SendCommand")]
    public void SendCommand()
    {
        Interface.SendCommand(Command);
    }

    private GameManager _manager;

    public void Start()
    {
        _manager = new GameManager();
        Text.text = _manager.GetStringBoard();
    }

    public void DoMove()
    {
        _manager.DoMove(InputField.text);
        Text.text = _manager.GetStringBoard();
    }

    public void SetPosition()
    {
        _manager.SetBoardPosition(InputField.text);
        Text.text = _manager.GetStringBoard();
    }
}
