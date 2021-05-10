#pragma once
#include "./CloudShaderHelper.cginc"
 
float GetDensity(float3 startPos, float3 dir, float maxSampleDistance, int sample_count, float raymarchOffset, out float intensity,out float depth) {
	float sampleStart, sampleEnd;
	if (!resolve_ray_start_end(startPos, dir, sampleStart, sampleEnd) ) {
		intensity = 0.0;
		depth = 1e6;
		return 0;
	}

	sampleEnd = min(maxSampleDistance, sampleEnd);
	float sample_step = min((sampleEnd - sampleStart) / sample_count, 1000);

    float3 sampleStartPos = startPos + dir * sampleStart;
	if (
		sampleEnd <= sampleStart ||	//Something blocked behind cloud and viewer.
		sampleStartPos.y < -200) {	//Below horizon.
		intensity = 0.0;
	    depth = 1e6;
		return 0.0;
	}

	float raymarchDistance = sampleStart + raymarchOffset * sample_step;

	RaymarchStatus result;
	InitRaymarchStatus(result);

	[loop]
	for (int j = 0; j < sample_count; j++, raymarchDistance += sample_step) {
        if (raymarchDistance > maxSampleDistance){
            break;
        }
		float3 rayPos = startPos + dir * raymarchDistance;
		IntegrateRaymarch(startPos, rayPos, dir, sample_step, result);
		if (result.intTransmittance < 0.005f) {
			break;
		}
	}

	depth = result.depth / result.depthweightsum;
	if (depth == 0.0f) {
		depth = length(sampleEnd - startPos);
	}
	intensity = result.intensity;
	return (1.0f - result.intTransmittance);	
}
