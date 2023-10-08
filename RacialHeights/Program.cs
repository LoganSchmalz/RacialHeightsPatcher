using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Runtime.CompilerServices;

namespace RacialHeights
{
    public class HeightData
    {
        public float MaleHeight { get; set; }
        public float FemaleHeight { get; set; }
        public HeightData(float mHeight, float fHeight)
        {
            MaleHeight = mHeight;
            FemaleHeight = fHeight;
        }
    }

    public class Program
    {
        static Lazy<Settings> LazySettings = new Lazy<Settings>();
        static Settings Settings => LazySettings.Value;
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings("Settings", "settings.json", out LazySettings)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            string config_file = Settings.config_file switch
            {
                Config.FKsDiverseMorrowind => "morrowind_heights.json",
                Config.RacialBodyMorphs => "rbm_heights.json",
                Config.Custom => "custom_heights.json",
                Config.Vanilla => "vanilla.json",
                _ => throw new NotImplementedException()
            };

            Dictionary<string, HeightData> config = Utils.FromJson<Dictionary<string, HeightData>>(state.RetrieveConfigFile(config_file)) ?? throw new Exception("Failed to load config");

            foreach (var item in config)
            {
                if (!state.LinkCache.TryResolve<IRaceGetter>(item.Key, out var race) || race == null || race.EditorID is null)
                {
                    Console.Out.WriteLine($"Race {item.Key} could not be resolved");
                    continue;
                }

                if (!config.ContainsKey(race.EditorID))
                {
                    Console.Out.WriteLine($"No config data defined for {race.EditorID}");
                    continue;
                }

                var newRace = race.DeepCopy();
                newRace.Height.Male = config[race.EditorID].MaleHeight;
                newRace.Height.Female = config[race.EditorID].FemaleHeight;

                state.PatchMod.Races.Set(newRace);
            }
        }
    }
}
