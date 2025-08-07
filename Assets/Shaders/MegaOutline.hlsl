float2 Texel;

TEXTURE2D(_CameraDepthNormalsTexture);
SAMPLER(sampler_CameraDepthNormalsTexture);





float ns_DecodeFloatRG(float2 enc) {
    float2 kDecodeDot = float2(1.0, 1 / 255.0);
    return dot(enc, kDecodeDot);
}


float checkSame(float2 centerNormal, float centerDepth, float4 theSample, float sensitivityNormals, float sensitivityDepth) {
    float2 diff = abs(centerNormal - theSample.xy) * sensitivityNormals;
    int isSameNormal = (diff.x + diff.y) < 0.1;
    float sampleDepth = ns_DecodeFloatRG(theSample.zw);
    float zdiff = abs(centerDepth - sampleDepth);
    int isSameDepth = zdiff * sensitivityDepth < 0.99 * centerDepth;
    return (isSameNormal * isSameDepth) ? 1.0 : 0.0;
}


//Currently passing in a texture, we want to sample the albedo value maybe then we can have nicer shaders, materials etc..
// Also want to pass in Gradient Top and Bottom Colors
void OutlineObject_float(float4 ScreenPos, float2 Texel, float SampleDistance, float SensitivityNormals, float SensitivityDepth, out float4 Outline, out float4 Inline ) {
    float sampleSizeX = Texel.x;
    float sampleSizeY = Texel.y;
    float2 screenUV = ScreenPos.xy;

    float2 _uv2 = screenUV + float2(-sampleSizeX, +sampleSizeY) * SampleDistance;
    float2 _uv3 = screenUV + float2(+sampleSizeX, -sampleSizeY) * SampleDistance;
    float2 _uv4 = screenUV + float2(sampleSizeX, sampleSizeY) * SampleDistance;
    float2 _uv5 = screenUV + float2(-sampleSizeX, -sampleSizeY) * SampleDistance;

    float4 center = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, screenUV);
    float4 sample1 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, _uv2);
    float4 sample2 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, _uv3);
    float4 sample3 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, _uv4);
    float4 sample4 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, _uv5);

    float edge = 1.0;

    float2 centerNormal = center.xy;
    float centerDepth = ns_DecodeFloatRG(center.zw);

    //float d = clamp(centerDepth * Falloff - 0.05, 0.0, 1.0);
    //float4 depthFade = float4(d, d, d, 1.0);

    edge *= checkSame(centerNormal, centerDepth, sample1, SensitivityNormals, SensitivityDepth);
    edge *= checkSame(centerNormal, centerDepth, sample2, SensitivityNormals, SensitivityDepth);
    edge *= checkSame(centerNormal, centerDepth, sample3, SensitivityNormals, SensitivityDepth);
    edge *= checkSame(centerNormal, centerDepth, sample4, SensitivityNormals, SensitivityDepth);

    if (edge > 0) {
        //Out = edge * lerp(GradientA, GradientB, screenUV.x);

        Inline = float4(0, 0, 0, 0);
        Outline = float4(1.0, 1.0, 1.0, 1.0); //SCreendimesniosn are actually object positions
    }
    else {
        //Out = edge * Color + (1.0 - edge) * (depthFade * Color);
        Inline = float4(1.0, 1.0, 1.0, 1.0);
        Outline = float4(0, 0, 0, 0);
    }
}



/*

/////// HEREHERE
    v2f vertRobert( appdata_img v, out float4 outpos : SV_POSITION )
    {
        v2f o;
        outpos = UnityObjectToClipPos(v.vertex);

        //o.vertex = mult(UNITY_MATRIX_MVP, v.vertex);
        //o.screenPos2 = ComputeScreenPos(o.vertex);


        float2 uv = v.texcoord.xy;
        o.uv[0] = uv;
        #if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            uv.y = 1-uv.y; //colour not here.. but this shits things kind of..
        #endif

        // calc coord for the X pattern
        // maybe nicer TODO for the future: 'rotated triangles'

        //colours not in here?
        o.uv[1] = uv + _MainTex_TexelSize.xy * half2(1,1) * _SampleDistance;
        o.uv[2] = uv + _MainTex_TexelSize.xy * half2(-1,-1) * _SampleDistance;
        o.uv[3] = uv + _MainTex_TexelSize.xy * half2(-1,1) * _SampleDistance;
        o.uv[4] = uv + _MainTex_TexelSize.xy * half2(1,-1) * _SampleDistance;
        return o;
    } */

/*

half4 fragRobert(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target {
        half4 sample1 = tex2D(_CameraDepthNormalsTexture, i.uv[1].xy);
        half4 sample2 = tex2D(_CameraDepthNormalsTexture, i.uv[2].xy);
        half4 sample3 = tex2D(_CameraDepthNormalsTexture, i.uv[3].xy);
        half4 sample4 = tex2D(_CameraDepthNormalsTexture, i.uv[4].xy);

        half edge = 1.0;

        edge *= CheckSame(sample1.xy, DecodeFloatRG(sample1.zw), sample2);
        edge *= CheckSame(sample3.xy, DecodeFloatRG(sample3.zw), sample4);


        half4 finalColor = lerp(_GradientBot, _GradientTop,  screenPos.x / _GradientHorizon );
   

         if(edge > 0)
             return lerp(tex2D(_MainTex, i.uv[0].xy), _BgColor, _BgFade);
             //return finalColor;
         else
             //return _BgColor;
             return finalColor;
    }*/