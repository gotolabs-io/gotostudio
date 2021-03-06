using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEditor;
using B83.ExpressionParser;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Math")]
	public class ExpressionAction : Action
	{
		public static readonly string[] alphabet = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n" };

		[HideInInspector]
		public Value[] inputs;

		public Value expression = new Value(typeof(string));
		public VariableRef output;

		private string cachedExpressionString;
		private Expression cachedExpression;
		private System.Type evaluatedOutputType;
		private int outputComponentCount;
		
		public ExpressionAction()
		{
			inputs = new Value[1];
			inputs[0] = new Value();
		}

		protected override void OnStart()
		{
			var expressionStringValue = expression.GetValue<string>(this);
				
			if(cachedExpression==null || expressionStringValue != cachedExpressionString)
				InitializeCaches(expressionStringValue);

			object[] outputComponents = new object[outputComponentCount];

			for (int c=0; c<outputComponentCount; c++)
			{
				for (int i=0; i<inputs.Length; i++)
				{
					if (cachedExpression.Parameters.ContainsKey(alphabet[i]))
					{
						System.Type type = inputs[i].GetVariable(this).Type;
						if (type == typeof(float))
							cachedExpression.Parameters[alphabet[i]].Value = inputs[i].GetValue<float>(this);
						else if (type == typeof(int))
							cachedExpression.Parameters[alphabet[i]].Value = inputs[i].GetValue<int>(this);
						else if (type == typeof(Vector2))
							cachedExpression.Parameters[alphabet[i]].Value = inputs[i].GetValue<Vector2>(this)[c];
						else if (type == typeof(Vector3))
							cachedExpression.Parameters[alphabet[i]].Value = inputs[i].GetValue<Vector3>(this)[c];
					}
				}
				if(evaluatedOutputType != typeof(int))
					outputComponents[c] = (float)cachedExpression.Value;
				else
					outputComponents[c] = (int)cachedExpression.Value;
			}

			System.Type outputVariableType = output.IsValid ? GetVariable(output.GUID).Variable.Type : null;

			if(outputVariableType == typeof(float) && evaluatedOutputType == typeof(float))
				output.SetValue((float)outputComponents[0], this);
			else if (outputVariableType == typeof(float) && evaluatedOutputType == typeof(int))
				output.SetValue((float)(int)outputComponents[0], this);
			else if (outputVariableType == typeof(int) && evaluatedOutputType == typeof(int))
				output.SetValue((int)outputComponents[0], this);
			else if (outputVariableType == typeof(int) && evaluatedOutputType == typeof(float))
				output.SetValue((int)(float)outputComponents[0], this);
			else if(evaluatedOutputType == typeof(Vector2) && outputVariableType == typeof(Vector2))
				output.SetValue(new Vector2((float)outputComponents[0], (float)outputComponents[1]), this);
			else if(evaluatedOutputType == typeof(Vector3) && outputVariableType == typeof(Vector3))
				output.SetValue(new Vector3((float)outputComponents[0], (float)outputComponents[1], (float)outputComponents[2]), this);	
			else if(output.IsValid)
				Debug.LogError("Expression type ("+ evaluatedOutputType +") and output variable type (" + outputVariableType +") do not match.");

			SetFinished();
		}

		private void InitializeCaches(string expressionStringValue)
		{
			var parser = new ExpressionParser();
			cachedExpressionString = expressionStringValue;
			cachedExpression = parser.EvaluateExpression(cachedExpressionString.ToLower());

			System.Type[] types = new System.Type[inputs.Length];
			for (int i = 0; i < inputs.Length; i++)
			{
				types[i] = inputs[i].GetVariable(this).Type;
				if (types[i] != typeof(int) && types[i] != typeof(float) && types[i] != typeof(Vector2) && types[i] != typeof(Vector3))
				{
					Debug.LogError("Expression Action only supports int, float, Vector2 and Vector3 variables!");
				}
			}

			if(types.Contains(typeof(Vector2)) && types.Contains(typeof(Vector3)))
			{
				Debug.LogError("Expression Action can't use Vector2 and Vector3 simultaneously!");
			}
			
			if(types.Contains(typeof(Vector3)))
			{
				evaluatedOutputType = typeof(Vector3);
				outputComponentCount = 3;
			}
			else if(types.Contains(typeof(Vector2)))
			{
				evaluatedOutputType = typeof(Vector2);
				outputComponentCount = 2;
			}
			else if(types.Contains(typeof(float)))
			{
				evaluatedOutputType = typeof(float);
				outputComponentCount = 1;
			}
			else
			{
				evaluatedOutputType = typeof(int);
				outputComponentCount = 1;
			}
		}
	}
}

/* * * * * * * * * * * * * *
* A simple expression parser
* --------------------------
* 
* The parser can parse a mathematical expression into a simple custom
* expression tree. It can recognise methods and fields/contants which
* are user extensible. It can also contain expression parameters which
* are registrated automatically. An expression tree can be "converted"
* into a delegate.
* 
* Written by Bunny83
* 2014-11-02
* 
* Features:
* - Elementary arithmetic [ + - * / ]
* - Power [ ^ ]
* - Brackets ( )
* - Most function from System.Math (abs, sin, round, floor, min, ...)
* - Constants ( e, PI )
* - MultiValue return (quite slow, produce extra garbage each call)
* 
* * * * * * * * * * * * * */
namespace B83.ExpressionParser
{
	public interface IValue
	{
		double Value { get; }
	}
	public class Number : IValue
	{
		private double m_Value;
		public double Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}
		public Number(double aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "" + m_Value + "";
		}
	}
	public class OperationSum : IValue
	{
		private IValue[] m_Values;
		public double Value
		{
			get { return m_Values.Select(v => v.Value).Sum(); }
		}
		public OperationSum(params IValue[] aValues)
		{
			// collapse unnecessary nested sum operations.
			List<IValue> v = new List<IValue>(aValues.Length);
			foreach (var I in aValues)
			{
				var sum = I as OperationSum;
				if (sum == null)
					v.Add(I);
				else
					v.AddRange(sum.m_Values);
			}
			m_Values = v.ToArray();
		}
		public override string ToString()
		{
			return "( " + string.Join(" + ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
		}
	}
	public class OperationProduct : IValue
	{
		private IValue[] m_Values;
		public double Value
		{
			get { return m_Values.Select(v => v.Value).Aggregate((v1, v2) => v1 * v2); }
		}
		public OperationProduct(params IValue[] aValues)
		{
			m_Values = aValues;
		}
		public override string ToString()
		{
			return "( " + string.Join(" * ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
		}

	}
	public class OperationPower : IValue
	{
		private IValue m_Value;
		private IValue m_Power;
		public double Value
		{
			get { return System.Math.Pow(m_Value.Value, m_Power.Value); }
		}
		public OperationPower(IValue aValue, IValue aPower)
		{
			m_Value = aValue;
			m_Power = aPower;
		}
		public override string ToString()
		{
			return "( " + m_Value + "^" + m_Power + " )";
		}

	}
	public class OperationNegate : IValue
	{
		private IValue m_Value;
		public double Value
		{
			get { return -m_Value.Value; }
		}
		public OperationNegate(IValue aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "( -" + m_Value + " )";
		}

	}
	public class OperationReciprocal : IValue
	{
		private IValue m_Value;
		public double Value
		{
			get { return 1.0 / m_Value.Value; }
		}
		public OperationReciprocal(IValue aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "( 1/" + m_Value + " )";
		}

	}

	public class MultiParameterList : IValue
	{
		private IValue[] m_Values;
		public IValue[] Parameters { get { return m_Values; } }
		public double Value
		{
			get { return m_Values.Select(v => v.Value).FirstOrDefault(); }
		}
		public MultiParameterList(params IValue[] aValues)
		{
			m_Values = aValues;
		}
		public override string ToString()
		{
			return string.Join(", ", m_Values.Select(v => v.ToString()).ToArray());
		}
	}

	public class CustomFunction : IValue
	{
		private IValue[] m_Params;
		private System.Func<double[], double> m_Delegate;
		private string m_Name;
		public double Value
		{
			get
			{
				if (m_Params == null)
					return m_Delegate(null);
				return m_Delegate(m_Params.Select(p => p.Value).ToArray());
			}
		}
		public CustomFunction(string aName, System.Func<double[], double> aDelegate, params IValue[] aValues)
		{
			m_Delegate = aDelegate;
			m_Params = aValues;
			m_Name = aName;
		}
		public override string ToString()
		{
			if (m_Params == null)
				return m_Name;
			return m_Name + "( " + string.Join(", ", m_Params.Select(v => v.ToString()).ToArray()) + " )";
		}
	}
	public class Parameter : Number
	{
		public string Name { get; private set; }
		public override string ToString()
		{
			return Name+"["+base.ToString()+"]";
		}
		public Parameter(string aName) : base(0)
		{
			Name = aName;
		}
	}

	public class Expression : IValue
	{
		public Dictionary<string, Parameter> Parameters = new Dictionary<string, Parameter>();
		public IValue ExpressionTree { get; set; }
		public double Value
		{
			get { return ExpressionTree.Value; }
		}
		public double[] MultiValue
		{
			get {
				var t = ExpressionTree as MultiParameterList;
				if (t != null)
				{
					double[] res = new double[t.Parameters.Length];
					for (int i = 0; i < res.Length; i++)
						res[i] = t.Parameters[i].Value;
					return res;
				}
				return null;
			}
		}
		public override string ToString()
		{
			return ExpressionTree.ToString();
		}
		public ExpressionDelegate ToDelegate(params string[] aParamOrder)
		{
			var parameters = new List<Parameter>(aParamOrder.Length);
			for(int i = 0; i < aParamOrder.Length; i++)
			{
				if (Parameters.ContainsKey(aParamOrder[i]))
					parameters.Add(Parameters[aParamOrder[i]]);
				else
					parameters.Add(null);
			}
			var parameters2 = parameters.ToArray();

			return (p) => Invoke(p, parameters2);
		}
		public MultiResultDelegate ToMultiResultDelegate(params string[] aParamOrder)
		{
			var parameters = new List<Parameter>(aParamOrder.Length);
			for (int i = 0; i < aParamOrder.Length; i++)
			{
				if (Parameters.ContainsKey(aParamOrder[i]))
					parameters.Add(Parameters[aParamOrder[i]]);
				else
					parameters.Add(null);
			}
			var parameters2 = parameters.ToArray();


			return (p) => InvokeMultiResult(p, parameters2);
		}
		double Invoke(double[] aParams, Parameter[] aParamList)
		{
			int count = System.Math.Min(aParamList.Length, aParams.Length);
			for (int i = 0; i < count; i++ )
			{
				if (aParamList[i] != null)
					aParamList[i].Value = aParams[i];
			}
			return Value;
		}
		double[] InvokeMultiResult(double[] aParams, Parameter[] aParamList)
		{
			int count = System.Math.Min(aParamList.Length, aParams.Length);
			for (int i = 0; i < count; i++)
			{
				if (aParamList[i] != null)
					aParamList[i].Value = aParams[i];
			}
			return MultiValue;
		}
		public static Expression Parse(string aExpression)
		{
			return new ExpressionParser().EvaluateExpression(aExpression);
		}

		public class ParameterException : System.Exception { public ParameterException(string aMessage) : base(aMessage) { } }
	}
	public delegate double ExpressionDelegate(params double[] aParams);
	public delegate double[] MultiResultDelegate(params double[] aParams);



	public class ExpressionParser
	{
		private List<string> m_BracketHeap = new List<string>();
		private Dictionary<string, System.Func<double>> m_Consts = new Dictionary<string, System.Func<double>>();
		private Dictionary<string, System.Func<double[], double>> m_Funcs = new Dictionary<string, System.Func<double[], double>>();
		private Expression m_Context;

		public ExpressionParser()
		{
			var rnd = new System.Random();
			m_Consts.Add("PI", () => System.Math.PI);
			m_Consts.Add("e", () => System.Math.E);
			m_Funcs.Add("sqrt", (p) => System.Math.Sqrt(p.FirstOrDefault()));
			m_Funcs.Add("abs", (p) => System.Math.Abs(p.FirstOrDefault()));
			m_Funcs.Add("ln", (p) => System.Math.Log(p.FirstOrDefault()));
			m_Funcs.Add("floor", (p) => System.Math.Floor(p.FirstOrDefault()));
			m_Funcs.Add("ceiling", (p) => System.Math.Ceiling(p.FirstOrDefault()));
			m_Funcs.Add("round", (p) => System.Math.Round(p.FirstOrDefault()));

			m_Funcs.Add("sin", (p) => System.Math.Sin(p.FirstOrDefault()));
			m_Funcs.Add("cos", (p) => System.Math.Cos(p.FirstOrDefault()));
			m_Funcs.Add("tan", (p) => System.Math.Tan(p.FirstOrDefault()));

			m_Funcs.Add("asin", (p) => System.Math.Asin(p.FirstOrDefault()));
			m_Funcs.Add("acos", (p) => System.Math.Acos(p.FirstOrDefault()));
			m_Funcs.Add("atan", (p) => System.Math.Atan(p.FirstOrDefault()));
			m_Funcs.Add("atan2", (p) => System.Math.Atan2(p.FirstOrDefault(),p.ElementAtOrDefault(1)));
			//System.Math.Floor
			m_Funcs.Add("min", (p) => System.Math.Min(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
			m_Funcs.Add("max", (p) => System.Math.Max(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
			m_Funcs.Add("rnd", (p) =>
			{
				if (p.Length == 2)
					return p[0] + rnd.NextDouble() * (p[1] - p[0]);
				if (p.Length == 1)
					return rnd.NextDouble() * p[0];
				return rnd.NextDouble();
			});
		}

		public void AddFunc(string aName, System.Func<double[],double> aMethod)
		{
			if (m_Funcs.ContainsKey(aName))
				m_Funcs[aName] = aMethod;
			else
				m_Funcs.Add(aName, aMethod);
		}

		public void AddConst(string aName, System.Func<double> aMethod)
		{
			if (m_Consts.ContainsKey(aName))
				m_Consts[aName] = aMethod;
			else
				m_Consts.Add(aName, aMethod);
		}
		public void RemoveFunc(string aName)
		{
			if (m_Funcs.ContainsKey(aName))
				m_Funcs.Remove(aName);
		}
		public void RemoveConst(string aName)
		{
			if (m_Consts.ContainsKey(aName))
				m_Consts.Remove(aName);
		}

		int FindClosingBracket(ref string aText, int aStart, char aOpen, char aClose)
		{
			int counter = 0;
			for (int i = aStart; i < aText.Length; i++)
			{
				if (aText[i] == aOpen)
					counter++;
				if (aText[i] == aClose)
					counter--;
				if (counter == 0)
					return i;
			}
			return -1;
		}

		void SubstitudeBracket(ref string aExpression, int aIndex)
		{
			int closing = FindClosingBracket(ref aExpression, aIndex, '(', ')');
			if (closing > aIndex + 1)
			{
				string inner = aExpression.Substring(aIndex + 1, closing - aIndex - 1);
				m_BracketHeap.Add(inner);
				string sub = "&" + (m_BracketHeap.Count - 1) + ";";
				aExpression = aExpression.Substring(0, aIndex) + sub + aExpression.Substring(closing + 1);
			}
			else throw new ParseException("Bracket not closed!");
		}

		IValue Parse(string aExpression)
		{
			aExpression = aExpression.Trim();
			int index = aExpression.IndexOf('(');
			while (index >= 0)
			{
				SubstitudeBracket(ref aExpression, index);
				index = aExpression.IndexOf('(');
			}
			if (aExpression.Contains(','))
			{
				string[] parts = aExpression.Split(',');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(Parse(s));
				}
				return new MultiParameterList(exp.ToArray());
			}
			else if (aExpression.Contains('+'))
			{
				string[] parts = aExpression.Split('+');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(Parse(s));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationSum(exp.ToArray());
			}
			else if (aExpression.Contains('-'))
			{
				string[] parts = aExpression.Split('-');
				List<IValue> exp = new List<IValue>(parts.Length);
				if (!string.IsNullOrEmpty(parts[0].Trim()))
					exp.Add(Parse(parts[0]));
				for (int i = 1; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(new OperationNegate(Parse(s)));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationSum(exp.ToArray());
			}
			else if (aExpression.Contains('*'))
			{
				string[] parts = aExpression.Split('*');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					exp.Add(Parse(parts[i]));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationProduct(exp.ToArray());
			}
			else if (aExpression.Contains('/'))
			{
				string[] parts = aExpression.Split('/');
				List<IValue> exp = new List<IValue>(parts.Length);
				if (!string.IsNullOrEmpty(parts[0].Trim()))
					exp.Add(Parse(parts[0]));
				for (int i = 1; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(new OperationReciprocal(Parse(s)));
				}
				return new OperationProduct(exp.ToArray());
			}
			else if (aExpression.Contains('^'))
			{
				int pos = aExpression.IndexOf('^');
				var val = Parse(aExpression.Substring(0, pos));
				var pow = Parse(aExpression.Substring(pos + 1));
				return new OperationPower(val, pow);
			}
			int pPos = aExpression.IndexOf("&");
			if (pPos > 0)
			{
				string fName = aExpression.Substring(0, pPos);
				foreach (var M in m_Funcs)
				{
					if (fName == M.Key)
					{
						var inner = aExpression.Substring(M.Key.Length);
						var param = Parse(inner);
						var multiParams = param as MultiParameterList;
						IValue[] parameters;
						if (multiParams != null)
							parameters = multiParams.Parameters;
						else
							parameters = new IValue[] { param };
						return new CustomFunction(M.Key, M.Value, parameters);
					}
				}
			}
			foreach (var C in m_Consts)
			{
				if (aExpression == C.Key)
				{
					return new CustomFunction(C.Key,(p)=>C.Value(),null);
				}
			}
			int index2a = aExpression.IndexOf('&');
			int index2b = aExpression.IndexOf(';');
			if (index2a >= 0 && index2b >= 2)
			{
				var inner = aExpression.Substring(index2a + 1, index2b - index2a - 1);
				int bracketIndex;
				if (int.TryParse(inner, out bracketIndex) && bracketIndex >= 0 && bracketIndex < m_BracketHeap.Count)
				{
					return Parse(m_BracketHeap[bracketIndex]);
				}
				else
					throw new ParseException("Can't parse substitude token");
			}
			double doubleValue;
			if (double.TryParse(aExpression, out doubleValue))
			{
				return new Number(doubleValue);
			}
			if (ValidIdentifier(aExpression))
			{
				if (m_Context.Parameters.ContainsKey(aExpression))
					return m_Context.Parameters[aExpression];
				var val = new Parameter(aExpression);
				m_Context.Parameters.Add(aExpression, val);
				return val;
			}

			throw new ParseException("Reached unexpected end within the parsing tree");
		}

		private bool ValidIdentifier(string aExpression)
		{
			aExpression = aExpression.Trim();
			if (string.IsNullOrEmpty(aExpression))
				return false;
			if (aExpression.Length < 1)
				return false;
			if (aExpression.Contains(" "))
				return false;
			if (!"abcdefghijklmnopqrstuvwxyz§$".Contains(char.ToLower(aExpression[0])))
				return false;
			if (m_Consts.ContainsKey(aExpression))
				return false;
			if (m_Funcs.ContainsKey(aExpression))
				return false;
			return true;
		}

		public Expression EvaluateExpression(string aExpression)
		{
			var val = new Expression();
			m_Context = val;
			val.ExpressionTree = Parse(aExpression);
			m_Context = null;
			m_BracketHeap.Clear();
			return val;
		}

		public double Evaluate(string aExpression)
		{
			return EvaluateExpression(aExpression).Value;
		}
		public static double Eval(string aExpression)
		{
			return new ExpressionParser().Evaluate(aExpression);
		}

		public class ParseException : System.Exception { public ParseException(string aMessage) : base(aMessage) { } }
	}
}
