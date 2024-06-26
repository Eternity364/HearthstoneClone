using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

// Class is mostly based on:
// https://github.com/Unity-Technologies/com.unity.services.samples.matchplay/blob/master/Assets/Scripts/Matchplay/Client/Services/MatchplayMatchmaker.cs

public enum MatchmakerPollingResult
{
    Success,
    TicketCreationError,
    TicketCancellationError,
    TicketRetrievalError,
    MatchAssignmentError
}

public class MatchmakingResult
{
    public string ip;
    public int port;
    public MatchmakerPollingResult result;
    public string resultMessage;
}

public class ClientMatchplayMatchmaker : IMatchmaker
{
    private string _lastUsedTicket;
    private bool _isMatchmaking = false;
    private CancellationTokenSource _cancelToken;
    private const int GetTicketCooldown = 1000;

    /// <summary>
    /// Create a ticket for the one user and begin matchmaking with their preferences
    /// </summary>
    /// <param name="userData">The Client's preferences and ID</param>
    public async Task<MatchmakingResult> FindMatch(UserData userData)
    {
        _cancelToken = new CancellationTokenSource();

        var createTicketOptions = UserDataToTicketRuleOptions(userData);
        var players = new List<Player> { new Player(userData.userAuthId) };

        try
        {
            _isMatchmaking = true;
            var createResult = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);

            _lastUsedTicket = createResult.Id;

            try
            {
                // Polling Loop
                while (!_cancelToken.IsCancellationRequested)
                {
                    var checkTicket = await MatchmakerService.Instance.GetTicketAsync(_lastUsedTicket);

                    if (checkTicket.Type == typeof(MultiplayAssignment))
                    {
                        var matchAssignment = (MultiplayAssignment)checkTicket.Value;

                        Debug.Log($"Polled Ticket: {_lastUsedTicket} Status: {matchAssignment.Status} ");

                        if (matchAssignment.Status == MultiplayAssignment.StatusOptions.Found)
                        {
                            return ReturnMatchResult(MatchmakerPollingResult.Success, "", matchAssignment);
                        }

                        if (matchAssignment.Status == MultiplayAssignment.StatusOptions.Timeout ||
                            matchAssignment.Status == MultiplayAssignment.StatusOptions.Failed)
                        {
                            return ReturnMatchResult(MatchmakerPollingResult.MatchAssignmentError,
                                $"Ticket: {_lastUsedTicket} - {matchAssignment.Status} - {matchAssignment.Message}");
                        }
                    }

                    await Task.Delay(GetTicketCooldown);
                }
            }
            catch (MatchmakerServiceException e)
            {
                return ReturnMatchResult(MatchmakerPollingResult.TicketRetrievalError, e.ToString());
            }
        }
        catch (MatchmakerServiceException e)
        {
            return ReturnMatchResult(MatchmakerPollingResult.TicketCreationError, e.ToString());
        }

        return ReturnMatchResult(MatchmakerPollingResult.TicketCancellationError, "Cancelled Matchmaking");
    }

    // Make sure we exit the matchmaking cycle through this method every time.
    MatchmakingResult ReturnMatchResult(MatchmakerPollingResult resultErrorType, string message = "",
        MultiplayAssignment assignment = null)
    {
        _isMatchmaking = false;

        if (assignment != null)
        {
            var parsedIP = assignment.Ip;
            var parsedPort = assignment.Port;

            if (parsedPort == null)
            {
                return new MatchmakingResult
                {
                    result = MatchmakerPollingResult.MatchAssignmentError,
                    resultMessage = $"Port missing? - {assignment.Port}\n-{assignment.Message}"
                };
            }

            return new MatchmakingResult
            {
                result = MatchmakerPollingResult.Success,
                ip = parsedIP,
                port = (int)parsedPort,
                resultMessage = assignment.Message
            };
        }

        return new MatchmakingResult
        {
            result = resultErrorType,
            resultMessage = message
        };
    }

    /// <summary>
    /// From Game player to matchmaking player
    /// </summary>
    static CreateTicketOptions UserDataToTicketRuleOptions(UserData data)
    {
        var attributes = new Dictionary<string, object>
        {
            // You can add attributes here to match players with others with same preferences, like game mode, map, etc
            // { k_MapRuleName, data.userGamePreferences.MapRules() }
        };

        // If you wanted to use a non-standard queue, say for a special event, you could spin up a new fleet that uses
        // a new build configuration that passes in the queue as a launch param, that overwrites the default here.
        return new CreateTicketOptions("Test", attributes);
    }

    public async Task CancelMatchmaking()
    {
        if (!_isMatchmaking)
        {
            return;
        }

        _isMatchmaking = false;
        if (_cancelToken.Token.CanBeCanceled)
        {
            _cancelToken.Cancel();
        }

        if (string.IsNullOrEmpty(_lastUsedTicket))
        {
            return;
        }

        Debug.Log($"Cancelling {_lastUsedTicket}");
        await MatchmakerService.Instance.DeleteTicketAsync(_lastUsedTicket);
    }

    public void Dispose()
    {
        Debug.Log("Dispose");
#pragma warning disable 4014
        CancelMatchmaking();
#pragma warning restore 4014
        _cancelToken?.Dispose();
    }
}
