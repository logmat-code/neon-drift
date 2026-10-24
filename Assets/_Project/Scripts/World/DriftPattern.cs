using System;
using System.Collections.Generic;

namespace NeonDrift.World
{
    public enum ChunkType
    {
        StraightShort,      // 24m, 0 deg (Smooth drift flick transition link, >=24m)
        StraightTraffic,    // 24m, 0 deg (Traffic Gap threading corridor)
        StraightRecovery,   // 24m, 0 deg (Speed recovery straight)
        CurveLeftSoft,      // 24m, -36 deg (Validated Soft Sweeper, R = 38.2m)
        CurveRightSoft,     // 24m, +36 deg (Validated Soft Sweeper, R = 38.2m)
        CurveLeftMedium,    // 24m, -38 deg (Validated Medium Sweeper, R = 36.2m)
        CurveRightMedium,   // 24m, +38 deg (Validated Medium Sweeper, R = 36.2m)
        TechnicalLeft,      // 22m, -40 deg (Validated Technical Apex Curve, R = 31.5m)
        TechnicalRight,     // 22m, +40 deg (Validated Technical Apex Curve, R = 31.5m)
        SweeperLeft,        // 26m, -40 deg (Validated Sustained High-Speed Drift, R = 37.2m)
        SweeperRight,       // 26m, +40 deg (Validated Sustained High-Speed Drift, R = 37.2m)
        HairpinLeft,        // 20m, -40 deg (Validated Wide Hairpin Turnaround, R = 28.6m)
        HairpinRight,       // 20m, +40 deg (Validated Wide Hairpin Turnaround, R = 28.6m)
        SCurveLR,           // 36m, compound +24 deg / -24 deg (Weight transfer S, R = 43.0m)
        SCurveRL,           // 36m, compound -24 deg / +24 deg (Weight transfer S, R = 43.0m)
        ChicaneLR,          // 28m, compound +20 deg / -20 deg (Fast flick chicane, R = 40.1m)
        ChicaneRL           // 28m, compound -20 deg / +20 deg (Fast flick chicane, R = 40.1m)
    }

    [System.Serializable]
    public struct ChunkDefinition
    {
        public ChunkType type;
        public float length;
        public float curvature;
        public int driftDirection; // -1 Left, +1 Right, 0 None
        public float trafficDensity;
        public bool isCompoundCurve;

        public ChunkDefinition(ChunkType t, float l, float c, int dir, float density, bool compound = false)
        {
            type = t;
            length = l;
            curvature = c;
            driftDirection = dir;
            trafficDensity = density;
            isCompoundCurve = compound;
        }
    }

    [System.Serializable]
    public class PatternDefinition
    {
        public string name;
        public ChunkDefinition[] chunks;
        public float baseWeight;
        public float minDifficulty; // 0.0 = start, 0.4 = mid, 0.7 = late
        public int netBias;         // -1 Left biased, +1 Right biased, 0 Balanced
        public int entryDirection;   // -1 Left, +1 Right, 0 Straight
        public int exitDirection;    // -1 Left, +1 Right, 0 Straight
        public float netHeadingDelta;

        public PatternDefinition(
            string n,
            ChunkDefinition[] c,
            float w,
            float diff,
            int bias = 0,
            int entryDir = 0,
            int exitDir = 0,
            float headingDelta = 0f)
        {
            name = n;
            chunks = c;
            baseWeight = w;
            minDifficulty = diff;
            netBias = bias;
            entryDirection = entryDir;
            exitDirection = exitDir;
            netHeadingDelta = headingDelta;
        }
    }

    /// <summary>
    /// Static library of curated self-balancing drift patterns for NEON DRIFT.
    /// Engineered around empirically validated radii (R = 28.6m–43.0m) and road width (22m) from DriftTuningGround.
    /// Delivers the canonical rhythm: DRIVE -> PREPARE -> DRIFT -> TRANSFER -> DRIFT -> RECOVERY.
    /// Strict adherence to heading envelope (+-45 deg), 24m transition straights, and net 0-deg balance.
    /// </summary>
    public static class DriftPatternLibrary
    {
        public static readonly List<PatternDefinition> Patterns = new List<PatternDefinition>
        {
            // =========================================================================
            // PATTERN A: FLOW (Medium Sweeper -> Transition -> Medium Sweeper -> Recovery)
            // =========================================================================
            new PatternDefinition("Flow_LR", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.30f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.30f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.25f)
            }, 1.3f, 0.0f, 0, -1, 1, 0f),

            new PatternDefinition("Flow_RL", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.30f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.30f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.25f)
            }, 1.3f, 0.0f, 0, 1, -1, 0f),

            // =========================================================================
            // PATTERN B: LONG DRIFT (Sustained Sweeper -> Flick Link -> Counter Sweeper)
            // =========================================================================
            new PatternDefinition("LongDrift_RL", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.SweeperRight, 26f, 40f, 1, 0.35f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.SweeperLeft, 26f, -40f, -1, 0.35f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.2f, 0.05f, 0, 1, -1, 0f),

            new PatternDefinition("LongDrift_LR", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.SweeperLeft, 26f, -40f, -1, 0.35f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.SweeperRight, 26f, 40f, 1, 0.35f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.2f, 0.05f, 0, -1, 1, 0f),

            // =========================================================================
            // PATTERN C: TRANSFER CHAIN (Triple Drift Combo: Medium -> Technical -> Recovery)
            // =========================================================================
            new PatternDefinition("TransferChain_LRL", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.TechnicalRight, 22f, 40f, 1, 0.30f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftSoft, 24f, -36f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 34f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.2f, 0.15f, 0, -1, 1, 0f),

            new PatternDefinition("TransferChain_RLR", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.TechnicalLeft, 22f, -40f, -1, 0.30f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightSoft, 24f, 36f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -34f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.2f, 0.15f, 0, 1, -1, 0f),

            // =========================================================================
            // PATTERN D: WIDE HAIRPIN (Technical Hairpin with complete counter-balance)
            // =========================================================================
            new PatternDefinition("Hairpin_Left", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.HairpinLeft, 20f, -40f, -1, 0.20f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.HairpinRight, 20f, 40f, 1, 0.20f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.0f, 0.20f, 0, -1, 1, 0f),

            new PatternDefinition("Hairpin_Right", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.HairpinRight, 20f, 40f, 1, 0.20f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.HairpinLeft, 20f, -40f, -1, 0.20f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.0f, 0.20f, 0, 1, -1, 0f),

            // =========================================================================
            // PATTERN E: S-FLOW (Soft Entry -> Fluid S-Curve -> Exit Sweeper)
            // =========================================================================
            new PatternDefinition("SFlow_LR", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.SCurveLR, 36f, 24f, 0, 0.40f, true),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.25f)
            }, 1.1f, 0.10f, 0, -1, 1, 0f),

            new PatternDefinition("SFlow_RL", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.SCurveRL, 36f, 24f, 0, 0.40f, true),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.25f)
            }, 1.1f, 0.10f, 0, 1, -1, 0f),

            // =========================================================================
            // PATTERN F: SPEED TO DRIFT (Boost Sprint -> Technical Combo)
            // =========================================================================
            new PatternDefinition("SpeedToDrift_R", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.SweeperRight, 26f, 40f, 1, 0.35f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.TechnicalLeft, 22f, -40f, -1, 0.35f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightSoft, 24f, 36f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftSoft, 24f, -36f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.1f, 0.05f, 0, 1, -1, 0f),

            new PatternDefinition("SpeedToDrift_L", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.SweeperLeft, 26f, -40f, -1, 0.35f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.TechnicalRight, 22f, 40f, 1, 0.35f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftSoft, 24f, -36f, -1, 0.25f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightSoft, 24f, 36f, 1, 0.25f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.1f, 0.05f, 0, -1, 1, 0f),

            // =========================================================================
            // PATTERN G: CHICANE SPRINT (Urban Flick -> Gap Straight -> Counter Flick)
            // =========================================================================
            new PatternDefinition("ChicaneSprint_LR", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.ChicaneLR, 28f, 20f, 0, 0.45f, true),
                new ChunkDefinition(ChunkType.StraightTraffic, 24f, 0f, 0, 0.45f),
                new ChunkDefinition(ChunkType.ChicaneRL, 28f, 20f, 0, 0.45f, true),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.30f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.30f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.0f, 0.10f, 0, 0, 0, 0f),

            new PatternDefinition("ChicaneSprint_RL", new ChunkDefinition[]
            {
                new ChunkDefinition(ChunkType.ChicaneRL, 28f, 20f, 0, 0.45f, true),
                new ChunkDefinition(ChunkType.StraightTraffic, 24f, 0f, 0, 0.45f),
                new ChunkDefinition(ChunkType.ChicaneLR, 28f, 20f, 0, 0.45f, true),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveRightMedium, 24f, 38f, 1, 0.30f),
                new ChunkDefinition(ChunkType.StraightShort, 24f, 0f, 0, 0.20f),
                new ChunkDefinition(ChunkType.CurveLeftMedium, 24f, -38f, -1, 0.30f),
                new ChunkDefinition(ChunkType.StraightRecovery, 24f, 0f, 0, 0.20f)
            }, 1.0f, 0.10f, 0, 0, 0, 0f)
        };
    }
}
