using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Matho
{
  public static (float x1, float x2) QuadraticFormula(float a, float b, float c)
  {
    float sqrtpart = b * b - 4 * a * c;

    if (sqrtpart > 0)
    {
      float x1 = (-b + Mathf.Sqrt(sqrtpart)) / (2 * a);
      float x2 = (-b - Mathf.Sqrt(sqrtpart)) / (2 * a);
      return (x1, x2);
    }
    else if (sqrtpart < 0)
    {
      sqrtpart *= -1;
      float x1 = -b / (2 * a);
      return (x1, float.NaN);
    }
    else
    {
      float x1 = (-b + Mathf.Sqrt(sqrtpart)) / (2 * a);
      return (x1, float.NaN);
    }

  }

  public static (float x1, float x2) QuadraticFunctionX(Vector2 vertex, float slope, float value)
  {
    float sqrt = Mathf.Sqrt((value - vertex.y) / slope);
    float x1 = vertex.x + sqrt;
    float x2 = vertex.x - sqrt;
    return (x1, x2);
  }

  public static (float y1, float NaN) QuadraticFunctionY(Vector2 vertex, float slope, float value)
  {
    float y1 = slope * Mathf.Pow(value - vertex.x, 2) + vertex.y;
    return (y1, float.NaN);
  }
  public static (float x1, float x2) CircleFunctionX(Vector2 vertex, float radius, float value)
  {
    float sqrt = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - vertex.y, 2));
    float x1 = vertex.x + sqrt;
    float x2 = vertex.x + sqrt;
    return (x1, x2);
  }

  public static (float y1, float y2) CircleFunctionY(Vector2 vertex, float radius, float value)
  {
    float sqrt = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - vertex.x, 2));
    float y1 = vertex.y + sqrt;
    float y2 = vertex.y - sqrt;
    return (y1, y2);
  }

  public static Vector2 RadianToVector2(float radian)
  {
    return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
  }

  public static Vector2 DegreeToVector2(float degree)
  {
    return RadianToVector2(degree * Mathf.Deg2Rad);
  }

  public static float Vector2ToDegree(Vector2 vector)
  {
    vector = vector.normalized;

    return 360 - (Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg * Mathf.Sign(vector.x));
  }

  public static float Percent(float value, float outOf)
  {
    return value / outOf;
  }

  public static bool IsEven(float value)
  {
    return value % 2 == 0;
  }

  public static float Clamp(float value, float a, float b)
  {
    if (a > b)
    {
      if (value < b) value = b;
      else if (value > a) value = a;
    }
    else
    {
      if (value < a) value = a;
      else if (value > b) value = b;
    }
    return value;
  }

  public static bool IsBetween(float value, float a, float b)
  {
    if (a > b) return value >= b && value <= a;
    return value >= a && value <= b;
  }
}
