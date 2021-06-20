using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HR_Toolkit
{

  public class InterpolationWithRotation : InterpolationRedirection
  {

    /// <summary>
    /// Power
    /// </summary>
    public float p;
    public float pRot;


    public override void ApplyRedirection(Transform realHandPos, Transform virtualHandPos, Transform warpOrigin, RedirectionObject target,
        Transform bodyTransform)
    {
      virtualHandPos.position = IDW(realHandPos.position, target);
      virtualHandPos.rotation = IDWRot(realHandPos.position, realHandPos.rotation, target);
    }

    Quaternion AverageQuaternionWithWeight(List<Quaternion> qArray, List<float> wArray)
    {
      var wArrayNormalized = new List<float>();
      foreach (var w in wArray)
      {
        wArrayNormalized.Add(w / wArray.Max());
      }

      float maxVal = float.MinValue;
      int maxIndex = 0;
      for (int i = 0; i < wArrayNormalized.Count; i++)
      { // finding the largest w
        if (wArrayNormalized[i] > maxVal)
        {
          maxVal = wArrayNormalized[i];
          maxIndex = i;
        }
      }
      Quaternion qAvg = qArray[maxIndex]; // initial qAvg is the one which wArray is the largest
      qArray.RemoveAt(maxIndex);
      wArrayNormalized.RemoveAt(maxIndex);

      float standardWeight;
      for (int i = 1; i < qArray.Count; i++)
      {
        standardWeight = 1.0f / (float)(i + 1);
        qAvg = Quaternion.Slerp(qAvg, qArray[i], standardWeight * wArrayNormalized[i]);
      }
      return qAvg;
    }

    private Quaternion IDWRot(Vector3 x, Quaternion xRot, RedirectionObject target)
    {
      var points = target.GetAllPositions();

      var qArray = new List<Quaternion>();
      var wArray = new List<float>();

      qArray.Add(xRot);
      wArray.Add(1); // todo: determine threshold

      foreach (var point in points)
      {
        var d = Vector3.Distance(x, point.GetRealPosition());
        if (d == 0f) // todo: determine threshold
        {
          return point.GetVirtualRotation();
        }
        var w = Mathf.Pow(1 / d, pRot);
        qArray.Add(point.GetVirtualRotation());
        wArray.Add(w);
      }

      return AverageQuaternionWithWeight(qArray, wArray);
    }

    private Vector3 IDW(Vector3 x, RedirectionObject target)
    {
      var u = Vector3.zero;
      var points = target.GetAllPositions();

      var topSum = Vector3.zero;
      var bottomSum = 0f;

      foreach (var point in points)
      {
        var d = Vector3.Distance(x, point.GetRealPosition());
        if (d == 0f)
        {
          return x + point.GetVirtualPosition() - point.GetRealPosition();
        }
        var w = Mathf.Pow(1 / d, p);


        topSum += w * (point.GetVirtualPosition() - point.GetRealPosition());
        bottomSum += w;

      }

      u = topSum / bottomSum;
      return x + u;
    }


  }
}