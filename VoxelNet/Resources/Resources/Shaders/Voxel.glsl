float saturate(float val)
{
	return clamp(val, 0.0, 1.0);
}

vec4 saturate(vec4 val)
{
	return clamp(val, 0.0, 1.0);
}

vec3 saturate(vec3 val)
{
	return clamp(val, 0.0, 1.0);
}

float depthToLinear(float depth, float nearPlane, float farPlane)
{
	return 2.0 * nearPlane * farPlane / (farPlane + nearPlane - (2.0 * depth - 1.0) * (farPlane - nearPlane));
}