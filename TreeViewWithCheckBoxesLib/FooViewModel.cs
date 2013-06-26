#region Copyright Notice

// Copyright Josh Smith
// Used under the Code Project Open License 1.02
//
// Edits by Lefteris Aslanoglou noted as required per the license

#endregion

namespace TreeViewWithCheckBoxesLib
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class FooViewModel : INotifyPropertyChanged
    {
        #region Data

        private bool? _isChecked = false;
        private FooViewModel _parent;

        #endregion // Data

        #region CreateFoos

        // Edit by Lefteris Aslanoglou
        // Constructor switched to public from private
        public FooViewModel(string name)
        {
            Name = name;
            Children = new List<FooViewModel>();
        }

        public static List<FooViewModel> CreateFoos()
        {
            var root = new FooViewModel("Weapons")
                {
                    IsInitiallySelected = true,
                    Children =
                        {
                            new FooViewModel("Blades")
                                {
                                    Children = { new FooViewModel("Dagger"), new FooViewModel("Machete"), new FooViewModel("Sword"), }
                                },
                            new FooViewModel("Vehicles")
                                {
                                    Children = { new FooViewModel("Apache Helicopter"), new FooViewModel("Submarine"), new FooViewModel("Tank"), }
                                },
                            new FooViewModel("Guns")
                                {
                                    Children = { new FooViewModel("AK 47"), new FooViewModel("Beretta"), new FooViewModel("Uzi"), }
                                },
                        }
                };

            root.Initialize();
            return new List<FooViewModel> { root };
        }

        // Edit by Lefteris Aslanoglou
        // Initialize method switched to public from private
        public void Initialize()
        {
            foreach (FooViewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateFoos

        #region Properties

        // Edit by Lefteris Aslanoglou
        // All setters switched to public from private
        public List<FooViewModel> Children { get; set; }

        public bool IsInitiallySelected { get; set; }

        // Edit by Lefteris Aslanoglou
        // Added IsExpanded as public property for it to be bindable
        public bool IsExpanded { get; set; }

        public string Name { get; set; }

        #region IsChecked

        /// <summary>
        ///     Gets/sets the state of the associated UI toggle (ex. CheckBox). The return value is calculated based on the check state of
        ///     all child FooViewModels.  Setting this property to true or false will set all children to the same check state, and setting it to
        ///     any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
            {
                return;
            }

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));
            }

            if (updateParent && _parent != null)
            {
                _parent.VerifyCheckState();
            }

            OnPropertyChanged("IsChecked");
        }

        private void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}