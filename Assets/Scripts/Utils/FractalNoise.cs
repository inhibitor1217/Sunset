using UnityEngine;

public static class FractalNoise
{
    public enum NoiseType
    {
        Block = 0, Linear = 1, Spline = 2
    };
    
    private const float HASH_MAX = 255f;
    private const int HASH_MASK = 0x7F;
    private static int[] hash = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};

    public static float[,] GetValues(
        int resolution, NoiseType noiseType,
        Vector2 offset, Vector2 scale, float rotation,
        int complexity,
        float subInfluence, Vector2 subOffset, float subScale, float subRotation 
    )
    {
        float[,] sum = new float[resolution, resolution], level;

        for (int subLevel = complexity - 1; subLevel >= 0; subLevel--)
        {
            level = generateLevel(
                resolution, 
                noiseType, 
                offset + subOffset * subLevel, 
                scale * Mathf.Pow(subScale, subLevel),
                rotation + subLevel * subRotation
            );

            if (subLevel == complexity - 1)
                sum = level;
            else
                sum = mixLevels(level, sum, subInfluence);
        }

        return sum;
    }

    static float getHashInterpolate(Vector2 pos, NoiseType noiseType)
    {
        int ix = Mathf.FloorToInt(pos.x);
        int iy = Mathf.FloorToInt(pos.y);

        switch (noiseType)
        {
        case NoiseType.Block:
            return getHashValue(ix, iy);
        case NoiseType.Linear:
            return Mathf.Lerp(
                Mathf.Lerp(getHashValue(ix, iy), getHashValue(ix + 1, iy), pos.x - ix),
                Mathf.Lerp(getHashValue(ix, iy + 1), getHashValue(ix + 1, iy + 1), pos.x - ix),
                pos.y - iy
            );
        case NoiseType.Spline:
            return spline(
                pos.y - iy,
                spline(pos.x - ix, getHashValue(ix-1, iy-1), getHashValue(ix, iy-1), getHashValue(ix+1, iy-1), getHashValue(ix+2, iy-1)),
                spline(pos.x - ix, getHashValue(ix-1, iy  ), getHashValue(ix, iy  ), getHashValue(ix+1, iy  ), getHashValue(ix+2, iy  )),
                spline(pos.x - ix, getHashValue(ix-1, iy+1), getHashValue(ix, iy+1), getHashValue(ix+1, iy+1), getHashValue(ix+2, iy+1)),
                spline(pos.x - ix, getHashValue(ix-1, iy+2), getHashValue(ix, iy+2), getHashValue(ix+1, iy+2), getHashValue(ix+2, iy+2))
            );
        }

        return 0;
    }

    static float getHashValue(int ix, int iy)
    {
        return hash[mod(hash[mod(ix)] + mod(iy))] / HASH_MAX;
    }

    static float spline(float t, float y0, float y1, float y2, float y3)
    {
        float a =             2f * y1;
		float b = -      y0           +      y2;
		float c =   2f * y0 - 5f * y1 + 4f * y2 - y3;
		float d = -      y0 + 3f * y1 - 3f * y2 + y3;

		return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    static int mod(int idx) { return idx >= 0 ? idx & HASH_MASK : HASH_MASK - (~idx) & HASH_MASK; }
    static Vector2 rotate(Vector2 dir, float angle)
    {
        return new Vector2(
            Mathf.Cos(angle) * dir.x - Mathf.Sin(angle) * dir.y,
            Mathf.Sin(angle) * dir.x + Mathf.Cos(angle) * dir.y
        );
    }

    public static float[,] generateLevel(
        int resolution, NoiseType noiseType,
        Vector2 offset, Vector2 scale, float rotation
    )
    {
        float[,] level = new float[resolution, resolution];

        float rotationRadians = rotation * Mathf.Deg2Rad;

        Vector2 point00 = offset;
        Vector2 point11 = offset + rotate(scale, rotationRadians);
        Vector2 point01 = offset + rotate(scale.y * Vector2.up, rotationRadians);
        Vector2 point10 = offset + rotate(scale.x * Vector2.right, rotationRadians);

        for (int x = 0; x < resolution; x++)
        {
            Vector2 point0 = Vector2.Lerp(point00, point10, x * 1f / resolution);
            Vector2 point1 = Vector2.Lerp(point01, point11, x * 1f / resolution);

            for (int y = 0; y < resolution; y++)
            {
                level[x, y] = getHashInterpolate(
                    Vector2.Lerp(point0, point1, y * 1f / resolution),
                    noiseType
                );
            }
        }

        return level;
    }

    static float[,] mixLevels(float[,] curLevel, float[,] subLevel, float subInfluence)
    {
        float[,] mixed = new float[curLevel.GetLength(0), curLevel.GetLength(1)];

        for (int x = 0; x < mixed.GetLength(0); x++)
        {
            for (int y = 0; y < mixed.GetLength(1); y++)
            {
                mixed[x, y] = Mathf.Lerp(curLevel[x, y], subLevel[x, y], subInfluence);
            }
        }

        return mixed;
    }


}
