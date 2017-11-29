﻿using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Extensions;
using Telerik.JustDecompiler.Ast.Expressions;
using Telerik.JustDecompiler.Ast.Statements;
using Telerik.JustDecompiler.Ast;
using Telerik.JustDecompiler.Languages;

namespace Telerik.JustDecompiler.Decompiler
{
    class AutoImplementedEventMatcher
    {
        private readonly EventDefinition eventDef;
        private FieldDefinition eventField;
        private ILanguage language;

        public AutoImplementedEventMatcher(EventDefinition eventDef, ILanguage language)
        {
            this.eventDef = eventDef;
            this.language = language;
        }

        public bool IsAutoImplemented(out FieldDefinition eventField)
        {
            eventField = GetField(eventDef);
            return eventField != null;
        }

        public bool IsAutoImplemented()
        {
            FieldDefinition fieldDef;
            return IsAutoImplemented(out fieldDef);
        }

        private FieldDefinition GetField(EventDefinition eventDef)
        {
            if (eventDef.InvokeMethod != null || eventDef.AddMethod == null || eventDef.RemoveMethod == null)
            {
                return null;
            }

            FieldDefinition eventField = GetFieldWithName(eventDef.DeclaringType, eventDef.EventType.FullName, eventDef.Name + "Event");
            if (eventField == null)
            {
				eventField = GetFieldWithName(eventDef.DeclaringType, eventDef.EventType.FullName, eventDef.Name);
            }

			if (eventField == null)
			{
				return null;
			}

			if (IsThreadUnsafeEvent(eventField) || IsThreadSafeEvent(eventField))
			{
				return eventField;
			}

            return null;
        }

        private static FieldDefinition GetFieldWithName(TypeDefinition typeDef, string eventTypeFullName, string name)
        {
            foreach (FieldDefinition fieldDef in typeDef.Fields)
            {
                if (fieldDef.Name == name && fieldDef.FieldType.FullName == eventTypeFullName)
                {
                    return fieldDef;
                }
            }

            return null;
        }

        private bool IsThreadUnsafeEvent(FieldDefinition eventField)
        {
            this.eventField = eventField;
			return IsThreadUnsafeOperation(eventDef.AddMethod, "Combine") &&
				IsThreadUnsafeOperation(eventDef.RemoveMethod, "Remove");
        }
  
        /// <remarks>
        /// this.eventField = (EventHandlerType)Delegate.Combine/Remove(this.eventField, value);
        /// return;
        /// </remarks>
		private bool IsThreadUnsafeOperation(MethodDefinition methodDef, string operationName)
		{
			BlockStatement methodBlockStatement;
			if (!CheckMethodAndDecompile(methodDef, out methodBlockStatement))
			{
				return false;
			}

			if (methodBlockStatement.Statements.Count != 2 || methodBlockStatement.Statements[1].CodeNodeType != CodeNodeType.ExpressionStatement ||
				(methodBlockStatement.Statements[1] as ExpressionStatement).Expression.CodeNodeType != CodeNodeType.ReturnExpression)
			{
				return false;
			}

			Expression resultExpression;
			Expression argumentExpression;
			if (!IsDelegateOperationStatement(methodBlockStatement.Statements[0], operationName, out resultExpression, out argumentExpression))
			{
				return false;
			}

			if (resultExpression.CodeNodeType != CodeNodeType.FieldReferenceExpression ||
				(resultExpression as FieldReferenceExpression).Field.Resolve() != eventField ||
				argumentExpression.CodeNodeType != CodeNodeType.FieldReferenceExpression ||
				(argumentExpression as FieldReferenceExpression).Field.Resolve() != eventField)
			{
				return false;
			}

			return true;
		}

        private bool CheckMethodAndDecompile(MethodDefinition methodDef, out BlockStatement methodBody)
        {
            if (!methodDef.HasParameters ||
                methodDef.Parameters.Count != 1)
            {
                methodBody = null;
                return false;
            }

            DecompilationPipeline pipeline = BaseLanguage.IntermediateRepresenationPipeline;
            pipeline.Run(methodDef.Body, this.language);
            methodBody = pipeline.Body;
            return true;
        }

        private bool IsThreadSafeEvent(FieldDefinition eventField)
        {
            this.eventField = eventField;
			return IsThreadSafeAutoImplOperation(eventDef.AddMethod, "Combine")
				&& IsThreadSafeAutoImplOperation(eventDef.RemoveMethod, "Remove");
        }

        /// <summary>
        /// Checks whether the event accessor is generated by the C# compiler
        /// </summary>
        /// <param name="methodDef"></param>
        /// <param name="operationName"></param>
        /// <returns></returns>
		//private bool CheckCSAutoImplOperation(MethodDefinition methodDef, string operationName)
		//{
		//    //if (methodDef.Module.Runtime == TargetRuntime.Net_2_0)
		//    //{
		//    //    // PresentationFramework v3.5
		//    //    return IsThreadUnsafeOperation(methodDef, operationName);
		//    //}
		//    return IsThreadSafeAutoImplOperation(methodDef, operationName);
		//}

		/// <remarks>
		/// V_0 = this.event_field;
		/// do
		/// {
		///     V_1 = V_0;
		///     V_2 = (EventHandlerType)Delegate.Combine/Remove(V_1, value);
		///     V_0 = Interlocked.CompareExchange<EventHandlerType>(ref this.event_field, V_2, V_1);
		/// }
		/// while (V_0 != V_1);
		/// return;
		/// </remarks>
		private bool IsThreadSafeAutoImplOperation(MethodDefinition methodDef, string operationName)
		{
			BlockStatement methodBlockStatement;
			if (!CheckMethodAndDecompile(methodDef, out methodBlockStatement) || methodBlockStatement.Statements.Count != 3)
			{
				return false;
			}

			VariableReference v0Variable = null;
			VariableReference v1Variable = null;
              
			if (!methodBlockStatement.Statements[0].IsAssignmentStatement())
			{
				return false;
			}

			BinaryExpression v0DeclarationAssignExpression = (methodBlockStatement.Statements[0] as ExpressionStatement).Expression as BinaryExpression;
			if (v0DeclarationAssignExpression.Left.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
				v0DeclarationAssignExpression.Right.CodeNodeType != CodeNodeType.FieldReferenceExpression ||
				(v0DeclarationAssignExpression.Right as FieldReferenceExpression).Field.Resolve() != eventField)
			{
				return false;
			}

			v0Variable = (v0DeclarationAssignExpression.Left as VariableReferenceExpression).Variable;

			if (methodBlockStatement.Statements[1].CodeNodeType != CodeNodeType.DoWhileStatement ||
				methodBlockStatement.Statements[2].CodeNodeType != CodeNodeType.ExpressionStatement ||
				(methodBlockStatement.Statements[2] as ExpressionStatement).Expression.CodeNodeType != CodeNodeType.ReturnExpression)
			{
				return false;
			}

			DoWhileStatement theDoWhileStatement = methodBlockStatement.Statements[1] as DoWhileStatement;

            Expression condition = theDoWhileStatement.Condition;
            if (condition.CodeNodeType == CodeNodeType.UnaryExpression && (condition as UnaryExpression).Operator == UnaryOperator.None)
            {
                condition = (condition as UnaryExpression).Operand;
            }

			if (condition.CodeNodeType != CodeNodeType.BinaryExpression)
			{
                return false;
			}

            BinaryExpression loopCondition = condition as BinaryExpression;
            CastExpression leftCast = loopCondition.Left as CastExpression;
            CastExpression rightCast = loopCondition.Right as CastExpression;
            if (loopCondition.Operator != BinaryOperator.ValueInequality ||
                leftCast == null ||
                leftCast.TargetType.Name != "Object" ||
                leftCast.Expression.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                rightCast == null ||
                rightCast.Expression.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                rightCast.TargetType.Name != "Object")
            {
                return false;
            }

            if ((leftCast.Expression as VariableReferenceExpression).Variable != v0Variable)
            {
                return false;
            }

            v1Variable = (rightCast.Expression as VariableReferenceExpression).Variable;

			return CheckLoopBody(theDoWhileStatement.Body, v0Variable, v1Variable, operationName);
		}

        private bool CheckLoopBody(BlockStatement loopBody, VariableReference v0Variable, VariableReference v1Variable, string operationName)
        {
            if (loopBody.Statements.Count != 3)
            {
                return false;
            }

            if (!loopBody.Statements[0].IsAssignmentStatement())
            {
                return false;
            }

            BinaryExpression v0Tov1Assign = (loopBody.Statements[0] as ExpressionStatement).Expression as BinaryExpression;
            if (v0Tov1Assign.Left.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                v0Tov1Assign.Right.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                (v0Tov1Assign.Right as VariableReferenceExpression).Variable != v0Variable)
            {
                return false;
            }

            if ((v0Tov1Assign.Left as VariableReferenceExpression).Variable != v1Variable)
            {
                return false;
            }

            Expression result;
            Expression argument;
            if (!IsDelegateOperationStatement(loopBody.Statements[1], operationName, out result, out argument) ||
                result.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                argument.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                (argument as VariableReferenceExpression).Variable != v1Variable)
            {
                return false;
            }

            VariableReference v2Variable = (result as VariableReferenceExpression).Variable;

            if (!loopBody.Statements[2].IsAssignmentStatement())
            {
                return false;
            }

            BinaryExpression exchangeAssignExpression = (loopBody.Statements[2] as ExpressionStatement).Expression as BinaryExpression;
            if (exchangeAssignExpression.Left.CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                (exchangeAssignExpression.Left as VariableReferenceExpression).Variable != v0Variable ||
                exchangeAssignExpression.Right.CodeNodeType != CodeNodeType.MethodInvocationExpression)
            {
                return false;
            }

            MethodInvocationExpression exchangeInvokeExpression = exchangeAssignExpression.Right as MethodInvocationExpression;
            if (exchangeInvokeExpression.MethodExpression.Method.DeclaringType.FullName != "System.Threading.Interlocked" ||
                exchangeInvokeExpression.MethodExpression.Method.HasThis ||
                exchangeInvokeExpression.MethodExpression.Method.Name != "CompareExchange" ||
                exchangeInvokeExpression.Arguments.Count != 3 ||
                exchangeInvokeExpression.Arguments[0].CodeNodeType != CodeNodeType.UnaryExpression)
            {
                return false;
            }

            UnaryExpression byRefArgument = exchangeInvokeExpression.Arguments[0] as UnaryExpression;
            if (byRefArgument.Operator != UnaryOperator.AddressReference ||
                byRefArgument.Operand.CodeNodeType != CodeNodeType.FieldReferenceExpression ||
                (byRefArgument.Operand as FieldReferenceExpression).Field.Resolve() != eventField)
            {
                return false;
            }

            if (exchangeInvokeExpression.Arguments[1].CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                (exchangeInvokeExpression.Arguments[1] as VariableReferenceExpression).Variable != v2Variable ||
                exchangeInvokeExpression.Arguments[2].CodeNodeType != CodeNodeType.VariableReferenceExpression ||
                (exchangeInvokeExpression.Arguments[2] as VariableReferenceExpression).Variable != v1Variable)
            {
                return false;
            }

            return true;
        }

        private bool IsDelegateOperationStatement(Statement statement, string operationName,
            out Expression newValueHolder, out Expression oldValueHolder)
        {
            newValueHolder = null;
            oldValueHolder = null;

            if (!statement.IsAssignmentStatement())
            {
                return false;
            }

            BinaryExpression assignExpression = (statement as ExpressionStatement).Expression as BinaryExpression;
            if (assignExpression.Right.CodeNodeType != CodeNodeType.CastExpression ||
                (assignExpression.Right as CastExpression).Expression.CodeNodeType != CodeNodeType.MethodInvocationExpression)
            {
                return false;
            }

            MethodInvocationExpression methodInvokeExpr = (assignExpression.Right as CastExpression).Expression as MethodInvocationExpression;
            if (methodInvokeExpr.Arguments.Count != 2 || methodInvokeExpr.MethodExpression.Method.HasThis ||
                methodInvokeExpr.MethodExpression.Method.DeclaringType.FullName != "System.Delegate" || methodInvokeExpr.MethodExpression.Method.Name != operationName)
            {
                return false;
            }

            if (methodInvokeExpr.Arguments[1].CodeNodeType != CodeNodeType.ArgumentReferenceExpression)
            {
                return false;
            }

            newValueHolder = assignExpression.Left;
            oldValueHolder = methodInvokeExpr.Arguments[0];
            return true;
        }
    }
}