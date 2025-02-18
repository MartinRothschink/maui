using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using ESize = ElmSharp.Size;
using XLabel = Microsoft.Maui.Controls.Label;
using XStackLayout = Microsoft.Maui.Controls.StackLayout;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	[System.Obsolete]
	public class EmptyItemAdaptor : ItemTemplateAdaptor, IEmptyAdaptor
	{
		static DataTemplate s_defaultEmptyTemplate = new DataTemplate(typeof(EmptyView));

		StructuredItemsView _structuredItemsView;
		public EmptyItemAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(itemsView, items, template)
		{
			_structuredItemsView = itemsView as StructuredItemsView;
		}

		public static EmptyItemAdaptor Create(ItemsView itemsView)
		{
			DataTemplate template = null;
			if (itemsView.EmptyView is View emptyView)
			{
				template = new DataTemplate(() =>
				{
					return emptyView;
				});
			}
			else
			{
				template = itemsView.EmptyViewTemplate ?? s_defaultEmptyTemplate;
			}
			var empty = new List<object>
			{
				itemsView.EmptyView ?? new object()
			};
			return new EmptyItemAdaptor(itemsView, empty, template);
		}

		public override ESize MeasureItem(int widthConstraint, int heightConstraint)
		{
			return new ESize(widthConstraint, heightConstraint);
		}

		public override EvasObject CreateNativeView(int index, EvasObject parent)
		{
			View emptyView = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				emptyView = selector.SelectTemplate(this[index], Element).CreateContent() as View;
			}
			else
			{
				emptyView = ItemTemplate.CreateContent() as View;
			}

			var header = CreateHeaderView();
			var footer = CreateFooterView();
			var layout = new XStackLayout();

			if (header != null)
			{
				layout.Children.Add(header);
			}
			layout.Children.Add(emptyView);
			if (footer != null)
			{
				layout.Children.Add(footer);
			}

			layout.Parent = Element;
			var renderer = Platform.GetOrCreateRenderer(layout);
			(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
			return renderer.NativeView;
		}

		public override void RemoveNativeView(EvasObject native)
		{
			native.Unrealize();
		}

		class EmptyView : XStackLayout
		{
			public EmptyView()
			{
				HorizontalOptions = LayoutOptions.Fill;
				VerticalOptions = LayoutOptions.Fill;
				Children.Add(
					new XLabel
					{
						Text = "No items found",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = Maui.TextAlignment.Center,
						VerticalTextAlignment = Maui.TextAlignment.Center,
					}
				);
			}
		}
	}
}
