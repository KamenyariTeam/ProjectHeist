﻿#ifndef COMBINED_SHAPE_LIGHT_PASS
#define COMBINED_SHAPE_LIGHT_PASS

#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"

half _HDREmulationScale;
half _UseSceneLighting;

half4 CombinedShapeLightShared(in SurfaceData2D surfaceData, in InputData2D inputData)
{
    #if defined(DEBUG_DISPLAY)
    half4 debugColor = 0;

    if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
    {
        return debugColor;
    }
    #endif

    half alpha = surfaceData.alpha;
    half4 color = half4(surfaceData.albedo, alpha);
    const half4 mask = surfaceData.mask;
    const half2 lightingUV = inputData.lightingUV;

    if (alpha == 0.0)
        discard;

#if USE_SHAPE_LIGHT_TYPE_0
    half4 shapeLight0 = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, lightingUV);

    if (any(_ShapeLightMaskFilter0))
    {
        half4 processedMask = (1 - _ShapeLightInvertedFilter0) * mask + _ShapeLightInvertedFilter0 * (1 - mask);
        shapeLight0 *= dot(processedMask, _ShapeLightMaskFilter0);
    }

    half4 shapeLight0Modulate = shapeLight0 * _ShapeLightBlendFactors0.x;
    half4 shapeLight0Additive = shapeLight0 * _ShapeLightBlendFactors0.y;
#else
    half4 shapeLight0Modulate = 0;
    half4 shapeLight0Additive = 0;
#endif

#if USE_SHAPE_LIGHT_TYPE_2
    half4 shapeLight2 = SAMPLE_TEXTURE2D(_ShapeLightTexture2, sampler_ShapeLightTexture2, lightingUV);

    if (any(_ShapeLightMaskFilter2))
    {
        half4 processedMask = (1 - _ShapeLightInvertedFilter2) * mask + _ShapeLightInvertedFilter2 * (1 - mask);
        shapeLight2 *= dot(processedMask, _ShapeLightMaskFilter2);
    }

    half4 shapeLight2Modulate = shapeLight2 * _ShapeLightBlendFactors2.x;
    half4 shapeLight2Additive = shapeLight2 * _ShapeLightBlendFactors2.y;
#else
    half4 shapeLight2Modulate = 0;
    half4 shapeLight2Additive = 0;
#endif

    half4 finalOutput;
#if !USE_SHAPE_LIGHT_TYPE_0 && !USE_SHAPE_LIGHT_TYPE_2
    finalOutput = color;
#else
    half4 finalModulate = shapeLight0Modulate;
    half4 finalAdditve = shapeLight0Additive;
    finalOutput = _HDREmulationScale * (color * finalModulate + finalAdditve);
#endif

    finalOutput = lerp(color, finalOutput, _UseSceneLighting);

    return max(0, finalOutput);
}
#endif
