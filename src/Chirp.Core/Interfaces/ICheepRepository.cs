using Chirp.Models;
namespace Chirp.Interfaces;

public interface ICheepRepository
{
    public int CheepsPerPage();
    public void CreateCheep(Cheep cheep);
    public Task<IEnumerable<Cheep>> GetCheeps(int page);
    public Task<IEnumerable<Cheep>> GetCheepsFromAuthor(string author, int page);
    public Task<IEnumerable<Cheep>> GetAllCheeps();
    public Task<IEnumerable<Cheep>> GetAllCheepsFromAuthor(string author);
}