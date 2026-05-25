using Klei.CustomSettings;
using MiniBase.Model.Enums;
using MiniBase.Model.Profiles;
using ProcGen;
using ProcGenGame;

namespace MiniBase.Model
{
    public class MoonletData
    {
        #region Properties

        /// <summary>Total size of the map in tiles.</summary>
        public Vector2I WorldSize { get; }

        /// <summary>The type of mini moonlet of this instance.</summary>
        internal Moonlet Type { get; }

        /// <summary>Main moonlet biome.</summary>
        public MiniBaseBiomeProfile Biome { get; }

        /// <summary>Bottom moonlet biome.</summary>
        public MiniBaseBiomeProfile CoreBiome { get; }

        /// <summary>Moonlet has a core biome or just the main biome.</summary>
        public bool HasCore { get; }

        #endregion

        public MoonletData(WorldGen worldGen)
        {
            var options = MiniBaseOptions.Instance;
            switch (worldGen.Settings.world.filePath)
            {
                case VanillaStartMap:
                    Type = Moonlet.Start;
                    Biome = options.GetBiome();
                    CoreBiome = options.GetCoreBiome();
                    HasCore = options.HasCore();
                    _extraTopMargin = options.BuildableSpaceTop;
                    break;
            }

            WorldSize = worldGen.WorldSize;
            _baseSize = MiniBaseOptions.Instance.GetBaseSize(Type);
        }

        #region Methods

        /// <summary>
        /// The leftmost tile position that is considered inside the liveable area or its borders.
        /// Tiles in the liveable area are part of the main biome.
        /// </summary>
        /// <param name="withBorders"></param>
        /// <returns></returns>
        public int Left(bool withBorders = false)
        {
            return SideMargin() + (withBorders ? 0 : MiniBaseOptions.BorderSize);
        }

        /// <summary>
        /// The rightmost tile position that is considered inside the liveable area or its borders.
        /// </summary>
        /// <param name="withBorders"></param>
        /// <returns></returns>
        public int Right(bool withBorders = false)
        {
            return Left(withBorders) + _baseSize.x + (withBorders ? MiniBaseOptions.BorderSize * 2 : 0);
        }

        public int Top(bool withBorders = false)
        {
            return WorldSize.y - MiniBaseOptions.TopMargin - _extraTopMargin -
                (withBorders ? 0 : MiniBaseOptions.BorderSize) + 1;
        }

        public int Bottom(bool withBorders = false)
        {
            return Top(withBorders) - _baseSize.y - (withBorders ? MiniBaseOptions.BorderSize * 2 : 0);
        }

        public int Width(bool withBorders = false)
        {
            return Right(withBorders) - Left(withBorders);
        }

        public int Height(bool withBorders = false)
        {
            return Top(withBorders) - Bottom(withBorders);
        }

        public Vector2I TopLeft(bool withBorders = false)
        {
            return new Vector2I(Left(withBorders), Top(withBorders));
        }

        public Vector2I TopRight(bool withBorders = false)
        {
            return new Vector2I(Right(withBorders), Top(withBorders));
        }

        public Vector2I BottomLeft(bool withBorders = false)
        {
            return new Vector2I(Left(withBorders), Bottom(withBorders));
        }

        public Vector2I BottomRight(bool withBorders = false)
        {
            return new Vector2I(Right(withBorders), Bottom(withBorders));
        }

        public bool InLiveableArea(Vector2I pos)
        {
            return pos.x >= Left() && pos.x < Right() && pos.y >= Bottom() && pos.y < Top();
        }
        
        /// <summary>
        /// Width of the vacuum area to the side of the base, outside of the borders. These areas are
        /// accessible via the side tunnels if enabled.
        /// </summary>
        /// <returns></returns>
        private int SideMargin()
        {
            return (WorldSize.x - _baseSize.x - 2 * MiniBaseOptions.BorderSize) / 2;
        }

        #endregion

        #region Static Functions

        /// <summary>
        /// Determines if a <see cref="WorldGen"/> instance represents a minibase map.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsMiniBaseWorld(WorldGen instance) =>
            instance.Settings.world.filePath == VanillaStartMap;

        /// <summary>
        /// Determines if a <see cref="WorldGen"/> instance represents a natural minibase map.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>

        /// <summary>
        /// If the starting asteroid is worlds/MiniBase we consider the playthrough
        /// to be a minibase one (for Cluster Generaton Manager compatibility).
        /// </summary>
        /// <returns></returns>
        public static bool IsMiniBaseCluster()
        {
            var clusterCache = SettingsCache.clusterLayouts.clusterCache;
            var world = clusterCache[
                    CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.ClusterLayout).id]
                .GetStartWorld();
            return world == VanillaStartMap;
        }

        #endregion

        #region Fields

        /// <summary>Additional margin at the top for landing rockets</summary>
        private readonly int _extraTopMargin;

        /// <summary>Size of the map where a borders and biomes will spawn.</summary>
        private readonly Vector2I _baseSize;

        #endregion

        #region Constants

        internal const string MiniBaseCluster = "clusters/MiniBase";
        internal const string VanillaStartMap = "worlds/MiniBase";

        #endregion
    }
}