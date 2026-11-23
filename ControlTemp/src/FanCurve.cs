namespace ControlTemp;

public static class FanCurve
{
    // Devuelve el % de ventilador para una temperatura, interpolando entre los puntos de la curva.
    public static float Evaluate(IEnumerable<CurvePoint>? points, float temp)
    {
        var p = points?.OrderBy(x => x.Temp).ToList();
        if (p is null || p.Count == 0) return 100;
        if (temp <= p[0].Temp) return p[0].Percent;

        for (int i = 1; i < p.Count; i++)
        {
            if (temp <= p[i].Temp)
            {
                var a = p[i - 1];
                var b = p[i];
                if (b.Temp == a.Temp) return b.Percent;
                return a.Percent + (b.Percent - a.Percent) * (temp - a.Temp) / (b.Temp - a.Temp);
            }
        }
        return p[^1].Percent;
    }
}
