using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI
{
    //用户数据
    static class UserData
    {
        public static List<SelectedHeroInfo> AllHero = new List<SelectedHeroInfo>();

        static UserData()
        {
            AllHero.Add(new SelectedHeroInfo() { name = "Knight", modelAssetPath = "" });
            AllHero.Add(new SelectedHeroInfo() { name = "Knight2", modelAssetPath = "" });
            AllHero.Add(new SelectedHeroInfo() { name = "Knight3", modelAssetPath = "" });
        }
    }
    //选英雄界面结构
    class SelectedHeroInfo
    {
        public string name;//英雄名
        public string modelAssetPath;//模型资源路径
    }
}
