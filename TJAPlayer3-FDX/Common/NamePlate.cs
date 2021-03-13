using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using FDK;
using Amaoto;

namespace TJAPlayer3
{
    public class NamePlate
    {
        public string PlayerName = "どんちゃん";
        public string TitleName = "GyuDonプレイヤー";
        public int Type = 1;

        public static void Init()
        {
            const string NAMEPLATE = @"NamePlate\";

            NamePlate_Base = TxC($"{NAMEPLATE}Base.png");
            NamePlate_Player = TxC($"{NAMEPLATE}Player.png");
            for (int i = 0; i < 7; i++)
                NamePlate_Type[i] = TxC($"{NAMEPLATE}{i}.png");

            FontRender FR1 = new FontRender(new FontFamily(TJAPlayer3.ConfigIni.FontName), 17, 4);
            txPlayerName = FR1.GetTexture(TJAPlayer3.NamePlate.PlayerName, Color.White, Color.Black, -4);
            txPlayerName.ReferencePoint = ReferencePoint.TopCenter;
          


            FontRender FR2 = new FontRender(new FontFamily(TJAPlayer3.ConfigIni.FontName), 15, 0);
            txTitleName = FR2.GetTexture(TJAPlayer3.NamePlate.TitleName, Color.Black, Color.Transparent, -1);
            txTitleName.ReferencePoint = ReferencePoint.TopCenter;
        }

        public static void nameplate(int x, int y)
        {
            NamePlate_Base?.Draw(x, y);
            NamePlate_Type[TJAPlayer3.NamePlate.Type]?.Draw(x, y + 1); 
            NamePlate_Player?.Draw(x + 12, y + 7, new Rectangle(0, 0, 49, 48));

            float Y = 0;
            if (txTitleName.TextureSize.width >= 152)
            {
                txTitleName.ScaleX = 152f / txTitleName.TextureSize.width;
                txTitleName.ScaleY = 35f / (txTitleName.TextureSize.width / 4.34f);
                Y = (txTitleName.TextureSize.height - (txTitleName.TextureSize.height * (35f / (txTitleName.TextureSize.width / 4.34f)))) / 2;

            }

            txPlayerName?.Draw(x + 143, y + 20);
            txTitleName?.Draw(x + 137, y + 2 + Y);

            //27, 609

            TJAPlayer3.act文字コンソール.tPrint(0, 0, C文字コンソール.Eフォント種別.白, $"{txTitleName.TextureSize.height - (txTitleName.TextureSize.height * (35f / (txTitleName.TextureSize.width / 4.34f)))}");
        }

        internal static Texture TxC(string FileName)
        {
            return new Texture(CSkin.Path(@"Graphics\" + FileName));
        }

        public static Texture NamePlate_Base;
        public static Texture NamePlate_Player;
        public static Texture[] NamePlate_Type = new Texture[7];
        public static Texture txPlayerName;
        public static Texture txTitleName;
    }
}
