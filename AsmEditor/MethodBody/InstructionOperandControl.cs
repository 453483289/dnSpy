﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using dnSpy.AsmEditor.Converters;

namespace dnSpy.AsmEditor.MethodBody {
	sealed class InstructionOperandControl : ContentControl {
		public static readonly DependencyProperty InstructionOperandVMProperty =
			DependencyProperty.Register("InstructionOperandVM", typeof(InstructionOperandVM), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null, OnInstructionOperandVMChanged));
		public static readonly DependencyProperty TextBoxStyleProperty =
			DependencyProperty.Register("TextBoxStyle", typeof(Style), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null));
		public static readonly DependencyProperty ComboBoxStyleProperty =
			DependencyProperty.Register("ComboBoxStyle", typeof(Style), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null));
		public static readonly DependencyProperty ComboBoxItemTemplateProperty =
			DependencyProperty.Register("ComboBoxItemTemplate", typeof(DataTemplate), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null));
		public static readonly DependencyProperty ComboBoxSelectionBoxItemTemplateProperty =
			DependencyProperty.Register("ComboBoxSelectionBoxItemTemplate", typeof(DataTemplate), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null));
		public static readonly DependencyProperty ButtonStyleProperty =
			DependencyProperty.Register("ButtonStyle", typeof(Style), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null));
		public static readonly DependencyProperty ButtonCommandProperty =
			DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(InstructionOperandControl),
			new FrameworkPropertyMetadata(null));

		public InstructionOperandVM InstructionOperandVM {
			get { return (InstructionOperandVM)GetValue(InstructionOperandVMProperty); }
			set { SetValue(InstructionOperandVMProperty, value); }
		}

		public Style TextBoxStyle {
			get { return (Style)GetValue(TextBoxStyleProperty); }
			set { SetValue(TextBoxStyleProperty, value); }
		}

		public Style ComboBoxStyle {
			get { return (Style)GetValue(ComboBoxStyleProperty); }
			set { SetValue(ComboBoxStyleProperty, value); }
		}

		public DataTemplate ComboBoxItemTemplate {
			get { return (DataTemplate)GetValue(ComboBoxItemTemplateProperty); }
			set { SetValue(ComboBoxItemTemplateProperty, value); }
		}

		public DataTemplate ComboBoxSelectionBoxItemTemplate {
			get { return (DataTemplate)GetValue(ComboBoxSelectionBoxItemTemplateProperty); }
			set { SetValue(ComboBoxSelectionBoxItemTemplateProperty, value); }
		}

		public Style ButtonStyle {
			get { return (Style)GetValue(ButtonStyleProperty); }
			set { SetValue(ButtonStyleProperty, value); }
		}

		public ICommand ButtonCommand {
			get { return (ICommand)GetValue(ButtonCommandProperty); }
			set { SetValue(ButtonCommandProperty, value); }
		}

		static void OnInstructionOperandVMChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var self = (InstructionOperandControl)d;
			self.OnInstructionOperandVMChanged((InstructionOperandVM)e.OldValue, (InstructionOperandVM)e.NewValue);
		}

		void OnInstructionOperandVMChanged(InstructionOperandVM oldValue, InstructionOperandVM newValue) {
			if (oldValue != null)
				oldValue.PropertyChanged -= instructionOperandVM_PropertyChanged;
			if (newValue != null) {
				newValue.PropertyChanged += instructionOperandVM_PropertyChanged;
				InitializeOperandType(newValue);
			}
		}

		void instructionOperandVM_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "InstructionOperandType")
				InitializeOperandType((InstructionOperandVM)sender);
		}

		void InitializeOperandType(InstructionOperandVM instructionOperandVM) {
			switch (instructionOperandVM.InstructionOperandType) {
			case InstructionOperandType.None:
				Content = null;
				break;

			case InstructionOperandType.SByte:
			case InstructionOperandType.Byte:
			case InstructionOperandType.Int32:
			case InstructionOperandType.Int64:
			case InstructionOperandType.Single:
			case InstructionOperandType.Double:
			case InstructionOperandType.String:
				// Don't cache the TextBox as a field in this class. The error border disappears when
				// switching from a textbox opcode to a non-textbox opcode and back to the textbox
				// again. Only solution seems to be to create a new textbox.
				var textBox = Content as TextBox;
				if (textBox == null) {
					textBox = new TextBox();
					var binding = new Binding("InstructionOperandVM.Text.StringValue") {
						Source = this,
						ValidatesOnDataErrors = true,
						ValidatesOnExceptions = true,
						UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
					};
					textBox.SetBinding(TextBox.TextProperty, binding);
					binding = new Binding("TextBoxStyle") {
						Source = this,
					};
					textBox.SetBinding(TextBox.StyleProperty, binding);
					Content = textBox;
				}
				break;

			case InstructionOperandType.Field:
			case InstructionOperandType.Method:
			case InstructionOperandType.Token:
			case InstructionOperandType.Type:
			case InstructionOperandType.MethodSig:
			case InstructionOperandType.SwitchTargets:
				var button = Content as FastClickButton;
				if (button == null) {
					button = new FastClickButton();
					var binding = new Binding("InstructionOperandVM.Other") {
						Source = this,
						Mode = BindingMode.OneWay,
						Converter = CilObjectConverter.Instance,
						ValidatesOnDataErrors = true,
						ValidatesOnExceptions = true,
						UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
					};
					button.SetBinding(Button.ContentProperty, binding);
					binding = new Binding("ButtonStyle") {
						Source = this,
					};
					button.SetBinding(Button.StyleProperty, binding);
					binding = new Binding("ButtonCommand") {
						Source = this,
					};
					button.SetBinding(Button.CommandProperty, binding);
					button.CommandParameter = button;
					Content = button;
				}
				break;

			case InstructionOperandType.BranchTarget:
			case InstructionOperandType.Local:
			case InstructionOperandType.Parameter:
				var comboBox = Content as ComboBox;
				if (comboBox == null) {
					comboBox = new ComboBox();
					comboBox.ItemTemplate = (DataTemplate)GetValue(ComboBoxItemTemplateProperty);
					ComboBoxAttachedProps.SetSelectionBoxItemTemplate(comboBox, (DataTemplate)GetValue(ComboBoxSelectionBoxItemTemplateProperty));
					var binding = new Binding("InstructionOperandVM.OperandListVM.Items") {
						Source = this,
					};
					comboBox.SetBinding(ComboBox.ItemsSourceProperty, binding);
					binding = new Binding("InstructionOperandVM.OperandListVM.SelectedIndex") {
						Source = this,
						ValidatesOnDataErrors = true,
						ValidatesOnExceptions = true,
						UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
					};
					comboBox.SetBinding(ComboBox.SelectedIndexProperty, binding);
					binding = new Binding("ComboBoxStyle") {
						Source = this,
					};
					comboBox.SetBinding(ComboBox.StyleProperty, binding);
					binding = new Binding("ComboBoxItemTemplate") {
						Source = this,
					};
					comboBox.SetBinding(ComboBox.ItemTemplateProperty, binding);
					binding = new Binding("ComboBoxSelectionBoxItemTemplate") {
						Source = this,
					};
					comboBox.SetBinding(ComboBoxAttachedProps.SelectionBoxItemTemplateProperty, binding);
					Content = comboBox;
				}
				break;

			default:
				throw new InvalidOperationException();
			}
		}
	}
}
