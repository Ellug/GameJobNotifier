using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface IGameJobHtmlParser
{
    IReadOnlyList<JobPosting> Parse(string html, Uri baseUri);
}
