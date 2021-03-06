using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing.Text;
using CSharpTest.Net.Collections;
using DxLibDLL;
using FDK;
using Amaoto;

namespace TJAPlayer3
{
	internal class CActSelect曲リスト : CActivity
	{
		// プロパティ

		public bool bIsEnumeratingSongs
		{
			get;
			set;
		}
		public bool bスクロール中
		{
			get
			{
				if( this.n目標のスクロールカウンタ == 0 )
				{
					return ( this.n現在のスクロールカウンタ != 0 );
				}
				return true;
			}
		}
		public int n現在のアンカ難易度レベル 
		{
			get;
			private set;
		}
		public int n現在選択中の曲の現在の難易度レベル
		{
			get
			{
				return this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す( this.r現在選択中の曲 );
			}
		}
		public Cスコア r現在選択中のスコア
		{
			get
			{
				if( this.r現在選択中の曲 != null )
				{
					return this.r現在選択中の曲.arスコア[ this.n現在選択中の曲の現在の難易度レベル ];
				}
				return null;
			}
		}
		public C曲リストノード r現在選択中の曲 
		{
			get;
			private set;
		}

		public int nスクロールバー相対y座標
		{
			get;
			private set;
		}

		// t選択曲が変更された()内で使う、直前の選曲の保持
		// (前と同じ曲なら選択曲変更に掛かる再計算を省略して高速化するため)
		private C曲リストノード song_last = null;

		
		// コンストラクタ

		public CActSelect曲リスト()
        {
            #region[ レベル数字 ]
            STレベル数字[] stレベル数字Ar = new STレベル数字[ 10 ];
            STレベル数字 st数字0 = new STレベル数字();
            STレベル数字 st数字1 = new STレベル数字();
            STレベル数字 st数字2 = new STレベル数字();
            STレベル数字 st数字3 = new STレベル数字();
            STレベル数字 st数字4 = new STレベル数字();
            STレベル数字 st数字5 = new STレベル数字();
            STレベル数字 st数字6 = new STレベル数字();
            STレベル数字 st数字7 = new STレベル数字();
            STレベル数字 st数字8 = new STレベル数字();
            STレベル数字 st数字9 = new STレベル数字();

            st数字0.ch = '0';
            st数字1.ch = '1';
            st数字2.ch = '2';
            st数字3.ch = '3';
            st数字4.ch = '4';
            st数字5.ch = '5';
            st数字6.ch = '6';
            st数字7.ch = '7';
            st数字8.ch = '8';
            st数字9.ch = '9';
            st数字0.ptX = 0;
            st数字1.ptX = 22;
            st数字2.ptX = 44;
            st数字3.ptX = 66;
            st数字4.ptX = 88;
            st数字5.ptX = 110;
            st数字6.ptX = 132;
            st数字7.ptX = 154;
            st数字8.ptX = 176;
            st数字9.ptX = 198;

            stレベル数字Ar[0] = st数字0;
            stレベル数字Ar[1] = st数字1;
            stレベル数字Ar[2] = st数字2;
            stレベル数字Ar[3] = st数字3;
            stレベル数字Ar[4] = st数字4;
            stレベル数字Ar[5] = st数字5;
            stレベル数字Ar[6] = st数字6;
            stレベル数字Ar[7] = st数字7;
            stレベル数字Ar[8] = st数字8;
            stレベル数字Ar[9] = st数字9;
            this.st小文字位置 = stレベル数字Ar;
            #endregion


            this.r現在選択中の曲 = null;
            this.n現在のアンカ難易度レベル = TJAPlayer3.ConfigIni.nDefaultCourse;
			base.b活性化してない = true;
			this.bIsEnumeratingSongs = false;
		}


		// メソッド

		public int n現在のアンカ難易度レベルに最も近い難易度レベルを返す( C曲リストノード song )
		{
			// 事前チェック。

			if( song == null )
				return this.n現在のアンカ難易度レベル;	// 曲がまったくないよ

			if( song.arスコア[ this.n現在のアンカ難易度レベル ] != null )
				return this.n現在のアンカ難易度レベル;	// 難易度ぴったりの曲があったよ

			if( ( song.eノード種別 == C曲リストノード.Eノード種別.BOX ) || ( song.eノード種別 == C曲リストノード.Eノード種別.BACKBOX ) )
				return 0;								// BOX と BACKBOX は関係無いよ


			// 現在のアンカレベルから、難易度上向きに検索開始。

			int n最も近いレベル = this.n現在のアンカ難易度レベル;

			for( int i = 0; i < (int)Difficulty.Total; i++ )
			{
				if( song.arスコア[ n最も近いレベル ] != null )
					break;	// 曲があった。

				n最も近いレベル = ( n最も近いレベル + 1 ) % (int)Difficulty.Total;	// 曲がなかったので次の難易度レベルへGo。（5以上になったら0に戻る。）
			}


			// 見つかった曲がアンカより下のレベルだった場合……
			// アンカから下向きに検索すれば、もっとアンカに近い曲があるんじゃね？

			if( n最も近いレベル < this.n現在のアンカ難易度レベル )
			{
				// 現在のアンカレベルから、難易度下向きに検索開始。

				n最も近いレベル = this.n現在のアンカ難易度レベル;

				for( int i = 0; i < (int)Difficulty.Total; i++ )
				{
					if( song.arスコア[ n最も近いレベル ] != null )
						break;	// 曲があった。

					n最も近いレベル = ( ( n最も近いレベル - 1 ) + (int)Difficulty.Total) % (int)Difficulty.Total;	// 曲がなかったので次の難易度レベルへGo。（0未満になったら4に戻る。）
				}
			}

			return n最も近いレベル;
		}
		public C曲リストノード r指定された曲が存在するリストの先頭の曲( C曲リストノード song )
		{
			List<C曲リストノード> songList = GetSongListWithinMe( song );
			return ( songList == null ) ? null : songList[ 0 ];
		}
		public C曲リストノード r指定された曲が存在するリストの末尾の曲( C曲リストノード song )
		{
			List<C曲リストノード> songList = GetSongListWithinMe( song );
			return ( songList == null ) ? null : songList[ songList.Count - 1 ];
		}

		private List<C曲リストノード> GetSongListWithinMe( C曲リストノード song )
		{
			if ( song.r親ノード == null )					// root階層のノートだったら
			{
				return TJAPlayer3.Songs管理.list曲ルート;	// rootのリストを返す
			}
			else
			{
				if ( ( song.r親ノード.list子リスト != null ) && ( song.r親ノード.list子リスト.Count > 0 ) )
				{
					return song.r親ノード.list子リスト;
				}
				else
				{
					return null;
				}
			}
		}


		public delegate void DGSortFunc( List<C曲リストノード> songList, E楽器パート eInst, int order, params object[] p);
		/// <summary>
		/// 主にCSong管理.cs内にあるソート機能を、delegateで呼び出す。
		/// </summary>
		/// <param name="sf">ソート用に呼び出すメソッド</param>
		/// <param name="eInst">ソート基準とする楽器</param>
		/// <param name="order">-1=降順, 1=昇順</param>
		public void t曲リストのソート( DGSortFunc sf, E楽器パート eInst, int order, params object[] p )
		{
			List<C曲リストノード> songList = GetSongListWithinMe( this.r現在選択中の曲 );
			if ( songList == null )
			{
				// 何もしない;
			}
			else
			{
//				CDTXMania.Songs管理.t曲リストのソート3_演奏回数の多い順( songList, eInst, order );
				sf( songList, eInst, order, p );
//				this.r現在選択中の曲 = CDTXMania
				this.t現在選択中の曲を元に曲バーを再構成する();
			}
		}

		public bool tBOXに入る()
		{
//Trace.TraceInformation( "box enter" );
//Trace.TraceInformation( "Skin現在Current : " + CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) );
//Trace.TraceInformation( "Skin現在System  : " + CSkin.strSystemSkinSubfolderFullName );
//Trace.TraceInformation( "Skin現在BoxDef  : " + CSkin.strBoxDefSkinSubfolderFullName );
//Trace.TraceInformation( "Skin現在: " + CSkin.GetSkinName( CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) ) );
//Trace.TraceInformation( "Skin現pt: " + CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) );
//Trace.TraceInformation( "Skin指定: " + CSkin.GetSkinName( this.r現在選択中の曲.strSkinPath ) );
//Trace.TraceInformation( "Skinpath: " + this.r現在選択中の曲.strSkinPath );
			bool ret = false;
			if ( CSkin.GetSkinName( TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName( false ) ) != CSkin.GetSkinName( this.r現在選択中の曲.strSkinPath )
				&& CSkin.bUseBoxDefSkin )
			{
				ret = true;
				// BOXに入るときは、スキン変更発生時のみboxdefスキン設定の更新を行う
				TJAPlayer3.Skin.SetCurrentSkinSubfolderFullName(
					TJAPlayer3.Skin.GetSkinSubfolderFullNameFromSkinName( CSkin.GetSkinName( this.r現在選択中の曲.strSkinPath ) ), false );
			}

//Trace.TraceInformation( "Skin変更: " + CSkin.GetSkinName( CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) ) );
//Trace.TraceInformation( "Skin変更Current : "+  CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) );
//Trace.TraceInformation( "Skin変更System  : "+  CSkin.strSystemSkinSubfolderFullName );
//Trace.TraceInformation( "Skin変更BoxDef  : "+  CSkin.strBoxDefSkinSubfolderFullName );

			if( ( this.r現在選択中の曲.list子リスト != null ) && ( this.r現在選択中の曲.list子リスト.Count > 0 ) )
			{
				this.r現在選択中の曲 = this.r現在選択中の曲.list子リスト[ 0 ];
				this.t現在選択中の曲を元に曲バーを再構成する();
				this.t選択曲が変更された(false);									// #27648 項目数変更を反映させる
				this.b選択曲が変更された = true;
			}
			return ret;
		}
		public bool tBOXを出る()
		{
//Trace.TraceInformation( "box exit" );
//Trace.TraceInformation( "Skin現在Current : " + CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) );
//Trace.TraceInformation( "Skin現在System  : " + CSkin.strSystemSkinSubfolderFullName );
//Trace.TraceInformation( "Skin現在BoxDef  : " + CSkin.strBoxDefSkinSubfolderFullName );
//Trace.TraceInformation( "Skin現在: " + CSkin.GetSkinName( CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) ) );
//Trace.TraceInformation( "Skin現pt: " + CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) );
//Trace.TraceInformation( "Skin指定: " + CSkin.GetSkinName( this.r現在選択中の曲.strSkinPath ) );
//Trace.TraceInformation( "Skinpath: " + this.r現在選択中の曲.strSkinPath );
			bool ret = false;
			if ( CSkin.GetSkinName( TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName( false ) ) != CSkin.GetSkinName( this.r現在選択中の曲.strSkinPath )
				&& CSkin.bUseBoxDefSkin )
			{
				ret = true;
			}
			// スキン変更が発生しなくても、boxdef圏外に出る場合は、boxdefスキン設定の更新が必要
			// (ユーザーがboxdefスキンをConfig指定している場合への対応のために必要)
			// tBoxに入る()とは処理が微妙に異なるので注意
			TJAPlayer3.Skin.SetCurrentSkinSubfolderFullName(
				( this.r現在選択中の曲.strSkinPath == "" ) ? "" : TJAPlayer3.Skin.GetSkinSubfolderFullNameFromSkinName( CSkin.GetSkinName( this.r現在選択中の曲.strSkinPath ) ), false );
//Trace.TraceInformation( "SKIN変更: " + CSkin.GetSkinName( CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) ) );
//Trace.TraceInformation( "SKIN変更Current : "+  CDTXMania.Skin.GetCurrentSkinSubfolderFullName(false) );
//Trace.TraceInformation( "SKIN変更System  : "+  CSkin.strSystemSkinSubfolderFullName );
//Trace.TraceInformation( "SKIN変更BoxDef  : "+  CSkin.strBoxDefSkinSubfolderFullName );
			if ( this.r現在選択中の曲.r親ノード != null )
			{
				this.r現在選択中の曲 = this.r現在選択中の曲.r親ノード;
				this.t現在選択中の曲を元に曲バーを再構成する();
				this.t選択曲が変更された(false);									// #27648 項目数変更を反映させる
				this.b選択曲が変更された = true;
			}
			return ret;
		}
		public void t現在選択中の曲を元に曲バーを再構成する()
		{
			this.tバーの初期化();
			for( int i = 0; i < 13; i++ )
			{
				//this.t曲名バーの生成( i, this.stバー情報[ i ].strタイトル文字列, this.stバー情報[ i ].ForeColor, this.stバー情報[i].BackColor);
			}
		}
		public void t次に移動()
		{
			if( this.r現在選択中の曲 != null )
			{
				this.n目標のスクロールカウンタ += 150;
			}
			this.b選択曲が変更された = true;
		}
		public void t前に移動()
		{
			if( this.r現在選択中の曲 != null )
			{
				this.n目標のスクロールカウンタ -= 150;
			}
			this.b選択曲が変更された = true;
		}
		public void t難易度レベルをひとつ進める()
		{
			if( ( this.r現在選択中の曲 == null ) || ( this.r現在選択中の曲.nスコア数 <= 1 ) )
				return;		// 曲にスコアが０～１個しかないなら進める意味なし。
			

			// 難易度レベルを＋１し、現在選曲中のスコアを変更する。

			this.n現在のアンカ難易度レベル = this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す( this.r現在選択中の曲 );

			for( int i = 0; i < (int)Difficulty.Total; i++ )
			{
				this.n現在のアンカ難易度レベル = ( this.n現在のアンカ難易度レベル + 1 ) % (int)Difficulty.Total;	// ５以上になったら０に戻る。
				if( this.r現在選択中の曲.arスコア[ this.n現在のアンカ難易度レベル ] != null )	// 曲が存在してるならここで終了。存在してないなら次のレベルへGo。
					break;
			}


			// 曲毎に表示しているスキル値を、新しい難易度レベルに合わせて取得し直す。（表示されている13曲全部。）

			C曲リストノード song = this.r現在選択中の曲;
			for( int i = 0; i < 5; i++ )
				song = this.r前の曲( song );

			for( int i = this.n現在の選択行 - 5; i < ( ( this.n現在の選択行 - 5 ) + 13 ); i++ )
			{
				int index = ( i + 13 ) % 13;
				for( int m = 0; m < 3; m++ )
				{
					this.stバー情報[ index ].nスキル値[ m ] = (int) song.arスコア[ this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す( song ) ].譜面情報.最大スキル[ m ];
				}
				song = this.r次の曲( song );
			}


			// 選曲ステージに変更通知を発出し、関係Activityの対応を行ってもらう。

			TJAPlayer3.stage選曲.t選択曲変更通知();
		}
        /// <summary>
        /// 不便だったから作った。
        /// </summary>
		public void t難易度レベルをひとつ戻す()
		{
			if( ( this.r現在選択中の曲 == null ) || ( this.r現在選択中の曲.nスコア数 <= 1 ) )
				return;		// 曲にスコアが０～１個しかないなら進める意味なし。
			

			// 難易度レベルを＋１し、現在選曲中のスコアを変更する。

			this.n現在のアンカ難易度レベル = this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す( this.r現在選択中の曲 );

            this.n現在のアンカ難易度レベル--;
            if( this.n現在のアンカ難易度レベル < 0 ) // 0より下になったら4に戻す。
            {
                this.n現在のアンカ難易度レベル = 4;
            }

            //2016.08.13 kairera0467 かんたん譜面が無い譜面(ふつう、むずかしいのみ)で、難易度を最上位に戻せない不具合の修正。
            bool bLabel0NotFound = true;
            for( int i = this.n現在のアンカ難易度レベル; i >= 0; i-- )
            {
                if( this.r現在選択中の曲.arスコア[ i ] != null )
                {
                    this.n現在のアンカ難易度レベル = i;
                    bLabel0NotFound = false;
                    break;
                }
            }
            if( bLabel0NotFound )
            {
                for( int i = 4; i >= 0; i-- )
                {
                    if( this.r現在選択中の曲.arスコア[ i ] != null )
                    {
                        this.n現在のアンカ難易度レベル = i;
                        break;
                    }
                }
            }

			// 曲毎に表示しているスキル値を、新しい難易度レベルに合わせて取得し直す。（表示されている13曲全部。）

			C曲リストノード song = this.r現在選択中の曲;
			for( int i = 0; i < 5; i++ )
				song = this.r前の曲( song );

			for( int i = this.n現在の選択行 - 5; i < ( ( this.n現在の選択行 - 5 ) + 13 ); i++ )
			{
				int index = ( i + 13 ) % 13;
				for( int m = 0; m < 3; m++ )
				{
					this.stバー情報[ index ].nスキル値[ m ] = (int) song.arスコア[ this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す( song ) ].譜面情報.最大スキル[ m ];
				}
				song = this.r次の曲( song );
			}


			// 選曲ステージに変更通知を発出し、関係Activityの対応を行ってもらう。

			TJAPlayer3.stage選曲.t選択曲変更通知();
		}


		/// <summary>
		/// 曲リストをリセットする
		/// </summary>
		/// <param name="cs"></param>
		public void Refresh(CSongs管理 cs, bool bRemakeSongTitleBar )		// #26070 2012.2.28 yyagi
		{
//			this.On非活性化();

			if ( cs != null && cs.list曲ルート.Count > 0 )	// 新しい曲リストを検索して、1曲以上あった
			{
				TJAPlayer3.Songs管理 = cs;

				if ( this.r現在選択中の曲 != null )			// r現在選択中の曲==null とは、「最初songlist.dbが無かった or 検索したが1曲もない」
				{
					this.r現在選択中の曲 = searchCurrentBreadcrumbsPosition( TJAPlayer3.Songs管理.list曲ルート, this.r現在選択中の曲.strBreadcrumbs );
					if ( bRemakeSongTitleBar )					// 選曲画面以外に居るときには再構成しない (非活性化しているときに実行すると例外となる)
					{
						this.t現在選択中の曲を元に曲バーを再構成する();
					}
#if false			// list子リストの中まではmatchしてくれないので、検索ロジックは手書きで実装 (searchCurrentBreadcrumbs())
					string bc = this.r現在選択中の曲.strBreadcrumbs;
					Predicate<C曲リストノード> match = delegate( C曲リストノード c )
					{
						return ( c.strBreadcrumbs.Equals( bc ) );
					};
					int nMatched = CDTXMania.Songs管理.list曲ルート.FindIndex( match );

					this.r現在選択中の曲 = ( nMatched == -1 ) ? null : CDTXMania.Songs管理.list曲ルート[ nMatched ];
					this.t現在選択中の曲を元に曲バーを再構成する();
#endif
					return;
				}
			}
			this.On非活性化();
			this.r現在選択中の曲 = null;
			this.On活性化();
		}


		/// <summary>
		/// 現在選曲している位置を検索する
		/// (曲一覧クラスを新しいものに入れ替える際に用いる)
		/// </summary>
		/// <param name="ln">検索対象のList</param>
		/// <param name="bc">検索するパンくずリスト(文字列)</param>
		/// <returns></returns>
		private C曲リストノード searchCurrentBreadcrumbsPosition( List<C曲リストノード> ln, string bc )
		{
			foreach (C曲リストノード n in ln)
			{
				if ( n.strBreadcrumbs == bc )
				{
					return n;
				}
				else if ( n.list子リスト != null && n.list子リスト.Count > 0 )	// 子リストが存在するなら、再帰で探す
				{
					C曲リストノード r = searchCurrentBreadcrumbsPosition( n.list子リスト, bc );
					if ( r != null ) return r;
				}
			}
			return null;
		}

		/// <summary>
		/// BOXのアイテム数と、今何番目を選択しているかをセットする
		/// </summary>
		public void t選択曲が変更された( bool bForce )	// #27648
		{
			C曲リストノード song = TJAPlayer3.stage選曲.r現在選択中の曲;
			if ( song == null )
				return;
			if ( song == song_last && bForce == false )
				return;
				
			song_last = song;
			List<C曲リストノード> list = TJAPlayer3.Songs管理.list曲ルート;
			int index = list.IndexOf( song ) + 1;
			if ( index <= 0 )
			{
				nCurrentPosition = nNumOfItems = 0;
            }
            else
            {
                nCurrentPosition = index;
                nNumOfItems = list.Count;
            }
            TJAPlayer3.stage選曲.act演奏履歴パネル.tSongChange();
        }

        // CActivity 実装

        public override void On活性化()
        {
            if (this.b活性化してる)
                return;

            // Reset to not performing calibration each time we
            // enter or return to the song select screen.
            TJAPlayer3.IsPerformingCalibration = false;



            this.e楽器パート = E楽器パート.DRUMS;
			this.b登場アニメ全部完了 = false;
			this.n目標のスクロールカウンタ = 0;
			this.n現在のスクロールカウンタ = 0;
			this.nスクロールタイマ = -1;

			// フォント作成。
			// 曲リスト文字は２倍（面積４倍）でテクスチャに描画してから縮小表示するので、フォントサイズは２倍とする。

			FontStyle regular = FontStyle.Regular;
			this.ft曲リスト用フォント = new Font( TJAPlayer3.ConfigIni.FontName, 40f, regular, GraphicsUnit.Pixel );
			

			// 現在選択中の曲がない（＝はじめての活性化）なら、現在選択中の曲をルートの先頭ノードに設定する。

			if( ( this.r現在選択中の曲 == null ) && ( TJAPlayer3.Songs管理.list曲ルート.Count > 0 ) )
				this.r現在選択中の曲 = TJAPlayer3.Songs管理.list曲ルート[ 0 ];

			if (!string.IsNullOrEmpty(TJAPlayer3.ConfigIni.FontName))
			{
				this.pfMusicName = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 22);
				this.pfSubtitle = new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 13,16);
			}
			else
			{
				this.pfMusicName = new CPrivateFastFont(new FontFamily("MS UI Gothic"), 22);
				this.pfSubtitle = new CPrivateFastFont(new FontFamily("MS UI Gothic"), 13,16);
			}

			ctBarAnime = new CCounter();
			ctBarAnime2 = new CCounter();

			// バー情報を初期化する。

			this.tバーの初期化();

			this.ct三角矢印アニメ = new CCounter(0, 1000, 1, TJAPlayer3.Timer);
			ct三角矢印アニメ.t開始();
			base.On活性化();

			this.t選択曲が変更された(true);		// #27648 2012.3.31 yyagi 選曲画面に入った直後の 現在位置/全アイテム数 の表示を正しく行うため
		}
		public override void On非活性化()
		{
			if( this.b活性化してない )
				return;

			TJAPlayer3.t安全にDisposeする(ref pfMusicName);
			TJAPlayer3.t安全にDisposeする(ref pfSubtitle);

			TJAPlayer3.t安全にDisposeする( ref this.ft曲リスト用フォント );


			for( int i = 0; i < 13; i++ )
				this.ct登場アニメ用[ i ] = null;

            this.ct三角矢印アニメ = null;

			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( this.b活性化してない )
				return;

			//this.tx曲名バー.Score = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_bar score.png" ), false );
			//this.tx曲名バー.Box = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_bar box.png" ), false );
			//this.tx曲名バー.Other = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_bar other.png" ), false );
			//this.tx選曲バー.Score = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_bar score selected.png" ), false );
			//this.tx選曲バー.Box = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_bar box selected.png" ), false );
			//this.tx選曲バー.Other = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_bar other selected.png" ), false );
			//this.txスキル数字 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_skill number on list.png" ), false );

            //this.tx曲バー_JPOP = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_JPOP.png" ), false );
            //this.tx曲バー_アニメ = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_anime.png" ), false );
            //this.tx曲バー_ゲーム = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_game.png" ), false );
            //this.tx曲バー_ナムコ = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_namco.png" ), false );
            //this.tx曲バー_クラシック = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_classic.png" ), false );
            //this.tx曲バー_バラエティ = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_variety.png" ), false );
            //this.tx曲バー_どうよう = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_child.png" ), false );
            //this.tx曲バー_ボカロ = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_vocaloid.png" ), false );
            //this.tx曲バー = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard.png" ), false );

            //this.tx曲バー_難易度[0] = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_Easy.png" ) );
            //this.tx曲バー_難易度[1] = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_Normal.png" ) );
            //this.tx曲バー_難易度[2] = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_Hard.png" ) );
            //this.tx曲バー_難易度[3] = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_Master.png" ) );
            //this.tx曲バー_難易度[4] = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_Edit.png" ) );

            //this.tx難易度星 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_levelstar.png" ), false );
            //this.tx難易度パネル = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_level_panel.png" ), false );
            //this.tx譜面分岐曲バー用 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_songboard_branch.png" ) );
            //this.tx譜面分岐中央パネル用 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_center panel_branch.png" ) );
            //this.txバー中央 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_center panel.png" ) );
            //this.tx上部ジャンル名 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_genrename.png" ) );
            //this.txレベル数字フォント = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_levelfont.png" ) );

            //this.txカーソル左 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_cursor left.png" ) );
            //this.txカーソル右 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\5_cursor right.png" ) );

			for( int i = 0; i < 13; i++ )
            {
                //this.t曲名バーの生成(i, this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);
                this.stバー情報[ i ].ttkタイトル = this.ttk曲名テクスチャを生成する( this.stバー情報[ i ].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);
            }

			int c = ( CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja" ) ? 0 : 1;
			#region [ Songs not found画像 ]
			try
			{
				using( Bitmap image = new Bitmap( 640, 128 ) )
				using( Graphics graphics = Graphics.FromImage( image ) )
				{
					string[] s1 = { "曲データが見つかりません。", "Songs not found." };
					string[] s2 = { "曲データをDTXManiaGR.exe以下の", "You need to install songs." };
					string[] s3 = { "フォルダにインストールして下さい。", "" };
					graphics.DrawString( s1[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float) 2f, (float) 2f );
					graphics.DrawString( s1[c], this.ft曲リスト用フォント, Brushes.White, (float) 0f, (float) 0f );
					graphics.DrawString( s2[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float) 2f, (float) 44f );
					graphics.DrawString( s2[c], this.ft曲リスト用フォント, Brushes.White, (float) 0f, (float) 42f );
					graphics.DrawString( s3[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float) 2f, (float) 86f );
					graphics.DrawString( s3[c], this.ft曲リスト用フォント, Brushes.White, (float) 0f, (float) 84f );

					this.txSongNotFound = new CTexture(image);

					this.txSongNotFound.vc拡大縮小倍率 = new PointF(0.5f, 0.5f);	// 半分のサイズで表示する。
				}
			}
			catch
			{
			}
			#endregion
			#region [ "曲データを検索しています"画像 ]
			try
			{
				using ( Bitmap image = new Bitmap( 640, 96 ) )
				using ( Graphics graphics = Graphics.FromImage( image ) )
				{
					string[] s1 = { "曲データを検索しています。", "Now enumerating songs." };
					string[] s2 = { "そのまましばらくお待ち下さい。", "Please wait..." };
					graphics.DrawString( s1[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float) 2f, (float) 2f );
					graphics.DrawString( s1[c], this.ft曲リスト用フォント, Brushes.White, (float) 0f, (float) 0f );
					graphics.DrawString( s2[c], this.ft曲リスト用フォント, Brushes.DarkGray, (float) 2f, (float) 44f );
					graphics.DrawString( s2[c], this.ft曲リスト用フォント, Brushes.White, (float) 0f, (float) 42f );
					this.txEnumeratingSongs = new CTexture(image);

					this.txEnumeratingSongs.vc拡大縮小倍率 = new PointF(0.5f, 0.5f);  // 半分のサイズで表示する。
				}
			}
			catch
			{
				Trace.TraceError( "txEnumeratingSongsテクスチャの作成に失敗しました。" );
				this.txEnumeratingSongs = null;
			}
			#endregion
			#region [ 曲数表示 ]
			//this.txアイテム数数字 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\ScreenSelect skill number on gauge etc.png" ), false );
			#endregion
			base.OnManagedリソースの作成();
		}
		public override void OnManagedリソースの解放()
		{
			if( this.b活性化してない )
				return;

			//CDTXMania.t安全にDisposeする( ref this.txアイテム数数字 );

			for( int i = 0; i < 13; i++ )
            {
                TJAPlayer3.tテクスチャの解放( ref this.stバー情報[ i ].txタイトル名 );
                this.stバー情報[ i ].ttkタイトル = null;
            }

			ClearTitleTextureCache();

			//CDTXMania.t安全にDisposeする( ref this.txスキル数字 );
			TJAPlayer3.tテクスチャの解放( ref this.txEnumeratingSongs );
            TJAPlayer3.tテクスチャの解放( ref this.txSongNotFound );
            //CDTXMania.t安全にDisposeする( ref this.tx曲名バー.Score );
            //CDTXMania.t安全にDisposeする( ref this.tx曲名バー.Box );
            //CDTXMania.t安全にDisposeする( ref this.tx曲名バー.Other );
            //CDTXMania.t安全にDisposeする( ref this.tx選曲バー.Score );
            //CDTXMania.t安全にDisposeする( ref this.tx選曲バー.Box );
            //CDTXMania.t安全にDisposeする( ref this.tx選曲バー.Other );

            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_JPOP );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_アニメ );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_ゲーム );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_ナムコ );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_クラシック );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_どうよう );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_バラエティ );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー_ボカロ );
            //CDTXMania.tテクスチャの解放( ref this.tx曲バー );
            //CDTXMania.tテクスチャの解放( ref this.tx譜面分岐曲バー用 );

       		//for( int i = 0; i < 5; i++ )
         //   {
         //       CDTXMania.tテクスチャの解放( ref this.tx曲バー_難易度[ i ] );
         //   }

         //   CDTXMania.tテクスチャの解放( ref this.tx難易度パネル );
         //   CDTXMania.tテクスチャの解放( ref this.txバー中央 );
         //   CDTXMania.tテクスチャの解放( ref this.tx難易度星 );
         //   CDTXMania.tテクスチャの解放( ref this.tx譜面分岐中央パネル用 );
         //   CDTXMania.tテクスチャの解放( ref this.tx上部ジャンル名 );
         //   CDTXMania.tテクスチャの解放( ref this.txレベル数字フォント );

         //   CDTXMania.tテクスチャの解放( ref this.txカーソル左 );
         //   CDTXMania.tテクスチャの解放( ref this.txカーソル右 );

			base.OnManagedリソースの解放();
		}
		public override int On進行描画()
		{
			if (this.b活性化してない)
				return 0;

			#region [ 初めての進行描画 ]
			//-----------------
			if (this.b初めての進行描画)
			{
				for (int i = 0; i < 13; i++)
					this.ct登場アニメ用[i] = new CCounter(-i * 10, 100, 3, TJAPlayer3.Timer);

				this.nスクロールタイマ = DX.GetNowCount();
				TJAPlayer3.stage選曲.t選択曲変更通知();

				this.n矢印スクロール用タイマ値 = DX.GetNowCount();
				base.b初めての進行描画 = false;
			}
			//-----------------
			#endregion

		
			// まだ選択中の曲が決まってなければ、曲ツリールートの最初の曲にセットする。

			if ((this.r現在選択中の曲 == null) && (TJAPlayer3.Songs管理.list曲ルート.Count > 0))
				this.r現在選択中の曲 = TJAPlayer3.Songs管理.list曲ルート[0];

			// 進行。
			if (n現在のスクロールカウンタ == 0)
				ct三角矢印アニメ.t進行Loop();
			else
				ct三角矢印アニメ.n現在の値 = 0;


			#region [ (2) 通常フェーズの進行。]
			//-----------------
			long n現在時刻 = DX.GetNowCount();

			if (n現在時刻 < this.nスクロールタイマ) // 念のため
				this.nスクロールタイマ = n現在時刻;

			const int nアニメ間隔 = 2;
			while ((n現在時刻 - this.nスクロールタイマ) >= nアニメ間隔)
			{
				int n加速度 = 1;
				int n残距離 = Math.Abs((int)(this.n目標のスクロールカウンタ - this.n現在のスクロールカウンタ));

				#region [ 残距離が遠いほどスクロールを速くする（＝n加速度を多くする）。]
				//-----------------
				if (n残距離 <= 10)
				{
					n加速度 = 1;
				}
				else if (n残距離 <= 100)
				{
					n加速度 = 2;
				}
				else if (n残距離 <= 300)
				{
					n加速度 = 3;
				}
				else if (n残距離 <= 500)
				{
					n加速度 = 4;
				}
				else
				{
					n加速度 = 8;
				}
				//-----------------
				#endregion

				#region [ 加速度を加算し、現在のスクロールカウンタを目標のスクロールカウンタまで近づける。 ]
				//-----------------
				if (this.n現在のスクロールカウンタ < this.n目標のスクロールカウンタ)        // (A) 正の方向に未達の場合：
				{
					this.n現在のスクロールカウンタ += n加速度;                             // カウンタを正方向に移動する。

					if (this.n現在のスクロールカウンタ > this.n目標のスクロールカウンタ)
						this.n現在のスクロールカウンタ = this.n目標のスクロールカウンタ;    // 到着！スクロール停止！
				}

				else if (this.n現在のスクロールカウンタ > this.n目標のスクロールカウンタ)   // (B) 負の方向に未達の場合：
				{
					this.n現在のスクロールカウンタ -= n加速度;                             // カウンタを負方向に移動する。

					if (this.n現在のスクロールカウンタ < this.n目標のスクロールカウンタ)    // 到着！スクロール停止！
						this.n現在のスクロールカウンタ = this.n目標のスクロールカウンタ;
				}
				//-----------------
				#endregion

				if (this.n現在のスクロールカウンタ >= 150)      // １行＝100カウント。
				{
					#region [ パネルを１行上にシフトする。]
					//-----------------

					// 選択曲と選択行を１つ下の行に移動。

					this.r現在選択中の曲 = this.r次の曲(this.r現在選択中の曲);
					this.n現在の選択行 = (this.n現在の選択行 + 1) % 13;


					// 選択曲から７つ下のパネル（＝新しく最下部に表示されるパネル。消えてしまう一番上のパネルを再利用する）に、新しい曲の情報を記載する。

					C曲リストノード song = this.r現在選択中の曲;
					for (int i = 0; i < 7; i++)
						song = this.r次の曲(song);

					int index = (this.n現在の選択行 + 7) % 13;    // 新しく最下部に表示されるパネルのインデックス（0～12）。
					this.stバー情報[index].strタイトル文字列 = song.strタイトル;
					this.stバー情報[index].ForeColor = song.ForeColor;
					this.stバー情報[index].BackColor = song.BackColor;
					this.stバー情報[index].strジャンル = song.strジャンル;
					this.stバー情報[index].strサブタイトル = song.strサブタイトル;
					this.stバー情報[index].ar難易度 = song.nLevel;
				
					for (int f = 0; f < (int)Difficulty.Total; f++)
					{
						if (song.arスコア[f] != null)
							this.stバー情報[index].b分岐 = song.arスコア[f].譜面情報.b譜面分岐;
					}


					// stバー情報[] の内容を1行ずつずらす。

					C曲リストノード song2 = this.r現在選択中の曲;
					for (int i = 0; i < 5; i++)
						song2 = this.r前の曲(song2);

					for (int i = 0; i < 13; i++)
					{
						int n = (((this.n現在の選択行 - 5) + i) + 13) % 13;
						this.stバー情報[n].eバー種別 = this.e曲のバー種別を返す(song2);
						song2 = this.r次の曲(song2);
						this.stバー情報[i].ttkタイトル = this.ttk曲名テクスチャを生成する(this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);

					}


					// 新しく最下部に表示されるパネル用のスキル値を取得。

					for (int i = 0; i < 3; i++)
						this.stバー情報[index].nスキル値[i] = (int)song.arスコア[this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す(song)].譜面情報.最大スキル[i];


					// 1行(100カウント)移動完了。

					this.n現在のスクロールカウンタ -= 150;
					this.n目標のスクロールカウンタ -= 150;

					this.t選択曲が変更された(false);             // スクロールバー用に今何番目を選択しているかを更新



					if (this.n目標のスクロールカウンタ == 0)
						TJAPlayer3.stage選曲.t選択曲変更通知();      // スクロール完了＝選択曲変更！

					//-----------------
					#endregion
				}
				else if (this.n現在のスクロールカウンタ <= -150)
				{
					#region [ パネルを１行下にシフトする。]
					//-----------------

					// 選択曲と選択行を１つ上の行に移動。

					this.r現在選択中の曲 = this.r前の曲(this.r現在選択中の曲);
					this.n現在の選択行 = ((this.n現在の選択行 - 1) + 13) % 13;


					// 選択曲から５つ上のパネル（＝新しく最上部に表示されるパネル。消えてしまう一番下のパネルを再利用する）に、新しい曲の情報を記載する。

					C曲リストノード song = this.r現在選択中の曲;
					for (int i = 0; i < 5; i++)
						song = this.r前の曲(song);

					int index = ((this.n現在の選択行 - 5) + 13) % 13; // 新しく最上部に表示されるパネルのインデックス（0～12）。
					this.stバー情報[index].strタイトル文字列 = song.strタイトル;
					this.stバー情報[index].ForeColor = song.ForeColor;
					this.stバー情報[index].BackColor = song.BackColor;
					this.stバー情報[index].strサブタイトル = song.strサブタイトル;
					this.stバー情報[index].strジャンル = song.strジャンル;
					this.stバー情報[index].ar難易度 = song.nLevel;
					
					for (int f = 0; f < (int)Difficulty.Total; f++)
					{
						if (song.arスコア[f] != null)
							this.stバー情報[index].b分岐 = song.arスコア[f].譜面情報.b譜面分岐;
					}

					// stバー情報[] の内容を1行ずつずらす。

					C曲リストノード song2 = this.r現在選択中の曲;
					for (int i = 0; i < 5; i++)
						song2 = this.r前の曲(song2);

					for (int i = 0; i < 13; i++)
					{
						int n = (((this.n現在の選択行 - 5) + i) + 13) % 13;
						this.stバー情報[n].eバー種別 = this.e曲のバー種別を返す(song2);
						song2 = this.r次の曲(song2);
						this.stバー情報[i].ttkタイトル = this.ttk曲名テクスチャを生成する(this.stバー情報[i].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);
					}


					// 新しく最上部に表示されるパネル用のスキル値を取得。

					for (int i = 0; i < 3; i++)
						this.stバー情報[index].nスキル値[i] = (int)song.arスコア[this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す(song)].譜面情報.最大スキル[i];

					this.n現在のスクロールカウンタ += 150;
					this.n目標のスクロールカウンタ += 150;

					this.t選択曲が変更された(false);             // スクロールバー用に今何番目を選択しているかを更新

					this.ttk選択している曲の曲名 = null;
					this.ttk選択している曲のサブタイトル = null;

					if (this.n目標のスクロールカウンタ == 0)
						TJAPlayer3.stage選曲.t選択曲変更通知();      // スクロール完了＝選択曲変更！
															//-----------------
					#endregion
				}

				if (this.b選択曲が変更された && n現在のスクロールカウンタ == 0)
				{
					if (this.ttk選択している曲の曲名 != null)
					{
						this.ttk選択している曲の曲名 = null;
						this.b選択曲が変更された = false;
					}
					if (this.ttk選択している曲のサブタイトル != null)
					{
						this.ttk選択している曲のサブタイトル = null;
						this.b選択曲が変更された = false;
                    }
                }
                this.nスクロールタイマ += nアニメ間隔;
            }
            //-----------------
            #endregion

            // 描画。

            if (this.r現在選択中の曲 == null)
            {
                #region [ 曲が１つもないなら「Songs not found.」を表示してここで帰れ。]
                //-----------------
                if (bIsEnumeratingSongs)
                {
                    this.txEnumeratingSongs?.t2D描画(320, 160);
                }
                else
                {
                    this.txSongNotFound?.t2D描画(320, 160);
                }
                //-----------------
                #endregion

                return 0;
            }



            #region [ (2) 通常フェーズの描画。]
            //-----------------
            for (int i = 0; i < 13; i++)    // パネルは全13枚。
            {
                if ((i == 0 && this.n現在のスクロールカウンタ > 0) ||       // 最上行は、上に移動中なら表示しない。
                    (i == 12 && this.n現在のスクロールカウンタ < 0))        // 最下行は、下に移動中なら表示しない。
                    continue;

                int nパネル番号 = (((this.n現在の選択行 - 5) + i) + 13) % 13;
                int n見た目の行番号 = i;
                int n次のパネル番号 = (this.n現在のスクロールカウンタ <= 0) ? ((i + 1) % 13) : (((i - 1) + 13) % 13);
                int xAnime = this.ptバーの座標[n見た目の行番号].X + ((int)((this.ptバーの座標[n次のパネル番号].X - this.ptバーの座標[n見た目の行番号].X) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 150.0)));
                int yAnime = this.ptバーの座標[n見た目の行番号].Y + ((int)((this.ptバーの座標[n次のパネル番号].Y - this.ptバーの座標[n見た目の行番号].Y) * (((double)Math.Abs(this.n現在のスクロールカウンタ)) / 150.0)));

                {
                    // (B) スクロール中の選択曲バー、またはその他のバーの描画。

                    #region [ バーテクスチャを描画。]
                    //-----------------
                    if (n現在のスクロールカウンタ != 0)
                    {
                        this.tジャンル別選択されていない曲バーの描画(xAnime, yAnime, this.stバー情報[nパネル番号].strジャンル, i);
                    }
                    else if (n見た目の行番号 != 5)
                    {
                        this.tジャンル別選択されていない曲バーの描画(xAnime, yAnime, this.stバー情報[nパネル番号].strジャンル, i);
                    }

                    if (this.stバー情報[nパネル番号].b分岐[TJAPlayer3.stage選曲.n現在選択中の曲の難易度] == true && n見た目の行番号 != 5)
                        TJAPlayer3.Tx.SongSelect_Branch.t2D描画(xAnime + 66, TJAPlayer3.Skin.SongSelect_Overall_Y - 5);
                    //-----------------
                    #endregion

                    #region [ タイトル名テクスチャを描画。]

					if(i == 5)
                    {
						float Y = ctBarAnime2.n現在の値 * (45f / 140f);
						float Max = 140 * (45f / 140f);
						if (ctBarAnime2.b進行中)
                        {

							if (n現在のスクロールカウンタ != 0)
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル)?.t2D拡大率考慮上中央基準描画(xAnime + 312, yAnime + 11 - Max + Y);
							else if (n見た目の行番号 != 5)
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル)?.t2D拡大率考慮上中央基準描画(xAnime + 312, yAnime + 11 - Max + Y);
						}
                        else
                        {
							if (n現在のスクロールカウンタ != 0)
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル)?.t2D拡大率考慮上中央基準描画(xAnime + 312, yAnime + 11);
							else if (n見た目の行番号 != 5)
								ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル)?.t2D拡大率考慮上中央基準描画(xAnime + 312, yAnime + 11);
						}
					}
                    else
                    {
						if (n現在のスクロールカウンタ != 0)
							ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル)?.t2D拡大率考慮上中央基準描画(xAnime + 312, yAnime + 11);
						else if (n見た目の行番号 != 5)
							ResolveTitleTexture(this.stバー情報[nパネル番号].ttkタイトル)?.t2D拡大率考慮上中央基準描画(xAnime + 312, yAnime + 11);
					}
				
					

					#endregion

					if (this.stバー情報[nパネル番号].ar難易度 != null)
					{
						int nX補正 = 0;
						if (this.stバー情報[nパネル番号].ar難易度[TJAPlayer3.stage選曲.n現在選択中の曲の難易度].ToString().Length == 2)
							nX補正 = -6;
						this.t小文字表示(xAnime + 65 + nX補正, 559, this.stバー情報[nパネル番号].ar難易度[TJAPlayer3.stage選曲.n現在選択中の曲の難易度].ToString());
					}
					//-----------------						
				}
			}
			#endregion

			ctBarAnime.t進行();
			ctBarAnime2.t進行();
			if(ctBarAnime.n現在の値 == ctBarAnime.n終了値)
            {
				bAniEnd = true;
            }

			if (this.n現在のスクロールカウンタ == 0)
			{
				#region [ Bar_Center ] 
				{
					if (!ctBarAnime.b進行中)
						ctBarAnime.t開始(0, 600, 1, TJAPlayer3.Timer);

					int bargenreindex = CStrジャンルtoNum.ForAC16(r現在選択中の曲.strジャンル);
					float y = 0;

					if (ctBarAnime.n現在の値 >= 430 && ctBarAnime.n現在の値 <= 500)
						y = (ctBarAnime.n現在の値 - 430f) * (60f / 70f);
                    else if (ctBarAnime.n現在の値 >= 500)
						y = (500 - 430f) * (60f / 70f);

					//中の拡大率
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex].vc拡大縮小倍率.Y = 1f + y;
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W.vc拡大縮小倍率.Y = 1f + y;
					//中上
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(328, 317 + 43 - y, new Rectangle(0, 43 , 624, 1));
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(328, 317 + 43 - y, new Rectangle(0, 43, 624, 1));
					//中下
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(328, 317 + 43, new Rectangle(0, 43, 624, 1));
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(328, 317 + 43, new Rectangle(0, 43, 624, 1));

					//拡大率を元に戻す
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex].vc拡大縮小倍率.Y = 1f;
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W.vc拡大縮小倍率.Y = 1f;
                    //上
                    TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(328, 317 - y, new Rectangle(0, 0, 624, 43));
                    TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(328, 317 - y, new Rectangle(0, 0, 624, 43));
                    //下
                    TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(328, 317 + 43 + y, new Rectangle(0, 43, 624, 43));
                    TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(328, 317 + 43 + y, new Rectangle(0, 43, 624, 43));


                }
                #endregion

                switch (r現在選択中の曲.eノード種別)
                {
                    case C曲リストノード.Eノード種別.SCORE:

                        #region [ Comment ]
                        /*
						if (TJAPlayer3.Tx.SongSelect_Frame_Score != null)
						{
							// 難易度がTower、Danではない
							if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != (int)Difficulty.Dan)
							{
								for (int i = 0; i < (int)Difficulty.Edit + 1; i++)
								{
									if (TJAPlayer3.stage選曲.r現在選択中の曲.nLevel[i] >= 0)
									{
										// レベルが0以上
										TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = System.Drawing.Color.FromArgb(255, 255, 255);
										if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 == 4)
										{
											// エディット
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
										}
										else if (i != 4)
										{
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
										}
									}
									else
									{
										// レベルが0未満 = 譜面がないとみなす
										TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = System.Drawing.Color.FromArgb(122, 122, 122);
										if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 == 4)
										{
											// エディット
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
										}
										else if (i != 4)
										{
											TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(60 * i, 0, 60, 360));
										}
									}
								}
							}
							else
							{
								if (TJAPlayer3.stage選曲.r現在選択中の曲.nLevel[TJAPlayer3.stage選曲.n現在選択中の曲の難易度] >= 0)
								{
									// 譜面がありますね
									TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = System.Drawing.Color.FromArgb(255, 255, 255);
									TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(494 + 120, TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(0, 360 + (360 * (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 - (int)Difficulty.Tower)), TJAPlayer3.Tx.SongSelect_Frame_Score.szテクスチャサイズ.Width, 360));
								}
								else
								{
									// ないですね
									TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = System.Drawing.Color.FromArgb(122, 122, 122);
									TJAPlayer3.Tx.SongSelect_Frame_Score.t2D下中央基準描画(494 + 120, TJAPlayer3.Skin.SongSelect_Overall_Y + 463, new Rectangle(0, 360 + (360 * (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 - (int)Difficulty.Tower)), TJAPlayer3.Tx.SongSelect_Frame_Score.szテクスチャサイズ.Width, 360));
								}
							}
						}
						#region[ 星 ]
						if (TJAPlayer3.Tx.SongSelect_Level != null)
						{
							// 全難易度表示
							// 難易度がTower、Danではない
							if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != (int)Difficulty.Dan)
							{
								for (int i = 0; i < (int)Difficulty.Edit + 1; i++)
								{
									for (int n = 0; n < TJAPlayer3.stage選曲.r現在選択中の曲.nLevel[i]; n++)
									{
										// 星11以上はループ終了
										//if (n > 9) break;
										// 裏なら鬼と同じ場所に
										if (i == 3 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 == 4) break;
										if (i == 4 && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 == 4)
										{
											TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (n * 17), new Rectangle(32 * i, 0, 32, 32));
										}
										if (i != 4)
										{
											TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(494 + (i * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (n * 17), new Rectangle(32 * i, 0, 32, 32));
										}
									}
								}
							}
							else
							{
								for (int i = 0; i < TJAPlayer3.stage選曲.r現在選択中の曲.nLevel[TJAPlayer3.stage選曲.n現在選択中の曲の難易度]; i++)
								{
									TJAPlayer3.Tx.SongSelect_Level.t2D下中央基準描画(494, TJAPlayer3.Skin.SongSelect_Overall_Y + 413 - (i * 17), new Rectangle(32 * TJAPlayer3.stage選曲.n現在選択中の曲の難易度, 0, 32, 32));
								}
							}
						}
						#endregion
						#region 選択カーソル
						if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != (int)Difficulty.Tower && TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != (int)Difficulty.Dan)
						{
							if (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 != 4)
							{
								TJAPlayer3.Tx.SongSelect_Score_Select?.t2D下中央基準描画(494 + (TJAPlayer3.stage選曲.n現在選択中の曲の難易度 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 443);
							}
							else
							{
								TJAPlayer3.Tx.SongSelect_Score_Select?.t2D下中央基準描画(494 + (3 * 60), TJAPlayer3.Skin.SongSelect_Overall_Y + 443);

							}
						}
						#endregion

						*/
                        #endregion

                        for (int i = 0; i < 4; i++)
                        {
							float op = 0;

							if (ctBarAnime.n現在の値 >= 500)
								op = (ctBarAnime.n現在の値 - 500f) * (255f / 100f);

							if (TJAPlayer3.stage選曲.r現在選択中の曲.nLevel[i] >= 0)
								TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = Color.FromArgb(0x40, 255, 255, 255);
							else
								TJAPlayer3.Tx.SongSelect_Frame_Score.color4 = Color.FromArgb(0x40, 122, 122, 122);
							TJAPlayer3.Tx.SongSelect_Frame_Score.Opacity = (int)op;
							TJAPlayer3.Tx.SongSelect_Frame_Score?.t2D描画(394 + 121 * i, 380, new Rectangle(i * 130, 0, 130, 59));							

							TJAPlayer3.Tx.SongSelect_Level.Opacity = (int)op;
							TJAPlayer3.Tx.SongSelect_Level?.t2D描画(487 + 121 * i, 396, new Rectangle(TJAPlayer3.stage選曲.r現在選択中の曲.nLevel[i] * 24, 0, 26, 26));
							
						}

                        break;

                    case C曲リストノード.Eノード種別.BOX:

                        break;

					case C曲リストノード.Eノード種別.BACKBOX:
						
						break;

					case C曲リストノード.Eノード種別.RANDOM:
						
						break;
						
				}
	
				if (TJAPlayer3.Tx.SongSelect_Branch_Text != null && TJAPlayer3.stage選曲.r現在選択中のスコア.譜面情報.b譜面分岐[TJAPlayer3.stage選曲.n現在選択中の曲の難易度])
					TJAPlayer3.Tx.SongSelect_Branch_Text.t2D描画(483, TJAPlayer3.Skin.SongSelect_Overall_Y + 21);

			}
            else
            {
				ctBarAnime.t停止();
				ctBarAnime.n現在の値 = 0;
            }

			for (int i = 0; i < 13; i++)    // パネルは全13枚。
			{
                if ((i == 0 && this.n現在のスクロールカウンタ > 0) ||       // 最上行は、上に移動中なら表示しない。
                    (i == 12 && this.n現在のスクロールカウンタ < 0))        // 最下行は、下に移動中なら表示しない。
                    continue;

                int nパネル番号 = (((this.n現在の選択行 - 5) + i) + 13) % 13;

                if ((i == 5) && (this.n現在のスクロールカウンタ == 0))
                {
                    // (A) スクロールが停止しているときの選択曲バーの描画。

                    #region [ タイトル名テクスチャを描画。]
                    //-----------------
                    if (this.stバー情報[nパネル番号].strタイトル文字列 != "" && this.ttk選択している曲の曲名 == null)
                        this.ttk選択している曲の曲名 = this.ttk曲名テクスチャを生成する(this.stバー情報[nパネル番号].strタイトル文字列, this.stバー情報[nパネル番号].ForeColor, this.stバー情報[nパネル番号].BackColor);
                    if (this.stバー情報[nパネル番号].strサブタイトル != "" && this.ttk選択している曲のサブタイトル == null)
                        this.ttk選択している曲のサブタイトル = this.ttkサブタイトルテクスチャを生成する(this.stバー情報[nパネル番号].strサブタイトル, this.stバー情報[nパネル番号].ForeColor, this.stバー情報[nパネル番号].BackColor);

                    float y = 0;

                    if (ctBarAnime.n現在の値 >= 430 && ctBarAnime.n現在の値 <= 500)
                        y = (ctBarAnime.n現在の値 - 430f) * (45f / 70f);
                    else if (ctBarAnime.n現在の値 >= 500)
                        y = (500 - 430f) * (45f / 70f);

                    if (this.ttk選択している曲のサブタイトル != null)
                    {
						float op = 0;
						if (ctBarAnime.n現在の値 >= 500)
							op = (ctBarAnime.n現在の値 - 500f) * (255f / 100f);
						CTexture tx選択している曲のサブタイトル = ResolveTitleTexture(ttk選択している曲のサブタイトル);
						tx選択している曲のサブタイトル.Opacity = (int)op;
                        tx選択している曲のサブタイトル?.t2D拡大率考慮上中央基準描画(640, 328);
                    }

                    ResolveTitleTexture(this.ttk選択している曲の曲名)?.t2D拡大率考慮上中央基準描画(640, 328 - (int)y);
                    //-----------------
                    #endregion
                }

            }
			//-----------------			

			if (this.e曲のバー種別を返す(this.r現在選択中の曲) == Eバー種別.Score && this.nStrジャンルtoNum(this.r現在選択中の曲.strジャンル) != 8)
			{
				if (TJAPlayer3.Tx.SongSelect_GenreText != null)
					TJAPlayer3.Tx.SongSelect_GenreText.t2D描画(496, TJAPlayer3.Skin.SongSelect_Overall_Y - 64, new Rectangle(0, 60 * this.nStrジャンルtoNum(this.r現在選択中の曲.strジャンル), 288, 60));
			}
			return 0;
		}
		
		


		// その他

		#region [ private ]
		//-----------------
		private enum Eバー種別 { Score, Box, Other }

		private struct STバー
		{
			public CTexture Score;
			public CTexture Box;
			public CTexture Other;
			public CTexture this[ int index ]
			{
				get
				{
					switch( index )
					{
						case 0:
							return this.Score;

						case 1:
							return this.Box;

						case 2:
							return this.Other;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch( index )
					{
						case 0:
							this.Score = value;
							return;

						case 1:
							this.Box = value;
							return;

						case 2:
							this.Other = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}

		private struct STバー情報
		{
			public CActSelect曲リスト.Eバー種別 eバー種別;
			public string strタイトル文字列;
			public CTexture txタイトル名;
			public STDGBVALUE<int> nスキル値;
			public Color col文字色;
			public Color ForeColor;
			public Color BackColor;
			public int[] ar難易度;
			public bool[] b分岐;
			public string strジャンル;
			public string strサブタイトル;
			public TitleTextureKey ttkタイトル;
			internal int[] Level;
		}

		private struct ST選曲バー
		{
			public CTexture Score;
			public CTexture Box;            
			public CTexture Other;
            public CTexture this[ int index ]
			{
				get
				{
					switch( index )
					{
						case 0:
							return this.Score;

						case 1:
							return this.Box;

						case 2:
							return this.Other;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch( index )
					{
						case 0:
							this.Score = value;
							return;

						case 1:
							this.Box = value;
							return;

						case 2:
							this.Other = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}

		public CCounter ctBarAnime;
		public CCounter ctBarAnime2;
		public bool bAniEnd = false;

        public bool b選択曲が変更された = true;
		private bool b登場アニメ全部完了;
		private Color color文字影 = Color.FromArgb( 0x40, 10, 10, 10 );
		private CCounter[] ct登場アニメ用 = new CCounter[ 13 ];
        private CCounter ct三角矢印アニメ;
		private CPrivateFastFont pfMusicName;
		private CPrivateFastFont pfSubtitle;

		// 2018-09-17 twopointzero: I can scroll through 2300 songs consuming approx. 200MB of memory.
		//                          I have set the title texture cache size to a nearby round number (2500.)
		//                          If we'd like title textures to take up no more than 100MB, for example,
		//                          then a cache size of 1000 would be roughly correct.
		private readonly LurchTable<TitleTextureKey, CTexture> _titleTextures =  new LurchTable<TitleTextureKey, CTexture>(LurchTableOrder.Access, 2500);

		private E楽器パート e楽器パート;
		private Font ft曲リスト用フォント;
		private long nスクロールタイマ;
		private int n現在のスクロールカウンタ;
		private int n現在の選択行;
		private int n目標のスクロールカウンタ;
        private readonly Point[] ptバーの基本座標 = new Point[] { new Point( 0x2c4, 5 ), new Point( 0x272, 56 ), new Point( 0x242, 107 ), new Point( 0x222, 158 ), new Point( 0x210, 209 ), new Point( 0x1d0, 270 ), new Point( 0x224, 362 ), new Point( 0x242, 413 ), new Point( 0x270, 464 ), new Point( 0x2ae, 515 ), new Point( 0x314, 566 ), new Point( 0x3e4, 617 ), new Point( 0x500, 668 ) };
		private Point[] ptバーの座標 = new Point[]
		  { new Point( 215, -118 ), new Point( 215, -118 ), new Point( 241, -32  ), new Point( 268, 57 ), new Point( 294, 147 ),
			new Point( 328, 317 ),
			new Point( 369, 486 ), new Point( 392, 576 ), new Point( 415, 666  ), new Point( 428, 756 ), new Point( 428, 781 ), new Point( 428, 781 ), new Point( 428, 781 ) };

		private STバー情報[] stバー情報 = new STバー情報[ 13 ];
		private CTexture txSongNotFound, txEnumeratingSongs;
		//private CTexture txスキル数字;
		//private CTexture txアイテム数数字;
		//private STバー tx曲名バー;
		//private ST選曲バー tx選曲バー;
  //      private CTexture txバー中央;
        private TitleTextureKey ttk選択している曲の曲名;
        private TitleTextureKey ttk選択している曲のサブタイトル;

        //private CTexture tx曲バー_アニメ;
        //private CTexture tx曲バー_JPOP;
        //private CTexture tx曲バー_クラシック;
        //private CTexture tx曲バー_ゲーム;
        //private CTexture tx曲バー_ナムコ;
        //private CTexture tx曲バー_バラエティ;
        //private CTexture tx曲バー_どうよう;
        //private CTexture tx曲バー_ボカロ;
        //private CTexture tx曲バー;

        private CTexture[] tx曲バー_難易度 = new CTexture[ 5 ];

        //private CTexture tx譜面分岐曲バー用;
        //private CTexture tx難易度パネル;
        //private CTexture tx上部ジャンル名;


        //private CTexture txカーソル左;
        //private CTexture txカーソル右;

        //private CTexture tx難易度星;
        //private CTexture tx譜面分岐中央パネル用;

        private long n矢印スクロール用タイマ値;

		private int nCurrentPosition = 0;
		private int nNumOfItems = 0;

		//private string strBoxDefSkinPath = "";


		private Eバー種別 e曲のバー種別を返す( C曲リストノード song )
		{
			if( song != null )
			{
				switch( song.eノード種別 )
				{
					case C曲リストノード.Eノード種別.SCORE:
					case C曲リストノード.Eノード種別.SCORE_MIDI:
						return Eバー種別.Score;

					case C曲リストノード.Eノード種別.BOX:
					case C曲リストノード.Eノード種別.BACKBOX:
						return Eバー種別.Box;
				}
			}
			return Eバー種別.Other;
		}
		private C曲リストノード r次の曲( C曲リストノード song )
		{
			if( song == null )
				return null;

			List<C曲リストノード> list = ( song.r親ノード != null ) ? song.r親ノード.list子リスト : TJAPlayer3.Songs管理.list曲ルート;
	
			int index = list.IndexOf( song );

			if( index < 0 )
				return null;

			if( index == ( list.Count - 1 ) )
				return list[ 0 ];

			return list[ index + 1 ];
		}
		private C曲リストノード r前の曲( C曲リストノード song )
		{
			if( song == null )
				return null;

			List<C曲リストノード> list = ( song.r親ノード != null ) ? song.r親ノード.list子リスト : TJAPlayer3.Songs管理.list曲ルート;

			int index = list.IndexOf( song );
	
			if( index < 0 )
				return null;

			if( index == 0 )
				return list[ list.Count - 1 ];

			return list[ index - 1 ];
		}
		private void tスキル値の描画( int x, int y, int nスキル値 )
		{
			if( nスキル値 <= 0 || nスキル値 > 100 )		// スキル値 0 ＝ 未プレイ なので表示しない。
				return;

			int color = ( nスキル値 == 100 ) ? 3 : ( nスキル値 / 25 );

			int n百の位 = nスキル値 / 100;
			int n十の位 = ( nスキル値 % 100 ) / 10;
			int n一の位 = ( nスキル値 % 100 ) % 10;


			// 百の位の描画。

			if( n百の位 > 0 )
				this.tスキル値の描画_１桁描画( x, y, n百の位, color );


			// 十の位の描画。

			if( n百の位 != 0 || n十の位 != 0 )
				this.tスキル値の描画_１桁描画( x + 7, y, n十の位, color );


			// 一の位の描画。

			this.tスキル値の描画_１桁描画( x + 14, y, n一の位, color );
		}
		private void tスキル値の描画_１桁描画( int x, int y, int n数値, int color )
		{
		}
		private void tバーの初期化()
		{
			C曲リストノード song = this.r現在選択中の曲;
			
			if( song == null )
				return;

			for( int i = 0; i < 5; i++ )
				song = this.r前の曲( song );

			for( int i = 0; i < 13; i++ )
			{
				this.stバー情報[ i ].strタイトル文字列 = song.strタイトル;
                this.stバー情報[ i ].strジャンル = song.strジャンル;
				this.stバー情報[ i ].col文字色 = song.col文字色;
                this.stバー情報[i].ForeColor = song.ForeColor;
                this.stバー情報[i].BackColor = song.BackColor;
				this.stバー情報[i].Level = song.nLevel;
				this.stバー情報[ i ].eバー種別 = this.e曲のバー種別を返す( song );
                this.stバー情報[ i ].strサブタイトル = song.strサブタイトル;
                this.stバー情報[ i ].ar難易度 = song.nLevel;
				
				for ( int f = 0; f < (int)Difficulty.Total; f++ )
                {
                    if( song.arスコア[ f ] != null )
                        this.stバー情報[ i ].b分岐 = song.arスコア[ f ].譜面情報.b譜面分岐;
                }
				
				for( int j = 0; j < 3; j++ )
					this.stバー情報[ i ].nスキル値[ j ] = (int) song.arスコア[ this.n現在のアンカ難易度レベルに最も近い難易度レベルを返す( song ) ].譜面情報.最大スキル[ j ];

                this.stバー情報[ i ].ttkタイトル = this.ttk曲名テクスチャを生成する( this.stバー情報[ i ].strタイトル文字列, this.stバー情報[i].ForeColor, this.stバー情報[i].BackColor);

				song = this.r次の曲( song );
			}

			this.n現在の選択行 = 5;
		}

		private void tジャンル別選択されていない曲バーの描画( int x, int y, string strジャンル,int i )
		{
			if( x >= 1280 || y >= 720)
				return;

            var rc = new Rectangle( 0, 48, 128, 48 );

			int bargenreindex = CStrジャンルtoNum.ForAC16(strジャンル);
			
			

			if(i == 5)
            {
                if (ctBarAnime2.b進行中)
                {
					float Y = ctBarAnime2.n現在の値 * (60f / 140f);
					float Max = 140 * (60f / 140f);

					//中の拡大率
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex].vc拡大縮小倍率.Y = Max - Y;
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W.vc拡大縮小倍率.Y = Max - Y;
					//中上
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(x, y + 43 - Max + Y, new Rectangle(0, 43, 624, 1));
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(x, y + 43 - Max + Y, new Rectangle(0, 43, 624, 1));
					//中下
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(x, y + 43, new Rectangle(0, 43, 624, 1));
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(x, y + 43, new Rectangle(0, 43, 624, 1));

					//拡大率を元に戻す
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex].vc拡大縮小倍率.Y = 1f;
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W.vc拡大縮小倍率.Y = 1f;
					//上
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(x, y - Max + Y, new Rectangle(0, 0, 624, 43));
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(x, y - Max + Y, new Rectangle(0, 0, 624, 43));
					//下
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(x, y + 43 + Max - Y, new Rectangle(0, 43, 624, 43));
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(x, y + 43 + Max - Y, new Rectangle(0, 43, 624, 43));
				}
				else
				{
					TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(x, y);
					TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(x, y);
				}


			}
			else
            {
				TJAPlayer3.Tx.SongSelect_Bar_Genre[bargenreindex]?.t2D描画(x, y);
				TJAPlayer3.Tx.SongSelect_Bar_Genre_W?.t2D描画(x, y);
			}


			
		}
        private int nStrジャンルtoNum( string strジャンル )
        {
            int nGenre = 8;
            switch( strジャンル )
            {
                case "アニメ":
                    nGenre = 0;
                    break;
                case "J-POP":
                    nGenre = 1;
                    break;
                case "ゲームミュージック":
                    nGenre = 2;
                    break;
                case "ナムコオリジナル":
                    nGenre = 3;
                    break;
                case "クラシック":
                    nGenre = 4;
                    break;
                case "どうよう":
                    nGenre = 5;
                    break;
                case "バラエティ":
                    nGenre = 6;
                    break;
                case "ボーカロイド":
                case "VOCALOID":
                    nGenre = 7;
                    break;
                default:
                    nGenre = 8;
                    break;

            }

            return nGenre;
        }

		private TitleTextureKey ttk曲名テクスチャを生成する(string str文字, Color forecolor, Color backcolor)
		{
			return new TitleTextureKey(str文字, pfMusicName, forecolor, backcolor, 530);
		}

		private TitleTextureKey ttkサブタイトルテクスチャを生成する(string str文字, Color forecolor, Color backcolor)
		{
			return new TitleTextureKey(str文字, pfSubtitle, forecolor, backcolor, 390);
		}

		private CTexture ResolveTitleTexture(TitleTextureKey titleTextureKey)
		{
			if (!_titleTextures.TryGetValue(titleTextureKey, out var texture))
			{
				texture = GenerateTitleTexture(titleTextureKey);
				_titleTextures.Add(titleTextureKey, texture);
			}

			return texture;
		}

		private static CTexture GenerateTitleTexture(TitleTextureKey titleTextureKey)
		{
			using (var bmp = new Bitmap(titleTextureKey.cPrivateFastFont.DrawPrivateFont(
				titleTextureKey.str文字, titleTextureKey.forecolor, titleTextureKey.backcolor)))
			{
				CTexture tx文字テクスチャ = TJAPlayer3.tテクスチャの生成(bmp);
				if (tx文字テクスチャ.szテクスチャサイズ.Width > titleTextureKey.maxHeight)
				{
					tx文字テクスチャ.vc拡大縮小倍率.X = (float)(((double)titleTextureKey.maxHeight) / tx文字テクスチャ.szテクスチャサイズ.Width);
				}

				return tx文字テクスチャ;
			}
		}

		private void ClearTitleTextureCache()
		{
			foreach (var titleTexture in _titleTextures.Values)
			{
				titleTexture.Dispose();
			}

			_titleTextures.Clear();
		}

		private sealed class TitleTextureKey
		{
			public readonly string str文字;
			public readonly CPrivateFastFont cPrivateFastFont;
			public readonly Color forecolor;
			public readonly Color backcolor;
			public readonly int maxHeight;

			public TitleTextureKey(string str文字, CPrivateFastFont cPrivateFastFont, Color forecolor, Color backcolor, int maxHeight)
			{
				this.str文字 = str文字;
				this.cPrivateFastFont = cPrivateFastFont;
				this.forecolor = forecolor;
				this.backcolor = backcolor;
				this.maxHeight = maxHeight;
			}

			
		}



		private void tアイテム数の描画()
		{
			string s = nCurrentPosition.ToString() + "/" + nNumOfItems.ToString();
			int x = 639 - 8 - 12;
			int y = 362;

			for ( int p = s.Length - 1; p >= 0; p-- )
			{
				tアイテム数の描画_１桁描画( x, y, s[ p ] );
				x -= 8;
			}
		}
		private void tアイテム数の描画_１桁描画( int x, int y, char s数値 )
		{
			int dx, dy;
			if ( s数値 == '/' )
			{
				dx = 48;
				dy = 0;
			}
			else
			{
				int n = (int) s数値 - (int) '0';
				dx = ( n % 6 ) * 8;
				dy = ( n / 6 ) * 12;
			}
			//if ( this.txアイテム数数字 != null )
			//{
			//	this.txアイテム数数字.t2D描画( CDTXMania.app.Device, x, y, new Rectangle( dx, dy, 8, 12 ) );
			//}
		}


        //数字フォント
        private CTexture txレベル数字フォント;
        [StructLayout( LayoutKind.Sequential )]
        private struct STレベル数字
        {
            public char ch;
            public int ptX;
        }
        private STレベル数字[] st小文字位置 = new STレベル数字[ 10 ];
        private void t小文字表示(int x, int y, string str)
        {
            foreach (char ch in str)
            {
                for (int i = 0; i < this.st小文字位置.Length; i++)
                {
                    if( this.st小文字位置[i].ch == ch )
                    {
                        Rectangle rectangle = new Rectangle( this.st小文字位置[i].ptX, 0, 22, 28 );
                        if (txレベル数字フォント != null)
                        {
							txレベル数字フォント.t2D描画(x, y, rectangle);
                        }
                        break;
                    }
                }
                x += 16;
            }
        }
		//-----------------
		#endregion
	}
}
