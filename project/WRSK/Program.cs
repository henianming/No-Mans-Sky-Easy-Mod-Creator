using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Newtonsoft.Json;

using NLua;

public static class Program {
	private static readonly Lua _lua = new Lua();

	private static void Main(string[] args) {
		try {
			_lua.State.Encoding = Encoding.UTF8;
			_lua.LoadCLRPackage();
			_lua.DoFile("lua/Main.lua");
		} catch (Exception e) {
			Console.WriteLine(e.Message);
		}

		Console.WriteLine("-----------------------------------------------");
		Console.WriteLine("End");
		Console.ReadKey();
	}

	public static object[] DoString(string chunk) {
		return _lua.DoString(chunk);
	}

	public static void InitDirectory(string path) {
		if (Directory.Exists(path) == true) {
			Directory.Delete(path, true);
		}

		Directory.CreateDirectory(path);
	}

	public static void GetFile(
		string rawRootPath,
		string targetRootPath,
		string subPath
	) {
		FileInfo fiRaw = new FileInfo($"{rawRootPath}/{subPath}");
		FileInfo fiTarget = new FileInfo($"{targetRootPath}/{subPath}");
		if (fiRaw.Exists == false) {
			Console.WriteLine($"原始文件不存在{fiRaw}");
		}

		if (Directory.Exists(fiTarget.DirectoryName) == false) {
			Directory.CreateDirectory(fiTarget.DirectoryName);
		}

		File.Copy(fiRaw.FullName, fiTarget.FullName);
	}

	public static string GetPackFileListStr(string path) {
		string result = "";

		DirectoryInfo di = new DirectoryInfo(path);
		FileInfo[] fiArr = di.GetFiles();
		foreach (FileInfo fiSub in fiArr) {
			result += $"{fiSub.FullName} ";
		}

		DirectoryInfo[] diArr = di.GetDirectories();
		foreach (DirectoryInfo diSub in diArr) {
			result += $"{diSub.FullName} ";
		}

		return result;
	}

	public static void Sleep(float sec) {
		Thread.Sleep((int)(sec * 1000));
	}

	public static void Log(string content, ConsoleColor color) {
		Console.ForegroundColor = color;
		Console.WriteLine(content);
		Console.ForegroundColor = ConsoleColor.White;
	}

	public static string[] Split(this string host, params string[] splitStr) {
		return host.Split(splitStr, StringSplitOptions.None);
	}

	public static string V(this XmlElement host, string setValue = null) {
		if (setValue == null) {
			return host.GetAttribute("value");
		} else {
			host.SetAttribute("value", setValue);
			return null;
		}
	}

	public static XmlElement[] N(this XmlElement host, string selectorAll) {
		string[] selectorAllArr = selectorAll.Split("/");
		List<XmlElement> resultList = new List<XmlElement>();
		NOne(resultList, host, selectorAllArr, 0);
		return resultList.ToArray();
	}

	public static XmlElement[] NSub(this XmlElement host, string selectorAll) {
		string[] selectorAllArr = selectorAll.Split("\\");
		List<XmlElement> resultList = new List<XmlElement>();
		NOne(resultList, host, selectorAllArr, 0);
		return resultList.ToArray();
	}

	private static void NOne(
		List<XmlElement> output,
		XmlElement host,
		string[] selectorAll,
		int selectorIdx
	) {
		string curSelector = selectorAll[selectorIdx];
		string[] selectorElementArr = curSelector.Split("&");

		List<XmlElement> result = new List<XmlElement>();

		bool isExpressionAvaible = IsExpressionAvaible(host, selectorElementArr);

		if (isExpressionAvaible == false) {
			return;
		}

		if (selectorElementArr[0] == "*") {
			//所有
			foreach (XmlElement child in host.ChildNodes) {
				result.Add(child);
			}
		} else if (selectorElementArr[0] == "!") {
			//没有name属性的
			foreach (XmlElement child in host.ChildNodes) {
				string name = child.GetAttribute("name");
				if (name == "") {
					result.Add(child);
				}
			}
		} else if (selectorElementArr[0] == "..") {
			//上一级
			NOne(output, host.ParentNode as XmlElement, selectorAll, selectorIdx + 1);
		} else if (selectorElementArr[0].StartsWith("@") == true) {
			//正则表达式
			string regexStr = selectorElementArr[0].Substring(1);
			foreach (XmlElement child in host.ChildNodes) {
				string name = child.GetAttribute("name");
				Match match = Regex.Match(name, regexStr);
				if (match.Success == true) {
					result.Add(child);
				}
			}
		} else {
			//全词匹配
			foreach (XmlElement child in host.ChildNodes) {
				string name = child.GetAttribute("name");
				if (name == selectorElementArr[0]) {
					result.Add(child);
				}
			}
		}

		if (selectorAll.Length - 1 == selectorIdx) {
			output.AddRange(result);
		} else {
			foreach (XmlElement resultElement in result) {
				NOne(output, resultElement, selectorAll, selectorIdx + 1);
			}
		}
	}

	public static bool IsExpressionAvaible(XmlElement node, string[] expressionArr) {
		if (expressionArr.Length == 1) {
			return true;
		}

		bool isAvaible = true;

		for (int i = 1; i < expressionArr.Length; i++) {
			if (IsExpressionAvaibleGroup(node, expressionArr[i]) == false) {
				isAvaible = false;
			}
		}

		return isAvaible;
	}

	public static bool IsExpressionAvaibleGroup(XmlElement node, string expressionGroup) {
		string[] expressionArr = expressionGroup.Split("|");

		bool isAvaible = false;

		foreach (string expression in expressionArr) {
			if (IsExpressionAvaibleOne(node, expression) == true) {
				isAvaible = true;
			}
		}

		return isAvaible;
	}

	public static bool IsExpressionAvaibleOne(XmlElement node, string expression) {
		if (CheckE(node, expression) == 1) {
			return true;
		} else if (CheckE(node, expression) == 0) {
			return false;
		}

		if (CheckNE(node, expression) == 1) {
			return true;
		} else if (CheckNE(node, expression) == 0) {
			return false;
		}

		if (CheckB(node, expression) == 1) {
			return true;
		} else if (CheckB(node, expression) == 0) {
			return false;
		}

		if (CheckBE(node, expression) == 1) {
			return true;
		} else if (CheckBE(node, expression) == 0) {
			return false;
		}

		if (CheckS(node, expression) == 1) {
			return true;
		} else if (CheckS(node, expression) == 0) {
			return false;
		}

		if (CheckSE(node, expression) == 1) {
			return true;
		} else if (CheckSE(node, expression) == 0) {
			return false;
		}

		Log($"未识别表达式{expression}", ConsoleColor.Red);

		return false;
	}

	public static int CheckE(XmlElement node, string expression) {
		Match match = Regex.Match(expression, "^(.+?)==(.+?)$");
		if (match.Success == false) {
			return -1;
		}

		string key = match.Groups[1].Value;
		string value = match.Groups[2].Value;
		return node.NSub(key)[0].V() == value ? 1 : 0;
	}

	public static int CheckNE(XmlElement node, string expression) {
		Match match = Regex.Match(expression, "^(.+?)~=(.+?)$");
		if (match.Success == false) {
			return -1;
		}

		string key = match.Groups[1].Value;
		string value = match.Groups[2].Value;
		return node.NSub(key)[0].V() != value ? 1 : 0;
	}

	public static int CheckB(XmlElement node, string expression) {
		Match match = Regex.Match(expression, "^(.+?)>(.+?)$");
		if (match.Success == false) {
			return -1;
		}

		string key = match.Groups[1].Value;
		string value = match.Groups[2].Value;

		double numData = double.Parse(node.NSub(key)[0].V());
		double numEx = double.Parse(value);

		return numData > numEx ? 1 : 0;
	}

	public static int CheckBE(XmlElement node, string expression) {
		Match match = Regex.Match(expression, "^(.+?)>=(.+?)$");
		if (match.Success == false) {
			return -1;
		}

		string key = match.Groups[1].Value;
		string value = match.Groups[2].Value;

		double numData = double.Parse(node.NSub(key)[0].V());
		double numEx = double.Parse(value);

		return numData >= numEx ? 1 : 0;
	}

	public static int CheckS(XmlElement node, string expression) {
		Match match = Regex.Match(expression, "^(.+?)<(.+?)$");
		if (match.Success == false) {
			return -1;
		}

		string key = match.Groups[1].Value;
		string value = match.Groups[2].Value;

		double numData = double.Parse(node.NSub(key)[0].V());
		double numEx = double.Parse(value);

		return numData < numEx ? 1 : 0;
	}

	public static int CheckSE(XmlElement node, string expression) {
		Match match = Regex.Match(expression, "^(.+?)<=(.+?)$");
		if (match.Success == false) {
			return -1;
		}

		string key = match.Groups[1].Value;
		string value = match.Groups[2].Value;

		double numData = double.Parse(node.NSub(key)[0].V());
		double numEx = double.Parse(value);

		return numData <= numEx ? 1 : 0;
	}

	public static void DoModify(string path, string json) {
		path = path.ToUpper();
		path = path.Replace(".MBIN", "");
		path += ".EXML";

		XmlDocument xml = new XmlDocument();
		xml.Load(path);
		XmlElement root = xml.DocumentElement;

		ModifyStruct modifyArr = JsonConvert.DeserializeObject<ModifyStruct>(json);
		XmlElement[] targetArr = root.N(modifyArr.selector);
		foreach (XmlElement target in targetArr) {
			target.V(modifyArr.value);
		}

		xml.Save(path);

		Console.WriteLine($"{new FileInfo(path).FullName}({modifyArr.selector})--->{targetArr.Length}处修改");
	}

	public static void DoModifyF(string path, string json, LuaFunction valueFunction) {
		path = path.ToUpper();
		path = path.Replace(".MBIN", "");
		path += ".EXML";

		XmlDocument xml = new XmlDocument();
		xml.Load(path);
		XmlElement root = xml.DocumentElement;

		ModifyStruct modifyArr = JsonConvert.DeserializeObject<ModifyStruct>(json);
		XmlElement[] targetArr = root.N(modifyArr.selector);
		foreach (XmlElement target in targetArr) {
			string value = valueFunction.Call(target)[0] as string;
			target.V(value);
		}

		xml.Save(path);

		Console.WriteLine($"{new FileInfo(path).FullName}({modifyArr.selector})--->{targetArr.Length}处修改");
	}
}

public class ModifyStruct {
	public string selector;
	public string value;
}
