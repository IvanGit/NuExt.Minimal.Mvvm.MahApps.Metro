using Minimal.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace MovieWpfApp.Models
{
    public sealed partial class MovieModel : MovieModelBase, IDataErrorInfo
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
        public ObservableCollection<PersonModel> Directors { get; set; } = [];

        [JsonPropertyOrder(3)]
        public ObservableCollection<PersonModel> Writers { get; set; } = [];

        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonPropertyOrder(4)")]
        [CustomAttribute("global::System.Text.Json.Serialization.JsonConverter(typeof(MovieWpfApp.Converters.JsonMovieReleaseDateConverter))")]
        private DateTime _releaseDate;

        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonPropertyOrder(5)")]
        private string _description = null!;

        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonPropertyOrder(6)")]
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

        #region IDataErrorInfo

        private static readonly string[] s_validatableProperties = [nameof(Name), nameof(ReleaseDate)];

        public string Error
        {
            get
            {
                IDataErrorInfo dataErrorInfo = this;
                var sb = new ValueStringBuilder();
                var separator = string.Empty;
                foreach (var property in s_validatableProperties)
                {
                    var error = dataErrorInfo[property];
                    if (string.IsNullOrEmpty(error)) continue;
                    sb.Append(separator);
                    sb.Append(error);
                    separator = Environment.NewLine;
                }
                return sb.ToString();
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var sb = new ValueStringBuilder();
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                        {
                            sb.Append(string.Format(Loc.Arg0_cannot_be_null_or_empty, Loc.Name));
                        }
                        break;
                    case nameof(ReleaseDate):
                        if (ReleaseDate < new DateTime(1895, 12, 25))
                        {
                            sb.Append(string.Format(Loc.Arg0_should_be_specified, Loc.Release_Date));
                        }
                        break;
                }
                return sb.ToString();
            }
        }

        #endregion
    }
}
