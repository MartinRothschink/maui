using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	[TestFixture]
	public class ControlTemplateTests : BaseTestFixture
	{
		public class ContentControl : StackLayout
		{
			public ContentControl()
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name", source: RelativeBindingSource.TemplatedParent));

				Children.Add(label);
				Children.Add(new ContentPresenter());
			}
		}

		public class PresenterWrapper : ContentView
		{
			public PresenterWrapper()
			{
				Content = new ContentPresenter();
			}
		}

		public class TestView : ContentView
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create(nameof(Name), typeof(string), typeof(TestView), default(string));

			public string Name
			{
				get { return (string)GetValue(NameProperty); }
				set { SetValue(NameProperty, value); }
			}

			public TestView()
			{
				ControlTemplate = new ControlTemplate(typeof(ContentControl));
			}
		}

		[Test]
		public void ResettingControlTemplateNullsPresenterContent()
		{
			var testView = new TestView
			{
				ControlTemplate = new ControlTemplate(typeof(PresenterWrapper))
			};

			var label = new Label();
			testView.Content = label;

			var child1 = ((IElementController)testView).LogicalChildren[0];
			var child2 = ((IElementController)child1).LogicalChildren[0];

			var originalPresenter = (ContentPresenter)child2;

			Assert.AreEqual(label, originalPresenter.Content);

			testView.ControlTemplate = new ControlTemplate(typeof(PresenterWrapper));

			Assert.IsNull(originalPresenter.Content);
		}

		[Test]
		public void NestedTemplateBindings()
		{
			var testView = new TestView();

			var child1 = ((IElementController)testView).LogicalChildren[0];
			var child2 = ((IElementController)child1).LogicalChildren[0];

			var label = (Label)child2;

			Assert.IsNull(label.Text);

			testView.Name = "Bar";
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void ParentControlTemplateDoesNotClearChildTemplate()
		{
			var parentView = new TestView();
			var childView = new TestView();

			parentView.Content = childView;
			childView.Content = new Button();

			var child1 = ((IElementController)childView).LogicalChildren[0];
			var child2 = ((IElementController)child1).LogicalChildren[1];

			var childPresenter = (ContentPresenter)child2;

			parentView.ControlTemplate = new ControlTemplate(typeof(ContentControl));
			Assert.IsNotNull(childPresenter.Content);
		}

		[Test]
		public void NullConstructor()
		{
			Func<object> func = null;
			Assert.Throws<ArgumentNullException>(() => new ControlTemplate(func));
		}

		class TestPage : ContentPage
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create(nameof(Name), typeof(string), typeof(TestPage), null);

			public string Name
			{
				get { return (string)GetValue(NameProperty); }
				set { SetValue(NameProperty, value); }
			}
		}

		class TestContent : ContentView
		{
			public TestContent()
			{
				Content = new Entry();
				Content.SetBinding(Entry.TextProperty, new Binding("Name", BindingMode.TwoWay, source: RelativeBindingSource.TemplatedParent));
			}
		}

		class ViewModel : INotifyPropertyChanged
		{
			string name;

			public string Name
			{
				get { return name; }
				set
				{
					if (name == value)
						return;
					name = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Test]
		public void DoubleTwoWayBindingWorks()
		{
			var page = new TestPage();
			var viewModel = new ViewModel
			{
				Name = "Jason"
			};
			page.BindingContext = viewModel;

			page.ControlTemplate = new ControlTemplate(typeof(TestContent));
			page.SetBinding(TestPage.NameProperty, "Name");

			var entry = ((ContentView)((IElementController)page).LogicalChildren[0]).Content as Entry;
			((IElementController)entry).SetValueFromRenderer(Entry.TextProperty, "Bar");
			viewModel.Name = "Raz";

			Assert.AreEqual("Raz", entry.Text);
		}
	}
}