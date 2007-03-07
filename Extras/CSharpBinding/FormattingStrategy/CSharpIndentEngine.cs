using System;
using System.Text;

using MonoDevelop.SourceEditor.Properties;

namespace CSharpBinding.FormattingStrategy {
	public partial class CSharpIndentEngine : ICloneable {
		IndentStack stack;
		
		StringBuilder linebuf;
		
		string keyword;
		
		string curIndent;
		
		bool needsReindent;
		bool popVerbatim;
		bool canBeLabel;
		bool isEscaped;
		
		int commentEndedAt;
		int firstNonLwsp;
		int lastNonLwsp;
		int wordStart;
		
		// previous char in the line
		char pc;
		
		// last significant char in the line (e.g. non-whitespace)
		char lc;
		
		int curLineNr;
		int cursor;
		
		// Constructors
		public CSharpIndentEngine ()
		{
			stack = new IndentStack ();
			linebuf = new StringBuilder ();
			Reset ();
		}
		
		// Properties
		public int Cursor {
			get { return cursor; }
		}
		
		public int LineNumber {
			get { return curLineNr; }
		}
		
		public int LineOffset {
			get { return linebuf.Length; }
		}
		
		public bool NeedsReindent {
			get { return needsReindent; }
		}
		
		public bool IsInsideVerbatimString {
			get { return stack.PeekInside (0) == Inside.VerbatimString; }
		}
		
		public bool IsInsideMultiLineComment {
			get { return stack.PeekInside (0) == Inside.MultiLineComment; }
		}
		
		string TabsToSpaces (string indent)
		{
			StringBuilder builder;
			int width;
			
			if (indent == String.Empty)
				return String.Empty;
			
			builder = new StringBuilder ();
			width = TextEditorProperties.TabIndent;
			for (int i = 0; i < indent.Length; i++) {
				if (indent[i] == '\t')
					builder.Append (' ', width);
				else
					builder.Append (indent[i]);
			}
			
			return builder.ToString ();
		}
		
		public string ThisLineIndent {
			get {
				if (TextEditorProperties.ConvertTabsToSpaces)
					return TabsToSpaces (curIndent);
				
				return curIndent;
			}
		}
		
		public string NewLineIndent {
			get {
				if (TextEditorProperties.ConvertTabsToSpaces)
					return TabsToSpaces (stack.PeekIndent (0));
				
				return stack.PeekIndent (0);
			}
		}
		
		// Methods
		
		// Resets the CSharpIndentEngine state machine
		public void Reset ()
		{
			stack.Reset ();
			
			linebuf.Length = 0;
			
			keyword = String.Empty;
			curIndent = String.Empty;
			
			needsReindent = false;
			popVerbatim = false;
			canBeLabel = true;
			isEscaped = false;
			
			commentEndedAt = -1;
			firstNonLwsp = -1;
			lastNonLwsp = -1;
			wordStart = -1;
			
			pc = '\0';
			lc = '\0';
			
			curLineNr = 1;
			cursor = 0;
		}
		
		// Clone the CSharpIndentEngine - useful if a consumer of this class wants
		// to test things w/o changing the real indent engine state
		public object Clone ()
		{
			CSharpIndentEngine engine = new CSharpIndentEngine ();
			
			engine.stack = (IndentStack) stack.Clone ();
			engine.linebuf = new StringBuilder (linebuf.ToString (), linebuf.Capacity);
			
			engine.keyword = keyword;
			engine.curIndent = curIndent;
			
			engine.needsReindent = needsReindent;
			engine.popVerbatim = popVerbatim;
			engine.canBeLabel = canBeLabel;
			engine.isEscaped = isEscaped;
			
			engine.commentEndedAt = commentEndedAt;
			engine.firstNonLwsp = firstNonLwsp;
			engine.lastNonLwsp = lastNonLwsp;
			engine.wordStart = wordStart;
			
			engine.pc = pc;
			engine.lc = lc;
			
			engine.curLineNr = curLineNr;
			engine.cursor = cursor;
			
			return engine;
		}
		
		void TrimIndent ()
		{
			switch (stack.PeekInside (0)) {
			case Inside.FoldedStatement:
			case Inside.Block:
				if (curIndent == String.Empty)
					return;
				
				// chop off the last tab (e.g. unindent 1 level)
				curIndent = curIndent.Substring (0, curIndent.Length - 1);
				break;
			default:
				curIndent = stack.PeekIndent (0);
				break;
			}
		}
		
		string WordIsKeyword ()
		{
			string str = linebuf.ToString (wordStart, linebuf.Length - wordStart);
			string[] keywords = new string [] {
				"namespace",
				"struct",
				"class",
				"enum",
				"switch",
				"case",
				"foreach",
				"while",
				"for",
				"do",
				"else",
				"if",
				"base",
				"this",
			};
			
			for (int i = 0; i < keywords.Length; i++) {
				if (str == keywords[i])
					return keywords[i];
			}
			
			return null;
		}
		
		bool WordIsDefault ()
		{
			string str = linebuf.ToString (wordStart, linebuf.Length - wordStart).Trim ();
			
			return str == "default";
		}
		
		void PushFoldedStatement ()
		{
			// don't let folded statements get too nested
			if (stack.PeekInside (0) == Inside.FoldedStatement &&
			    stack.PeekInside (1) == Inside.FoldedStatement)
				return;
			
			stack.Push (Inside.FoldedStatement, keyword, curLineNr, 0);
		}
		
		// Handlers for specific characters
		void PushHash (Inside inside)
		{
			// ignore if we are inside a string, char, or comment
			if ((inside & (Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			// ignore if '#' is not the first significant char on the line
			if (lc != '\0')
				return;
			
			stack.Push (Inside.PreProcessor, keyword, curLineNr, 0);
			
			curIndent = String.Empty;
			needsReindent = true;
		}
		
		void PushSlash (Inside inside)
		{
			// ignore these
			if ((inside & (Inside.PreProcessor | Inside.LineComment | Inside.StringOrChar)) != 0)
				return;
			
			if (inside == Inside.MultiLineComment) {
				// check for end of multi-line comment block
				if (pc == '*') {
					// restore the keyword and pop the multiline comment
					commentEndedAt = linebuf.Length;
					keyword = stack.PeekKeyword (0);
					stack.Pop ();
				}
			} else {
				// FoldedStatement, Block, Attribute or ParenList
				// check for the start of a single-line comment
				if (pc == '/')
					stack.Push (Inside.LineComment, keyword, curLineNr, 0);
			}
		}
		
		void PushBackSlash (Inside inside)
		{
			// string and char literals can have \-escapes
			if ((inside & (Inside.StringLiteral | Inside.CharLiteral)) != 0)
				isEscaped = !isEscaped;
		}
		
		void PushStar (Inside inside)
		{
			int n;
			
			if (pc != '/')
				return;
			
			// got a "/*" - might start a MultiLineComment
			if ((inside & (Inside.StringOrChar | Inside.Comment)) != 0) {
				if ((inside & Inside.MultiLineComment) != 0)
					Console.WriteLine ("Watch out! Nested /* */ comment detected!");
				return;
			}
			
			// push a new multiline comment onto the stack
			if (inside != Inside.PreProcessor)
				n = linebuf.Length - firstNonLwsp;
			else
				n = linebuf.Length;
			
			stack.Push (Inside.MultiLineComment, keyword, curLineNr, n);
		}
		
		void PushQuote (Inside inside)
		{
			Inside type;
			
			// ignore if in these
			if ((inside & (Inside.PreProcessor | Inside.Comment | Inside.CharLiteral)) != 0)
				return;
			
			if (inside == Inside.VerbatimString) {
				if (popVerbatim) {
					// back in the verbatim-string-literal token
					popVerbatim = false;
				} else {
					/* need to see the next char before we pop the
					 * verbatim-string-literal */
					popVerbatim = true;
				}
			} else if (inside == Inside.StringLiteral) {
				// check that it isn't escaped
				if (!isEscaped) {
					keyword = stack.PeekKeyword (0);
					stack.Pop ();
				}
			} else {
				// FoldedStatement, Block, Attribute or ParenList
				if (pc == '@')
					type = Inside.VerbatimString;
				else
					type = Inside.StringLiteral;
				
				// push a new string onto the stack
				stack.Push (type, keyword, curLineNr, 0);
			}
		}
		
		void PushSQuote (Inside inside)
		{
			if (inside == Inside.CharLiteral) {
				// check that it's not escaped
				if (isEscaped)
					return;
				
				keyword = stack.PeekKeyword (0);
				stack.Pop ();
				return;
			}
			
			if ((inside & (Inside.PreProcessor | Inside.String | Inside.Comment)) != 0) {
				// eat it
				return;
			}
			
			// push a new char literal onto the stack 
			stack.Push (Inside.CharLiteral, keyword, curLineNr, 0);
		}
		
		void PushColon (Inside inside)
		{
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			// can't be a case/label if there's no preceeding text
			if (wordStart == -1)
				return;
			
			// goto-label or case statement
			if (keyword == "case" || keyword == "default") {
				// case (or default) statement
				if (stack.PeekKeyword (0) != "switch")
					return;
				
				TrimIndent ();
				needsReindent = true;
			} else if (canBeLabel) {
				if (inside == Inside.FoldedStatement) {
					// not a label, probably a multi-line ternary statement
					return;
				}
				
				// goto label
				
				/* FIXME: make this configurable...
				 * options:
				 * a) left-justified (+1 space)
				 * b) up 1 level of indent
				 * c) no change
				 **/
				curIndent = " ";
				needsReindent = true;
			}
		}
		
		void PushSemicolon (Inside inside)
		{
			if (inside != Inside.FoldedStatement)
				return;
			
			// chain-pop folded statements
			while (stack.PeekInside (0) == Inside.FoldedStatement)
				stack.Pop ();
			
			keyword = String.Empty;
		}
		
		void PushOpenSq (Inside inside)
		{
			int n = 1;
			
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			// push a new attribute onto the stack
			if (firstNonLwsp != -1)
				n += linebuf.Length - firstNonLwsp;
			
			stack.Push (Inside.Attribute, keyword, curLineNr, n);
		}
		
		void PushCloseSq (Inside inside)
		{
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			if (inside != Inside.Attribute) {
				Console.WriteLine ("can't pop a '[' if we ain't got one?");
				return;
			}
			
			// pop this attribute off the stack
			keyword = stack.PeekKeyword (0);
			stack.Pop ();
		}
		
		void PushOpenParen (Inside inside)
		{
			int n = 1;
			
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			// push a new paren list onto the stack
			if (firstNonLwsp != -1)
				n += linebuf.Length - firstNonLwsp;
			
			stack.Push (Inside.ParenList, keyword, curLineNr, n);
			
			keyword = String.Empty;
		}
		
		void PushCloseParen (Inside inside)
		{
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			if (inside != Inside.ParenList) {
				Console.WriteLine ("can't pop a '(' if we ain't got one?");
				return;
			}
			
			// pop this paren list off the stack
			keyword = stack.PeekKeyword (0);
			stack.Pop ();
		}
		
		void PushOpenBrace (Inside inside)
		{
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			// push a new block onto the stack
			if (inside == Inside.FoldedStatement) {
				string pKeyword = stack.PeekKeyword (0);
				
				stack.Pop ();
				
				if ((pKeyword == "base" || pKeyword == "this" || pKeyword == "class") &&
				    stack.PeekInside (0) == Inside.FoldedStatement) {
					// Folded Constructor or class
					pKeyword = stack.PeekKeyword (0);
					stack.Pop ();
				}
				
				if (firstNonLwsp == -1)
					curIndent = stack.PeekIndent (0);
				
				stack.Push (Inside.Block, pKeyword, curLineNr, 0);
			} else if (stack.PeekKeyword (0) == "switch" && (keyword == "default" || keyword == "case")) {
				// this is the Emacs-Way(tm) but we could make it configurable
				stack.Push (Inside.Block, keyword, curLineNr, -1);
			} else {
				stack.Push (Inside.Block, keyword, curLineNr, 0);
			}
			
			keyword = String.Empty;
			if (firstNonLwsp == -1)
				needsReindent = true;
		}
		
		void PushCloseBrace (Inside inside)
		{
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) != 0)
				return;
			
			keyword = String.Empty;
			
			if (inside != Inside.Block) {
				Console.WriteLine ("can't pop a '{' if we ain't got one?");
				return;
			}
			
			// pop this block off the stack
			// Note: do not restore the keyword
			stack.Pop ();
			if (firstNonLwsp == -1) {
				TrimIndent ();
				needsReindent = true;
			}
		}
		
		void PushNewLine (Inside inside)
		{
			switch (inside) {
			case Inside.PreProcessor:
				// pop the preprocesor state unless the eoln is escaped
				if (lc != '\\') {
					keyword = stack.PeekKeyword (0);
					stack.Pop ();
				}
				break;
			case Inside.MultiLineComment:
				// nothing to do
				break;
			case Inside.LineComment:
				// pop the line comment
				keyword = stack.PeekKeyword (0);
				stack.Pop ();
				break;
			case Inside.VerbatimString:
				// nothing to do
				break;
			case Inside.StringLiteral:
				if (isEscaped) {
					/* I don't think c# allows breaking a
					 * normal string across lines even
					 * when escaping the carriage
					 * return... but how else should we
					 * handle this? */
					break;
				}
				
				/* not escaped... error!! but what can we do,
				 * eh? allow folding across multiple lines I
				 * guess... */
				break;
			case Inside.CharLiteral:
				/* this is an error... what to do? guess we'll
				 * just pretend it never happened */
				break;
			case Inside.Attribute:
				// nothing to do
				break;
			case Inside.ParenList:
				// nothing to do
				break;
			default:
				// Empty, FoldedStatement, and Block
				
				// if no text entered on this line, nothing to do
				if (lastNonLwsp == -1)
					break;
				
				switch (lc) {
				case '\0':
					// nothing entered on this line
					break;
				case ':':
					canBeLabel = canBeLabel && inside != Inside.FoldedStatement;
					
					if ((keyword == "default" || keyword == "case") || canBeLabel)
						break;
					
					PushFoldedStatement ();
					break;
				case '[':
					// handled elsewhere
					break;
				case ']':
					// handled elsewhere
					break;
				case '(':
					// handled elsewhere
					break;
				case '{':
					// handled elsewhere
					break;
				case '}':
					// handled elsewhere
					break;
				case ';':
					// handled elsewhere
					break;
				default:
					// case '/':
					if (commentEndedAt == lastNonLwsp)
						break;
					
					if (stack.PeekLineNr (0) == curLineNr)
						break;
					
					if (inside == Inside.Block) {
						if (pc == ',') {
							// avoid indenting if we are in a list
							/* Note: this may negate the need for checking
							 * for "enum" and "struct" below */
							break;
						}
						
						if (stack.PeekKeyword (0) == "struct" || stack.PeekKeyword (0) == "enum") {
							// just variable/value declarations
							break;
						}
					}
					
					PushFoldedStatement ();
					break;
				}
				
				break;
			}
			
			linebuf.Length = 0;
			
			curIndent = stack.PeekIndent (0);
			
			canBeLabel = true;
			
			commentEndedAt = -1;
			firstNonLwsp = -1;
			lastNonLwsp = -1;
			wordStart = -1;
			
			pc = '\0';
			lc = '\0';
			
			curLineNr++;
			cursor++;
		}
		
		// This is the main logic of this class...
		public void Push (char c)
		{
			Inside inside;
			
			inside = stack.PeekInside (0);
			
			// pop the verbatim-string-literal
			if (inside == Inside.VerbatimString && popVerbatim && c != '"') {
				keyword = stack.PeekKeyword (0);
				stack.Pop ();
				
				inside = stack.PeekInside (0);
			}
			
			needsReindent = false;
			
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) == 0 &&
			    wordStart != -1 && (linebuf.Length - wordStart) > 1) {
				if (Char.IsWhiteSpace (c) || c == '(' || c == '{') {
					string tmp = WordIsKeyword ();
					if (tmp != null)
						keyword = tmp;
				} else if (c == ':' && WordIsDefault ()) {
					keyword = "default";
				}
			}
			
			//Console.WriteLine ("Pushing '{0}'; wordStart = {1}; keyword = {2}", c, wordStart, keyword);
			
			switch (c) {
			case '#':
				PushHash (inside);
				break;
			case '/':
				PushSlash (inside);
				break;
			case '\\':
				PushBackSlash (inside);
				break;
			case '*':
				PushStar (inside);
				break;
			case '"':
				PushQuote (inside);
				break;
			case '\'':
				PushSQuote (inside);
				break;
			case ':':
				PushColon (inside);
				break;
			case ';':
				PushSemicolon (inside);
				break;
			case '[':
				PushOpenSq (inside);
				break;
			case ']':
				PushCloseSq (inside);
				break;
			case '(':
				PushOpenParen (inside);
				break;
			case ')':
				PushCloseParen (inside);
				break;
			case '{':
				PushOpenBrace (inside);
				break;
			case '}':
				PushCloseBrace (inside);
				break;
			case '\n':
				PushNewLine (inside);
				return;
			default:
				break;
			}
			
			inside = stack.PeekInside (0);
			if ((inside & (Inside.PreProcessor | Inside.StringOrChar | Inside.Comment)) == 0) {
				if (!Char.IsWhiteSpace (c)) {
					if (firstNonLwsp == -1)
						firstNonLwsp = linebuf.Length;
					
					if (wordStart != -1 && c != ':' && Char.IsWhiteSpace (pc)) {
						// goto labels must be single word tokens
						canBeLabel = false;
					} else if (wordStart == -1 && Char.IsDigit (c)) {
						// labels cannot start with a digit
						canBeLabel = false;
					}
					
					lastNonLwsp = linebuf.Length;
					
					if (c != ':') {
						if (Char.IsWhiteSpace (pc) || lc == ':')
							wordStart = linebuf.Length;
						else if (pc == '\0')
							wordStart = 0;
					}
				}
			} else if (c != '\\' && (inside & (Inside.StringLiteral | Inside.CharLiteral)) != 0) {
				// Note: PushBackSlash() will handle untoggling isEscaped if c == '\\'
				isEscaped = false;
			}
			
			pc = c;
			if (!Char.IsWhiteSpace (c))
				lc = c;
			
			linebuf.Append (c);
			
			cursor++;
		}
		
		public void Debug ()
		{
			Console.WriteLine ("\ncurLineNr = {0}\ncursor = {1}\nneedsReindent = {2}",
			                   curLineNr, cursor, needsReindent);
			Console.WriteLine ("stack:");
			for (int i = 0; i < stack.Count; i++) {
				switch (stack.PeekInside (i)) {
				case Inside.PreProcessor:
					Console.WriteLine ("\tpreprocessor directive");
					break;
				case Inside.MultiLineComment:
					Console.WriteLine ("\t/* */ comment block");
					break;
				case Inside.LineComment:
					Console.WriteLine ("\t// comment");
					break;
				case Inside.VerbatimString:
					Console.WriteLine ("\tverbatim string");
					break;
				case Inside.StringLiteral:
					Console.WriteLine ("\tstring literal");
					break;
				case Inside.CharLiteral:
					Console.WriteLine ("\tchar literal");
					break;
				case Inside.Attribute:
					Console.WriteLine ("\t[ ] attribute");
					break;
				case Inside.ParenList:
					Console.WriteLine ("\t( ) paren list");
					break;
				case Inside.FoldedStatement:
					if (stack.PeekKeyword (i) != String.Empty)
						Console.WriteLine ("\t{0}-statement", stack.PeekKeyword (i));
					else
						Console.WriteLine ("\tfolded statement?");
					break;
				case Inside.Block:
					if (stack.PeekKeyword (i) != String.Empty)
						Console.WriteLine ("\t{0} {1} block", stack.PeekKeyword (i), "{ }");
					else
						Console.WriteLine ("\tmethod {0} block?", "{ }");
					break;
				}
			}
		}
	}
}
