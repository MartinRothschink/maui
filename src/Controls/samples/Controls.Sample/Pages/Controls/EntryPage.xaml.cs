﻿using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class EntryPage
	{
		public EntryPage()
		{
			InitializeComponent();

			sldSelection.Maximum = entrySelection.Text.Length;
			sldSelection.Value = entrySelection.SelectionLength;
			sldCursorPosition.Maximum = entryCursor.Text.Length;
			sldCursorPosition.Value = entryCursor.CursorPosition;
		}

		protected override void OnAppearing()
		{
			IsFocusEntry.Focus();

			entryCursor.PropertyChanged += OnEntryPropertyChanged;
			entrySelection.PropertyChanged += OnEntrySelectionPropertyChanged;

			IsFocusEntry.Focused += OnFocused;
			IsFocusEntry.Unfocused += OnUnfocused;
		}

		protected override void OnDisappearing()
		{
			entryCursor.PropertyChanged -= OnEntryPropertyChanged;
			entrySelection.PropertyChanged -= OnEntrySelectionPropertyChanged;

			IsFocusEntry.Focused -= OnFocused;
			IsFocusEntry.Unfocused -= OnUnfocused;
		}

		void OnFocused(object sender, FocusEventArgs e)
		{
			Debug.WriteLine("Focused");
		}

		void OnUnfocused(object sender, FocusEventArgs e)
		{
			Debug.WriteLine("Unfocused");
		}

		void OnSlideCursorPositionValueChanged(object sender, ValueChangedEventArgs e)
		{
			entryCursor.CursorPosition = (int)e.NewValue;
		}

		void OnSlideSelectionValueChanged(object sender, ValueChangedEventArgs e)
		{
			entrySelection.SelectionLength = (int)e.NewValue;
		}

		void OnEntryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Entry.CursorPosition))
				lblCursor.Text = $"CursorPosition = {((Entry)sender).CursorPosition}";
			if (e.PropertyName == nameof(Entry.Text))
				sldCursorPosition.Maximum = ((Entry)sender).Text.Length;
		}

		void OnEntrySelectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Entry.SelectionLength))
				lblSelection.Text = $"SelectionLength = {((Entry)sender).SelectionLength}";
			if (e.PropertyName == nameof(Entry.Text))
				sldSelection.Maximum = ((Entry)sender).Text.Length;
		}

		void OnEntryCompleted(object sender, EventArgs e)
		{
			var text = ((Entry)sender).Text;
			DisplayAlert("Completed", text, "Ok");
		}
	}
}