using Replay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace BattleLink.Singleplayer
{
    internal class MainJsonTest
    {
        static void Main(string[] args)
        {
            Vec2 pos = Vec2.Forward;

            string json = JsonUtils.SerializeObject(pos, 10);



            Console.WriteLine(json);
            Console.WriteLine("finn");
        }
    }
}
