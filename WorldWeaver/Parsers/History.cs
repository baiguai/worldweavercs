using System;
using System.Collections.Generic;

public class CommandHistoryManager
{
    private string _commandHistory = "";

    /// <summary>
    /// Adds a command to the history.
    /// </summary>
    /// <param name="command">The command to add.</param>
    public void AddCommand(string command)
    {
        if (!string.IsNullOrWhiteSpace(command))
        {
            _commandHistory = command;
        }
    }

    /// <summary>
    /// Handles user input and navigates the command history.
    /// </summary>
    /// <returns>The entered command string.</returns>
    public string HandleInput(string inputString)
    {
        switch (inputString)
        {
            case "^":
                if (!_commandHistory.Equals(""))
                {
                    return _commandHistory;
                }
                else
                {
                    return inputString;
                }
                break;

            default:
                _commandHistory = inputString;
                return inputString;
        }

        return inputString;
    }
}