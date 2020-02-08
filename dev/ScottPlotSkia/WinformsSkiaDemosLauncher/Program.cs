using Harmony;
using ScottPlotDemos;
using ScottPlotSkia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace WinformsSkiaDemosLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var harmony = HarmonyInstance.Create("DemosUseSkiaControlPatch");

            var demosAssembly = typeof(FormMain).GetTypeInfo().Assembly;
            var formsInDemos = demosAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Form)));

            var transpiler = typeof(Program).GetMethod("Transpiler");

            foreach (var fType in formsInDemos)
            {
                var original = fType.GetMethod("InitializeComponent", BindingFlags.NonPublic | BindingFlags.Instance);
                harmony.Patch(original, null, null, new HarmonyMethod(transpiler));
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
