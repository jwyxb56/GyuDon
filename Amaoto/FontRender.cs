using Amaoto;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace Amaoto
{
	/// <summary>
	/// 文字テクスチャを生成するクラス。
	/// </summary>
	public class FontRender
	{
		/// <summary>
		/// 文字テクスチャを生成するクラスの初期化をします。
		/// </summary>
		/// <param name="fontFamily">書体名。</param>
		/// <param name="fontSize">フォントサイズ。</param>
		/// <param name="edge">縁取りの大きさ。</param>
		/// <param name="fontStyle">フォントスタイル。</param>
		public FontRender(FontFamily fontFamily, int fontSize, int edge)
		{
			this.FontFamily = fontFamily;
			this.FontSize = fontSize * 96.0f / 96.0f;
			this.FontStyle = FontStyle.Regular;
			this.Edge = edge;
		}

		/// <summary>
		/// 文字テクスチャを生成します。
		/// </summary>
		/// <param name="text">文字列。</param>
		/// <returns>テクスチャ。</returns>
		public Texture GetTexture(string text, Color ForeColor, Color BackColor, float Tick, bool OnEdge = true)
		{
			if (text == null || text == "" || text == " ") return new Texture();

			if (ForeColor == Color.FromArgb(255, 255, 255) || ForeColor == Color.White)
				ForeColor = Color.FromArgb(250, 250, 250);

			if (ForeColor == Color.FromArgb(0, 0, 0) || ForeColor == Color.Black)
				ForeColor = Color.FromArgb(4, 4, 4);
			string[] strName = new string[text.Length];
			for (int i = 0; i < text.Length; i++) strName[i] = text.Substring(i, 1);
			float fNowPos = 0, fNowPos2 = 0, fTotalXSize = 0;

			//分担させて的確なサイズを取得する akasoko
			#region [ 的確なサイズを取得する ]
			for (int i = 0; i < strName.Length; i++)
			{
				var size2 = MeasureText(strName[i]);

				fTotalXSize += (size2.Width) + Tick;

				if (i == 0)
					fTotalXSize += (size2.Width) + 10;
			}
			#endregion
			try
			{
				var size = MeasureText(text);
				var bitmap = new Bitmap((int)Math.Ceiling(fTotalXSize), (int)Math.Ceiling(size.Height + 5));
				bitmap.MakeTransparent();
				var graphics = Graphics.FromImage(bitmap);
				var stringFormat = new StringFormat(StringFormat.GenericTypographic);
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				stringFormat.Trimming = StringTrimming.None;
				graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

				if (OnEdge)
				{
					for (int i = 0; i < strName.Length; i++)
					{
						var size3 = MeasureText(strName[i]);
						var bitmap3 = new Bitmap((int)Math.Ceiling(size3.Width), (int)Math.Ceiling(size3.Height + 5));
						bitmap3.MakeTransparent();
						var graphics3 = Graphics.FromImage(bitmap3);
						var stringFormat3 = new StringFormat(StringFormat.GenericTypographic);
						stringFormat3.FormatFlags = StringFormatFlags.NoWrap;
						stringFormat3.Trimming = StringTrimming.None;
						graphics3.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
						graphics3.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

						var gp3 = new System.Drawing.Drawing2D.GraphicsPath();

						if (Edge > 0)
						{
							gp3.AddString(strName[i], FontFamily, (int)FontStyle, FontSize, new Point(Edge / 2, Edge / 2), stringFormat3);
							// 縁取りをする。
							graphics3.DrawPath(new Pen(BackColor, Edge) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round }, gp3);
						}
						graphics.DrawImage(bitmap3, (int)fNowPos + 4, 8);
						fNowPos += (bitmap3.Width) + Tick;

						#region [ リソースを解放する ]
						if (bitmap3 != null) bitmap3.Dispose(); bitmap3 = null;
						if (graphics3 != null) graphics3.Dispose(); graphics3 = null;
						if (stringFormat3 != null) stringFormat3.Dispose(); stringFormat3 = null;
						if (gp3 != null) gp3.Dispose(); gp3 = null;
						#endregion
					}
				}
				for (int i = 0; i < strName.Length; i++)
				{
					var size3 = MeasureText(strName[i]);
					var bitmap3 = new Bitmap((int)Math.Ceiling(size3.Width), (int)Math.Ceiling(size3.Height));
					bitmap3.MakeTransparent();
					var graphics3 = Graphics.FromImage(bitmap3);
					var stringFormat3 = new StringFormat(StringFormat.GenericTypographic);
					stringFormat3.FormatFlags = StringFormatFlags.NoWrap;
					stringFormat3.Trimming = StringTrimming.None;
					graphics3.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
					graphics3.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

					var gp3 = new System.Drawing.Drawing2D.GraphicsPath();

					gp3.AddString(strName[i], FontFamily, (int)FontStyle, FontSize, new Point(Edge / 2, Edge / 2), stringFormat3);
					graphics3.FillPath(new SolidBrush(ForeColor), gp3);

					graphics.DrawImage(bitmap3, (int)fNowPos2 + 4, 8);
					fNowPos2 += (bitmap3.Width) + Tick;

					#region [ リソースを解放する ]
					if (bitmap3 != null) bitmap3.Dispose(); bitmap3 = null;
					if (graphics3 != null) graphics3.Dispose(); graphics3 = null;
					if (stringFormat3 != null) stringFormat3.Dispose(); stringFormat3 = null;
					if (gp3 != null) gp3.Dispose(); gp3 = null;
					#endregion
				}
				var rtx = new Texture(bitmap);
				return rtx;
			}
			catch { return new Texture(new Bitmap(16, 16)); }
		}

		private SizeF MeasureText(string text)
		{
			var bitmap = new Bitmap(16, 16);
			// .NETの敗北
			var graphicsSize = Graphics.FromImage(bitmap).
				MeasureString(text, new Font(FontFamily, FontSize, FontStyle, GraphicsUnit.Pixel));
			var trueGraphicsSize = Graphics.FromImage(bitmap).
				MeasureString(text, new Font(FontFamily, FontSize, FontStyle, GraphicsUnit.Pixel), (int)graphicsSize.Width, StringFormat.GenericTypographic);
			bitmap.Dispose();
			if (trueGraphicsSize.Width == 0 || trueGraphicsSize.Height == 0)
			{
				// サイズが0だったとき、とりあえずテクスチャとして成り立つそれっぽいサイズを返す。
				trueGraphicsSize = new SizeF(16f, 16f);
			}

			if (Edge > 0)
			{
				// 縁取りをするので、補正分。
				trueGraphicsSize.Width += Edge;
				trueGraphicsSize.Height += Edge;
			}

			return trueGraphicsSize;
		}
		public Texture GetTextureV(string text, Color ForeColor, Color BackColor, float f間隔 = 0.0f, bool bEdge = true)
		{
			if (ForeColor == Color.FromArgb(255, 255, 255) || ForeColor == Color.White)
				ForeColor = Color.FromArgb(250, 250, 250);

			if (ForeColor == Color.FromArgb(0, 0, 0) || ForeColor == Color.Black)
				ForeColor = Color.FromArgb(4, 4, 4);
			float fNowPos;
			float fYSize = 0.0f;
			float fXSize = 0.0f;
			string[] strText = new string[text.Length];

			for (int i = 0; i < strText.Length; i++)
			{
				strText[i] = text[i].ToString();
				var size = MeasureText(text);

				fYSize += size.Height;
				fXSize += size.Width;
			}

			var bitmap = new Bitmap((int)Math.Ceiling(fXSize / strText.Length), (int)Math.Ceiling(fYSize));
			bitmap.MakeTransparent();
			var graphics = Graphics.FromImage(bitmap);
			var stringFormat = GetStringFormat(graphics);

			for (int g = 0; g < 2; g++)
			{
				fNowPos = 0.0f;
				for (int i = 0; i < strText.Length; i++)
				{
					var size = MeasureText(text);
					var bitmap2 = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
					bitmap2.MakeTransparent();
					var graphics2 = Graphics.FromImage(bitmap2);
					var stringFormat2 = GetStringFormat(graphics2);

					DrawVString(strText[i], graphics, graphics2, stringFormat2, bitmap2, fNowPos, g == 0 ? true : false, ForeColor, BackColor, bEdge);
					fNowPos += (size.Height + f間隔) * 0.75f;

					bitmap2.Dispose();
					graphics2.Dispose();
					stringFormat2.Dispose();
				}
			}
			var tex = new Texture(bitmap);
			// 破棄

			graphics.Dispose();
			stringFormat.Dispose();

			return tex;
		}
		private void DrawVString(string text, Graphics graphics, Graphics graphics2, StringFormat stringFormat, Bitmap bitmap, float fNowPos, bool IsEdge, Color ForeColor, Color BackColor, bool bEdge)
		{
			var gp = new GraphicsPath();
			gp.AddString(text, FontFamily, (int)FontStyle, FontSize, new Point(Edge / 2, Edge / 2), stringFormat);

			if (IsEdge)
			{
				if (bEdge)
					graphics2.DrawPath(new Pen(BackColor, Edge) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round }, gp);
			}
			else graphics2.FillPath(new SolidBrush(ForeColor), gp);
			graphics.DrawImage(bitmap, 0, (int)fNowPos);
		}

		private static StringFormat GetStringFormat(Graphics graphics)
		{
			var stringFormat = new StringFormat(StringFormat.GenericTypographic);
			// どんなに長くて単語の区切りが良くても改行しない
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			// どんなに長くてもトリミングしない
			stringFormat.Trimming = StringTrimming.None;
			// ハイクオリティレンダリング
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			// アンチエイリアスをかける
			graphics.SmoothingMode = SmoothingMode.HighQuality;

			return stringFormat;
		}


		/// <summary>
		/// 文字色。
		/// </summary>
		public Color ForeColor { get; set; }
		/// <summary>
		/// 縁色。
		/// </summary>
		public Color BackColor { get; set; }
		/// <summary>
		/// 縁取りのサイズ。
		/// </summary>
		public int Edge { get; set; }
		private FontFamily FontFamily;
		private FontStyle FontStyle;
		private float FontSize;
	}
}
