using Harmony;
using ScottPlotDemos;
using ScottPlotSkia;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace WinformsSkiaDemosLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var harmony = HarmonyInstance.Create("DemosUseSkiaControlPatch");

            var original = typeof(FormPlotTypes).GetMethod("InitializeComponent", BindingFlags.NonPublic | BindingFlags.Instance);
            var transpiler = typeof(Program).GetMethod("Transpiler");

            harmony.Patch(original, null, null, new HarmonyMethod(transpiler));

            FormMain form = new FormMain();
            form.ShowDialog();
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj)
                {
                    var operand = instruction.operand as ConstructorInfo;
                    if (operand.DeclaringType.Name == "FormsPlot")
                    {
                        // replace {new FormsPlot()} with {new FormsPlotSkia()}
                        instruction.operand = typeof(FormsPlotSkia).GetConstructor(new Type[] { });
                    }
                }
                yield return instruction;
            }
        }
    }
}
