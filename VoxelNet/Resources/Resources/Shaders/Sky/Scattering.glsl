float pow2(in float n)
{
    return n * n;
}
float pow3(in float n)
{
    return pow2(n) * n;
}
float pow4(in float n)
{
    return pow2(pow2(n));
}
float pow5(in float n)
{
    return pow2(pow2(n)) * n;
}
float pow6(in float n)
{
    return pow2(pow2(n) * n);
}
float pow7(in float n)
{
    return pow2(pow2(n) * n) * n;
}
float pow8(in float n)
{
    return pow2(pow2(pow2(n)));
}
float pow9(in float n)
{
    return pow2(pow2(pow2(n))) * n;
}
float pow10(in float n)
{
    return pow2(pow2(pow2(n)) * n);
}
float pow11(in float n)
{
    return pow2(pow2(pow2(n)) * n) * n;
}
float pow12(in float n)
{
    return pow2(pow2(pow2(n) * n));
}
float pow13(in float n)
{
    return pow2(pow2(pow2(n) * n)) * n;
}
float pow14(in float n)
{
    return pow2(pow2(pow2(n) * n) * n);
}
float pow15(in float n)
{
    return pow2(pow2(pow2(n) * n) * n) * n;
}
float pow16(in float n)
{
    return pow2(pow2(pow2(pow2(n))));
}

#define max0(n) max(0.0, n)
#define min1(n) min(1.0, n)
#define clamp01(n) clamp(n, 0.0, 1.0)

#define Gamma 0.80
#define IntensityMax 0.25

#define Pi 3.14159265358979

#define steps 8
#define steps_inv 2.0 / steps

float getLuma(vec3 colour)
{
    return dot(colour, vec3(0.2125, 0.7154, 0.0721));
}

vec3 colourSaturation(vec3 colour, float saturation)
{
    return colour * saturation + getLuma(colour) * (1.0 - saturation);
}

#define DIRECT_MOONLIGHT_SATURATION 0.0

#define moonLight colourSaturation(vec3(0.0, 0.0, 1.0), DIRECT_MOONLIGHT_SATURATION) * 0.5

#define atmosphereHeight 8000
#define earthRadius 637100
#define mieMultiplier 5
#define ozoneMultiplier 1     // 1. for physically based 
#define rayleighDistribution 8 //physically based 
#define mieDistribution 1.8     //physically based 
#define rayleighCoefficient vec3(5.8e-6,1.35e-5,3.31e-5) // Physically based (Bruneton, Neyret)
#define ozoneCoefficient (vec3(3.426,8.298,.356) * 6e-5 / 100.) // Physically based (Kutz)
#define mieCoefficient ( 3e-6 * mieMultiplier) //good default

#define upVector vec3(0,1,0)

#define phaseRayleigh(a) ( .4 * (a) + 1.14 )
#define getEarth(a) pow(smoothstep(-.1,.1,dot(upVector,a)),1.)

float phaseMie(float x)
{
    const vec3 c = vec3(.256098, .132268, .010016);
    const vec3 d = vec3(-1.5, -1.74, -1.98);
    const vec3 e = vec3(1.5625, 1.7569, 1.9801);
    return dot((x * x + 1.) * c / pow(d * x + e, vec3(2.1, 2.4, 2.6)), vec3(.33333333333, .33333333333, .33333333333));
}

vec3 WavelengthToRGB(float Wavelength)
{
    float factor;
    float Red, Green, Blue;

    if ((Wavelength >= 380) && (Wavelength < 440))
    {
        Red = -(Wavelength - 440) / (440 - 380);
        Green = 0.0;
        Blue = 1.0;
    }
    else if ((Wavelength >= 440) && (Wavelength < 490))
    {
        Red = 0.0;
        Green = (Wavelength - 440) / (490 - 440);
        Blue = 1.0;
    }
    else if ((Wavelength >= 490) && (Wavelength < 510))
    {
        Red = 0.0;
        Green = 1.0;
        Blue = -(Wavelength - 510) / (510 - 490);
    }
    else if ((Wavelength >= 510) && (Wavelength < 580))
    {
        Red = (Wavelength - 510) / (580 - 510);
        Green = 1.0;
        Blue = 0.0;
    }
    else if ((Wavelength >= 580) && (Wavelength < 645))
    {
        Red = 1.0;
        Green = -(Wavelength - 645) / (645 - 580);
        Blue = 0.0;
    }
    else if ((Wavelength >= 645) && (Wavelength < 781))
    {
        Red = 1.0;
        Green = 0.0;
        Blue = 0.0;
    }
    else
    {
        Red = 0.0;
        Green = 0.0;
        Blue = 0.0;
    }

    // Let the intensity fall off near the vision limits

    if ((Wavelength >= 380) && (Wavelength < 420))
    {
        factor = 0.3 + 0.7 * (Wavelength - 380) / (420 - 380);
    }
    else if ((Wavelength >= 420) && (Wavelength < 701))
    {
        factor = 1.0;
    }
    else if ((Wavelength >= 701) && (Wavelength < 781))
    {
        factor = 0.3 + 0.7 * (780 - Wavelength) / (780 - 700);
    }
    else
    {
        factor = 0.0;
    }

    // Don't want 0^x = 1 for x <> 0
    //float r = Red == 0.0 ? 0 : ((int) round(IntensityMax * pow(Red * factor, Gamma)) / 255);
    //float g = Green == 0.0 ? 0 : ((int) round(IntensityMax * pow(Green * factor, Gamma)) / 255);
    //float b = Blue == 0.0 ? 0 : ((int) round(IntensityMax * pow(Blue * factor, Gamma)) / 255);

    return vec3(Red, Green, Blue);
}

vec2 GetThickness(vec3 rd)
{
    vec2 sr = earthRadius + vec2(atmosphereHeight, atmosphereHeight * mieDistribution / rayleighDistribution);

    vec3 ro = -upVector * earthRadius;
    float b = dot(rd, ro);
    return b + sqrt(sr * sr + (b * b - dot(ro, ro)));
}

vec3 Absorb(vec2 a)
{
    return exp(-(a).x * (ozoneCoefficient * ozoneMultiplier + rayleighCoefficient) - 1.11 * (a).y * mieCoefficient);
}

float shdwcrv(float x)
{
    x = clamp(x, 0., 1.);
    x = -x * x + 1.;
    x = -x * x + 1.;
    x = -x * x + 1.;
    x = -x * x + 1.;
    return x;
}

float sphSoftShadow(vec3 position, vec3 L)
{
    const float k = 10.;
    //vec4 sph = vec4(-up*earthRadius,earthRadius);
    vec3 oc = position + upVector * earthRadius;
    float b = dot(oc, L);
    float c = dot(oc, oc) - earthRadius * earthRadius;
    float h = b * b - c;
    
    float d = -earthRadius + sqrt(max(0.0, earthRadius * earthRadius - h));
    float t = -b - sqrt(max(0.0, h));
    //return (t < 0.0) ? 1.0 : shdwcrv(k * d / t);
    if(t < 0.0)
        return 1.0;
    else
        return shdwcrv(k * d / t);
}

vec3 GetSkyScatter(vec3 col, vec3 V, vec3 sunVec, vec3 moonVec, float sunIntensity)
{
    vec2 thickness = GetThickness(V) / steps;

    float dotVS = dot(V, sunVec);
    float dotVM = dot(V, moonVec);

    vec3 viewAbsorb = Absorb(thickness);
    vec4 scatterCoeff = 1. - exp(-thickness.xxxy * vec4(rayleighCoefficient, mieCoefficient));

    vec3 scatterS = scatterCoeff.xyz * phaseRayleigh(dotVS) + (phaseMie(dotVS) * 0.0000275);
    vec3 scatterM = scatterCoeff.xyz * phaseRayleigh(dotVM);

    vec3 sunAbsorb = Absorb(GetThickness(sunVec) * steps_inv) * getEarth(sunVec);
    vec3 moonAbsorb = Absorb(GetThickness(moonVec) * steps_inv) * getEarth(moonVec);
    
    vec3 skyColorS = col + (sin(max0(pow16(dotVS) - 0.9935) / 0.015 * Pi) * sunAbsorb * 200.0);

    vec3 skyColorM = col + (moonLight);

    float ertShdwSun = sphSoftShadow(thickness.x * V, sunVec);
    float ertShdwMoon = sphSoftShadow(thickness.x * V, moonVec);

    for (int i = 0; i < int(steps); i++)
    {
        scatterS *= sunAbsorb * 1.6;
        scatterM *= moonAbsorb * moonLight * 4.725;

        skyColorS = skyColorS * viewAbsorb + scatterS;
        skyColorM = skyColorM * viewAbsorb + scatterM;
    }

    return (skyColorS * ertShdwSun) + (skyColorM * ertShdwMoon);
}