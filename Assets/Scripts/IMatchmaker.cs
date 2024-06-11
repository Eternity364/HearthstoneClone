using System;
using System.Threading.Tasks;

public interface IMatchmaker : IDisposable
{
    public Task<MatchmakingResult> FindMatch(UserData userData);
}
