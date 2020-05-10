using UnityEngine;

public static class ExtraMath
{
  public static bool integerWithin(int value, int min, int max)
  {
    return value >= min && value <= max;
  }

  public static int ThreeDimToIndex(int x, int y, int z, int width, int height)
  {
    return x + width * y + width * height * z;
  }

  public static int IndexToThreeDimX(int index, int width)
  {
    return index % width;
  }

  public static int IndexToThreeDimY(int index, int width, int height)
  {
    return (index / width) % height;
  }

  public static int IndexToThreeDimZ(int index, int width, int height)
  {
    return index / (width * height);
  }

  public static Vector3 RotatePointZN(Vector3 point, float zAngle, float nAngle)
  {
    float theta = zAngle * Mathf.Deg2Rad; // Rotate 10° from origin along z-axis
    float azimuth = nAngle * Mathf.Deg2Rad; // Rotate 20° from origin along the axis defined by theta

    return new Vector3(
      point.x * Mathf.Cos(theta) * Mathf.Sin(azimuth),
      point.y * Mathf.Sin(theta) * Mathf.Sin(azimuth),
      point.z * Mathf.Cos(azimuth)
    );
  }

  public static Vector3 RotatePointAroundPoint(Vector3 toRotate, Vector3 around, Quaternion newRotation)
  {
    return newRotation * (toRotate - around) + around;
  }

  public static Vector3 RotatePointAroundPoint(Vector3 toRotate, Vector3 around, float xr, float yr, float zr)
  {
    Quaternion newRotation = new Quaternion();
    newRotation.eulerAngles = new Vector3(xr, yr, zr);
    return ExtraMath.RotatePointAroundPoint(toRotate, around, newRotation);
  }
}
