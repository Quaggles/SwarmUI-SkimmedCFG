using System.IO;
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace Quaggles.Extensions.SkimmedCFG;

public class SkimmedCFGExtension : Extension
{
    private const string Prefix = "[SkimmedCFG] ";
    private const string FeatureId = "skimmedcfg";

    public List<string> Nodes =
    [
        "Skimmed CFG///Base",
        "Skimmed CFG - linear interpolation///Linear Interpolation",
        "Skimmed CFG - linear interpolation dual scales///Linear Interpolation Dual Scales",
        "Skimmed CFG - replace///Replace",
        "Skimmed CFG - Timed flip///Timed Flip",
        "Skimmed CFG - Difference CFG///Difference CFG"
    ];
    
    public Dictionary<string, HashSet<T2IParamType>> ModeUsedParams = [];

    public List<string> DifferenceMethods =
    [
        "linear_distance///Linear Distance",
        "squared_distance///Squared Distance",
        "root_distance///Root Distance",
        "absolute_sum///Absolute Sum"
    ];

    public T2IRegisteredParam<string> Mode,
        DifferenceMethod;
    
    public T2IRegisteredParam<double> SkimmingCfg,
        SkimmingCfgNegative,
        OverrideCoreCfg,
        TimedFlipAt,
        DifferenceEndAtPercentage;
    
    public T2IRegisteredParam<bool> BaseFullSkimNegative,
        BaseDisableFlippingFilter,
        TimedReverse;

    public override void OnInit()
    {
        base.OnInit();
        
        // Add the JS file, which manages the install buttons for the comfy nodes
        ScriptFiles.Add("assets/skimmedcfg.js");

        // Define required nodes
        ComfyUIBackendExtension.NodeToFeatureMap["Skimmed CFG"] = FeatureId;
        
        // Add required custom node as installable feature
        InstallableFeatures.RegisterInstallableFeature(new("Skimmed CFG", FeatureId, "https://github.com/Extraltodeus/Skimmed_CFG", "Extraltodeus", "This will install the Skimmed_CFG ComfyUI node developed by Extraltodeus.\nDo you wish to install?"));
        
        // Prevents install button from being shown during backend load if it looks like it was installed
        // it will appear if the backend loads and the backend reports it's not installed
        if (Directory.Exists(Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, $"{ComfyUIBackendExtension.Folder}/DLNodes/Skimmed_CFG")))
        {
            ComfyUIBackendExtension.FeaturesSupported.UnionWith([FeatureId]);
            ComfyUIBackendExtension.FeaturesDiscardIfNotFound.UnionWith([FeatureId]);
        }

        T2IParamGroup paramGroup = new("SkimmedCFG", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: 9);
        int orderCounter = 0;
        Mode = T2IParamTypes.Register<string>(new($"{Prefix}Mode",
            "Many tooltips are adapted from the <a href=\"https://github.com/Extraltodeus/Skimmed_CFG/blob/main/README.md\">Skimmed_CFG repository readme</a>" +
            "\n<i><b>Base:</b></i> Original version" +
            "\n<i><b>Linear Interpolation:</b></i> Instead of replacing, does a linear interpolation in between the values." +
            "\n<i><b>Linear Interpolation Dual Scales:</b></i> Uses two scales. Positive scale is \"Skimming CFG\", negative scale is \"Skimming CFG Negative\". The name is more related to a visually intuitive relation rather than fully from the predictions. A higher positive will tend to go towards high saturation and vice versa with the other slider." +
            "\n<i><b>Replace:</b></i> Replace the values within the negative by those in the positive prediction, nullifying (actually giving an equivalent scale of 1) the effect of values targeted by the filter." +
            "\n<i><b>Timed Flip:</b></i> To be used with normal scales. Enhances the randomness and overall quality of the image. A bit less of an antiburn and a lot more of an enhancer. SDE Samplers react extremely well to it." +
            "\n<i><b>Difference CFG:</b></i> Other algorithms based on changes depending on the scale. Brings back what goes too far in comparison.",
            Nodes.First(),
            GetValues: _ => Nodes,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            ChangeWeight: 1,
            OrderPriority: orderCounter++
        ));
        SkimmingCfg = T2IParamTypes.Register<double>(new($"{Prefix}Skimming CFG",
            "Basically how much do you like them burned. Recommended: 2-3 for maximum antiburn, 5-7 for colorful/strong style. 4 is cruise scale.",
            "4",
            Min: 0, Max: 10, Step: 0.5,
            ViewType: ParamViewType.SLIDER,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        OverrideCoreCfg = T2IParamTypes.Register<double>(new($"{Prefix}Override Core CFG",
            "Overrides the SwarmUI CFG parameter if enabled, useful for going beyond the default SwarmUI maximum of 20",
            "10",
            Min: 0, Max: 128, Step: 0.5,
            ViewType: ParamViewType.SLIDER,
            Group: paramGroup,
            Toggleable: true,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        BaseFullSkimNegative = T2IParamTypes.Register<bool>(new($"{Prefix}(Base) Full Skim Negative",
            "<i><b>Only works with \"Base\" Mode.</b></i> Fully skim some part of the conflicting influence.",
            "false",
            Group: paramGroup,
            IsAdvanced: true,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        BaseDisableFlippingFilter = T2IParamTypes.Register<bool>(new($"{Prefix}(Base) Disable Flipping Filter",
            "<i><b>Only works with \"Base\" Mode.</b></i> The skimming CFG will have much more control. It is meant to be used with the 'Full Skim Negative' toggle on.",
            "false",
            Group: paramGroup,
            IsAdvanced: true,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        SkimmingCfgNegative = T2IParamTypes.Register<double>(new($"{Prefix}(Dual) Skimming CFG Negative",
            "<i><b>Only works with \"Linear Interpolation Dual Scales\" Mode.</b></i>",
            "4",
            Min: 0, Max: 10, Step: 0.5,
            ViewType: ParamViewType.SLIDER,
            IsAdvanced: true,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        TimedFlipAt = T2IParamTypes.Register<double>(new($"{Prefix}(Timed) Flip At",
            "<i><b>Only works with \"Timed Flip\" Mode.</b></i>. Relative to the step progression.\\nCompletely at 0 will give smoother results\\nCompletely at one will give noisier results.\\nThe influence is more important from 0% to 30%",
            "0.3",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            IsAdvanced: true,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        TimedReverse = T2IParamTypes.Register<bool>(new($"{Prefix}(Timed) Reverse",
            "<i><b>Only works with \"Timed Flip\" Mode.</b></i>. If turned on you will obtain a composition closer to what you would normally get with no modification.",
            "false",
            IsAdvanced: true,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        DifferenceMethod = T2IParamTypes.Register<string>(new($"{Prefix}(Difference) Method",
            "<i><b>Only works with \"Difference CFG\" Mode.</b></i>.",
            "linear_distance",
            GetValues: _ => DifferenceMethods,
            IsAdvanced: true,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            ChangeWeight: 1,
            OrderPriority: orderCounter++
        ));
        DifferenceEndAtPercentage = T2IParamTypes.Register<double>(new($"{Prefix}(Difference) End At Percentage",
            "<i><b>Only works with \"Difference CFG\" Mode.</b></i>. Relative to the step progression. 0 means disabled, 1 means active until the end.",
            "0.8",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            IsAdvanced: true,
            Group: paramGroup,
            FeatureFlag: FeatureId,
            OrderPriority: orderCounter++
        ));
        
        // Keeps track of the parameters that are used by different node types
        ModeUsedParams.Add("Skimmed CFG", [
            SkimmingCfg.Type,
            BaseFullSkimNegative.Type,
            BaseDisableFlippingFilter.Type,
        ]);
        ModeUsedParams.Add("Skimmed CFG - linear interpolation", [
            SkimmingCfg.Type,
        ]);
        ModeUsedParams.Add("Skimmed CFG - linear interpolation dual scales", [
            SkimmingCfg.Type,
            SkimmingCfgNegative.Type,
        ]);
        ModeUsedParams.Add("Skimmed CFG - Timed flip", [
            TimedFlipAt.Type,
            TimedReverse.Type,
        ]);
        ModeUsedParams.Add("Skimmed CFG - Difference CFG", [
            SkimmingCfg.Type,
            DifferenceMethod.Type,
            DifferenceEndAtPercentage.Type,
        ]);
        
        WorkflowGenerator.AddModelGenStep(g =>
        {
            // Required param
            if (!g.UserInput.TryGet(Mode, out var nodeName))
                return;
            
            if (!g.Features.Contains(FeatureId))
                throw new SwarmUserErrorException("SkimmedCFG parameters specified, but feature isn't installed");
            
            // Override the core SwarmUI CFG if enabled
            if (g.UserInput.TryGet(OverrideCoreCfg, out var cfg)) 
                g.UserInput.Set(T2IParamTypes.CFGScale, cfg);

            var arguments = new JObject();
            arguments.Add("model", g.LoadingModel);
            switch (nodeName)
            {
                case "Skimmed CFG":
                    arguments.Add("Skimming_CFG", g.UserInput.Get(SkimmingCfg));
                    arguments.Add("full_skim_negative", g.UserInput.Get(BaseFullSkimNegative));
                    arguments.Add("disable_flipping_filter", g.UserInput.Get(BaseDisableFlippingFilter));
                    break;
                case "Skimmed CFG - linear interpolation":
                    arguments.Add("Skimming_CFG", g.UserInput.Get(SkimmingCfg));
                    break;
                case "Skimmed CFG - linear interpolation dual scales":
                    arguments.Add("Skimming_CFG_positive", g.UserInput.Get(SkimmingCfg));
                    arguments.Add("Skimming_CFG_negative", g.UserInput.Get(SkimmingCfgNegative));
                    break;
                case "Skimmed CFG - Timed flip":
                    arguments.Add("flip_at", g.UserInput.Get(TimedFlipAt));
                    arguments.Add("reverse", g.UserInput.Get(TimedReverse));
                    break;
                case "Skimmed CFG - Difference CFG":
                    arguments.Add("reference_CFG", g.UserInput.Get(SkimmingCfg));
                    arguments.Add("method", g.UserInput.Get(DifferenceMethod));
                    arguments.Add("end_at_percentage", g.UserInput.Get(DifferenceEndAtPercentage));
                    break;
            }
            
            // If a parameter is not used by the selected Mode remove it from the param list so it's cleaner
            if (ModeUsedParams.TryGetValue(nodeName, out var usedParams))
                foreach (var param in ModeUsedParams.SelectMany(x => x.Value))
                    if (!usedParams.Contains(param))
                        g.UserInput.ValuesInput.Remove(param.ID);

            string node = g.CreateNode(nodeName, arguments);
            g.FinalModel = [node, 0];
            g.LoadingModel = [node, 0];
        }, -13);
    }
}