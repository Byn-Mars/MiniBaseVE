using System.Collections.Generic;
using Klei.CustomSettings;
using MiniBase.Model;
using MiniBase.Model.Enums;
using Newtonsoft.Json;
using PeterHan.PLib.Options;
using ProcGen;
using static MiniBase.Model.Profiles.MiniBaseBiomeProfiles;
using static MiniBase.Model.Profiles.MiniBaseCoreBiomeProfiles;

namespace MiniBase
{
    [ModInfo("")]
    [ConfigFile("config.json", true)]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MiniBaseOptions
    {
        #region Properties
        
        #region Worldgen
        
        [Option("Western Feature", "The geyser, vent, or volcano on the left side of the map", WorldGenCategory)]
        [JsonProperty]
        public FeatureType FeatureWest { get; set; } = FeatureType.PollutedWater;

        [Option("Eastern Feature", "The geyser, vent, or volcano on the right side of the map", WorldGenCategory)]
        [JsonProperty]
        public FeatureType FeatureEast { get; set; } = FeatureType.RandomUseful;

        [Option("Southern Feature", "The geyser, vent, or volcano at the bottom of the map\nInaccessible until the abyssalite boundary is breached", WorldGenCategory)]
        [JsonProperty]
        public FeatureType FeatureSouth { get; set; } = FeatureType.OilReservoir;

        [Option("Main Biome", "The main biome of the map\nDetermines available resources, flora, and fauna", WorldGenCategory)]
        [JsonProperty]
        public BiomeType Biome { get; set; } = BiomeType.Temperate;

        [Option("Southern Biome", "The small biome at the bottom of the map\nProtected by a layer of abyssalite", WorldGenCategory)]
        [JsonProperty]
        public CoreType CoreBiome { get; set; } = CoreType.Magma;

        [Option("Resource Density", "Modifies the density of available resources", WorldGenCategory)]
        [JsonProperty]
        public ResourceModifier ResourceMod { get; set; } = ResourceModifier.Normal;

        [Option("Space Access", "Allows renewable resources to be collected from meteorites\nDoes not significantly increase the liveable area", WorldGenCategory)]
        [JsonProperty]
        public AccessType SpaceAccess { get; set; } = AccessType.Classic;

        [Option("Tunnel Access", "Adds tunnels for access to left and right sides", WorldGenCategory)]
        [JsonProperty]
        public TunnelAccessType TunnelAccess { get; set; } = TunnelAccessType.None;
        
        #endregion
        
        #region Size options
        
        [Option("Base Width", "The width of the liveable area\nMap Size must be set to 'Custom' for this to apply", SizeCategory)]
        [Limit(20, 100)]
        [JsonProperty]
        public int BaseWidth { get; set; } = 76;

        [Option("Base Height", "The height of the liveable area\nMap Size must be set to 'Custom' for this to apply", SizeCategory)]
        [Limit(20, 100)]
        [JsonProperty]
        public int BaseHeight { get; set; } = 49;
        
        [Option("Buildable space top", "The number of buildable tiles above the neutronium layer of the starting asteroid.", SizeCategory)]
        [Limit(0, 5)]
        [JsonProperty]
        public int BuildableSpaceTop { get; set; }
        
        #endregion
               
        #region Anytime options
        
        [Option("Care Package Timer (Cycles)", "Period of care package drops, in cycles\nLower values give more frequent drops", AnytimeCategory)]
        [Limit(1, 10)]
        [JsonProperty]
        public int CarePackageFrequency { get; set; } = 2;
        
        #endregion
        
      
        #region Constants
        
        /// <summary>Thickness of the neutronium border.</summary>
        public const int BorderSize = 3;
        /// <summary>Non-colonizable margin at the top of the map.</summary>
        public const int TopMargin = 3;
        /// <summary>Margin at the top of the map that can be built in or be used to land rockets.</summary>
        public const int ColonizableExtraMargin = 8;
        public const int CornerSize = 7;
        public const int DiagonalBorderSize = 4;
        public const int SpaceAccessSize = 8;
        public const int SideAccessSize = 5;
        public const int CoreMin = 0;
        public const int CoreDeviation = 3;
        public const int CoreBorder = 3;
        
        #endregion

        #region Debug
        [JsonProperty]
        public bool DebugMode;
        [JsonProperty]
        public bool FastImmigration;
        [JsonProperty]
        public bool SkipLiveableArea;
        #endregion
        
        #region Methods

        public Vector2I GetWorldSize(Moonlet type)
        {
            if (type == Moonlet.Start)
            {
                return new Vector2I(
                    BaseWidth + (2 * BorderSize) + 36,
                    BaseHeight + (2 * BorderSize) + TopMargin + ColonizableExtraMargin);
            }
            
            return new Vector2I(
                50 + (2 * BorderSize),
                60 + (2 * BorderSize) + TopMargin + ColonizableExtraMargin);
        }

        public Vector2I GetBaseSize(Moonlet type)
        {
            if (type == Moonlet.Start)
            {
                return new Vector2I(BaseWidth, BaseHeight);
            }
            
            var worldSize = GetWorldSize(type);
            return new Vector2I(
                worldSize.x - 2 * BorderSize,
                worldSize.y - 2 * BorderSize - TopMargin - ColonizableExtraMargin);
        }

        public MiniBaseBiomeProfile GetBiome()
        {
            return _biomeTypeMap.TryGetValue(Biome, out var profile) ? profile : TemperateProfile;
        }

        public bool HasCore()
        {
            return CoreBiome != CoreType.None;
        }

        public MiniBaseBiomeProfile GetCoreBiome()
        {
            return HasCore() ? _coreTypeMap[CoreBiome] : MagmaCoreProfile;
        }

        public float GetResourceModifier()
        {
            switch(ResourceMod)
            {
                case ResourceModifier.Poor: return 0.5f;
                case ResourceModifier.Rich: return 1.5f;
                default: return 1.0f;
            }
        }


        /// <summary>
        /// Determines whether a world should be spawned and at what distance.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="distance">Distance from the cluster center at which to spawn this world.</param>
        /// <returns>True if the world should be spawned, false otherwise.</returns>

        #endregion
        
        #region Static functions
        
        public static void Reload()
        {
            _instance = POptions.ReadSettings<MiniBaseOptions>() ?? new MiniBaseOptions();
        }
        
        #endregion
        
        #region Dictionaries

        private static Dictionary<BiomeType, MiniBaseBiomeProfile> _biomeTypeMap = new Dictionary<BiomeType, MiniBaseBiomeProfile>()
        {
            { BiomeType.Temperate, TemperateProfile },
            { BiomeType.Forest, ForestProfile },
            { BiomeType.Swamp, SwampProfile },
            { BiomeType.Frozen, FrozenProfile },
            { BiomeType.Desert, DesertProfile },
            { BiomeType.Barren, BarrenProfile },
            { BiomeType.Strange, StrangeProfile },
            { BiomeType.DeepEssence, DeepEssenceProfile },
        };

        private static Dictionary<CoreType, MiniBaseBiomeProfile> _coreTypeMap = new Dictionary<CoreType, MiniBaseBiomeProfile>()
        {
            { CoreType.Magma, MagmaCoreProfile },
            { CoreType.Ocean, OceanCoreProfile },
            { CoreType.Frozen, FrozenCoreProfile },
            { CoreType.Oil, OilCoreProfile },
            { CoreType.Metal, MetalCoreProfile },
            { CoreType.Fertile, FertileCoreProfile },
            { CoreType.Boneyard, BoneyardCoreProfile },
            { CoreType.Aesthetic, AestheticCoreProfile },
            { CoreType.Pearl, PearlCoreProfile },
            { CoreType.Radioactive, RadioactiveCoreProfile },
        };
        
        public static Dictionary<FeatureType, string> GeyserDictionary = new Dictionary<FeatureType, string>()
        {
            { FeatureType.WarmWater, "GeyserGeneric_" + GeyserGenericConfig.HotWater },
            { FeatureType.SaltWater, "GeyserGeneric_" + GeyserGenericConfig.SaltWater },
            { FeatureType.SlushSaltWater, "GeyserGeneric_" + GeyserGenericConfig.SlushSaltWater },
            { FeatureType.PollutedWater, "GeyserGeneric_" + GeyserGenericConfig.FilthyWater },
            { FeatureType.CoolSlush, "GeyserGeneric_" + GeyserGenericConfig.SlushWater },
            { FeatureType.CoolSteam, "GeyserGeneric_" + GeyserGenericConfig.Steam },
            { FeatureType.HotSteam, "GeyserGeneric_" + GeyserGenericConfig.HotSteam },
            { FeatureType.NaturalGas, "GeyserGeneric_" + GeyserGenericConfig.Methane },
            { FeatureType.Hydrogen, "GeyserGeneric_" + GeyserGenericConfig.HotHydrogen },
            { FeatureType.OilFissure, "GeyserGeneric_" + GeyserGenericConfig.OilDrip },
            { FeatureType.OilReservoir, "OilWell" },
            { FeatureType.SmallVolcano, "GeyserGeneric_" + GeyserGenericConfig.SmallVolcano },
            { FeatureType.Volcano, "GeyserGeneric_" + GeyserGenericConfig.BigVolcano },
            { FeatureType.Copper, "GeyserGeneric_" + GeyserGenericConfig.MoltenCopper },
            { FeatureType.Gold, "GeyserGeneric_" + GeyserGenericConfig.MoltenGold },
            { FeatureType.Iron, "GeyserGeneric_" + GeyserGenericConfig.MoltenIron },
            { FeatureType.Cobalt, "GeyserGeneric_" + GeyserGenericConfig.MoltenCobalt },
            { FeatureType.Aluminum, "GeyserGeneric_" + GeyserGenericConfig.MoltenAluminum },
            { FeatureType.Tungsten, "GeyserGeneric_" + GeyserGenericConfig.MoltenTungsten },
            { FeatureType.Niobium, "GeyserGeneric_" + GeyserGenericConfig.MoltenNiobium },
            { FeatureType.Sulfur, "GeyserGeneric_" + GeyserGenericConfig.LiquidSulfur },
            { FeatureType.ColdCo2, "GeyserGeneric_" + GeyserGenericConfig.LiquidCO2 },
            { FeatureType.HotCo2, "GeyserGeneric_" + GeyserGenericConfig.HotCO2 },
            { FeatureType.InfectedPo2, "GeyserGeneric_" + GeyserGenericConfig.SlimyPO2 },
            { FeatureType.HotPo2, "GeyserGeneric_" + GeyserGenericConfig.HotPO2 },
            { FeatureType.Chlorine, "GeyserGeneric_" + GeyserGenericConfig.ChlorineGas },
        };

        public static FeatureType[] RandomWaterFeatures =
        {
            FeatureType.WarmWater,
            FeatureType.SaltWater,
            FeatureType.PollutedWater,
            FeatureType.CoolSlush,
            FeatureType.SlushSaltWater,
        };

        public static FeatureType[] RandomUsefulFeatures =
        {
            FeatureType.WarmWater,
            FeatureType.SaltWater,
            FeatureType.SlushSaltWater,
            FeatureType.PollutedWater,
            FeatureType.CoolSlush,
            FeatureType.CoolSteam,
            FeatureType.HotSteam,
            FeatureType.NaturalGas,
            FeatureType.Hydrogen,
            FeatureType.OilFissure,
            FeatureType.OilReservoir,
        };

        public static FeatureType[] RandomMagmaVolcanoFeatures =
        {
            FeatureType.SmallVolcano,
            FeatureType.Volcano,
        };

        public static FeatureType[] RandomVolcanoFeatures =
        {
            FeatureType.SmallVolcano,
            FeatureType.Volcano,
            FeatureType.Copper,
            FeatureType.Gold,
            FeatureType.Iron,
            FeatureType.Cobalt,
            FeatureType.Aluminum,
            FeatureType.Tungsten,
        };

        public static FeatureType[] RandomMetalVolcanoFeatures =
        {
            FeatureType.Copper,
            FeatureType.Gold,
            FeatureType.Iron,
            FeatureType.Cobalt,
            FeatureType.Aluminum,
            FeatureType.Tungsten,
        };
        
        #endregion
        
        #region Singleton implementation
        public static MiniBaseOptions Instance
        {
            get
            {
                if (_instance == null)
                {
                    Reload();   
                }
                return _instance;
            }
        }
        private static MiniBaseOptions _instance;
        #endregion
        
        #region Fields
        private const string SizeCategory = "1. Change the size of the liveable area.";
        private const string WorldGenCategory = "2. Map generation";
        private const string AnytimeCategory = "3. These options may be changed at any time";
        #endregion
    }
}
    #endregion