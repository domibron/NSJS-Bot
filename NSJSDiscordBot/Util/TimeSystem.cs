using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSJSDiscordBot.Util
{
    public class TimeSystem
    {
        private static DateTime _oldTme = DateTime.Now;

        public static float deltaTime;


        public async Task UpdateDeltaTime()
        {
            while (true)
            {
                //await Task.Delay(0001);

                DateTime dateTime = DateTime.Now;

                deltaTime = await GetDelta(dateTime, _oldTme);
                _oldTme = dateTime;
            }
        }

        public static Task<float> GetDelta(DateTime timeA, DateTime timeB)
        {
            TimeSpan delta = timeA - timeB;


            return Task.FromResult((float)delta.TotalSeconds);

            //delta.Days

            //return ;
        }
    }
}
