using Minimal.Mvvm;
using System.Diagnostics;

namespace MetroWpfApp.Models
{
    [DebuggerDisplay("Name={Name}")]
    public sealed class PersonModel: BindableBase, ICloneable<PersonModel>
    {
        #region Properties

        private string _name = null!;
        public required string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion

        #region Methods

        public PersonModel Clone()
        {
            return new PersonModel() { Name = Name};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override bool Equals(object? obj)
        {
            return obj is PersonModel model && string.Equals(Name, model.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
