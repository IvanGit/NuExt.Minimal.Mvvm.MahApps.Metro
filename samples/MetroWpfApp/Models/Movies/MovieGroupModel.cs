using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace MetroWpfApp.Models
{
    public sealed class MovieGroupModel : MovieModelBase
    {
        #region Properties

        [JsonIgnore]
        public override bool CanDrag => !IsRoot;

        [JsonIgnore]
        public override bool IsEditable => !IsRoot;

        [JsonIgnore]
        public bool IsLost { get; init; }

        [JsonIgnore]
        public bool IsRoot { get; init; }

        [JsonPropertyOrder(2)]
        public ObservableCollection<MovieModelBase> Items { get; set; } = [];

        [JsonPropertyOrder(0)]
        public override MovieKind Kind => MovieKind.Group;

        #endregion

        #region Methods

        public override MovieModelBase Clone()
        {
            return new MovieGroupModel() { Name = Name, Parent = Parent };
        }

        public void CollapseAll()
        {
            Collapse();
            Items.OfType<MovieGroupModel>().ForEach(item => item.CollapseAll());
        }

        public override void Expand()
        {
            Parent?.Expand();
            base.Expand();
        }

        public void ExpandAll()
        {
            Expand();
            Items.OfType<MovieGroupModel>().ForEach(item => item.ExpandAll());
        }

        protected override bool OnCanDrop(MovieModelBase model)
        {
            if (IsLost)
            {
                return false;
            }
            if (model == this || model.Parent == this || model.Parent == null && IsRoot)
            {
                return false;
            }
            var parent = Parent;
            while (parent != null)
            {
                if (parent == model)
                {
                    return false;
                }
                parent = parent.Parent;
            }
            return true;
        }

        protected override bool OnDrop(MovieModelBase model)
        {
            model.Parent?.Items.Remove(model);
            if (IsRoot == false)
            {
                Items.Add(model);
                model.Parent = this;
            }
            else
            {
                model.Parent = null;
            }
            return true;
        }

        public override void UpdateFrom(MovieModelBase clone)
        {
            Name = clone.Name;
        }

        #endregion
    }
}
