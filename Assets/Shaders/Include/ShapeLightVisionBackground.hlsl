﻿#if !defined(COMBINED_SHAPE_LIGHT_PASS)
#define COMBINED_SHAPE_LIGHT_PASS

half _HDREmulationScale;
half _UseSceneLighting;
half4 _RendererColor;

half4 CombinedShapeLightShared(half4 color, half4 mask, half2 lightingUV, half2 worldUV)
{
    color = color * _RendererColor;

#if USE_SHAPE_LIGHT_TYPE_0
    half4 shapeLight0 = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, lightingUV);
    
    if (any(_ShapeLightMaskFilter0))
    {
        float4 processedMask = (1 - _ShapeLightInvertedFilter0) * mask + _ShapeLightInvertedFilter0 * (1 - mask);
        shapeLight0 *= dot(processedMask, _ShapeLightMaskFilter0);
    }

    half4 shapeLight0Modulate = shapeLight0 * _ShapeLightBlendFactors0.x;
    half4 shapeLight0Additive = shapeLight0 * _ShapeLightBlendFactors0.y;
#else
    half4 shapeLight0Modulate = 0;
    half4 shapeLight0Additive = 0;
#endif

#if USE_SHAPE_LIGHT_TYPE_1
    half4 visionMask = SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, lightingUV);

    if (any(_ShapeLightMaskFilter1))
    {
        float4 processedMask = (1 - _ShapeLightInvertedFilter1) * mask + _ShapeLightInvertedFilter1 * (1 - mask);
        visionMask *= dot(processedMask, _ShapeLightMaskFilter1);
    }
    visionMask = min(visionMask, 1);
#else
    half4 visionMask = 0;
#endif

#if USE_SHAPE_LIGHT_TYPE_2
    half4 shapeLight2 = SAMPLE_TEXTURE2D(_ShapeLightTexture2, sampler_ShapeLightTexture2, lightingUV);

    if (any(_ShapeLightMaskFilter2))
    {
        float4 processedMask = (1 - _ShapeLightInvertedFilter2) * mask + _ShapeLightInvertedFilter2 * (1 - mask);
        shapeLight2 *= dot(processedMask, _ShapeLightMaskFilter2);
    }

    half4 shapeLight2Modulate = shapeLight2 * _ShapeLightBlendFactors2.x;
    half4 shapeLight2Additive = shapeLight2 * _ShapeLightBlendFactors2.y;
#else
    half4 shapeLight2Modulate = 0;
    half4 shapeLight2Additive = 0;
#endif

#if USE_SHAPE_LIGHT_TYPE_3
    half4 shapeLight3 = SAMPLE_TEXTURE2D(_ShapeLightTexture3, sampler_ShapeLightTexture3, lightingUV);

    if (any(_ShapeLightMaskFilter3))
    {
        float4 processedMask = (1 - _ShapeLightInvertedFilter3) * mask + _ShapeLightInvertedFilter3 * (1 - mask);
        shapeLight3 *= dot(processedMask, _ShapeLightMaskFilter3);
    }

    half4 shapeLight3Modulate = shapeLight3 * _ShapeLightBlendFactors3.x;
    half4 shapeLight3Additive = shapeLight3 * _ShapeLightBlendFactors3.y;
#else
    half4 shapeLight3Modulate = 0;
    half4 shapeLight3Additive = 0;
#endif

    half4 finalOutput;
#if !USE_SHAPE_LIGHT_TYPE_0 && !USE_SHAPE_LIGHT_TYPE_1 && !USE_SHAPE_LIGHT_TYPE_2 && ! USE_SHAPE_LIGHT_TYPE_3
    finalOutput = color;
#else
    half4 finalModulate = shapeLight0Modulate + shapeLight2Modulate * visionMask + shapeLight3Modulate;
    half4 finalAdditve = shapeLight0Additive + shapeLight2Additive * visionMask + shapeLight3Additive;
    finalOutput = _HDREmulationScale * (color * finalModulate + finalAdditve);
#endif
    finalOutput.a = color.a;
    
    half4 gray = finalOutput;
    gray.r = gray.g = gray.b = dot(finalOutput, half4(0.3, 0.59, 0.11, 0)) * 0.5;
    finalOutput = finalOutput * visionMask + (1 - visionMask) * gray;

    finalOutput = finalOutput * _UseSceneLighting + (1 - _UseSceneLighting) * color;
    
    return max(0, finalOutput);
}
#endif
