namespace Fall2024_Assignment3_jtadams5.Models
{
    public class Actor
    {
        public int ActorID { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string imdbURL { get; set; }
        public string photoURL { get; set; }

        public string? Reviews {get; set;}

        public ICollection<MovieActor>? MovieActors { get; set; }
    }
}
