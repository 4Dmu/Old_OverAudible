using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OverAudible.Behaviors
{
    public class MultiSelectBehavior : Behavior<ListView>
    {
        #region SelectedItems Attached Property
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(ObservableCollection<object>),
            typeof(MultiSelectBehavior),
            new PropertyMetadata(new ObservableCollection<object>(), PropertyChangedCallback));

        #endregion

        public MultiSelectBehavior()
        {
            SelectedItems = new ObservableCollection<object>();
        }

        public ObservableCollection<object> SelectedItems
        {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.OldValue is ObservableCollection<object>)
            {
                (dependencyPropertyChangedEventArgs.OldValue as ObservableCollection<object>).CollectionChanged -=
                    (s, e) => SelectedItemsChanged(dependencyObject, e); // TO FIX event unbsubscription via anonymous delegate
            }

            if (dependencyPropertyChangedEventArgs.NewValue is ObservableCollection<object>)
            {
                (dependencyPropertyChangedEventArgs.NewValue as ObservableCollection<object>).CollectionChanged +=
                    (s, e) => SelectedItemsChanged(dependencyObject, e);
            }
        }

        private static void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is MultiSelectBehavior)
            {
                var listViewBase = (sender as MultiSelectBehavior).AssociatedObject;

                var listSelectedItems = listViewBase.SelectedItems;
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (listSelectedItems.Contains(item))
                        {
                            listSelectedItems.Remove(item);
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        listSelectedItems.Add(item);
                    }
                }
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                }
            }

            foreach (var item in e.AddedItems)
            {
                SelectedItems.Add(item);
            }
        }
    }
}
