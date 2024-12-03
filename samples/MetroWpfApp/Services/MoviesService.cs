using MovieWpfApp.Converters;
using MovieWpfApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieWpfApp.Services
{
    internal sealed class MoviesService
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new MovieModelBaseConverter(),
                new PersonModelConverter(),
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        public MoviesService(string sourceFilePath)
        {
            SourceFilePath = sourceFilePath ?? throw new ArgumentNullException(nameof(sourceFilePath));
            Debug.Assert(File.Exists(sourceFilePath));
        }

        #region Properties

        public List<PersonModel> Persons { get; } = [];

        private List<MovieModelBase>? PlainMovieList { get; set; }

        public string SourceFilePath { get; }

        #endregion

        #region Methods

        public ValueTask<bool> AddAsync(MovieModelBase model, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PlainMovieList!.Add(model);
            model.Parent?.Items.Add(model);
            return new ValueTask<bool>(true);
        }

        public ValueTask<bool> DeleteAsync(MovieModelBase model, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool result = PlainMovieList!.Remove(model);
            if (result)
            {
                model.Parent?.Items.Remove(model);
                if (model is MovieGroupModel group)
                {
                    group.Items.Clear();
                }
            }
            return new ValueTask<bool>(result);
        }

        public ValueTask<List<MovieModelBase>> GetMoviesAsync(CancellationToken cancellationToken)
        {
            static void LookupItems(ILookup<MovieGroupModel?, MovieModelBase> lookup, ICollection<MovieModelBase> items,
                MovieGroupModel? parent, HashSet<MovieModelBase> processedItems, bool isRoot = false)
            {
                var children = lookup[parent];
                foreach (var child in children)
                {
                    if (!processedItems.Add(child))
                    {
                        //Debug.Assert(false);
                        continue;
                    }
                    switch (child)
                    {
                        case MovieGroupModel g:
                            LookupItems(lookup, g.Items, g, processedItems);
                            break;
                    }
                    if (isRoot)
                    {
                        items.Add(child);
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            var rootGroup = new MovieGroupModel() { IsRoot = true, Name = Loc.Movies };
            var list = new List<MovieModelBase>() { rootGroup };

            var lookup = PlainMovieList!.ToLookup(x => x.Parent);
            var processedItems = new HashSet<MovieModelBase>();
            LookupItems(lookup, rootGroup.Items, null, processedItems, true);

            var lostItems = PlainMovieList!.Except(processedItems).ToList();
            if (lostItems.Count > 0)
            {
                foreach (var group in lostItems.OfType<MovieGroupModel>())
                {
                    LookupItems(lookup, group.Items, group, processedItems);
                }
                lostItems = lostItems.Except(processedItems).ToList();
                var lostGroup = new MovieGroupModel() { IsRoot = true, IsLost = true, Name = Loc.Lost };
                lostItems.ForEach(lostGroup.Items.Add);
                list.Add(lostGroup);
            }

            return new ValueTask<List<MovieModelBase>>(list);
        }

        public ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var movies = JsonSerializer.Deserialize<List<MovieModelBase>>(File.ReadAllText(SourceFilePath), _options);

            PlainMovieList = movies!.AsPlainList();
            foreach (var movie in PlainMovieList.OfType<MovieModel>())
            {
                AddUniquePersons(movie.Directors);
                AddUniquePersons(movie.Writers);
            }
            Persons.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

            //var s = JsonSerializer.Serialize(movies, options);
            //File.WriteAllText(SourceFilePath, s);

            return default;

            void AddUniquePersons(ObservableCollection<PersonModel> persons)
            {
                foreach (var person in persons)
                {
                    if (Persons.All(p => p.Name != person.Name))
                    {
                        Persons.Add(person);
                    }
                }
            }
        }

        public ValueTask<bool> SaveAsync(MovieModelBase original, MovieModelBase clone, CancellationToken cancellationToken)
        {
            _ = this;
            cancellationToken.ThrowIfCancellationRequested();
            original.UpdateFrom(clone);
            return new ValueTask<bool>(true);
        }

        #endregion
    }
}
