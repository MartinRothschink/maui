﻿using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC;

class PointTypeConverter : ICompiledTypeConverter
{
	public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
	{
		var module = context.Body.Method.Module;
		do
		{
			if (string.IsNullOrEmpty(value))
				break;
			if (Point.TryParse(value.Trim(), out var point))
			{
				yield return Instruction.Create(OpCodes.Ldc_R8, point.X);
				yield return Instruction.Create(OpCodes.Ldc_R8, point.Y);
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics", "Point"), parameterTypes: new[] {
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double")}));
				yield break;
			}
		} while (false);
		throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Point));
	}
}
