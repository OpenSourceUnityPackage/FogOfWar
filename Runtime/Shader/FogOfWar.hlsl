sampler2D _OpacFogOfWarRT;
sampler2D _TransparentFogOfWarRT;
half3 _FogOfWarColor;
half _FogOfWarTransparencyOpacity;

half2 hash( half2 p ) // replace this by something better
{
    p = half2( dot(p,half2(127.1,311.7)), dot(p,half2(269.5,183.3)) );
    return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

float noise( in half2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;

    half2  i = floor( p + (p.x+p.y)*K1 );
    half2  a = p - i + (i.x+i.y)*K2;
    float m = step(a.y,a.x); 
    half2  o = half2(m,1.0-m);
    half2  b = a - o + K2;
    half2  c = a - 1.0 + 2.0*K2;
    half3  h = max( 0.5-half3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
    half3  n = h*h*h*h*half3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
    return dot( n, half3(70.0, 70.0, 70.0) );
}

half4 ProcessFogOfWar(half2 uv)
{
    half OpacFogValue = tex2D(_OpacFogOfWarRT, uv).r;
    half TransparentFogValue = tex2D(_TransparentFogOfWarRT, uv).r;
    return half4((noise(32 * uv + _Time.x) + noise(64 * uv - _Time.y)) / 2.0 * _FogOfWarColor, 1 - (OpacFogValue + (TransparentFogValue * _FogOfWarTransparencyOpacity)));
}
