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

        public virtual ICollection<Actor> Actors { get; set; } = new List<Actor>();
    }
}
