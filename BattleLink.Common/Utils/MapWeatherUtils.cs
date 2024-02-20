using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace BattleLink.Common.Utils
{
    public static class MapWeatherUtils
    {
      //  public override void GetSeasonTimeFactorOfCampaignTime(
      //CampaignTime ct,
      //out float timeFactorForSnow,
      //out float timeFactorForRain,
      //bool snapCampaignTimeToWeatherPeriod = true)
      //  {
      //      if (snapCampaignTimeToWeatherPeriod)
      //          ct = CampaignTime.Hours((float)((int)(ct.ToHours / this.WeatherUpdatePeriod.ToHours / 2.0) * (int)this.WeatherUpdatePeriod.ToHours * 2));
      //      float yearProgress = (float)ct.ToSeasons % 4f;
      //      timeFactorForSnow = this.CalculateTimeFactorForSnow(yearProgress);
      //      timeFactorForRain = this.CalculateTimeFactorForRain(yearProgress);
      //  }

      //  private float CalculateTimeFactorForSnow(/*Parameter with token 08001760*/float yearProgress)
      //  {
      //      float timeFactorForSnow = 0.0f;
      //      if ((double)yearProgress > 1.5 && (double)yearProgress <= 3.5)
      //          timeFactorForSnow = MBMath.Map(yearProgress, 1.5f, 3.5f, 0.0f, 1f);
      //      else if ((double)yearProgress <= 1.5)
      //          timeFactorForSnow = MBMath.Map(yearProgress, 0.0f, 1.5f, 0.75f, 0.0f);
      //      else if ((double)yearProgress > 3.5)
      //          timeFactorForSnow = MBMath.Map(yearProgress, 3.5f, 4f, 1f, 0.75f);
      //      return timeFactorForSnow;
      //  }

      //  private float CalculateTimeFactorForRain(/*Parameter with token 08001761*/float yearProgress)
      //  {
      //      float timeFactorForRain = 0.0f;
      //      if ((double)yearProgress > 1.0 && (double)yearProgress <= 2.5)
      //          timeFactorForRain = MBMath.Map(yearProgress, 1f, 2.5f, 0.0f, 1f);
      //      else if ((double)yearProgress <= 1.0)
      //          timeFactorForRain = MBMath.Map(yearProgress, 0.0f, 1f, 1f, 0.0f);
      //      else if ((double)yearProgress > 2.5)
      //          timeFactorForRain = 1f;
      //      return timeFactorForRain;
      //  }


    }
}
