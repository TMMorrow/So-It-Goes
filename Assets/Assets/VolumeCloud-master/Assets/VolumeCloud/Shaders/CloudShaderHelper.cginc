#include "UnityCG.cginc"

#define EARTH_RADIUS 6371000.0
#define EARTH_CENTER float3(0, -EARTH_RADIUS, 0)

#define TRANSMITTANCE_SAMPLE_STEP 512.0f

static const float bayerOffsets[3][3] = {
	{0, 7, 3},
	{6, 5, 2},
	{4, 1, 8}
};

//Base shape
float _CloudStartHeight;
float _CloudEndHeight;
sampler3D _BaseTex;
float _BaseTile;
sampler2D _HeightDensity;
//Detal shape
sampler3D _DetailTex;
float _DetailTile;
float _DetailStrength;
//Curl distortion
sampler2D _CurlNoise;
float _CurlTile;
float _CurlStrength;
//Top offset
float _CloudTopOffset;

//Overall cloud size.
float _CloudSize;
//Overall Density
float _CloudOverallDensity;
float _CloudCoverageModifier;
float _CloudTypeModifier;

half4 _WindDirection;
sampler2D _WeatherTex;
float _WeatherTexSize;

//Lighting
float _ScatteringCoefficient;
float _ExtinctionCoefficient;
float _SilverIntensity;
float _SilverSpread;

float SampleDensity(float3 worldPos, int lod, bool cheap, out float wetness);

float Remap(float original_value, float original_min, float original_max, float new_min, float new_max)
{
	return new_min + (((original_value - original_min) / (original_max - original_min)) * (new_max - new_min));
}

float RemapClamped(float original_value, float original_min, float original_max, float new_min, float new_max)
{
	return new_min + (saturate((original_value - original_min) / (original_max - original_min)) * (new_max - new_min));
}

float HeightPercent(float3 worldPos) {
	float sqrMag = worldPos.x * worldPos.x + worldPos.z * worldPos.z;

	float heightOffset = EARTH_RADIUS - sqrt(max(0.0, EARTH_RADIUS * EARTH_RADIUS - sqrMag));

	return saturate((worldPos.y + heightOffset - _CloudStartHeight) / (_CloudEndHeight-_CloudStartHeight));
}

static float4 cloudGradients[3] = {
	float4(0, 0.07, 0.08, 0.15),
	float4(0, 0.2, 0.42, 0.6),
	float4(0, 0.08, 0.75, 1)
};

float SampleHeight(float heightPercent,float cloudType) {
	float4 gradient;
	float cloudTypeVal;
	if (cloudType < 0.5) {
		gradient = lerp(cloudGradients[0], cloudGradients[1], cloudType*2.0);
	}
	else {
		gradient = lerp(cloudGradients[1], cloudGradients[2], (cloudType - 0.5)*2.0);
	} 

	return RemapClamped(heightPercent, gradient.x, gradient.y, 0.0, 1.0)
			* RemapClamped(heightPercent, gradient.z, gradient.w, 1.0, 0.0);
}

float3 ApplyWind(float3 worldPos) {
	float heightPercent = HeightPercent(worldPos);
	
	// skew in wind direction
	worldPos.xz -= (heightPercent) * _WindDirection.xy * _CloudTopOffset;

	//animate clouds in wind direction and add a small upward bias to the wind direction
	worldPos.xz -= (_WindDirection.xy + float3(0.0, 0.1, 0.0)) * _Time.y * _WindDirection.z;
	worldPos.y -= _WindDirection.z * 0.4 * _Time.y;
	return worldPos;
}

float HenryGreenstein(float g, float cosTheta) {

	float k = 3.0 / (8.0 * 3.1415926f) * (1.0 - g * g) / (2.0 + g * g);
	return k * (1.0 + cosTheta * cosTheta) / pow(abs(1.0 + g * g - 2.0 * g * cosTheta), 1.5);
}

float4 ProcessBaseTex(float4 texSample) {
	texSample = saturate(texSample - 0.3f) / 0.7f;
	float low_freq_fBm = (texSample.g * .625) + (texSample.b * 0.25) + (texSample.a * 0.125);
	float sampleResult = RemapClamped(low_freq_fBm, -0.3f * texSample.r, 1.0, 0.0, 1.0);

	return min(1.0f, sampleResult * 2.0f);
}

float ApplyCoverageToDensity(float sampleResult, float coverage){
	//sampleResult = RemapClamped(sampleResult, 1.0 - coverage, 1.0, 0.0, 1.0);
	//sampleResult *= coverage;
	sampleResult -= (1.0f - coverage);
	return max(0.0f, sampleResult);
}

float SampleDensity(float3 worldPos,int lod, bool cheap, out float wetness) {
	//Store the pos without wind applied.
	float3 unwindWorldPos = worldPos;
	
	//Sample the weather map.
	float4 coverageSampleUV = float4((unwindWorldPos.xz / _WeatherTexSize), 0, 0);
	coverageSampleUV.xy = (coverageSampleUV.xy + 0.5);
	float3 weatherData = tex2Dlod(_WeatherTex, coverageSampleUV);
	weatherData *= float3(_CloudCoverageModifier, 1.0, _CloudTypeModifier);
	float cloudCoverage = weatherData.r;
	float cloudType = weatherData.b;
	wetness = weatherData.g;

	//Calculate the normalized height between[0,1]
	float heightPercent = HeightPercent(worldPos);
	if (heightPercent <= 0.0f || heightPercent >= 1.0f)
		return 0.0;

	//Sample base noise.
	worldPos = ApplyWind(worldPos);
	float4 tempResult = tex3Dlod(_BaseTex, float4(worldPos / _CloudSize * _BaseTile, lod)).rgba;
	float sampleResult = ProcessBaseTex(tempResult);

	//Sample Height-Density map.
	float2 densityAndErodeness = tex2Dlod(_HeightDensity, float4(cloudType, heightPercent, 0.0, 0.0)).rg;

	sampleResult *= densityAndErodeness.x;
	//Clip the result using coverage map.
	sampleResult = ApplyCoverageToDensity(sampleResult, cloudCoverage);

	if (!cheap) {
		float2 curl_noise = tex2Dlod(_CurlNoise, float4(unwindWorldPos.xz / _CloudSize * _CurlTile, 0.0, 1.0)).rg;
		worldPos.xz += curl_noise.rg * (1.0 - heightPercent) * _CloudSize * _CurlStrength;

		float3 tempResult2;
		tempResult2 = tex3Dlod(_DetailTex, float4(worldPos / _CloudSize * _DetailTile, lod)).rgb;
		float detailsampleResult = (tempResult2.r * 0.625) + (tempResult2.g * 0.25) + (tempResult2.b * 0.125);
		//Detail sample result here is worley-perlin fbm.

		//On cloud marked with low erodness, we see cauliflower style, so when doing erodness, we use 1.0f - detail.
		//On cloud marked with high erodness, we see thin line style, so when doing erodness we use detail.
		float detail_modifier = lerp(1.0f - detailsampleResult, detailsampleResult, densityAndErodeness.y);
		sampleResult = RemapClamped(sampleResult, min(0.8, (1.0f - detailsampleResult) * _DetailStrength), 1.0, 0.0, 1.0);
	} else {
		sampleResult = RemapClamped(sampleResult, min(0.8, _DetailStrength * 0.5f), 1.0, 0.0, 1.0);
	}

	//sampleResult = pow(sampleResult, 1.2);
	return max(0, sampleResult) * _CloudOverallDensity;
}

float _MultiScatteringA;
float _MultiScatteringB;
float _MultiScatteringC;

//We raymarch to sun using length of pattern 1,2,4,8, corresponding to step value.
//First sample(length 1) should sample at length 0.5, meaning an average inside length 1.
//Second sample should sample at 1.5, meaning an average inside [1, 2],
//Third should sample at 3.0, which is [2, 4]
//Forth at 6.0, meaning [4, 8]
static const float shadowSampleDistance[5] = {
	0.5, 1.5, 3.0, 6.0, 12.0
};

static const float shadowSampleContribution[5] = {
	1.0f, 1.0f, 2.0f, 4.0f, 8.0f
};

float SampleOpticsDistanceToSun(float3 worldPos) {
	int mipmapOffset = 0.5;
	float opticsDistance = 0.0f;
	[unroll]
	for (int i = 0; i < 5; i++) {
		half3 direction = _WorldSpaceLightPos0;
		float3 samplePoint = worldPos + direction * shadowSampleDistance[i] * TRANSMITTANCE_SAMPLE_STEP;
		float wetness;
		float sampleResult = SampleDensity(samplePoint, mipmapOffset, true, wetness);
		opticsDistance += shadowSampleContribution[i] * TRANSMITTANCE_SAMPLE_STEP * sampleResult;
		mipmapOffset += 0.5;
	}
	return opticsDistance;
}

float SampleEnergy(float3 worldPos, float3 viewDir) {
	float opticsDistance = SampleOpticsDistanceToSun(worldPos);
	float result = 0.0f;
	float cosTheta = dot(viewDir, _WorldSpaceLightPos0);
	[unroll]
	for (int octaveIndex = 0; octaveIndex < 2; octaveIndex++) {	//Multi scattering approximation from Frostbite.
		float transmittance = exp(-_ExtinctionCoefficient * pow(_MultiScatteringB, octaveIndex) * opticsDistance);
		float ecMult = pow(_MultiScatteringC, octaveIndex);
		float phase = lerp(HenryGreenstein(.1f * ecMult, cosTheta), HenryGreenstein((0.99 - _SilverSpread) * ecMult, cosTheta), 0.5f);
		result += phase * transmittance * _ScatteringCoefficient * pow(_MultiScatteringA, octaveIndex);
	}
	return result;
}

//Code from https://area.autodesk.com/blogs/game-dev-blog/volumetric-clouds/.
bool ray_trace_sphere(float3 center, float3 rd, float3 offset, float radius, out float t1, out float t2) {
	float3 p = center - offset;
	float b = dot(p, rd);
	float c = dot(p, p) - (radius * radius);

	float f = b * b - c;
	if (f >= 0.0) {
		float dem = sqrt(f);
		t1 = -b - dem;
		t2 = -b + dem;
		return true;
	}
	return false;
}

bool resolve_ray_start_end(float3 ws_origin, float3 ws_ray, out float start, out float end) {
	//case includes on ground, inside atm, above atm.
	float ot1, ot2, it1, it2;
	bool outIntersected = ray_trace_sphere(ws_origin, ws_ray, EARTH_CENTER, EARTH_RADIUS + _CloudEndHeight, ot1, ot2);
	if (!outIntersected || ot2 < 0.0f)
		return false;	//you see nothing.

	bool inIntersected = ray_trace_sphere(ws_origin, ws_ray, EARTH_CENTER, EARTH_RADIUS + _CloudStartHeight, it1, it2);
	
	if (inIntersected) {
		if (it1 * it2 < 0) {
			//we're on ground.
			start = max(it2, 0);
			end = ot2;
		}
		else {
			//we're inside atm, or above atm.
			if (ot1 * ot2 < 0) {		//Inside atm.
				if (it1 > 0.0) {
					//Look down.
					end = it1;
				}
				else {
					//Look up.
					end = ot2;
				}
				start = 0.0f;
			} else {			//Outside atm
				if (ot1 < 0.0) {
					return false;
				}
				else {
					start = ot1;
					end = it1;
				}
			}
		}
	}
	else {
		end = ot2;
		start = max(ot1, 0);
	}
	return true;
}

struct RaymarchStatus {
	float intensity;
	float depth;
	float depthweightsum;
	float intTransmittance;
};

void InitRaymarchStatus(inout RaymarchStatus result){
	result.intTransmittance = 1.0f;
	result.intensity = 0.0f;
	result.depthweightsum = 0.00001f;
	result.depth = 0.0f;
}

void IntegrateRaymarch(float3 startPos, float3 rayPos, float3 viewdir, float stepsize, inout RaymarchStatus result){
	float wetness;
	float density = SampleDensity(rayPos, 0, false, wetness);
	if (density <= 0.0f)
		return;
	float extinction = _ExtinctionCoefficient * density;

	float clampedExtinction = max(extinction, 1e-7);
	float transmittance = exp(-extinction * stepsize);
			
	float luminance = SampleEnergy(rayPos, viewdir) * lerp(1.0f, 0.3f, wetness);
	float integScatt = (luminance - luminance * transmittance) / clampedExtinction;
	float depthWeight = result.intTransmittance;		//Is it a better idead to use (1-transmittance) * intTransmittance as depth weight?

	result.intensity += result.intTransmittance * integScatt;
	result.depth += depthWeight * length(rayPos - startPos);
	result.depthweightsum += depthWeight;
	result.intTransmittance *= transmittance;
}