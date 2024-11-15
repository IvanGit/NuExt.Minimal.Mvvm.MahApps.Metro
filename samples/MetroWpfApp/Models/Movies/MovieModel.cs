using Minimal.Mvvm;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace MetroWpfApp.Models
{
    public sealed partial class MovieModel : MovieModelBase
    {
        #region Properties

        [JsonIgnore]
        public override bool CanDrag => true;

        [JsonIgnore]
        public PersonModel? Director => Directors.FirstOrDefault();

        [JsonIgnore]
        public override bool IsEditable => true;

        [JsonPropertyOrder(0)]
        public override MovieKind Kind => MovieKind.Movie;

        [JsonPropertyOrder(2)]
        public ObservableCollection<PersonModel> Directors { get; set; } = new();

        [JsonPropertyOrder(3)]
        public ObservableCollection<PersonModel> Writers { get; set; } = new();

        [Notify, CustomAttribute("[global::System.Text.Json.Serialization.JsonPropertyOrder(4)]")]
        [CustomAttribute("[global::System.Text.Json.Serialization.JsonConverter(typeof(MetroWpfApp.Converters.JsonMovieReleaseDateConverter))]")]
        private DateTime _releaseDate;

        [Notify, CustomAttribute("[global::System.Text.Json.Serialization.JsonPropertyOrder(5)]")]
        private string _description = null!;

        [Notify, CustomAttribute("[global::System.Text.Json.Serialization.JsonPropertyOrder(6)]")]
        private string _storyline = null!;

        #endregion

        #region Methods

        public override MovieModelBase Clone()
        {
            var movie = new MovieModel() { Name = Name, ReleaseDate = ReleaseDate, Description = Description, Storyline = Storyline, Parent = Parent };
            Directors.ForEach(p => movie.Directors.Add(p.Clone()));
            Writers.ForEach(p => movie.Writers.Add(p.Clone()));
            return movie;
        }

        public override void UpdateFrom(MovieModelBase clone)
        {
            if (clone is not MovieModel movie)
            {
                throw new InvalidCastException();
            }

            Name = movie.Name;

            Directors.Clear();
            movie.Directors.ForEach(Directors.Add);

            Writers.Clear();
            movie.Writers.ForEach(Writers.Add);

            ReleaseDate = movie.ReleaseDate;
            Description = movie.Description;
            Storyline = movie.Storyline;

            RaisePropertyChanged(nameof(Director));
        }

        #endregion
    }
}
