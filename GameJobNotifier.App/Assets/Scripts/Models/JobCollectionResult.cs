namespace GameJobNotifier.App.Models;

public sealed record JobCollectionResult(
    IReadOnlyList<JobPosting> Postings,
    string CollectorName);
