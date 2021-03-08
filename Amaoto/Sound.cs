using System;
using System.Security.Permissions;
using DxLibDLL;

namespace Amaoto
{
    /// <summary>
    /// サウンド管理を行うクラス。
    /// </summary>
    public class Sound : IDisposable, IPlayable
    {
        /// <summary>
        /// サウンドを生成します。
        /// </summary>
        public Sound(string fileName, ASync sync = ASync.off)
        {
            Sync = sync;
            switch (sync)
            {
                case ASync.off:
                    ID = DX.LoadSoundMem(fileName);
                    break;
                case ASync.on:
                    DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_FILE);
                    DX.SetUseASyncLoadFlag(DX.TRUE);
                    ID = DX.LoadSoundMem(fileName);
                    DX.SetUseASyncLoadFlag(DX.FALSE);
                    DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_MEMNOPRESS_PLUS);

                    
                    break;
            }
            if (ID != -1)
            {
                IsEnable = true;
            }
            FileName = fileName;

            Volume = 1.0;

            
        }
        ~Sound()
        {
            if (IsEnable)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (Sync == ASync.on ? DX.CheckHandleASyncLoad(ID) != 0 : false) return;

            if (DX.DeleteSoundMem(ID) != -1)
            {
                IsEnable = false;
            }
        }

        /// <summary>
        /// サウンドを再生します。
        /// </summary>
        /// <param name="playFromBegin">はじめから</param>
        public void Play(bool playFromBegin = true)
        {
            if (IsEnable)
            {
                DX.PlaySoundMem(ID, DX.DX_PLAYTYPE_BACK, playFromBegin ? 1 : 0);
            }
        }
        /// <summary>
        /// サウンドを指定した時刻から再生します。
        /// </summary>
        /// <param name="playFromBegin">はじめから</param>
        public void Play(long time)
        {
            if (IsEnable)
            {
                if (IsPlaying) Stop();

                DX.SetSoundCurrentTime((int)time, ID);
                DX.PlaySoundMem(ID, DX.DX_PLAYTYPE_BACK, DX.FALSE);
            }
        }

        /// <summary>
        /// サウンドを停止します。
        /// </summary>
        public void Stop()
        {
            if (IsEnable)
            {
                DX.StopSoundMem(ID);
            }
        }

        /// <summary>
        /// 有効かどうか。
        /// </summary>
        public bool IsEnable { get; private set; }

        /// <summary>
        /// ファイル名。
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// ID。
        /// </summary>
        public int ID { get; private set; }


        public long TotalTime { get; set; }

        /// <summary>
        /// 再生中かどうか。
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return DX.CheckSoundMem(ID) == 1;
            }
        }

        /// <summary>
        /// パン。
        /// </summary>
        public int Pan
        {
            get
            {
                return _pan;
            }
            set
            {
                _pan = value;
                DX.ChangePanSoundMem(value, ID);
            }
        }

        /// <summary>
        /// 音量。
        /// </summary>
        public double Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = (int)(value * 255);
                DX.ChangeVolumeSoundMem(_volume, ID);
            }
        }

        /// <summary>
        /// 再生位置。msが単位。
        /// </summary>
        public double Time
        {
            get
            {
                return DX.GetSoundCurrentTime(ID);
            }
            set
            {
                DX.SetSoundCurrentTime((int)value, ID);
            }
        }




        /// <summary>
        /// 再生速度を倍率で変更する。
        /// </summary>
        public double PlaySpeed
        {
            get
            {
                return _ratio;
            }
            set
            {
                _ratio = value;
                DX.ResetFrequencySoundMem(ID);
                var freq = DX.GetFrequencySoundMem(ID);
                // 倍率変更
                var speed = value * freq;
                // 1秒間に再生すべきサンプル数を上げ下げすると速度が変化する。
                DX.SetFrequencySoundMem((int)speed, ID);
            }
        }

        public ASync Sync;

        /// <summary>
        /// SoftHandleゲッチュ
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static int GetSoftHandle(string Path)
        {
            var handle = -1;
            DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_FILE);
            DX.SetUseASyncLoadFlag(DX.TRUE);
            handle = DX.LoadSoftSound(Path);
            DX.SetUseASyncLoadFlag(DX.FALSE);
            DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_MEMNOPRESS_PLUS);
            return handle;
        }

        /// <summary>
        /// 非同期読み込み
        /// </summary>
        public enum ASync
        {
            on,
            off
        }
        private int _pan;
        private int _volume;
        private double _ratio;
    }
}