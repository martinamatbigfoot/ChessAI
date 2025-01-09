using System.Collections;
using System.Collections.Generic;
using RookWorks.Gameplay;
using RookWorks.Visualization;
using UnityEngine;
using TMPro;

public class CommandTests : MonoBehaviour
{
    public BoardView View;
    public TMP_InputField InputField;
    public TextMeshProUGUI Text;
    public StockfishInterface Interface;
    public string Command;

    [ContextMenu("SendCommand")]
    public void SendCommand()
    {
        Interface.SendCommand(Command);
    }
}
