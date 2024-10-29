namespace Fall2024_Assignment3_jtadams5.Models
{
    public class Movie
    {
        public int MovieID { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int ReleaseYear { get; set; }
        public string PosterURL { get; set; }
        public string imdbURL { get; set; }

        // Try adding a short plot summary
        public string? PlotSummary { get; set; }

        public string? Reviews { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }
    }
}
