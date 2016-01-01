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
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using dnSpy.AsmEditor.Commands;
using dnSpy.AsmEditor.Properties;
using dnSpy.AsmEditor.ViewHelpers;
using dnSpy.NRefactory;
using dnSpy.Shared.UI.Highlighting;
using ICSharpCode.Decompiler;

namespace dnSpy.AsmEditor.MethodBody {
	sealed class LocalsListHelper : ListBoxHelperBase<LocalVM> {
		readonly TypeSigCreator typeSigCreator;

		protected override string AddNewBeforeSelectionMessage {
			get { return dnSpy_AsmEditor_Resources.Local_Command1; }
		}

		protected override string AddNewAfterSelectionMessage {
			get { return dnSpy_AsmEditor_Resources.Local_Command2; }
		}

		protected override string AppendNewMessage {
			get { return dnSpy_AsmEditor_Resources.Local_Command3; }
		}

		protected override string RemoveSingularMessage {
			get { return dnSpy_AsmEditor_Resources.Local_Command4; }
		}

		protected override string RemovePluralMessage {
			get { return dnSpy_AsmEditor_Resources.Local_Command5; }
		}

		protected override string RemoveAllMessage {
			get { return dnSpy_AsmEditor_Resources.Local_Command6; }
		}

		public LocalsListHelper(ListView listView, Window ownerWindow)
			: base(listView) {
			this.typeSigCreator = new TypeSigCreator(ownerWindow);
		}

		protected override LocalVM[] GetSelectedItems() {
			return listBox.SelectedItems.Cast<LocalVM>().ToArray();
		}

		protected override void OnDataContextChangedInternal(object dataContext) {
			this.coll = ((MethodBodyVM)dataContext).CilBodyVM.LocalsListVM;
			this.coll.CollectionChanged += coll_CollectionChanged;
			InitializeLocals(this.coll);

			AddStandardMenuHandlers("AddLocal");
		}

		void coll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null)
				InitializeLocals(e.NewItems);
		}

		void InitializeLocals(System.Collections.IList list) {
			foreach (LocalVM local in list)
				local.TypeSigCreator = typeSigCreator;
		}

		protected override void CopyItemsAsText(LocalVM[] locals) {
			Array.Sort(locals, (a, b) => a.Index.CompareTo(b.Index));

			var output = new NoSyntaxHighlightOutput();

			for (int i = 0; i < locals.Length; i++) {
				if (i > 0)
					output.WriteLine();

				var local = locals[i];
				output.Write(local.Index.ToString(), TextTokenType.Number);
				output.Write("\t", TextTokenType.Text);
				output.Write(local.IsPinned ? dnSpy_AsmEditor_Resources.Local_Pinned_Character : string.Empty, TextTokenType.Text);
				output.Write("\t", TextTokenType.Text);
				output.Write(local.IsCompilerGenerated ? dnSpy_AsmEditor_Resources.Local_CompilerGenerated_Character : string.Empty, TextTokenType.Text);
				output.Write("\t", TextTokenType.Text);
				output.Write(local.Name ?? string.Empty, TextTokenType.Local);
				output.Write("\t", TextTokenType.Text);
				BodyUtils.WriteObject(output, local.Type);
			}
			if (locals.Length > 1)
				output.WriteLine();

			Clipboard.SetText(output.ToString());
		}
	}
}
