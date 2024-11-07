using MetroWpfApp.Converters;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace MetroWpfApp.Models
{
    public sealed class MovieModel : MovieModelBase
    {
        #region Properties

        [JsonIgnore]
        public override bool CanDrag => true;

        private string _description = null!;
        [JsonPropertyOrder(5)]
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        [JsonIgnore] public PersonModel? Director => Directors.FirstOrDefault();

        [JsonPropertyOrder(2)]
        public ObservableCollection<PersonModel> Directors { get; set; } = new();

        [JsonIgnore]
        public override bool IsEditable => true;

        [JsonPropertyOrder(0)]
        public override MovieKind Kind => MovieKind.Movie;

        private DateTime _releaseDate;
        [JsonPropertyOrder(4)]
        [JsonConverter(typeof(JsonMovieReleaseDateConverter))]
        public DateTime ReleaseDate
        {
            get => _releaseDate;
            set => SetProperty(ref _releaseDate, value);
        }

        private string _storyline = null!;
        [JsonPropertyOrder(6)]
        public string Storyline
        {
            get => _storyline;
            set => SetProperty(ref _storyline, value);
        }

        [JsonPropertyOrder(3)]
        public ObservableCollection<PersonModel> Writers { get; set; } = new();

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
