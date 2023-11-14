using Chirp.Models;
using Chirp.CDTO;
namespace Chirp.Interfaces;

public interface ICheepRepository
{
    public int CheepsPerPage();
    public Task CreateCheep(CheepDTO cheepDTO);
    public Task<IEnumerable<Cheep>> GetCheeps(int page);
    public Task<IEnumerable<Cheep>> GetCheepsFromAuthor(string author, int page);
    public Task<IEnumerable<Cheep>> GetAllCheeps();
    public Task<IEnumerable<Cheep>> GetAllCheepsFromAuthor(string author);
}