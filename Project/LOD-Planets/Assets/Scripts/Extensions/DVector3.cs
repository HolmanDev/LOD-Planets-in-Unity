[System.Serializable]
public class DVector3
{
    public double x;
    public double y;
    public double z;

    public DVector3(double x, double y, double z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static DVector3 operator -(DVector3 vector) {
        return new DVector3(-vector.x, -vector.y, -vector.z);
    }

    public static DVector3 operator +(DVector3 vec1, DVector3 vec2) {
        return new DVector3(vec1.x + vec2.x, vec1.y + vec2.y, vec1.z + vec2.z);
    }

    public static DVector3 operator -(DVector3 vec1, DVector3 vec2) {
        return vec1 + (-vec2);
    }

    public static DVector3 operator *(DVector3 vec, double factor) {
        return new DVector3(vec.x * factor, vec.y * factor, vec.z * factor);
    }

    public static DVector3 operator /(DVector3 vec, double divisor) {
        return new DVector3(vec.x / divisor, vec.y / divisor, vec.z / divisor);
    }

    public double magnitude { get { return System.Math.Sqrt(x*x + y*y + z*z); } }

    public DVector3 normalized { get { return this / magnitude; } }

    public static explicit operator UnityEngine.Vector3(DVector3 vec) {
        return new UnityEngine.Vector3((float) vec.x, (float) vec.z, (float) vec.y);
    }

    public static explicit operator DVector3(UnityEngine.Vector3 vec) {
        return new DVector3(vec.x, vec.z, vec.y);
    }

    public DVector3 Translate(DVector3 position)
    {
        x += position.x;
        y += position.y;
        z += position.z;
        return this;
    }

    public DVector3 Scale(DVector3 scale)
    {
        x *= scale.x;
        y *= scale.y;
        z *= scale.z;
        return this;
    }
}
