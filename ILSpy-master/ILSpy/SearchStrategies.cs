﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;
using ICSharpCode.ILSpy.TreeNodes;
using Mono.Cecil;
using Code = Mono.Cecil.Cil.Code;

namespace ICSharpCode.ILSpy
{
	abstract class AbstractSearchStrategy
	{
		protected string[] searchTerm;
		protected Regex regex;
		protected bool fullNameSearch;

		protected AbstractSearchStrategy(params string[] terms)
		{
			if (terms.Length == 1 && terms[0].Length > 2) {
				var search = terms[0];
				if (search.StartsWith("/", StringComparison.Ordinal) && search.Length > 4) {
					var regexString = search.Substring(1, search.Length - 1);
					fullNameSearch = search.Contains("\\.");
					if (regexString.EndsWith("/", StringComparison.Ordinal))
						regexString = regexString.Substring(0, regexString.Length - 1);
					regex = SafeNewRegex(regexString);
				} else {
					fullNameSearch = search.Contains(".");
				}

				terms[0] = search;
			}

			searchTerm = terms;
		}

		protected float CalculateFitness(MemberReference member)
		{
			string text = member.Name;

			// Probably compiler generated types without meaningful names, show them last
			if (text.StartsWith("<")) {
				return 0;
			}

			// Constructors always have the same name in IL:
			// Use type name instead
			if (text == ".cctor" || text == ".ctor") {
				text = member.DeclaringType.Name;
			}

			// Ignore generic arguments, it not possible to search based on them either
			text = ReflectionHelper.SplitTypeParameterCountFromReflectionName(text);

			return 1.0f / text.Length;
		}

		protected virtual bool IsMatch(FieldDefinition field, Language language)
		{
			return false;
		}

		protected virtual bool IsMatch(PropertyDefinition property, Language language)
		{
			return false;
		}

		protected virtual bool IsMatch(EventDefinition ev, Language language)
		{
			return false;
		}

		protected virtual bool IsMatch(MethodDefinition m, Language language)
		{
			return false;
		}

		protected virtual bool MatchName(IMemberDefinition m, Language language)
		{
			return IsMatch(t => GetLanguageSpecificName(language, m, regex != null ? fullNameSearch : t.Contains(".")));
		}

		protected virtual bool IsMatch(Func<string, string> getText)
		{
			if (regex != null) {
				return regex.IsMatch(getText(""));
			}

			for (int i = 0; i < searchTerm.Length; ++i) {
				// How to handle overlapping matches?
				var term = searchTerm[i];
				if (string.IsNullOrEmpty(term)) continue;
				string text = getText(term);
				switch (term[0]) {
					case '+': // must contain
						term = term.Substring(1);
						goto default;
					case '-': // should not contain
						if (term.Length > 1 && text.IndexOf(term.Substring(1), StringComparison.OrdinalIgnoreCase) >= 0)
							return false;
						break;
					case '=': // exact match
						{
							var equalCompareLength = text.IndexOf('`');
							if (equalCompareLength == -1)
								equalCompareLength = text.Length;

							if (term.Length > 1 && String.Compare(term, 1, text, 0, Math.Max(term.Length, equalCompareLength), StringComparison.OrdinalIgnoreCase) != 0)
								return false;
						}
						break;
					default:
						if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
							return false;
						break;
				}
			}
			return true;
		}

		string GetLanguageSpecificName(Language language, IMemberDefinition member, bool fullName = false)
		{
			switch (member) {
				case TypeDefinition t:
					return language.TypeToString(t, fullName);
				case FieldDefinition f:
					return fullName ? language.TypeToString(f.DeclaringType, fullName) + "." + language.FormatFieldName(f) : language.FormatFieldName(f);
				case PropertyDefinition p:
					return fullName ? language.TypeToString(p.DeclaringType, fullName) + "." + language.FormatPropertyName(p) : language.FormatPropertyName(p);
				case MethodDefinition m:
					return fullName ? language.TypeToString(m.DeclaringType, fullName) + "." + language.FormatMethodName(m) : language.FormatMethodName(m);
				case EventDefinition e:
					return fullName ? language.TypeToString(e.DeclaringType, fullName) + "." + language.FormatEventName(e) : language.FormatEventName(e);
				default:
					throw new NotSupportedException(member?.GetType() + " not supported!");
			}
		}

		void Add<T>(IEnumerable<T> items, TypeDefinition type, Language language, Action<SearchResult> addResult, Func<T, Language, bool> matcher, Func<T, ImageSource> image) where T : MemberReference
		{
			foreach (var item in items) {
				if (matcher(item, language)) {
					addResult(new SearchResult
					{
						Member = item,
						Fitness = CalculateFitness(item),
						Image = image(item),
						Name = GetLanguageSpecificName(language, (IMemberDefinition)item),
						LocationImage = TypeTreeNode.GetIcon(type),
						Location = language.TypeToString(type, includeNamespace: true)
					});
				}
			}
		}

		public virtual void Search(TypeDefinition type, Language language, Action<SearchResult> addResult)
		{
			Add(type.Fields, type, language, addResult, IsMatch, FieldTreeNode.GetIcon);
			Add(type.Properties, type, language, addResult, IsMatch, p => PropertyTreeNode.GetIcon(p));
			Add(type.Events, type, language, addResult, IsMatch, EventTreeNode.GetIcon);
			Add(type.Methods.Where(NotSpecialMethod), type, language, addResult, IsMatch, MethodTreeNode.GetIcon);

			foreach (TypeDefinition nestedType in type.NestedTypes) {
				Search(nestedType, language, addResult);
			}
		}

		bool NotSpecialMethod(MethodDefinition arg)
		{
			return (arg.SemanticsAttributes & (
				MethodSemanticsAttributes.Setter
				| MethodSemanticsAttributes.Getter
				| MethodSemanticsAttributes.AddOn
				| MethodSemanticsAttributes.RemoveOn
				| MethodSemanticsAttributes.Fire)) == 0;
		}

		Regex SafeNewRegex(string unsafePattern)
		{
			try {
				return new Regex(unsafePattern, RegexOptions.Compiled);
			} catch (ArgumentException) {
				return null;
			}
		}
	}

	class LiteralSearchStrategy : AbstractSearchStrategy
	{
		readonly TypeCode searchTermLiteralType;
		readonly object searchTermLiteralValue;

		public LiteralSearchStrategy(params string[] terms)
			: base(terms)
		{
			if (searchTerm.Length == 1) {
				var lexer = new Lexer(new LATextReader(new System.IO.StringReader(searchTerm[0])));
				var value = lexer.NextToken();

				if (value != null && value.LiteralValue != null) {
					TypeCode valueType = Type.GetTypeCode(value.LiteralValue.GetType());
					switch (valueType) {
					case TypeCode.Byte:
					case TypeCode.SByte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
						searchTermLiteralType = TypeCode.Int64;
						searchTermLiteralValue = CSharpPrimitiveCast.Cast(TypeCode.Int64, value.LiteralValue, false);
						break;
					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.String:
						searchTermLiteralType = valueType;
						searchTermLiteralValue = value.LiteralValue;
						break;
					}
				}
			}
		}

		protected override bool IsMatch(FieldDefinition field, Language language)
		{
			return IsLiteralMatch(field.Constant);
		}

		protected override bool IsMatch(PropertyDefinition property, Language language)
		{
			return MethodIsLiteralMatch(property.GetMethod) || MethodIsLiteralMatch(property.SetMethod);
		}

		protected override bool IsMatch(EventDefinition ev, Language language)
		{
			return MethodIsLiteralMatch(ev.AddMethod) || MethodIsLiteralMatch(ev.RemoveMethod) || MethodIsLiteralMatch(ev.InvokeMethod);
		}

		protected override bool IsMatch(MethodDefinition m, Language language)
		{
			return MethodIsLiteralMatch(m);
		}

		bool IsLiteralMatch(object val)
		{
			if (val == null)
				return false;
			switch (searchTermLiteralType) {
				case TypeCode.Int64:
					TypeCode tc = Type.GetTypeCode(val.GetType());
					if (tc >= TypeCode.SByte && tc <= TypeCode.UInt64)
						return CSharpPrimitiveCast.Cast(TypeCode.Int64, val, false).Equals(searchTermLiteralValue);
					else
						return false;
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.String:
					return searchTermLiteralValue.Equals(val);
				default:
					// substring search with searchTerm
					return IsMatch(t => val.ToString());
			}
		}

		bool MethodIsLiteralMatch(MethodDefinition m)
		{
			if (m == null)
				return false;
			var body = m.Body;
			if (body == null)
				return false;
			if (searchTermLiteralType == TypeCode.Int64) {
				long val = (long)searchTermLiteralValue;
				foreach (var inst in body.Instructions) {
					switch (inst.OpCode.Code) {
					case Code.Ldc_I8:
						if (val == (long)inst.Operand)
							return true;
						break;
					case Code.Ldc_I4:
						if (val == (int)inst.Operand)
							return true;
						break;
					case Code.Ldc_I4_S:
						if (val == (sbyte)inst.Operand)
							return true;
						break;
					case Code.Ldc_I4_M1:
						if (val == -1)
							return true;
						break;
					case Code.Ldc_I4_0:
						if (val == 0)
							return true;
						break;
					case Code.Ldc_I4_1:
						if (val == 1)
							return true;
						break;
					case Code.Ldc_I4_2:
						if (val == 2)
							return true;
						break;
					case Code.Ldc_I4_3:
						if (val == 3)
							return true;
						break;
					case Code.Ldc_I4_4:
						if (val == 4)
							return true;
						break;
					case Code.Ldc_I4_5:
						if (val == 5)
							return true;
						break;
					case Code.Ldc_I4_6:
						if (val == 6)
							return true;
						break;
					case Code.Ldc_I4_7:
						if (val == 7)
							return true;
						break;
					case Code.Ldc_I4_8:
						if (val == 8)
							return true;
						break;
					}
				}
			} else if (searchTermLiteralType != TypeCode.Empty) {
				Code expectedCode;
				switch (searchTermLiteralType) {
				case TypeCode.Single:
					expectedCode = Code.Ldc_R4;
					break;
				case TypeCode.Double:
					expectedCode = Code.Ldc_R8;
					break;
				case TypeCode.String:
					expectedCode = Code.Ldstr;
					break;
				default:
					throw new InvalidOperationException();
				}
				foreach (var inst in body.Instructions) {
					if (inst.OpCode.Code == expectedCode && searchTermLiteralValue.Equals(inst.Operand))
						return true;
				}
			} else {
				foreach (var inst in body.Instructions) {
					if (inst.OpCode.Code == Code.Ldstr && IsMatch(t => (string)inst.Operand))
						return true;
				}
			}
			return false;
		}
	}

	enum MemberSearchKind
	{
		All,
		Field,
		Property,
		Event,
		Method
	}

	class MemberSearchStrategy : AbstractSearchStrategy
	{
		MemberSearchKind searchKind;

		public MemberSearchStrategy(string term, MemberSearchKind searchKind = MemberSearchKind.All)
			: this(new[] { term }, searchKind)
		{
		}

		public MemberSearchStrategy(string[] terms, MemberSearchKind searchKind = MemberSearchKind.All)
			: base(terms)
		{
			this.searchKind = searchKind;
		}

		protected override bool IsMatch(FieldDefinition field, Language language)
		{
			return (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Field) && MatchName(field, language);
		}

		protected override bool IsMatch(PropertyDefinition property, Language language)
		{
			return (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Property) && MatchName(property, language);
		}

		protected override bool IsMatch(EventDefinition ev, Language language)
		{
			return (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Event) && MatchName(ev, language);
		}

		protected override bool IsMatch(MethodDefinition m, Language language)
		{
			return (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Method) && MatchName(m, language);
		}
	}

	class TypeSearchStrategy : AbstractSearchStrategy
	{
		public TypeSearchStrategy(params string[] terms)
			: base(terms)
		{
		}

		public override void Search(TypeDefinition type, Language language, Action<SearchResult> addResult)
		{
			if (MatchName(type, language)) {
				string name = language.TypeToString(type, includeNamespace: false);
				addResult(new SearchResult {
					Member = type,
					Fitness = CalculateFitness(type),
					Image = TypeTreeNode.GetIcon(type),
					Name = name,
					LocationImage = type.DeclaringType != null ? TypeTreeNode.GetIcon(type.DeclaringType) : Images.Namespace,
					Location = type.DeclaringType != null ? language.TypeToString(type.DeclaringType, includeNamespace: true) : type.Namespace
				});
			}

			foreach (TypeDefinition nestedType in type.NestedTypes) {
				Search(nestedType, language, addResult);
			}
		}
	}

	class TypeAndMemberSearchStrategy : AbstractSearchStrategy
	{
		public TypeAndMemberSearchStrategy(params string[] terms)
			: base(terms)
		{
		}

		public override void Search(TypeDefinition type, Language language, Action<SearchResult> addResult)
		{
			if (MatchName(type, language))
			{
				string name = language.TypeToString(type, includeNamespace: false);
				addResult(new SearchResult
				{
					Member = type,
					Image = TypeTreeNode.GetIcon(type),
					Fitness = CalculateFitness(type),
					Name = name,
					LocationImage = type.DeclaringType != null ? TypeTreeNode.GetIcon(type.DeclaringType) : Images.Namespace,
					Location = type.DeclaringType != null ? language.TypeToString(type.DeclaringType, includeNamespace: true) : type.Namespace
				});
			}

			base.Search(type, language, addResult);
		}

		protected override bool IsMatch(FieldDefinition field, Language language)
		{
			return MatchName(field, language);
		}

		protected override bool IsMatch(PropertyDefinition property, Language language)
		{
			return MatchName(property, language);
		}

		protected override bool IsMatch(EventDefinition ev, Language language)
		{
			return MatchName(ev, language);
		}

		protected override bool IsMatch(MethodDefinition m, Language language)
		{
			return MatchName(m, language);
		}
	}
}
