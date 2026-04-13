namespace GameJobNotifier.App.Models;

public sealed record JobChange(JobChangeType ChangeType, JobPosting Posting);
