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