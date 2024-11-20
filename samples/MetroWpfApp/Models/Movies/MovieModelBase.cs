using Minimal.Mvvm;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows;

namespace MetroWpfApp.Models
{
    [DebuggerDisplay("Name={Name}")]
    public abstract partial class MovieModelBase: BindableBase, ICloneable<MovieModelBase>, IDragDrop
    {
        #region Properties

        [JsonIgnore]
        public abstract bool CanDrag { get; }

        [JsonIgnore]
        public abstract bool IsEditable { get; }

        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonIgnore")]
        private bool _isExpanded;

        [JsonPropertyOrder(0)]
        public abstract MovieKind Kind { get; }

        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonPropertyOrder(1)")]
        private string _name = null!;

        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonIgnore")]
        private MovieGroupModel? _parent;

        #endregion

        #region Methods

        public bool CanDrop(IDragDrop draggedObject)
        {
            if (draggedObject is not MovieModelBase model) return false;
            return OnCanDrop(model);
        }

        public bool Drop(IDragDrop draggedObject)
        {
            if (draggedObject is not MovieModelBase model) return false;
            return OnCanDrop(model) && OnDrop(model);
        }

        public abstract MovieModelBase Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public string GetPath()
        {
            var path = $"\\{Name}";
            var parent = Parent;
            while (parent != null)
            {
                path = $"\\{parent.Name}" + path;
                parent = parent.Parent;
            }
            return path;
        }

        protected virtual bool OnCanDrop(MovieModelBase model)
        {
            return false;
        }

        protected virtual bool OnDrop(MovieModelBase model)
        {
            return false;
        }

        public abstract void UpdateFrom(MovieModelBase clone);

        #endregion
    }

    public enum MovieKind
    {
        Group,
        Movie
    }

    public static class MovieModelBaseExtensions
    {
        public static List<MovieModelBase> AsPlainList(this IEnumerable<MovieModelBase> items)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(items);
#else
            Throw.IfNull(items);
#endif
            var list = new List<MovieModelBase>();
            ProcessList(list, items);
            return list;

            static void ProcessList(ICollection<MovieModelBase> list, IEnumerable<MovieModelBase> items)
            {
                foreach (var item in items)
                {
                    list.Add(item);
                    if (item is MovieGroupModel groupModel)
                    {
                        ProcessList(list, groupModel.Items);
                    }
                }
            }
        }

        public static MovieModelBase? FindByPath(this ICollection<MovieModelBase> items, string? path)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(items);
#else
            Throw.IfNull(items);
#endif
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            foreach (var item in items)
            {
                if (item.GetPath() == path)
                {
                    return item;
                }
                switch (item)
                {
                    case MovieGroupModel g:
                        var x = g.Items.FindByPath(path);
                        if (x != null)
                        {
                            return x;
                        }
                        break;
                }
            }
            return null;
        }
    }
}
