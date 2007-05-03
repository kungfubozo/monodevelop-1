//
// IRefactorer.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.CodeDom;
using MonoDevelop.Projects.Text;
using MonoDevelop.Projects.Parser;

namespace MonoDevelop.Projects.CodeGeneration
{
	public interface IRefactorer
	{
		RefactorOperations SupportedOperations { get; }
		
		IClass CreateClass (RefactorerContext ctx, string directory, string namspace, CodeTypeDeclaration type);
		IClass RenameClass (RefactorerContext ctx, IClass cls, string newName);
		MemberReferenceCollection FindClassReferences (RefactorerContext ctx, string fileName, IClass cls);
		
		IMember AddMember (RefactorerContext ctx, IClass cls, CodeTypeMember memberInfo);
		void RemoveMember (RefactorerContext ctx, IClass cls, IMember member);
		IMember RenameMember (RefactorerContext ctx, IClass cls, IMember member, string newName);
		IMember ReplaceMember (RefactorerContext ctx, IClass cls, IMember oldMember, CodeTypeMember memberInfo);
		MemberReferenceCollection FindMemberReferences (RefactorerContext ctx, string fileName, IClass cls, IMember member);
		
		LocalVariable RenameVariable (RefactorerContext ctx, LocalVariable var, string newName);
		MemberReferenceCollection FindVariableReferences (RefactorerContext ctx, string fileName, LocalVariable var);
		
		IParameter RenameParameter (RefactorerContext ctx, IMethod method, IParameter param, string newName);
		MemberReferenceCollection FindParameterReferences (RefactorerContext ctx, string fileName, IMethod method, IParameter param);
	}
	
	public class MemberReference
	{
		int position;
		int line;
		int column;
		string fileName;
		string name;
		string textLine;
		RefactorerContext rctx;
		
		public MemberReference (RefactorerContext rctx, string fileName, int position, int line, int column, string name, string textLine)
		{
			this.position = position;
			this.line = line;
			this.column = column;
			this.fileName = fileName;
			this.name = name;
			this.rctx = rctx;
			this.textLine = textLine;
			if (textLine == null || textLine.Length == 0)
				textLine = name;
		}
		
		public int Position {
			get { return position; }
		}
		
		public int Line {
			get { return line; }
		}
		
		public int Column {
			get { return column; }
		}
		
		public string FileName {
			get { return fileName; }
		}
		
		public string TextLine {
			get { return textLine; }
		}
		
		public virtual void Rename (string newName)
		{
			if (rctx == null)
				throw new InvalidOperationException ("Refactory context not available.");

			IEditableTextFile file = rctx.GetFile (fileName);
			if (file != null) {
				file.DeleteText (position, name.Length);
				file.InsertText (position, newName);
				rctx.Save ();
			}
		}
	}
	
	public class MemberReferenceCollection: CollectionBase
	{
		public void Add (MemberReference reference)
		{
			List.Add (reference);
		}
		
		public void AddRange (IEnumerable collection)
		{
			foreach (MemberReference mref in collection)
				List.Add (mref);
		}
		
		public MemberReference this [int n] {
			get { return (MemberReference) List [n]; }
		}
		
		public void RenameAll (string newName)
		{
			ArrayList list = new ArrayList ();
			list.AddRange (this);
			list.Sort (new MemberComparer ());
			
			foreach (MemberReference mref in list) {
				mref.Rename (newName);
			}
		}
		
		class MemberComparer: IComparer
		{
			public int Compare (object o1, object o2)
			{
				MemberReference r1 = (MemberReference) o1;
				MemberReference r2 = (MemberReference) o2;
				int c = r1.FileName.CompareTo (r2.FileName);
				if (c != 0) return c;
				return r2.Position - r1.Position; 
			}
		}
	}
}
