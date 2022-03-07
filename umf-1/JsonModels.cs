namespace umf_1;

public class AreaModel
{
    public int Lambda { get; set; }
    public int Gamma { get; set; }

    public double[] DischargeRatioX { get; set; }
    public double[] DischargeRatioY { get; set; }

    public int[] AmountX { get; set; }
    public int[] AmountY { get; set; }

    public double[] PivotX { get; set; }
    public double[] PivotY { get; set; }
}

public class SlaeAccuracyModel
{
    public int MaxIter { get; set; }
    public double Eps { get; set; }
    public double Delta { get; set; }
}